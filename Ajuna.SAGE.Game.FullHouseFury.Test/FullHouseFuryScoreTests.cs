﻿using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryScoreTests : FullHouseFuryBaseTest
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

            while ((outAsset[0] as GameAsset).LevelState != LevelState.Score)
            {
                byte[] handArray = new byte[DeckAsset.HAND_LIMIT_SIZE];
                for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
                {
                    handArray[i] = (outAsset[1] as DeckAsset).GetHandCard(i, out _, out _);
                }

                bool battleResult = Engine.Transition(_user, BATTLE, inAsset = outAsset, out outAsset,
                    FullHouseFuryUtil.EvaluateAttack(handArray).Positions.Select(pos => (byte)pos).ToArray());
                Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }
        }

        [Test]
        public void Test_ScoreLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(12));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            Assert.That(preGame.Level, Is.EqualTo(1));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Score));
            Assert.That(preGame.Round, Is.EqualTo(7));
            Assert.That(preDeck.DeckSize, Is.EqualTo(37));

            bool resultFirst = Engine.Transition(_user, SCORE, inAsset, out IAsset[] outAssets);
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

            // deck is reset
            Assert.That(deck.DeckSize, Is.EqualTo(deck.MaxDeckSize));
            
            // hand is reset
            Assert.That(deck.HandCardsCount(), Is.EqualTo(0));
            Assert.That(deck.IsHandSlotEmpty(0), Is.True);
            Assert.That(deck.IsHandSlotEmpty(1), Is.True);
            Assert.That(deck.IsHandSlotEmpty(2), Is.True);
            Assert.That(deck.IsHandSlotEmpty(3), Is.True);
            Assert.That(deck.IsHandSlotEmpty(4), Is.True);
            Assert.That(deck.IsHandSlotEmpty(5), Is.True);
            Assert.That(deck.IsHandSlotEmpty(6), Is.True);
            Assert.That(deck.IsHandSlotEmpty(7), Is.True);

            // atack is reset
            Assert.That(game.AttackType, Is.EqualTo(PokerHand.HighCard));
            Assert.That(game.AttackScore, Is.EqualTo(0));

            // player is partially reset
            Assert.That(game.PlayerEndurance, Is.EqualTo(game.MaxPlayerEndurance));

            // new boss is set
            Assert.That(game.BossHealth, Is.GreaterThan(200));
            Assert.That(game.BossDamage, Is.EqualTo(0));

            Assert.That(game.Level, Is.EqualTo(2));

            // verify that the boon and bane are set
            Assert.That(towr.GetBoonAndBane(0).boon, Is.EqualTo(BonusType.FortunesFavor));
            Assert.That(towr.GetBoonAndBane(0).bane, Is.EqualTo(MalusType.HeavyBurden));
            Assert.That(towr.GetBoonAndBane(1).boon, Is.EqualTo(BonusType.RapidRecovery));
            Assert.That(towr.GetBoonAndBane(1).bane, Is.EqualTo(MalusType.UniformSuitPenalty));
            Assert.That(towr.GetBoonAndBane(2).boon, Is.EqualTo(BonusType.DivineIntervention));
            Assert.That(towr.GetBoonAndBane(2).bane, Is.EqualTo(MalusType.ReducedEndurance));
        }

    }
}
