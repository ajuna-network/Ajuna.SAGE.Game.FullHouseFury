using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{

    /// <summary>
    /// Game asset class for the FullHouseFury game.
    /// </summary>
    public partial class GameAsset : BaseAsset
    {
        public const int HAND_OFFSET = 16;
        public const int HAND_REGION_SIZE = 4;

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
        public ushort MaxHealth
        {
            get => Data.ReadValue<ushort>(9);
            set => Data?.SetValue<ushort>(9, value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ...XX... ........ ........
        public ushort Health
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
        /// ........ ........ XXXX.... ........
        public uint AttackHand
        {
            get
            {
                byte[] handBytes = Data.Read(HAND_OFFSET, HAND_REGION_SIZE);
                return BitConverter.ToUInt32(handBytes, 0);
            }
            set
            {
                byte[] handBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < HAND_REGION_SIZE; i++)
                {
                    Data.Set((byte)(HAND_OFFSET + i), ByteType.Full, handBytes[i]);
                }
            }
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ....H... ........
        public PokerHand AttackType
        {
            get => (PokerHand)Data.Read(20, ByteType.High);
            set => Data?.Set(20, ByteType.High, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ .....XX. ........
        public ushort AttackScore
        {
            get => Data.ReadValue<ushort>(21);
            set => Data.SetValue<ushort>(21, value);
        }
    }

    public partial class GameAsset
    {
        public bool IsBossAlive => Health > 0;

        public void NewGame()
        {
            GameState = GameState.Running;
            LevelState = LevelState.Preparation;
            Level = 1;
            Round = 1;
            MaxHealth = 100;
            Health = MaxHealth;
            Discard = 3;
            HandSize = 7;

            ClearAttackHand();
        }

        public byte GetAttackHandCard(int handPosition)
        {
            if (handPosition < 0 || handPosition >= 4)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 4.");
            }

            uint handValue = AttackHand;
            int bitOffset = handPosition * 6;
            return (byte)((handValue >> bitOffset) & 0x3F);
        }

        public void SetAttackHandCard(int handPosition, byte cardIndex)
        {
            if (handPosition < 0 || handPosition >= 4)
            {
                throw new ArgumentOutOfRangeException(nameof(handPosition), "Hand position must be between 0 and 4.");
            }

            if (cardIndex > 51)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index must be between 0 and 51.");
            }

            uint handValue = AttackHand;
            int bitOffset = handPosition * 6;
            uint mask = 0x3FU << bitOffset;
            handValue = (handValue & ~mask) | (((uint)cardIndex & 0x3F) << bitOffset);
            AttackHand = handValue;
        }

        public void ClearAttackHand()
        {
            uint empty = 0;
            for (int i = 0; i < 5; i++)
            {
                int bitOffset = i * 6;
                empty |= ((uint)DeckAsset.EMPTY_SLOT & 0x3F) << bitOffset;
            }
            AttackHand = empty;
        }
    }
}
