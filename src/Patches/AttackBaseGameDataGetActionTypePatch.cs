using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.GameData;
using HarmonyLib;
using UnityEngine;

namespace CombatExtender.Patches
{
    internal static class AttackUtils
    {
        public static bool IsAttackSpell(AttackBase attack)
        {
            return attack != null && attack.AbilityOrigin != null && attack.AbilityOrigin.IsSpell;
        }

        public static bool HasFreeCastStatus(StatusEffectManager statusManager)
        {
            return statusManager.HasStatusEffect((st) =>
                st != null && st.StatusEffectType == StatusEffectType.AttackSpeedMultiplier
                && st.GameData.Keywords.Count == 1
                && st.GameData.Keywords[0].DebugName == "ArenaMod01_FreeCast_Keyword"
            );
        }

        public static bool IsAttackFriendly(AttackBase attack)
        {
            return attack.AttackData.AffectedTargetType == TargetType.Friendly
            || attack.AttackData.AffectedTargetType == TargetType.Ally
            || attack.AttackData.AffectedTargetType == TargetType.Self
            || attack.AttackData.AffectedTargetType == TargetType.AllyNotSelf;
        }

        public static bool IsAttackBuff(AttackBase attack)
        {
            return IsAttackSpell(attack) && IsAttackFriendly(attack);
        }

        // owner instance ID -> attack DebugName -> whether it belongs to one of the owner's spells.
        // Nested so lookups don't build a string key on every call.
        private static readonly Dictionary<int, Dictionary<string, bool>> s_spellAttackCache =
            new Dictionary<int, Dictionary<string, bool>>();

        /// <summary>
        /// Whether the attack data belongs to one of the owner's spells. Cached per owner and
        /// attack DebugName, because nothing changes an ability's attack at runtime.
        /// </summary>
        public static bool IsSpellAttackData(AttackBaseGameData attackData, GameObject owner)
        {
            if (attackData == null || owner == null)
                return false;

            var ownerId = owner.GetInstanceID();
            var key = attackData.DebugName ?? string.Empty;

            // Dictionary<string, bool> byAttack;
            if (!s_spellAttackCache.TryGetValue(ownerId, out var byAttack))
            {
                byAttack = new();
                s_spellAttackCache[ownerId] = byAttack;
            }

            if (byAttack.TryGetValue(key, out var isSpell))
                return isSpell;

            CharacterStats stats = ComponentUtils.GetComponent<CharacterStats>(owner);
            if (stats == null || stats.AbilityList == null)
                return false; // not cached: the owner may not be set up yet

            isSpell = stats.AbilityList.Abilities.Any(
                (ability) => ability != null
                            && ability.IsSpell && ability.GenericAbilityData != null
                            && ability.GenericAbilityData.Attack == attackData
            );
            byAttack[key] = isSpell;
            return isSpell;
        }

        /// <summary>Drops all cached results, e.g. after a level up or loading a save.</summary>
        public static void ClearSpellAttackCache()
        {
            s_spellAttackCache.Clear();
        }
    }

    [HarmonyPatch(typeof(AttackBaseGameData), "GetActionType", new[] { typeof(AttackBase), typeof(GameObject) })]
    internal static class AttackBaseGameDataGetActionTypePatch
    {
        // Runs after the original. __result holds its return value; assign it to override.
        private static void Postfix(AttackBaseGameData __instance, AttackBase instance, GameObject owner, ref TacticalActionType __result)
        {
            try
            {
                StatusEffectManager statusManager = ComponentUtils.GetComponent<StatusEffectManager>(owner);
                if (__instance == null || instance == null || statusManager == null)
                    return;
                // For Buff spell, change to Free Action
                if (
                    // __result == TacticalActionType.Cast -> This check will never fire because of the GetIsLongCast patch.
                    // We should use the original checks, this will not touch ability that is naturally a Standard action.
                    __instance.CastSpeed.Duration > 0f
                    && AttackUtils.IsAttackSpell(instance)
                    && AttackUtils.HasFreeCastStatus(statusManager)
                )
                {
                    // Free action for friendly buff spell, standard action for damaging spell
                    __result = AttackUtils.IsAttackFriendly(instance) ? TacticalActionType.Free : TacticalActionType.Standard;

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    [HarmonyPatch(typeof(AttackBaseGameData), "GetRecoveryDuration",
        new[] { typeof(AttackBase), typeof(GameObject), typeof(bool), typeof(StatBreakdown) })]
    internal static class AttackBaseGameDataGetRecoveryDurationPatch
    {
        // Free actions have no recovery. Repeats the GetActionType patch's Free rule
        // (friendly spell + free-cast status) instead of calling GetActionType, so the
        // two patches can never call each other in a loop.
        private static void Postfix(AttackBaseGameData __instance, AttackBase instance, GameObject owner, ref AdjustedValue __result)
        {
            try
            {
                if (__instance == null || instance == null || owner == null || !AttackUtils.IsAttackBuff(instance))
                    return;

                StatusEffectManager statusManager = ComponentUtils.GetComponent<StatusEffectManager>(owner);
                if (statusManager == null || !AttackUtils.HasFreeCastStatus(statusManager))
                    return;
                __result = new AdjustedValue(0f);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    // This will change most Cast -> Standard since this function got called first.
    [HarmonyPatch(typeof(AttackBaseGameData), "GetIsLongCast", new[] { typeof(GameObject) })]
    internal static class AttackBaseGameDataGetIsLongCastPatch
    {
        // Free and Standard actions both have no cast time, so friendliness doesn't matter
        // here: any long spell cast becomes short while the free-cast status is active.
        // This method gets no AttackBase, so the spell check goes through the owner's abilities.
        private static void Postfix(AttackBaseGameData __instance, GameObject owner, ref bool __result)
        {
            try
            {
                if (!__result || __instance == null || owner == null)
                    return;
                StatusEffectManager statusManager = ComponentUtils.GetComponent<StatusEffectManager>(owner);
                if (statusManager == null || !AttackUtils.HasFreeCastStatus(statusManager))
                    return;

                // Checked last: the status check is cheaper and usually fails first.
                if (AttackUtils.IsSpellAttackData(__instance, owner))
                    __result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    [HarmonyPatch(typeof(AttackBase), "IsCastSpeedInstant")]
    internal static class AttackBaseIsCastSpeedInstantPatch
    {
        // Runs after the original. __result holds its return value; assign it to override.
        private static void Postfix(AttackBase __instance, ref bool __result)
        {
            try
            {
                StatusEffectManager statusManager = ComponentUtils.GetComponent<StatusEffectManager>(__instance.Owner);
                if (__instance == null || statusManager == null)
                    return;
                // Replicate the logic in AttackBaseGameDataGetActionTypePatch to make sure the spell is cast instantly
                if (
                    !__result
                    && AttackUtils.IsAttackSpell(__instance)
                    && AttackUtils.HasFreeCastStatus(statusManager)
                )
                {
                    __result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

}
