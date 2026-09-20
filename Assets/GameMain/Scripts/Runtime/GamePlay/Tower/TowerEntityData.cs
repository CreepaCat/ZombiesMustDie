using System;

namespace ZombiesMustDie
{
    public sealed class TowerEntityData : EntityData
    {
        public TowerEntityData(int id, int towerId, DRTowerLevel level, TowerBuildPoint point,
            TowerService service, Guid operationId)
            : base(id, level.EntityId)
        {
            TowerId = towerId;
            Level = level;
            BuildPoint = point;
            Service = service;
            OperationId = operationId;
        }
        public int TowerId { get; }
        public DRTowerLevel Level { get; }
        public TowerBuildPoint BuildPoint { get; }
        public TowerService Service { get; }
        public Guid OperationId { get; }
    }
}
