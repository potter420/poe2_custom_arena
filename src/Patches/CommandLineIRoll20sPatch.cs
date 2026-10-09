using System;
using Game;
using HarmonyLib;
using Onyx;

namespace CombatExtender.Patches
{
    /// <summary>
    /// Keeps achievements enabled when cheats are turned on with the IRoll20s console command.
    /// The original still runs unchanged (cheat toggle, God Challenge check, console messages,
    /// analytics); afterwards DisableAchievements is put back to what it was before the call.
    /// </summary>
    [HarmonyPatch(typeof(CommandLine), "IRoll20s", new Type[0])]
    internal static class CommandLineIRoll20sPatch
    {
        // __state holds DisableAchievements from before the call, or null if there is no tracker.
        private static void Prefix(out bool? __state)
        {
            __state = null;
            try
            {
                var tracker = SingletonBehavior<AchievementTracker>.Instance;
                if (tracker != null)
                    __state = tracker.DisableAchievements;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void Postfix(bool? __state)
        {
            if (__state == null)
                return;

            try
            {
                var tracker = SingletonBehavior<AchievementTracker>.Instance;
                if (tracker == null || tracker.DisableAchievements == __state.Value)
                    return;

                tracker.DisableAchievements = __state.Value;
                Log.Info($"IRoll20s: kept achievements enabled (DisableAchievements restored to {__state.Value})");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
