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
            CombatController owner,
            DRWeapon weaponConfig = null)
            : base(entityId, entityTypeId)
        {
            WeaponId = weaponId;
            Owner = owner;
            WeaponConfig = weaponConfig;
        }

        public int WeaponId { get; }
        public CombatController Owner { get; }

        /// <summary>
        /// 可选的武器配置。为空时由 WeaponEntity 根据 WeaponId 查询数据表。
        /// </summary>
        public DRWeapon WeaponConfig { get; }
    }
}
