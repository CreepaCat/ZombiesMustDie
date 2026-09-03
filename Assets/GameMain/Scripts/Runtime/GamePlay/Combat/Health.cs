using System;
using UnityEngine;
namespace ZombiesMustDie
{
    public class Health : MonoBehaviour
    {
        [SerializeField] float maxHealth = 100;

        float currentHealth;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damageToTake)
        {
            currentHealth -= damageToTake;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            Debug.Log(transform.name + "受到伤害" + damageToTake);

            if (currentHealth <= 0f)
            {
                Debug.Log(transform.name + "死亡");
            }
        }
    }
}
