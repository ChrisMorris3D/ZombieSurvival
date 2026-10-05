using UnityEngine;

namespace CrispyCube
{
    public class EnemyAnimationStateRelay : StateMachineBehaviour
    {
        [SerializeField] EnemyState completedState;
        [SerializeField] bool completeAtNormalizedTime;

        bool completionSent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            completionSent = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (completeAtNormalizedTime && stateInfo.normalizedTime >= 1f)
            {
                NotifyCompletion(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!completeAtNormalizedTime)
            {
                AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
                if (animator.IsInTransition(layerIndex) && nextState.fullPathHash == stateInfo.fullPathHash)
                {
                    return;
                }

                NotifyCompletion(animator);
            }
        }

        void NotifyCompletion(Animator animator)
        {
            if (completionSent)
            {
                return;
            }

            completionSent = true;
            EnemyBase enemy = animator.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.OnAnimationStateComplete(completedState);
            }
        }
    }
}
