using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayPreparationSubState : ScreenBaseState
    {
        public PlayState PlayState => ParentState as PlayState;

        public PlayPreparationSubState(FlowController flowController, ScreenBaseState parent)
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

            TemplateContainer elementInstance = ElementInstance("UI/Frames/PreparationFrame");

            var frameButtons = new Button[] {
                ButtonAction("BATTLE", PlayState.VtrBtnAction)
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicPreparation());
            
            PlayState.AddFrameButtons(frameButtons);

            if (PlayState.GameAsset != null)
            {
                PlayState.SetLevel(PlayState.GameAsset.Level.ToString());
                PlayState.SetRound(PlayState.GameAsset.Round.ToString());
            }
            else
            {
                PlayState.SetLevel("-");
                PlayState.SetRound("-");
            }

            floatBody.Add(elementInstance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }


        private void ExtrinsicPreparation()
        {
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.PREPARATION, new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset }, out IAsset[] _);

            if (resultFirst)
            {
                FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
            }
            else
            {
                Debug.LogError("Failed to transition to START");
            }
        }
    }
}