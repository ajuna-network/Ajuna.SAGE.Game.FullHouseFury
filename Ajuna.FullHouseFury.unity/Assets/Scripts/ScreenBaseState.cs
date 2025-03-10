using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public abstract class ScreenBaseState
    {
        protected FlowController FlowController { get; private set; }

        protected ScreenBaseState ParentState { get; private set; }

        protected ScreenBaseState(FlowController flowController, ScreenBaseState parentState = null)
        {
            FlowController = flowController;
            ParentState = parentState;
        }

        public abstract void EnterState();

        public abstract void ExitState();

        internal TemplateContainer ElementInstance(string elementPath, int widthPerc = 100, int heightPerc = 100)
        {
            var element = Resources.Load<VisualTreeAsset>(elementPath);
            var elementInstance = element.Instantiate();
            elementInstance.style.width = new Length(widthPerc, LengthUnit.Percent);
            elementInstance.style.height = new Length(heightPerc, LengthUnit.Percent);
            return elementInstance;
        }

        internal Button ButtonAction(string name, VisualTreeAsset vtrBtnAction)
        {
            var templateContainer = vtrBtnAction.Instantiate();
            var _btnAction = templateContainer.Q<Button>("BtnAction");
            _btnAction.text = name;
            return _btnAction;
        }

    }
}