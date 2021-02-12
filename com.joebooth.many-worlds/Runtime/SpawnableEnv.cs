using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManyWorlds
{
    public class SpawnableEnv: MonoBehaviour
    {
        [Header("Single-World mode vs Many-Worlds mode")]
        [Tooltip(
            "With Many-Worlds, each environment spawns "+
            "in a unique physics scene.\nWhen disabled, all environments "+
            "are spawned in a single world/physics scense. This is more "+
            "performant but means your ml-agents environment must be robust "+
            "to spawning in different locations.")]
        public bool UseManyWorlds;
        
        [Header("Padding for Single-World mode")]
        [Tooltip("How much padding bettween spawned environments as a multiple of the envionment size (i.e. 1 = a gap of one envionment.")]
        public float paddingBetweenEnvs = 1f;

        [HideInInspector]
        public Bounds bounds;

        Scene _spawnedScene;
        PhysicsScene _spawnedPhysicsScene;

        private void FixedUpdate()
        {
            if (UseManyWorlds)
                _spawnedPhysicsScene.Simulate(Time.fixedDeltaTime);
        }

        public void UpdateBounds()
        {
            bounds.size = Vector3.zero; // reset
            foreach (BoxCollider col in GetComponentsInChildren<BoxCollider>())
            {
                var b = new Bounds();
                b.center = col.transform.position;
                b.size = new Vector3(
                    col.size.x * col.transform.lossyScale.x,
                    col.size.y * col.transform.lossyScale.y,
                    col.size.z * col.transform.lossyScale.z);
                bounds.Encapsulate(b);
            }
            TerrainCollider[] terrainColliders = GetComponentsInChildren<TerrainCollider>();
            foreach (TerrainCollider col in terrainColliders)
            {
                var b = new Bounds();
                b.center = col.transform.position + (col.terrainData.size/2);
                b.size =  col.terrainData.size;
                bounds.Encapsulate(b);
            }
        }
        public bool IsPointWithinBoundsInWorldSpace(Vector3 point)
        {
            // lazy initialize the Bounds; handles case where Factory was not used
            if (bounds.size == Vector3.zero)
                UpdateBounds();

            var boundsInWorldSpace = new Bounds(
                bounds.center + transform.position,
                bounds.size
            );
            bool isInBounds = boundsInWorldSpace.Contains(point);
            return isInBounds;
        }

        public void SetSceneAndPhysicsScene(Scene spawnedScene, PhysicsScene spawnedPhysicsScene)
        {
            _spawnedScene = spawnedScene;
            _spawnedPhysicsScene = spawnedPhysicsScene;
        }
        public PhysicsScene GetPhysicsScene()
        {
            return _spawnedPhysicsScene != null ? _spawnedPhysicsScene : Physics.defaultPhysicsScene;
        }
        public static void TriggerPhysicsStep()
        {
            var uniquePhysicsEnvs = FindObjectsOfType<SpawnableEnv>()
                .Where(x=>x.UseManyWorlds)
                .ToList();
            foreach (var env in uniquePhysicsEnvs)
            {
                env._spawnedPhysicsScene.Simulate(Time.fixedDeltaTime);
            }
        }
    }
}