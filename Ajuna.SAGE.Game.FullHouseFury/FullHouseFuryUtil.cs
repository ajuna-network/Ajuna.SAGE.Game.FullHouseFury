using Ajuna.SAGE.Game.FullHouseFury.Model;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Linq;

[assembly: InternalsVisibleTo("Ajuna.SAGE.Game.CasinoJam.Test")]

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public partial class FullHouseFuryUtil
    {
        public const byte COLLECTION_ID = 1;

        public const byte BLOCKTIME_SEC = 6;

        public const uint BLOCKS_PER_DAY = 24 * BLOCKS_PER_HOUR;
        public const uint BLOCKS_PER_HOUR = 60 * BLOCKS_PER_MINUTE;
        public const uint BLOCKS_PER_MINUTE = 10;

        public static byte MatchType(AssetType assetType)
        {
            return MatchType(assetType, AssetSubType.None);
        }

        public static byte MatchType(AssetType assetType, AssetSubType machineSubType)
        {
            var highHalfByte = (byte)assetType << 4;
            var lowHalfByte = (byte)machineSubType;
            return (byte)(highHalfByte | lowHalfByte);
        }

        /// <summary>
        /// Evaluates a poker hand (up to 5 cards provided as card indexes 0–51)
        /// and returns its ranking while also outputting a numeric score calculated as:
        /// 
        /// - HighCard: factor 1 × (highest card's value, with Ace = 14) + ((1-1)^2 * 10)
        /// - Pair: factor 2 × (rank of the pair) + ((2-1)^2 * 10)
        /// - TwoPair: factor 3 × (highest pair's rank) + ((3-1)^2 * 10)
        /// - ThreeOfAKind: factor 4 × (rank of the triple) + ((4-1)^2 * 10)
        /// - Straight: factor 5 × (highest card in the straight) + ((5-1)^2 * 10)
        /// - Flush: factor 6 × (highest card in the flush) + ((6-1)^2 * 10)
        /// - FullHouse: factor 7 × (rank of the triple part) + ((7-1)^2 * 10)
        /// - FourOfAKind: factor 8 × (rank of the quadruple) + ((8-1)^2 * 10)
        /// - StraightFlush: factor 9 × (highest card) + ((9-1)^2 * 10)
        /// - RoyalFlush: factor 10 × 14 + ((10 - 1)^2 * 10)
        /// </summary>
        /// <param name="cardIndexes">An array of card indexes (max 5).</param>
        /// <param name="score">The computed score (higher is better).</param>
        /// <returns>The PokerHand ranking.</returns>
        public static PokerHand Evaluate(byte[] cardIndexes, out ushort score)
        {
            score = 0;
            if (cardIndexes == null)
            {
                throw new ArgumentNullException(nameof(cardIndexes));
            }

            if (cardIndexes.Length == 0 || cardIndexes.Length > 5)
            {
                throw new ArgumentException("Hand must have between 1 and 5 cards.", nameof(cardIndexes));
            }

            // Array for rank frequencies (indices 1..13, Ace = 1, King = 13)
            int[] rankCounts = new int[14]; // index 0 unused.
            int minRank = 14, maxRank = 0;
            int firstSuit = -1;
            bool isFlush = true;
            List<int> straightRanks = new List<int>(); // raw rank values (Ace = 1)
            List<int> kickerRanks = new List<int>();     // for kicker, treat Ace as high (14)

            foreach (byte index in cardIndexes)
            {
                if (index > 51)
                {
                    throw new ArgumentOutOfRangeException(nameof(cardIndexes), "Each card index must be between 0 and 51.");
                }

                int rank = (index % 13) + 1; // 1..13
                int suit = index / 13;
                rankCounts[rank]++;
                if (rank < minRank)
                {
                    minRank = rank;
                }

                if (rank > maxRank)
                {
                    maxRank = rank;
                }

                straightRanks.Add(rank);
                kickerRanks.Add(rank == 1 ? 14 : rank); // Ace as 14 for kicker

                if (firstSuit == -1)
                {
                    firstSuit = suit;
                }
                else if (suit != firstSuit)
                {
                    isFlush = false;
                }
            }

            // Check for straight:
            bool isStraight = false;
            if (straightRanks.Distinct().Count() == cardIndexes.Length)
            {
                if (maxRank - minRank == cardIndexes.Length - 1)
                {
                    isStraight = true;
                }
                // Special case: Ace-low straight (A,2,3,4,5)
                else if (cardIndexes.Length == 5 && straightRanks.OrderBy(x => x)
                           .SequenceEqual(new List<int> { 1, 2, 3, 4, 5 }))
                {
                    isStraight = true;
                }
            }

            int fours = rankCounts.Count(c => c == 4);
            int triples = rankCounts.Count(c => c == 3);
            int pairs = rankCounts.Count(c => c == 2);

            PokerHand category;
            if (isStraight && isFlush)
            {
                // Royal Flush: Ace, 10, J, Q, K
                if (cardIndexes.Length == 5 &&
                    straightRanks.OrderBy(x => x).SequenceEqual(new List<int> { 1, 10, 11, 12, 13 }))
                {
                    category = PokerHand.RoyalFlush;
                }
                else
                {
                    category = PokerHand.StraightFlush;
                }
            }
            else if (fours == 1)
            {
                category = PokerHand.FourOfAKind;
            }
            else if (triples == 1 && pairs == 1)
            {
                category = PokerHand.FullHouse;
            }
            else if (isFlush)
            {
                category = PokerHand.Flush;
            }
            else if (isStraight)
            {
                category = PokerHand.Straight;
            }
            else if (triples == 1)
            {
                category = PokerHand.ThreeOfAKind;
            }
            else if (pairs == 2)
            {
                category = PokerHand.TwoPair;
            }
            else if (pairs == 1)
            {
                category = PokerHand.Pair;
            }
            else
            {
                category = PokerHand.HighCard;
            }

            // Determine the kicker value based on hand category.
            int kicker = 0;
            switch (category)
            {
                case PokerHand.HighCard:
                    kicker = kickerRanks.Max(); // only highest card counts.
                    break;
                case PokerHand.Pair:
                    // Find the pair rank (highest pair if more than one, though with 5 cards you can only have one pair).
                    for (int r = 13; r >= 1; r--)
                    {
                        if (rankCounts[r] == 2)
                        {
                            kicker = (r == 1 ? 14 : r);
                            break;
                        }
                    }
                    break;
                case PokerHand.TwoPair:
                    // Use the highest pair rank.
                    for (int r = 13; r >= 1; r--)
                    {
                        if (rankCounts[r] == 2)
                        {
                            kicker = (r == 1 ? 14 : r);
                            break;
                        }
                    }
                    break;
                case PokerHand.ThreeOfAKind:
                    for (int r = 13; r >= 1; r--)
                    {
                        if (rankCounts[r] == 3)
                        {
                            kicker = (r == 1 ? 14 : r);
                            break;
                        }
                    }
                    break;
                case PokerHand.Straight:
                case PokerHand.Flush:
                case PokerHand.StraightFlush:
                    kicker = kickerRanks.Max();
                    break;
                case PokerHand.FullHouse:
                    // Use the rank of the triple part.
                    for (int r = 13; r >= 1; r--)
                    {
                        if (rankCounts[r] == 3)
                        {
                            kicker = (r == 1 ? 14 : r);
                            break;
                        }
                    }
                    break;
                case PokerHand.FourOfAKind:
                    for (int r = 13; r >= 1; r--)
                    {
                        if (rankCounts[r] == 4)
                        {
                            kicker = (r == 1 ? 14 : r);
                            break;
                        }
                    }
                    break;
                case PokerHand.RoyalFlush:
                    kicker = 14; // Ace is highest.
                    break;
                default:
                    kicker = 0;
                    break;
            }

            // Determine the factor based on hand category.
            int factor = category switch
            {
                PokerHand.HighCard => 1,
                PokerHand.Pair => 2,
                PokerHand.TwoPair => 3,
                PokerHand.ThreeOfAKind => 4,
                PokerHand.Straight => 5,
                PokerHand.Flush => 6,
                PokerHand.FullHouse => 7,
                PokerHand.FourOfAKind => 8,
                PokerHand.StraightFlush => 9,
                PokerHand.RoyalFlush => 10,
                _ => 0
            };

            // New scoring: score = (factor * kicker) + ((factor - 1)^2 * 10)
            score = (ushort) (factor * kicker + (Math.Pow(factor - 1, 2) * 10));

            return category;
        }
    }
}