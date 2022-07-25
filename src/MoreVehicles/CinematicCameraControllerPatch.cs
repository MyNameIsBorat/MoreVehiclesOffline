// <copyright file="CinematicCameraControllerPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using SkyTools.Patching;
    using UnityEngine;
    using static Constants;

    /// <summary>
    /// A static class that provides the patch objects for the Cinematic Camera Controller methods.
    /// </summary>
    internal static class CinematicCameraControllerPatch
    {
        /// <summary>Gets the patch for the 'GetRandomVehicle' method.</summary>
        public static IPatch GetRandomVehicle { get; } = new CinematicCameraController_GetRandomVehicle();

        /// <summary>Gets the patch for the 'GetVehicleWithName' method.</summary>
        public static IPatch GetVehicleWithName { get; } = new CinematicCameraController_GetVehicleWithName();

        /// <summary>Gets the patch for the 'GetNearestVehicle' method.</summary>
        public static IPatch GetNearestVehicle { get; } = new CinematicCameraController_GetNearestVehicle();

        private sealed class CinematicCameraController_GetRandomVehicle : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(CinematicCameraController).GetMethod(
                    nameof(CinematicCameraController.GetRandomVehicle),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ItemClass.Service), typeof(ItemClass.SubService), typeof(ItemClass.Level) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }

        private sealed class CinematicCameraController_GetVehicleWithName : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(CinematicCameraController).GetMethod(
                    nameof(CinematicCameraController.GetVehicleWithName),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(string) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }

        private sealed class CinematicCameraController_GetNearestVehicle : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(CinematicCameraController).GetMethod(
                    nameof(CinematicCameraController.GetNearestVehicle),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(Vector3) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }
    }
}
