using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Linq;
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
        None,
        Preparation,
        Battle,
        Score,
        Shop,
    }

    public class FlowController : MonoBehaviour
    {
        public readonly FullHouseFuryIdentifier START = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Start);
        public readonly FullHouseFuryIdentifier PLAY = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Play);
        public readonly FullHouseFuryIdentifier PREPARATION = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Preparation);
        public readonly FullHouseFuryIdentifier BATTLE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Battle);
        public readonly FullHouseFuryIdentifier DISCARD = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Discard);
        public readonly FullHouseFuryIdentifier SCORE = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Score);
        public readonly FullHouseFuryIdentifier SHOP = FullHouseFuryIdentifier.Create(FullHouseFuryAction.Shop);
        
        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public Vector2 ScrollOffset { get; set; }

        public CacheData CacheData { get; private set; }

        public VisualElement VelContainer { get; private set; }

        public ScreenState CurrentState { get; private set; }

        public ScreenSubState CurrentSubState { get; private set; }

        private ScreenBaseState _currentState;
        private ScreenBaseState _currentSubState;
        private readonly Dictionary<ScreenState, ScreenBaseState> _stateDictionary = new();
        private readonly Dictionary<ScreenState, Dictionary<ScreenSubState, ScreenBaseState>> _subStateDictionary = new();

        /// <summary>
        /// The blockchain info provider
        /// </summary>
        public IBlockchainInfoProvider BlockchainInfoProvider { get; private set; }

        /// <summary>
        /// The game engine
        /// </summary>
        public Engine<FullHouseFuryIdentifier, FullHouseFuryRule> Engine { get; private set; }

        /// <summary>
        /// The user account
        /// </summary>
        public IAccount User { get; private set; }

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
                { ScreenSubState.Preparation, new PlayPreparationSubState(this, playState) },
                { ScreenSubState.Battle, new PlayBattleSubState(this, playState) },
                { ScreenSubState.Score, new PlayScoreSubState(this, playState) },
                { ScreenSubState.Shop, new PlayShopSubState(this, playState) },
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

            // initialize game engine
            var randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
            Debug.Log($"Random Seed: {randomSeed}");
            BlockchainInfoProvider = new BlockchainInfoProvider(randomSeed);
            Engine = FullHouseFuryGame.Create(BlockchainInfoProvider);
            User = Engine.AccountManager.Account(Engine.AccountManager.Create());
            User.Balance.Deposit(1_000_000);

            // update block number
            InvokeRepeating(nameof(UpdatedBlocknumber), 0f, 6f);

            // call insital flow state
            ChangeScreenState(ScreenState.Welcome);
        }

        private void UpdatedBlocknumber()
        {
            BlockchainInfoProvider.CurrentBlockNumber++;
            Debug.Log($"Blocknumber: {BlockchainInfoProvider.CurrentBlockNumber}");
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
            CurrentSubState = ScreenSubState.None;

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
                CurrentSubState = newSubState;

                // exit current sub state if any
                _currentSubState?.ExitState();

                // change the sub state
                _currentSubState = _subStateDictionary[parentState][newSubState];

                // enter current sub state
                _currentSubState.EnterState();
            }
            else
            {
                Debug.LogWarning($"Substate {newSubState} not found for state {parentState}");
            }
        }

        /// <summary>
        /// Get the asset of the user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        /// <returns></returns>
        public T GetAsset<T>(IAccount user, AssetType type, AssetSubType subType) where T : BaseAsset
        {
            BaseAsset? result = Engine.AssetManager
                .AssetOf(user)
                .Select(p => (BaseAsset)p)
                .Where(p => p.AssetType == type && p.AssetSubType == subType)
                .FirstOrDefault();
            var typedResult = result as T;
            return typedResult;
        }
    }
}