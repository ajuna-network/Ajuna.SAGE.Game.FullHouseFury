using System.Collections.Generic;

namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public static class EffectsRegistry
    {
        public static readonly Dictionary<BonusType, IEffect> BoonEffects = new Dictionary<BonusType, IEffect>()
        {
            { BonusType.HeartHeal, new FxHeartHeal() },
        };

        public static readonly Dictionary<MalusType, IEffect> BaneEffects = new Dictionary<MalusType, IEffect>()
        {
            { MalusType.SpadeHeal, new FxSpadeHeal() },
        };
    }
}