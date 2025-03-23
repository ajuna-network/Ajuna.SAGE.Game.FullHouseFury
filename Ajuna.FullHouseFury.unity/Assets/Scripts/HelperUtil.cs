using Ajuna.SAGE.Core;
using Ajuna.SAGE.Game.FullHouseFury;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public static partial class HelperUtil
    {
        /// <summary>
        /// Get the sprite name for a card based on its suit and rank.
        /// </summary>
        /// <param name="suit"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static string GetCardSpritName(Suit suit, Rank rank)
        {
            var result = "Deck05_";
            var value = (int)rank - 1;
            switch (suit)
            {
                case Suit.Clubs:
                    value += 3 * 14;
                    break;
                case Suit.Diamonds:
                    value += 2 * 14;
                    break;
                case Suit.Hearts:
                    value += 0 * 14;
                    break;
                case Suit.Spades:
                    value += 1 * 14;
                    break;
            }
            return result + value;
        }


        /// <summary>
        /// Arranges the child elements of <paramref name="velHandCards"/> in a fan,
        /// by rotating and translating each card around a pivot.
        /// </summary>
        /// <param name="velHandCards">Parent VisualElement containing card elements as children.</param>
        /// <param name="totalFanAngle">Total angle spread (in degrees) for the fan.</param>
        /// <param name="radius">Distance from the pivot to each card (fan radius).</param>
        public static void ArrangeHand(VisualElement velHandCards, float totalFanAngle = 30f, float radius = 100f)
        {
            // Get children as a list (each child is a "card")
            var cards = velHandCards.Children().ToList();
            int cardCount = cards.Count;
            if (cardCount == 0) return;

            // Where do we want the pivot? For example, near the bottom-center of the parent.
            // We'll use the parent's layout size to figure out a pivot at the bottom-center.
            float pivotX = velHandCards.contentRect.width * 0.5f;
            float pivotY = velHandCards.contentRect.height;

            // We'll spread from -halfFan to +halfFan around a "center" angle of 0.
            float halfFan = totalFanAngle * 0.5f;

            // Pre-calculate step angle between each card.
            // If there's only 1 card, we can skip the step or just set angle = 0.
            float angleStep = (cardCount > 1) ? totalFanAngle / (cardCount - 1) : 0f;

            for (int i = 0; i < cardCount; i++)
            {
                // Compute the angle for this card, in degrees
                float angleDeg = -halfFan + i * angleStep;  // from -halfFan to +halfFan
                float angleRad = angleDeg * Mathf.Deg2Rad;

                // Position of each card, if we revolve them around pivot
                float cardX = pivotX + radius * Mathf.Sin(angleRad);
                float cardY = pivotY - radius * Mathf.Cos(angleRad);

                // Access the card element
                VisualElement card = cards[i];

                // Set the pivot on the card itself to be bottom-center (so the "bottom middle"
                // of the card is the point that rotates around). In UXML, that's:
                // transform-origin: 50% 100%;
                card.style.transformOrigin =
                    new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(100, LengthUnit.Percent));

                // Now position the card so that the pivot sits at (cardX, cardY).
                // Because the transform-origin is bottom-center, we can place the card
                // by setting its left/top to that pivot location.
                card.style.left = cardX;
                card.style.top = cardY;

                // Finally, rotate the card around that pivot by angleDeg
                card.style.rotate = new Rotate(new Angle(angleDeg, AngleUnit.Degree));
            }
        }

        /// <summary>
        /// Arranges the children of <paramref name="hand"/> so they overlap horizontally.
        /// OverlapOffset < card width => cards will overlap.
        /// </summary>
        public static void OverlapHand(VisualElement hand, float overlapOffset = 100f)
        {
            // Make sure the parent can absolutely position its children
            hand.style.position = Position.Relative;

            var cards = hand.Children().ToList();
            for (int i = 0; i < cards.Count; i++)
            {
                VisualElement card = cards[i];

                // Ensure each card is absolute-positioned
                card.style.position = Position.Absolute;

                // Shift each card to the right by overlapOffset * i
                float xPos = overlapOffset * i;
                card.style.left = xPos;
                card.style.top = 0;

                // Optional: if you want the last card on top, 
                // ensure the last child in the hierarchy is drawn last.
                // Or set card.style.zIndex = i; 
            }
        }

        public static Color GetRarityColor(RarityType rarity)
        {
            switch (rarity)
            {
                case RarityType.Common:
                    return new Color(250f / 255, 250f / 255, 250f / 255, 255 / 255);

                case RarityType.Uncommon:
                    return new Color(50f / 255, 250f / 255, 50f / 255, 255f / 255);

                case RarityType.Rare:
                    return new Color(50f / 255, 50f / 255, 250f / 255, 255f / 255);

                case RarityType.Epic:
                    return new Color(250f / 255, 50f / 255, 250f / 255, 255f / 255);

                case RarityType.Legendary:
                    return new Color(250f / 255, 150f / 255, 0f / 255, 255f / 255);

                case RarityType.Mythical:
                    return new Color(250f / 255, 50f / 255, 50f / 255, 255f / 255);

                default:
                    return new Color(50f / 255, 50f / 255, 50f / 255, 255f / 255);
            }
        }
    }
}