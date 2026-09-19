using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 直线飞行子弹实体，使用 SphereCast 完成连续命中检测和穿透处理。
    /// </summary>
    public sealed class BulletEntity : Entity
    {
        private const int HitBufferSize = 16;

        private static readonly RaycastHitDistanceComparer HitComparer = new RaycastHitDistanceComparer();

        private RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private readonly HashSet<CombatTarget> hitTargets = new HashSet<CombatTarget>();

        private BulletEntityData bulletData;
        private Vector3 direction;
        private float elapsedLifetime;
        private int remainingPenetrations;
        private bool isHiding;

        public BulletEntityData BulletData => bulletData;
        public int RemainingPenetrations => remainingPenetrations;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            bulletData = userData as BulletEntityData;
            if (bulletData == null)
            {
                Log.Error("Bullet entity data is invalid.");
                HideBullet();
                return;
            }

            Vector3 localDirection = bulletData.Direction;
            direction = localDirection.sqrMagnitude > 0f
                ? (CachedTransform.parent != null
                    ? CachedTransform.parent.TransformDirection(localDirection).normalized
                    : localDirection.normalized)
                : CachedTransform.forward;
            elapsedLifetime = 0f;
            remainingPenetrations = bulletData.PenetrationCount;
            isHiding = false;
            hitTargets.Clear();
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            bulletData = null;
            direction = Vector3.zero;
            elapsedLifetime = 0f;
            remainingPenetrations = 0;
            isHiding = false;
            hitTargets.Clear();

            base.OnHide(isShutdown, userData);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (bulletData == null || isHiding)
            {
                return;
            }

            elapsedLifetime += elapseSeconds;
            if (elapsedLifetime >= bulletData.Lifetime)
            {
                HideBullet();
                return;
            }

            float moveDistance = bulletData.Speed * elapseSeconds;
            if (moveDistance > 0f)
            {
                MoveAndDetectHits(moveDistance);
            }
        }

        private void MoveAndDetectHits(float moveDistance)
        {
            Vector3 origin = CachedTransform.position;
            int hitCount;
            // Saturation otherwise permits a nearer wall to be omitted from the results.
            while ((hitCount = Physics.SphereCastNonAlloc(
                origin,
                bulletData.CollisionRadius,
                direction,
                hitBuffer,
                moveDistance,
                bulletData.HitLayer,
                bulletData.TriggerInteraction)) == hitBuffer.Length)
            {
                Array.Resize(ref hitBuffer, hitBuffer.Length * 2);
            }

            if (hitCount > 1)
            {
                Array.Sort(hitBuffer, 0, hitCount, HitComparer);
            }

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Collider hitCollider = hit.collider;
                if (hitCollider == null || ShouldIgnore(hitCollider))
                {
                    continue;
                }

                CombatTarget target = hitCollider.GetComponentInParent<CombatTarget>();
                if (target == null)
                {
                    CachedTransform.position = hit.point;
                    EffectSpawner.ShowBulletHit(hit.point, hit.normal);
                    HideBullet();
                    return;
                }

                if (!bulletData.SourceFaction.IsHostileTo(target.Faction) || !hitTargets.Add(target))
                {
                    continue;
                }

                DamageInfo damageInfo = new DamageInfo(
                    bulletData.Damage,
                    bulletData.Instigator,
                    bulletData.Source,
                    bulletData.SourceFaction,
                    hit.point,
                    direction);

                DamageResult result = target.TakeDamage(in damageInfo);
                Debug.Log(target.transform + " TakeDamage " + bulletData.Damage);
                if (!result.Succeeded)
                {
                    continue;
                }

                EffectSpawner.ShowBulletHit(hit.point, hit.normal);
                if (remainingPenetrations <= 0)
                {
                    CachedTransform.position = hit.point;
                    HideBullet();
                    return;
                }

                remainingPenetrations--;
            }

            CachedTransform.position = origin + direction * moveDistance;
        }

        private bool ShouldIgnore(Collider hitCollider)
        {
            Transform hitTransform = hitCollider.transform;
            return IsPartOf(hitTransform, bulletData.Instigator) ||
                   IsPartOf(hitTransform, bulletData.Source);
        }

        private static bool IsPartOf(Transform candidate, GameObject root)
        {
            return root != null &&
                   (candidate == root.transform || candidate.IsChildOf(root.transform));
        }

        private void HideBullet()
        {
            if (isHiding)
            {
                return;
            }

            isHiding = true;
            if (GameEntry.Entity != null && Entity != null)
            {
                GameEntry.Entity.HideEntity(Entity);
                return;
            }

            gameObject.SetActive(false);
        }

        private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
