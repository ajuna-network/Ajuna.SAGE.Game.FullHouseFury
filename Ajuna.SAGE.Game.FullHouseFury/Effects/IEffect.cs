using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public interface IEffect
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<GameEvent> Triggers { get; }

        /// <summary>
        /// Invoked exactly once, when the effect is added to the player.
        /// For example, to adjust permanent stats like HandSize or Discard.
        /// </summary>
        void Add(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context);

        /// <summary>
        /// Invoked once when the effect is removed from the player (if your design allows removal).
        /// You can undo any changes made in AddEffect here, e.g., restore the old HandSize.
        /// </summary>
        void Remove(GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context);

        /// <summary>
        /// Called for each event in <see cref="Triggers"/>, letting the effect apply dynamic logic
        /// (e.g., adjusting damage, healing the player on hearts, etc.) if needed.
        /// </summary>
        void Apply(GameEvent gameEvent, GameAsset game, DeckAsset deck, TowerAsset tower, byte level, object? context);
    }

}
