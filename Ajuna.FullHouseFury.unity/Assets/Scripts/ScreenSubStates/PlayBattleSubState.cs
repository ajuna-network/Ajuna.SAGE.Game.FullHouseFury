﻿using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public enum SortCards
    {
        Rank,
        Suit
    }

    public class PlayBattleSubState : ScreenBaseState
    {
        private VisualElement _velAttackCards, _velHandCards;
        private Label _lblPokerHandText;
        private List<HandCard> _handCards = new List<HandCard>();

        private Label _txtBossName;
        private VisualElement _velBossCurrentHealthValue;
        private Label _lblBossHealthText;

        private Label _txtPlayerName;
        private VisualElement _velPlayerHealthValue;
        private Label _lblPlayerHealthText;

        private Label _lblDeckSize;
        private Label _lblDiscards;
        private Label _lblFatigue;
        private VisualElement _velEnduranceValue;
        private Label _lblEnduranceText;
        private Button[] _frameButtons;

        private Label _lblRarityMultiSign;
        private Label _lblRarityMultiplier;
        private Label _lblPokerMultiSign;
        private Label _lblPokerMultiplier;

        private Label _lblDmgSignText;
        private Label _lblBaseDamage;

        private Label _lblBaseDamageText;

        private Label _lblFactor;
        private Label _lblKicker;
        private Label _lblBonus;
        private VisualElement _velHandSort;

        private SortCards _sortCards;

        public PlayState PlayState => ParentState as PlayState;

        public PlayBattleSubState(FlowController flowController, ScreenBaseState parent)
            : base(flowController, parent)
        {
            _sortCards = SortCards.Rank;
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            // make sure we load the assets first
            PlayState.LoadAssets();

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer elementInstance = ElementInstance("UI/Frames/BattleFrame");

            _velAttackCards = elementInstance.Q<VisualElement>("VelAttackCards");
            _velHandCards = elementInstance.Q<VisualElement>("VelHandCards");

            var velPokerHand = elementInstance.Q<VisualElement>("VelPokerHand");
            _lblPokerHandText = velPokerHand.Q<Label>("TxtPokerHand");

            var velBoss = elementInstance.Q<VisualElement>("VelBoss");
            _txtBossName = velBoss.Q<Label>("TxtBossName");
            _velBossCurrentHealthValue = velBoss.Q<VisualElement>("VelCurrentValue");
            _lblBossHealthText = velBoss.Q<Label>("TxtValue");

            var velDamage = velBoss.Q<VisualElement>("VelDamage");

            _lblFactor = velDamage.Q<Label>("TxtFactor");
            _lblFactor.style.display = DisplayStyle.None;
            _lblKicker = velDamage.Q<Label>("TxtKicker");
            _lblKicker.style.display = DisplayStyle.None;
            _lblBonus = velDamage.Q<Label>("TxtBonus");
            _lblBonus.style.display = DisplayStyle.None;

            _lblRarityMultiSign = velDamage.Q<Label>("TxtRarityMultiSign");
            _lblRarityMultiSign.style.display = DisplayStyle.None;
            _lblRarityMultiplier = velDamage.Q<Label>("TxtRarityMultiplier");
            _lblRarityMultiplier.style.display = DisplayStyle.None;
            _lblPokerMultiSign = velDamage.Q<Label>("TxtPokerMultiSign");
            _lblPokerMultiSign.style.display = DisplayStyle.None;
            _lblPokerMultiplier = velDamage.Q<Label>("TxtPokerMultiplier");
            _lblPokerMultiplier.style.display = DisplayStyle.None;

            _lblDmgSignText = velDamage.Q<Label>("TxtDmgSign");
            _lblDmgSignText.style.display = DisplayStyle.None;
            _lblBaseDamage = velDamage.Q<Label>("TxtBaseDamage");
            _lblBaseDamage.style.display = DisplayStyle.None;

            _lblBaseDamageText = velDamage.Q<Label>("TxtBaseDamageText");
            _lblBaseDamageText.style.display = DisplayStyle.None;

            var velHand = elementInstance.Q<VisualElement>("VelHand");
            _velHandSort = elementInstance.Q<VisualElement>("VelHandSort");
            _velHandSort.RegisterCallback<ClickEvent>(evt => ToggleSort());

            var velPlayer = elementInstance.Q<VisualElement>("VelPlayer");
            _txtPlayerName = velPlayer.Q<Label>("TxtPlayerName");
            _velPlayerHealthValue = velPlayer.Q<VisualElement>("VelCurrentValue");
            _lblPlayerHealthText = velPlayer.Q<Label>("TxtValue");
            _txtPlayerName.text = "PLAYER";

            var velDeckSize = velPlayer.Q<VisualElement>("VelDeckSize");
            _lblDeckSize = velDeckSize.Q<Label>("TxtValue");

            var velDiscards = velPlayer.Q<VisualElement>("VelDiscards");
            _lblDiscards = velDiscards.Q<Label>("TxtValue");

            var velFatigue = velPlayer.Q<VisualElement>("VelFatigue");
            _lblFatigue = velFatigue.Q<Label>("TxtValue");

            var velEndurance = velPlayer.Q<VisualElement>("VelEndurance");
            _velEnduranceValue = velEndurance.Q<VisualElement>("VelCurrentValue");
            _lblEnduranceText = velEndurance.Q<Label>("TxtValue");

            UpdatePlayers(velPlayer, velBoss);

            _frameButtons = new Button[] {
                ButtonAction("DISCARD", PlayState.VtrBtnAction),
                ButtonAction("ATTACK", PlayState.VtrBtnAction)
            };
            _frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicDiscard());
            _frameButtons[1].RegisterCallback<ClickEvent>(evt => ExtrinsicAttack());
            PlayState.AddFrameButtons(_frameButtons);

            floatBody.Add(elementInstance);

            LoadHandCards();
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }

        private void UpdatePlayers(VisualElement velPlayer, VisualElement velBoss)
        {
            velPlayer.Q<VisualElement>("VelPlayerCont").Add(PlayState.VelCurrentPlayer);
            velBoss.Q<VisualElement>("VelPlayerCont").Add(PlayState.VelCurrentOpponent);

            _txtPlayerName.text = PlayState.CurrentPlayer.ShortName();
            _txtBossName.text = PlayState.CurrentOpponent.ShortName();
        }

        private void LoadHandCards()
        {
            _handCards.Clear();

            for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
            {
                PlayState.DeckAsset.GetHandCard(i, out byte cardIndex, out byte rarity);
                if (cardIndex != DeckAsset.EMPTY_SLOT)
                {
                    var card = new Card(cardIndex, rarity);
                    var handCard = new HandCard(i, card);
                    var templateContainer = PlayState.VelCard.Instantiate();
                    var velCard = templateContainer.Q<VisualElement>("VelCard");
                    var velGlow = templateContainer.Q<VisualElement>("VelGlow");
                    var txtCard = templateContainer.Q<Label>("TxtMultiplier");
                    txtCard.text = ((int)card.Rarity).ToString();
                    velGlow.style.backgroundColor = new StyleColor(HelperUtil.GetRarityColor(card.Rarity));
                    velCard.RegisterCallback<ClickEvent>(evt => HandCardStateChange(handCard));
                    velCard.style.backgroundImage = new StyleBackground(PlayState.SprDeck.FirstOrDefault(s => s.name ==
                        HelperUtil.GetCardSpritName(card.Suit, card.Rank)));
                    handCard.VisualElement = templateContainer;
                    _handCards.Add(handCard);
                }
            }

            ReloadCards();
        }

        private void HandCardStateChange(HandCard handCard)
        {
            switch (handCard.HandCardState)
            {
                case HandCardState.InHand:

                    if (_velAttackCards.childCount >= 5)
                    {
                        Debug.Log("Attack limit reached");
                        return;
                    }

                    handCard.SetState(HandCardState.InPlay);
                    break;

                case HandCardState.InPlay:
                    handCard.SetState(HandCardState.InHand);
                    break;
            }

            ReloadCards();
        }

        private void ToggleSort()
        {
            _sortCards = _sortCards == SortCards.Rank ? SortCards.Suit : SortCards.Rank;
            ReloadCards();
        }

        private void ReloadCards()
        {
            _velHandCards.Clear();
            _velAttackCards.Clear();

            IEnumerable<HandCard> _sortedHandCards = _handCards.OrderByDescending(p => p.Card.Rank == Rank.Ace ? 14 : (int)p.Card.Rank).ThenBy(p => p.Card.Suit);
            switch (_sortCards)
            {

                case SortCards.Suit:
                    _sortedHandCards = _handCards.OrderBy(p => p.Card.Suit).ThenByDescending(p => p.Card.Rank == Rank.Ace ? 14 : (int)p.Card.Rank);
                    break;

                case SortCards.Rank:
                default:
                    break;
            }

            foreach (var handCard in _sortedHandCards)
            {
                switch (handCard.HandCardState)
                {
                    case HandCardState.InPlay:
                        _velAttackCards.Add(handCard.VisualElement);
                        break;

                    case HandCardState.InHand:
                        _velHandCards.Add(handCard.VisualElement);
                        break;
                }
            }

            UpdateBattleStats();
        }

        private void UpdateBattleStats()
        {
            var maxBossHealth = PlayState.GameAsset.MaxBossHealth;
            var currentBossHealth = PlayState.GameAsset.BossHealth > 0 ? PlayState.GameAsset.BossHealth : 0;

            var maxPlayerHealth = PlayState.GameAsset.MaxPlayerHealth;
            var currentPlayerHealth = PlayState.GameAsset.PlayerHealth > 0 ? PlayState.GameAsset.PlayerHealth : 0;

            _lblBossHealthText.text = $"{currentBossHealth}";
            _velBossCurrentHealthValue.style.width = new StyleLength(new Length((float)currentBossHealth / maxBossHealth * 100, LengthUnit.Percent));

            _lblPlayerHealthText.text = $"{currentPlayerHealth}";
            _velPlayerHealthValue.style.width = new StyleLength(new Length((float)currentPlayerHealth / maxPlayerHealth * 100, LengthUnit.Percent));

            _lblDeckSize.text = $"{PlayState.DeckAsset.DeckSize}/{PlayState.DeckAsset.MaxDeckSize}";
            _lblDiscards.text = $"{PlayState.GameAsset.Discard}";
            _lblFatigue.text = $"{PlayState.GameAsset.FatigueDamage}";

            // discard button
            _frameButtons[0].SetEnabled(PlayState.GameAsset.Discard > 0);

            var playerEndurance = PlayState.GameAsset.PlayerEndurance;
            var maxPlayerEndurance = PlayState.GameAsset.MaxPlayerEndurance;
            _lblEnduranceText.text = $"{playerEndurance}/{maxPlayerEndurance}";
            _velEnduranceValue.style.width = new StyleLength(new Length((float)playerEndurance / maxPlayerEndurance * 100, LengthUnit.Percent));

            if (_velAttackCards.childCount > 0)
            {
                /*
                    score = multiplier * (factor * kicker + bonus);

                    scoreCard = new ushort[4];
                    scoreCard[0] = (ushort)multiplier;
                    scoreCard[1] = (ushort)factor;
                    scoreCard[2] = (ushort)kicker;
                    scoreCard[3] = (ushort)bonus;
                */

                var attackCardsArray = _handCards.Where(p => p.HandCardState == HandCardState.InPlay).Select(p => FullHouseFuryUtil.EncodeCardByte(p.Card.Index, (byte)p.Card.Rarity)).ToArray();
                var evaluation = FullHouseFuryUtil.Evaluate(attackCardsArray, PlayState.DeckAsset.PokerHandLevels(), out ushort score, out ushort[] scoreCard);
                _lblPokerHandText.text = evaluation.ToString();

                Debug.Log($"{scoreCard[0]},{scoreCard[1]},{scoreCard[2]},{scoreCard[3]}");

                var factor = scoreCard[0];
                var kicker = scoreCard[1];
                var bonus = scoreCard[2];
                var multi_rarity = scoreCard[3];
                var multi_pokerh = scoreCard[4];

                _lblFactor.text = factor.ToString();
                _lblKicker.text = kicker.ToString();
                _lblBonus.text = bonus.ToString();

                _lblFactor.style.display = DisplayStyle.Flex;
                _lblKicker.style.display = DisplayStyle.Flex;
                _lblBonus.style.display = DisplayStyle.Flex;

                _lblRarityMultiplier.text = multi_rarity.ToString();
                _lblPokerMultiplier.text = multi_pokerh.ToString();

                _lblDmgSignText.text = "=";
                _lblBaseDamage.text = score.ToString();

                _lblRarityMultiSign.style.display = DisplayStyle.Flex;
                _lblRarityMultiplier.style.display = DisplayStyle.Flex;
                _lblPokerMultiSign.style.display = DisplayStyle.Flex;
                _lblPokerMultiplier.style.display = DisplayStyle.Flex;

                _lblDmgSignText.style.display = DisplayStyle.Flex;
                _lblBaseDamage.style.display = DisplayStyle.Flex;
                _lblBaseDamageText.style.display = DisplayStyle.Flex;
                _frameButtons[1].SetEnabled(true);
            }
            else
            {
                _frameButtons[1].SetEnabled(false);

                _lblFactor.style.display = DisplayStyle.None;
                _lblKicker.style.display = DisplayStyle.None;
                _lblBonus.style.display = DisplayStyle.None;

                _lblRarityMultiSign.style.display = DisplayStyle.None;
                _lblRarityMultiplier.style.display = DisplayStyle.None;
                _lblPokerMultiSign.style.display = DisplayStyle.None;
                _lblPokerMultiplier.style.display = DisplayStyle.None;
                _lblDmgSignText.style.display = DisplayStyle.None;
                _lblBaseDamage.style.display = DisplayStyle.None;
                _lblBaseDamageText.style.display = DisplayStyle.None;
                _lblPokerHandText.text = "";
            }

        }

        private void ExtrinsicDiscard()
        {
            var discardHand = _handCards.Where(p => p.HandCardState == HandCardState.InPlay).Select(p => (byte)p.Index).ToArray();
            if (discardHand == null || discardHand.Length == 0)
            {
                Debug.Log("No cards selected for discard");
                return;
            }
            var inAsset = new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset };
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.DISCARD, inAsset, out IAsset[] outAssets, discardHand);

            if (resultFirst)
            {
                FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
            }
            else
            {
                Debug.LogError("Failed to transition to START");
            }
        }

        private void ExtrinsicAttack()
        {
            var attackHand = _handCards.Where(p => p.HandCardState == HandCardState.InPlay).Select(p => (byte)p.Index).ToArray();
            if (attackHand == null || attackHand.Length == 0)
            {
                Debug.Log("No cards selected for attack");
                return;
            }

            var inAsset = new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset };
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.BATTLE, inAsset, out IAsset[] outAssets, attackHand);

            if (!resultFirst)
            {
                Debug.LogError("Failed to transition to START");
                return;
            }

            switch (PlayState.GameAsset.LevelState)
            {
                case LevelState.Battle:
                    FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
                    return;

                case LevelState.Score:
                    FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Score);
                    return;

                default:
                    Debug.LogError($"Wrong LevelState {PlayState.GameAsset.LevelState}!");
                    return;
            }
        }
    }
}

