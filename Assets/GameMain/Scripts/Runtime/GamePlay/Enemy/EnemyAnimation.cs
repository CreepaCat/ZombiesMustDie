using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 代替Animator来处理敌人的动画播放
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimation : MonoBehaviour
    {

        private Animator m_Animator;

        public bool IsInteracting => m_Animator.GetBool(EnemyAnimationParamConfig.Param_IsInteracting);

        void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
        }


        public bool PlayTargetAnimation(string targetAnimation, bool isInteracting)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(EnemyAnimationParamConfig.Param_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimation, 0.2f);
            return true;
        }

        public bool PlayTargetAnimation(int targetAnimationID, bool isInteracting)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(EnemyAnimationParamConfig.Param_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimationID, 0.2f);
            return true;
        }

    }
}
