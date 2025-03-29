using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;
using System.Net.Sockets;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{

    /// <summary>
    /// Game asset class for the FullHouseFury game.
    /// </summary>
    public partial class GameAsset : BaseAsset
    {
        public GameAsset(uint ownerId, uint genesis)
            : base(ownerId, genesis)
        {
            AssetType = AssetType.Game;
            GameState = GameState.None;
            LevelState = LevelState.None;
        }

        public GameAsset(IAsset asset)
            : base(asset) { }

        public GameState GameState
        {
            get => (GameState)Data.Read<byte>(1);
            set => Data.Set<byte>(1, (byte)value);
        }

        public LevelState LevelState
        {
            get => (LevelState)Data.Read<byte>(2);
            set => Data.Set<byte>(2, (byte)value);
        }

        public byte Token
        {
            get => Data.Read<byte>(3);
            set => Data.Set<byte>(3, value);
        }
        public byte Level
        {
            get => Data.Read<byte>(4);
            set => Data.Set<byte>(4, value);
        }

        public byte Round
        {
            get => Data.Read<byte>(5);
            set => Data.Set<byte>(5, value);
        }

        public byte BossType
        {
            get => Data.Read<byte>(6);
            set => Data.Set<byte>(6, value);
        }

        public ushort MaxBossHealth
        {
            get => Data.Read<ushort>(7);
            set => Data.Set<ushort>(7, value);
        }

        public ushort BossDamage
        {
            get => Data.Read<ushort>(9);
            set => Data?.Set<ushort>(9, value);
        }

        public byte Discard
        {
            get => Data.Read<byte>(11);
            set => Data.Set<byte>(11, value);
        }

        public byte HandSize
        {
            get => Data.Read<byte>(12);
            set => Data.Set<byte>(12, value);
        }

        public PokerHand AttackType
        {
            get => (PokerHand)Data.Read<byte>(13);
            set => Data.Set<byte>(13, (byte)value);
        }

        public ushort AttackScore
        {
            get => Data.Read<ushort>(14);
            set => Data.Set<ushort>(14, value);
        }

        private byte[] AttackHand
        {
            get => Data.Read(16, 5);
            set => Data.Set(16, value);
        }

        public byte MaxPlayerEndurance
        {
            get => Data.Read<byte>(21);
            set => Data.Set<byte>(21, value);
        }

        public byte PlayerEndurance
        {
            get => Data.Read<byte>(22);
            set => Data.Set<byte>(22, value);
        }

        public ushort MaxPlayerHealth
        {
            get => Data.Read<ushort>(23);
            set => Data.Set<ushort>(23, value);
        }

        public ushort PlayerDamage
        {
            get => Data.Read<ushort>(25);
            set => Data?.Set<ushort>(25, value);
        }

        public ushort FatigueDamage
        {
            get => Data.Read<ushort>(27);
            set => Data?.Set<ushort>(27, value);
        }
    }

    public partial class GameAsset
    {
        public int BossHealth => MaxBossHealth - BossDamage;

        public bool IsBossAlive => BossHealth > 0;

        public int PlayerHealth => MaxPlayerHealth - PlayerDamage;

        public bool IsPlayerAlive => PlayerHealth > 0;

        public void New()
        {
            GameState = GameState.Running;
            LevelState = LevelState.Preparation;
            Token = 0;
            Level = 1;
            Round = 0;
            MaxBossHealth = 100;
            BossDamage = 0;

            // Player stats
            MaxPlayerEndurance = 10;
            PlayerEndurance = MaxPlayerEndurance;
            Discard = 3;
            HandSize = 7;
            MaxPlayerHealth = 100;
            PlayerDamage = 0;
            FatigueDamage = 1;

            ClearAttack();
        }

        /// <summary>
        /// Get the card at the specified position in the attack hand.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void GetAttackHand(int position, out byte cardIndex, out byte rarity)
        {
            if (position < 0 || position > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 4.");
            }

            FullHouseFuryUtil.DecodeCardByte(AttackHand[position], out cardIndex, out rarity);
        }

        /// <summary>
        /// Set the card at the specified position in the attack hand.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cardIndex"></param>
        /// <param name="rarity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetAttackHand(int position, byte cardIndex, byte rarity)
        {
            if (cardIndex > 51)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index must be between 0 and 51.");
            }

            SetAttackHand(position, FullHouseFuryUtil.EncodeCardByte(cardIndex, rarity));
        }

        public void SetAttackHand(int position, byte encodedCard)
        {
            if (position < 0 || position > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 4.");
            }

            var hand = AttackHand;
            hand[position] = encodedCard;
            AttackHand = hand;
        }

        public void ClearAttack()
        {
            AttackScore = 0;
            AttackType = PokerHand.None;

            for (int i = 0; i < 5; i++)
            {
                SetAttackHand(i, FullHouseFuryUtil.EncodeCardByte(DeckAsset.EMPTY_SLOT, 0));
            }
        }
    }

}
