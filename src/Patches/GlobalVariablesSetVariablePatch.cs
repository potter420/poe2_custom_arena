using System;
using CombatExtender.Arena;
using Game;
using HarmonyLib;
using Onyx;

namespace CombatExtender.Patches
{
    /// <summary>
    /// Starts an arena wave when the global b_custom_arena_begin_wave changes to 1.
    /// Only a change counts: setting it to 1 when it's already 1 does nothing.
    /// </summary>
    [HarmonyPatch(typeof(GlobalVariables), "SetVariable",
        new[] { typeof(string), typeof(int), typeof(ExceptionMode), typeof(bool) })]
    internal static class GlobalVariablesSetVariablePatch
    {
        private const string BeginWaveVariable = "b_custom_arena_begin_wave";

        // __state is true when the variable was not 1 before the call.
        private static void Prefix(GlobalVariables __instance, string name, int val, out bool __state)
        {
            __state = false;
            if (val != 1 || !IsBeginWave(name))
                return;

            try
            {
                // TODO: read the current value and set __state only if it isn't 1 yet.
                __state = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        // Runs only after the original returned normally, so a failed set starts no wave.
        private static void Postfix(string name, int val, bool __state)
        {
            if (!__state)
                return;

            try
            {
                Log.Info($"SetVariable: {name} -> {val}, starting arena wave");
                ArenaWaveSpawner.BeginWave();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static bool IsBeginWave(string name)
        {
            return string.Equals(name, BeginWaveVariable, StringComparison.OrdinalIgnoreCase);
        }
    }
}
