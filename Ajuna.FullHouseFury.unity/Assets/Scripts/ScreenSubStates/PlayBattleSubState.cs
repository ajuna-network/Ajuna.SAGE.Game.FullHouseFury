using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Core;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.STP;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Assets.Scripts.ScreenStates
{
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
        private Label _lblBaseDamageText;
        private Label _lblDmgSignText;
        private Label _lblBaseDamage;

        public PlayState PlayState => ParentState as PlayState;

        public PlayBattleSubState(FlowController flowController, ScreenBaseState parent)
            : base(flowController, parent)
        {

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
            _txtBossName.text = "C.COX";

            var velDamage = velBoss.Q<VisualElement>("VelDamage");
            _lblDmgSignText = velDamage.Q<Label>("TxtDmgSign");
            _lblDmgSignText.style.display = DisplayStyle.None;
            _lblBaseDamage = velDamage.Q<Label>("TxtBaseDamage");
            _lblBaseDamage.style.display = DisplayStyle.None;
            _lblBaseDamageText = velDamage.Q<Label>("TxtBaseDamageText");
            _lblBaseDamageText.style.display = DisplayStyle.None;

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


            _frameButtons = new Button[] {
                ButtonAction("DISCARD", PlayState.VtrBtnAction),
                ButtonAction("ATTACK", PlayState.VtrBtnAction)
            };
            _frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicDiscard());
            _frameButtons[1].RegisterCallback<ClickEvent>(evt => ExtrinsicAttack());
            PlayState.AddFrameButtons(_frameButtons);

            PlayState.SetLevel(PlayState.GameAsset.Level.ToString());
            PlayState.SetRound(PlayState.GameAsset.Round.ToString());

            floatBody.Add(elementInstance);

            LoadHandCards();
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }

        private void LoadHandCards() 
        {
            _handCards.Clear();

            for (int i = 0; i < DeckAsset.HAND_LIMIT_SIZE; i++)
            {
                var handCardIndex = PlayState.DeckAsset.GetHandCard(i);
                if (handCardIndex != DeckAsset.EMPTY_SLOT)
                {
                    var card = new Card(handCardIndex);
                    var handCard = new HandCard(i, card);
                    var templateContainer = PlayState.VelCard.Instantiate();
                    var velCard = templateContainer.Q<VisualElement>("VelCard");
                    velCard.RegisterCallback<ClickEvent>(evt => HandCardStateChange(handCard));
                    velCard.style.backgroundImage = new StyleBackground(PlayState.SprDeck.FirstOrDefault(s => s.name ==
                        HelperUtil.GetCardSpritName(card.Suit, card.Rank)));
                    handCard.VisualElement = velCard;
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

        private void ReloadCards()
        {
            _velHandCards.Clear();
            _velAttackCards.Clear();

            foreach (var handCard in _handCards.OrderByDescending(p => p.Card.Rank).ThenBy(p => p.Card.Suit))
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
            var currentBossHealth = PlayState.GameAsset.BossHealth;

            var maxPlayerHealth = PlayState.GameAsset.MaxPlayerHealth;
            var currentPlayerHealth = PlayState.GameAsset.PlayerHealth;

            _lblBossHealthText.text = $"{currentBossHealth} / {maxBossHealth}";
            _velBossCurrentHealthValue.style.width = new StyleLength(new Length((float)currentBossHealth / maxBossHealth * 100, LengthUnit.Percent));

            _lblPlayerHealthText.text = $"{currentPlayerHealth} / {maxPlayerHealth}";
            _velPlayerHealthValue.style.width = new StyleLength(new Length((float)currentPlayerHealth / maxPlayerHealth * 100, LengthUnit.Percent));

            _lblDeckSize.text = $"{PlayState.DeckAsset.DeckSize} / {PlayState.DeckAsset.MaxDeckSize}";
            _lblDiscards.text = $"{PlayState.GameAsset.Discard}";
            _lblFatigue.text = $"{PlayState.GameAsset.FatigueDamage}";

            // discard button
            _frameButtons[0].SetEnabled(PlayState.GameAsset.Discard > 0);

            var playerEndurance = PlayState.GameAsset.PlayerEndurance;
            var maxPlayerEndurance = PlayState.GameAsset.MaxPlayerEndurance;
            _lblEnduranceText.text = $"{playerEndurance} / {maxPlayerEndurance}";
            _velEnduranceValue.style.width = new StyleLength(new Length((float)playerEndurance / maxPlayerEndurance * 100, LengthUnit.Percent));

            if (_velAttackCards.childCount > 0)
            {
                var attackCardsArray = _handCards.Where(p => p.HandCardState == HandCardState.InPlay).Select(p => p.Card.Index).ToArray();
                var evaluation = FullHouseFuryUtil.Evaluate(attackCardsArray, out ushort score);
                _lblPokerHandText.text = evaluation.ToString();
                
                _lblDmgSignText.text = "+";
                _lblBaseDamage.text = score.ToString();

                _lblDmgSignText.style.display = DisplayStyle.Flex;
                _lblBaseDamage.style.display = DisplayStyle.Flex;
                _lblBaseDamageText.style.display = DisplayStyle.Flex;
                _frameButtons[1].SetEnabled(true);
            } 
            else
            {
                _frameButtons[1].SetEnabled(false);
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

            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.DISCARD, new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset }, out IAsset[] outAssets, discardHand);

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
            if (attackHand == null ||attackHand.Length == 0)
            {
                Debug.Log("No cards selected for attack");
                return;
            }

            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.BATTLE, new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset }, out IAsset[] outAssets, attackHand);

            if (resultFirst)
            {
                PlayState.LoadAssets();

                switch (PlayState.GameAsset.LevelState)
                {
                    case LevelState.Preparation:
                        resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.PREPARATION, new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset }, out IAsset[] _);

                        if (resultFirst)
                        {
                            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
                        }
                        else
                        {
                            Debug.LogError("Failed to transition to PREPARATION");
                        }
                        break;

                    case LevelState.Score:
                        FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Score);
                        break;

                    default:
                        Debug.LogError($"Wrong LevelState {PlayState.GameAsset.LevelState}!");
                        return;
                }
            }
            else
            {
                Debug.LogError("Failed to transition to START");
            }
        }
    }
}

