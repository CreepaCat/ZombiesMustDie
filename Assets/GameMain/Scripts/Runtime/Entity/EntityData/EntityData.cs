using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 实体数据。
    /// </summary>
    public abstract class EntityData
    {

        [SerializeField]
        private int m_Id = 0;

        [SerializeField]
        private int m_TypeId = 0;

        [SerializeField]
        private Vector3 m_Position = Vector3.zero;

        [SerializeField]
        private Quaternion m_Rotation = Quaternion.identity;

        public EntityData(int entityId, int typeId)
        {
            m_Id = entityId;
            m_TypeId = typeId;
        }

        public int Id { get { return m_Id; } } //实体编号
        public int TypeId { get { return m_TypeId; } } //实体类型编号

        public Vector3 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        public Quaternion Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }


    }
}
