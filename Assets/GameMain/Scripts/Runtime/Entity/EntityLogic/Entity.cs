using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
namespace ZombiesMustDie
{
    /// <summary>
    /// 实体逻辑。
    /// </summary>
    public abstract class Entity : EntityLogic
    {
        [SerializeField]
        private EntityData m_EntityData = null;

        public int Id { get { return Entity.Id; } } //实体编号

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_EntityData = userData as EntityData;
            if (m_EntityData == null)
            {
                Log.Error("Entity data is invalid.");
                return;
            }

            Name = Utility.Text.Format("[Entity {0}]", Id);
            CachedTransform.localPosition = m_EntityData.Position;
            CachedTransform.localRotation = m_EntityData.Rotation;
            CachedTransform.localScale = Vector3.one;

        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            m_EntityData = null;
            base.OnHide(isShutdown, userData);
        }

        //todo:实体的Attach和Detach回调方法，暂时不需要

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

    }
}
