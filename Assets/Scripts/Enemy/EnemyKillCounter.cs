using UnityEngine;

namespace CrispyCube
{
    [DisallowMultipleComponent]
    public class EnemyKillCounter : MonoBehaviour
    {
        [SerializeField] IntegerVariable killCount;

        void Awake()
        {
            if (killCount != null)
            {
                killCount.SetValue(0);
            }
        }

        void OnEnable()
        {
            EnemyHealthController.Killed += IncrementKillCount;
        }

        void OnDisable()
        {
            EnemyHealthController.Killed -= IncrementKillCount;
        }

        void IncrementKillCount()
        {
            if (killCount != null)
            {
                killCount.ApplyChange(1);
            }
        }
    }
}
