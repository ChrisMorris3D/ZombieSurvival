using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrispyCube
{
    public abstract class EnemyBase : MonoBehaviour
    {
        public Animator anim;

        [Header("CONSTANTS")]
        public FloatReference AttackDamageConstant;
        public FloatReference HealthConstant;
        public FloatReference SpeedConstant;

        public FloatReference TriggerRadius;
        public FloatReference AttackRadius;

        [Header("COLLIDERS")]
        public CapsuleCollider attackCollider;
        public CapsuleCollider triggerCollider;

        [Header("AUDIO")]
        public AudioSource audioSource;
        public AudioClip attackAudio;
        public AudioClip damageAudio;
        public AudioClip deathAudio;

        [Header("GIZMOS")]
        public bool drawGizmos;
        public GizmoColors gizmoColor;

        bool attacking;
        bool chasing;

        float currentSpeed;
        float currentAttackDamage;

        Transform playerTransform;

        public float CurrentSpeed => currentSpeed;
        public float CurrentAttackDamage => currentAttackDamage;
        protected bool IsAttacking => attacking;
        protected bool IsChasing => chasing;
        protected Transform PlayerTransform => playerTransform;

        protected virtual void Start()
        {
            InitializeDynamicStats();
            InitializeColliders();
            InitializePlayer();
        }

        protected virtual void InitializeDynamicStats()
        {
            currentSpeed = SpeedConstant.Value;
            currentAttackDamage = AttackDamageConstant.Value;
        }

        protected virtual void InitializeColliders()
        {
            attackCollider.radius = AttackRadius.Value;
            triggerCollider.radius = TriggerRadius.Value;
        }

        protected virtual void InitializePlayer()
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            playerTransform = player != null ? player.transform : null;
        }

        public virtual void Attack()
        {
            if (attacking)
            {
                return;
            }

            StartCoroutine(AttackPlayerLoop());
        }

        public virtual void TakeDamage()
        {
            audioSource.PlayOneShot(damageAudio);
        }

        public virtual void Die()
        {
            audioSource.PlayOneShot(deathAudio);
        }

        public virtual void ToggleChasing(bool active)
        {
            if (active == chasing)
            {
                return;
            }

            chasing = active;
            if (chasing)
            {
                StartCoroutine(ChasePlayerLoop());
            }
        }

        public virtual void SetDynamicSpeed(float newSpeed)
        {
            currentSpeed = Mathf.Max(0f, newSpeed);
        }

        protected virtual void MoveTowardPlayer()
        {
            if (playerTransform == null)
            {
                return;
            }

            Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            float distance = Vector3.Distance(targetPosition, transform.position);

            if (distance > AttackRadius.Value)
            {
                transform.LookAt(targetPosition);
                transform.position += transform.forward * currentSpeed * Time.deltaTime;
            }
        }

        protected virtual IEnumerator ChasePlayerLoop()
        {
            while (chasing)
            {
                MoveTowardPlayer();
                yield return null;
            }
        }

        protected virtual IEnumerator AttackPlayerLoop()
        {
            attacking = true;
            audioSource.PlayOneShot(attackAudio);
            anim.SetTrigger("Attack");

            while (attacking)
            {
                yield return new WaitForSeconds(1);
                attacking = false;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Handles.color = attacking ? gizmoColor.attackActiveColor : gizmoColor.attackInactiveColor;
            Handles.DrawWireDisc(transform.position, Vector3.up, AttackRadius.Value);

            Handles.color = chasing ? gizmoColor.chasingActiveColor : gizmoColor.chasingInactiveColor;
            Handles.DrawWireDisc(transform.position, Vector3.up, TriggerRadius.Value);
        }
#endif
    }
}
