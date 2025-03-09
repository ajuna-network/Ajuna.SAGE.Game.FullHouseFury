using System.Collections.Generic;
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
            FlowController.ChangeScreenState(ScreenState.Play);
        }
    }
}