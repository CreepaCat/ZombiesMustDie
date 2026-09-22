using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZombiesMustDie
{
    /// <summary>
    /// 总部基地，玩家保护对象
    /// </summary>
    public class HeadQuarter : MonoBehaviour
    {
        Health health;
        public event Action<float, float> HealthChanged;
        private void Awake()
        {
            health = GetComponent<Health>();

        }

        private void OnEnable()
        {
            health.Died += OnDie;
            health.HealthChanged += OnHealthChanged;
        }
        private void OnDisable()
        {
            health.Died -= OnDie;
            health.HealthChanged -= OnHealthChanged;
        }

        private void Update()
        {

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("按下加速键");
                Time.timeScale *= 2f;
            }

        }

        public static GameObject GetInstance()
        {
            return GameObject.FindGameObjectWithTag("HeadQuarter");
        }
        public static Vector3 GetPosition()
        {
            return GameObject.FindGameObjectWithTag("HeadQuarter").transform.position;
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            HealthChanged.Invoke(currentHealth, maxHealth);
        }

        private void OnDie()
        {
            GameObject.FindWithTag("LevelManager").GetComponent<LevelController>().FailLevel();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("HeadQuarter OnTriggerEnter");

            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                other.GetComponent<EnemyEntity>().RecycleImmediately();
                health.TakeDamage(10);
            }
        }
    }
}
