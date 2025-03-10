﻿using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class PlayState : ScreenBaseState
    {
        internal VisualTreeAsset VelCard { get; }
        internal Sprite[] SprDeck { get; }

        internal VisualTreeAsset VtrBtnAction { get; }

        private VisualElement _bottomBound;
        private VisualElement _velActionButtons;

        internal GameAsset GameAsset { get; private set; }

        internal DeckAsset DeckAsset { get; private set; }

        public PlayState(FlowController _flowController)
            : base(_flowController)
        {
            VelCard = Resources.Load<VisualTreeAsset>("UI/Elements/VelCard");
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

        private void OnClickBtnAttack(ClickEvent evt)
        {
            Debug.Log($"Clicked on Attack!");
        }

        internal void LoadAssets()
        {
            GameAsset = FlowController.GetAsset<GameAsset>(FlowController.User, AssetType.Game, AssetSubType.None);
            DeckAsset = FlowController.GetAsset<DeckAsset>(FlowController.User, AssetType.Deck, AssetSubType.None);
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