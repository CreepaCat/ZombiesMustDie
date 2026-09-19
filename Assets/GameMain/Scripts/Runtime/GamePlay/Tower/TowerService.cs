using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>关卡级交易协调器。待加载实体与旧塔并存，验证成功后才提交替换。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerWallet))]
    public sealed class TowerService : MonoBehaviour
    {
        [SerializeField] private PlayerWallet wallet;
        [SerializeField] private string towerGroup = "Tower";
        [SerializeField] private string weaponGroup = "Weapon";
        [SerializeField] private string bulletGroup = "Bullet";
        [SerializeField] private string muzzlePath = "Muzzle";
        [SerializeField, Min(1f)] private float loadTimeout = 20f;
        private static readonly HashSet<TowerService> services = new HashSet<TowerService>();
        private static int nextEntityId;
        private readonly Dictionary<TowerBuildPoint, Slot> slots = new Dictionary<TowerBuildPoint, Slot>();
        private readonly Dictionary<int, Pending> loads = new Dictionary<int, Pending>();
        private bool subscribed;
        private bool closing;
        private bool executing;
        private float nextHealthCheck;
        public PlayerWallet Wallet => wallet;
        public string LastError { get; private set; }
        public event Action Changed;

        private sealed class Slot
        {
            public TowerBuildPoint Point;
            public TowerEntity Tower;
            public int WeaponId;
            public readonly List<Guid> Payments = new List<Guid>();
            public Pending Pending;
        }

        private sealed class Pending
        {
            public Slot Slot;
            public Guid Operation;
            public TowerEntityData Data;
            public WeaponEntityData WeaponData;
            public DRWeapon Config;
            public TowerEntity Tower;
            public int WeaponId;
            public float Deadline;
            public float CooldownDeadline;
        }

        private void Awake() { if (wallet == null) wallet = GetComponent<PlayerWallet>(); }
        private void OnEnable() { closing = false; services.Add(this); }

        public bool TryBuild(TowerBuildPoint point, int towerId)
        {
            if (!Ready(point) || point.Service != null || point.State != TowerOperationState.Empty || !point.Allows(towerId))
                return Fail("建造点不可用或不允许该塔类型。");
            DRTower config = GameEntry.DataTable.GetDataTable<DRTower>()?.GetDataRow(towerId);
            DRTowerLevel level = config != null ? GameEntry.DataTable.GetDataTable<DRTowerLevel>()?.GetDataRow(config.InitialLevelId) : null;
            if (level == null || level.Level != 1) return Fail("初始等级配置无效。");
            if (!Validate(level, out DRWeapon weapon)) return false;
            var slot = new Slot { Point = point };
            slots.Add(point, slot);
            point.Service = this;
            return Begin(slot, towerId, level, weapon);
        }

        public bool TryUpgrade(TowerBuildPoint point)
        {
            if (!Ready(point) || !slots.TryGetValue(point, out Slot slot) || slot.Pending != null ||
                slot.Tower == null || point.State != TowerOperationState.Working) return Fail("防御塔正忙或不存在。");
            var previous = slot.Tower.Data;
            DRTowerLevel next = GameEntry.DataTable.GetDataTable<DRTowerLevel>()?.GetDataRow(previous.Level.NextLevelId);
            if (next == null || next.Level != previous.Level.Level + 1) return Fail("已满级或下一等级配置无效。");
            if (!Validate(next, out DRWeapon weapon)) return false;
            return Begin(slot, previous.TowerId, next, weapon);
        }

        private bool Begin(Slot slot, int towerId, DRTowerLevel level, DRWeapon weapon)
        {
            executing = true;
            var operation = Guid.NewGuid();
            slot.Point.OperationId = operation;
            slot.Point.State = slot.Tower == null ? TowerOperationState.Building : TowerOperationState.Upgrading;
            var old = slot.Tower != null ? slot.Tower.Data : null;
            var pending = new Pending
            {
                Slot = slot, Operation = operation, Config = weapon,
                Data = new TowerEntityData(NewEntityId(), towerId, level, slot.Point, this, operation,
                    old != null ? old.InitialPaid : level.Cost, (old?.UpgradePaid ?? 0) + (old != null ? level.Cost : 0)),
                Deadline = Time.realtimeSinceStartup + Mathf.Max(1f, loadTimeout),
                CooldownDeadline = Time.time + ((slot.Tower?.Combat.CurrentWeapon as WeaponEntity)?.CooldownRemaining ?? 0f)
            };
            slot.Pending = pending;
            bool paid = false;
            try
            {
                paid = wallet.TrySpend(operation, level.Cost);
                if (!paid) { Rollback(pending, "金币不足。", false); return false; }
                // 钱包监听器可能在通知中卸载关卡，此时不再提交加载。
                if (closing || slot.Pending != pending) { wallet.Refund(operation); return false; }
                if (slot.Tower != null) slot.Tower.SetWorking(false);
                Subscribe();
                loads.Add(pending.Data.Id, pending);
                DREntity entity = GameEntry.DataTable.GetDataTable<DREntity>().GetDataRow(level.EntityId);
                GameEntry.Entity.ShowEntity<TowerEntity>(pending.Data.Id,
                    AssetUtility.GetEntityAsset(entity.AssetName), towerGroup, pending.Data);
                LastError = null;
                Notify();
                return true;
            }
            catch (Exception e) { Rollback(pending, e.Message, paid); return false; }
            finally { executing = false; }
        }

        public bool TryDemolish(TowerBuildPoint point)
        {
            if (!Ready(point) || !slots.TryGetValue(point, out Slot slot) || slot.Pending != null ||
                slot.Tower == null || point.State != TowerOperationState.Working) return Fail("防御塔正忙或不存在。");
            executing = true;
            point.OperationId = Guid.NewGuid();
            point.State = TowerOperationState.Demolishing;
            try
            {
                // 先从服务移除，Hide 回调和余额通知均不能重复进入此交易。
                slots.Remove(point);
                slot.Tower.SetWorking(false);
                Hide(slot.WeaponId);
                Hide(slot.Tower.Id);
                foreach (Guid payment in slot.Payments) wallet.Refund(payment);
                ResetPoint(point);
                LastError = null;
                Notify();
                return true;
            }
            finally { executing = false; }
        }

        private bool Ready(TowerBuildPoint point) => !closing && !executing && isActiveAndEnabled &&
            wallet != null && point != null && point.isActiveAndEnabled && GameEntry.Entity != null &&
            GameEntry.Event != null && GameEntry.DataTable != null;

        private bool Validate(DRTowerLevel level, out DRWeapon weapon)
        {
            weapon = GameEntry.DataTable.GetDataTable<DRWeapon>()?.GetDataRow(level.WeaponId);
            var entities = GameEntry.DataTable.GetDataTable<DREntity>();
            if (level.Cost < 0 || !Positive(level.Range) || !Positive(level.TurnSpeed) ||
                weapon == null || weapon.Attack <= 0 || !Positive(weapon.FireInterval) ||
                weapon.AreaRadius < 0 || float.IsNaN(weapon.AreaRadius) || float.IsInfinity(weapon.AreaRadius) ||
                string.IsNullOrEmpty(entities?.GetDataRow(level.EntityId)?.AssetName) ||
                string.IsNullOrEmpty(entities?.GetDataRow(weapon.EntityId)?.AssetName) ||
                GameEntry.Entity.GetEntityGroup(towerGroup) == null || GameEntry.Entity.GetEntityGroup(weaponGroup) == null)
                return Fail("塔/武器配置或 Entity Group 无效。");
            if (weapon.AreaRadius == 0f && (weapon.PenetrationCount != 0 || !Positive(weapon.BulletSpeed) ||
                string.IsNullOrEmpty(entities.GetDataRow(weapon.BulletEntityId)?.AssetName) ||
                GameEntry.Entity.GetEntityGroup(bulletGroup) == null)) return Fail("单体塔需要有效子弹、Bullet 组和零穿透配置。");
            return true;
        }

        private static bool Positive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        private void Subscribe()
        {
            if (subscribed) return;
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
            subscribed = true;
        }

        private void OnShown(object sender, GameEventArgs args)
        {
            var e = (ShowEntitySuccessEventArgs)args;
            if (!loads.TryGetValue(e.Entity.Id, out Pending p)) return;
            bool towerResult = e.Entity.Id == p.Data.Id;
            if (!ReferenceEquals(e.UserData, towerResult ? (object)p.Data : p.WeaponData)) return;
            if (!Current(p)) { Rollback(p, "建造操作已取消。"); return; }
            executing = true;
            try
            {
                if (towerResult)
                {
                    loads.Remove(p.Data.Id);
                    p.Tower = e.Entity.Logic as TowerEntity;
                    if (p.Tower == null || p.Tower.Data != p.Data) throw new InvalidOperationException("塔实体初始化失败。");
                    p.WeaponId = NewEntityId();
                    // 未提交前不装备，避免影响旧塔或在加载回调之前攻击。
                    p.WeaponData = p.Config.AreaRadius > 0f
                        ? new WeaponEntityData(p.WeaponId, p.Config.EntityId, p.Config.Id, null, p.Config)
                        : new ProjectileWeaponEntityData(p.WeaponId, p.Config.EntityId, p.Config.Id, null,
                            muzzlePath, bulletGroup, Mathf.Max(5f, p.Data.Level.Range / p.Config.BulletSpeed + 1f), weaponConfig: p.Config);
                    loads.Add(p.WeaponId, p);
                    var config = GameEntry.DataTable.GetDataTable<DREntity>().GetDataRow(p.Config.EntityId);
                    GameEntry.Entity.ShowEntity(p.WeaponId, p.Config.AreaRadius > 0f ? typeof(AreaWeaponEntity) : typeof(ProjectileWeaponEntity),
                        AssetUtility.GetEntityAsset(config.AssetName), weaponGroup, p.WeaponData);
                }
                else
                {
                    var weapon = e.Entity.Logic as WeaponEntity;
                    if (weapon == null || weapon.WeaponData != p.Config || p.Tower == null || !p.Tower.Available)
                        throw new InvalidOperationException("武器初始化失败。");
                    GameEntry.Entity.AttachEntity(e.Entity, p.Tower.Entity, p.Tower.TowerCombat.WeaponMount);
                    e.Entity.transform.localPosition = Vector3.zero;
                    e.Entity.transform.localRotation = Quaternion.identity;
                    weapon.ConfigureTowerWeapon(p.CooldownDeadline);
                    if (!p.Tower.Combat.EquipWeapon(weapon)) throw new InvalidOperationException("装备武器失败。");
                    Commit(p);
                }
            }
            catch (Exception ex) { Rollback(p, ex.Message); }
            finally { executing = false; }
        }

        private bool Current(Pending p) => !closing && p.Slot.Point != null && p.Slot.Point.isActiveAndEnabled &&
            p.Slot.Pending == p && p.Slot.Point.OperationId == p.Operation;

        private void Commit(Pending p)
        {
            Slot slot = p.Slot;
            var previous = slot.Tower;
            int previousWeapon = slot.WeaponId;
            loads.Remove(p.Data.Id);
            loads.Remove(p.WeaponId);
            slot.Pending = null;
            slot.Tower = p.Tower;
            slot.WeaponId = p.WeaponId;
            slot.Payments.Add(p.Operation);
            slot.Point.Tower = p.Tower;
            slot.Point.State = TowerOperationState.Working;
            Hide(previousWeapon);
            if (previous != null) Hide(previous.Id);
            p.Tower.SetWorking(true);
            LastError = null;
            Notify();
        }

        private void OnFailed(object sender, GameEventArgs args)
        {
            var e = (ShowEntityFailureEventArgs)args;
            if (loads.TryGetValue(e.EntityId, out Pending p) &&
                (ReferenceEquals(e.UserData, p.Data) || ReferenceEquals(e.UserData, p.WeaponData)))
                Rollback(p, e.ErrorMessage);
        }

        private void Rollback(Pending p, string reason, bool refund = true, int alreadyHiding = 0)
        {
            if (p.Slot.Pending != p) return;
            p.Slot.Pending = null;
            loads.Remove(p.Data.Id);
            loads.Remove(p.WeaponId);
            Hide(p.WeaponId);
            if (p.Data.Id != alreadyHiding) Hide(p.Data.Id);
            // 保持点位锁直到退款通知结束，避免可重入建造。
            if (refund && wallet != null) wallet.Refund(p.Operation);
            if (!slots.TryGetValue(p.Slot.Point, out Slot registered) || registered != p.Slot)
            {
                LastError = reason;
                Notify();
                return;
            }
            if (p.Slot.Tower != null && p.Slot.Point != null)
            {
                p.Slot.Tower.SetWorking(true);
                p.Slot.Point.State = TowerOperationState.Working;
            }
            else
            {
                slots.Remove(p.Slot.Point);
                ResetPoint(p.Slot.Point);
            }
            LastError = reason;
            Notify();
        }

        internal void OnTowerHidden(TowerEntity tower, TowerEntityData data)
        {
            if (data.BuildPoint == null || !slots.TryGetValue(data.BuildPoint, out Slot slot)) return;
            if (slot.Tower == tower) ReleasePoint(data.BuildPoint, tower.Id);
            else if (slot.Pending != null && slot.Pending.Data == data) Rollback(slot.Pending, "加载中的塔被回收。", true, tower.Id);
        }

        public void ReleasePoint(TowerBuildPoint point) => ReleasePoint(point, 0);

        private void ReleasePoint(TowerBuildPoint point, int alreadyHiding)
        {
            if (ReferenceEquals(point, null) || !slots.TryGetValue(point, out Slot slot)) return;
            slots.Remove(point);
            Pending pending = slot.Pending;
            slot.Pending = null;
            if (slot.Tower != null) slot.Tower.SetWorking(false);
            if (pending != null)
            {
                loads.Remove(pending.Data.Id);
                loads.Remove(pending.WeaponId);
                Hide(pending.WeaponId);
                if (pending.Data.Id != alreadyHiding) Hide(pending.Data.Id);
                if (wallet != null) wallet.Refund(pending.Operation);
            }
            Hide(slot.WeaponId);
            if (slot.Tower != null && slot.Tower.Id != alreadyHiding) Hide(slot.Tower.Id);
            if (wallet != null) foreach (Guid payment in slot.Payments) wallet.ForgetPayment(payment);
            ResetPoint(point);
            Notify();
        }

        public void ShutdownLevel()
        {
            closing = true;
            foreach (var point in new List<TowerBuildPoint>(slots.Keys)) ReleasePoint(point);
        }

        public static void ShutdownAll()
        {
            foreach (var service in new List<TowerService>(services)) if (service != null) service.ShutdownLevel();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup < nextHealthCheck) return;
            nextHealthCheck = Time.realtimeSinceStartup + 0.2f;
            foreach (var slot in new List<Slot>(slots.Values))
            {
                if (slot.Pending != null && Time.realtimeSinceStartup >= slot.Pending.Deadline)
                    Rollback(slot.Pending, "实体加载超时，已退回本次费用。");
                else if (slot.Tower != null && slot.Pending == null &&
                    (slot.Tower.Combat.CurrentWeapon == null || !GameEntry.Entity.HasEntity(slot.WeaponId)))
                    ReleasePoint(slot.Point);
            }
        }

        private void OnDisable()
        {
            ShutdownLevel();
            services.Remove(this);
            if (subscribed && GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
                GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
            }
            subscribed = false;
        }

        private static void ResetPoint(TowerBuildPoint point)
        {
            if (point == null) return;
            point.Tower = null;
            point.Service = null;
            point.State = TowerOperationState.Empty;
            point.OperationId = Guid.Empty;
        }

        private static int NewEntityId()
        {
            do { if (nextEntityId == int.MinValue) nextEntityId = 0; --nextEntityId; }
            while (GameEntry.Entity.HasEntity(nextEntityId) || GameEntry.Entity.IsLoadingEntity(nextEntityId));
            return nextEntityId;
        }

        private static void Hide(int id)
        {
            if (id != 0 && GameEntry.Entity != null && (GameEntry.Entity.HasEntity(id) || GameEntry.Entity.IsLoadingEntity(id)))
                GameEntry.Entity.HideEntity(id);
        }

        private bool Fail(string reason) { LastError = reason; return false; }
        private void Notify()
        {
            if (Changed == null) return;
            foreach (Action listener in Changed.GetInvocationList())
                try { listener(); } catch (Exception e) { Debug.LogException(e); }
        }
    }
}
