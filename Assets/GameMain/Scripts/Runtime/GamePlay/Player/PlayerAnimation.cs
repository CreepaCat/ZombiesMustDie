using UnityEngine;

namespace ZombiesMustDie
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator m_Animator;

        public bool IsInteracting => m_Animator.GetBool(PlayerAnimationParamConfig.Param_b_IsInteracting);

        void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
        }


        public bool PlayTargetAnimation(string targetAnimation, bool isInteracting = false)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(PlayerAnimationParamConfig.Param_b_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimation, 0.2f);
            return true;
        }

        public bool PlayTargetAnimation(int targetAnimationID, bool isInteracting = false)
        {
            if (IsInteracting) { return false; }
            m_Animator.SetBool(PlayerAnimationParamConfig.Param_b_IsInteracting, isInteracting);
            m_Animator.CrossFade(targetAnimationID, 0.2f);
            return true;
        }

        /// <summary>
        /// 播放玩家射击动画
        /// </summary>
        public void PlayFire()
        {
            if (IsInteracting) { return; }
            m_Animator.SetTrigger(PlayerAnimationParamConfig.Param_t_Fire);
            // m_Animator.CrossFade(, 0.2f);
        }


        /// <summary>
        /// 播放玩家换弹动画
        /// </summary>
        public void PlayReload()
        {
            //todo:换弹动画
            Debug.Log("玩家正在换弹");
        }
    }
}
