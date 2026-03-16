using System.Collections;
using UnityEngine;

namespace CrispyCube
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField] GameObject enemyPrefab;
        [SerializeField] Transform playerTarget;
        [SerializeField] Transform spawnParent;

        [Header("SPAWN COUNT")]
        [SerializeField] int minEnemiesPerWave = 1;
        [SerializeField] int maxEnemiesPerWave = 3;

        [Header("SPAWN DISTANCE")]
        [SerializeField] float minSpawnDistance = 10f;
        [SerializeField] float maxSpawnDistance = 20f;
        [SerializeField] float spawnHeightOffset = 0f;

        [Header("WAVE TIMER")]
        [SerializeField] float minSecondsBetweenWaves = 3f;
        [SerializeField] float maxSecondsBetweenWaves = 8f;
        [SerializeField] bool spawnOnStart = true;

        Coroutine spawnLoopRoutine;

        void Start()
        {
            if (playerTarget == null)
            {
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null)
                {
                    playerTarget = player.transform;
                }
            }

            if (spawnOnStart)
            {
                BeginSpawning();
            }
        }

        public void BeginSpawning()
        {
            if (spawnLoopRoutine != null)
            {
                return;
            }

            spawnLoopRoutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            if (spawnLoopRoutine == null)
            {
                return;
            }

            StopCoroutine(spawnLoopRoutine);
            spawnLoopRoutine = null;
        }

        public void SpawnWave()
        {
            if (enemyPrefab == null || playerTarget == null)
            {
                return;
            }

            int enemiesToSpawn = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                Vector3 spawnPosition = GetSpawnPosition();
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, spawnParent);
            }
        }

        IEnumerator SpawnLoop()
        {
            while (true)
            {
                if (enemyPrefab != null && playerTarget != null)
                {
                    SpawnWave();
                }

                float waitTime = Random.Range(minSecondsBetweenWaves, maxSecondsBetweenWaves);
                yield return new WaitForSeconds(waitTime);
            }
        }

        Vector3 GetSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            if (randomCircle == Vector2.zero)
            {
                randomCircle = Vector2.right;
            }

            float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnDistance;

            Vector3 spawnPosition = playerTarget.position + offset;
            spawnPosition.y = playerTarget.position.y + spawnHeightOffset;

            return spawnPosition;
        }

        void OnValidate()
        {
            minEnemiesPerWave = Mathf.Max(0, minEnemiesPerWave);
            maxEnemiesPerWave = Mathf.Max(minEnemiesPerWave, maxEnemiesPerWave);

            minSpawnDistance = Mathf.Max(0f, minSpawnDistance);
            maxSpawnDistance = Mathf.Max(minSpawnDistance, maxSpawnDistance);

            minSecondsBetweenWaves = Mathf.Max(0f, minSecondsBetweenWaves);
            maxSecondsBetweenWaves = Mathf.Max(minSecondsBetweenWaves, maxSecondsBetweenWaves);
        }

        void OnDrawGizmosSelected()
        {
            Transform target = playerTarget != null ? playerTarget : transform;

            Gizmos.color = new Color(1f, 0.75f, 0f, 0.25f);
            Gizmos.DrawWireSphere(target.position, minSpawnDistance);

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(target.position, maxSpawnDistance);
        }
    }
}
