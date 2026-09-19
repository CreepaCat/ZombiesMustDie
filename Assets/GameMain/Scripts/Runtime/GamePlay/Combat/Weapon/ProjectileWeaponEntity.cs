using GameFramework.DataTable;
using GameFramework.Entity;
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
            string muzzlePath = null,
            string bulletEntityGroupName = "Bullet",
            float bulletLifetime = 5f,
            float bulletCollisionRadius = 0.05f,
            int bulletHitLayerMask = -1,
            DRWeapon weaponConfig = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide)
            : base(entityId, entityTypeId, weaponId, owner, weaponConfig)
        {
            MuzzlePath = muzzlePath;
            BulletEntityGroupName = bulletEntityGroupName;
            BulletLifetime = Mathf.Max(0.01f, bulletLifetime);
            BulletCollisionRadius = Mathf.Max(0.001f, bulletCollisionRadius);
            BulletHitLayer = bulletHitLayerMask;
            TriggerInteraction = triggerInteraction;
        }

        public string MuzzlePath { get; }
        public string BulletEntityGroupName { get; }
        public float BulletLifetime { get; }
        public float BulletCollisionRadius { get; }
        public LayerMask BulletHitLayer { get; }
        public QueryTriggerInteraction TriggerInteraction { get; }
    }

    /// <summary>
    /// 从枪口请求生成 BulletEntity，并使用 DRWeapon 构造其运行数据。
    /// </summary>
    public class ProjectileWeaponEntity : WeaponEntity
    {
        private static int nextBulletEntityId;

        private Transform muzzle;
        private string bulletEntityGroupName;
        private float bulletLifetime;
        private float bulletCollisionRadius;
        private LayerMask bulletHitLayer;
        private QueryTriggerInteraction triggerInteraction;

        public int LastBulletEntityId { get; private set; }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            ProjectileWeaponEntityData entityData = userData as ProjectileWeaponEntityData;
            if (entityData == null)
            {
                Log.Error("Projectile weapon entity data is invalid.");
                return;
            }

            muzzle = ResolveChild(entityData.MuzzlePath);
            bulletEntityGroupName = entityData.BulletEntityGroupName;
            bulletLifetime = entityData.BulletLifetime;
            bulletCollisionRadius = entityData.BulletCollisionRadius;
            bulletHitLayer = entityData.BulletHitLayer;
            triggerInteraction = entityData.TriggerInteraction;
            LastBulletEntityId = 0;
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            muzzle = null;
            bulletEntityGroupName = null;
            bulletLifetime = 0f;
            bulletCollisionRadius = 0f;
            bulletHitLayer = default;
            LastBulletEntityId = 0;

            base.OnHide(isShutdown, userData);
        }

        protected override bool OnAttack(in AttackRequest request)
        {
            if (muzzle == null || GameEntry.Entity == null || GameEntry.DataTable == null)
            {
                return false;
            }

            IEntityGroup bulletEntityGroup = string.IsNullOrEmpty(bulletEntityGroupName)
                ? null
                : GameEntry.Entity.GetEntityGroup(bulletEntityGroupName);
            Transform bulletParent = (bulletEntityGroup?.Helper as Component)?.transform;
            if (bulletParent == null)
            {
                Log.Error("Bullet entity group '{0}' does not exist.", bulletEntityGroupName);
                return false;
            }

            IDataTable<DREntity> entityTable = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity bulletEntityConfig = entityTable?.GetDataRow(WeaponData.BulletEntityId);
            if (bulletEntityConfig == null || string.IsNullOrEmpty(bulletEntityConfig.AssetName))
            {
                Log.Error("Bullet entity data row '{0}' is invalid.", WeaponData.BulletEntityId);
                return false;
            }

            int bulletEntityId = GenerateBulletEntityId();
            Quaternion shotWorldRotation = GetShotRotation(in request);
            //子弹生成时的位置和旋转 以枪口坐标为准，转换到子弹池父对象
            Vector3 bulletLocalPosition = bulletParent.InverseTransformPoint(muzzle.position);
            Quaternion bulletLocalRotation = Quaternion.Inverse(bulletParent.rotation) * shotWorldRotation;
            BulletEntityData bulletData = new BulletEntityData(
                bulletEntityId,
                WeaponData.BulletEntityId,
                Owner.gameObject,
                gameObject,
                Owner.Owner.Faction,
                WeaponData.Attack,
                WeaponData.BulletSpeed,
                bulletLifetime,
                WeaponData.PenetrationCount,
                bulletLocalPosition,
                bulletLocalRotation,
                bulletCollisionRadius,
                bulletHitLayer,
                triggerInteraction);

            GameEntry.Entity.ShowEntity<BulletEntity>(
                bulletEntityId,
                AssetUtility.GetEntityAsset(bulletEntityConfig.AssetName),
                bulletEntityGroupName,
                bulletData);
            // Debug.LogFormat($"show bullet:{0}, id:{1}", WeaponData.BulletEntityId, bulletEntityId);
            LastBulletEntityId = bulletEntityId;

            //播放开火音效
            GameEntry.Sound.PlaySound(WeaponData.SoundId);
            //播放枪口火焰
            EffectSpawner.ShowMuzzleFlash(Entity, muzzle);
            return true;
        }

        private Quaternion GetShotRotation(in AttackRequest request)
        {
            Vector3 direction = request.HasAimPoint ? request.AimPoint - muzzle.position : request.AimDirection;
            Quaternion rotation = direction.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(direction.normalized) : muzzle.rotation;
            float spreadAngle = Mathf.Max(0f, WeaponData.SpreadAngle);
            if (spreadAngle <= 0f)
            {
                return rotation;
            }

            Vector2 spread = Random.insideUnitCircle * spreadAngle;
            return rotation * Quaternion.Euler(-spread.y, spread.x, 0f);
        }

        private int GenerateBulletEntityId()
        {
            int entityId;
            do
            {
                if (nextBulletEntityId == int.MinValue)
                {
                    nextBulletEntityId = 0;
                }

                entityId = --nextBulletEntityId;
            }
            while (GameEntry.Entity.HasEntity(entityId) || GameEntry.Entity.IsLoadingEntity(entityId));

            return entityId;
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
