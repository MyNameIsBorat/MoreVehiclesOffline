// <copyright file="Compatibility.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System.Collections.Generic;
    using System.Linq;
    using ColossalFramework.Plugins;
    using static MoreVehicles.WorkshopMods;

    /// <summary>
    /// An utility class for checking the compatibility with other installed mods.
    /// </summary>
    internal static class Compatibility
    {
        private static readonly ulong[] IncompatibleModIds =
        {
            TrafficManagerPresidentEditionStable,
        };

        /// <summary>Checks for enabled incompatible mods and prepares a notification message text if any found.</summary>
        /// <returns><c>true</c> if there are any active incompatible mod detected; otherwise, <c>false</c>.</returns>
        public static bool AreAnyIncompatibleModsActive()
        {
            var mods = new HashSet<ulong>();
            foreach (ulong id in PluginManager.instance.GetPluginsInfo().Select(p => p.publishedFileID.AsUInt64))
            {
                mods.Add(id);
            }

            return IncompatibleModIds.Any(id => mods.Contains(id));
        }
    }
}
