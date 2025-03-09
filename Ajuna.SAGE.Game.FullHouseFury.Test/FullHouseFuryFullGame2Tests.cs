using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryFull2Tests : FullHouseFuryBaseTest
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

            BlockchainInfoProvider.CurrentBlockNumber++;

            resultFirst = Engine.Transition(_user, START, [], out _);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            resultFirst = Engine.Transition(_user, PLAY, [game, deck], out _);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;
        }

        [Test]
        public void Test_FullGameLoop_TillFatigue_ScoreLevel()
        {
            // Retrieve current game and deck assets.
            GameAsset game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            DeckAsset deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            // set boos to max health
            game.MaxBossHealth = ushort.MaxValue;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

            // Loop until game.LevelState becomes Score or deck is exhausted.
            while (game.LevelState != LevelState.Score && deck.DeckSize > 0)
            {
                IAsset[] outAsset = null;

                switch (game.LevelState)
                {
                    case LevelState.Preparation:
                        bool prepResult = Engine.Transition(_user, PREPARATION, [game, deck], out outAsset);
                        Assert.That(prepResult, Is.True, "PREP_LEVEL transition should succeed.");
                        break;

                    case LevelState.Battle:

                        // take the first card from the hand to play
                        byte[] attackHand = [0];

                        bool battleResult = Engine.Transition(_user, BATTLE, [game, deck], out outAsset, attackHand);
                        Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");
                        break;

                    default:
                        Assert.Fail("Unexpected LevelState.");
                        break;
                }

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);

                game = outAsset[0] as GameAsset;
                deck = outAsset[1] as DeckAsset;
            }

            Assert.That(game.Round, Is.EqualTo(17), "Round is not correct.");
            Assert.That(game.IsBossAlive, Is.EqualTo(true), "Boss should be alive.");
            Assert.That(game.IsPlayerAlive, Is.EqualTo(false), "Player should be dead.");

            Assert.That(game.GameState, Is.EqualTo(GameState.Finished), "Game should eventually transition to Finished state.");
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Score), "Game should eventually transition to Score state.");
        }
    }
}
