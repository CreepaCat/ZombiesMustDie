using System;
using UnityEngine;

namespace ZombiesMustDie
{
    public enum TowerOperationState { Empty, Building, Working, Upgrading, Demolishing }

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

        public int? OpenManageForm(TowerService service)
        {
            if (!isActiveAndEnabled || service == null || !service.isActiveAndEnabled ||
                (Service != null && Service != service) || GameEntry.UI == null) return null;
            var data = new TowerManageFormData(service, this);
            var existing = GameEntry.UI.GetUIForm(UIFormId.TowerManageForm) as TowerManageForm;
            if (existing != null) { existing.Bind(data); return existing.UIForm.SerialId; }
            return GameEntry.UI.OpenUIForm(UIFormId.TowerManageForm, data);
        }

        private void OnDisable()
        {
            if (Service != null) Service.ReleasePoint(this);
        }
    }
}
