namespace ZombiesMustDie
{
    /// <summary>
    /// 用于验证枪械发射流程的具体武器类型。
    /// </summary>
    public sealed class RifleWeapon : ProjectileWeaponEntity
    {
        public bool Fire()
        {
            return TryAttack();
        }
    }
}
