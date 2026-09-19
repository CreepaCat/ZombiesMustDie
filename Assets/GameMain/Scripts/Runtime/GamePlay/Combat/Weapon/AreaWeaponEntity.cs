using System.Collections.Generic;
using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>目标位置瞬时范围攻击；以 CombatTarget 去重多碰撞体。</summary>
    public sealed class AreaWeaponEntity : WeaponEntity
    {
        private readonly HashSet<CombatTarget> damaged = new HashSet<CombatTarget>();

        protected override bool OnAttack(in AttackRequest request)
        {
            if (!request.HasAimPoint || WeaponData.AreaRadius <= 0f) return false;
            damaged.Clear();
            foreach (Collider hit in Physics.OverlapSphere(request.AimPoint, WeaponData.AreaRadius,
                ~0, QueryTriggerInteraction.Collide))
            {
                CombatTarget target = hit.GetComponentInParent<CombatTarget>();
                if (target == null || !target.isActiveAndEnabled || target.IsDead ||
                    target.Faction != CombatFaction.Enemy || !damaged.Add(target)) continue;
                DealDamage(target, hit.ClosestPoint(request.AimPoint), target.transform.position - request.AimPoint);
            }
            return true;
        }
    }
}
