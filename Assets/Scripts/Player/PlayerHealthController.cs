using UnityEngine;
using UnityEngine.Events;

namespace CrispyCube
{
    public class PlayerHealthController : MonoBehaviour
    {
        public FloatVariable PlayerHealth;
        public FloatVariable MaxHealth;

        public UnityEvent DamageEvent;
        public UnityEvent DeathEvent;

        void Start()
        {
            ResetHealthToMax();
        }

        void ResetHealthToMax()
        {
            PlayerHealth.Value = MaxHealth.Value;
        }

        public void TakeDamage(float damage)
        {
            PlayerHealth.Value -= damage;
        }

        void OnTriggerEnter(Collider other)
        {
            EnemyBase enemy = other.gameObject.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                if (other.gameObject.tag == "AttackTrigger")
                {
                    ReceiveDamage(enemy);
                    enemy.Attack();
                }

                if (other.gameObject.tag == "ActivationTrigger")
                {
                    enemy.ToggleChasing(true);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            EnemyBase enemy = other.gameObject.GetComponentInParent<EnemyBase>();
            if (enemy != null && other.gameObject.tag == "ActivationTrigger")
            {
                enemy.ToggleChasing(false);
            }
        }

        private void ReceiveDamage(EnemyBase enemy)
        {
            if (enemy != null)
            {
                PlayerHealth.ApplyChange(-enemy.CurrentAttackDamage);
                DamageEvent.Invoke();
            }

            if (PlayerHealth.Value <= 0.0f)
            {
                DeathEvent.Invoke();
            }
        }
    }
}
