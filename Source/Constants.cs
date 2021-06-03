// <copyright file="Constants.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    /// <summary>
    /// A static class defining various constant values.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// A string representing the mod name in the save game metadata.
        /// </summary>
        public const string MetadataModName = "User/More Vehicles";

        /// <summary>
        /// The maximum active vehicle count in the vanilla game (array buffer size).
        /// </summary>
        public const int VanillaMaxVehicleCount = 16384;

        /// <summary>
        /// The maximum parked vehicle count in the vanilla game (array buffer size).
        /// </summary>
        public const int VanillaMaxParkedVehicleCount = 32768;

        /// <summary>
        /// The maximum active or parked vehicle count ensured by this mod (array buffer size).
        /// </summary>
        public const int ModdedMaxVehicleCount = ushort.MaxValue + 1;
    }
}
