﻿using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Manager;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury.Model;
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
                GetCreateGameTransition(),
            };

            return result;
        }

        /// <summary>
        /// Get Create Player transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetCreateGameTransition()
        {
            var identifier = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);
            byte matchType = FullHouseFuryUtil.MatchType(AssetType.Game, AssetSubType.None);

            FullHouseFuryRule[] rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 0u),
                new FullHouseFuryRule(FullHouseFuryRuleType.SameNotExist, FullHouseFuryRuleOp.MatchType, matchType)
            };

            ITransitioFee? fee = default;

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(e.Id, b);
                var deck = new DeckAsset(e.Id, b);

                return new IAsset[] { game, deck };
            };

            return (identifier, rules, fee, function);
        }

        /// <summary>
        /// Get Create Player transition set
        /// </summary>
        /// <returns></returns>
        private static (FullHouseFuryIdentifier, FullHouseFuryRule[], ITransitioFee?, TransitionFunction<FullHouseFuryRule>) GetStartTransition()
        {
            var identifier = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);
            byte gameAt = FullHouseFuryUtil.MatchType(AssetType.Game, AssetSubType.None);
            byte deckAt = FullHouseFuryUtil.MatchType(AssetType.Deck, AssetSubType.None);

            FullHouseFuryRule[] rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 2u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameAt, deckAt),
                // TODO: verify gamestate is running in rules
            };

            ITransitioFee? fee = default;

            TransitionFunction<FullHouseFuryRule> function = (e, r, f, a, h, b, c, m) =>
            {
                var game = new GameAsset(a.ElementAt(0));
                var deck = new DeckAsset(a.ElementAt(1));

                if (game.GameState == GameState.Running)
                {
                    // gme is already running
                    return Array.Empty<IAsset>();
                }

                // initialize a new game
                game.NewGame();

                // initialize a new deck
                deck.NewDeck();

                // empty hand
                deck.EmptyHand();

                return new IAsset[] { game, deck };
            };

            return (identifier, rules, fee, function);
        }

    }
}