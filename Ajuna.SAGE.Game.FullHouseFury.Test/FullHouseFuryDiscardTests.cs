using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryDiscardTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Start);
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Play);
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Preparation);
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Battle);
        private readonly FullHouseFuryIdentifier DISCARD = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Discard);
        private readonly FullHouseFuryIdentifier SCORE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Score);
        private readonly FullHouseFuryIdentifier SHOP = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Shop);

        private IAccount _user;

        [SetUp]
        public void Setup()
        {
            _user = Engine.AccountManager.Account(Engine.AccountManager.Create());
            Assert.That(_user, Is.Not.Null);
            _user.Balance.Deposit(1_000_000);

            bool resultFirst = false;
            GameAsset game = null;
            DeckAsset deck = null;
            TowerAsset towr = null;
            IAsset[] inAsset = [];
            IAsset[] outAsset = [];

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, START, [], out outAsset);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, PLAY, inAsset = outAsset, out outAsset);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, PREPARATION, inAsset = outAsset, out outAsset);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_DiscardLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(5));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            var preHand = new Card?[DeckAsset.HAND_LIMIT_SIZE];
            for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
            {
                preHand[i] = preDeck.TryGetHandCard(i, out byte cardIndex, out byte rarity) && cardIndex != DeckAsset.EMPTY_SLOT ? new Card(cardIndex, 0) : null;
            }
            var preHandString = "3♠ 9♠ J♥ 2♠ 7♣ K♥ A♥";
            Assert.That(string.Join(" ", preHand.Select(c => c.ToString())).Trim(), Is.EqualTo(preHandString));

            byte[] config = [0, 1, 3];

            bool resultFirst = Engine.Transition(_user, DISCARD, inAsset, out IAsset[] outAssets, config);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;
            var towr = outAssets[2] as TowerAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
            Assert.That(towr, Is.Not.Null);

            var hand = new Card?[DeckAsset.HAND_LIMIT_SIZE];
            for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
            {
                hand[i] = deck.TryGetHandCard(i, out byte cardIndex, out byte rarity) && cardIndex != DeckAsset.EMPTY_SLOT ? new Card(cardIndex, rarity) : null;
            }

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Battle));

            Assert.That(deck.DeckSize, Is.EqualTo(42));

            Assert.That(game.AttackType, Is.EqualTo(PokerHand.None));
            Assert.That(game.AttackScore, Is.EqualTo(0));

            var handString = string.Join(" ", hand.Select(c => c.ToString())).Trim();
            Assert.That(handString, Is.Not.EqualTo(preHandString));
            Assert.That(handString, Is.EqualTo("2♦ 2♣ J♥ Q♠ 7♣ K♥ A♥"));

            var rarityString = string.Join(" ", hand.Where(c => c != null).Select(c => ((int)c.Value.Rarity).ToString())).Trim();
            Assert.That(rarityString, Is.EqualTo("1 1 1 2 1 1 1"));
        }
    }
}
