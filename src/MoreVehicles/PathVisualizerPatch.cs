// <copyright file="PathVisualizerPatch.cs" company="dymanoid">
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
    /// A static class that provides the patch objects for the Path Visualizer methods.
    /// </summary>
    internal static class PathVisualizerPatch
    {
        /// <summary>Gets the patch for the 'AddPathsImpl' method.</summary>
        public static IPatch AddPathsImpl { get; } = new PathVisualizer_AddPathsImpl();

        private sealed class PathVisualizer_AddPathsImpl : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(PathVisualizer).GetMethod(
                    "AddPathsImpl",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), typeof(int) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
                => CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
        }
    }
}
