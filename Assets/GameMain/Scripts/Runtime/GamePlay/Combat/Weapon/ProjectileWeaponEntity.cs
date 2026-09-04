using System;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 创建远程武器实体时传入的子弹生成配置。
    /// </summary>
    public sealed class ProjectileWeaponEntityData : WeaponEntityData
    {
        public ProjectileWeaponEntityData(
            int entityId,
            int entityTypeId,
            int weaponId,
            CombatController owner,
            GameObject projectilePrefab,
            string muzzlePath = null)
            : base(entityId, entityTypeId, weaponId, owner)
        {
            ProjectilePrefab = projectilePrefab;
            MuzzlePath = muzzlePath;
        }

        public GameObject ProjectilePrefab { get; }
        public string MuzzlePath { get; }
    }

    /// <summary>
    /// 从枪口生成子弹，并使用 DRWeapon 的速度与散布数据设置初始运动。
    /// </summary>
    public sealed class ProjectileWeaponEntity : WeaponEntity
    {
        private Transform muzzle;
        private GameObject projectilePrefab;

        public event Action<GameObject, ProjectileWeaponEntity> ProjectileSpawned;

        public GameObject LastProjectile { get; private set; }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            ProjectileWeaponEntityData entityData = userData as ProjectileWeaponEntityData;
            if (entityData == null)
            {
                Log.Error("Projectile weapon entity data is invalid.");
                return;
            }

            projectilePrefab = entityData.ProjectilePrefab;
            muzzle = ResolveChild(entityData.MuzzlePath);
            LastProjectile = null;
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            muzzle = null;
            projectilePrefab = null;
            LastProjectile = null;
            ProjectileSpawned = null;

            base.OnHide(isShutdown, userData);
        }

        protected override bool OnAttack()
        {
            if (muzzle == null || projectilePrefab == null)
            {
                return false;
            }

            Quaternion shotRotation = GetShotRotation();
            GameObject projectile = Instantiate(projectilePrefab, muzzle.position, shotRotation);
            LastProjectile = projectile;

            Rigidbody projectileBody = projectile.GetComponent<Rigidbody>();
            if (projectileBody != null)
            {
                projectileBody.linearVelocity = shotRotation * Vector3.forward * WeaponData.BulletSpeed;
            }

            ProjectileSpawned?.Invoke(projectile, this);
            return true;
        }

        private Quaternion GetShotRotation()
        {
            float spreadAngle = Mathf.Max(0f, WeaponData.SpreadAngle);
            if (spreadAngle <= 0f)
            {
                return muzzle.rotation;
            }

            Vector2 spread = UnityEngine.Random.insideUnitCircle * spreadAngle;
            return muzzle.rotation * Quaternion.Euler(-spread.y, spread.x, 0f);
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
                Log.Warning("Can not find muzzle '{0}' on weapon '{1}'.", relativePath, Name);
                return CachedTransform;
            }

            return child;
        }
    }
}
