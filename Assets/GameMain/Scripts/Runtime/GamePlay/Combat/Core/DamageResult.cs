namespace ZombiesMustDie
{
    public readonly struct DamageResult
    {
        public bool Succeeded { get; }
        public float AppliedDamage { get; }
        public bool IsDead { get; }

        public DamageResult(bool succeeded, float appliedDamage, bool isDead)
        {
            Succeeded = succeeded;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
        }

        public static DamageResult Failed(bool isDead = false)
        {
            return new DamageResult(false, 0f, isDead);
        }
    }
}
