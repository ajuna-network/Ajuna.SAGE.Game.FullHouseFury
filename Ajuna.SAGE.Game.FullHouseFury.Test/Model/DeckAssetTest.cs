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
            deckAsset.ClearHand();
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
            Assert.That(deckAsset.CountCardsInDeck(), Is.EqualTo(52), "Deck should have 52 cards initially.");

            // Remove a card and verify count decreases.
            deckAsset.RemoveCardFromDeck(5);
            Assert.That(deckAsset.CountCardsInDeck(), Is.EqualTo(51), "Deck should have 51 cards after removal.");
        }

        [Test]
        public void DeckAsset_RemoveCardFromDeck_WorksAsExpected()
        {
            // Remove card at index 20.
            Assert.That(deckAsset.GetCardState(20), Is.True, "Card at index 20 should initially be in the deck.");
            deckAsset.RemoveCardFromDeck(20);
            Assert.That(deckAsset.GetCardState(20), Is.False, "Card at index 20 should be removed after calling RemoveCardFromDeck.");

            // Attempting to remove the same card again should throw an exception.
            Assert.That(() => deckAsset.RemoveCardFromDeck(20), Throws.InvalidOperationException);
        }

        [Test]
        public void DeckAsset_DrawRandomCard_WorksAsExpected()
        {
            int initialCount = deckAsset.CountCardsInDeck();
            byte drawnCard = deckAsset.DrawRandomCard(random);
            // Verify that the deck count decreases by one.
            Assert.That(deckAsset.CountCardsInDeck(), Is.EqualTo(initialCount - 1), "Deck count should decrease by one after drawing a card.");
            // Verify that the drawn card is no longer available.
            Assert.That(deckAsset.GetCardState(drawnCard), Is.False, "Drawn card should be removed from the deck.");
        }

        #endregion

        #region Hand Functions

        [Test]
        public void DeckAsset_Hand_ClearAndEmptySlots_WorksAsExpected()
        {
            // Clear the hand and verify that all 10 slots are marked as empty.
            deckAsset.ClearHand();
            for (int i = 0; i < 10; i++)
            {
                Assert.That(deckAsset.GetCardInHand(i), Is.EqualTo(DeckAsset.EMPTY_SLOT), $"Hand slot {i} should be empty after clearing hand.");
                Assert.That(deckAsset.IsHandSlotEmpty(i), Is.True, $"Hand slot {i} should be empty.");
                Assert.That(deckAsset.IsHandSlotOccupied(i), Is.False, $"Hand slot {i} should not be occupied.");
            }
        }

        [Test]
        public void DeckAsset_Hand_SetGetCardInHand_WorksAsExpected()
        {
            // Clear the hand before setting cards.
            deckAsset.ClearHand();

            // Set specific cards in various hand slots.
            deckAsset.SetCardInHand(0, 10); // For example, card index 10.
            deckAsset.SetCardInHand(5, 25); // For example, card index 25.
            deckAsset.SetCardInHand(9, 51); // For example, last valid card index.

            // Retrieve them and verify.
            Assert.That(deckAsset.GetCardInHand(0), Is.EqualTo(10), "Hand slot 0 should have card index 10.");
            Assert.That(deckAsset.GetCardInHand(5), Is.EqualTo(25), "Hand slot 5 should have card index 25.");
            Assert.That(deckAsset.GetCardInHand(9), Is.EqualTo(51), "Hand slot 9 should have card index 51.");

            // Check occupancy status.
            Assert.That(deckAsset.IsHandSlotOccupied(0), Is.True, "Hand slot 0 should be occupied.");
            Assert.That(deckAsset.IsHandSlotEmpty(1), Is.True, "Hand slot 1 should be empty.");
        }

        [Test]
        public void DeckAsset_Hand_InvalidSlot_ThrowsException()
        {
            // Accessing an invalid hand slot should throw an exception.
            Assert.That(() => deckAsset.GetCardInHand(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.GetCardInHand(10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.SetCardInHand(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => deckAsset.SetCardInHand(10, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DeckAsset_Hand_InvalidCardIndex_ThrowsException()
        {
            // Setting a card with an invalid card index should throw an exception.
            Assert.That(() => deckAsset.SetCardInHand(0, 52), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        #endregion
    }
}
