using UnityEngine;

namespace ZombiesMustDie
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator m_Animator;

        public bool IsInteracting => m_Animator.GetBool(PlayerAnimationParamConfig.Param_IsInteracting);

        void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
        }


        public bool PlayTargetAnimation(string targetAnimation, bool isInteracting = false)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(PlayerAnimationParamConfig.Param_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimation, 0.2f);
            return true;
        }

        public bool PlayTargetAnimation(int targetAnimationID, bool isInteracting = false)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(PlayerAnimationParamConfig.Param_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimationID, 0.2f);
            return true;
        }
    }
}
