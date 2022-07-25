// <copyright file="VehicleManagerCustomizer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System.Reflection;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>
    /// A class that modifies the internal structure of the game's VehicleManager.
    /// </summary>
    internal static class VehicleManagerCustomizer
    {
        /// <summary>
        /// Performs the customization of the game class.
        /// </summary>
        public static void Customize()
        {
            ChangeArraysSize(ModdedMaxVehicleCount, ModdedMaxVehicleCount);
            Log.Info("The 'More Vehicles' mod customized the vehicle manager");
        }

        /// <summary>
        /// Reverts any custom changes made to the game class.
        /// </summary>
        public static void Revert()
        {
            ChangeArraysSize(VanillaMaxVehicleCount, VanillaMaxParkedVehicleCount);
            Log.Info("The 'More Vehicles' mod reverted the vehicle manager customization");
        }

        private static void ChangeArraysSize(uint vehicleCount, uint parkedVehicleCount)
        {
            ChangeArray16Size<Vehicle>(nameof(VehicleManager.m_vehicles), vehicleCount);
            ChangeArray16Size<VehicleParked>(nameof(VehicleManager.m_parkedVehicles), parkedVehicleCount);

            ChangeArraySize("m_renderBuffer", vehicleCount >> 6);
            ChangeArraySize("m_renderBuffer2", parkedVehicleCount >> 6);
            ChangeArraySize(nameof(VehicleManager.m_updatedParked), parkedVehicleCount >> 6);
        }

        private static void ChangeArray16Size<T>(string arrayFieldName, uint size)
            where T : struct
        {
            var arrayField = GetField(arrayFieldName);
            if (arrayField != null)
            {
                var newArray = new Array16<T>(size);
                arrayField.SetValue(VehicleManager.instance, newArray);
                newArray.CreateItem(out _);
            }
        }

        private static void ChangeArraySize(string arrayFieldName, uint size)
        {
            var arrayField = GetField(arrayFieldName);
            if (arrayField != null)
            {
                ulong[] newArray = new ulong[size];
                arrayField.SetValue(VehicleManager.instance, newArray);
            }
        }

        private static FieldInfo GetField(string fieldName)
        {
            var field = typeof(VehicleManager).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Log.Error($"The 'More Vehicles' mod failed to setup the Vehicle Manager: no field '{fieldName}' in VehicleManager");
            }

            return field;
        }
    }
}
