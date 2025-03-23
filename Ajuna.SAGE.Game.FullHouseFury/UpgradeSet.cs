using System;

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public class UpgradeSet
    {
        public UpgradeSet(ushort upgrade)
        {
            var bytes = BitConverter.GetBytes(upgrade);
            FeatureType = (FeatureType)(bytes[0] >> 4);
            FeatureEnum = (byte)(bytes[0] & 0x0F);
            Level = bytes[1];
        }

        public UpgradeSet(FeatureType featureType, byte featureEnum, byte level)
        {
            FeatureType = featureType;
            FeatureEnum = featureEnum;
            Level = level;
        }
        
        public ushort Encode()
        {
            var bytes = new byte[2];
            bytes[0] = (byte)((byte)FeatureType << 4 | FeatureEnum);
            bytes[1] = Level;
            return BitConverter.ToUInt16(bytes, 0);
        }

        public FeatureType FeatureType { get; }
        public byte FeatureEnum { get; }
        public byte Level { get; }
    }
}