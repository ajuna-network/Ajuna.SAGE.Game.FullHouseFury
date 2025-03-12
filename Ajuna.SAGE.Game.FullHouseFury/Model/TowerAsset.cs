using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{

    /// <summary>
    /// Tower asset class for the FullHouseFury game.
    /// </summary>
    public partial class TowerAsset : BaseAsset
    {

        public TowerAsset(uint ownerId, uint genesis)
            : base(ownerId, genesis)
        {
            AssetType = AssetType.Tower;
        }

        public TowerAsset(IAsset asset)
            : base(asset)
        { }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ XXXX.... ........
        public uint SingleBoons
        {
            get => BitConverter.ToUInt32(Data.Read(16, 4), 0);
            set => Data.Set(16, BitConverter.GetBytes(value));
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ....XXXX ........
        public uint MultiBoons
        {
            get => BitConverter.ToUInt32(Data.Read(20, 4), 0);
            set => Data.Set(20, BitConverter.GetBytes(value));
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ XXXX....
        public uint SingleBanes
        {
            get => BitConverter.ToUInt32(Data.Read(24, 4), 0);
            set => Data.Set(24, BitConverter.GetBytes(value));
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// ........ ........ ........ ....XXXX
        public uint MultiBanes
        {
            get => BitConverter.ToUInt32(Data.Read(28, 4), 0);
            set => Data.Set(28, BitConverter.GetBytes(value));
        }
    }

    public partial class TowerAsset
    {
        public void New()
        {
            SingleBoons = 0;
            MultiBoons = 0;
            SingleBanes = 0;
            MultiBanes = 0;
        }

        public void SetBoon(byte boonIndex, byte value)
        {
            if (boonIndex < 32)
            {
                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException("Boon on index 0 - 31 value must be 0 or 1");
                }
                SingleBoons = (uint)((SingleBoons & ~(1U << boonIndex)) | ((uint)value << boonIndex));
            }
            else if (boonIndex < 48)
            {
                if (value > 3)
                {
                    throw new ArgumentOutOfRangeException("Boon on index 32 - 47 value must be 0, 1, 2 or 3");
                }
                int offset = boonIndex - 32;
                MultiBoons = (uint)((MultiBoons & ~(3U << offset)) | ((uint)value << offset));
            }
        }

        public byte[] GetAllBoons()
        {
            byte[] boons = new byte[48];

            for (int i = 0; i < 32; i++)
            {
                boons[i] = (byte)((SingleBoons >> i) & 1);
            }

            for (int i = 0; i < 16; i++)
            {
                boons[i + 32] = (byte)((MultiBoons >> i) & 3);
            }

            return boons;
        }

        public void SetBanes(byte index, byte value)
        {
            if (index < 32)
            {
                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException("Boon on index 0 - 31 value must be 0 or 1");
                }
                SingleBanes = (uint)((SingleBanes & ~(1U << index)) | ((uint)value << index));
            }
            else if (index < 48)
            {
                if (value > 3)
                {
                    throw new ArgumentOutOfRangeException("Boon on index 32 - 47 value must be 0, 1, 2 or 3");
                }
                int offset = index - 32;
                MultiBanes = (uint)((MultiBanes & ~(3U << offset)) | ((uint)value << offset));
            }
        }

        public byte[] GetAllBanes()
        {
            byte[] banes = new byte[48];

            for (int i = 0; i < 32; i++)
            {
                // Retrieve single boons as a 0 or 1 value.
                banes[i] = (byte)((SingleBanes >> i) & 1);
            }

            for (int i = 0; i < 16; i++)
            {
                // Retrieve multi boons (indexes 32 to 47).
                banes[i + 32] = (byte)((MultiBanes >> i) & 3);
            }

            return banes;
        }
    }
}
