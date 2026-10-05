using System.Collections.Generic;
using UnityEngine;

namespace CrispyCube
{
    [CreateAssetMenu(fileName = "EnemyPrefabList", menuName = "Enemies/Enemy Prefab List")]
    public class EnemyPrefabList : ScriptableObject
    {
        [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();

        public bool TryGetRandomPrefab(out GameObject enemyPrefab)
        {
            enemyPrefab = null;
            int validPrefabCount = 0;

            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                GameObject candidate = enemyPrefabs[i];
                if (candidate == null)
                {
                    continue;
                }

                validPrefabCount++;
                if (Random.Range(0, validPrefabCount) == 0)
                {
                    enemyPrefab = candidate;
                }
            }

            return enemyPrefab != null;
        }
    }
}
