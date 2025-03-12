using System;
using NUnit.Framework;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Game.FullHouseFury.Test.Model
{
    [TestFixture]
    public class DeckAssetTest
    {
        private DeckAsset deckAsset;
        private Random random;

        [SetUp]
        public void Setup()
        {
            // Create a new DeckAsset for each test.
            deckAsset = new DeckAsset(1, 1);
            // Clear the hand region to ensure consistency.
            deckAsset.EmptyHand();
            // Use a fixed seed for deterministic tests.
            random = new Random(42);
        }

        #region Generic Deck Functions

        [Test]
        public void DeckAsset_GetSetCardState_WorksAsExpected()
        {
            // Verify that card at index 10 is initially in the deck.
            Assert.That(deckAsset.GetCardState(10), Is.True, "Card at index 10 should initially be in the deck.");

            // Remove card at index 10 and verify.
            deckAsset.SetCardState(10, false);
            Assert.That(deckAsset.GetCardState(10), Is.False, "Card at index 10 should be removed from the deck.");

            // Set card at index 10 back to true and verify.
            deckAsset.SetCardState(10, true);
            Assert.That(deckAsset.GetCardState(10), Is.True, "Card at index 10 should be in the deck after setting it back.");

            // Boundary check: card at index 51 (last valid card) is accessible.
            Assert.That(deckAsset.GetCardState(51), Is.True, "Card at index 51 should initially be in the deck.");

            // Out-of-range index should throw exception.
            Assert.That(() => deckAsset.GetCardState(52), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DeckAsset_CountCardsInDeck_WorksAsExpected()
        {
            // Initially, the deck should contain all 52 cards.
            Assert.That(deckAsset.DeckSize, Is.EqualTo(52), "Deck should have 52 cards initially.");

            // Remove a card and verify count decreases.
            deckAsset.RemoveCard(5);
            Assert.That(deckAsset.DeckSize, Is.EqualTo(51), "Deck should have 51 cards after removal.");
        }

        [Test]
        public void DeckAsset_RemoveCardFromDeck_WorksAsExpected()
        {
            // Remove card at index 20.
            Assert.That(deckAsset.GetCardState(20), Is.True, "Card at index 20 should initially be in the deck.");
            deckAsset.RemoveCard(20);
            Assert.That(deckAsset.GetCardState(20), Is.False, "Card at index 20 should be removed after calling RemoveCardFromDeck.");

            // Attempting to remove the same card again should throw an exception.
            Assert.That(() => deckAsset.RemoveCard(20), Throws.InvalidOperationException);
        }

        [Test]
        public void DeckAsset_DrawRandomCard_WorksAsExpected()
        {
            int initialCount = deckAsset.DeckSize;
            byte drawnCard = deckAsset.DrawCard(1);
            // Verify that the deck count decreases by one.
            Assert.That(deckAsset.DeckSize, Is.EqualTo(initialCount - 1), "Deck count should decrease by one after drawing a card.");
            // Verify that the drawn card is no longer available.
            Assert.That(deckAsset.GetCardState(drawnCard), Is.False, "Drawn card should be removed from the deck.");
        }

        #endregion

        #region Hand Functions

        [Test]
        public void DeckAsset_Hand_ClearAndEmptySlots_WorksAsExpected()
        {
            // Clear the hand and verify that all 10 slots are marked as empty.
            deckAsset.EmptyHand();
            for (int i = 0; i < 10; i++)
            {
                Assert.That(deckAsset.GetHandCard(i), Is.EqualTo(DeckAsset.EMPTY_SLOT), $"Hand slot {i} should be empty after clearing hand.");
                Assert.That(deckAsset.IsHandSlotEmpty(i), Is.True, $"Hand slot {i} should be empty.");
                Assert.That(deckAsset.IsHandSlotOccupied(i), Is.False, $"Hand slot {i} should not be occupied.");
            }
        }

        [Test]
        public void DeckAsset_Hand_SetGetCardInHand_WorksAsExpected()
        {
            // Clear the hand before setting cards.
            deckAsset.EmptyHand();

            // Set specific cards in various hand slots.
            deckAsset.SetHandCard(0, 10); // For example, card index 10.
            deckAsset.SetHandCard(5, 25); // For example, card index 25.
            deckAsset.SetHandCard(9, 51); // For example, last valid card index.

            // Retrieve them and verify.
            Assert.That(deckAsset.GetHandCard(0), Is.EqualTo(10), "Hand slot 0 should have card index 10.");
            Assert.That(deckAsset.GetHandCard(5), Is.EqualTo(25), "Hand slot 5 should have card index 25.");
            Assert.That(deckAsset.GetHandCard(9), Is.EqualTo(51), "Hand slot 9 should have card index 51.");

            // Check occupancy status.
            Assert.That(deckAsset.IsHandSlotOccupied(0), Is.True, "Hand slot 0 should be occupied.");
            Assert.That(deckAsset.IsHandSlotEmpty(1), Is.True, "Hand slot 1 should be empty.");
        }

        [Test]
        public void DeckAsset_Hand_InvalidSlot_ThrowsException()
        {
            // Accessing an invalid hand slot should throw an exception.
            Assert.That(() => deckAsset.GetHandCard(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.GetHandCard(10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.SetHandCard(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.SetHandCard(10, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DeckAsset_Hand_InvalidCardIndex_ThrowsException()
        {
            // Setting a card with an invalid card index should throw an exception.
            Assert.That(() => deckAsset.SetHandCard(0, 52), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        #endregion

        #region New Functions Tests

        [Test]
        public void DeckAsset_DrawAllCards_ExhaustsDeck()
        {
            // Ensure deck is new.
            deckAsset.New();
            Assert.That(deckAsset.DeckSize, Is.EqualTo(52));

            // Draw all 52 cards.
            for (int i = 0; i < 52; i++)
            {
                // For testing, we use a constant random byte (0) to always pick the first available card.
                byte drawnCard = deckAsset.DrawCard(0);
                Assert.That(drawnCard, Is.InRange(0, 51), $"Drawn card at iteration {i} should be within valid range.");
            }

            // After drawing 52 cards, the deck should be empty.
            Assert.That(deckAsset.DeckSize, Is.EqualTo(0), "Deck size should be 0 after drawing all cards.");

            // Attempting to draw another card should throw an exception.
            Assert.Throws<InvalidOperationException>(() => deckAsset.DrawCard(0));
        }

        [Test]
        public void DeckAsset_Draw_Hand_FillsHandProperly()
        {
            // Reset deck and hand.
            deckAsset.New();
            deckAsset.EmptyHand();
            int initialDeckSize = deckAsset.DeckSize; // Should be 52
            int targetHandSize = 10;

            // Create a deterministic random hash (32 bytes) for testing.
            byte[] randomHash = new byte[32];
            for (int i = 0; i < randomHash.Length; i++)
            {
                randomHash[i] = (byte)(i + 1); // Values 1 to 32.
            }

            // Draw cards into the hand.
            deckAsset.Draw((byte)targetHandSize, randomHash);

            // Verify that exactly targetHandSize hand slots are occupied.
            int countInHand = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!deckAsset.IsHandSlotEmpty(i))
                    countInHand++;
            }
            Assert.That(countInHand, Is.EqualTo(targetHandSize), "Hand should be filled with the target number of cards.");

            // Verify that the deck size decreased by the number of drawn cards.
            Assert.That(deckAsset.DeckSize, Is.EqualTo(initialDeckSize - targetHandSize), "Deck size should decrease by the number of drawn cards.");
        }

        [Test]
        public void DeckAsset_AddCard_IncreasesDeckSize()
        {
            deckAsset.New();
            // Remove a specific card.
            byte removed = deckAsset.RemoveCard(10);
            Assert.That(deckAsset.DeckSize, Is.EqualTo(51), "Deck size should be 51 after removal.");

            // Add the same card back.
            byte added = deckAsset.AddCard(removed);
            Assert.That(added, Is.EqualTo(10), "Added card index should be equal to the removed card index.");
            Assert.That(deckAsset.DeckSize, Is.EqualTo(52), "Deck size should be restored to 52 after adding the card back.");
        }

        #endregion

    }
}
