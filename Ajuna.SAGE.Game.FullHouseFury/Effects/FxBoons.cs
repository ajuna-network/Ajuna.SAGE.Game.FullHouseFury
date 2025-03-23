using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    /// <summary>
    /// 
    /// </summary>
    public class FxHeartHeal : IEffect
    {
        public string Name => "Heart Heal";

        public string Description => "Heals player for the sum of the ranks of all hearts in the attack";

        public IEnumerable<GameEvent> Triggers => new[] { GameEvent.OnAttack };

        public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

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
    /// 
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
                game.HandSize = (byte)Math.Min(DeckAsset.HAND_LIMIT_SIZE, game.HandSize + ctx.Value);
            }
        }

        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (context is ModifyContext ctx)
            {
                game.HandSize = (byte)Math.Max(1, game.HandSize - ctx.Value);
            }
        }

        public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
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

        // No static changes, so Add/Remove are empty
        public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) { }
        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) { }

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
                    game.MaxPlayerEndurance = (byte)Math.Min(game.MaxPlayerEndurance + ctx.Value, byte.MaxValue);
                    game.PlayerEndurance = (byte)Math.Min(game.PlayerEndurance + ctx.Value, game.MaxPlayerEndurance);
                }

            }

            public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
            {
                if (context is ModifyContext ctx)
                {
                    game.MaxPlayerEndurance = (byte)Math.Max(game.MaxPlayerEndurance - level, 1);
                    if (game.PlayerEndurance > game.MaxPlayerEndurance)
                    {
                        game.PlayerEndurance = game.MaxPlayerEndurance;
                    }
                }
            }

            public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
            {
                // No runtime effect, so do nothing
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

            public string Description =>
                "At the start of each round, the deck is refilled back to its maximum size.";

            // Trigger at the start of the round
            public IEnumerable<GameEvent> Triggers => Array.Empty<GameEvent>();

            // No instant changes to stats, so empty
            public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) { }
            public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context) { }

            public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
            {
            }
        }
    }
}