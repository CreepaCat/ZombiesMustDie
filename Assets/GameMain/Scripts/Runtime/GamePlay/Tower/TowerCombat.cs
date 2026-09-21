using UnityEngine;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    public sealed class TowerCombat : MonoBehaviour
    {
        [SerializeField] private Transform turret;
        [SerializeField] private Transform sightOrigin;
        [SerializeField, Range(0f, 180f)] private float aimTolerance = 5f;
        [SerializeField] private LayerMask obstructionLayers = ~0;
        [SerializeField] private LayerMask bulletHitLayer = ~0; //子弹或攻击可命中的layer
        private TowerEntity tower;
        private TowerTargetDetector detector;
        private DRTowerLevel level;
        public Transform WeaponMount => turret != null ? turret : transform;
        public LayerMask BulletHitLayer => bulletHitLayer;

        internal void Configure(TowerEntity owner, DRTowerLevel config)
        {
            tower = owner;
            level = config;
            detector = GetComponent<TowerTargetDetector>();
            detector.Clear();
        }

        private void Update()
        {
            if (tower == null || !tower.IsWorking || level == null || Time.timeScale <= 0f) return;
            CombatTarget target = detector.FindTarget(level.Range);
            if (target == null) return;
            Vector3 origin = sightOrigin != null ? sightOrigin.position : WeaponMount.position;
            Vector3 aim = target.transform.position;
            var collider = target.GetComponentInChildren<Collider>();
            if (collider != null) aim = collider.bounds.center;
            Vector3 direction = aim - origin;
            if (direction.sqrMagnitude < 0.000001f) return;
            WeaponMount.rotation = Quaternion.RotateTowards(WeaponMount.rotation,
                Quaternion.LookRotation(direction), level.TurnSpeed * Time.deltaTime);
            if (Vector3.Angle(WeaponMount.forward, direction) > aimTolerance) return;
            // 忽略自身和目标的碰撞体；其余实体或场景几何均可遮挡。
            foreach (var hit in Physics.RaycastAll(origin, direction.normalized, direction.magnitude,
                obstructionLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(transform) ||
                    hit.collider.GetComponentInParent<CombatTarget>() == target) continue;
                return;
            }
            var request = new AttackRequest(aim, direction.normalized);
            tower.Combat.TryAttack(in request);
        }
    }
}
