﻿using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryScoreTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Start(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Play(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Preparation(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Battle(AssetType.Game, AssetSubType.None);
        private readonly FullHouseFuryIdentifier SCORE = FullHouseFuryIdentifier.Score(AssetType.Game, AssetSubType.None);

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

            resultFirst = Engine.Transition(_user, PREPARATION, [game, deck], out IAsset[] outAsset);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            BlockchainInfoProvider.CurrentBlockNumber++;

            while (game.LevelState != LevelState.Score)
            {
                game = outAsset[0] as GameAsset;
                deck = outAsset[1] as DeckAsset;

                byte[] handArray = new byte[10];
                for (int i = 0; i < 10; i++)
                {
                    handArray[i] = deck.GetHandCard(i);
                }

                bool battleResult = Engine.Transition(_user, BATTLE, [game, deck], out outAsset,
                    FullHouseFuryUtil.EvaluateAttack(handArray).Positions.Select(pos => (byte)pos).ToArray());
                Assert.That(battleResult, Is.True, "BATTLE_LEVEL transition should succeed.");

                BlockchainInfoProvider.CurrentBlockNumber++;

                Assert.That(outAsset, Is.Not.Null);
            }

        }

        [Test]
        public void Test_ScoreLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(10));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);

            Assert.That(preGame.Level, Is.EqualTo(1));
            Assert.That(preGame.LevelState, Is.EqualTo(LevelState.Score));
            Assert.That(preGame.Round, Is.EqualTo(5));
            Assert.That(preDeck.DeckSize, Is.EqualTo(39));

            bool resultFirst = Engine.Transition(_user, SCORE, [preGame, preDeck], out IAsset[] outAssets);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);

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
            Assert.That(deck.IsHandSlotEmpty(8), Is.True);
            Assert.That(deck.IsHandSlotEmpty(9), Is.True);

            // atack is reset
            Assert.That(game.AttackType, Is.EqualTo(PokerHand.None));
            Assert.That(game.AttackScore, Is.EqualTo(0));

            // player is partially reset
            Assert.That(game.PlayerEndurance, Is.EqualTo(game.MaxPlayerEndurance));

            // new boss is set
            Assert.That(game.BossHealth, Is.GreaterThan(200));
            Assert.That(game.BossDamage, Is.EqualTo(0));

            Assert.That(game.Level, Is.EqualTo(2));
        }

    }
}
