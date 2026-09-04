using System.Collections.Generic;
using UnityEngine;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatTarget))]
    public class CombatController : MonoBehaviour
    {
        private const int HitBufferSize = 32;

        [Header("攻击配置")]
        [SerializeField, Min(0f)] private float attackDamage = 5f;

        [Header("近战伤害区域")]
        [SerializeField] private Transform meleeSphereCenter;
        [SerializeField, Min(0f)] private float meleeRadius = 0.6f;
        [SerializeField] private LayerMask targetLayer;

        [Header("武器")]
        [SerializeField] private Transform weaponSocket;
        [SerializeField] private GameObject equippedWeapon;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<CombatTarget> hitTargets = new HashSet<CombatTarget>();
        private CombatTarget owner;

        public CombatTarget Owner
        {
            get
            {
                if (owner == null)
                {
                    owner = GetComponent<CombatTarget>();
                }

                return owner;
            }
        }

        public GameObject EquippedWeapon => equippedWeapon;
        public Transform WeaponSocket => weaponSocket;
        public float AttackDamage => attackDamage;

        private void Awake()
        {
            owner = GetComponent<CombatTarget>();
        }

        public void ConfigureAttack(float damage)
        {
            attackDamage = Mathf.Max(0f, damage);
        }

        public void EquipWeapon(GameObject weapon)
        {
            equippedWeapon = weapon;
        }

        public GameObject UnequipWeapon()
        {
            GameObject weapon = equippedWeapon;
            equippedWeapon = null;
            return weapon;
        }
        /// <summary>
        /// 近战攻击目标判定
        /// </summary>
        /// <returns>成功伤害目标数</returns>
        public int MeleeAttack()
        {
            if (meleeSphereCenter == null || meleeRadius <= 0f || attackDamage <= 0f)
            {
                return 0;
            }

            hitTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(
                meleeSphereCenter.position,
                meleeRadius,
                hitBuffer,
                targetLayer,
                QueryTriggerInteraction.UseGlobal);

            int damagedTargetCount = 0;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hitBuffer[i];
                if (hitCollider == null)
                {
                    continue;
                }

                CombatTarget target = hitCollider.GetComponentInParent<CombatTarget>();
                if (target == null || target == Owner || !hitTargets.Add(target))
                {
                    continue;
                }

                Vector3 hitPoint = hitCollider.ClosestPoint(meleeSphereCenter.position);
                Vector3 hitDirection = target.transform.position - transform.position;
                hitDirection.y = 0f;
                hitDirection = hitDirection.sqrMagnitude > 0f ? hitDirection.normalized : transform.forward;
                //检查伤害是否成功
                DamageResult result = TryDealDamage(target, attackDamage, hitPoint, hitDirection);
                if (result.Succeeded)
                {
                    damagedTargetCount++;
                }
            }

            return damagedTargetCount;
        }

        public DamageResult TryDealDamage(
            IDamageable target,
            float damage,
            Vector3 hitPoint,
            Vector3 hitDirection,
            GameObject source = null)
        {
            if (target == null || target.IsDead || damage <= 0f || !Owner.Faction.IsHostileTo(target.Faction))
            {
                return DamageResult.Failed(target != null && target.IsDead);
            }

            DamageInfo damageInfo = new DamageInfo(
                damage,
                gameObject,
                source != null ? source : equippedWeapon != null ? equippedWeapon : gameObject,
                Owner.Faction,
                hitPoint,
                hitDirection);

            return target.TakeDamage(in damageInfo);
        }

        private void OnValidate()
        {
            attackDamage = Mathf.Max(0f, attackDamage);
            meleeRadius = Mathf.Max(0f, meleeRadius);
        }

        private void OnDrawGizmosSelected()
        {
            if (meleeSphereCenter == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeSphereCenter.position, meleeRadius);
        }
    }
}
