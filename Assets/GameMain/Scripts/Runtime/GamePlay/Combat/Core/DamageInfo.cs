using UnityEngine;

namespace ZombiesMustDie
{
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public GameObject Instigator { get; }
        public GameObject Source { get; }
        public CombatFaction SourceFaction { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }

        public DamageInfo(
            float amount,
            GameObject instigator,
            GameObject source,
            CombatFaction sourceFaction,
            Vector3 hitPoint,
            Vector3 hitDirection)
        {
            Amount = amount;
            Instigator = instigator;
            Source = source;
            SourceFaction = sourceFaction;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
        }
    }
}
