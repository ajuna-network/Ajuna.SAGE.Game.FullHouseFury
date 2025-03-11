using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;
using System.Data;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    public partial class DeckAsset : BaseAsset
    {
        public const byte EMPTY_SLOT = 63;

        public const byte DECK_LIMIT_SIZE = 62;
        public const byte HAND_LIMIT_SIZE = 10;

        public DeckAsset(uint ownerId, uint genesis)
            : base(ownerId, genesis)
        {
            AssetType = AssetType.Deck;

            // Initialize deck region: first 52 bits set to 1.
            NewDeck();

            // Initialize hand region: all slots empty.
            EmptyHand();
        }

        public DeckAsset(IAsset asset)
            : base(asset)
        { }

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
                byte[] deckBytes = Data.Read(8, 8);
                return BitConverter.ToUInt64(deckBytes, 0);
            }
            set
            {
                byte[] deckBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 8; i++)
                {
                    Data.Set((byte)(8 + i), ByteType.Full, deckBytes[i]);
                }
            }
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ XXXXXXXX ........
        public ulong Hand
        {
            get
            {
                byte[] handBytes = Data.Read(16, 8);
                return BitConverter.ToUInt64(handBytes, 0);
            }
            set
            {
                byte[] handBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 8; i++)
                {
                    Data.Set((byte)(16 + i), ByteType.Full, handBytes[i]);
                }
            }
        }
    }

    /// <summary>
    /// Deck-specific methods.
    /// </summary>
    public partial class DeckAsset
    {
        public void NewDeck()
        {
            Deck = ulong.MaxValue;
            MaxDeckSize = 52;
            DeckSize = MaxDeckSize;
        }

        public bool GetCardState(byte index)
        {
            if (index >= MaxDeckSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ((Deck >> index) & 1UL) == 1UL;
        }

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
        public void SetHandCard(int handPosition, byte cardIndex)
        {
            if (handPosition < 0 || handPosition >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), $"Hand position must be between 0 and <{HAND_LIMIT_SIZE}.");
            }

            if (cardIndex >= MaxDeckSize && cardIndex != EMPTY_SLOT)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), $"Card index must be between 0 and <{MaxDeckSize}.");
            }

            ulong handValue = Hand;
            int bitOffset = handPosition * 6;
            ulong mask = 0x3FUL << bitOffset;
            handValue = (handValue & ~mask) | (((ulong)cardIndex & 0x3F) << bitOffset);
            Hand = handValue;
        }

        public byte GetHandCard(int handPosition)
        {
            if (handPosition < 0 || handPosition >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), $"Hand position must be between 0 and <{HAND_LIMIT_SIZE}.");
            }

            ulong handValue = Hand;
            int bitOffset = handPosition * 6;
            return (byte)((handValue >> bitOffset) & 0x3F);
        }

        public void EmptyHand()
        {
            ulong empty = 0;
            for (int i = 0; i < HAND_LIMIT_SIZE; i++)
            {
                int bitOffset = i * 6;
                empty |= ((ulong)EMPTY_SLOT & 0x3F) << bitOffset;
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
        public void Draw(byte handSize, byte[] randomHash)
        {
            if (handSize > HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handSize), "Hand size cannot exceed maximum hand size (10).");
            }

            // Count how many cards are already in hand.
            int currentCount = HandCardsCount();

            // Fill empty slots until we reach the desired hand size.
            for (int i = 0; i < HAND_LIMIT_SIZE && currentCount < handSize; i++)
            {
                if (IsHandSlotEmpty(i))
                {
                    // If the deck is empty, exit early.
                    if (DeckSize == 0)
                    {
                        break;
                    }

                    byte randomByte = (byte)(randomHash[i % randomHash.Length] % DeckSize);

                    byte drawnCard = DrawCard(randomByte);
                    SetHandCard(i, drawnCard);
                    currentCount++;
                }
            }
        }

        public bool TryGetHandCard(int handPosition, out byte cardIndex)
        {
            if (handPosition < 0 || handPosition >= HAND_LIMIT_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 9.");
            }
            cardIndex = GetHandCard(handPosition);
            return cardIndex != EMPTY_SLOT;
        }

        public bool IsHandSlotEmpty(int handPosition) => GetHandCard(handPosition) == EMPTY_SLOT;

        public bool IsHandSlotOccupied(int handPosition) => !IsHandSlotEmpty(handPosition);
    }
}