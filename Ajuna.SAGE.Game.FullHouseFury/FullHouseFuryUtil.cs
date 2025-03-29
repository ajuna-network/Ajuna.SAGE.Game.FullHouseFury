using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("Ajuna.SAGE.Game.CasinoJam.Test")]

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public partial class FullHouseFuryUtil
    {
        public const byte DATA_SIZE = 32;

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
        /// Evaluates an encoded poker hand, calculates its score and scoring breakdown, and returns the determined poker hand category.
        /// </summary>
        /// <param name="attackHand"></param>
        /// <param name="score"></param>
        /// <param name="scoreCard"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PokerHand Evaluate(byte[] attackHand, out ushort score, out ushort[] scoreCard)
        {
            score = 0;
            scoreCard = new ushort[4];

            int handSize = attackHand.Length;
            if (handSize == 0 || handSize > 5)
            {
                throw new ArgumentException("Hand must have between 1 and 5 cards.", nameof(attackHand));
            }

            var cards = new Card[handSize];

            for (int i = 0; i < handSize; i++)
            {
                DecodeCardByte(attackHand[i], out byte cardIndex, out byte rarity);
                cards[i] = new Card(cardIndex, rarity);
            }

            var rankedCards = new Dictionary<int, List<Card>>();
            var suitedCards = new Dictionary<int, List<Card>>();
            foreach (var card in cards)
            {
                // Group by Rank.
                if (!rankedCards.TryGetValue((int)card.Rank, out var rankList))
                {
                    rankList = new List<Card>();
                    rankedCards[(int)card.Rank] = rankList;
                }
                rankList.Add(card);

                // Group by Suit.
                if (!suitedCards.TryGetValue((int)card.Suit, out var suitList))
                {
                    suitList = new List<Card>();
                    suitedCards[(int)card.Suit] = suitList;
                }
                suitList.Add(card);
            }

            // Determine flush: for 5-card hands, flush if one suit has all cards.
            bool isFlush = (handSize == 5) && suitedCards.Any(s => s.Value.Count == 5);

            // Determine straight: sort distinct ranks.
            bool isStraight = false;
            if (handSize == 5)
            {
                var distinctRanks = rankedCards.Keys.OrderBy(r => r).ToList();
                if (distinctRanks.Count == 5)
                {
                    int minRank = distinctRanks.First();
                    int maxRank = distinctRanks.Last();
                    if (maxRank - minRank == 4)
                    {
                        isStraight = true;
                    }
                    else if (distinctRanks.SequenceEqual(new List<int> { 1, 10, 11, 12, 13 }))
                    {
                        isStraight = true;
                    }
                }
            }

            // Count groups from rankedCards.
            int fours = rankedCards.Values.Count(g => g.Count == 4);
            int triples = rankedCards.Values.Count(g => g.Count == 3);
            int pairs = rankedCards.Values.Count(g => g.Count == 2);

            List<Card> scoringCards = new List<Card>();

            // Determine hand category.
            PokerHand category;
            if (isStraight && isFlush)
            {
                var distinctRanks = rankedCards.Keys.OrderBy(r => r).ToList();
                if (distinctRanks.SequenceEqual(new List<int> { 1, 10, 11, 12, 13 }))
                    category = PokerHand.RoyalFlush;
                else
                    category = PokerHand.StraightFlush;
                scoringCards = cards.ToList(); // All cards count.
            }
            else if (fours == 1)
            {
                category = PokerHand.FourOfAKind;
                scoringCards = rankedCards.Values.First(g => g.Count == 4);
            }
            else if (triples == 1 && pairs >= 1)
            {
                category = PokerHand.FullHouse;
                // For a full house, both the triple and pair are core.
                var tripleGroup = rankedCards.Values.First(g => g.Count == 3);
                var pairGroup = rankedCards.Values.First(g => g.Count == 2);
                scoringCards.AddRange(tripleGroup);
                scoringCards.AddRange(pairGroup);
            }
            else if (isFlush)
            {
                category = PokerHand.Flush;
                scoringCards = cards.ToList();
            }
            else if (isStraight)
            {
                category = PokerHand.Straight;
                scoringCards = cards.ToList();
            }
            else if (triples == 1)
            {
                category = PokerHand.ThreeOfAKind;
                scoringCards = rankedCards.Values.First(g => g.Count == 3);
            }
            else if (pairs == 2)
            {
                category = PokerHand.TwoPair;
                // Combine both pairs.
                scoringCards = rankedCards.Values.Where(g => g.Count == 2)
                                                 .SelectMany(g => g)
                                                 .ToList();
            }
            else if (pairs == 1)
            {
                category = PokerHand.Pair;
                scoringCards = rankedCards.Values.First(g => g.Count == 2);
            }
            else
            {
                category = PokerHand.HighCard;
                scoringCards.Add(cards.OrderByDescending(c => c.Rank == Rank.Ace ? 14 : (int)c.Rank).First());
            }

            // Compute the kicker.
            // For hands built from a specific group (Pair, TwoPair, Three/Four-of-a-Kind, FullHouse),
            // the kicker is the maximum rank among the core cards.
            // For hands where all cards count, we take the highest card.
            int kicker = scoringCards.Max(c => c.Rank == Rank.Ace ? 14 : (int)c.Rank);

            // Map the hand category to a factor.
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

            // Compute the effective multiplier from the rarities of only the scoring cards.
            ushort multiplier = (ushort)scoringCards.Aggregate(1, (acc, c) => acc * (int)c.Rarity);

            // Calculate bonus and final score.
            int bonus = (factor - 1) * (factor - 1) * 10;
            int baseScore = factor * kicker + bonus;
            score = (ushort)(baseScore * multiplier);

            scoreCard[0] = multiplier;
            scoreCard[1] = (ushort)factor;
            scoreCard[2] = (ushort)kicker;
            scoreCard[3] = (ushort)bonus;

            return category;
        }

        public static PokerHand OldEvaluate(byte[] attackHand, out ushort score, out ushort[] scoreCard)
        {
            score = 0;

            int handSize = attackHand.Length;
            var cardIndexes = new byte[handSize];
            var rarities = new byte[handSize];
            var ranks = new int[handSize]; // Store rank for each card

            for (int i = 0; i < handSize; i++)
            {
                DecodeCardByte(attackHand[i], out byte cardIndex, out byte rarity);
                cardIndexes[i] = cardIndex;
                rarities[i] = rarity;
                ranks[i] = (cardIndex % 13) + 1; // 1..13 (Ace = 1)
            }

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
            bool isFlush = cardIndexes.Length == 5;
            List<int> straightRanks = new List<int>(); // raw rank values (Ace = 1)
            List<int> kickerRanks = new List<int>();     // for kicker, treat Ace as high (14)

            for (int i = 0; i < handSize; i++)
            {
                byte index = cardIndexes[i];

                if (index > 51)
                {
                    throw new ArgumentOutOfRangeException(nameof(cardIndexes), "Each card index must be between 0 and 51.");
                }

                int rank = ranks[i];
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
            if (cardIndexes.Length == 5 && straightRanks.Distinct().Count() == 5)
            {
                if (maxRank - minRank == 4)
                {
                    isStraight = true;
                }
                // Special case: royal straight (A,10,J,Q,K)
                else if (straightRanks.OrderBy(x => x).SequenceEqual(new List<int> { 1, 10, 11, 12, 13 }))
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
                if (straightRanks.OrderBy(x => x).SequenceEqual(new List<int> { 1, 10, 11, 12, 13 }))
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

            // --- Compute the effective multiplier only from the cards contributing to the hand ---
            List<int> contributingIndices = new List<int>();

            switch (category)
            {
                case PokerHand.HighCard:
                    {
                        // Only the highest card counts.
                        int maxKicker = -1;
                        int idx = -1;
                        for (int i = 0; i < handSize; i++)
                        {
                            int cardKicker = (ranks[i] == 1 ? 14 : ranks[i]);
                            if (cardKicker > maxKicker)
                            {
                                maxKicker = cardKicker;
                                idx = i;
                            }
                        }
                        if (idx != -1)
                            contributingIndices.Add(idx);
                    }
                    break;

                case PokerHand.Pair:
                    {
                        // Find the rank that makes the pair.
                        int pairRank = 0;
                        for (int r = 13; r >= 1; r--)
                        {
                            if (rankCounts[r] == 2)
                            {
                                pairRank = r;
                                break;
                            }
                        }
                        for (int i = 0; i < handSize; i++)
                        {
                            if (ranks[i] == pairRank)
                                contributingIndices.Add(i);
                        }
                    }
                    break;

                case PokerHand.TwoPair:
                    {
                        // Collect indices for both pairs.
                        List<int> pairRanks = new List<int>();
                        for (int r = 1; r <= 13; r++)
                        {
                            if (rankCounts[r] == 2)
                                pairRanks.Add(r);
                        }
                        for (int i = 0; i < handSize; i++)
                        {
                            if (pairRanks.Contains(ranks[i]))
                                contributingIndices.Add(i);
                        }
                    }
                    break;

                case PokerHand.ThreeOfAKind:
                    {
                        int threeRank = 0;
                        for (int r = 13; r >= 1; r--)
                        {
                            if (rankCounts[r] == 3)
                            {
                                threeRank = r;
                                break;
                            }
                        }
                        for (int i = 0; i < handSize; i++)
                        {
                            if (ranks[i] == threeRank)
                                contributingIndices.Add(i);
                        }
                    }
                    break;

                case PokerHand.FourOfAKind:
                    {
                        int fourRank = 0;
                        for (int r = 13; r >= 1; r--)
                        {
                            if (rankCounts[r] == 4)
                            {
                                fourRank = r;
                                break;
                            }
                        }
                        for (int i = 0; i < handSize; i++)
                        {
                            if (ranks[i] == fourRank)
                                contributingIndices.Add(i);
                        }
                    }
                    break;

                // For the remaining categories, all cards in the hand are used.
                default:
                    for (int i = 0; i < handSize; i++)
                    {
                        contributingIndices.Add(i);
                    }
                    break;
            }

            ushort multiplier = 1;
            foreach (int idx in contributingIndices)
            {
                multiplier = (ushort)(multiplier * rarities[idx]);
            }

            // New scoring: score = (factor * kicker) + ((factor - 1)^2 * 10)
            score = (ushort)(factor * kicker + (Math.Pow(factor - 1, 2) * 10));
            score *= multiplier;

            scoreCard = new ushort[4];
            scoreCard[0] = (ushort)multiplier;
            scoreCard[1] = (ushort)factor;
            scoreCard[2] = (ushort)kicker;
            scoreCard[3] = (ushort)(Math.Pow(factor - 1, 2) * 10);

            return category;
        }

        /// <summary>
        /// Evaluates the best attack from a hand of up to 8 card slots.
        /// The hand is represented as a byte array of length 8 where each element is either
        /// a card index (0–51) or DeckAsset.EMPTY_SLOT (indicating an empty slot).
        /// The function returns a BestAttack instance with the best combination of 1 to 5 cards.
        /// </summary>
        /// <param name="hand">An array of 8 bytes representing the hand.</param>
        /// <returns>A BestAttack instance with the best attack combination.</returns>
        public static BestPokerHand EvaluateAttack(byte[] hand)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (hand.Length != DeckAsset.HAND_LIMIT_SIZE)
            {
                throw new ArgumentException($"Hand must be exactly {DeckAsset.HAND_LIMIT_SIZE} cards (slots).", nameof(hand));
            }

            // Get the positions that are not empty.
            List<int> availablePositions = new List<int>();
            for (int i = 0; i < hand.Length; i++)
            {
                if (hand[i] != DeckAsset.EMPTY_SLOT)
                {
                    availablePositions.Add(i);
                }
            }

            if (availablePositions.Count == 0)
            {
                throw new ArgumentException("No cards in hand.", nameof(hand));
            }

            BestPokerHand best = null;

            // Try all combination sizes from 1 to up to 5 cards (or the count of available cards).
            int maxComboSize = Math.Min(5, availablePositions.Count);
            for (int r = 1; r <= maxComboSize; r++)
            {
                foreach (var combo in Combinations(availablePositions, r))
                {
                    // Build the card indexes for this combination.
                    byte[] comboCards = combo.Select(pos => hand[pos]).ToArray();
                    ushort comboScore;
                    PokerHand category = Evaluate(comboCards, out comboScore, out _);
                    BestPokerHand current = new BestPokerHand
                    {
                        Category = category,
                        Score = comboScore,
                        Positions = combo.ToArray(),
                        CardIndexes = comboCards
                    };

                    if (best == null || CompareAttack(current, best) > 0)
                    {
                        best = current;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Compares two BestAttack instances.
        /// Returns a positive number if a is better than b, 0 if equal, negative if a is worse.
        /// Comparison is done first on the PokerHand category (assuming higher enum value is better),
        /// then on the numeric score.
        /// </summary>
        private static int CompareAttack(BestPokerHand a, BestPokerHand b)
        {
            int catComparison = ((int)a.Category).CompareTo((int)b.Category);
            if (catComparison != 0)
            {
                return catComparison;
            }

            return a.Score.CompareTo(b.Score);
        }

        /// <summary>
        /// Generates all combinations (of size k) from a sequence of T.
        /// </summary>
        public static IEnumerable<List<T>> Combinations<T>(IEnumerable<T> elements, int k)
        {
            if (k == 0)
            {
                yield return new List<T>();
                yield break;
            }

            int i = 0;
            foreach (T element in elements)
            {
                IEnumerable<T> remaining = elements.Skip(i + 1);
                foreach (List<T> combination in Combinations(remaining, k - 1))
                {
                    combination.Insert(0, element);
                    yield return combination;
                }
                i++;
            }
        }

        public static string[]? GetBonusInfo(BonusType bonusType)
        {
            return bonusType switch
            {
                BonusType.None => null,
                BonusType.DeckRefill => new string[] { "Deck Refill", "At the start of each combat round, the deck is fully or partially refilled." },
                BonusType.ExtraEndurance => new string[] { "Extra Endurance", "Increase the player's maximum endurance by +1." },
                BonusType.HeartHeal => new string[] { "Heart Heal", "Each Heart card in your hand heals the player for 5 HP." },
                BonusType.DamageBoost => new string[] { "Damage Boost", "Increase overall damage output by 20%." },
                BonusType.ExtraCardDraw => new string[] { "Extra Card Draw", "Increase the hand size by one extra card during the attack phase." },
                BonusType.FaceCardBonus => new string[] { "Face Card Bonus", "If your attack hand consists solely of face cards (J, Q, K), your poker hand score is multiplied by 2." },
                BonusType.SuitDiversityBonus => new string[] { "Suit Diversity Bonus", "Your poker hand score is multiplied by the number of different suits present in your hand." },
                BonusType.LuckyDraw => new string[] { "Lucky Draw", "Increases the chance to draw high cards." },
                BonusType.CriticalStrikeChance => new string[] { "Critical Strike Chance", "Increases chance for critical hit damage." },
                BonusType.RapidRecovery => new string[] { "Rapid Recovery", "Reduces cooldown between rounds." },
                BonusType.ShieldOfValor => new string[] { "Shield of Valor", "Grants temporary damage reduction for one round." },
                BonusType.MysticInsight => new string[] { "Mystic Insight", "Reveals one additional card from the deck." },
                BonusType.ArcaneSurge => new string[] { "Arcane Surge", "Boosts attack score by a flat bonus." },
                BonusType.RighteousFury => new string[] { "Righteous Fury", "Increases damage output when below half health." },
                BonusType.BlessedAura => new string[] { "Blessed Aura", "Heals a small amount each round." },
                BonusType.FortunesFavor => new string[] { "Fortune's Favor", "Slightly increases overall hand evaluation score." },
                BonusType.NimbleFingers => new string[] { "Nimble Fingers", "Allows an extra card swap per round." },
                BonusType.EagleEye => new string[] { "Eagle Eye", "Improves card selection accuracy." },
                BonusType.UnyieldingSpirit => new string[] { "Unyielding Spirit", "Prevents endurance loss once per game." },
                BonusType.DivineIntervention => new string[] { "Divine Intervention", "Randomly nullifies one enemy effect." },
                BonusType.ZealousCharge => new string[] { "Zealous Charge", "Increases attack speed for the next round." },
                BonusType.RelentlessAssault => new string[] { "Relentless Assault", "Increases consecutive attack bonus." },
                BonusType.VitalStrike => new string[] { "Vital Strike", "Boosts damage for high-value cards." },
                BonusType.PurityOfHeart => new string[] { "Purity of Heart", "Reduces negative effects from banes." },
                BonusType.CelestialGuidance => new string[] { "Celestial Guidance", "Improves probability of drawing rare cards." },
                BonusType.SwiftReflexes => new string[] { "Swift Reflexes", "Reduces chance of fatigue damage." },
                BonusType.InspiringPresence => new string[] { "Inspiring Presence", "Boosts team morale, increasing score slightly." },
                BonusType.Serendipity => new string[] { "Serendipity", "Randomly upgrades one card's value each round." },
                BonusType.ArcaneWisdom => new string[] { "Arcane Wisdom", "Grants extra strategic information about the deck." },
                BonusType.MajesticRoar => new string[] { "Majestic Roar", "Boosts your hand's overall score by a flat bonus." },
                BonusType.FortuitousWinds => new string[] { "Fortuitous Winds", "Increases overall card quality by a small margin." },
                BonusType.StalwartResolve => new string[] { "Stalwart Resolve", "Improves defense, slightly increasing healing effectiveness." },
                _ => null,
            };
        }

        public static string[]? GetMalusInfo(MalusType malusType)
        {
            return malusType switch
            {
                MalusType.None => null,
                MalusType.HalvedDamage => new string[] { "Halved Damage", "All damage output is reduced by 50%." },
                MalusType.SpadeHeal => new string[] { "Spade Heals Opponent", "Each Spade card played heals the opponent for 3 HP." },
                MalusType.ReducedEndurance => new string[] { "Reduced Endurance", "Decrease the player's maximum endurance by 1." },
                MalusType.IncreasedFatigueRate => new string[] { "Increased Fatigue Rate", "Fatigue damage increases at an accelerated exponential rate." },
                MalusType.LowerCardValue => new string[] { "Lower Card Value", "All card values are reduced by 20%, weakening potential hands." },
                MalusType.NumberCardPenalty => new string[] { "Number Card Penalty", "If your attack hand contains any number cards (2–10), your poker hand score is halved." },
                MalusType.UniformSuitPenalty => new string[] { "Uniform Suit Penalty", "If your attack hand consists of only one suit, your poker hand score is reduced by 50%." },
                MalusType.Misfortune => new string[] { "Misfortune", "Increases chance of drawing low cards." },
                MalusType.SluggishRecovery => new string[] { "Sluggish Recovery", "Increases endurance consumption." },
                MalusType.CursedDraw => new string[] { "Cursed Draw", "Reduces the quality of drawn cards." },
                MalusType.WeakStrike => new string[] { "Weak Strike", "Decreases your damage output by 10%." },
                MalusType.UnluckyTiming => new string[] { "Unlucky Timing", "Delays the next round's attack." },
                MalusType.BlightedAura => new string[] { "Blighted Aura", "Increases chance for negative card effects." },
                MalusType.CrumblingDeck => new string[] { "Crumbling Deck", "Decreases deck refill efficiency." },
                MalusType.DiminishedInsight => new string[] { "Diminished Insight", "Loses one extra card from hand randomly." },
                MalusType.VulnerableState => new string[] { "Vulnerable State", "Increases damage taken from boss by 20%." },
                MalusType.RecklessPlay => new string[] { "Reckless Play", "Increases chance of self-inflicted fatigue damage." },
                MalusType.WeakenedSpirit => new string[] { "Weakened Spirit", "Reduces overall hand score by 10%." },
                MalusType.GrimFate => new string[] { "Grim Fate", "Decreases chance for critical hits." },
                MalusType.SlipperyFingers => new string[] { "Slippery Fingers", "Increases likelihood of misplays." },
                MalusType.BloodCurse => new string[] { "Blood Curse", "Transfers 5% of your damage to the opponent." },
                MalusType.Despair => new string[] { "Despair", "Lowers your damage boost effects by 20%." },
                MalusType.FracturedWill => new string[] { "Fractured Will", "Reduces extra endurance benefits." },
                MalusType.EnervatingPresence => new string[] { "Enervating Presence", "Decreases your overall card evaluation score." },
                MalusType.MisalignedFocus => new string[] { "Misaligned Focus", "Increases the chance of drawing duplicate cards." },
                MalusType.HeavyBurden => new string[] { "Heavy Burden", "Reduces the effectiveness of healing boons." },
                MalusType.StumblingStep => new string[] { "Stumbling Step", "Slightly reduces the chance for bonus attacks." },
                MalusType.DimmedVision => new string[] { "Dimmed Vision", "Lowers your probability of drawing high cards." },
                MalusType.FadingResolve => new string[] { "Fading Resolve", "Decreases your strategic planning by a small margin." },
                MalusType.ToxicMiasma => new string[] { "Toxic Miasma", "Every drawn Heart card now inflicts minor damage on you." },
                MalusType.CursedFate => new string[] { "Cursed Fate", "Reduces the effectiveness of any boons by 10%." },
                MalusType.SourLuck => new string[] { "Sour Luck", "Increases the probability of drawing detrimental cards." },
                _ => null,
            };
        }

        /// <summary>
        /// Decodes a card byte into its card index and rarity.
        /// </summary>
        /// <param name="encodedCard"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        public static void DecodeCardByte(byte encodedCard, out byte cardIndex, out byte rarity)
        {
            cardIndex = (byte)(encodedCard & 0x3F);
            rarity = (byte)((encodedCard >> 6) & 0x03);
        }

        /// <summary>
        /// Encodes a card index and rarity into a single byte.
        /// </summary>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <returns></returns>
        public static byte EncodeCardByte(byte cardIndex, byte rarity)
        {
            return (byte)((cardIndex & 0b0011_1111) | ((rarity & 0b0000_0011) << 6));
        }

        /// <summary>
        /// Determines the upgrade price for a given feature type and level.
        /// </summary>
        /// <param name="featureType"></param>
        /// <param name="featureEnum"></param>
        /// <param name="level"></param>
        /// <param name="baseAssets"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public static bool UpgradeInfo(FeatureType featureType, byte featureEnum, byte level, IAsset[] baseAssets, out byte price)
        {
            return Upgrade(false, featureType, featureEnum, level, baseAssets, out price);
        }

        /// <summary>
        /// Determines the upgrade price for a given feature type and level.
        /// </summary>
        /// <param name="doUpgrade"></param>
        /// <param name="featureType"></param>
        /// <param name="featureEnum"></param>
        /// <param name="level"></param>
        /// <param name="baseAssets"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private static bool Upgrade(bool doUpgrade, FeatureType featureType, byte featureEnum, byte level, IAsset[] baseAssets, out byte price)
        {
            var game = baseAssets[0] as GameAsset;
            var deck = baseAssets[1] as DeckAsset;
            var towr = baseAssets[2] as TowerAsset;

            price = 0;

            if (game == null || deck == null || towr == null)
            {
                return false;
            }

            switch (featureType)
            {
                case FeatureType.RarityLevel:
                    return Upgrade(doUpgrade, (RarityType)featureEnum, level, deck, out price);

                case FeatureType.PokerHandLevel:
                    return Upgrade(doUpgrade, (PokerHand)featureEnum, level, deck, out price);

                case FeatureType.None:
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Determines the upgrade price for a given rarity level and level.
        /// </summary>
        /// <param name="featureEnum"></param>
        /// <param name="level"></param>
        /// <param name="deck"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private static bool Upgrade(bool doUpgrade, RarityType featureEnum, byte level, DeckAsset deck, out byte price)
        {
            price = 0;
            var currentLevel = deck.GetRarity(featureEnum);

            if (currentLevel == level || level > DeckAsset.MAX_RARITY_LEVEL) 
            { 
                return false;
            }

            if (level - currentLevel != 1)
            {
                return false;
            }

            switch (featureEnum)
            {
                case RarityType.Uncommon:
                case RarityType.Rare:
                case RarityType.Epic:
                case RarityType.Legendary:
                    price = (byte)(level * (int)featureEnum);
                    break;

                case RarityType.Mythical:
                case RarityType.Common:
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the upgrade price for a given poker hand feature and level.
        /// </summary>
        /// <param name="doUpgrade"></param>
        /// <param name="featureEnum"></param>
        /// <param name="level"></param>
        /// <param name="deck"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private static bool Upgrade(bool doUpgrade, PokerHand featureEnum, byte level, DeckAsset deck, out byte price)
        {
            price = 0;
            var currentLevel = deck.GetPokerHandLevel(featureEnum);

            if (currentLevel == level || level > DeckAsset.MAX_POKERHAND_LEVEL)
            {
                return false;
            }

            switch (featureEnum)
            {
                case PokerHand.HighCard:
                case PokerHand.Pair:
                case PokerHand.TwoPair:
                case PokerHand.ThreeOfAKind:
                case PokerHand.Straight:
                case PokerHand.Flush:
                case PokerHand.FullHouse:
                case PokerHand.FourOfAKind:
                case PokerHand.StraightFlush:
                case PokerHand.RoyalFlush:
                    price = (byte)level;
                    break;

                case PokerHand.None:
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to upgrade a feature to a given level.
        /// </summary>
        /// <param name="featureType"></param>
        /// <param name="featureEnum"></param>
        /// <param name="level"></param>
        /// <param name="baseAssets"></param>
        /// <returns></returns>
        public static bool TryUpgrade(FeatureType featureType, byte featureEnum, byte level, IAsset[] baseAssets)
        {
            if (!UpgradeInfo(featureType, featureEnum, level, baseAssets, out byte price))
            {
                return false;
            }

            var game = baseAssets[0] as GameAsset;
            var deck = baseAssets[1] as DeckAsset;
            var towr = baseAssets[2] as TowerAsset;

            if (game == null || price > game.Token)
            {
                return false;
            }

            if (!Upgrade(true, featureType, featureEnum, level, baseAssets, out _))
            {
                return false;
            }

            // pay upgrade price
            game.Token -= price;

            return Upgrade(true, featureType, featureEnum, level, baseAssets, out _);

        }

    }
}
