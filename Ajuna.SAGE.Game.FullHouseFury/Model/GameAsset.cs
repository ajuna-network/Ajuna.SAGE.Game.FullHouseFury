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
            : base(asset)
        { }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ....X... ........ ........ ........
        public byte Token
        {
            get => Data.Read(4, ByteType.Full);
            set => Data?.Set(4, ByteType.Full, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .....H.. ........ ........ ........
        public GameState GameState
        {
            get => (GameState)Data.Read(5, ByteType.High);
            set => Data?.Set(5, ByteType.High, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .....L.. ........ ........ ........
        public LevelState LevelState
        {
            get => (LevelState)Data.Read(5, ByteType.Low);
            set => Data?.Set(5, ByteType.Low, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ......X. ........ ........ ........
        public byte Level
        {
            get => Data.Read(6, ByteType.Full);
            set => Data?.Set(6, ByteType.Full, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .......X ........ ........ ........
        public byte Round
        {
            get => Data.Read(7, ByteType.Full);
            set => Data?.Set(7, ByteType.Full, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ .XX..... ........ ........
        public ushort MaxBossHealth
        {
            get => Data.ReadValue<ushort>(9);
            set => Data?.SetValue<ushort>(9, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ...XX... ........ ........
        public ushort BossDamage
        {
            get => Data.ReadValue<ushort>(11);
            set => Data?.SetValue<ushort>(11, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ .....H.. ........ ........
        public byte Discard
        {
            get => Data.Read(13, ByteType.High);
            set => Data?.Set(13, ByteType.High, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ .....L.. ........ ........
        public byte HandSize
        {
            get => Data.Read(13, ByteType.Low);
            set => Data?.Set(13, ByteType.Low, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ XXXXX... ........
        private byte[] AttackHand
        {
            get => Data.Read(16, 5);
            set => Data.Set(16, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ .....H.. ........
        public PokerHand AttackType
        {
            get => (PokerHand)Data.Read(21, ByteType.High);
            set => Data?.Set(21, ByteType.High, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ......XX ........
        public ushort AttackScore
        {
            get => Data.ReadValue<ushort>(22);
            set => Data.SetValue<ushort>(22, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ .H......
        public byte MaxPlayerEndurance
        {
            get => Data.Read(25, ByteType.High);
            set => Data?.Set(25, ByteType.High, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ .L......
        public byte PlayerEndurance
        {
            get => Data.Read(25, ByteType.Low);
            set => Data?.Set(25, ByteType.Low, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ ..XX....
        public ushort MaxPlayerHealth
        {
            get => Data.ReadValue<ushort>(26);
            set => Data?.SetValue<ushort>(26, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ ....XX..
        public ushort PlayerDamage
        {
            get => Data.ReadValue<ushort>(28);
            set => Data?.SetValue<ushort>(28, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ ......XX
        public ushort FatigueDamage
        {
            get => Data.ReadValue<ushort>(30);
            set => Data?.SetValue<ushort>(30, value);
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
