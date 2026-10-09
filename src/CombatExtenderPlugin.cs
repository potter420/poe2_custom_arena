using BepInEx;
using CombatExtender.Hotkeys;
using HarmonyLib;

namespace CombatExtender
{
    [BepInPlugin(Guid, Name, Version)]
    public class CombatExtenderPlugin : BaseUnityPlugin
    {
        public const string Guid = "combatextender";
        public const string Name = "CombatExtender";
        public const string Version = "0.1.0";

        // Used to start coroutines from static code such as Harmony patches.
        public static CombatExtenderPlugin Instance { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log.Init(Logger);
            // Applies every [HarmonyPatch] class in this assembly.
            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(CombatExtenderPlugin).Assembly);

            DebugHotkey.Init(Config);

            Log.Info($"{Name} v{Version} loaded.");
        }

        private void Update()
        {
            DebugHotkey.Update();
        }

        private void OnDestroy()
        {
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }
    }
}
