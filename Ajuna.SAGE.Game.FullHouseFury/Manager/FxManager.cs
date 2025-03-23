using Ajuna.SAGE.Game.FullHouseFury.Effects;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ajuna.SAGE.Game.FullHouseFury.Manager
{
    public class FxManager
    {
        private readonly List<(IEffect effect, byte level)> _activeEffects = new List<(IEffect effect, byte level)>();

        public FxManager(TowerAsset tower)
        {
            // read boons array
            var boonsArray = tower.GetAllBoons();
            foreach (BonusType bonus in Enum.GetValues(typeof(BonusType)))
            {
                if (bonus == BonusType.None) continue;
                byte idx = (byte)bonus;
                if (idx < boonsArray.Length)
                {
                    byte level = boonsArray[idx];
                    if (level > 0 && EffectsRegistry.BoonEffects.TryGetValue(bonus, out var effect))
                    {
                        _activeEffects.Add((effect, level));
                    }
                }
            }

            // read banes array
            var banesArray = tower.GetAllBanes();
            foreach (MalusType malus in Enum.GetValues(typeof(MalusType)))
            {
                if (malus == MalusType.None) continue;
                byte idx = (byte)malus;
                if (idx < banesArray.Length)
                {
                    byte level = banesArray[idx];
                    if (level > 0 && EffectsRegistry.BaneEffects.TryGetValue(malus, out var effect))
                    {
                        _activeEffects.Add((effect, level));
                    }
                }
            }
        }

        public void TriggerEvent(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, object? context = null)
        {
            foreach (var (effect, level) in _activeEffects)
            {
                if (effect.Triggers.Contains(gameEvent))
                {
                    effect.Apply(gameEvent, game, deck, tower, level, context);
                }
            }
        }
    }
}