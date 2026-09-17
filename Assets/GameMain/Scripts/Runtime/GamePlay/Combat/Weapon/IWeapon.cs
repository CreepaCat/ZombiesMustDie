using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 武器对角色战斗系统公开的公共能力。
    /// </summary>
    public interface IWeapon
    {
        int WeaponId { get; }
        DRWeapon WeaponData { get; }
        CombatController Owner { get; }
        GameObject WeaponObject { get; }
        bool IsReady { get; }
        float CooldownRemaining { get; }

        void Equip(CombatController owner);
        void Unequip();
        bool TryAttack();
        bool TryAttack(in AttackRequest request);
        bool TryReload(float duration);
        int CurrentMagazineAmmo { get; }
        bool IsReloading { get; }
        void ResetCooldown();
    }
}
