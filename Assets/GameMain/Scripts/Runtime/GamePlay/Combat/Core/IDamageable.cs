namespace ZombiesMustDie
{
    public interface IDamageable
    {
        CombatFaction Faction { get; }
        bool IsDead { get; }
        DamageResult TakeDamage(in DamageInfo damageInfo);
    }
}
