using System.Collections.Generic;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;
using FrameworkEntity = UnityGameFramework.Runtime.Entity;

namespace ZombiesMustDie
{
    /// <summary>从实体表显示一次性特效，并在异步加载后附着枪口特效。</summary>
    public static class EffectSpawner
    {
        private const int MuzzleFlashTypeId = 40001;
        private const int BulletHitTypeId = 40002;
        private const int AreaTowerAttackTypeId = 40401;
        private const int AreaTowerHitTypeId = 40402;
        private const string EffectGroupName = "Effect";
        private static readonly Dictionary<int, PendingAttachment> Pending = new Dictionary<int, PendingAttachment>();
        private static int nextEffectId = int.MaxValue;
        private static bool subscribed;

        //枪口火焰旋转调整
        private static Vector3 MuzzleFlashRotation = new Vector3(90, 0, 0);

        public static bool ShowAreaTowerAttack(FrameworkEntity weapon, Transform muzzle)
        {
            if (weapon == null || muzzle == null) return false;
            return Show(AreaTowerAttackTypeId, Vector3.zero, Quaternion.identity, 0.15f, 2f,
                new PendingAttachment(weapon, muzzle, Quaternion.Euler(0f, 0f, 0f)));
        }

        public static bool ShowAreaTowerHit(Vector3 point, Vector3 normal)
        {
            Quaternion rotation = normal.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(normal) : Quaternion.identity;
            return Show(AreaTowerHitTypeId, point, rotation, 0.25f, 3f, null);
        }

        public static bool ShowMuzzleFlash(FrameworkEntity weapon, Transform muzzle)
        {
            if (weapon == null || muzzle == null || !muzzle.IsChildOf(weapon.transform)) return false;
            return Show(MuzzleFlashTypeId, Vector3.zero, Quaternion.Euler(MuzzleFlashRotation), 0.15f, 2f,
                new PendingAttachment(weapon, muzzle, Quaternion.Euler(90f, 0f, 0f)));
        }

        public static bool ShowBulletHit(Vector3 point, Vector3 normal)
        {
            Quaternion rotation = normal.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(normal) : Quaternion.identity;
            return Show(BulletHitTypeId, point, rotation, 0.25f, 3f, null);
        }

        private static bool Show(int typeId, Vector3 worldPosition, Quaternion worldRotation,
            float emissionDuration, float maxLifetime, PendingAttachment attachment)
        {
            if (GameEntry.Entity == null || GameEntry.DataTable == null) return false;
            GameFramework.Entity.IEntityGroup group = GameEntry.Entity.GetEntityGroup(EffectGroupName);
            Transform groupTransform = (group?.Helper as Component)?.transform;
            IDataTable<DREntity> table = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity row = table?.GetDataRow(typeId);
            if (groupTransform == null || row == null || string.IsNullOrEmpty(row.AssetName))
            {
                Log.Warning("Cannot show effect '{0}': check Entity table and Effect group.", typeId);
                return false;
            }

            int id = GenerateId();
            if (attachment != null)
            {
                Subscribe();
                Pending.Add(id, attachment);
            }

            Vector3 localPosition = groupTransform.InverseTransformPoint(worldPosition);
            Quaternion localRotation = Quaternion.Inverse(groupTransform.rotation) * worldRotation;
            var data = new ParticleEffectEntityData(id, typeId, localPosition, localRotation,
                attachment != null, attachment != null ? attachment.LocalRotation : Quaternion.identity,
                emissionDuration, maxLifetime);
            GameEntry.Entity.ShowEntity<ParticleEffectEntity>(id,
                AssetUtility.GetEntityAsset(row.AssetName), EffectGroupName, data);
            return true;
        }

        private static int GenerateId()
        {
            int id;
            do
            {
                if (nextEffectId <= 0) nextEffectId = int.MaxValue;
                id = nextEffectId--;
            }
            while (GameEntry.Entity.HasEntity(id) || GameEntry.Entity.IsLoadingEntity(id) || Pending.ContainsKey(id));
            return id;
        }

        private static void Subscribe()
        {
            if (subscribed || GameEntry.Event == null) return;
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
            subscribed = true;
        }

        private static void OnShown(object sender, GameEventArgs args)
        {
            var e = (ShowEntitySuccessEventArgs)args;
            if (!Pending.TryGetValue(e.Entity.Id, out PendingAttachment attachment)) return;
            Pending.Remove(e.Entity.Id);
            UnsubscribeWhenIdle();

            FrameworkEntity parent = attachment.Parent;
            Transform muzzle = attachment.Muzzle;
            if (parent == null || muzzle == null ||
                GameEntry.Entity.GetEntity(parent.Id) != parent ||
                parent.Logic == null || !parent.Logic.Available ||
                !muzzle.IsChildOf(parent.transform))
            {
                GameEntry.Entity.HideEntity(e.Entity);
                return;
            }

            GameEntry.Entity.AttachEntity(e.Entity, parent, muzzle);
        }

        private static void OnFailed(object sender, GameEventArgs args)
        {
            var e = (ShowEntityFailureEventArgs)args;
            if (!Pending.Remove(e.EntityId)) return;
            UnsubscribeWhenIdle();
            Log.Warning("Effect entity '{0}' failed to load: {1}", e.EntityId, e.ErrorMessage);
        }

        private static void UnsubscribeWhenIdle()
        {
            if (!subscribed || Pending.Count != 0 || GameEntry.Event == null) return;
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
            subscribed = false;
        }

        private sealed class PendingAttachment
        {
            public PendingAttachment(FrameworkEntity parent, Transform muzzle, Quaternion localRotation)
            {
                Parent = parent;
                Muzzle = muzzle;
                LocalRotation = localRotation;
            }

            public FrameworkEntity Parent { get; }
            public Transform Muzzle { get; }
            public Quaternion LocalRotation { get; }
        }
    }
}
