namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    /// <summary>
    /// Holds the best attack hand evaluation.
    /// </summary>
    public class BestPokerHand
    {
        /// <summary>
        /// The evaluated poker hand category.
        /// </summary>
        public PokerHand Category { get; set; }
        /// <summary>
        /// The numeric score computed for the attack.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// The positions (indices in the 10-card hand array) used for this attack.
        /// </summary>
        public int[] Positions { get; set; }
        /// <summary>
        /// The card indexes (0–51) corresponding to the chosen positions.
        /// </summary>
        public byte[] CardIndexes { get; set; }

        public override string ToString()
        {
            return $"Category: {Category}, Score: {Score}, Positions: [{string.Join(", ", Positions)}], Cards: [{string.Join(", ", CardIndexes)}]";
        }
    }
}