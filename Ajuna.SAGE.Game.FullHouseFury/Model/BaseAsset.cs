
using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    /// <summary>
    /// Base asset class for all assets in the CasinoJam game.
    /// </summary>
    public class BaseAsset : Asset
    {
        public BaseAsset(uint ownerId, uint score = 0, uint genesis = 0)
            : base(Utils.GenerateRandomId(), ownerId, FullHouseFuryUtil.COLLECTION_ID, score, genesis, new byte[Constants.DNA_SIZE])
        { }

        public BaseAsset(uint ownerId, byte collectionId, uint score, uint genesis)
            : base(Utils.GenerateRandomId(), ownerId, collectionId, score, genesis, new byte[Constants.DNA_SIZE])
        { }

        public BaseAsset(uint id, uint ownerId, byte collectionId, uint score, uint genesis)
            : base(id, ownerId, collectionId, score, genesis, new byte[Constants.DNA_SIZE])
        { }

        public BaseAsset(IAsset asset)
            : base(asset.Id, asset.OwnerId, asset.CollectionId, asset.Score, asset.Genesis, asset.Data)
        { }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// H....... ........ ........ ........
        public AssetType AssetType
        {
            get => (AssetType)Data.Read(0, ByteType.High);
            set => Data?.Set(0, ByteType.High, (byte)value);
        }

        /// 00000000 00111111 11112222 22222233
        /// 01234567 89012345 67890123 45678901
        /// L....... ........ ........ ........
        public AssetSubType AssetSubType
        {
            get => (AssetSubType)Data.Read(0, ByteType.Low);
            set => Data?.Set(0, ByteType.Low, (byte)value);
        }

        /// <inheritdoc/>
        public override byte[] MatchType => Data != null && Data.Length > 0 ? new byte[] { Data[0] } : Array.Empty<byte>();

    }
}