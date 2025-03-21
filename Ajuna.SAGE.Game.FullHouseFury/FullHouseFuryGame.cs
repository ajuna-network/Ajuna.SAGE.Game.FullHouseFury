using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Manager;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury.Effects;
using Ajuna.SAGE.Game.FullHouseFury.Manager;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Ajuna.SAGE.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Ajuna.SAGE.Game.FullHouseFury.Test")]

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public class FullHouseFuryGame
    {
        /// <summary>
        /// Create an instance of the HeroJam game engine
        /// </summary>
        /// <param name="blockchainInfoProvider"></param>
        /// <returns></returns>
        public static Engine<FullHouseFuryIdentifier, FullHouseFuryRule> Create(IBlockchainInfoProvider blockchainInfoProvider)
        {
            var engineBuilder = new EngineBuilder<FullHouseFuryIdentifier, FullHouseFuryRule>(blockchainInfoProvider);

            engineBuilder.SetVerifyFunction(GetVerifyFunction());

            var rulesAndTransitions = GetRulesAndTranstionSets();
            foreach (var (identifier, rules, fee, transition) in rulesAndTransitions)
            {
                engineBuilder.AddTransition(identifier, rules, fee, transition);
            }

            return engineBuilder.Build();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal static Func<IAccount, FullHouseFuryRule, IAsset[], uint, object?, IBalanceManager, IAssetManager, bool> GetVerifyFunction()
        {
            return (p, r, a, b, c, m, s) =>
            {
                switch (r.RuleTypeEnum)
                {
                    case FullHouseFuryRuleType.AssetCount:
                        {
                            return r.RuleOpEnum switch
                            {
                                FullHouseFuryRuleOp.EQ => a.Length == BitConverter.ToUInt32(r.RuleValue),
                                FullHouseFuryRuleOp.GE => a.Length >= BitConverter.ToUInt32(r.RuleValue),
                                FullHouseFuryRuleOp.GT => a.Length > BitConverter.ToUInt32(r.RuleValue),
                                FullHouseFuryRuleOp.LT => a.Length < BitConverter.ToUInt32(r.RuleValue),
                                FullHouseFuryRuleOp.LE => a.Length <= BitConverter.ToUInt32(r.RuleValue),
                                FullHouseFuryRuleOp.NE => a.Length != BitConverter.ToUInt32(r.RuleValue),
                                _ => false,
                            };
                        }

                    case FullHouseFuryRuleType.IsOwnerOf:
                        {
                            if (r.RuleOpEnum != FullHouseFuryRuleOp.Index)
                            {
                                return false;
                            }
                            var assetIndex = BitConverter.ToUInt32(r.RuleValue);
                            if (a.Length <= assetIndex)
                            {
                                return false;
                            }

                            return p.IsOwnerOf(a[assetIndex]);
                        }

                    case FullHouseFuryRuleType.IsOwnerOfAll:
                        {
                            if (r.RuleOpEnum != FullHouseFuryRuleOp.None)
                            {
                                return false;
                            }

                            for (int i = 0; i < a.Length; i++)
                            {
                                if (!p.IsOwnerOf(a[i]))
                                {
                                    return false;
                                }
                            }
                            return true;
                        }

                    case FullHouseFuryRuleType.SameExist:
                        {
                            var accountAssets = s.AssetOf(p);
                            if (accountAssets == null || accountAssets.Count() == 0)
                            {
                                return false;
                            }

                            return accountAssets.Any(a => a.MatchType.SequenceEqual(r.RuleValue));
                        }

                    case FullHouseFuryRuleType.SameNotExist:
                        {
                            var accountAssets = s.AssetOf(p);
                            if (accountAssets == null || accountAssets.Count() == 0)
                            {
                                return true;
                            }

                            return !accountAssets.Any(a => a.MatchType.SequenceEqual(r.RuleValue));
                        }

                    case FullHouseFuryRuleType.AssetTypesAt:
                        {
                            if (r.RuleOpEnum != FullHouseFuryRuleOp.Composite)
                            {
                                return false;
                            }

                            for (int i = 0; i < r.RuleValue.Length; i++)
                            {
                                byte composite = r.RuleValue[i];

                                if (composite == 0)
                                {
                                    continue;
                                }

                                byte assetType = (byte)(composite >> 4);
                                byte assetSubType = (byte)(composite & 0x0F);

                                if (a.Length <= i)
                                {
                                    return false;
                                }

                                var baseAsset = a[i] as BaseAsset;
                                if (baseAsset == null
                                || (byte)baseAsset.AssetType != assetType
                                || (assetSubType != (byte)AssetSubType.None && (byte)baseAsset.AssetSubType != assetSubType))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }

                    default:
                        throw new NotSupportedException($"Unsupported RuleType {r.RuleType}!");
                }
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<(FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>)> GetRulesAndTranstionSets()
        {
            var result = new List<(FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>)>
            {
                GetStartTransition(),
                GetPlayTransition(),
                GetPreparationTransition(),
                GetBattleTransition(),
                GetDiscardTransition(),
                GetScoreTransition(),
                GetShopTransition(),
            };

            return result;
        }

        /// <summary>
        /// Get Create Player transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetStartTransition()
        {
            var identifier = FullHouseFuryConfig.Start(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(e.Id, b);
                var deck = new DeckAsset(e.Id, b);
                var towr = new TowerAsset(e.Id, b);

                return new IAsset[] { game, deck, towr };
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Start Player transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetPlayTransition()
        {
            var identifier = FullHouseFuryConfig.Play(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState == GameState.Running)
                {
                    // gme is already running
                    return result;
                }

                // initialize a new game
                game.New();

                // initialize a new deck
                deck.New();

                // initialize a new tower
                towr.New();

                // empty hand
                deck.EmptyHand();

                return result;
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Preparation transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetPreparationTransition()
        {
            var identifier = FullHouseFuryConfig.Preparation(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState != GameState.Running)
                {
                    // game is not running
                    return result;
                }

                if (game.LevelState != LevelState.Preparation)
                {
                    // levelstate is not in preparation state
                    return result;
                }

                // initiate the effects manager
                var fxManager = new FxManager(towr);

                if (game.Level > 1)
                {
                    byte? position = c as byte?;
                    // no position is provided, we use random position.
                    if (position == null || position > 2)
                    {
                        // take one of the 3 positions [0, 1, 2]
                        position = (byte)(h[0] % 3);
                    }

                    var choice = towr.GetBoonAndBane(position.Value);

                    var currentBoon = towr.GetBoon((byte)choice.boon, out bool isMaxedBoon);
                    if (!isMaxedBoon)
                    {
                        towr.SetBoon((byte)choice.boon, (byte)(currentBoon + 1));
                        if (EffectsRegistry.BoonEffects.TryGetValue(choice.boon, out var effect))
                        {
                            effect.Add(game, deck, towr, 1, new ModifyContext(currentBoon, 1));
                        }
                    }

                    var currentBane = towr.GetBane((byte)choice.bane, out bool isMaxedBane);
                    if (!isMaxedBane)
                    {
                        towr.SetBane((byte)choice.bane, (byte)(currentBane + 1));
                        if (EffectsRegistry.BaneEffects.TryGetValue(choice.bane, out var effect))
                        {
                            effect.Add(game, deck, towr, 1, new ModifyContext(currentBane, 1));
                        }
                    }
                }

                game.Round = 1; // reset round
                game.LevelState = LevelState.Battle;

                game.ClearAttack();

                towr.ClearChoices();

                deck.Draw(game.HandSize, h, out byte[] cards);
                fxManager.TriggerEvent(GameEvent.OnDraw, game, deck, towr, cards);

                return result;
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Battle transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetBattleTransition()
        {
            var identifier = FullHouseFuryConfig.Battle(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState != GameState.Running)
                {
                    // game is not running
                    return result;
                }

                if (game.LevelState != LevelState.Battle)
                {
                    // levelstate is not in preparation state
                    return result;
                }

                byte[]? positions = c as byte[];
                if (positions == null)
                {
                    // attack hand is not provided
                    return result;
                }

                if (positions.Length == 0 || positions.Length > 5)
                {
                    // attack hand size min one and max five cards
                    return result;
                }

                if (positions.Max() > 10)
                {
                    // only 10 hand slots available
                    return result;
                }

                byte[] attackCards = new byte[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    if (deck.IsHandSlotEmpty(positions[i]))
                    {
                        // hand slot is empty
                        return result;
                    }

                    attackCards[i] = deck.GetHandCard(positions[i], out _, out _);
                }

                // initiate the effects manager
                var fxManager = new FxManager(towr);

                // remove the actual played hand
                for (int i = 0; i < positions.Length; i++)
                {
                    deck.SetHandCard(positions[i], DeckAsset.EMPTY_SLOT, 0);
                }

                // clear attack
                game.ClearAttack();

                // set attack
                for (int i = 0; i < attackCards.Length; i++)
                {
                    game.SetAttackHand(i, attackCards[i]);
                }
                game.AttackType = FullHouseFuryUtil.Evaluate(attackCards, out ushort score, out _);
                game.AttackScore = score;

                // on attack event
                fxManager.TriggerEvent(GameEvent.OnAttack, game, deck, towr, new AttackContext(game.AttackType, game.AttackScore, attackCards));

                // boss attack
                game.BossDamage += game.AttackScore;
                //fxManager.TriggerEvent(GameEvent.OnBossDamage, game, deck, towr, game.AttackScore);

                if (game.PlayerEndurance > 0)
                {
                    game.PlayerEndurance--;
                }
                else
                {
                    game.PlayerDamage = (ushort)Math.Min(game.PlayerDamage + game.FatigueDamage, ushort.MaxValue);
                    //fxManager.TriggerEvent(GameEvent.OnPlayerDamage, game, deck, towr, game.FatigueDamage);

                    game.FatigueDamage = (ushort)Math.Min(game.FatigueDamage * 2, ushort.MaxValue);

                }

                // continue playing as long both parties are alive.
                if (game.IsBossAlive && game.IsPlayerAlive)
                {
                    game.LevelState = LevelState.Battle;

                    // next round
                    game.Round = (byte)Math.Min(game.Round + 1, byte.MaxValue);
                    fxManager.TriggerEvent(GameEvent.OnRoundStart, game, deck, towr, game.Round);

                    // draw new cards for the played ones
                    deck.Draw(game.HandSize, h, out byte[] cards);
                    fxManager.TriggerEvent(GameEvent.OnDraw, game, deck, towr, cards);
                }
                else
                {
                    game.LevelState = LevelState.Score;
                }

                // game is finished if player is dead, or he has no more cards to draw
                if (!game.IsPlayerAlive || ((deck.DeckSize + deck.HandCardsCount()) == 0))
                {
                    game.GameState = GameState.Finished;
                }

                return result;
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Discard cards from the hand
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetDiscardTransition()
        {
            var identifier = FullHouseFuryConfig.Discard(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState != GameState.Running)
                {
                    return result;
                }

                if (game.LevelState != LevelState.Battle)
                {
                    return result;
                }

                if (game.Discard == 0)
                {
                    return result;
                }

                byte[]? positions = c as byte[];
                if (positions == null)
                {
                    // positions are not provided
                    return result;
                }

                if (positions.Length == 0 || positions.Length > 10)
                {
                    // attack hand size min one and max five cards
                    return result;
                }

                if (positions.Max() > 10)
                {
                    // only 10 hand slots available
                    return result;
                }

                // initiate the effects manager
                var fxManager = new FxManager(towr);

                byte[] discardCards = new byte[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    if (deck.IsHandSlotEmpty(positions[i]))
                    {
                        // hand slot is empty
                        return result;
                    }

                    discardCards[i] = deck.GetHandCard(positions[i], out _, out _);
                }

                // remove the actual played hand
                for (int i = 0; i < positions.Length; i++)
                {
                    deck.SetHandCard(positions[i], DeckAsset.EMPTY_SLOT, 0);
                }

                // reduce discard count
                game.Discard--;
                fxManager.TriggerEvent(GameEvent.OnDiscard, game, deck, towr, game.Discard);

                // draw new cards for the discarded ones
                deck.Draw(game.HandSize, h, out byte[] cards);
                fxManager.TriggerEvent(GameEvent.OnDraw, game, deck, towr, cards);

                return result;
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Score transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetScoreTransition()
        {
            var identifier = FullHouseFuryConfig.Score(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState != GameState.Running)
                {
                    return result;
                }

                if (game.LevelState != LevelState.Score)
                {
                    return result;
                }

                // initiate the effects manager
                var fxManager = new FxManager(towr);

                // clear attack
                game.ClearAttack();

                // payout tokens
                var bonus = game.Round < 4 ? 3 : game.Round < 8 ? 2 : 1;
                game.Token = (byte)Math.Min(game.Token + game.Level + bonus, byte.MaxValue);

                // next level
                game.Level = (byte)Math.Min(game.Level + 1, byte.MaxValue);
                fxManager.TriggerEvent(GameEvent.OnLevelStart, game, deck, towr, game.Level);

                // set next boss
                game.MaxBossHealth = (ushort)(Math.Pow(game.Level, 2) * 100);
                game.BossDamage = 0;

                // don't reset player health
                //game.MaxPlayerHealth = 100;
                //game.PlayerDamage = 0;

                // reset player endurance
                game.PlayerEndurance = game.MaxPlayerEndurance;

                // empty hand
                deck.EmptyHand();

                // reset deck
                deck.New();

                // set of 3 boons and banes combos, to choose from in preparation
                for(int i = 0; i < 3; i++)
                {
                    var valBoon = (BonusType)((h[(i * 2) + 0] % 32) + 1);
                    var valBane = (MalusType)((h[(i * 2) + 1] % 32) + 1);
                    towr.SetBoonAndBane(i, valBoon, valBane);
                }

                // restart with preparation
                game.LevelState = LevelState.Preparation;

                return result;
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Shop transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetShopTransition()
        {
            var identifier = FullHouseFuryConfig.Shop(out FullHouseFuryRule[] rules, out ITransitioFee? fee);

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));
                var towr = new TowerAsset(a.ElementAt(2));
                var result = new IAsset[] { game, deck, towr };

                if (game.GameState != GameState.Running)
                {
                    return result;
                }

                // shop is only available in the preparation state
                if (game.LevelState != LevelState.Preparation)
                {
                    return result;
                }

                if (game.Token == 0)
                {
                    // not enough tokens to buy anything
                    return result;
                }

                ushort[]? upgrades = c as ushort[];
                if (upgrades == null)
                {
                    // upgrades are not provided
                    return result;
                }

                // verify all upgrade are valid
                var totalPrice = 0;
                foreach (var upgrade in upgrades)
                {
                    var upgradeSet = new UpgradeSet(upgrade);
                    if (!FullHouseFuryUtil.UpgradeInfo(upgradeSet.FeatureType, upgradeSet.FeatureEnum, upgradeSet.Level, result, out byte price))
                    {
                        // upgrade is not valid
                        return result;
                    }

                    totalPrice += price;
                    if (game.Token < totalPrice)
                    {
                        // not enough tokens to buy all upgrades
                        return result;
                    }
                }

                // upgrade
                foreach (var upgrade in upgrades)
                {
                    var upgradeSet = new UpgradeSet(upgrade);
                    if (!FullHouseFuryUtil.TryUpgrade(upgradeSet.FeatureType, upgradeSet.FeatureEnum, upgradeSet.Level, result))
                    {
                        throw new NotSupportedException($"Unsupported upgrade {upgradeSet.FeatureType} {upgradeSet.FeatureEnum} {upgradeSet.Level} this should never happen!");
                    }
                }

                return result;
            };

            return (identifier, rules, fee, function);
        }
    }

}