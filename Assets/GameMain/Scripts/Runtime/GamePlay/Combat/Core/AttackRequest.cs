using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>本次攻击的世界空间瞄准信息；default 使用武器自身朝向。</summary>
    public readonly struct AttackRequest
    {
        public readonly Vector3 AimPoint;
        public readonly Vector3 AimDirection;
        public readonly bool HasAimPoint;

        public AttackRequest(Vector3 aimPoint, Vector3 aimDirection, bool hasAimPoint = true)
        {
            AimPoint = aimPoint;
            AimDirection = aimDirection;
            HasAimPoint = hasAimPoint;
        }
    }
}
