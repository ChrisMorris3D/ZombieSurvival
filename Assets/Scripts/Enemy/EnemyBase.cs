using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrispyCube
{
    public enum EnemyState
    {
        Spawning,
        Idle,
        Chasing,
        Attacking,
        ReceivingHit,
        Dying
    }

    public abstract class EnemyBase : MonoBehaviour
    {
        public Animator anim;

        [Header("CONSTANTS")]
        public FloatReference AttackDamageConstant;
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

        [Header("SPAWN FX")]
        [SerializeField] ParticleSystem spawnEffectPrefab;
        [SerializeField, Min(0.01f)] float spawnEffectDuration = 0.75f;

        [Header("STATE MACHINE")]
        [SerializeField] float attackFallbackDuration = 3f;
        [SerializeField] float receiveHitFallbackDuration = 2.5f;
        [SerializeField] float deathFallbackDuration = 3.5f;

        [Header("GIZMOS")]
        public bool drawGizmos;
        [SerializeField] bool drawDebugLabel;
        public GizmoColors gizmoColor;

        static readonly int AttackTrigger = Animator.StringToHash("Attack");
        static readonly int ReceiveHitTrigger = Animator.StringToHash("ReceiveHit");
        static readonly int DeathTrigger = Animator.StringToHash("Death");

        EnemyState currentState;
        bool stateInitialized;
        Coroutine stateTimeoutRoutine;
        bool playerInDetectionRange;
        bool playerInAttackRange;
        float currentSpeed;
        float currentAttackDamage;

        Transform playerTransform;

        public EnemyState CurrentState => currentState;
        public bool CanAttack => currentState == EnemyState.Idle || currentState == EnemyState.Chasing;
        public float CurrentSpeed => currentSpeed;
        public float CurrentAttackDamage => currentAttackDamage;
        protected Transform PlayerTransform => playerTransform;

        protected virtual void Start()
        {
            InitializeDynamicStats();
            InitializeColliders();
            InitializePlayer();
            PlaySpawnEffect();
            ChangeState(EnemyState.Spawning);
        }

        protected virtual void Update()
        {
            if (currentState == EnemyState.Chasing)
            {
                MoveTowardPlayer();
            }
        }

        protected virtual void InitializeDynamicStats()
        {
            currentSpeed = SpeedConstant.Value;
            currentAttackDamage = AttackDamageConstant.Value;
        }

        protected virtual void InitializeColliders()
        {
            if (attackCollider != null)
            {
                attackCollider.radius = AttackRadius.Value;
            }

            if (triggerCollider != null)
            {
                triggerCollider.radius = TriggerRadius.Value;
            }
        }

        protected virtual void InitializePlayer()
        {
            ThirdPersonPlayerMovement player = FindAnyObjectByType<ThirdPersonPlayerMovement>();
            playerTransform = player != null ? player.transform : null;
        }

        public virtual void Attack()
        {
            if (currentState == EnemyState.Spawning || currentState == EnemyState.ReceivingHit ||
                currentState == EnemyState.Attacking || currentState == EnemyState.Dying)
            {
                return;
            }

            ChangeState(EnemyState.Attacking);
        }

        public virtual void ReceiveHit()
        {
            if (currentState == EnemyState.Spawning || currentState == EnemyState.Dying)
            {
                return;
            }

            ChangeState(EnemyState.ReceivingHit, true);
        }

        public virtual void TakeDamage()
        {
            ReceiveHit();
        }

        public virtual void Die()
        {
            ChangeState(EnemyState.Dying);
        }

        public virtual void ToggleChasing(bool active)
        {
            playerInDetectionRange = active;

            if (currentState == EnemyState.Idle && active)
            {
                ChangeState(EnemyState.Chasing);
            }
            else if (currentState == EnemyState.Chasing && !active)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        public virtual void ToggleAttackRange(bool active)
        {
            playerInAttackRange = active;
        }

        public virtual void SetDynamicSpeed(float newSpeed)
        {
            currentSpeed = Mathf.Max(0f, newSpeed);
        }

        public void OnAnimationStateComplete(EnemyState completedState)
        {
            if (completedState == currentState)
            {
                CompleteCurrentState();
            }
        }

        public void OnSpawnAnimationComplete()
        {
            if (currentState == EnemyState.Spawning)
            {
                CompleteCurrentState();
            }
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

        void ChangeState(EnemyState nextState, bool restart = false)
        {
            if ((stateInitialized && currentState == EnemyState.Dying) ||
                (stateInitialized && !restart && currentState == nextState))
            {
                return;
            }

            if (stateTimeoutRoutine != null)
            {
                StopCoroutine(stateTimeoutRoutine);
                stateTimeoutRoutine = null;
            }

            currentState = nextState;
            stateInitialized = true;

            switch (currentState)
            {
                case EnemyState.Spawning:
                    break;
                case EnemyState.Idle:
                case EnemyState.Chasing:
                    break;
                case EnemyState.Attacking:
                    PlayOneShot(attackAudio);
                    SetAnimatorTrigger(AttackTrigger);
                    StartStateTimeout(attackFallbackDuration);
                    break;
                case EnemyState.ReceivingHit:
                    PlayOneShot(damageAudio);
                    ResetAnimatorTrigger(AttackTrigger);
                    SetAnimatorTrigger(ReceiveHitTrigger);
                    StartStateTimeout(receiveHitFallbackDuration);
                    break;
                case EnemyState.Dying:
                    EnterDyingState();
                    break;
            }
        }

        void EnterDyingState()
        {
            currentSpeed = 0f;
            playerInDetectionRange = false;
            playerInAttackRange = false;

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            ResetAnimatorTrigger(AttackTrigger);
            ResetAnimatorTrigger(ReceiveHitTrigger);
            PlayOneShot(deathAudio);
            SetAnimatorTrigger(DeathTrigger);
            StartStateTimeout(deathFallbackDuration);
        }

        void CompleteCurrentState()
        {
            if (stateTimeoutRoutine != null)
            {
                StopCoroutine(stateTimeoutRoutine);
                stateTimeoutRoutine = null;
            }

            if (currentState == EnemyState.Dying)
            {
                Destroy(gameObject);
                return;
            }

            if (currentState == EnemyState.Spawning)
            {
                ChangeState(EnemyState.Chasing);
            }
            else if (currentState == EnemyState.Attacking || currentState == EnemyState.ReceivingHit)
            {
                ChangeState(playerInDetectionRange ? EnemyState.Chasing : EnemyState.Idle);
            }
        }

        void StartStateTimeout(float duration)
        {
            stateTimeoutRoutine = StartCoroutine(StateTimeout(currentState, Mathf.Max(0f, duration)));
        }

        IEnumerator StateTimeout(EnemyState expectedState, float duration)
        {
            yield return new WaitForSeconds(duration);
            stateTimeoutRoutine = null;

            if (currentState == expectedState)
            {
                CompleteCurrentState();
            }
        }

        void SetAnimatorTrigger(int trigger)
        {
            if (anim != null)
            {
                anim.SetTrigger(trigger);
            }
        }

        void ResetAnimatorTrigger(int trigger)
        {
            if (anim != null)
            {
                anim.ResetTrigger(trigger);
            }
        }

        void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        void PlaySpawnEffect()
        {
            if (spawnEffectPrefab == null)
            {
                return;
            }

            ParticleSystem spawnEffect = Instantiate(
                spawnEffectPrefab,
                transform.position,
                Quaternion.identity);

            spawnEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem[] particleSystems = spawnEffect.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = particleSystems[i].main;
                main.loop = false;
                main.duration = spawnEffectDuration;
            }

            ParticleSystem.MainModule rootMain = spawnEffect.main;
            rootMain.stopAction = ParticleSystemStopAction.Destroy;
            spawnEffect.Play(true);
        }

        void OnValidate()
        {
            attackFallbackDuration = Mathf.Max(0f, attackFallbackDuration);
            receiveHitFallbackDuration = Mathf.Max(0f, receiveHitFallbackDuration);
            deathFallbackDuration = Mathf.Max(0f, deathFallbackDuration);
            spawnEffectDuration = Mathf.Max(0.01f, spawnEffectDuration);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                bool attacking = currentState == EnemyState.Attacking || playerInAttackRange;
                Handles.color = attacking ? gizmoColor.attackActiveColor : gizmoColor.attackInactiveColor;
                Handles.DrawWireDisc(transform.position, Vector3.up, AttackRadius.Value);

                bool chasing = currentState == EnemyState.Chasing || playerInDetectionRange;
                Handles.color = chasing ? gizmoColor.chasingActiveColor : gizmoColor.chasingInactiveColor;
                Handles.DrawWireDisc(transform.position, Vector3.up, TriggerRadius.Value);
            }

            if (drawDebugLabel)
            {
                EnemyHealthController health = GetComponent<EnemyHealthController>();
                float currentHealth = health != null
                    ? (Application.isPlaying ? health.CurrentHealth : health.MaxHealth)
                    : 0f;
                float maxHealth = health != null ? health.MaxHealth : 0f;

                Handles.Label(
                    transform.position + Vector3.up * 2.25f,
                    $"State: {currentState}\nHealth: {currentHealth:0.##} / {maxHealth:0.##}");
            }
        }
#endif
    }
}
