using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 创建武器实体时传入的运行时数据。
    /// </summary>
    public class WeaponEntityData : EntityData
    {
        public WeaponEntityData(
            int entityId,
            int entityTypeId,
            int weaponId,
            CombatController owner)
            : base(entityId, entityTypeId)
        {
            WeaponId = weaponId;
            Owner = owner;
        }

        public int WeaponId { get; }
        public CombatController Owner { get; }
    }

    /// <summary>
    /// 所有武器实体的基类，统一管理装备关系、DRWeapon 数据和攻击冷却。
    /// </summary>
    public abstract class WeaponEntity : Entity, IWeapon
    {
        private DRWeapon weaponData;
        private CombatController owner;
        private float nextAttackTime;

        public int WeaponId => weaponData != null ? weaponData.Id : 0;
        public DRWeapon WeaponData => weaponData;
        public CombatController Owner => owner;
        public GameObject WeaponObject => gameObject;
        public float CooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);
        public bool IsReady => Available && weaponData != null && owner != null && CooldownRemaining <= 0f;

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

            if (GameEntry.DataTable == null)
            {
                Log.Error("DataTable component is not initialized.");
                return;
            }

            IDataTable<DRWeapon> weaponTable = GameEntry.DataTable.GetDataTable<DRWeapon>();
            if (weaponTable == null)
            {
                Log.Error("Weapon data table is not loaded.");
                return;
            }

            weaponData = weaponTable.GetDataRow(entityData.WeaponId);
            if (weaponData == null)
            {
                Log.Error("Weapon data row '{0}' does not exist.", entityData.WeaponId);
                return;
            }

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
            CombatController previousOwner = owner;
            owner = null;

            if (previousOwner != null)
            {
                previousOwner.UnequipWeapon(this);
            }
        }

        public bool TryAttack()
        {
            if (!IsReady || !OnAttack())
            {
                return false;
            }

            nextAttackTime = Time.time + Mathf.Max(0f, weaponData.FireInterval);
            return true;
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
        protected abstract bool OnAttack();
    }
}
