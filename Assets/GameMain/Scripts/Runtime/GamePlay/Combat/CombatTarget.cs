using System.Collections.Generic;
using UnityEngine;
namespace ZombiesMustDie
{
    /// <summary>
    /// 战斗脚本，用于定义一个可战斗对象
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour
    {
        [SerializeField] float damage = 5f;

        [Header("近战伤害区域")]
        [SerializeField] Transform meeleSphereCenter = null;
        [SerializeField] float meeleRadius = 0.6f;

        //对战目标
        [SerializeField] LayerMask targetLayer;

        [Header("战斗目标碰撞体缓存")]
        [SerializeField]
        Collider[] targetColliders = new Collider[32];

        Health health;
        HashSet<CombatTarget> targets;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void Start()
        {
            targets = new();
        }

        /// <summary>
        ///受到伤害
        /// </summary>
        /// <param name="instigator">伤害触发者</param>
        /// <param name="damageToTake">伤害值</param>
        public void TakeDamage(CombatTarget instigator, float damageToTake)
        {
            health.TakeDamage(damageToTake);
        }

        //近战
        public void MeleeAttack()
        {
            int colliderNum = Physics.OverlapSphereNonAlloc(meeleSphereCenter.position, meeleRadius, targetColliders, targetLayer);
            if (colliderNum > 0)
            {
                for (int i = 0; i < colliderNum; i++)
                {
                    if (targetColliders[i] == null) continue;
                    CombatTarget target;
                    if (targetColliders[i].transform.TryGetComponent(out target)
                            && !ReferenceEquals(target, this))
                    {
                        targets.Add(target);
                    }
                }
            }

            foreach (var target in targets)
            {
                target.TakeDamage(this, damage);
            }

            targets.Clear();
        }

        void OnDrawGizmos()
        {
            if (meeleSphereCenter == null) return;
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(meeleSphereCenter.position, meeleRadius);
        }
    }
}
