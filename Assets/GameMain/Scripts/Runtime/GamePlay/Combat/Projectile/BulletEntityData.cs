using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 子弹实体每次显示时使用的运行时数据。
    /// </summary>
    public sealed class BulletEntityData : EntityData
    {
        public BulletEntityData(
            int entityId,
            int entityTypeId,
            GameObject instigator,
            GameObject source,
            CombatFaction sourceFaction,
            float damage,
            float speed,
            float lifetime,
            int penetrationCount,
            Vector3 position,
            Quaternion rotation,
            float collisionRadius,
            LayerMask hitLayer,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
            : base(entityId, entityTypeId)
        {
            Instigator = instigator;
            Source = source;
            SourceFaction = sourceFaction;
            Damage = Mathf.Max(0f, damage);
            Speed = Mathf.Max(0f, speed);
            Lifetime = Mathf.Max(0.01f, lifetime);
            PenetrationCount = Mathf.Max(0, penetrationCount);
            CollisionRadius = Mathf.Max(0.001f, collisionRadius);
            HitLayer = hitLayer;
            TriggerInteraction = triggerInteraction;
            Position = position;
            Rotation = rotation;
            Direction = rotation * Vector3.forward;
        }

        public GameObject Instigator { get; }
        public GameObject Source { get; }
        public CombatFaction SourceFaction { get; }
        public float Damage { get; }
        public float Speed { get; }
        public float Lifetime { get; }
        public int PenetrationCount { get; }
        public float CollisionRadius { get; }
        public LayerMask HitLayer { get; }
        public QueryTriggerInteraction TriggerInteraction { get; }
        public Vector3 Direction { get; }
    }
}
