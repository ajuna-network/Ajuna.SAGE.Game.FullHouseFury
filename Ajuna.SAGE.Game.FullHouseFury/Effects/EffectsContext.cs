namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public struct ModifyContext
    {
        public byte OldLvl { get; }
        public byte NewLvl { get; }

        public ModifyContext(byte oldLvl, byte newLvl)
        {
            OldLvl = oldLvl;
            NewLvl = newLvl;
        }
    }

    public struct AttackContext
    {
        public PokerHand Hand { get; }

        public ushort Score { get; }

        public byte[]? Cards { get; }

        public AttackContext(PokerHand hand, ushort score, byte[] cards)
        {
            Hand = hand;
            Score = score;
            Cards = cards;
        }
    }
}
