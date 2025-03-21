using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayShopSubState : ScreenBaseState
    {
        private VisualTreeAsset _vtrUpgrade;

        public PlayState PlayState => ParentState as PlayState;

        public PlayShopSubState(FlowController flowController, ScreenBaseState parent)
            : base(flowController, parent)
        {
            _vtrUpgrade = Resources.Load<VisualTreeAsset>("UI/Elements/VelUpgrade");
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            // make sure we load the assets first
            PlayState.LoadAssets();

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer elementInstance = ElementInstance("UI/Frames/ShopFrame");

            // add your stuff here
            var velRarityUpgrades = elementInstance.Q<VisualElement>("VelRarityUpgrades");
            var velRarityUpgradesCont = velRarityUpgrades.Q<VisualElement>("VelValues");
            CreateRarityUpgrades(velRarityUpgradesCont);

            var frameButtons = new Button[] {
                ButtonAction("BACK", PlayState.VtrBtnAction),
                ButtonAction("BUY", PlayState.VtrBtnAction)
            };
            frameButtons[0].RegisterCallback<ClickEvent>(evt => FramePreparation());
            frameButtons[1].RegisterCallback<ClickEvent>(evt => ExtrinsicShop());
            PlayState.AddFrameButtons(frameButtons);

            floatBody.Add(elementInstance);

        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

        }

        private void CreateRarityUpgrades(VisualElement velContainer)
        {
            foreach (RarityType rarity in Enum.GetValues(typeof(RarityType)))
            { 
                if(rarity == RarityType.Common || rarity == RarityType.Mythical)
                {
                    continue;
                }

                var templateCont = _vtrUpgrade.Instantiate();
                var txtValueDesc = templateCont.Q<Label>("TxtValueDesc");
                txtValueDesc.text = rarity.ToString().Length > 6 ? rarity.ToString().Substring(0, 5) + "." : rarity.ToString();
                
                var txtValue = templateCont.Q<Label>("TxtLevel");
                var currentLevel = PlayState.DeckAsset.GetRarity(rarity);
                var maxLevel = DeckAsset.MAX_RARITY_LEVEL;
                txtValue.text = $"{currentLevel}\n/\n{maxLevel}";

                var pipe = templateCont.Q<VisualElement>("VelPipeFill");
                pipe.style.height = new Length((currentLevel / (float)maxLevel) * 100, LengthUnit.Percent);

                var isUpgradable = FullHouseFuryUtil.UpgradeInfo(FeatureType.RarityLevel, (byte)rarity, (byte) (currentLevel  + 1), new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset }, out byte price);
                var txtPrice = templateCont.Q<Label>("TxtPrice");
                txtPrice.text = price.ToString();

                velContainer.SetEnabled(isUpgradable);
                velContainer.Add(templateCont);
            }
            
        }

        private void FramePreparation()
        {
            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Preparation);
        }

        private void ExtrinsicShop()
        {
            
        }
    }
}