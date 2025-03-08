using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryPreparationTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier CREATE_GAME = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier START_GAME = FullHouseFuryIdentifier.Start(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PREP_LEVEL = FullHouseFuryIdentifier.Preparation(AssetType.Game, AssetSubType.None);

        private IAccount _user;

        [SetUp]
        public void Setup()
        {
            _user = Engine.AccountManager.Account(Engine.AccountManager.Create());
            Assert.That(_user, Is.Not.Null);
            _user.Balance.Deposit(1_000_000);

            bool resultFirst = false;
            IAsset[] outAssets = null;

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, CREATE_GAME, [], out outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preFeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            resultFirst = Engine.Transition(_user, START_GAME, [preGame, preFeck], out outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_PreparationLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(4));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            bool resultFirst = Engine.Transition(_user, PREP_LEVEL, [preGame, preDeck], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Battle));

            Assert.That(deck.DeckSize, Is.EqualTo(45));
            Assert.That(deck.IsHandSlotEmpty(0), Is.False);
            Assert.That(deck.IsHandSlotEmpty(1), Is.False);
            Assert.That(deck.IsHandSlotEmpty(2), Is.False);
            Assert.That(deck.IsHandSlotEmpty(3), Is.False);
            Assert.That(deck.IsHandSlotEmpty(4), Is.False);
            Assert.That(deck.IsHandSlotEmpty(5), Is.False);
            Assert.That(deck.IsHandSlotEmpty(6), Is.False);
            Assert.That(deck.IsHandSlotEmpty(7), Is.True);
            Assert.That(deck.IsHandSlotEmpty(8), Is.True);
            Assert.That(deck.IsHandSlotEmpty(9), Is.True);
        }
    }
}
