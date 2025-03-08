using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public struct FullHouseFuryIdentifier : ITransitionIdentifier
    {
        public byte TransitionType { get; set; }
        public byte TransitionSubType { get; set; }

        public FullHouseFuryIdentifier(byte transitionType, byte transitionSubType)
        {
            TransitionType = transitionType;
            TransitionSubType = transitionSubType;
        }

        public FullHouseFuryIdentifier(byte transitionType) : this(transitionType, 0)
        {
        }

        public static FullHouseFuryIdentifier Create(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Create << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Start(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Start << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Preparation(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Preparation << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

    }
}