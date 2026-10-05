using System;
using UnityEngine;

namespace CrispyCube
{
    [DisallowMultipleComponent]
    public class EnemyHealthController : MonoBehaviour
    {
        public static event Action Killed;

        [Header("HEALTH")]
        [SerializeField] FloatReference maxHealth = new FloatReference(1f);
        [SerializeField] float currentHealth;

        [Header("REFERENCES")]
        [SerializeField] EnemyBase enemy;

        bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth != null ? Mathf.Max(0f, maxHealth.Value) : 0f;
        public bool IsDead => isDead;

        void Awake()
        {
            if (enemy == null)
            {
                enemy = GetComponent<EnemyBase>();
            }

            currentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Clamp(currentHealth - damage, 0f, MaxHealth);
            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            if (enemy != null)
            {
                enemy.TakeDamage();
            }
        }

        public void Kill()
        {
            if (isDead)
            {
                return;
            }

            currentHealth = 0f;
            Die();
        }

        void Die()
        {
            isDead = true;
            Killed?.Invoke();

            if (enemy != null)
            {
                enemy.Die();
                return;
            }

            Destroy(gameObject);
        }
    }
}
