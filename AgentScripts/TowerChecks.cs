using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ZombiesMustDie;
using GE = ZombiesMustDie.GameEntry;
using Object = UnityEngine.Object;

public static class TowerChecks
{
    const string Folder = "Assets/GameMain/Prefabs/Entities/__TowerChecks";
    static readonly List<string> passed = new List<string>();
    static readonly List<GameObject> objects = new List<GameObject>();
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed.Add(name); }
    static void Field(object obj, string field, object value) => obj.GetType().GetField(field, Flags).SetValue(obj, value);
    static object Call(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, Flags).Invoke(obj, args);
    static GameObject Make(string name)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(10000, 0, 10000);
        objects.Add(go);
        return go;
    }
    public static object Setup()
    {
        if (EditorApplication.isPlaying) throw new Exception("Setup requires edit mode.");
        if (AssetDatabase.IsValidFolder(Folder)) throw new Exception("Test folder already exists; inspect before cleanup.");
        AssetDatabase.CreateFolder("Assets/GameMain/Prefabs/Entities", "__TowerChecks");
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        try
        {
            var tower = new GameObject("TestTower");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tower, scene);
            tower.AddComponent<TowerEntity>();
            PrefabUtility.SaveAsPrefabAsset(tower, Folder + "/Tower.prefab");
            var weapon = new GameObject("TestWeapon");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(weapon, scene);
            new GameObject("Muzzle").transform.SetParent(weapon.transform, false);
            PrefabUtility.SaveAsPrefabAsset(weapon, Folder + "/Weapon.prefab");
            return "Temporary test prefabs created.";
        }
        finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene); }
    }
    public static object Cleanup()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before cleanup.");
        if (AssetDatabase.IsValidFolder(Folder)) AssetDatabase.DeleteAsset(Folder);
        return "Temporary test prefabs removed.";
    }
    static async Task Until(Func<bool> condition)
    {
        float deadline = Time.realtimeSinceStartup + 8f;
        while (!condition())
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Timed out waiting for Entity lifecycle.");
            await Task.Delay(30);
        }
    }
    static void SetEntity(int id, string path)
    {
        var table = GE.DataTable.GetDataTable<DREntity>();
        table.RemoveDataRow(id);
        table.AddDataRow("\t" + id + "\ttest\t" + path, null);
    }
    static TowerBuildPoint Point()
    {
        var p = Make("TestPoint").AddComponent<TowerBuildPoint>();
        Field(p, "allowedTowerIds", new[] { 2 });
        return p;
    }
    static void PauseTower(TowerBuildPoint point) => Call(point.Tower, "SetWorking", false);

    public static async Task<object> Extra()
    {
        if (!EditorApplication.isPlaying) throw new Exception("Run requires Play Mode.");
        passed.Clear();
        objects.Clear();
        TowerService service = null;
        try
        {
            SetEntity(50002, "__TowerChecks/Tower");
            SetEntity(20401, "__TowerChecks/Weapon");
            SetEntity(50001, "__TowerChecks/Tower");
            SetEntity(20301, "__TowerChecks/Weapon");
            service = Make("ExtraTowerService").AddComponent<TowerService>();
            var wallet = service.Wallet;
            var point = Point();
            Check(!wallet.TrySpend(Guid.NewGuid(), 501) && wallet.Balance == 500, "insufficient funds unchanged");
            Check(service.TryBuild(point, 2), "automatic tower build accepted");
            await Until(() => point.State == TowerOperationState.Working);
            var targetObject = Make("AutomaticTarget");
            targetObject.transform.position += Vector3.forward * 4;
            var target = targetObject.AddComponent<CombatTarget>();
            target.Configure(CombatFaction.Enemy, true, false);
            targetObject.AddComponent<SphereCollider>();
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            objects.Add(wall);
            wall.transform.position = point.SpawnPoint.position + Vector3.forward * 2;
            wall.transform.localScale = new Vector3(3, 3, .3f);
            Physics.SyncTransforms();
            point.Tower.GetComponent<TowerTargetDetector>().Clear();
            await Task.Delay(400);
            Check(target.Health.CurrentHealth == 100, "automatic fire blocked by geometry");
            Object.Destroy(wall);
            await Until(() => target.Health.CurrentHealth < 100);
            PauseTower(point);
            Check(target.Health.CurrentHealth == 70, "automatic chain attacks after obstruction removed");

            // An unlimited tower still observes cooldown with a positive magazine configuration.
            var config = new DRWeapon();
            config.ParseDataRow("\t999\ttest\t\t20401\t0\t0\t1\t0\t0.01\t1\t0\t0\t3", null);
            var area = (WeaponEntity)point.Tower.Combat.CurrentWeapon;
            typeof(WeaponEntity).GetField("weaponData", Flags).SetValue(area, config);
            typeof(WeaponEntity).GetField("currentMagazineAmmo", Flags).SetValue(area, 1);
            area.ConfigureTowerWeapon(Time.time);
            var request = new AttackRequest(targetObject.transform.position, Vector3.forward);
            Check(area.TryAttack(in request), "unlimited tower first shot");
            await Task.Delay(40);
            Check(area.TryAttack(in request) && area.CurrentMagazineAmmo == 1 && !area.TryReload(1),
                "unlimited tower does not consume magazine or reload");
            Check(service.TryDemolish(point), "automatic tower demolished");
            Object.Destroy(targetObject);
            await Task.Delay(40);

            var weaponTable = GE.DataTable.GetDataTable<DRWeapon>();
            weaponTable.RemoveDataRow(301);
            weaponTable.AddDataRow("\t301\tSlowTestBullet\t\t20301\t20003\t30001\t20\t5\t0.5\t0\t0\t0\t0", null);
            var single = Point();
            Field(single, "allowedTowerIds", new[] { 1 });
            Check(service.TryBuild(single, 1), "projectile tower build accepted");
            await Until(() => single.State == TowerOperationState.Working);
            PauseTower(single);
            var bulletTargetGo = Make("BulletTarget");
            bulletTargetGo.transform.position += Vector3.forward * 4;
            var bulletTarget = bulletTargetGo.AddComponent<CombatTarget>();
            bulletTarget.Configure(CombatFaction.Enemy, true, false);
            bulletTargetGo.AddComponent<SphereCollider>();
            Physics.SyncTransforms();
            var shot = new AttackRequest(bulletTargetGo.transform.position, Vector3.forward);
            var gun = (ProjectileWeaponEntity)single.Tower.Combat.CurrentWeapon;
            Check(gun.TryAttack(in shot), "projectile emitted through Entity");
            int bulletId = gun.LastBulletEntityId;
            Check(service.TryUpgrade(single), "upgrade while bullet in flight");
            await Until(() => single.State == TowerOperationState.Working);
            PauseTower(single);
            await Until(() => bulletTarget.Health.CurrentHealth < 100);
            Check(bulletTarget.Health.CurrentHealth == 80, "in-flight bullet retains original damage after upgrade");
            await Until(() => !GE.Entity.HasEntity(bulletId) && !GE.Entity.IsLoadingEntity(bulletId));
            Check(service.TryDemolish(single), "projectile tower demolished");

            // Missing tower asset exercises the first loading stage.
            SetEntity(50002, "__TowerChecks/MissingTower");
            Check(service.TryBuild(point, 2), "missing tower transaction accepted");
            await Until(() => point.State == TowerOperationState.Empty);
            Check(wallet.Balance == 500, "tower load failure refunds in full");
            SetEntity(50002, "__TowerChecks/Tower");

            Check(service.TryBuild(point, 2), "timeout transaction accepted");
            var slots = (System.Collections.IDictionary)typeof(TowerService).GetField("slots", Flags).GetValue(service);
            var slot = slots[point];
            var pending = slot.GetType().GetField("Pending").GetValue(slot);
            pending.GetType().GetField("Deadline").SetValue(pending, -1f);
            Field(service, "nextHealthCheck", 0f);
            Call(service, "Update");
            await Task.Delay(250);
            Check(point.Tower == null && point.State == TowerOperationState.Empty && wallet.Balance == 500,
                "timeout refunds once and ignores stale load");
            return new { success = true, count = passed.Count, passed = passed.ToArray() };
        }
        catch (Exception e) { return new { success = false, error = e.ToString(), passed = passed.ToArray() }; }
        finally
        {
            if (service != null) service.ShutdownLevel();
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            SetEntity(50002, "Tower/AreaTower"); SetEntity(20401, "Weapon/TowerArea");
            SetEntity(50001, "Tower/SingleTower"); SetEntity(20301, "Weapon/TowerGun");
            var table = GE.DataTable.GetDataTable<DRWeapon>();
            table.RemoveDataRow(301);
            table.AddDataRow("\t301\tTowerGun1\t单体塔一级\t20301\t20003\t30001\t20\t100\t0.5\t0\t0\t0\t0", null);
        }
    }

    public static async Task<object> Run()
    {
        if (!EditorApplication.isPlaying) throw new Exception("Run requires Play Mode.");
        await Until(() => GE.DataTable != null && GE.DataTable.GetDataTable<DRTower>() != null &&
            GE.DataTable.GetDataTable<DRTowerLevel>()?.Count == 4 && GE.Entity != null);
        passed.Clear();
        objects.Clear();
        TowerService service = null;
        try
        {
            Check(GE.DataTable.GetDataTable<DRTower>().Count == 2 &&
                GE.DataTable.GetDataTable<DRWeapon>().GetDataRow(401).AreaRadius == 3f &&
                GE.DataTable.GetDataTable<DRUIForm>().GetDataRow(104) != null, "preload tower/weapon/UI tables");
            foreach (string name in new[] { "Tower", "Weapon", "Bullet" })
                if (GE.Entity.GetEntityGroup(name) == null) GE.Entity.AddEntityGroup(name, 60, 16, 60, 0);
            SetEntity(50002, "__TowerChecks/Tower");
            SetEntity(20401, "__TowerChecks/Weapon");

            service = Make("TestTowerService").AddComponent<TowerService>();
            PlayerWallet wallet = service.Wallet;
            long initial = wallet.Balance;
            Guid receipt = Guid.NewGuid();
            Check(wallet.TrySpend(receipt, 10) && !wallet.TrySpend(receipt, 10) &&
                wallet.Balance == initial - 10, "wallet duplicate debit rejected");
            Check(wallet.Refund(receipt) && !wallet.Refund(receipt) && !wallet.TrySpend(receipt, 10) &&
                wallet.Balance == initial, "wallet exactly-once refund and replay rejection");

            var point = Point();
            Check(service.TryBuild(point, 2) && !service.TryBuild(point, 2) &&
                wallet.Balance == initial - 150, "build locks point and charges once");
            await Until(() => point.State == TowerOperationState.Working || point.State == TowerOperationState.Empty);
            Check(point.Tower != null, "GF tower and weapon load and attach");
            PauseTower(point);
            var firstTower = point.Tower;
            Check(firstTower.Combat.Owner.Faction == CombatFaction.Player &&
                !firstTower.Combat.Owner.IsTargetable && firstTower.Combat.Owner.Invulnerable, "tower faction and target flags");
            var damage = new DamageInfo(999, null, null, CombatFaction.Enemy, Vector3.zero, Vector3.forward);
            Check(!firstTower.Combat.Owner.TakeDamage(in damage).Succeeded &&
                !firstTower.Combat.Owner.IsDead, "tower ignores incoming damage");

            var targetObject = Make("EnemyWithTwoColliders");
            targetObject.transform.position += Vector3.forward * 3;
            var target = targetObject.AddComponent<CombatTarget>();
            target.Configure(CombatFaction.Enemy, true, false);
            targetObject.AddComponent<SphereCollider>();
            targetObject.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            firstTower.GetComponent<TowerTargetDetector>().Clear();
            Check(firstTower.GetComponent<TowerTargetDetector>().FindTarget(10) == target, "detector finds living enemy");
            var request = new AttackRequest(targetObject.transform.position, Vector3.forward);
            var weapon = (WeaponEntity)firstTower.Combat.CurrentWeapon;
            Check(weapon.TryAttack(in request) && target.Health.CurrentHealth == 70, "area damage deduplicates colliders");
            Check(!weapon.TryAttack(in request) && target.Health.CurrentHealth == 70, "weapon cooldown prevents repeated hit");
            float deadline = Time.time + weapon.CooldownRemaining;
            Check(service.TryUpgrade(point) && !service.TryUpgrade(point) && !service.TryDemolish(point) &&
                wallet.Balance == initial - 350, "upgrade locks all operations and charges once");
            await Until(() => point.State == TowerOperationState.Working);
            PauseTower(point);
            var upgraded = (WeaponEntity)point.Tower.Combat.CurrentWeapon;
            Check(point.Tower != firstTower && point.Tower.Data.Level.Level == 2 &&
                point.Tower.Data.TotalPaid == 350, "upgrade commits entity and paid totals");
            Check(Mathf.Abs((Time.time + upgraded.CooldownRemaining) - Mathf.Max(Time.time, deadline)) < .12f,
                "upgrade preserves weapon cooldown deadline");
            Check(!service.TryUpgrade(point), "max level rejects upgrade");
            await Until(() => upgraded.CooldownRemaining <= 0f);
            Check(upgraded.TryAttack(in request) && target.Health.CurrentHealth == 20, "upgraded area damage uses new config");

            // Mutating configuration after payment must never alter the refund.
            var levels = GE.DataTable.GetDataTable<DRTowerLevel>();
            levels.RemoveDataRow(2002);
            levels.AddDataRow("\t2002\ttest\t2\t50002\t402\t12\t180\t999\t0", null);
            Check(service.TryDemolish(point) && !service.TryDemolish(point) &&
                wallet.Balance == initial && point.State == TowerOperationState.Empty, "demolition refunds actual spend exactly once");
            levels.RemoveDataRow(2002);
            levels.AddDataRow("\t2002\ttest\t2\t50002\t402\t12\t180\t200\t0", null);
            Object.Destroy(targetObject);
            await Task.Delay(50);

            // Missing asset produces an actual GF asynchronous failure.
            SetEntity(20401, "__TowerChecks/MissingWeapon");
            Check(service.TryBuild(point, 2), "failed-build transaction accepted");
            await Until(() => point.State == TowerOperationState.Empty);
            Check(wallet.Balance == initial && point.Tower == null, "weapon load failure refunds and releases point");
            SetEntity(20401, "__TowerChecks/Weapon");
            Check(service.TryBuild(point, 2), "rebuild after failure");
            await Until(() => point.State == TowerOperationState.Working);
            PauseTower(point);
            var oldTower = point.Tower;
            SetEntity(20401, "__TowerChecks/MissingWeapon");
            Check(service.TryUpgrade(point), "failed-upgrade transaction accepted");
            await Until(() => point.State == TowerOperationState.Working);
            Check(point.Tower == oldTower && point.Tower.Data.Level.Level == 1 &&
                point.Tower.IsWorking && wallet.Balance == initial - 150, "failed upgrade restores old tower and refunds only upgrade");
            SetEntity(20401, "__TowerChecks/Weapon");

            var pendingPoint = Point();
            Check(service.TryBuild(pendingPoint, 2), "pending build started");
            service.ReleasePoint(pendingPoint);
            await Task.Delay(250);
            Check(pendingPoint.Tower == null && pendingPoint.State == TowerOperationState.Empty &&
                wallet.Balance == initial - 150, "cancelled load cannot resurrect tower or retain charge");
            int towerId = point.Tower.Id;
            int weaponId = ((WeaponEntity)point.Tower.Combat.CurrentWeapon).Id;
            service.ShutdownLevel();
            Check(wallet.Balance == initial - 150 && point.State == TowerOperationState.Empty &&
                !GE.Entity.HasEntity(towerId) && !GE.Entity.HasEntity(weaponId), "level exit hides entities without demolition refund");
            return new { success = true, count = passed.Count, passed = passed.ToArray() };
        }
        catch (Exception e) { return new { success = false, error = e.ToString(), passed = passed.ToArray() }; }
        finally
        {
            if (service != null) service.ShutdownLevel();
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            SetEntity(50002, "Tower/AreaTower");
            SetEntity(20401, "Weapon/TowerArea");
        }
    }
}
