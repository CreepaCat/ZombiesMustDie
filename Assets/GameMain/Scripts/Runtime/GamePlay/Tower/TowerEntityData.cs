using System;

namespace ZombiesMustDie
{
    public sealed class TowerEntityData : EntityData
    {
        public TowerEntityData(int id, int towerId, DRTowerLevel level, TowerBuildPoint point,
            TowerService service, Guid operationId, int initialPaid, long upgradePaid)
            : base(id, level.EntityId)
        {
            TowerId = towerId;
            Level = level;
            BuildPoint = point;
            Service = service;
            OperationId = operationId;
            InitialPaid = initialPaid;
            UpgradePaid = upgradePaid;
        }
        public int TowerId { get; }
        public DRTowerLevel Level { get; }
        public TowerBuildPoint BuildPoint { get; }
        public TowerService Service { get; }
        public Guid OperationId { get; }
        public int InitialPaid { get; }
        public long UpgradePaid { get; }
        public long TotalPaid => InitialPaid + UpgradePaid;
    }
}
