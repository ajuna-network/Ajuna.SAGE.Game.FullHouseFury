using Ajuna.SAGE.Game.FullHouseFury.Model;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public enum HandCardState
    {
        InHand,
        InDiscard,
        InPlay
    }

    public class HandCard
    {
        public int Index { get; private set; }

        public Card Card { get; private set; }

        public HandCardState HandCardState { get; private set;}

        public VisualElement VisualElement { get; set; }

        public HandCard(int index, Card card) 
        {
            Index = index;
            Card = card;
            HandCardState = HandCardState.InHand;
        }

        public void SetState(HandCardState state)
        {
            HandCardState = state;
        }
    }
}