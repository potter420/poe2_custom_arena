using System;
using Game.GameData;
using HarmonyLib;

namespace CombatExtender.Patches
{
    [HarmonyPatch(typeof(RecipeData), "IsModifiableItem", new[] { typeof(ItemGameData) })]
    internal static class RecipeDataIsModifiableItemPatch
    {
        // Runs after the original. __result holds its return value; assign it to override.
        private static void Postfix(RecipeData __instance, ItemGameData item, ref bool __result)
        {
            try
            {
                EquippableGameData equipData = item as EquippableGameData;
                if (__instance == null || equipData == null)
                    return;
                var slot = equipData.EquippableComponent.EquipmentSlot;
                if (!__result && __instance.CanModifyArmor && (
                    slot == PermittedEquipmentSlot.RingAnyHand
                    || slot == PermittedEquipmentSlot.RingLeftHand
                    || slot == PermittedEquipmentSlot.RingRightHand
                    || slot == PermittedEquipmentSlot.Cape
                    || slot == PermittedEquipmentSlot.Neck
                    || slot == PermittedEquipmentSlot.Waist
                    || slot == PermittedEquipmentSlot.Head
                    || slot == PermittedEquipmentSlot.Feet
                    || slot == PermittedEquipmentSlot.Hand
                ))
                    __result = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
