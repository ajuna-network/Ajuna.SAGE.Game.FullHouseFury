using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryBoonAndBaneTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Start();
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Play();
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Preparation();
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Battle();
        private readonly FullHouseFuryIdentifier SCORE = FullHouseFuryIdentifier.Score();

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

            while ((outAsset[0] as GameAsset).LevelState != LevelState.Score)
            {
                byte[] handArray = new byte[10];
                for (int i = 0; i < 10; i++)
                {
                    handArray[i] = (outAsset[1] as DeckAsset).GetHandCard(i);
                }

                bool battleResult = Engine.Transition(_user, BATTLE, inAsset = outAsset, out outAsset,
                    FullHouseFuryUtil.EvaluateAttack(handArray).Positions.Select(pos => (byte)pos).ToArray());
                Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }

            resultFirst = Engine.Transition(_user, SCORE, inAsset = outAsset, out outAsset);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");
        }

        [Test]
        public void Test_Preparation_Level2()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(10));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower, AssetSubType.None);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(2));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Preparation));
            Assert.That(preGame.Round, Is.EqualTo(5));
            Assert.That(preDeck.DeckSize, Is.EqualTo(52));

            Assert.That(preTowr.GetBoonAndBane(0).boon, Is.EqualTo(BonusType.DeckRefill));
            Assert.That(preTowr.GetBoonAndBane(0).bane, Is.EqualTo(MalusType.VulnerableState));
            Assert.That(preTowr.GetBoonAndBane(1).boon, Is.EqualTo(BonusType.ZealousCharge));
            Assert.That(preTowr.GetBoonAndBane(1).bane, Is.EqualTo(MalusType.SpadeHealsOpponent));
            Assert.That(preTowr.GetBoonAndBane(2).boon, Is.EqualTo(BonusType.InspiringPresence));
            Assert.That(preTowr.GetBoonAndBane(2).bane, Is.EqualTo(MalusType.ReducedEndurance));


            // choice is 2
            byte? choice = 2;

            var choosenBoon = preTowr.GetBoonAndBane(2).boon;
            var choosenBane = preTowr.GetBoonAndBane(2).bane;

            bool resultFirst = Engine.Transition(_user, PREPARATION, inAsset, out IAsset[] outAssets, choice);
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

            Assert.That(preTowr.GetBoonAndBane(0).boon, Is.EqualTo(BonusType.None));
            Assert.That(preTowr.GetBoonAndBane(0).bane, Is.EqualTo(MalusType.None));
            Assert.That(preTowr.GetBoonAndBane(1).boon, Is.EqualTo(BonusType.None));
            Assert.That(preTowr.GetBoonAndBane(1).bane, Is.EqualTo(MalusType.None));
            Assert.That(preTowr.GetBoonAndBane(2).boon, Is.EqualTo(BonusType.None));
            Assert.That(preTowr.GetBoonAndBane(2).bane, Is.EqualTo(MalusType.None));

            // verify that the boon and bane are a assigned correctly

            var allBoons = towr.GetAllBoons();
            for (int i = 0; i < allBoons.Length; i++)
            {
                var val = allBoons[i];

                if (i == (int)choosenBoon)
                {
                    Assert.That(val, Is.EqualTo(1));
                }
                else
                {
                    Assert.That(val, Is.EqualTo(0));
                }
            }

            var allBanes = towr.GetAllBanes();
            for (int i = 0; i < allBanes.Length; i++)
            {
                var val = allBanes[i];
                if (i == (int)choosenBane)
                {
                    Assert.That(val, Is.EqualTo(1));
                }
                else
                {
                    Assert.That(val, Is.EqualTo(0));
                }
            }
        }
    }
}
