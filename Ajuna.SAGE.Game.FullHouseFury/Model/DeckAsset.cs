using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;

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
            : base(asset)
        { }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ....H... ........ ........ ........
        public byte DeckRefill
        {
            get => Data.Read(4, ByteType.High);
            set => Data?.Set(4, ByteType.High, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .....X.. ........ ........ ........
        public byte DrawRarity
        {
            get => Data.Read(5, ByteType.Full);
            set => Data?.Set(5, ByteType.Full, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ......X. ........ ........ ........

        public byte MaxDeckSize
        {
            get => Data.Read(6, ByteType.Full);
            set => Data.Set(6, ByteType.Full, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .......X ........ ........ ........

        public byte DeckSize
        {
            get => Data.Read(7, ByteType.Full);
            set => Data.Set(7, ByteType.Full, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ XXXXXXXX ........ ........
        public ulong Deck
        {
            get
            {
                byte[] bytes = Data.Read(8, 8);
                return BitConverter.ToUInt64(bytes, 0);
            }
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 8; i++)
                {
                    Data.Set((byte)(8 + i), ByteType.Full, bytes[i]);
                }
            }
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ XXXXXXXX ........
        private byte[] Hand
        {
            get => Data.Read(16, 8);
            set => Data.Set(16, value);
        }

        /// Encodes 10 poker hand levels in 30 bits (stored in 4 bytes).
        /// Each level uses 3 bits (values 0–7).
        /// The 2 highest bits remain unused.
        /// </summary>
        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ XXXX....
        /// <summary>
        public uint PokerHandLevel
        {
            get
            {
                // Read exactly 4 bytes from offset 24.
                byte[] levelBytes = Data.Read(24, 4);
                return BitConverter.ToUInt32(levelBytes, 0);
            }
            set
            {
                byte[] levelBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    Data.Set((byte)(24 + i), ByteType.Full, levelBytes[i]);
                }
            }
        }
    }

    /// <summary>
    /// Deck-specific methods.
    /// </summary>
    public partial class DeckAsset
    {
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

            var hand = Hand;
            hand[position] = encodedCard;
            Hand = hand;
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
            if (pos < 0 || pos >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(pos), $"Hand position must be between 0 and {HAND_LIMIT_SIZE - 1}.");
            }

            FullHouseFuryUtil.DecodeCardByte(Hand[pos], out cardIndex, out rarity);
            return Hand[pos];
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
            Hand = empty;
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
            if (index < 0 || index > (int)PokerHand.RoyalFlush)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int bitOffset = index * BitsPerLevel;
            // Extract 3 bits corresponding to the level.
            return (byte)((PokerHandLevel >> bitOffset) & 0x7U);
        }

        /// <summary>
        /// Sets the 3-bit level (0–7) for the specified poker hand index (0–9).
        /// </summary>
        public void SetPokerHandLevel(PokerHand pokerHand, byte levelValue)
        {
            var index = (int)pokerHand;
            if (index < 0 || index > (int)PokerHand.RoyalFlush)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (levelValue > MAX_POKERHAND_LEVEL)
            {
                throw new ArgumentOutOfRangeException(nameof(levelValue), $"Level value must be between 0 and {MAX_POKERHAND_LEVEL}.");
            }

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
                    DrawRarity = (byte)(DrawRarity | value);
                    break;
                case RarityType.Rare:
                    DrawRarity = (byte)(DrawRarity | (value << 2));
                    break;
                case RarityType.Epic:
                    DrawRarity = (byte)(DrawRarity | (value << 4));
                    break;
                case RarityType.Legendary:
                    DrawRarity = (byte)(DrawRarity | (value << 6));
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