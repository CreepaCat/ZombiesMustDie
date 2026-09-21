using UnityEngine;

namespace ZombiesMustDie
{
    public class EnemyEntityData : EntityData
    {
        //todo:记录关卡管理器

        public float MoveSpeed = 3.5f;
        public float RotationSpeed = 1000f;
        public float StoppingDistance = 0.5f;
        public float MaxHealth = 100;
        // public Vector3 SpawnPoint = Vector3.zero;
        public EnemyEntityData(float moveSpeed, float rotationSpeed, float stoppingDistance, float maxHp,
            int entityId, int typeId) : base(entityId, typeId)
        {
            MoveSpeed = moveSpeed;
            RotationSpeed = rotationSpeed;
            StoppingDistance = stoppingDistance;
            MaxHealth = maxHp;
        }
    }
}
