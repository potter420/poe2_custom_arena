using Game;
using HarmonyLib;

namespace CombatExtender.Patches
{
    /// <summary>
    /// Fixes PartyAuraAbility applying its effects to the owner twice. It inherits
    /// OverrideEffectApplication = false from GenericAbility, so ActivateHelper applies the
    /// effects to the owner, and then the aura applies them again to every party member in
    /// range, owner included. AuraAbility already returns true here; this does the same for
    /// PartyAuraAbility so only the aura path applies effects.
    /// </summary>
    [HarmonyPatch(typeof(GenericAbility), "OverrideEffectApplication", MethodType.Getter)]
    internal static class PartyAuraAbilityOverrideEffectApplicationPatch
    {
        // PartyAuraAbility doesn't override the getter, so it runs this base version.
        // Subclasses with their own override (like AuraAbility) never reach this patch.
        private static void Postfix(GenericAbility __instance, ref bool __result)
        {
            if (!__result && __instance is PartyAuraAbility)
                __result = true;
        }
    }
}
