using UnityEngine;

namespace ZombiesMustDie
{
    public static class PlayerAnimationParamConfig
    {
        public static readonly int Param_SpeedX = Animator.StringToHash("SpeedX");
        public static readonly int Param_SpeedZ = Animator.StringToHash("SpeedZ");
        public static readonly int Param_Velocity = Animator.StringToHash("Velocity");
        public static readonly int Param_IsInteracting = Animator.StringToHash("IsInteracting");


        public static readonly int Clip_Death = Animator.StringToHash("Death");
        public static readonly int Clip_Roll = Animator.StringToHash("Roll");
        public static readonly int Clip_Fire = Animator.StringToHash("Fire");
    }
}
