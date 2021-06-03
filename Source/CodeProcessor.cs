// <copyright file="CodeProcessor.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace MoreVehicles
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using Harmony;

    /// <summary>
    /// A helper class providing methods for IL code manipulation.
    /// </summary>
    internal static class CodeProcessor
    {
        /// <summary>
        /// Replaces the values of <see cref="int"/> operands for all instructions that support operand of that type.
        /// </summary>
        /// <param name="instructions">A collection of IL instructions to process. Cannot be null.</param>
        /// <param name="oldValue">The old operand value to search for.</param>
        /// <param name="newValue">The new operand value to set.</param>
        ///
        /// <returns>A collection of processed IL code instructions.</returns>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instructions"/> is null.</exception>
        public static IEnumerable<CodeInstruction> ReplaceOperands(IEnumerable<CodeInstruction> instructions, int oldValue, int newValue)
        {
            if (instructions == null)
            {
                throw new ArgumentNullException(nameof(instructions));
            }

            return ReplaceOperandsCore(instructions, oldValue, newValue);
        }

        private static IEnumerable<CodeInstruction> ReplaceOperandsCore(IEnumerable<CodeInstruction> instructions, int oldValue, int newValue)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode.OperandType == OperandType.InlineI && (int)instruction.operand == oldValue)
                {
                    instruction.operand = newValue;
                }

                yield return instruction;
            }
        }
    }
}
