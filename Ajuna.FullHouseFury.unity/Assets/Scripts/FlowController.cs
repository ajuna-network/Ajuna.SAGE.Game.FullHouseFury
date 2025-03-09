using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public enum ScreenState
    {
        Welcome,
        Menu,
        Play,
    }

    public enum ScreenSubState
    {
        Dashboard,
    }

    public class FlowController : MonoBehaviour
    {
        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public Vector2 ScrollOffset { get; set; }

        public CacheData CacheData { get; private set; }

        public VisualElement VelContainer { get; private set; }

        public ScreenState CurrentState { get; private set; }

        private ScreenBaseState _currentState;
        private ScreenBaseState _currentSubState;
        private readonly Dictionary<ScreenState, ScreenBaseState> _stateDictionary = new();
        private readonly Dictionary<ScreenState, Dictionary<ScreenSubState, ScreenBaseState>> _subStateDictionary = new();

        private void Awake()
        {
            CacheData = new CacheData();

            // Initialize states
            _stateDictionary.Add(ScreenState.Welcome, new WelcomeState(this));
            _stateDictionary.Add(ScreenState.Menu, new MenuState(this));

            var playState = new PlayState(this);
            _stateDictionary.Add(ScreenState.Play, playState);

            var mainScreenSubStates = new Dictionary<ScreenSubState, ScreenBaseState>
            {
                { ScreenSubState.Dashboard, new MainDashboardSubState(this, playState) },
            };

            _subStateDictionary.Add(ScreenState.Play, mainScreenSubStates);
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            VelContainer = root.Q<VisualElement>("VelContainer");
            var label = VelContainer.Q<Label>("LblAssetProgress");

            if (VelContainer.childCount > 1)
            {
                Debug.Log("Plaese remove development work, before starting!");
                return;
            }

            // call insital flow state
            ChangeScreenState(ScreenState.Welcome);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Change the screen state
        /// </summary>
        /// <param name="newScreenState"></param>
        internal void ChangeScreenState(ScreenState newScreenState)
        {
            CurrentState = newScreenState;

            // exit current state if any
            _currentState?.ExitState();

            // change the state
            _currentState = _stateDictionary[newScreenState];

            // exit any active sub-state when changing the main state
            _currentSubState?.ExitState();
            _currentSubState = null;

            // enter current state
            _currentState.EnterState();
        }

        /// <summary>
        /// Change the sub state of the current screen state
        /// </summary>
        /// <param name="parentState"></param>
        /// <param name="newSubState"></param>
        internal void ChangeScreenSubState(ScreenState parentState, ScreenSubState newSubState)
        {
            if (_subStateDictionary.ContainsKey(parentState) && _subStateDictionary[parentState].ContainsKey(newSubState))
            {
                // exit current sub state if any
                _currentSubState?.ExitState();

                // change the sub state
                _currentSubState = _subStateDictionary[parentState][newSubState];

                // enter current sub state
                _currentSubState.EnterState();
            }
        }
    }
}