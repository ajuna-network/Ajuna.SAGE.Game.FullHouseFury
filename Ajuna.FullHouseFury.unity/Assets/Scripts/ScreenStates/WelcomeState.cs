using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Core;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Assets.Scripts.ScreenStates
{
    public class WelcomeState : ScreenBaseState
    {
        private Button _btnStart;

        private VisualElement _velLogo;

        public WelcomeState(FlowController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/WelcomeUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _btnStart = instance.Q<Button>("BtnStart");
            _btnStart.RegisterCallback<ClickEvent>(OnClickBtnStart);

            // add container
            FlowController.VelContainer.Add(instance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");
            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnClickBtnStart(ClickEvent evt)
        {
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.START, new IAsset[] { }, out IAsset[] _);

            if (resultFirst)
            {
                FlowController.ChangeScreenState(ScreenState.Menu);
            } 
            else
            {
                Debug.LogError("Failed to transition to START");
            }
        }
    }
}