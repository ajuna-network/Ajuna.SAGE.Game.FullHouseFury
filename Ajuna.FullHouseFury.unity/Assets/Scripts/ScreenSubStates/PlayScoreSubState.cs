using Assets.Scripts.ScreenStates;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayScoreSubState : ScreenBaseState
    {
        public PlayState PlayState => ParentState as PlayState;

        public PlayScoreSubState(FlowController flowController, ScreenBaseState parent)
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