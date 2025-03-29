using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{
    [TestFixture]
    public class FullHouseFuryFull2Tests : FullHouseFuryBaseTest
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
        }

        [Test]
        public void Test_FullGameLoop_TillFatigue_ScoreLevel()
        {
            // Retrieve current game and deck assets.
            GameAsset game = GetAsset<GameAsset>(_user, AssetType.Game);
            DeckAsset deck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            TowerAsset towr = GetAsset<TowerAsset>(_user, AssetType.Tower);

            IAsset[] inAsset = [game, deck, towr];
            IAsset[] outAsset = [];

            // set boos to max health
            game.MaxBossHealth = ushort.MaxValue;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
            Assert.That(towr, Is.Not.Null);

            bool prepResult = Engine.Transition(_user, PREPARATION, inAsset, out outAsset);
            Assert.That(prepResult, Is.True, "PREP_LEVEL transition should succeed.");

            // Loop until game.LevelState becomes Score
            while (game.LevelState != LevelState.Score)
            {
                game = outAsset[0] as GameAsset;
                deck = outAsset[1] as DeckAsset;
                towr = outAsset[1] as TowerAsset;

                // take the first card from the hand to play
                byte[] attackHand = [0];

                bool battleResult = Engine.Transition(_user, BATTLE, inAsset = outAsset, out outAsset, attackHand);
                Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }

            Assert.That(game.Round, Is.EqualTo(17), "Round is not correct.");
            Assert.That(game.IsBossAlive, Is.EqualTo(true), "Boss should be alive.");
            Assert.That(game.IsPlayerAlive, Is.EqualTo(false), "Player should be dead.");

            Assert.That(game.GameState, Is.EqualTo(GameState.Finished), "Game should eventually transition to Finished state.");
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Score), "Game should eventually transition to Score state.");
        }
    }
}