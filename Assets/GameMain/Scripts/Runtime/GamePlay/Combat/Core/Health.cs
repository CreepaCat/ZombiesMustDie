using System;
using UnityEngine;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        private float currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0f;

        private void Awake()
        {
            ResetHealth();
        }

        public DamageResult TakeDamage(float damage)
        {
            //死亡、负数、非数、无限大检查
            if (IsDead || damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                return DamageResult.Failed(IsDead);
            }

            float appliedDamage = Mathf.Min(damage, currentHealth);
            currentHealth -= appliedDamage;
            HealthChanged?.Invoke(currentHealth, maxHealth);

            bool died = IsDead;
            Debug.Log($"{name}受到伤害{appliedDamage}", this);

            if (died)
            {
                Debug.Log($"{name}死亡", this);
                Died?.Invoke();
            }

            return new DamageResult(true, appliedDamage, died);
        }

        public void ResetHealth()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(int maxHp)
        {
            maxHealth = maxHp;
            ResetHealth();
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
        }
    }
}
