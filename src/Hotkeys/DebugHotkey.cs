using System;
using BepInEx.Configuration;
using UnityEngine;

namespace CombatExtender.Hotkeys
{
    /// <summary>
    /// Hotkey that prints a message to the in-game console and the BepInEx log.
    /// The key is set in BepInEx\config\combatextender.cfg (default Ctrl+F9).
    /// </summary>
    internal static class DebugHotkey
    {
        private static ConfigEntry<KeyboardShortcut> s_key;

        public static void Init(ConfigFile config)
        {
            s_key = config.Bind("Hotkeys", "DebugMessage",
                new KeyboardShortcut(KeyCode.I, KeyCode.LeftControl),
                "Prints a debug message to the in-game console.");
        }

        // Called every frame from the plugin's Update.
        public static void Update()
        {
            if (s_key == null || !s_key.Value.IsDown())
                return;

            try
            {
                GameObject objectByID = Game.InstanceID.GetObjectByID(Game.SpecialCharacterInstanceID.HoveredGuid);
                if (objectByID == null)
                {

                    Game.Console.AddMessage("CombatExtender hotkey pressed, but no object is hovered.");
                    return;
                }
                string text = (GUIUtility.systemCopyBuffer = Game.InstanceID.GetObjectID(objectByID).ToString());
                Game.Console.AddMessage($"Hovered Object => {objectByID.name}({text}, {objectByID.GetType().Name})");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
