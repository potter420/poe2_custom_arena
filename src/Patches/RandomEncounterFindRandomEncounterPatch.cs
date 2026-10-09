using System;
using Game;
using Game.GameData;
using HarmonyLib;
using UnityEngine;

namespace CombatExtender.Patches
{
    [HarmonyPatch(typeof(RandomEncounter), "FindRandomEncounter",
        new[] { typeof(WorldMapEncounterGameData), typeof(GameObject) })]
    internal static class RandomEncounterFindRandomEncounterPatch
    {
        // Return false to skip the original. Set __result first, because Preactivate
        // returns early when no encounter is found.
        private static bool Prefix(WorldMapEncounterGameData encounterData, GameObject gameObject, ref RandomEncounter __result)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return true;
        }

        // Runs after the original. __result is the chosen encounter (null if none was found)
        // and can be replaced here.
        private static void Postfix(WorldMapEncounterGameData encounterData, GameObject gameObject, ref RandomEncounter __result)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
