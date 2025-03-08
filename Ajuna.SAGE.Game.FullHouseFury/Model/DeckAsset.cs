using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;
using System.ComponentModel;
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

        public int CountCardsInDeck()
        {
            int count = 0;
            ulong deckValue = Deck;
            for (int i = 0; i < 52; i++)
            {
                if (((deckValue >> i) & 1UL) == 1UL)
                {
                    count++;
                }
            }
            return count;
        }

        public byte RemoveCardFromDeck(byte index)
        {
            if (!GetCardState(index))
            {
                throw new InvalidOperationException("Card is not in the deck.");
            }

            SetCardState(index, false);
            return index;
        }

        public byte DrawRandomCard(Random random)
        {
            int availableCount = CountCardsInDeck();
            if (availableCount == 0)
            {
                throw new InvalidOperationException("Deck is empty.");
            }

            int chosenIndex = random.Next(availableCount);
            int currentCount = 0;
            for (byte i = 0; i < 52; i++)
            {
                if (GetCardState(i))
                {
                    if (currentCount == chosenIndex)
                    {
                        RemoveCardFromDeck(i);
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
        public void SetCardInHand(int handPosition, byte cardIndex)
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

        public byte GetCardInHand(int handPosition)
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

        public bool IsHandSlotEmpty(int handPosition)
        {
            return GetCardInHand(handPosition) == EMPTY_SLOT;
        }

        public bool IsHandSlotOccupied(int handPosition)
        {
            return !IsHandSlotEmpty(handPosition);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public struct Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public readonly byte Index => (byte)((byte)Suit * 13 + (byte)Rank - 1);

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public Card(byte cardIndex)
        {
            if (cardIndex > 51)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index must be between 0 and 51.");
            }
            // Use division and modulo to map card index to suit and rank.
            Suit = (Suit)(cardIndex / 13);
            Rank = (Rank)((cardIndex % 13) + 1);
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}