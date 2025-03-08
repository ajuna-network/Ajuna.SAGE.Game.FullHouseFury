using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryFullTests : FullHouseFuryBaseTest
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
        }

        [Test]
        public void Test_FullGameLoop_TillScoreLevel()
        {
            // Retrieve current game and deck assets.
            GameAsset game = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            DeckAsset deck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

            // Loop until game.LevelState becomes Score or deck is exhausted.
            while (game.LevelState != LevelState.Score && deck.DeckSize > 0)
            {
                IAsset[] outAsset = null;

                switch (game.LevelState)
                {
                    case LevelState.Preparation:
                        bool prepResult = Engine.Transition(_user, PREP_LEVEL, new IAsset[] { game, deck }, out outAsset);
                        Assert.That(prepResult, Is.True, "PREP_LEVEL transition should succeed.");
                        break;

                    case LevelState.Battle:

                        if (game.Round == 1)
                        {
                            var hand = new Card?[10];
                            for (int i = 0; i < 10; i++)
                            {
                                hand[i] = deck.TryGetHandCard(i, out byte cardIndex) ? new Card(cardIndex) : null;
                            }
                            Assert.That(string.Join(" ", hand.Select(c => c.ToString())).Trim(), Is.EqualTo("3♠ Q♥ A♣ 4♠ 6♠ J♠ 7♣"));
                        }

                        // Convert the hand from deck into a byte[10]
                        byte[] handArray = new byte[10];
                        for (int i = 0; i < 10; i++)
                        {
                            handArray[i] = deck.GetHandCard(i);
                        }

                        // Evaluate the best attack from the hand.
                        var bestAttack = FullHouseFuryUtil.EvaluateAttack(handArray);

                        // Create an AttackHand instance from the chosen positions.
                        AttackHand attackHand = new AttackHand(bestAttack.Positions.Select(pos => (byte)pos).ToArray());

                        bool battleResult = Engine.Transition(_user, BATTLE_LEVEL, [game, deck], out outAsset, attackHand);
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

            Assert.That(game.LevelState, Is.EqualTo(LevelState.Score), "Game should eventually transition to Score state.");

            Assert.That(game.Round, Is.EqualTo(1), "Round is not correct.");
        }
    }
}
