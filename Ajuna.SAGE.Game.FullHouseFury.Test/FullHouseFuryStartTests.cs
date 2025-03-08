using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryStartTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier CREATE_GAME = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier START_GAME = FullHouseFuryIdentifier.Start(AssetType.Game, AssetSubType.None);

        private IAccount _user;

        [SetUp]
        public void Setup()
        {
            _user = Engine.AccountManager.Account(Engine.AccountManager.Create());
            Assert.That(_user, Is.Not.Null);
            _user.Balance.Deposit(1_000_000);

            BlockchainInfoProvider.CurrentBlockNumber++;

            bool resultFirst = Engine.Transition(_user, CREATE_GAME, [], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_StartGame()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(3));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preFeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            bool resultFirst = Engine.Transition(_user, START_GAME, [preGame, preFeck], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(game.Level, Is.EqualTo(1));
            Assert.That(game.Round, Is.EqualTo(1));
        }
    }
}
