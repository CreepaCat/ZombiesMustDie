using UnityEngine;

namespace ZombiesMustDie
{
    [RequireComponent(typeof(CombatController), typeof(TowerCombat), typeof(TowerTargetDetector))]
    public sealed class TowerEntity : Entity
    {
        public TowerEntityData Data { get; private set; }
        public CombatController Combat { get; private set; }
        public TowerCombat TowerCombat { get; private set; }
        public bool IsWorking { get; private set; }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Combat = GetComponent<CombatController>();
            TowerCombat = GetComponent<TowerCombat>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            Data = userData as TowerEntityData;
            IsWorking = false;
            Combat.Owner.Configure(CombatFaction.Player, false, true);
            Combat.Owner.Health.ResetHealth();
            if (Data == null) return;
            // 建造点提供世界坐标，Entity 组本身可以有变换。
            if (Data.BuildPoint != null)
                CachedTransform.SetPositionAndRotation(Data.BuildPoint.SpawnPoint.position, Data.BuildPoint.SpawnPoint.rotation);
            TowerCombat.Configure(this, Data.Level);
        }

        internal void SetWorking(bool working) => IsWorking = working;

        protected override void OnHide(bool isShutdown, object userData)
        {
            IsWorking = false;
            var previous = Data;
            Data = null;
            Combat.UnequipWeapon();
            GetComponent<TowerTargetDetector>().Clear();
            if (previous?.Service != null) previous.Service.OnTowerHidden(this, previous);
            base.OnHide(isShutdown, userData);
        }
    }
}
