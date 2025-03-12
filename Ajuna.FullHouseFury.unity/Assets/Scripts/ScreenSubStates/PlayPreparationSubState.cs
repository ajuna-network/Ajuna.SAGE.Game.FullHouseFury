using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayPreparationSubState : ScreenBaseState
    {
        private Label _txtBossName;
        private Label _txtLevelName;

        private VisualElement _velBossCurrentHealthValue;
        private Label _lblBossHealthText;

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

            var velLevel = elementInstance.Q<VisualElement>("VelLevel");
            _txtBossName = velLevel.Q<Label>("TxtBossName");
            _txtLevelName = velLevel.Q<Label>("TxtLevelName");

            var velBoss = elementInstance.Q<VisualElement>("VelBoss");
            _velBossCurrentHealthValue = velBoss.Q<VisualElement>("VelCurrentValue");
            _lblBossHealthText = velBoss.Q<Label>("TxtValue");
            _txtBossName.text = "C.COX";

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

            UpdateBattleStats();

            floatBody.Add(elementInstance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }

        private void UpdateBattleStats()
        {
            _txtLevelName.text = $"Lvl. {PlayState.GameAsset.Level}";

            var maxBossHealth = PlayState.GameAsset.MaxBossHealth;
            var currentBossHealth = PlayState.GameAsset.BossHealth > 0 ? PlayState.GameAsset.BossHealth : 0;

            //var maxPlayerHealth = PlayState.GameAsset.MaxPlayerHealth;
            //var currentPlayerHealth = PlayState.GameAsset.PlayerHealth > 0 ? PlayState.GameAsset.PlayerHealth : 0;

            _lblBossHealthText.text = $"{currentBossHealth}";
            _velBossCurrentHealthValue.style.width = new StyleLength(new Length((float)currentBossHealth / maxBossHealth * 100, LengthUnit.Percent));

            //_lblPlayerHealthText.text = $"{currentPlayerHealth}";
            //_velPlayerHealthValue.style.width = new StyleLength(new Length((float)currentPlayerHealth / maxPlayerHealth * 100, LengthUnit.Percent));

        }

        private void ExtrinsicPreparation()
        {
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.PREPARATION, new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset }, out IAsset[] _);

            if (!resultFirst)
            {
                Debug.LogWarning("Wasn't successfull in executing the ExtrinsicPreparation!"); 
                return;
            }

            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
        }
    }
}