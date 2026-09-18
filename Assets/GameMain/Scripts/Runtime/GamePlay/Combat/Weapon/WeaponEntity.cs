using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 所有武器实体的基类，统一管理装备关系、DRWeapon 数据和攻击冷却。
    /// </summary>
    public abstract class WeaponEntity : Entity, IWeapon
    {
        private DRWeapon weaponData;
        private CombatController owner;
        private float nextAttackTime;
        private int currentMagazineAmmo;
        private bool isReloading;
        private float reloadFinishTime;
        private Transform poolParent;

        public int WeaponId => weaponData != null ? weaponData.Id : 0;
        public DRWeapon WeaponData => weaponData;
        public CombatController Owner => owner;
        public GameObject WeaponObject => gameObject;
        public float CooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);
        public int CurrentMagazineAmmo { get { UpdateReload(); return currentMagazineAmmo; } }
        public bool IsReloading { get { UpdateReload(); return isReloading; } }
        public bool IsReady => Available && weaponData != null && owner != null &&
            owner.isActiveAndEnabled && !owner.Owner.IsDead /*&& !IsReloading &&
            (weaponData.MagazineSize <= 0 || CurrentMagazineAmmo > 0) && CooldownRemaining <= 0f*/;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            poolParent = CachedTransform.parent;
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            weaponData = null;
            owner = null;
            nextAttackTime = Time.time;

            WeaponEntityData entityData = userData as WeaponEntityData;
            if (entityData == null)
            {
                Log.Error("Weapon entity data is invalid.");
                return;
            }

            weaponData = entityData.WeaponConfig;
            if (weaponData == null && GameEntry.DataTable == null)
            {
                Log.Error("DataTable component is not initialized.");
                return;
            }

            if (weaponData == null)
            {
                IDataTable<DRWeapon> weaponTable = GameEntry.DataTable.GetDataTable<DRWeapon>();
                if (weaponTable == null)
                {
                    Log.Error("Weapon data table is not loaded.");
                    return;
                }

                weaponData = weaponTable.GetDataRow(entityData.WeaponId);
            }

            if (weaponData == null)
            {
                Log.Error("Weapon data row '{0}' does not exist.", entityData.WeaponId);
                return;
            }

            currentMagazineAmmo = Mathf.Max(0, weaponData.MagazineSize);
            isReloading = false;
            if (entityData.Owner != null)
            {
                entityData.Owner.EquipWeapon(this);
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            CombatController previousOwner = owner;
            if (previousOwner != null)
            {
                previousOwner.UnequipWeapon(this);
            }

            owner = null;
            weaponData = null;
            nextAttackTime = 0f;
            currentMagazineAmmo = 0;
            isReloading = false;
            reloadFinishTime = 0f;

            // Also restore manually parented weapons owned by non-Entity players.
            CachedTransform.SetParent(poolParent, false);
            base.OnHide(isShutdown, userData);
        }

        public void Equip(CombatController newOwner)
        {
            if (owner == newOwner)
            {
                return;
            }

            CombatController previousOwner = owner;
            if (previousOwner != null)
            {
                previousOwner.UnequipWeapon(this);
            }

            owner = newOwner;
        }

        public void Unequip()
        {
            isReloading = false;
            CombatController previousOwner = owner;
            owner = null;

            if (previousOwner != null)
            {
                previousOwner.UnequipWeapon(this);
            }
        }

        public bool TryAttack()
        {
            return TryAttack(default);
        }

        public bool TryAttack(in AttackRequest request)
        {
            if (Time.timeScale <= 0f || !IsReady || !OnAttack(in request))
            {
                Debug.Log("武器没准备好");
                return false;
            }

            if (weaponData.MagazineSize > 0)
            {
                currentMagazineAmmo--;
            }
            nextAttackTime = Time.time + Mathf.Max(0f, weaponData.FireInterval);
            return true;
        }

        /// <summary>备用弹药不限量；卸下或回收时取消换弹。</summary>
        public bool TryReload(float duration)
        {
            if (!Available || weaponData == null || owner == null || !owner.isActiveAndEnabled ||
                owner.Owner.IsDead || IsReloading || weaponData.MagazineSize <= 0 ||
                CurrentMagazineAmmo >= weaponData.MagazineSize || float.IsNaN(duration) ||
                float.IsInfinity(duration) || Time.timeScale <= 0f)
            {
                return false;
            }
            isReloading = true;
            reloadFinishTime = Time.time + Mathf.Max(0f, duration);
            UpdateReload();
            return true;
        }

        private void UpdateReload()
        {
            if (isReloading && Time.time >= reloadFinishTime)
            {
                isReloading = false;
                currentMagazineAmmo = weaponData != null ? Mathf.Max(0, weaponData.MagazineSize) : 0;
            }
        }

        public void ResetCooldown()
        {
            nextAttackTime = Time.time;
        }

        protected DamageResult DealDamage(
            IDamageable target,
            Vector3 hitPoint,
            Vector3 hitDirection)
        {
            if (owner == null || weaponData == null)
            {
                return DamageResult.Failed(target != null && target.IsDead);
            }

            return owner.TryDealDamage(
                target,
                weaponData.Attack,
                hitPoint,
                hitDirection,
                gameObject);
        }

        /// <summary>
        /// 执行一次具体攻击。返回 true 时才会进入冷却。
        /// </summary>
        protected abstract bool OnAttack(in AttackRequest request);
    }
}
