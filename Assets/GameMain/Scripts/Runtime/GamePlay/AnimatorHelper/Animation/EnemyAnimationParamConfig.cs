using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 敌人动画控制器参数与动画片段StringToHash配置
    /// </summary>
    public static class EnemyAnimationParamConfig
    {
        public static readonly int Param_Speed = Animator.StringToHash("Speed");
        public static readonly int Param_IsInteracting = Animator.StringToHash("IsInteracting");


        public static readonly int Clip_Death = Animator.StringToHash("Death");
        public static readonly int Clip_Attack01 = Animator.StringToHash("Attack_01");
        public static readonly int Clip_Attack02 = Animator.StringToHash("Attack_02");
    }
}
