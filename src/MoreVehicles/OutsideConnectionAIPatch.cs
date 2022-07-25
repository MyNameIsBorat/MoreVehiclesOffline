// <copyright file="OutsideConnectionAIPatch.cs" company="dymanoid">
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
    /// A static class that provides the patch objects for the Outside Connection AI methods.
    /// </summary>
    internal static class OutsideConnectionAIPatch
    {
        /// <summary>Gets the patch for the 'DummyTrafficProbability' method.</summary>
        public static IPatch DummyTrafficProbability { get; } = new OutsideConnectionAI_DummyTrafficProbability();

        private sealed class OutsideConnectionAI_DummyTrafficProbability : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(OutsideConnectionAI).GetMethod(
                    "DummyTrafficProbability",
                    BindingFlags.Static | BindingFlags.NonPublic,
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
