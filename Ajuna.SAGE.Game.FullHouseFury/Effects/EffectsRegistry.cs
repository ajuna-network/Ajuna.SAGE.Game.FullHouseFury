using System.Collections.Generic;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public static class EffectsRegistry
    {
        public static readonly Dictionary<BonusType, IEffect> BoonEffects = new Dictionary<BonusType, IEffect>()
        {
            { BonusType.HeartHeal, new FxSuitHeal(Suit.Hearts) },
        };

        public static readonly Dictionary<MalusType, IEffect> BaneEffects = new Dictionary<MalusType, IEffect>()
        {
            { MalusType.SpadeOpHeal, new FxSuitOpHeal(Suit.Spades) },
        };
    }
}