// <copyright file="VehiclePatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using SkyTools.Patching;
    using static Constants;

    /// <summary>
    /// A static class that provides the patch objects for the Vehicle struct methods.
    /// </summary>
    internal static class VehiclePatch
    {
        /// <summary>Gets the patch for the 'GetTargetFrame' method.</summary>
        public static IPatch GetTargetFrame { get; } = new Vehicle_GetTargetFrame();

        private sealed class Vehicle_GetTargetFrame : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(Vehicle).GetMethod(
                    "GetTargetFrame",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(VehicleInfo), typeof(ushort) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }
    }
}
