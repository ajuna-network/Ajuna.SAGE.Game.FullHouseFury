using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Assets.Scripts.ScreenStates;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayScoreSubState : ScreenBaseState
    {
        private Label _txtBossName;
        private VisualElement _velBossCurrentHealthValue;
        private Label _lblBossHealthText;

        private Label _txtPlayerName;
        private VisualElement _velPlayerHealthValue;
        private Label _lblPlayerHealthText;
        private Label _lblResult;

        public PlayState PlayState => ParentState as PlayState;

        public PlayScoreSubState(FlowController flowController, ScreenBaseState parent)
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

            TemplateContainer elementInstance = ElementInstance("UI/Frames/ScoreFrame");


            _lblResult = elementInstance.Q<Label>("TxtResult");

            var velPlayer = elementInstance.Q<VisualElement>("VelPlayer");
            _txtPlayerName = velPlayer.Q<Label>("TxtPlayerName");
            _velPlayerHealthValue = velPlayer.Q<VisualElement>("VelCurrentValue");
            _lblPlayerHealthText = velPlayer.Q<Label>("TxtValue");

            var velBoss = elementInstance.Q<VisualElement>("VelBoss");
            _txtBossName = velBoss.Q<Label>("TxtBossName");
            _velBossCurrentHealthValue = velBoss.Q<VisualElement>("VelCurrentValue");
            _lblBossHealthText = velBoss.Q<Label>("TxtValue");

            UpdatePlayers(velPlayer, velBoss);

            var frameButtons = new Button[] {
                ButtonAction("NEXT", PlayState.VtrBtnAction),
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicScore());
            PlayState.AddFrameButtons(frameButtons);

            UpdateBattleStats();

            floatBody.Add(elementInstance);

        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
            if (PlayState.GameAsset.GameState == GameState.Finished)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void UpdatePlayers(VisualElement velPlayer, VisualElement velBoss)
        {
            velPlayer.Q<VisualElement>("VelPlayerCont").Add(PlayState.VelCurrentPlayer);
            velBoss.Q<VisualElement>("VelPlayerCont").Add(PlayState.VelCurrentOpponent);

            _txtPlayerName.text = PlayState.CurrentPlayer.ShortName();
            _txtBossName.text = PlayState.CurrentOpponent.ShortName();

        }
        private void ExtrinsicScore()
        {
            if (PlayState.GameAsset.GameState != GameState.Running)
            {
                FlowController.ChangeScreenState(ScreenState.Menu);
                return;
            }

            var inAsset = new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset };
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.SCORE, inAsset, out IAsset[] _);
            if (!resultFirst)
            {
                Debug.LogWarning("Wasn't successfull in executing the ExtrinsicScore!");
                return;
            }

            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Preparation);
        }

        private void UpdateBattleStats()
        {
            _lblResult.text = PlayState.GameAsset.GameState == GameState.Running ? "YOU WON!" : "YOU LOST!";

            var maxBossHealth = PlayState.GameAsset.MaxBossHealth;
            var currentBossHealth = PlayState.GameAsset.BossHealth > 0 ? PlayState.GameAsset.BossHealth : 0;

            var maxPlayerHealth = PlayState.GameAsset.MaxPlayerHealth;
            var currentPlayerHealth = PlayState.GameAsset.PlayerHealth > 0 ? PlayState.GameAsset.PlayerHealth : 0;

            _lblBossHealthText.text = $"{currentBossHealth}";
            _velBossCurrentHealthValue.style.width = new StyleLength(new Length((float)currentBossHealth / maxBossHealth * 100, LengthUnit.Percent));

            _lblPlayerHealthText.text = $"{currentPlayerHealth}";
            _velPlayerHealthValue.style.width = new StyleLength(new Length((float)currentPlayerHealth / maxPlayerHealth * 100, LengthUnit.Percent));
        }
    }
}