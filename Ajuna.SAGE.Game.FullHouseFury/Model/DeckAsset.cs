using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;
using System.Linq;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    public partial class DeckAsset : BaseAsset
    {
        public const byte EMPTY_SLOT = 63;

        public const byte DECK_LIMIT_SIZE = 62;
        public const byte HAND_LIMIT_SIZE = 8;

        public const byte MAX_RARITY_LEVEL = 3; 
        public const byte MAX_POKERHAND_LEVEL = 7;

        public DeckAsset(uint ownerId, uint genesis)
            : base(ownerId, genesis)
        {
            AssetType = AssetType.Deck;

            // Initialize deck region: first 52 bits set to 1.
            New();

            // Initialize hand region: all slots empty.
            EmptyHand();
        }

        public DeckAsset(IAsset asset)
            : base(asset) { }

        public byte DeckRefill
        {
            get => Data.Read<byte>(1);
            set => Data.Set<byte>(1, value);
        }

        public byte DrawRarity
        {
            get => Data.Read<byte>(2);
            set => Data.Set<byte>(2, value);
        }

        public byte MaxDeckSize
        {
            get => Data.Read<byte>(3);
            set => Data.Set<byte>(3, value);
        }

        public byte DeckSize
        {
            get => Data.Read<byte>(4);
            set => Data.Set<byte>(4, value);
        }

        private ulong Deck
        {
            get => Data.Read<ulong>(5);
            set => Data.Set<ulong>(5, value);
        }

        private ulong Hand
        {
            get => Data.Read<ulong>(13);
            set => Data.Set<ulong>(13, value);
        }

        private uint PokerHandLevel
        {
            get => Data.Read<uint>(21);
            set => Data.Set<uint>(21, value);
        }
    }

    /// <summary>
    /// Deck-specific methods.
    /// </summary>
    public partial class DeckAsset
    {
        /// <summary>
        /// Only use this at the start of a new game, not during a game
        /// </summary>
        public void New()
        {
            DeckRefill = 0;
            DrawRarity = 0;
            Deck = ulong.MaxValue;
            MaxDeckSize = 52;
            DeckSize = MaxDeckSize;

            SetRarity(RarityType.Uncommon, 1);
        }

        /// <summary>
        /// Reset the deck for the next level.
        /// </summary>
        public void Reset()
        {
            Deck = ulong.MaxValue;
            DeckSize = MaxDeckSize;
        }

        /// <summary>
        /// Get the state of a card in the deck.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool GetCardState(byte index)
        {
            if (index >= MaxDeckSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ((Deck >> index) & 1UL) == 1UL;
        }

        /// <summary>
        /// Set the state of a card in the deck.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetCardState(byte index, bool state)
        {
            if (index >= MaxDeckSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ulong deckValue = Deck;
            if (state)
            {
                deckValue |= (1UL << index);
            }
            else
            {
                deckValue &= ~(1UL << index);
            }

            Deck = deckValue;
        }

        /// <summary>
        /// Add a card to the deck.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte AddCard(byte index)
        {
            if (GetCardState(index))
            {
                throw new InvalidOperationException("Card is already in the deck.");
            }

            SetCardState(index, true);
            DeckSize++;
            return index;
        }

        /// <summary>
        /// Remove a card from the deck.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte RemoveCard(byte index)
        {
            if (!GetCardState(index))
            {
                throw new InvalidOperationException("Card is not in the deck.");
            }

            SetCardState(index, false);
            DeckSize--;
            return index;
        }

        /// <summary>
        /// Draw a random card from the deck.
        /// </summary>
        /// <param name="randomByte"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public byte DrawCard(byte randomByte)
        {
            if (DeckSize == 0)
            {
                throw new InvalidOperationException("Deck is empty.");
            }

            if (DeckSize <= randomByte)
            {
                throw new ArgumentOutOfRangeException(nameof(randomByte), "Random byte must be less than deck size.");
            }

            int currentCount = 0;
            for (byte i = 0; i < MaxDeckSize; i++)
            {
                if (GetCardState(i))
                {
                    if (currentCount == randomByte)
                    {
                        RemoveCard(i);
                        return i;
                    }
                    currentCount++;
                }
            }
            throw new Exception("Failed to draw a random card.");
        }
    }

    /// <summary>
    /// Hand-specific methods.
    /// </summary>
    public partial class DeckAsset
    {
        /// <summary>
        /// Set the card at the specified hand position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetHandCard(int position, byte cardIndex, byte rarity)
        {
            if (cardIndex >= MaxDeckSize && cardIndex != EMPTY_SLOT)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), $"Card index must be between 0 and {MaxDeckSize - 1}.");
            }

            if (cardIndex > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex));
            }

            if (rarity > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(rarity));
            }

            // make sure rarity is set to 0 if cardIndex is EMPTY_SLOT
            if (cardIndex == EMPTY_SLOT)
            {
                rarity = 0;
            }

            SetHandCard(position, FullHouseFuryUtil.EncodeCardByte(cardIndex, rarity));
        }

        /// <summary>
        /// Set the card at the specified hand position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="encodedCard"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetHandCard(int position, byte encodedCard)
        {
            if (position < 0 || position >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Hand position must be between 0 and {HAND_LIMIT_SIZE - 1}.");
            }

            var bytes = BitConverter.GetBytes(Hand);
            bytes[position] = encodedCard;
            Hand = BitConverter.ToUInt64(bytes);
        }

        /// <summary>
        /// Get the card at the specified hand position.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte GetHandCard(int pos, out byte cardIndex, out byte rarity)
        {
            var bytes = BitConverter.GetBytes(Hand);
            if (pos < 0 || pos >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(pos), $"Hand position must be between 0 and {HAND_LIMIT_SIZE - 1}.");
            }

            FullHouseFuryUtil.DecodeCardByte(bytes[pos], out cardIndex, out rarity);
            return bytes[pos];
        }


        /// <summary>
        /// Clear all cards from the hand.
        /// </summary>
        public void EmptyHand()
        {
            var empty = new byte[HAND_LIMIT_SIZE];
            for (int i = 0; i < HAND_LIMIT_SIZE; i++)
            {

                empty[i] = FullHouseFuryUtil.EncodeCardByte(EMPTY_SLOT, 0);
            }
            Hand = BitConverter.ToUInt64(empty);
        }

        /// <summary>
        /// Count how many cards are already in hand.
        /// </summary>
        /// <returns></returns>
        public int HandCardsCount()
        {
            // Count how many cards are already in hand.
            int currentCount = 0;
            for (int i = 0; i < HAND_LIMIT_SIZE; i++)
            {
                if (!IsHandSlotEmpty(i))
                {
                    currentCount++;
                }
            }
            return currentCount;
        }

        /// <summary>
        /// Draw cards from the deck and fill empty slots in hand, up to the actual hand size.
        /// </summary>
        /// <param name="handSize"></param>
        /// <param name="randomHash"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Draw(byte handSize, byte[] randomHash, out byte[] newCards)
        {
            if (handSize > HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handSize), "Hand size cannot exceed maximum hand size (10).");
            }

            newCards = new byte[] { };

            // Count how many cards are already in hand.
            int startHandSize = HandCardsCount();

            if (startHandSize >= handSize)
            {
                // If the hand is already full, exit early.
                return;
            }

            newCards = new byte[handSize - startHandSize];

            var currentHandSize = startHandSize;
            // Fill empty slots until we reach the desired hand size.
            for (int i = 0; i < HAND_LIMIT_SIZE && currentHandSize < handSize; i++)
            {

                if (IsHandSlotEmpty(i))
                {
                    // If the deck is empty, exit early.
                    if (DeckSize == 0)
                    {
                        break;
                    }

                    var randA = randomHash[(i * 2) % randomHash.Length];
                    var randB = randomHash[((i * 2) + 1) % randomHash.Length];

                    byte randCardIndex = (byte)(randA % DeckSize);
                    byte drawnCard = DrawCard(randCardIndex);

                    var rarityPerc = (double)randB * 100 / byte.MaxValue;
                    var rarity = EvaluateRarity(rarityPerc);

                    SetHandCard(i, drawnCard, (byte)rarity);
                    newCards[currentHandSize - startHandSize] = drawnCard;
                    currentHandSize++;
                }
            }
        }

        /// <summary>
        /// Get the card at the specified hand position.
        /// </summary>
        /// <param name="handPosition"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool TryGetHandCard(int handPosition, out byte cardIndex, out byte rarity)
        {
            if (handPosition < 0 || handPosition >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 9.");
            }
            GetHandCard(handPosition, out cardIndex, out rarity);
            return true;
        }

        public bool IsHandSlotEmpty(int position) => TryGetHandCard(position, out byte cardIndex, out _) && cardIndex == EMPTY_SLOT;
    }

    /// <summary>
    /// PokerLevel specific methods.
    /// </summary>
    public partial class DeckAsset
    {
        private const int BitsPerLevel = 3;

        /// <summary>
        /// Gets the 3-bit level (0–7) for the specified poker hand index (0–9).
        /// </summary>
        public byte GetPokerHandLevel(PokerHand pokerHand)
        {
            var index = (int)pokerHand;
            int bitOffset = index * BitsPerLevel;
            // Extract 3 bits corresponding to the level.
            return (byte)((PokerHandLevel >> bitOffset) & 0x7U);
        }

        /// <summary>
        /// Gets the 3-bit level (0–7) for all poker hands.
        /// </summary>
        /// <returns></returns>
        public byte[] PokerHandLevels()
            => Enumerable.Range((int)PokerHand.HighCard, (int)PokerHand.RoyalFlush)
                .Select(i => GetPokerHandLevel((PokerHand)i))
                .ToArray();

        /// <summary>
        /// Sets the 3-bit level (0–7) for the specified poker hand index (0–9).
        /// </summary>
        public void SetPokerHandLevel(PokerHand pokerHand, byte levelValue)
        {
            if (levelValue > MAX_POKERHAND_LEVEL)
            {
                throw new ArgumentOutOfRangeException(nameof(levelValue), $"Level value must be between 0 and {MAX_POKERHAND_LEVEL}.");
            }

            var index = (int)pokerHand;
            int bitOffset = index * BitsPerLevel;
            uint levels = PokerHandLevel;
            // Clear the 3 bits for this poker hand.
            levels &= ~(0x7U << bitOffset);
            // Set the new level value.
            levels |= ((uint)levelValue & 0x7U) << bitOffset;
            PokerHandLevel = levels;
        }
    }

    /// <summary>
    /// Rarity specific methods.
    /// </summary>
    public partial class DeckAsset
    {
        public byte GetRarity(RarityType rarity)
        {
            switch (rarity)
            {
                case RarityType.Common:
                    return 0;
                case RarityType.Uncommon:
                    return (byte)((0b0000_0011 & DrawRarity) >> 0);
                case RarityType.Rare:
                    return (byte)((0b0000_1100 & DrawRarity) >> 2);
                case RarityType.Epic:
                    return (byte)((0b0011_0000 & DrawRarity) >> 4);
                case RarityType.Legendary:
                    return (byte)((0b1100_0000 & DrawRarity) >> 6);
                case RarityType.Mythical:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rarity), "Invalid rarity value.");
            }
        }

        public void SetRarity(RarityType rarity, byte value)
        {
            if (value > MAX_RARITY_LEVEL)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Rarity value must be between 0 and {MAX_RARITY_LEVEL}.");
            }

            switch(rarity)
            {
                case RarityType.Common:
                    // nothing to store
                    break;
                case RarityType.Uncommon:
                    DrawRarity = (byte)((DrawRarity & ~0b0000_0011) | (value << 0));
                    break;
                case RarityType.Rare:
                    DrawRarity = (byte)((DrawRarity & ~0b0000_1100) | (value << 2));
                    break;
                case RarityType.Epic:
                    DrawRarity = (byte)((DrawRarity & ~0b0011_0000) | (value << 4));
                    break;
                case RarityType.Legendary:
                    DrawRarity = (byte)((DrawRarity & ~0b1100_0000) | (value << 6));
                    break;
                case RarityType.Mythical:
                    // nothing to store
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rarity), "Invalid rarity value.");
            }
        }

        /// <summary>
        /// Get the rarity percentages.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRarityPercs()
        {
            return new byte[] {
                0,
                GetRarity(RarityType.Common),
                (byte)(4 * GetRarity(RarityType.Uncommon)),
                (byte)(3 * GetRarity(RarityType.Rare)),
                (byte)(2 * GetRarity(RarityType.Epic)),
                (byte)(1 * GetRarity(RarityType.Legendary)),
                0
            };
        }

        /// <summary>
        /// Evaluate the rarity of a card based on a random value.
        /// </summary>
        /// <param name="rarityValue"></param>
        /// <returns></returns>
        public RarityType EvaluateRarity(double rarityValue)
        {
            var rarityPercs = GetRarityPercs();

            var rarityPercCum = 0;
            foreach (RarityType rarity in Enum.GetValues(typeof(RarityType)))
            {
                rarityPercCum += rarityPercs[(int)rarity];
                if (rarityValue < rarityPercCum)
                {
                    return rarity;
                }
            }

            return RarityType.Common;
        }
    }
}