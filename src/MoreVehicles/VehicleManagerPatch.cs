// <copyright file="VehicleManagerPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ColossalFramework.IO;
    using Harmony;
    using SkyTools.Patching;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>
    /// A static class that provides the patch objects for the Vehicle Manager methods.
    /// </summary>
    internal static class VehicleManagerPatch
    {
        /// <summary>Gets the patch for the 'Deserialize' method of the nested 'VehicleManager.Data' class.</summary>
        public static IPatch DataDeserialize { get; } = new VehicleManager_Data_Deserialize();

        /// <summary>Gets the patch for the 'UpdateData' method.</summary>
        public static IPatch UpdateData { get; } = new VehicleManager_UpdateData();

        /// <summary>Gets the patch for the 'SimulationStepImpl' method.</summary>
        public static IPatch SimulationStepImpl { get; } = new VehicleManager_SimulationStepImpl();

        /// <summary>Gets the patch for the 'AirlineModified' method.</summary>
        public static IPatch AirlineModified { get; } = new VehicleManager_AirlineModified();

        private sealed class VehicleManager_Data_Deserialize : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(VehicleManager.Data).GetMethod(
                    nameof(VehicleManager.Data.Deserialize),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(DataSerializer) },
                    new ParameterModifier[0]);

            private static int FindLoadLocalInstructionForStoreLocal(
                List<CodeInstruction> instructions,
                int startIndex,
                CodeInstruction storeLocalInstruction,
                bool searchBackwards)
            {
                var storeLocalOp = storeLocalInstruction.opcode;
                OpCode loadLocalOp;
                int loadLocalOperand = 0;
                if (storeLocalOp == OpCodes.Stloc_0)
                {
                    loadLocalOp = OpCodes.Ldloc_0;
                }
                else if (storeLocalOp == OpCodes.Stloc_1)
                {
                    loadLocalOp = OpCodes.Ldloc_1;
                }
                else if (storeLocalOp == OpCodes.Stloc_2)
                {
                    loadLocalOp = OpCodes.Ldloc_2;
                }
                else if (storeLocalOp == OpCodes.Stloc_3)
                {
                    loadLocalOp = OpCodes.Ldloc_3;
                }
                else if (storeLocalOp == OpCodes.Stloc_S)
                {
                    loadLocalOp = OpCodes.Ldloc_S;
                    loadLocalOperand = ((LocalBuilder)storeLocalInstruction.operand).LocalIndex;
                }
                else if (storeLocalOp == OpCodes.Stloc)
                {
                    loadLocalOp = OpCodes.Ldloc;
                    loadLocalOperand = ((LocalBuilder)storeLocalInstruction.operand).LocalIndex;
                }
                else
                {
                    Log.Error($"The 'More Vehicles' mod failed to customize the vehicle manager: unexpected store local IL '{storeLocalOp}'");
                    return -1;
                }

                return searchBackwards
                    ? instructions.FindLastIndex(startIndex, Search)
                    : instructions.FindIndex(startIndex, Search);

                bool Search(CodeInstruction i) =>
                    i.opcode == loadLocalOp
                        && (i.opcode != OpCodes.Ldloc_S && i.opcode != OpCodes.Ldloc
                            || ((LocalBuilder)i.operand).LocalIndex == loadLocalOperand);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var vehicleCountGetter = AccessTools.Method(
                    typeof(VehicleManager_Data_Deserialize),
                    nameof(VehicleManager_Data_Deserialize.GetMaxVehicleCount));

                var parkedVehicleCountGetter = AccessTools.Method(
                    typeof(VehicleManager_Data_Deserialize),
                    nameof(VehicleManager_Data_Deserialize.GetMaxParkedVehicleCount));

                const string bufferName = nameof(Array16<Vehicle>.m_buffer);

                return PatchCode(instructionList, nameof(VehicleManager.m_vehicles), bufferName, vehicleCountGetter, fullScanCount: 1)
                    && PatchCode(instructionList, nameof(VehicleManager.m_parkedVehicles), bufferName, parkedVehicleCountGetter, fullScanCount: 2)
                ? instructionList
                : instructions;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix()
            {
                var vehicles = VehicleManager.instance.m_vehicles.m_buffer;
                Array.Clear(vehicles, 0, vehicles.Length);

                var parkedVehicles = VehicleManager.instance.m_parkedVehicles.m_buffer;
                Array.Clear(parkedVehicles, 0, parkedVehicles.Length);
                return true;
            }

            private static bool PatchCode(
                List<CodeInstruction> instructionList,
                string targetArrayName,
                string bufferArrayName,
                MethodInfo arraySizeGetter,
                int fullScanCount)
            {
                // Find instruction 'ldfld class Array16`1<...> VehicleManager::targetArrayName'
                int index = instructionList.FindIndex(
                    i => i.opcode == OpCodes.Ldfld && i.operand.ToString().Contains(targetArrayName));

                if (index < 0 || ++index >= instructionList.Count)
                {
                    Log.Error($"The 'More Vehicles' mod failed to customize the vehicle manager: no IL instruction for '{targetArrayName}'");
                    return false;
                }

                var instruction = instructionList[index];

                // Ensure that the next instruction is 'ldfld !0[] class Array16`1<...>::bufferArrayName'
                if (instruction.opcode != OpCodes.Ldfld
                    || !instruction.operand.ToString().Contains(bufferArrayName)
                    || ++index >= instructionList.Count)
                {
                    Log.Error($"The 'More Vehicles' mod failed to customize the vehicle manager: no IL instruction for '{bufferArrayName}'");
                    return false;
                }

                // Find the corresponding 'load local' instruction, e.g. 'ldloc.1'
                instruction = instructionList[index];
                index = FindLoadLocalInstructionForStoreLocal(instructionList, index, instruction, searchBackwards: false);

                // Ensure that the next instruction is 'ldlen'
                if (index < 0 || ++index >= instructionList.Count || instructionList[index].opcode != OpCodes.Ldlen)
                {
                    Log.Error("The 'More Vehicles' mod failed to customize the vehicle manager: ldloc and ldlen IL instructions not found");
                    return false;
                }

                if (index + 2 >= instructionList.Count)
                {
                    Log.Error("The 'More Vehicles' mod failed to customize the vehicle manager: stloc IL instructions not found, method end reached");
                    return false;
                }

                // 'conv.i4' might be at index + 1, so trying index + 2
                instruction = instructionList[index + 1];
                if (instruction.opcode == OpCodes.Conv_I4)
                {
                    instruction = instructionList[index + 2];
                }

                var opcode = instruction.opcode;
                if (opcode != OpCodes.Stloc
                    && opcode != OpCodes.Stloc_0
                    && opcode != OpCodes.Stloc_1
                    && opcode != OpCodes.Stloc_2
                    && opcode != OpCodes.Stloc_3
                    && opcode != OpCodes.Stloc_S)
                {
                    Log.Error("The 'More Vehicles' mod failed to customize the vehicle manager: stloc IL instructions not found");
                    return false;
                }

                // Remove the 'ldloc' instruction completely...
                instructionList.RemoveAt(--index);

                // ...and replace the 'ldlen' instruction with a call to our custom method
                // which determines the vehicle count for deserialization.
                // For our 'patched' save games, a new value will be used. For new games and old saves - the standard value.
                instructionList[index] = new CodeInstruction(OpCodes.Call, arraySizeGetter);

                index = instructionList.Count - 1;
                while (fullScanCount-- > 0)
                {
                    // Find the last 'ldloc' of our array size variable
                    index = FindLoadLocalInstructionForStoreLocal(instructionList, index, instruction, searchBackwards: true);
                    if (index < 0)
                    {
                        Log.Error("The 'More Vehicles' mod failed to customize the vehicle manager: last ldloc IL instructions not found");
                        return false;
                    }

                    // Replace the last 'ldloc' instruction which loads the array size with our custom value.
                    // This is because we need to perform the last loop over the whole (enlarged) array.
                    instructionList[index] = new CodeInstruction(OpCodes.Ldc_I4, ModdedMaxVehicleCount);
                }

                return true;
            }

            private static bool IsModEnabled()
            {
                var mods = SimulationManager.instance.m_metaData?.m_modOverride;
                return mods != null && mods.TryGetValue(MetadataModName, out bool modEnabled) && modEnabled;
            }

            private static int GetMaxVehicleCount() => IsModEnabled() ? ModdedMaxVehicleCount : VanillaMaxVehicleCount;

            private static int GetMaxParkedVehicleCount() => IsModEnabled() ? ModdedMaxVehicleCount : VanillaMaxParkedVehicleCount;
        }

        private sealed class VehicleManager_UpdateData : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(VehicleManager).GetMethod(
                    nameof(VehicleManager.UpdateData),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(SimulationManager.UpdateMode) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                var replacedVehicleCount = CodeProcessor.ReplaceOperands(instructions, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
                return CodeProcessor.ReplaceOperands(replacedVehicleCount, VanillaMaxParkedVehicleCount, ModdedMaxVehicleCount);
            }
        }

        private sealed class VehicleManager_SimulationStepImpl : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(VehicleManager).GetMethod(
                    "SimulationStepImpl",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                var replaced1024 = CodeProcessor.ReplaceOperands(instructions, 1024, 4096);
                return CodeProcessor.ReplaceOperands(replaced1024, VanillaMaxVehicleCount, ModdedMaxVehicleCount);
            }
        }

        private sealed class VehicleManager_AirlineModified : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(VehicleManager).GetMethod(
                    nameof(VehicleManager.AirlineModified),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(VehicleManager __instance)
            {
                for (int i = 0; i < __instance.m_vehicles.m_buffer.Length; ++i)
                {
                    var info = __instance.m_vehicles.m_buffer[i].Info;
                    if (info != null)
                    {
                        (info.m_vehicleAI as PassengerPlaneAI)?.SetAirline((ushort)i, ref __instance.m_vehicles.m_buffer[i]);
                    }
                }

                return false;
            }
        }
    }
}
