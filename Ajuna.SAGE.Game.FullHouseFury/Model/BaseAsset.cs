
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
            : base(Utils.GenerateRandomId(), ownerId, FullHouseFuryUtil.COLLECTION_ID, score, genesis, new byte[FullHouseFuryUtil.DATA_SIZE])
        { }

        public BaseAsset(uint ownerId, byte collectionId, uint score, uint genesis)
            : base(Utils.GenerateRandomId(), ownerId, collectionId, score, genesis, new byte[FullHouseFuryUtil.DATA_SIZE])
        { }

        public BaseAsset(uint id, uint ownerId, byte collectionId, uint score, uint genesis)
            : base(id, ownerId, collectionId, score, genesis, new byte[FullHouseFuryUtil.DATA_SIZE])
        { }

        public BaseAsset(IAsset asset)
            : base(asset.Id, asset.OwnerId, asset.CollectionId, asset.Score, asset.Genesis, asset.Data)
        { }

        public AssetType AssetType
        {
            get => (AssetType)Data.Read<byte>(0);
            set => Data.Set<byte>(0, (byte)value);
        }

        /// <inheritdoc/>
        public override byte[] MatchType => Data != null && Data.Length > 0 ? new byte[] { Data[0] } : Array.Empty<byte>();
    }
}