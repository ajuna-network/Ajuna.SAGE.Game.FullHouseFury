using UnityEngine;

namespace Assets.Scripts.ScreenStates
{
    internal class MainDashboardSubState : ScreenBaseState
    {
        public PlayState MainScreenState => ParentState as PlayState;

        public MainDashboardSubState(FlowController flowController, ScreenBaseState parent)
            : base(flowController, parent)
        {

        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }
    }
}