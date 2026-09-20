using System;
using UnityEngine;

namespace ZombiesMustDie
{
    public enum TowerOperationState { Empty, Building, Working, Upgrading }

    [DisallowMultipleComponent]
    public sealed class TowerBuildPoint : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int[] allowedTowerIds = { 1, 2 };
        public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;
        public TowerEntity Tower { get; internal set; }
        public TowerService Service { get; internal set; }
        public TowerOperationState State { get; internal set; }
        public Guid OperationId { get; internal set; }
        public bool IsBusy => State != TowerOperationState.Empty && State != TowerOperationState.Working;
        public bool Allows(int towerId) => allowedTowerIds != null && Array.IndexOf(allowedTowerIds, towerId) >= 0;

        private void OnDisable()
        {
            if (Service != null) Service.ReleasePoint(this);
        }
    }
}
