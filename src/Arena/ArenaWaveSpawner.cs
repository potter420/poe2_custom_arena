using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Game;
using Game.GameData;
using Onyx;

namespace CombatExtender.Arena
{
    /// <summary>
    /// Spawns a custom arena wave. Runs as a Unity coroutine on the plugin, so it is
    /// spread over frames on the main thread rather than run on a background thread
    /// (Unity objects can only be touched from the main thread).
    /// </summary>
    internal static class ArenaWaveSpawner
    {
        private static bool s_running;
        // Square only need 2 points
        private readonly static List<Vector3> s_spawnBoxes = new List<Vector3>
        {
            new (17.0f, 0.0f, 17.0f),
            new (25.4f, 0.0f, 25.4f),
        };
        private static Dictionary<int, Vector3> s_spawnPoints = new();

        private static void PopulatePoints(int numPointsPerSide) {
            if (s_spawnPoints != null && s_spawnPoints.Count == numPointsPerSide * numPointsPerSide)
                return;
            s_spawnPoints.Clear();
            // This is a square, so distance = side / (numPointsPerSide - 1)
            float distance = (s_spawnBoxes[1].x - s_spawnBoxes[0].x) / (numPointsPerSide - 1);
            for (int i = 0; i < numPointsPerSide; i++)
            {
                for (int j = 0; j < numPointsPerSide; j++)
                {
                    int index = i * numPointsPerSide + j;
                    s_spawnPoints[index] = new Vector3(s_spawnBoxes[0].x + i * distance, 0.0f, s_spawnBoxes[0].z + j * distance);
                }
            }
        }
        /// <summary>Starts the wave spawn unless one is already running.</summary>
        public static void BeginWave()
        {
            var plugin = CombatExtenderPlugin.Instance;
            if (plugin == null)
            {
                Log.Warn("ArenaWaveSpawner: plugin not ready, wave not started");
                return;
            }
            if (s_running)
            {
                Log.Warn("ArenaWaveSpawner: a wave is already spawning, ignored");
                return;
            }

            plugin.StartCoroutine(SpawnWave());
        }

        private static void SpawnEnemy(Vector3 position)
        {
            GameObject gameObject = Game.ResourceManager.LoadAsset("lax01_cre_razorback_piglet", Game.AssetBundleHint.Characters, instantiate: false) as GameObject;
            Vector3 sourcePosition = position;
            NavMeshHit hit = default;
      		if (NavMeshUtility.SamplePosition(sourcePosition, out hit, 100f, NavMeshUtility.WalkableAreasMask))
      		{
     			sourcePosition = hit.position;
      		}
            sourcePosition = new Vector3(sourcePosition.x, sourcePosition.y, sourcePosition.z);
            // Modify some of game object characteristics
            AIController ai = gameObject.GetComponent<AIController>();
            if (ai != null)
                ai.MustDieForCombatToEnd = true;
            // CharacterStats stat = gameObject.GetComponent<CharacterStats>();
            // if (stat != null)
            // {
            //     stat.CharacterStatsData.
            // }
            ResourceManager.InstantiateObject<GameObject>(gameObject, sourcePosition, Quaternion.identity);
        }

        private static IEnumerator SpawnWave()
        {
            s_running = true;
            Log.Info("ArenaWaveSpawner: wave started");
            try
            {
                GlobalVariables gvs = SingletonBehavior<GlobalVariables>.Instance;
                 var isTransitionToCustomArena = gvs.GetVariable("b_custom_arena_in_location");
                WorldMapEncounterGameData encounterData = SingletonBehavior<RandomEncounterManager>.Instance.ActiveEncounter;
                RootObjectContainer rootObjectContainer = RootObjectContainer.GetRootObjectContainer(RootObjectContainer.ContainerType.LocalScene);
                if (
                    encounterData == null || encounterData.DebugName != "RE_Custom_Encounter"
                    || rootObjectContainer == null || rootObjectContainer.SceneName != "rm_lax01_arena_standard_01"
                    || isTransitionToCustomArena != 1
                )
                    yield return null;
                // Called from inside GlobalVariables.SetVariable: wait a frame so the
                // spawning doesn't run in the middle of the variable change.
                ArenaWaveSpawner.PopulatePoints(5);
                // Pick a random spawn point for each enemy.
                int pointIndex = Random.Range(0, s_spawnPoints.Count);
                Vector3 spawnPosition = s_spawnPoints[pointIndex];
                Log.Info($"ArenaWaveSpawner: spawning enemy at {spawnPosition}");
                ArenaWaveSpawner.SpawnEnemy(spawnPosition);
                List<int> usedIndices = new List<int> { pointIndex };
                usedIndices.Add(pointIndex);

                // var root = RootObjectContainer.GetRootObjectContainerForScene(RootObjectContainer.ContainerType.LocalScene, "ar_2102_prologue_berath");
                // var src = root.transform.GetChild(1).GetChild(9).GetChild(2).gameObject;
                // var vault = new GameObject("ArenaTemplates");
                // vault.SetActive(false);
                // Object.DontDestroyOnLoad(vault);
                // var t = Object.Instantiate(src, vault.transform); // never wakes up
                // t.name = "Custom_Switch_Steward";

                gvs.SetVariable("b_custom_arena_begin_wave", 0, ExceptionMode.Catch, false);
                yield return null;
                // TODO: spawn the enemies here. Use `yield return null;` to continue on the
                // next frame, or `yield return new WaitForSeconds(x);` to wait.
            }
            finally
            {
                s_running = false;
                Log.Info("ArenaWaveSpawner: wave finished");
            }
        }
    }
}
