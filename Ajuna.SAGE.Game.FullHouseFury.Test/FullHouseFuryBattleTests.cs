using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryBattleTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier CREATE_GAME = FullHouseFuryIdentifier.Create(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier START_GAME = FullHouseFuryIdentifier.Start(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PREP_LEVEL = FullHouseFuryIdentifier.Preparation(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier BATTLE_LEVEL = FullHouseFuryIdentifier.Battle(AssetType.Game, AssetSubType.None);

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

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, CREATE_GAME, [], out _);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            resultFirst = Engine.Transition(_user, START_GAME, [game, deck], out _);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            resultFirst = Engine.Transition(_user, PREP_LEVEL, [game, deck], out IAsset[] _);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_BattleLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(5));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            byte[] config = [0, 1, 3];

            bool resultFirst = Engine.Transition(_user, BATTLE_LEVEL, [preGame, preDeck], out IAsset[] outAssets, config);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Preparation));

            Assert.That(deck.DeckSize, Is.EqualTo(45));
            Assert.That(deck.IsHandSlotEmpty(0), Is.True);
            Assert.That(deck.IsHandSlotEmpty(1), Is.True);
            Assert.That(deck.IsHandSlotEmpty(2), Is.False);
            Assert.That(deck.IsHandSlotEmpty(3), Is.True);
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

            Assert.That(game.Health, Is.EqualTo(game.MaxHealth - game.AttackScore));
        }
    }
}
