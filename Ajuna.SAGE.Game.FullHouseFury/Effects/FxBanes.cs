using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public class FxSpadeHeal : IEffect
    {
        public string Name => "Spade Heal";

        public string Description => "Heals enemy for the sum of the ranks of all spades in the attack";

        public IEnumerable<GameEvent> Triggers => new[] { GameEvent.OnAttack };

        public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

        public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (gameEvent == GameEvent.OnAttack && context is AttackContext atkCtx)
            {
                if (atkCtx.Cards == null)
                {
                    return;
                }

                int rankCummulative = 0;
                foreach (var cardIndex in atkCtx.Cards)
                {
                    var card = new Card(cardIndex, 0);
                    if (card.Suit == Suit.Spades)
                    {
                        rankCummulative += (byte)card.Rank;
                    }
                }

                int healAmount = rankCummulative;
                int newDamage = Math.Max(0, game.BossDamage - healAmount);
                game.BossDamage = (ushort)newDamage;
            }
        }
    }

    public class FxHalvedDamage : IEffect
    {
        public string Name => "Halved Damage";
        public string Description => "Halves the player's final damage to the boss.";

        public IEnumerable<GameEvent> Triggers => new[] { GameEvent.OnAttack };

        public void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

        public void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
        }

        public void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context)
        {
            if (gameEvent == GameEvent.OnAttack && context is AttackContext atkCtx)
            {
                // Suppose atkCtx has a Damage or Score property that represents how much we deal
                if (atkCtx.Score > 0)
                {
                    // Halve the damage for each level (if level=1 => half, level=2 => quarter, etc.)
                    int newDamage = atkCtx.Score;
                    for (int i = 0; i < level; i++)
                    {
                        newDamage /= 2;
                    }

                    game.AttackScore = (ushort)Math.Max(0, newDamage);
                }
            }
        }
    }
}