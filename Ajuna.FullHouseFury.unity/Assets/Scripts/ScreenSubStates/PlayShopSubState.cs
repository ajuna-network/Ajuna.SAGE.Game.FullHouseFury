using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Assets.Scripts.ScreenStates;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayShopSubState : ScreenBaseState
    {
        public PlayState PlayState => ParentState as PlayState;

        public PlayShopSubState(FlowController flowController, ScreenBaseState parent)
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

            TemplateContainer elementInstance = ElementInstance("UI/Frames/ShopFrame");

            // add your stuff here


            var frameButtons = new Button[] {
                ButtonAction("BACK", PlayState.VtrBtnAction),
                ButtonAction("BUY", PlayState.VtrBtnAction)
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => FramePreparation());
            frameButtons[1].RegisterCallback<ClickEvent>(evt => ExtrinsicShop());
            PlayState.AddFrameButtons(frameButtons);

            floatBody.Add(elementInstance);

        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

        }

        private void FramePreparation()
        {
            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Preparation);
        }

        private void ExtrinsicShop()
        {
            
        }
    }
}