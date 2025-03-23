using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class PlayShopSubState : ScreenBaseState
    {
        private VisualTreeAsset _vtrUpgrade;

        private PokerHand _selectedPokerHand;

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

            _selectedPokerHand = PokerHand.Pair;

            // add your stuff here
            var velRarityUpgrades = elementInstance.Q<VisualElement>("VelRarityUpgrades");
            var velRarityUpgradesCont = velRarityUpgrades.Q<VisualElement>("VelValues");
            CreateRarityUpgrades(velRarityUpgradesCont);

            var velHandUpgrades = elementInstance.Q<VisualElement>("VelHandUpgrades");
            var velHandUpgradesCont = velHandUpgrades.Q<VisualElement>("VelValues");
            CreateHandUpgrades(velHandUpgradesCont);

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
            foreach (RarityType enumValue in Enum.GetValues(typeof(RarityType)))
            { 
                if(enumValue == RarityType.Common || enumValue == RarityType.Mythical)
                {
                    continue;
                }

                var templateCont = _vtrUpgrade.Instantiate();
                var txtValueDesc = templateCont.Q<Label>("TxtValueDesc");
                txtValueDesc.text = enumValue.ToString().Length > 6 ? enumValue.ToString().Substring(0, 5) + "." : enumValue.ToString();
                
                var txtValue = templateCont.Q<Label>("TxtLevel");
                var currentLevel = PlayState.DeckAsset.GetRarity(enumValue);
                var maxLevel = DeckAsset.MAX_RARITY_LEVEL;
                txtValue.text = $"{currentLevel}\n/\n{maxLevel}";

                var pipe = templateCont.Q<VisualElement>("VelPipeFill");
                pipe.style.height = new Length((currentLevel / (float)maxLevel) * 100, LengthUnit.Percent);

                var isUpgradable = FullHouseFuryUtil.UpgradeInfo(FeatureType.RarityLevel, (byte)enumValue, (byte) (currentLevel  + 1), new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset }, out byte price);
                var txtPrice = templateCont.Q<Label>("TxtPrice");
                txtPrice.text = price.ToString();

                velContainer.SetEnabled(isUpgradable);
                velContainer.Add(templateCont);
            }
            
        }

        private void CreateHandUpgrades(VisualElement velContainer)
        {
            var leftArrow = velContainer.Q<VisualElement>("VelLeftArrow");
            leftArrow.RegisterCallback<ClickEvent>(evt => {
                var currentIndex = (int)_selectedPokerHand;
                _selectedPokerHand = (PokerHand)(currentIndex <= 1 ? 10 : currentIndex - 1);
                LoadPokerHand(velContainer);
            });
            var rightArrow = velContainer.Q<VisualElement>("VelRightArrow");
            rightArrow.RegisterCallback<ClickEvent>(evt => {
                var currentIndex = (int)_selectedPokerHand;
                _selectedPokerHand = (PokerHand)(currentIndex >= 10 ? 1 : currentIndex + 1);
                LoadPokerHand(velContainer);
            });

            LoadPokerHand(velContainer);
        }

        private void LoadPokerHand(VisualElement velContainer)
        {
            var txtPokerHand = velContainer.Q<Label>("TxtValueDesc");
            txtPokerHand.text = _selectedPokerHand.ToString();

            var txtValue = velContainer.Q<Label>("TxtLevel");
            var currentLevel = PlayState.DeckAsset.GetPokerHandLevel(_selectedPokerHand);
            var maxLevel = DeckAsset.MAX_POKERHAND_LEVEL;
            txtValue.text = $"{currentLevel}\n/\n{maxLevel}";

            var pipe = velContainer.Q<VisualElement>("VelPipeFill");
            pipe.style.height = new Length((currentLevel / (float)maxLevel) * 100, LengthUnit.Percent);

            var isUpgradable = FullHouseFuryUtil.UpgradeInfo(FeatureType.PokerHandLevel, (byte)_selectedPokerHand, (byte)(currentLevel + 1), new IAsset[] { PlayState.GameAsset, PlayState.DeckAsset, PlayState.TowrAsset }, out byte price);
            var txtPrice = velContainer.Q<Label>("TxtPrice");
            txtPrice.text = price.ToString();

            var handContainer = velContainer.Q<VisualElement>("VelHandCont");

            handContainer.Clear();
            switch (_selectedPokerHand)
            {
                case PokerHand.HighCard:
                    handContainer.Add(GetCard(10, 1));
                    break;
                case PokerHand.Pair:
                    handContainer.Add(GetCard(5, 1));
                    handContainer.Add(GetCard(18, 1));
                    break;
                case PokerHand.TwoPair:
                    handContainer.Add(GetCard(6, 1));
                    handContainer.Add(GetCard(19, 1));
                    handContainer.Add(GetCard(21, 1));
                    handContainer.Add(GetCard(34, 1));
                    break;
                case PokerHand.ThreeOfAKind:
                    handContainer.Add(GetCard(7, 1));
                    handContainer.Add(GetCard(20, 1));
                    handContainer.Add(GetCard(33, 1));
                    break;
                case PokerHand.Straight:
                    handContainer.Add(GetCard(30, 1));
                    handContainer.Add(GetCard(18, 1));
                    handContainer.Add(GetCard(45, 1));
                    handContainer.Add(GetCard(33, 1));
                    handContainer.Add(GetCard(8, 1));
                    break;
                case PokerHand.Flush:
                    handContainer.Add(GetCard(28, 1));
                    handContainer.Add(GetCard(30, 1));
                    handContainer.Add(GetCard(31, 1));
                    handContainer.Add(GetCard(33, 1));
                    handContainer.Add(GetCard(34, 1));
                    break;
                case PokerHand.FullHouse:
                    handContainer.Add(GetCard(13, 1));
                    handContainer.Add(GetCard(26, 1));
                    handContainer.Add(GetCard(39, 1));
                    handContainer.Add(GetCard(5, 1));
                    handContainer.Add(GetCard(18, 1));
                    break;
                case PokerHand.FourOfAKind:
                    handContainer.Add(GetCard(8, 1));
                    handContainer.Add(GetCard(21, 1));
                    handContainer.Add(GetCard(34, 1));
                    handContainer.Add(GetCard(47, 1));
                    break;
                case PokerHand.StraightFlush:
                    handContainer.Add(GetCard(18, 1));
                    handContainer.Add(GetCard(19, 1));
                    handContainer.Add(GetCard(20, 1));
                    handContainer.Add(GetCard(21, 1));
                    handContainer.Add(GetCard(22, 1));
                    break;
                case PokerHand.RoyalFlush:
                    handContainer.Add(GetCard(48, 1));
                    handContainer.Add(GetCard(49, 1));
                    handContainer.Add(GetCard(50, 1));
                    handContainer.Add(GetCard(51, 1));
                    handContainer.Add(GetCard(39, 1));
                    break;
            }

        }

        private TemplateContainer GetCard(byte cardIndex, byte rarity)
        {
            var card = new Card(cardIndex, rarity);
             var templateContainer = PlayState.VelCard.Instantiate();
            var velCard = templateContainer.Q<VisualElement>("VelCard");
            var velGlow = templateContainer.Q<VisualElement>("VelGlow");
            var txtCard = templateContainer.Q<Label>("TxtMultiplier");
            txtCard.text = ((int)card.Rarity).ToString();
            velGlow.style.backgroundColor = new StyleColor(HelperUtil.GetRarityColor(card.Rarity));
            velCard.style.backgroundImage = new StyleBackground(PlayState.SprDeck.FirstOrDefault(s => s.name ==
                HelperUtil.GetCardSpritName(card.Suit, card.Rank)));
            return templateContainer;
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