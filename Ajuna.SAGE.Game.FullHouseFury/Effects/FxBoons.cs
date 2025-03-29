using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    /// <summary>
    /// Effect that heals the player for the sum of the ranks of all cards of a given suit in the attack.
    /// </summary>
    public class FxSuitHeal : IEffect
    {
        public string Name => $"{Suit} Heal";

        public string Description => $"Heals player for the sum of the ranks of all {Suit.ToString().ToLower()}s  in the attack";

        public Suit Suit { get; }

        /// <summary>
        /// Effect that heals the player for the sum of the ranks of all cards of a given suit in the attack.
        /// </summary>
        /// <param name="suit"></param>
        public FxSuitHeal(Suit suit)
        {
            Suit = suit;
        }

        public IEnumerable<GameEvent> Triggers => new[] { GameEvent.OnAttack };

        public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (gameEvent == GameEvent.OnAttack && context is AttackContext ctx)
            {
                if (ctx.Cards == null)
                {
                    return;
                }

                int rankCummulative = 0;
                foreach (var cardIndex in ctx.Cards)
                {
                    var card = new Card(cardIndex, 0);
                    if (card.Suit == Suit.Hearts)
                    {
                        rankCummulative += (byte)card.Rank;
                    }
                }

                int healAmount = rankCummulative;
                int newDamage = Math.Max(0, game.PlayerDamage - healAmount);
                game.PlayerDamage = (ushort)newDamage;
            }
        }
    }

    /// <summary>
    /// Effect that adds an extra card drawn.
    /// </summary>
    public class FxExtraCardDraw : IEffect
    {
        public string Name => "Extra Card Draw";
        public string Description => "At the start of each round, draw additional cards equal to the level.";

        public IEnumerable<GameEvent> Triggers => Array.Empty<GameEvent>();

        public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (context is ModifyContext ctx) 
            {
                var change = ctx.NewLvl - ctx.OldLvl;
                game.HandSize = (byte)Math.Min(DeckAsset.HAND_LIMIT_SIZE, game.HandSize + change);
            }
        }

        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (context is ModifyContext ctx)
            {
                var change = ctx.NewLvl - ctx.OldLvl;
                game.HandSize = (byte)Math.Max(1, game.HandSize + change);
            }
        }
    }

    /// <summary>
    /// If the entire attack consists of only face cards (J, Q, K), deal additional damage to the boss
    /// equal to the sum of those cards' ranks.
    /// E.g., J=11, Q=12, K=13 => If user plays {J♥, Q♠}, that sums to 23. 
    /// This effect triggers only if *all* cards are face cards.
    /// </summary>
    public class FxFaceCardBonus : IEffect
    {
        public string Name => "Face Card Bonus";

        public string Description => "If your entire attack is made of face cards (J, Q, K), deal bonus damage to the boss equal to the sum of those ranks.";

        public IEnumerable<GameEvent> Triggers => new[] { GameEvent.OnAttack };

        public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (gameEvent == GameEvent.OnAttack && context is AttackContext ctx && ctx.Cards != null)
            {
                if (ctx.Cards.Length == 0) return;

                bool allFaceCards = true;
                int rankSum = 0;

                foreach (var cardIndex in ctx.Cards)
                {
                    var card = new Card(cardIndex, 0);
                    // Face cards = J=11, Q=12, K=13
                    if (card.Rank < Rank.Jack || card.Rank > Rank.King)
                    {
                        allFaceCards = false;
                        break;
                    }
                    rankSum += (int)card.Rank;  // J=11, Q=12, K=13
                }

                // Only apply effect if *all* are face cards
                if (allFaceCards)
                {
                    game.AttackScore += (ushort) (rankSum * level);
                }
            }
        }

        /// <summary>
        /// Permanently raises the player's MaxPlayerEndurance (and current PlayerEndurance)
        /// by 'level'. Useful if you want more stamina/fatigue tolerance.
        /// This is a purely static effect, so we do all changes in Add/Remove.
        /// </summary>
        public class FxEnduranceUp : IEffect
        {
            public string Name => "Extra Endurance";
            public string Description =>
                "Increases the player's maximum endurance by [level] and refills current endurance accordingly.";

            // No need to listen for any events
            public IEnumerable<GameEvent> Triggers => Array.Empty<GameEvent>();

            public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
            {
                if (context is ModifyContext ctx)
                {
                    var change = ctx.NewLvl - ctx.OldLvl;
                    game.MaxPlayerEndurance = (byte)Math.Min(game.MaxPlayerEndurance + change, byte.MaxValue);
                    game.PlayerEndurance = (byte)Math.Min(game.PlayerEndurance + change, game.MaxPlayerEndurance);
                }

            }

            public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
            {
                if (context is ModifyContext ctx)
                {
                    var change = ctx.NewLvl - ctx.OldLvl;
                    game.MaxPlayerEndurance = (byte)Math.Max(game.MaxPlayerEndurance + change, 1);
                    if (game.PlayerEndurance > game.MaxPlayerEndurance)
                    {
                        game.PlayerEndurance = game.MaxPlayerEndurance;
                    }
                }
            }

        }

        /// <summary>
        /// At the start of each round, refills the deck fully (or partially).
        /// This effect triggers on OnRoundStart.
        /// If you want it to trigger once per *level*, you can incorporate that logic inside Apply.
        /// </summary>
        public class FxDeckRefill : IEffect
        {
            public string Name => "Deck Refill";

            public string Description => "Deck refills, when empty for each refill you have.";

            public IEnumerable<GameEvent> Triggers => Array.Empty<GameEvent>();

            public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) 
            {
                if (context is ModifyContext ctx)
                {
                    var change = ctx.NewLvl - ctx.OldLvl;
                    deck.DeckRefill = (byte)Math.Min(deck.DeckRefill + change, byte.MaxValue);
               }
            }

            public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) 
            {
                if (context is ModifyContext ctx)
                {
                    var change = ctx.NewLvl - ctx.OldLvl;
                    deck.DeckRefill = (byte)Math.Max(deck.DeckRefill + change, byte.MinValue);
                }
            }

        }
    }
}