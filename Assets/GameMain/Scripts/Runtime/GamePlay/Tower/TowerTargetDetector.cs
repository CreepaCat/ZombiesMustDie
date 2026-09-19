using System;
using UnityEngine;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    public sealed class TowerTargetDetector : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float searchInterval = 0.2f;
        [SerializeField] private LayerMask enemyLayers = ~0;
        private Collider[] hits = new Collider[32];
        private float nextSearch;
        public CombatTarget CurrentTarget { get; private set; }

        public CombatTarget FindTarget(float range)
        {
            if (IsValid(CurrentTarget, range)) return CurrentTarget;
            CurrentTarget = null;
            if (Time.time < nextSearch) return null;
            nextSearch = Time.time + Mathf.Max(0.02f, searchInterval);
            int count;
            while ((count = Physics.OverlapSphereNonAlloc(transform.position, range, hits, enemyLayers,
                QueryTriggerInteraction.Collide)) == hits.Length) Array.Resize(ref hits, hits.Length * 2);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var target = hits[i].GetComponentInParent<CombatTarget>();
                if (!IsValid(target, range)) continue;
                float distance = (target.transform.position - transform.position).sqrMagnitude;
                if (distance >= nearest) continue;
                nearest = distance;
                CurrentTarget = target;
            }
            return CurrentTarget;
        }

        private bool IsValid(CombatTarget target, float range) => target != null && target.isActiveAndEnabled &&
            !target.IsDead && target.Faction == CombatFaction.Enemy && target.IsTargetable &&
            (target.transform.position - transform.position).sqrMagnitude <= range * range;

        public void Clear() { CurrentTarget = null; nextSearch = 0f; }
        private void OnDisable() => Clear();
    }
}
