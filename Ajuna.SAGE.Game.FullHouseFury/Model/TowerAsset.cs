﻿using Ajuna.SAGE.Core.Model;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
            : base(asset) { }

        public ulong TowerLevel
        {
            get => Data.Read<ulong>(4);
            set => Data.Set<ulong>(4, value);
        }

        public uint BoonsAndBanes
        {
            get => Data.Read<uint>(12);
            set => Data.Set<uint>(12, value);
        }

        public uint SingleBoons
        {
            get => Data.Read<uint>(16);
            set => Data.Set<uint>(16, value);
        }

        public uint MultiBoons
        {
            get => Data.Read<uint>(20);
            set => Data.Set<uint>(20, value);
        }

        public uint SingleBanes
        {
            get => Data.Read<uint>(24);
            set => Data.Set<uint>(24, value);
        }

        public uint MultiBanes
        {
            get => Data.Read<uint>(28);
            set => Data.Set<uint>(28, value);
        }
    }

    public partial class TowerAsset
    {
        public void New()
        {
            ClearChoices();

            SingleBoons = 0;
            MultiBoons = 0;
            SingleBanes = 0;
            MultiBanes = 0;
        }

        public void ClearChoices()
        {
            SetBoonAndBane(0, BonusType.None, MalusType.None);
            SetBoonAndBane(1, BonusType.None, MalusType.None);
            SetBoonAndBane(2, BonusType.None, MalusType.None);
        }

        public (BonusType boon, MalusType bane) GetBoonAndBane(int position)
        {
            if (position < 0 || position > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Boons and banes position must be between 0 and <3.");
            }

            uint boonsAndBanes = BoonsAndBanes;
            int bitOffset = (position * 2) * 6;

            var boon = (BonusType)((boonsAndBanes >> bitOffset) & 0x3F);
            var bane = (MalusType)((boonsAndBanes >> (bitOffset + 6)) & 0x3F);

            return (boon, bane);
        }

        public void SetBoonAndBane(int position, BonusType boon, MalusType bane)
        {
            if (position < 0 || position > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Boons and banes position must be between 0 and <3.");
            }

            uint boonsAndBanes = BoonsAndBanes;
            int bitOffset = (position * 2) * 6;

            // Clear existing boon and bane values at the position
            uint boonMask = 0x3FU << bitOffset;
            uint baneMask = 0x3FU << (bitOffset + 6);

            boonsAndBanes &= ~(boonMask | baneMask); // Clear bits

            // Set new boon and bane values
            boonsAndBanes |= ((uint)boon & 0x3F) << bitOffset;
            boonsAndBanes |= ((uint)bane & 0x3F) << (bitOffset + 6);

            BoonsAndBanes = boonsAndBanes;
        }

        public byte GetBoon(byte index, out bool isMaxed) 
            => GetAttribute(SingleBoons, MultiBoons, index, out isMaxed);

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

        public byte GetBane(byte index, out bool isMaxed) 
            => GetAttribute(SingleBanes, MultiBanes, index, out isMaxed);

        public void SetBane(byte index, byte value)
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
    public partial class TowerAsset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetAttribute(uint singleValue, uint multiValue, byte index, out bool isMaxed)
        {
            if (index < 32)
            {
                var value = (byte)((singleValue >> index) & 1);
                isMaxed = value > 0;
                return value;
            }
            else if (index < 48)
            {
                var value = (byte)((multiValue >> (index - 32)) & 3);
                isMaxed = value > 2;
                return value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("GetAttribute index must be between 0 and 47.");
            }
        }
    }

    public partial class TowerAsset
    {
        public bool GetTowerLevel(byte index)
        {
            if (index > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ((TowerLevel >> index) & 1UL) == 1UL;
        }

        public void SetTowerLevel(byte index, bool state)
        {
            if (index > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ulong towerLevels = TowerLevel;
            if (state)
            {
                towerLevels |= (1UL << index);
            }
            else
            {
                towerLevels &= ~(1UL << index);
            }

            TowerLevel = towerLevels;
        }

        public void Achievement(byte level, byte bossType)
        {
            var index =  (4 *level) + bossType;
            if (index > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetTowerLevel((byte)index, true);
        }
    }
}
