using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;
using System.Data;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    public partial class DeckAsset : BaseAsset
    {
        // Common constants and initialization.
        public const byte DECK_OFFSET = 8;

        public const byte HAND_OFFSET = 16;
        public const byte REGION_SIZE = 8; // 8 bytes for both deck and hand regions.
        public const byte EMPTY_SLOT = 63;

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

        public byte DeckSize 
        { 
            get => Data.Read(7, ByteType.Full); 
            set => Data.Set(7, ByteType.Full, value);
        }

        // Helpers to read and write the deck and hand regions.
        public ulong Deck
        {
            get
            {
                byte[] deckBytes = Data.Read(DECK_OFFSET, REGION_SIZE);
                return BitConverter.ToUInt64(deckBytes, 0);
            }
            set
            {
                byte[] deckBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < REGION_SIZE; i++)
                {
                    Data.Set((byte)(DECK_OFFSET + i), ByteType.Full, deckBytes[i]);
                }
            }
        }

        public ulong Hand
        {
            get
            {
                byte[] handBytes = Data.Read(HAND_OFFSET, REGION_SIZE);
                return BitConverter.ToUInt64(handBytes, 0);
            }
            set
            {
                byte[] handBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < REGION_SIZE; i++)
                {
                    Data.Set((byte)(HAND_OFFSET + i), ByteType.Full, handBytes[i]);
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
            Data.Set(DECK_OFFSET, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            DeckSize = 52;
        }

        public bool GetCardState(byte index)
        {
            if (index >= 52)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ((Deck >> index) & 1UL) == 1UL;
        }

        public void SetCardState(byte index, bool state)
        {
            if (index >= 52)
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
            for (byte i = 0; i < 52; i++)
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
            if (handPosition < 0 || handPosition >= 10)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 9.");
            }

            if (cardIndex > 51)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index must be between 0 and 51.");
            }

            ulong handValue = Hand;
            int bitOffset = handPosition * 6;
            ulong mask = 0x3FUL << bitOffset;
            handValue = (handValue & ~mask) | (((ulong)cardIndex & 0x3F) << bitOffset);
            Hand = handValue;
        }

        public byte GetHandCard(int handPosition)
        {
            if (handPosition < 0 || handPosition >= 10)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 9.");
            }

            ulong handValue = Hand;
            int bitOffset = handPosition * 6;
            return (byte)((handValue >> bitOffset) & 0x3F);
        }

        public void EmptyHand()
        {
            ulong empty = 0;
            for (int i = 0; i < 10; i++)
            {
                int bitOffset = i * 6;
                empty |= ((ulong)EMPTY_SLOT & 0x3F) << bitOffset;
            }
            Hand = empty;
        }

        public void Draw(byte handSize, byte[] randomHash)
        {
            if (handSize > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(handSize), "Hand size cannot exceed maximum hand size (10).");
            }

            // Count how many cards are already in hand.
            int currentCount = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!IsHandSlotEmpty(i))
                {
                    currentCount++;
                }
            }

            // Fill empty slots until we reach the desired hand size.
            for (int i = 0; i < 10 && currentCount < handSize; i++)
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


        public bool IsHandSlotEmpty(int handPosition) => GetHandCard(handPosition) == EMPTY_SLOT;

        public bool IsHandSlotOccupied(int handPosition) => !IsHandSlotEmpty(handPosition);
    }
}