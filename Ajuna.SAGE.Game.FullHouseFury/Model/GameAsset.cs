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
        /// .......H ........ ........ ........
        public GameState GameState
        {
            get => (GameState)Data.Read(7, ByteType.High);
            set => Data?.Set(7, ByteType.High, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// .......L ........ ........ ........
        public LevelState LevelState
        {
            get => (LevelState)Data.Read(7, ByteType.Low);
            set => Data?.Set(7, ByteType.Low, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ X....... ........ ........
        public byte Level
        {
            get => Data.Read(8, ByteType.Full);
            set => Data?.Set(8, ByteType.Full, value);
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
        /// ........ .....X.. ........ ........
        public byte Discard
        {
            get => Data.Read(13, ByteType.Full);
            set => Data?.Set(13, ByteType.Full, value);
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
            MaxHealth = 100;
            Health = MaxHealth;
            Discard = 3;
        }

    }
}
