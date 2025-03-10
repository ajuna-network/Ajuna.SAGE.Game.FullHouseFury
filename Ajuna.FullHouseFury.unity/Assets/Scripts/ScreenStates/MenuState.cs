using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class MenuState : ScreenBaseState
    {
        private Button _btnPlay;
        private Button _btnQuit;

        public MenuState(FlowController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/MenuUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _btnPlay = instance.Q<Button>("BtnPlay");
            _btnPlay.RegisterCallback<ClickEvent>(OnClickBtnPlay);

            // add container
            FlowController.VelContainer.Add(instance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");
            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnClickBtnPlay(ClickEvent evt)
        {
            var preGame = FlowController.GetAsset<GameAsset>(FlowController.User, AssetType.Game, AssetSubType.None);
            var preFeck = FlowController.GetAsset<DeckAsset>(FlowController.User, AssetType.Deck, AssetSubType.None);

            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.PLAY, new IAsset[] { preGame, preFeck }, out IAsset[] outAssets);

            if (resultFirst)
            {
                FlowController.ChangeScreenState(ScreenState.Play);
            }
            else
            {
                Debug.LogError("Failed to transition to PLAY");
            }
        }
    }
}