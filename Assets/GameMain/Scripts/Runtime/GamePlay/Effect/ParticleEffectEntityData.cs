using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>一次性粒子特效的运行数据；附着特效在完成附着前不播放。</summary>
    public sealed class ParticleEffectEntityData : EntityData
    {
        public ParticleEffectEntityData(int entityId, int typeId, Vector3 position,
            Quaternion rotation, bool waitForAttachment, float emissionDuration, float maxLifetime)
            : base(entityId, typeId)
        {
            Position = position;
            Rotation = rotation;
            WaitForAttachment = waitForAttachment;
            EmissionDuration = Mathf.Max(0f, emissionDuration);
            MaxLifetime = Mathf.Max(0.01f, maxLifetime);
        }

        public bool WaitForAttachment { get; }
        public float EmissionDuration { get; }
        public float MaxLifetime { get; }
    }
}
