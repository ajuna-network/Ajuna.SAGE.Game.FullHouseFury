using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryFull1Tests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Start();
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Play();
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Preparation();
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Battle();

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
        public void Test_FullGameLoop_TillWin_ScoreLevel()
        {
            // Retrieve current game and deck assets.
            GameAsset game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            DeckAsset deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);
            TowerAsset towr = GetAsset<TowerAsset>(_user, AssetType.Tower, AssetSubType.None);

            IAsset[] inAsset = [game, deck, towr];
            IAsset[] outAsset = null;

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
                towr = outAsset[2] as TowerAsset;

                if (game.Round == 1)
                {
                    var hand = new Card?[DeckAsset.HAND_LIMIT_SIZE];
                    for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
                    {
                        hand[i] = deck.TryGetHandCard(i, out byte cardIndex, out byte rarity) && cardIndex != DeckAsset.EMPTY_SLOT ? new Card(cardIndex, rarity) : null;
                    }
                    //Assert.That(string.Join(" ", hand.Select(c => c.ToString())).Trim(), Is.EqualTo("3♠ Q♥ A♣ 4♠ 6♠ J♠ 7♣"));
                }

                // Convert the hand from deck into a byte[10]
                byte[] handArray = new byte[DeckAsset.HAND_LIMIT_SIZE];
                for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
                {
                    handArray[i] = deck.GetHandCard(i, out _, out _);
                }

                // Evaluate the best attack from the hand.
                var bestAttack = FullHouseFuryUtil.EvaluateAttack(handArray);

                // Create an AttackHand instance from the chosen positions.
                byte[] attackHand = bestAttack.Positions.Select(pos => (byte)pos).ToArray();

                bool battleResult = Engine.Transition(_user, BATTLE, inAsset = outAsset, out outAsset, attackHand);
                Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }

            Assert.That(game.Round, Is.EqualTo(7), "Round is not correct.");
            Assert.That(game.IsBossAlive, Is.EqualTo(false), "Boss should be alive.");
            Assert.That(game.IsPlayerAlive, Is.EqualTo(true), "Player should be dead.");

            Assert.That(game.GameState, Is.EqualTo(GameState.Running), "Game should eventually transition to Finished state.");
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Score), "Game should eventually transition to Score state.");
        }
    }
}
