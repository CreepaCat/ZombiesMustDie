using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 统一的受击入口。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IDamageable
    {
        [SerializeField] private CombatFaction faction = CombatFaction.Neutral;

        private Health health;
        [SerializeField] private bool isTargetable = true;
        [SerializeField] private bool invulnerable;
        public bool IsTargetable => isTargetable;
        public bool Invulnerable => invulnerable;
        public void Configure(CombatFaction value, bool targetable, bool immune)
        {
            faction = value;
            isTargetable = targetable;
            invulnerable = immune;
        }

        public CombatFaction Faction => faction;
        public bool IsDead => Health.IsDead;
        public Health Health
        {
            get
            {
                if (health == null)
                {
                    health = GetComponent<Health>();
                }

                return health;
            }
        }

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        public DamageResult TakeDamage(in DamageInfo damageInfo)
        {
            return invulnerable ? DamageResult.Failed(IsDead) : Health.TakeDamage(damageInfo.Amount);
        }
    }
}
