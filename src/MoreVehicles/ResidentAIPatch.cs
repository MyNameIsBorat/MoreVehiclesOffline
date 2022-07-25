// <copyright file="ResidentAIPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using SkyTools.Patching;
    using static Constants;

    /// <summary>
    /// A static class that provides the patch objects for the Resident AI methods.
    /// </summary>
    internal static class ResidentAIPatch
    {
        /// <summary>Gets the patch for the 'DoRandomMove' method.</summary>
        public static IPatch DoRandomMove { get; } = new ResidentAI_DoRandomMove();

        private sealed class ResidentAI_DoRandomMove : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(ResidentAI).GetMethod(
                    "DoRandomMove",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }
    }
}
