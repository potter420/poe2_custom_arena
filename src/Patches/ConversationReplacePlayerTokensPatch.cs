using System;
using System.Text.RegularExpressions;
using Onyx;
using Game;
using HarmonyLib;

namespace CombatExtender.Patches
{
    /// <summary>
    /// Adds [Global name] tags to conversation text. Runs after the original
    /// ReplacePlayerTokens, so the player tokens are already replaced.
    /// Tags that can't be resolved are left in the text unchanged.
    /// </summary>
    [HarmonyPatch(typeof(Conversation), "ReplacePlayerTokens", new[] { typeof(string) })]
    internal static class ConversationReplacePlayerTokensPatch
    {
        // Matches [Global name] and captures the name. Extra spaces are allowed: [Global  name ]
        private static readonly Regex GlobalTag = new Regex(@"\[Global\s+([^\]\s]+)\s*\]");

        private static void Postfix(ref string __result)
        {
            // Conversation text is processed often; skip the regex when there's no tag.
            if (string.IsNullOrEmpty(__result) || __result.IndexOf("[Global", StringComparison.Ordinal) < 0)
                return;

            try
            {
                __result = GlobalTag.Replace(__result, ReplaceTag);
            }
            catch (Exception ex)
            {
                // On failure the text keeps the original's result.
                Log.Error(ex);
            }
        }

        private static string ReplaceTag(Match match)
        {
            var name = match.Groups[1].Value;
            var value = ResolveGlobal(name);
            if (value == null)
            {
                Log.Warn($"ReplacePlayerTokens: no value for [Global {name}], left unchanged");
                return match.Value;
            }
            return value;
        }

        // TODO: look up the global variable and return its value as text.
        // Return null when the global doesn't exist, so the tag stays visible.
        private static string ResolveGlobal(string name)
        {
            int value = SingletonBehavior<GlobalVariables>.Instance.GetVariable(name);
            return value < 0 ? null : value.ToString();
        }
    }
}
