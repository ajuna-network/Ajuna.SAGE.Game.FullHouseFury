using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryCreateGameTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier CREATE_GAME = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);

        private IAccount _user;

        [SetUp]
        public void Setup()
        {
            _user = Engine.AccountManager.Account(Engine.AccountManager.Create());
            Assert.That(_user, Is.Not.Null);
            _user.Balance.Deposit(1_000_000);

            // Increment block number before each transition.
            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_CreateGame()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(2));

            bool resultFirst = Engine.Transition(_user, CREATE_GAME, [], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
        }
    }
}
