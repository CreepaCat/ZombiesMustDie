using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 创建近战武器实体时传入的范围检测配置。
    /// </summary>
    public sealed class MeleeWeaponEntityData : WeaponEntityData
    {
        public MeleeWeaponEntityData(
            int entityId,
            int entityTypeId,
            int weaponId,
            CombatController owner,
            float attackRadius,
            LayerMask targetLayer,
            string attackOriginPath = null)
            : base(entityId, entityTypeId, weaponId, owner)
        {
            AttackRadius = Mathf.Max(0f, attackRadius);
            TargetLayer = targetLayer;
            AttackOriginPath = attackOriginPath;
        }

        public float AttackRadius { get; }
        public LayerMask TargetLayer { get; }
        public string AttackOriginPath { get; }
    }

    /// <summary>
    /// 通过球形范围查询执行近战攻击，并保证同一次攻击只伤害每个目标一次。
    /// </summary>
    public sealed class MeleeWeaponEntity : WeaponEntity
    {
        private const int HitBufferSize = 32;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<CombatTarget> hitTargets = new HashSet<CombatTarget>();

        private Transform attackOrigin;
        private float attackRadius;
        private LayerMask targetLayer;

        public int LastHitCount { get; private set; }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            MeleeWeaponEntityData entityData = userData as MeleeWeaponEntityData;
            if (entityData == null)
            {
                Log.Error("Melee weapon entity data is invalid.");
                return;
            }

            attackRadius = entityData.AttackRadius;
            targetLayer = entityData.TargetLayer;
            attackOrigin = ResolveChild(entityData.AttackOriginPath);
            LastHitCount = 0;
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            hitTargets.Clear();
            attackOrigin = null;
            attackRadius = 0f;
            targetLayer = default;
            LastHitCount = 0;

            base.OnHide(isShutdown, userData);
        }

        protected override bool OnAttack()
        {
            if (attackOrigin == null || attackRadius <= 0f)
            {
                return false;
            }

            hitTargets.Clear();
            LastHitCount = 0;

            int hitCount = Physics.OverlapSphereNonAlloc(
                attackOrigin.position,
                attackRadius,
                hitBuffer,
                targetLayer,
                QueryTriggerInteraction.UseGlobal);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hitBuffer[i];
                if (hitCollider == null)
                {
                    continue;
                }

                CombatTarget target = hitCollider.GetComponentInParent<CombatTarget>();
                if (target == null || target == Owner.Owner || !hitTargets.Add(target))
                {
                    continue;
                }

                Vector3 hitPoint = hitCollider.ClosestPoint(attackOrigin.position);
                Vector3 hitDirection = target.transform.position - Owner.transform.position;
                hitDirection.y = 0f;
                hitDirection = hitDirection.sqrMagnitude > 0f
                    ? hitDirection.normalized
                    : Owner.transform.forward;

                DamageResult result = DealDamage(target, hitPoint, hitDirection);
                if (result.Succeeded)
                {
                    LastHitCount++;
                }
            }

            return true;
        }

        private Transform ResolveChild(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return CachedTransform;
            }

            Transform child = CachedTransform.Find(relativePath);
            if (child == null)
            {
                Log.Warning("Can not find melee attack origin '{0}' on weapon '{1}'.", relativePath, Name);
                return CachedTransform;
            }

            return child;
        }
    }
}
