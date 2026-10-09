using System;
using BepInEx.Logging;

namespace CombatExtender
{
    /// <summary>
    /// Static access to the plugin's BepInEx logger so patches can log without a plugin reference.
    /// Output goes to the BepInEx console and BepInEx\LogOutput.log.
    /// </summary>
    internal static class Log
    {
        private static ManualLogSource _source;

        public static void Init(ManualLogSource source) { _source = source; }

        public static void Info(string message) { if (_source != null) _source.LogInfo(message); }
        public static void Warn(string message) { if (_source != null) _source.LogWarning(message); }
        public static void Error(string message) { if (_source != null) _source.LogError(message); }
        public static void Error(Exception ex) { if (_source != null) _source.LogError(ex); }
    }
}
