﻿using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{

    [TestFixture]
    public class FullHouseFuryDiscardTests : FullHouseFuryBaseTest
    {
        private readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Start();
        private readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Play();
        private readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Preparation();
        private readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Battle();
        private readonly FullHouseFuryIdentifier DISCARD = FullHouseFuryIdentifier.Discard();
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
        public void Test_DiscardLevel()
        {
            Assert.That(BlockchainInfoProvider.CurrentBlockNumber, Is.EqualTo(5));

            var preGame = GetAsset<GameAsset>(_user, AssetType.Game, AssetSubType.None);
            var preDeck = GetAsset<DeckAsset>(_user, AssetType.Deck, AssetSubType.None);
            var preTowr = GetAsset<TowerAsset>(_user, AssetType.Tower, AssetSubType.None);
            IAsset[] inAsset = [preGame, preDeck, preTowr];

            var preHand = new Card?[10];
            for (int i = 0; i < 10; i++)
            {
                preHand[i] = preDeck.TryGetHandCard(i, out byte cardIndex) ? new Card(cardIndex) : null;
            }
            var preHandString = "3♠ Q♥ A♣ 4♠ 6♠ J♠ 7♣";
            Assert.That(string.Join(" ", preHand.Select(c => c.ToString())).Trim(), Is.EqualTo(preHandString));

            byte[] config = [0, 1, 3];

            bool resultFirst = Engine.Transition(_user, DISCARD, inAsset, out IAsset[] outAssets, config);
            Assert.That(resultFirst, Is.True, "transition result should succeed.");

            // Capture key state after the first gamble.
            var game = outAssets[0] as GameAsset;
            var deck = outAssets[1] as DeckAsset;
            var towr = outAssets[2] as TowerAsset;

            Assert.That(game, Is.Not.Null);
            Assert.That(deck, Is.Not.Null);
            Assert.That(towr, Is.Not.Null);

            var hand = new Card?[10];
            for (int i = 0; i < 10; i++)
            {
                hand[i] = deck.TryGetHandCard(i, out byte cardIndex) ? new Card(cardIndex) : null;
            }

            Assert.That(game.GameState, Is.EqualTo(GameState.Running));
            Assert.That(game.LevelState, Is.EqualTo(LevelState.Battle));

            Assert.That(deck.DeckSize, Is.EqualTo(42));

            Assert.That(game.AttackType, Is.EqualTo(PokerHand.None));
            Assert.That(game.AttackScore, Is.EqualTo(0));

            var handString = string.Join(" ", hand.Select(c => c.ToString())).Trim();
            Assert.That(handString, Is.Not.EqualTo(preHandString));
            Assert.That(handString, Is.EqualTo("3♦ 6♥ A♣ 8♠ 6♠ J♠ 7♣"));    

        }
    }
}
