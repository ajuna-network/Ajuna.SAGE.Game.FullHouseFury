using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Ajuna.SAGE.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryBattleTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Start(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Play(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Preparation(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Battle(AssetType.Game, AssetSubType.None);

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
        public void Test_BattleLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(5));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower, AssetSubType.None);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preDeck.DeckSize, Is.EqualTo(45));

            var preEndurance = preGame.PlayerEndurance;

            byte[] config = [0, 1, 3];

            bool resultFirst = Engine.Transition(_user, BATTLE, inAsset, out IAsset[] outAssets, config);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;
            var towr = outAssets[2] as TowerAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
            Assert.That(towr, Is.Not.Null);

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Battle));

            Assert.That(deck.DeckSize, Is.EqualTo(42));
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

            Assert.That(new Card(game.GetAttackHandCard(0)).ToString(), Is.EqualTo("3♠"));
            Assert.That(new Card(game.GetAttackHandCard(1)).ToString(), Is.EqualTo("Q♥"));
            Assert.That(new Card(game.GetAttackHandCard(2)).ToString(), Is.EqualTo("4♠"));

            Assert.That(game.AttackType, Is.EqualTo(PokerHand.HighCard));
            Assert.That(game.AttackScore, Is.EqualTo(12));

            Assert.That(game.BossHealth, Is.EqualTo(game.MaxBossHealth - game.AttackScore));

            Assert.That(game.PlayerEndurance, Is.EqualTo(preEndurance - 1));
        }
    }
}
