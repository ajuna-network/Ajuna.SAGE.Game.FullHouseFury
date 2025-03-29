using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryShopTests : FullHouseFuryBaseTest
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
            // make sure to reset the engine before each test
            Reset();

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

            while ((outAsset[0] as GameAsset).LevelState != LevelState.Score)
            {
                byte[] handArray = new byte[DeckAsset.HAND_LIMIT_SIZE];
                for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
                {
                    handArray[i] = (outAsset[1] as DeckAsset).GetHandCard(i, out _, out _);
                }

                resultFirst = Engine.Transition(_user, BATTLE, inAsset = outAsset, out outAsset,
                    FullHouseFuryUtil.EvaluateAttack(handArray).Positions.Select(pos => (byte)pos).ToArray());
                Assert.That(resultFirst, Is.True, "transition result should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }

            resultFirst = Engine.Transition(_user, SCORE, inAsset, out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_ShopLevel_Rarity()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(13));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(2));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(preDeck.DeckSize, Is.EqualTo(52));

            Assert.That(preGame.Token, Is.EqualTo(3));
            Assert.That(preDeck.GetRarity(RarityType.Rare), Is.EqualTo(0));

            var u1 = new UpgradeSet(FeatureType.RarityLevel, (byte) RarityType.Rare, 1);

            ushort[] config = { u1.Encode() };

            bool resultFirst = Engine.Transition(_user, SHOP, inAsset, out IAsset[] outAssets, config);
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

            Assert.That(game.Token, Is.EqualTo(0));

            Assert.That(deck.GetRarity(RarityType.Rare), Is.EqualTo(1));
        }

        [Test]
        public void Test_ShopLevel_PokerHand()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(13));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(2));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(preDeck.DeckSize, Is.EqualTo(52));

            Assert.That(preGame.Token, Is.EqualTo(3));
            Assert.That(preDeck.GetPokerHandLevel(PokerHand.Pair), Is.EqualTo(0));

            var u1 = new UpgradeSet(FeatureType.PokerHandLevel, (byte)PokerHand.Pair, 1);

            ushort[] config = { u1.Encode() };

            bool resultFirst = Engine.Transition(_user, SHOP, inAsset, out IAsset[] outAssets, config);
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

            Assert.That(game.Token, Is.EqualTo(2));

            Assert.That(deck.GetPokerHandLevel(PokerHand.Pair), Is.EqualTo(1));
        }

        [Test]
        public void Test_ShopLevel_PokerHand_Two()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(13));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(2));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(preDeck.DeckSize, Is.EqualTo(52));

            Assert.That(preGame.Token, Is.EqualTo(3));
            Assert.That(preDeck.GetPokerHandLevel(PokerHand.Pair), Is.EqualTo(0));
            Assert.That(preDeck.GetPokerHandLevel(PokerHand.TwoPair), Is.EqualTo(0));

            var u1 = new UpgradeSet(FeatureType.PokerHandLevel, (byte)PokerHand.Pair, 1);
            var u2 = new UpgradeSet(FeatureType.PokerHandLevel, (byte)PokerHand.TwoPair, 1);

            ushort[] config = { u1.Encode(), u2.Encode() };

            bool resultFirst = Engine.Transition(_user, SHOP, inAsset, out IAsset[] outAssets, config);
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

            Assert.That(game.Token, Is.EqualTo(1));

            Assert.That(deck.GetPokerHandLevel(PokerHand.Pair), Is.EqualTo(1));
            Assert.That(deck.GetPokerHandLevel(PokerHand.TwoPair), Is.EqualTo(1));
        }

        [Test]
        public void Test_ShopLevel_Rarity_Two()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(13));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(2));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(preDeck.DeckSize, Is.EqualTo(52));

            Assert.That(preGame.Token, Is.EqualTo(3));
            preGame.Token = 4;

            Assert.That(preDeck.GetRarity(RarityType.Uncommon), Is.EqualTo(1));

            var u1 = new UpgradeSet(FeatureType.RarityLevel, (byte)RarityType.Uncommon, 2);

            ushort[] config = { u1.Encode() };

            bool resultFirst = Engine.Transition(_user, SHOP, inAsset, out IAsset[] outAssets, config);
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

            Assert.That(game.Token, Is.EqualTo(0));
            Assert.That(deck.GetRarity(RarityType.Uncommon), Is.EqualTo(2));

        }
    }
}
