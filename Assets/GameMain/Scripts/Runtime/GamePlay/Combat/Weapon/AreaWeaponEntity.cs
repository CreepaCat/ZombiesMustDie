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
            //todo：播放音效 和 特效
            //播放开火音效
            GameEntry.Sound.PlaySound(WeaponData.SoundId);
            //播放攻击特效
            EffectSpawner.ShowAreaTowerAttack(Entity, transform.Find("Muzzle"));
            foreach (Collider hit in Physics.OverlapSphere(request.AimPoint, WeaponData.AreaRadius,
                ~0, QueryTriggerInteraction.Collide))
            {
                CombatTarget target = hit.GetComponentInParent<CombatTarget>();
                if (target == null || !target.isActiveAndEnabled || target.IsDead ||
                    target.Faction != CombatFaction.Enemy || !damaged.Add(target)) continue;
                //todo：对每个攻击到的对象，播放命中音效和特效
                Vector3 hitPoint = hit.ClosestPoint(request.AimPoint);
                Vector3 hitDirection = target.transform.position - request.AimPoint;
                DealDamage(target, hitPoint, hitDirection);

                EffectSpawner.ShowAreaTowerHit(hitPoint, hitDirection.normalized);
            }
            return true;
        }
    }
}
