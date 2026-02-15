using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace CrispyCube
{
    public class Enemy : MonoBehaviour
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


        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        bool attacking;
        bool chasing;

        float dynamicSpeed;
        float dynamicAttackDamage;

        Transform playerTransform;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        void Start()
        {
            InitializeDynamicStats();
            InitializeColliders();
            InitializePlayer();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        void InitializeDynamicStats()
        {
            dynamicSpeed = SpeedConstant.Value;
            dynamicAttackDamage = AttackDamageConstant.Value;
        }

        void InitializeColliders()
        {
            attackCollider.radius = AttackRadius.Value;
            triggerCollider.radius = TriggerRadius.Value;
        }

        void InitializePlayer()
        {
            playerTransform = FindAnyObjectByType<PlayerMovement>().transform;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void Attack()
        {
            StartCoroutine(AttackPlayerLoop());
        }

        public void TakeDamage()
        {
            audioSource.PlayOneShot(damageAudio);
        }

        public void Die()
        {
            audioSource.PlayOneShot(deathAudio);
        }

        public void ToggleChasing(bool active)
        {
            if (active)
            {
                chasing = true;
                StartCoroutine(ChasePlayerLoop());
            }
            else
            {
                chasing = false;
            }
        }

        void MoveTowardPlayer()
        {
            Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            float distance = Vector3.Distance(targetPosition, this.transform.position);
            Debug.Log("Distance = " + distance);

            if (distance > AttackRadius.Value)
            {
                transform.LookAt(targetPosition);
                transform.position += transform.forward * dynamicSpeed * Time.deltaTime;
            }
            else
            {

            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        IEnumerator ChasePlayerLoop()
        {
            Debug.Log("Chase Player: Start");
            while (chasing)
            {
                MoveTowardPlayer();
                yield return null;
            }

            Debug.Log("Chase Player: End");
            yield return null;
        }

        IEnumerator AttackPlayerLoop()
        {
            attacking = true;
            audioSource.PlayOneShot(attackAudio);
            Debug.Log("Attack Player: Start");
            anim.SetTrigger("Attack");

            while (attacking)
            {
                yield return new WaitForSeconds(1);
                Debug.Log("Attack Player: End");
                attacking = false;
            }
            yield return null;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                if (attacking)
                {
                    Handles.color = gizmoColor.attackActiveColor;
                }
                else
                {
                    Handles.color = gizmoColor.attackInactiveColor;
                }
                Handles.DrawWireDisc(transform.position, Vector3.up, AttackRadius.Value);

                if (chasing)
                {
                    Handles.color = gizmoColor.chasingActiveColor;
                }
                else
                {
                    Handles.color = gizmoColor.chasingInactiveColor;
                }
                Handles.DrawWireDisc(transform.position, Vector3.up, TriggerRadius.Value);
            }
        }
    }
}
