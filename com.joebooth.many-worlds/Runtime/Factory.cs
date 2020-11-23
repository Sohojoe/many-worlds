using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManyWorlds
{
    /// <summary>
    /// World holds references to envIds, prefabs, brains for spawning environments.
    /// </summary>
    [System.Serializable]
    public class Factory
    {
        [System.Serializable]
        public class SpawnableEnvDefinition
        {
            public string envId;
            public SpawnableEnv envPrefab;
        }

        [SerializeField]
        public List<SpawnableEnvDefinition> spawnableEnvDefinitions = new List<SpawnableEnvDefinition>();

        /// <summary>
        /// The number of SpawnableEnvs inside the World.
        /// </summary>
        public int Count
        {
            get { return spawnableEnvDefinitions.Count; }
        }

        [Tooltip("The envId to spawn if not overriden from python")]
        public string envIdDefault;
        [Tooltip("The number of environments to spawn in Training Mode if not overriden from python")]
        public int trainingNumEnvsDefault = 16;
        [Tooltip("The number of environments to spawn in Inference Mode if not overriden from python")]
        public int inferenceNumEnvsDefault = 3;

        /// <summary>
        /// Return prefab for this EnvId else null
        /// </summary>
        public SpawnableEnv GetPrefabFor(string thisEnvId)
        {
            var entry = spawnableEnvDefinitions
                .FirstOrDefault(x=>x.envId==thisEnvId);
            return entry?.envPrefab; 
        }

        /// <summary>
        /// Spawn a number of environments. The enviromentment must include SpawnableEnv
        /// </summary>
        public void SpawnSpawnableEnv(GameObject parent, int numInstances, SpawnableEnv envPrefab)
        {
            CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);

            Vector3 spawnStartPos = parent.transform.position;
            SpawnableEnv spawnableEnv = envPrefab.GetComponent<SpawnableEnv>();
            spawnableEnv.UpdateBounds();
            Vector3 step = new Vector3(0f, 0f, spawnableEnv.bounds.size.z + (spawnableEnv.bounds.size.z*spawnableEnv.paddingBetweenEnvs));
            if (spawnableEnv.UseManyWorlds)
                step = Vector3.zero;

            for (int i = 0; i < numInstances; i++)
            {
                var env = Agent.Instantiate(envPrefab, spawnStartPos, envPrefab.gameObject.transform.rotation);
                spawnStartPos += step;
                if (spawnableEnv.UseManyWorlds)
                {
                    Scene scene = SceneManager.CreateScene($"SpawnedEnv-{i}", csp);
                    PhysicsScene physicsScene = scene.GetPhysicsScene();
                    SceneManager.MoveGameObjectToScene(env.gameObject, scene);
                    SpawnableEnv spawnedEnv = env.GetComponent<SpawnableEnv>();
                    spawnedEnv.SetSceneAndPhysicsScene(scene, physicsScene);
                    // only render the 1st scene
                    if (i == 0) {
                        // var cam = Camera.FindObjectOfType<Camera>();
                        // cam.scene = scene;
                        // // Camera.main.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Removes all the Brains of the BroadcastHub
        /// </summary>
        public void Clear()
        {
            spawnableEnvDefinitions.Clear();
        }
    }
}
