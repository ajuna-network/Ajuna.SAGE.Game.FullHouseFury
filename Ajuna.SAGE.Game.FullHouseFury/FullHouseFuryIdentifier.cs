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

        public static FullHouseFuryIdentifier Start(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Start << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Play(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Play << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Preparation(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Preparation << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Battle(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Battle << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Discard(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Discard << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

        public static FullHouseFuryIdentifier Score(AssetType assetType, AssetSubType assetSubType)
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Score << 4 | (byte)AssetType.None, (byte)(((byte)assetType << 4) + (byte)assetSubType));

    }
}