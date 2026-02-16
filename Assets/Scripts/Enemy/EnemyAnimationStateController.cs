using UnityEngine;

namespace CrispyCube
{
    [DisallowMultipleComponent]
    public class EnemyAnimationStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Enemy enemy;
        [SerializeField] Animator animator;

        [Header("Blend Tree")]
        [SerializeField] string speedParameter = "MoveSpeed";
        [SerializeField] bool normalizeToZeroOne = true;
        [SerializeField] float maxSpeedForBlend = 3f;
        [SerializeField] float parameterDampTime = 0.1f;
        [SerializeField] float movementThreshold = 0.02f;

        [Header("Variant Randomization")]
        [SerializeField] bool randomizeVariantsOnStart = true;
        [SerializeField] string idleIndexParameter = "IdleIndex";
        [SerializeField] int idleVariantCount = 4;
        [SerializeField] string walkingIndexParameter = "WalkingIndex";
        [SerializeField] int walkingVariantCount = 2;
        [SerializeField] string runningIndexParameter = "RunningIndex";
        [SerializeField] int runningVariantCount = 1;

        int speedParamHash;
        Vector3 lastPosition;

        void Reset()
        {
            enemy = GetComponent<Enemy>();
            animator = GetComponentInChildren<Animator>();
        }

        void Awake()
        {
            if (enemy == null)
            {
                enemy = GetComponent<Enemy>();
            }

            if (animator == null)
            {
                animator = (enemy != null && enemy.anim != null) ? enemy.anim : GetComponentInChildren<Animator>();
            }

            speedParamHash = Animator.StringToHash(speedParameter);
            lastPosition = transform.position;
        }

        void OnEnable()
        {
            lastPosition = transform.position;
            UpdateAnimatorSpeed(0f);
        }

        void Start()
        {
            if (!randomizeVariantsOnStart || animator == null)
            {
                return;
            }

            RandomizeVariantIndex(idleIndexParameter, idleVariantCount);
            RandomizeVariantIndex(walkingIndexParameter, walkingVariantCount);
            RandomizeVariantIndex(runningIndexParameter, runningVariantCount);
        }

        void LateUpdate()
        {
            if (enemy == null || animator == null)
            {
                return;
            }

            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            float worldSpeed = Time.deltaTime > 0f ? distanceMoved / Time.deltaTime : 0f;
            bool isMoving = worldSpeed > movementThreshold;

            float speedValue = isMoving ? enemy.CurrentSpeed : 0f;
            if (normalizeToZeroOne)
            {
                speedValue = maxSpeedForBlend > 0f ? Mathf.Clamp01(speedValue / maxSpeedForBlend) : 0f;
            }

            UpdateAnimatorSpeed(speedValue);
            lastPosition = transform.position;
        }

        void UpdateAnimatorSpeed(float speedValue)
        {
            if (parameterDampTime > 0f)
            {
                animator.SetFloat(speedParamHash, speedValue, parameterDampTime, Time.deltaTime);
            }
            else
            {
                animator.SetFloat(speedParamHash, speedValue);
            }
        }

        void RandomizeVariantIndex(string parameterName, int variantCount)
        {
            if (string.IsNullOrWhiteSpace(parameterName) || variantCount <= 0)
            {
                return;
            }

            if (!HasIntParameter(parameterName))
            {
                Debug.LogWarning($"Animator on '{name}' does not have int parameter '{parameterName}'.", this);
                return;
            }

            int randomIndex = variantCount > 1 ? Random.Range(0, variantCount) : 0;
            animator.SetInteger(parameterName, randomIndex);
        }

        bool HasIntParameter(string parameterName)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == AnimatorControllerParameterType.Int &&
                    parameters[i].name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        void OnValidate()
        {
            if (maxSpeedForBlend < 0f)
            {
                maxSpeedForBlend = 0f;
            }

            if (parameterDampTime < 0f)
            {
                parameterDampTime = 0f;
            }

            if (movementThreshold < 0f)
            {
                movementThreshold = 0f;
            }

            if (idleVariantCount < 1)
            {
                idleVariantCount = 1;
            }

            if (walkingVariantCount < 1)
            {
                walkingVariantCount = 1;
            }

            if (runningVariantCount < 1)
            {
                runningVariantCount = 1;
            }
        }
    }
}
