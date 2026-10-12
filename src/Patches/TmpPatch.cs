// static void Prefix(Game.StatusEffectManager __instance, StatusEffectGameData __0)
// {
//     if (__instance.ToString() != "Player_Vellenia {StatusEffectManager}")
//         return;
//     var name = __0?.ToString() ?? "null";
//     UnityExplorer.ExplorerCore.Log(
//         $"AddNewEffect {name} on {__instance.gameObject.name}\n" +
//         new System.Diagnostics.StackTrace(1, false));
// }


// static void Postfix(Game.StatusEffectManager __instance, Game.StatusEffectAddResult __result, Game.GameData.StatusEffectGameData __0, Game.IStatusEffectOrigin __1, bool __2, float __3, bool __4)
// {
//     if (__instance.ToString() != "Player_Vellenia {StatusEffectManager}")
//         return;
//     try
//     {
//         StringBuilder sb = new StringBuilder();
//         sb.AppendLine("--------------------");
//         sb.AppendLine("Game.StatusEffectAddResult Game.StatusEffectManager::AddNewEffect(Game.GameData.StatusEffectGameData gameData, Game.IStatusEffectOrigin origin, bool empowered, float shapeAdjust, bool skipResistance)");
//         sb.Append("- __instance: ").AppendLine(__instance.ToString());
//         sb.Append("- Parameter 0 'gameData': ").AppendLine(__0?.ToString() ?? "null");
//         sb.Append("- Parameter 1 'origin': ").AppendLine(__1?.ToString() ?? "null");
//         sb.Append("- Parameter 2 'empowered': ").AppendLine(__2.ToString());
//         sb.Append("- Parameter 3 'shapeAdjust': ").AppendLine(__3.ToString());
//         sb.Append("- Parameter 4 'skipResistance': ").AppendLine(__4.ToString());
//         sb.Append($"- Return value: {__result.ResultType}");
//         UnityExplorer.ExplorerCore.Log(sb.ToString());
//     }
//     catch (System.Exception ex)
//     {
//         UnityExplorer.ExplorerCore.LogWarning($"Exception in patch of Game.StatusEffectAddResult Game.StatusEffectManager::AddNewEffect(Game.GameData.StatusEffectGameData gameData, Game.IStatusEffectOrigin origin, bool empowered, float shapeAdjust, bool skipResistance):\n{ex}");
//     }
// }
