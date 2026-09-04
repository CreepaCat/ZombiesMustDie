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
            return Health.TakeDamage(damageInfo.Amount);
        }
    }
}
