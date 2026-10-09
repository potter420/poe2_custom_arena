using System;
using Game;
using Game.GameData;
using UnityEngine;
using UnityEngine.AI;
using Onyx;
using HarmonyLib;

namespace CombatExtender.Patches
{
    [HarmonyPatch(typeof(RandomEncounterSpawner), "Preactivate", new Type[0])]
    internal static class RandomEncounterSpawnerPreactivatePatch
    {
        private readonly static ConversationDataManager convoDM = new();
        private readonly static string conversationName = "99_custom_arena_master_script";
        // Return false to skip the original Preactivate. That also skips base.Preactivate(),
        // the OnSceneEntered subscription and setting m_spawned.
        private static bool Prefix(RandomEncounterSpawner __instance)
        {
            try
            {
                var isTransitionToCustomArena = SingletonBehavior<GlobalVariables>.Instance.GetVariable("b_custom_arena_in_location");
                WorldMapEncounterGameData encounterData = SingletonBehavior<RandomEncounterManager>.Instance.ActiveEncounter;
                RootObjectContainer rootObjectContainer = RootObjectContainer.GetRootObjectContainer(RootObjectContainer.ContainerType.LocalScene);
                if (
                    encounterData == null || encounterData.DebugName != "RE_Custom_Encounter"
                    || rootObjectContainer == null || rootObjectContainer.SceneName != "rm_lax01_arena_standard_01"
                    || isTransitionToCustomArena != 1
                )
                    return true;
                Log.Info($"Got the correct scene, start spawning creatures for encounter {encounterData.DebugName}");
                NPCInteraction interaction = null;
                for (int i = 0; i < rootObjectContainer.transform.childCount; i++)
                {
                    var child = rootObjectContainer.transform.GetChild(i);
                    if (child.name.StartsWith("Spawned_NPC_Prologue_steward_"))
                    {
                        Log.Info($"Found existing steward {child.name}, do nothing, this maybe a load from save game");
                        interaction = child.GetComponent<NPCInteraction>();
                        if (interaction != null){
                            convoDM.Init();
                            var conversationID = convoDM.FindConversationIDByName(conversationName);
                            interaction.Conversation.Assign(conversationID);
                        }
                        return true;
                    }
                }
                GameObject gameObject = Game.ResourceManager.LoadAsset("NPC_Prologue_steward", Game.AssetBundleHint.Characters, instantiate: false) as GameObject;
                gameObject.name = "Spawned_NPC_Prologue_steward_001";
                Vector3 sourcePosition = new(34.2f, 0.1f, 9.7f);
                NavMeshHit hit = default;
          		if (NavMeshUtility.SamplePosition(sourcePosition, out hit, 100f, NavMeshUtility.WalkableAreasMask))
          		{
         			sourcePosition = hit.position;
          		}
                sourcePosition = new Vector3(sourcePosition.x, sourcePosition.y, sourcePosition.z);
                // Modify some of game object characteristics
                interaction = gameObject.GetComponent<NPCInteraction>();
                if (interaction != null){
                    convoDM.Init();
                    var conversationID = convoDM.FindConversationIDByName(conversationName);
                    interaction.Conversation.Assign(conversationID);
                }
                ResourceManager.InstantiateObject<GameObject>(gameObject, sourcePosition, Quaternion.identity);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return true;
        }
    }
}
