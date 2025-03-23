namespace Ajuna.SAGE.Game.FullHouseFury.Effects
{
    public struct ModifyContext
    {
        public byte Level { get; }
        public byte Value { get; }

        public ModifyContext(byte level, byte value)
        {
            Level = level;
            Value = value;
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
