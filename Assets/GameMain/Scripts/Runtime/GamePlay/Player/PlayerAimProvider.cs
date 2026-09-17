using System;
using UnityEngine;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    public sealed class PlayerAimProvider : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField, Min(0.1f)] private float maxDistance = 100f;
        [SerializeField] private LayerMask hitLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        private RaycastHit[] hits = new RaycastHit[32];
        private CombatController combat;

        public bool TryGetAttackRequest(out AttackRequest request)
        {
            request = default;
            if (aimCamera == null) aimCamera = Camera.main;
            if (aimCamera == null) return false;
            if (combat == null) combat = GetComponent<CombatController>();
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float distance = Mathf.Max(0.1f, maxDistance);
            int count;
            // A full NonAlloc buffer may omit the nearest wall; grow and repeat.
            while ((count = Physics.RaycastNonAlloc(ray, hits, distance, hitLayer, triggerInteraction)) == hits.Length)
                Array.Resize(ref hits, hits.Length * 2);
            Vector3 point = ray.GetPoint(distance);
            for (int i = 0; i < count; i++)
            {
                Transform candidate = hits[i].transform;
                if (candidate == transform || candidate.IsChildOf(transform)) continue;
                GameObject weapon = combat != null ? combat.EquippedWeapon : null;
                if (weapon != null && (candidate == weapon.transform || candidate.IsChildOf(weapon.transform))) continue;
                if (hits[i].distance < distance)
                {
                    distance = hits[i].distance;
                    point = hits[i].point;
                }
            }
            request = new AttackRequest(point, ray.direction);
            return true;
        }
    }
}
