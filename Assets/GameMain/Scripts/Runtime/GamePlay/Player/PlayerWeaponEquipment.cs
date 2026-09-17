using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>从角色表装备默认远程武器，同时负责加载取消与实体回收。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatController))]
    public sealed class PlayerWeaponEquipment : MonoBehaviour
    {
        [SerializeField, Min(1)] private int characterId = 1;
        [SerializeField] private string weaponEntityGroup = "Weapon";
        [SerializeField] private string muzzlePath = "Muzzle";
        [SerializeField] private string bulletEntityGroup = "Bullet";
        [SerializeField, Min(0.01f)] private float bulletLifetime = 5f;
        [SerializeField, Min(0.001f)] private float bulletCollisionRadius = 0.05f;
        [SerializeField] private LayerMask bulletHitLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        private static int nextWeaponId;
        private CombatController combat;
        private int weaponEntityId;
        private bool subscribed;
        private bool attempted;

        private void Awake() => combat = GetComponent<CombatController>();
        private void OnEnable() => attempted = false;

        private void Update()
        {
            if (combat.Owner.IsDead)
            {
                ReleaseWeapon();
                return;
            }
            if (attempted || combat.CurrentWeapon != null || GameEntry.Entity == null ||
                GameEntry.Event == null || GameEntry.DataTable == null) return;
            if (GameEntry.DataTable.GetDataTable<DRCharacter>() == null ||
                GameEntry.DataTable.GetDataTable<DRWeapon>() == null ||
                GameEntry.DataTable.GetDataTable<DREntity>() == null) return;
            attempted = true;
            TryEquipDefaultWeapon();
        }

        public bool TryEquipDefaultWeapon()
        {
            if (!isActiveAndEnabled || combat.Owner.IsDead || weaponEntityId != 0 ||
                combat.CurrentWeapon != null || GameEntry.Entity == null ||
                GameEntry.DataTable == null || GameEntry.Event == null) return false;
            DRCharacter character = GameEntry.DataTable.GetDataTable<DRCharacter>()?.GetDataRow(characterId);
            DRWeapon weapon = character != null
                ? GameEntry.DataTable.GetDataTable<DRWeapon>()?.GetDataRow(character.DefaultWeaponId) : null;
            DREntity entity = weapon != null
                ? GameEntry.DataTable.GetDataTable<DREntity>()?.GetDataRow(weapon.EntityId) : null;
            if (entity == null || string.IsNullOrEmpty(entity.AssetName) || combat.WeaponSocket == null ||
                GameEntry.Entity.GetEntityGroup(weaponEntityGroup) == null ||
                GameEntry.Entity.GetEntityGroup(bulletEntityGroup) == null)
            {
                Log.Warning("Cannot equip default weapon: check Character/Weapon/Entity tables, WeaponSocket and entity groups.");
                return false;
            }
            if (!subscribed)
            {
                GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnWeaponShown);
                GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnWeaponFailed);
                subscribed = true;
            }
            do
            {
                if (nextWeaponId == int.MinValue) nextWeaponId = 0;
                weaponEntityId = --nextWeaponId;
            }
            while (GameEntry.Entity.HasEntity(weaponEntityId) || GameEntry.Entity.IsLoadingEntity(weaponEntityId));
            var data = new ProjectileWeaponEntityData(weaponEntityId, weapon.EntityId, weapon.Id, combat,
                muzzlePath, bulletEntityGroup, bulletLifetime, bulletCollisionRadius, bulletHitLayer,
                weapon, triggerInteraction);
            GameEntry.Entity.ShowEntity<RifleWeapon>(weaponEntityId,
                AssetUtility.GetEntityAsset(entity.AssetName), weaponEntityGroup, data);
            return true;
        }

        private void OnWeaponShown(object sender, GameEventArgs args)
        {
            var e = (ShowEntitySuccessEventArgs)args;
            if (e.Entity.Id != weaponEntityId) return;
            if (!isActiveAndEnabled || combat.Owner.IsDead)
            {
                ReleaseWeapon();
                return;
            }
            // Player currently is a MonoBehaviour. Use GF attachment when hosted by an Entity.
            var parent = combat.GetComponentInParent<UnityGameFramework.Runtime.Entity>();
            if (parent != null)
                GameEntry.Entity.AttachEntity(e.Entity, parent, combat.WeaponSocket);
            else
                e.Entity.transform.SetParent(combat.WeaponSocket, false);
            e.Entity.transform.localPosition = Vector3.zero;
            e.Entity.transform.localRotation = Quaternion.identity;
        }

        private void OnWeaponFailed(object sender, GameEventArgs args)
        {
            var e = (ShowEntityFailureEventArgs)args;
            if (e.EntityId != weaponEntityId) return;
            weaponEntityId = 0;
            Log.Warning("Default weapon failed to load: {0}", e.ErrorMessage);
        }

        private void ReleaseWeapon()
        {
            if (weaponEntityId == 0) return;
            int id = weaponEntityId;
            weaponEntityId = 0;
            if (GameEntry.Entity != null &&
                (GameEntry.Entity.HasEntity(id) || GameEntry.Entity.IsLoadingEntity(id)))
            {
                GameEntry.Entity.HideEntity(id);
            }
        }

        private void OnDisable()
        {
            ReleaseWeapon();
            if (subscribed && GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnWeaponShown);
                GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnWeaponFailed);
            }
            subscribed = false;
        }
    }
}
