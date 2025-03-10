using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class PlayBattleSubState : ScreenBaseState
    {
        private VisualElement _velAttackCards, _velHandCards;

        private List<HandCard> _handCards = new List<HandCard>();

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

            var frameButtons = new Button[] {
                ButtonAction("DISCARD", PlayState.VtrBtnAction),
                ButtonAction("ATTACK", PlayState.VtrBtnAction)
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicDiscard());
            frameButtons[1].RegisterCallback<ClickEvent>(evt => ExtrinsicAttack());
            PlayState.AddFrameButtons(frameButtons);

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

            foreach (var handCard in _handCards)
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
        }

        private void ExtrinsicDiscard()
        {
            throw new NotImplementedException();
        }

        private void ExtrinsicAttack()
        {
            throw new NotImplementedException();
        }
    }
}

