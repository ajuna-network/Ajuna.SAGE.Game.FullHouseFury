using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct Card
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
            var suitUnicode = Suit switch
            {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                _ => throw new InvalidEnumArgumentException(nameof(Suit), (int)Suit, typeof(Suit))
            };

            var rankString = Rank switch
            {
                Rank.Ace => "A",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => ((int)Rank).ToString()
            };

            if (rankString.Length == 0)
            {
                suitUnicode = "#";
            }

            return $"{rankString}{suitUnicode}";
        }
    }

}