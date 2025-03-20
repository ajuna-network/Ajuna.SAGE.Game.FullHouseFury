using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

namespace Assets.Scripts
{
    public class PlayPreparationSubState : ScreenBaseState
    {
        private Label _txtBossName;
        private Label _txtLevelName;

        private VisualElement _velBossCurrentHealthValue;
        private Label _lblBossHealthText;

        private VisualElement[] _velChoices;

        public PlayState PlayState => ParentState as PlayState;

        public int _selectedBoonOrBane = 0;

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

            _velChoices = new VisualElement[] {
                elementInstance.Q<VisualElement>("VelBoonAndBane1"),
                elementInstance.Q<VisualElement>("VelBoonAndBane2"),
                elementInstance.Q<VisualElement>("VelBoonAndBane3")
            };

            var frameButtons = new Button[] {
                ButtonAction("BATTLE", PlayState.VtrBtnAction)
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => ExtrinsicPreparation());
            
            PlayState.AddFrameButtons(frameButtons);

            UpdateBattleStats();

            UpdateBoonOrBane(0, _velChoices[0]);
            UpdateBoonOrBane(1, _velChoices[1]);
            UpdateBoonOrBane(2, _velChoices[2]);

            BoonAndBaneClicked(_selectedBoonOrBane);

            floatBody.Add(elementInstance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }

        private void UpdateBoonOrBane(int index, VisualElement velBoonAndBane)
        {
            var boonAndBane = PlayState.TowrAsset.GetBoonAndBane(index);
            var boni = FullHouseFuryUtil.GetBonusInfo(boonAndBane.boon);
            var mali = FullHouseFuryUtil.GetMalusInfo(boonAndBane.bane);

            if (boni is null || mali is null) 
            {
                velBoonAndBane.style.display = DisplayStyle.None;
                Debug.Log($"Boon {boonAndBane.boon} or Bane {boonAndBane.bane} is None");
                return;
            }
            
            velBoonAndBane.style.display = DisplayStyle.Flex;
            velBoonAndBane.RegisterCallback<ClickEvent>(evt => BoonAndBaneClicked(index));

            var velBoon = velBoonAndBane.Q<VisualElement>("VelBoon");
            var velBane = velBoonAndBane.Q<VisualElement>("VelBane");
            var boonName = velBoon.Q<Label>("TxtBoonOrBaneName");
            var boonDesc = velBoon.Q<Label>("TxtBoonOrBaneDesc");
            var baneName = velBane.Q<Label>("TxtBoonOrBaneName");
            var baneDesc = velBane.Q<Label>("TxtBoonOrBaneDesc");

            boonName.text = boni[0].ToString();
            boonDesc.text = boni[1].ToString();
            baneName.text = mali[0].ToString();
            baneDesc.text = mali[1].ToString();
        }

        private void BoonAndBaneClicked(int index)
        {
            Debug.Log($"Clicked on {index}!");
            _selectedBoonOrBane = index;

            var green = new Color(50/255f, 200 / 255f, 50 / 255f, 100 / 255f);
            var greenSel = new Color(50 / 255f, 200 / 255f, 50 / 255f, 255 / 255f);

            var red = new Color(200 / 255f, 50 / 255f, 50 / 255f, 100 / 255f);
            var redSel = new Color(200 / 255f, 50 / 255f, 50 / 255f, 255 / 255f);

            for (int i = 0; i < 3; i++)
            {
                var isSelected = (i == index);

                var velBase = _velChoices[i];

                var vel1 = velBase.Children().ElementAt(0);
                vel1.style.backgroundColor = isSelected ? greenSel : green;

                var vel2 = velBase.Children().ElementAt(1);
                vel2.style.backgroundColor = isSelected ? redSel : red;
            }
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
            var inAsset = new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset };
            bool resultFirst = FlowController.Engine.Transition(FlowController.User, FlowController.PREPARATION, inAsset, out IAsset[] _, (byte)_selectedBoonOrBane);

            if (!resultFirst)
            {
                Debug.LogWarning("Wasn't successfull in executing the ExtrinsicPreparation!"); 
                return;
            }

            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Battle);
        }
    }
}