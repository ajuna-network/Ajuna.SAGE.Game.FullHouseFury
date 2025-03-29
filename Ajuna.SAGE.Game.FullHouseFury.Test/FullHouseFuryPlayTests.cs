using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryPlayTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Start);
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Play);
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Preparation);
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Battle);
        private readonly FullHouseFuryIdentifier SCORE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Score);
        private readonly FullHouseFuryIdentifier SHOP = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Shop);

        private IAccount _user;

        [SetUp]
        public void Setup()
        {
            _user = Engine.AccountManager.Account(Engine.AccountManager.Create());
            Assert.That(_user, Is.Not.Null);
            _user.Balance.Deposit(1_000_000);

            BlockchainInfoProvider.CurrentBlockNumber++;

            bool resultFirst = Engine.Transition(_user, START, [], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_PlayGame()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(3));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            var inAssets = new IAsset[] { preGame, preDeck, preTowr };


            bool resultFirst = Engine.Transition(_user, PLAY, inAssets, out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;
            var towr = outAssets[2] as TowerAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
            Assert.That(towr, Is.Not.Null);

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(game.Level, Is.EqualTo(1));
            Assert.That(game.Round, Is.EqualTo(0));
        }
    }
}
