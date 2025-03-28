﻿using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class PlayState : ScreenBaseState
    {
        internal VisualTreeAsset VelCard { get; }
        internal VisualTreeAsset VelPlayer { get; }

        internal Sprite[] SprDeck { get; }

        internal VisualTreeAsset VtrBtnAction { get; }

        private VisualElement _topBound;
        private VisualElement _bottomBound;
        private VisualElement _velActionButtons;

        private Label _txtLevel;
        private Label _txtRound;
        private Label _txtToken;

        internal GameAsset GameAsset { get; private set; }
        internal DeckAsset DeckAsset { get; private set; }
        internal TowerAsset TowrAsset { get; private set; }

        public Player CurrentPlayer { get; internal set; }
        public TemplateContainer VelCurrentPlayer { get; internal set; }

        public Player CurrentOpponent { get; internal set; }
        public TemplateContainer VelCurrentOpponent { get; internal set; }

        public PlayState(FlowController _flowController)
            : base(_flowController)
        {
            VelCard = Resources.Load<VisualTreeAsset>("UI/Elements/VelCard");
            VelPlayer = Resources.Load<VisualTreeAsset>("UI/Elements/VelPlayer");

            SprDeck = Resources.LoadAll<Sprite>("Textures/Deck05");

            VtrBtnAction = Resources.Load<VisualTreeAsset>("UI/Elements/BtnAction");
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/PlayUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _topBound = instance.Q<VisualElement>("TopBound");
            var velLevel = _topBound.Q<VisualElement>("VelLevel");
            _txtLevel = velLevel.Q<Label>("TxtLevel");

            var velRound = _topBound.Q<VisualElement>("VelRound");
            _txtRound = velRound.Q<Label>("TxtRound");

            var velToken = _topBound.Q<VisualElement>("VelToken");
            _txtToken = velToken.Q<Label>("TxtToken");

            _bottomBound = instance.Q<VisualElement>("BottomBound");
            _velActionButtons = _bottomBound.Q<VisualElement>("VelActionButtons");

            // add container
            FlowController.VelContainer.Add(instance);

            FlowController.ChangeScreenSubState(ScreenState.Play, ScreenSubState.Preparation);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");
        }

        public void SetToken(string token)
        {
            _txtToken.text = token;
        }

        public void SetLevel(string level)
        {
            _txtLevel.text = level;
        }

        public void SetRound(string round)
        {
            _txtRound.text = round;
        }

        internal void LoadAssets()
        {
            GameAsset = FlowController.GetAsset<GameAsset>(FlowController.User, AssetType.Game, AssetSubType.None);
            DeckAsset = FlowController.GetAsset<DeckAsset>(FlowController.User, AssetType.Deck, AssetSubType.None);
            TowrAsset = FlowController.GetAsset<TowerAsset>(FlowController.User, AssetType.Tower, AssetSubType.None);

            if (GameAsset != null)
            {
                SetToken(GameAsset.Token.ToString());
                SetLevel(GameAsset.Level.ToString());
                SetRound(GameAsset.Round.ToString());
            }
            else
            {
                SetToken("-");
                SetLevel("-");
                SetRound("-");
            }

        }

        internal void AddFrameButtons(Button[] frameButtons)
        {
            _velActionButtons.Clear();
            foreach (var button in frameButtons)
            {
                _velActionButtons.Add(button);
            }
        }
    }
}