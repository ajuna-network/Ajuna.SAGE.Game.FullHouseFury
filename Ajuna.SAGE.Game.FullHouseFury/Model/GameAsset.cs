using Ajuna.SAGE.Core;
using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury.Model
{
    /// <summary>
    /// GameAsset stores the current game progress, including the floor level (progress),
    /// the enemy's current health, and the available discards during combat.
    /// The values are stored in the underlying asset's Data array at fixed offsets.
    /// </summary>
    public class GameAsset : BaseAsset
    {
        // Offsets for game-specific fields (assuming Data is at least 20 bytes long)
        private const byte FLOOR_OFFSET = 8;          // Floor level stored at bytes 8-11 (4 bytes)
        private const byte ENEMY_HEALTH_OFFSET = 12;    // Enemy health stored at bytes 12-15 (4 bytes)
        private const byte DISCARDS_OFFSET = 16;        // Discards available stored at bytes 16-19 (4 bytes)
        private const byte FIELD_SIZE = 4;              // Each field uses 4 bytes (uint)

        public GameAsset(uint ownerId, uint genesis)
            : base(ownerId, genesis)
        {
            AssetType = AssetType.Game; // Ensure your AssetType enum includes 'Game'
            // Initialize default values (e.g., floor 1, enemy health 100, discards available 3)
            SetFloorLevel(1);
            SetEnemyHealth(100);
            SetDiscardsAvailable(3);
        }

        public GameAsset(IAsset asset)
            : base(asset)
        { }

        /// <summary>
        /// Gets the current floor level (game progress).
        /// </summary>
        public uint GetFloorLevel()
        {
            byte[] floorBytes = Data.Read(FLOOR_OFFSET, FIELD_SIZE);
            return BitConverter.ToUInt32(floorBytes, 0);
        }

        /// <summary>
        /// Sets the current floor level (game progress).
        /// </summary>
        public void SetFloorLevel(uint level)
        {
            byte[] floorBytes = BitConverter.GetBytes(level);
            Data.Set(FLOOR_OFFSET, floorBytes);
        }

        /// <summary>
        /// Gets the enemy's current health.
        /// </summary>
        public uint GetEnemyHealth()
        {
            byte[] healthBytes = Data.Read(ENEMY_HEALTH_OFFSET, FIELD_SIZE);
            return BitConverter.ToUInt32(healthBytes, 0);
        }

        /// <summary>
        /// Sets the enemy's current health.
        /// </summary>
        public void SetEnemyHealth(uint health)
        {
            byte[] healthBytes = BitConverter.GetBytes(health);
            Data.Set(ENEMY_HEALTH_OFFSET, healthBytes);
        }

        /// <summary>
        /// Gets the number of discards available for this combat.
        /// </summary>
        public uint GetDiscardsAvailable()
        {
            byte[] discardBytes = Data.Read(DISCARDS_OFFSET, FIELD_SIZE);
            return BitConverter.ToUInt32(discardBytes, 0);
        }

        /// <summary>
        /// Sets the number of discards available for this combat.
        /// </summary>
        public void SetDiscardsAvailable(uint discards)
        {
            byte[] discardBytes = BitConverter.GetBytes(discards);
            Data.Set(DISCARDS_OFFSET, discardBytes);
        }
    }
}
