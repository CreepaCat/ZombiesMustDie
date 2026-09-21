using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.AI;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 在spawnPoint位置按间隔生成每波敌人，全部生成并清场后推进关卡。
    /// </summary>
    [RequireComponent(typeof(LevelController))]
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private int enemyTypeId = 10101;
        [SerializeField] private int characterId = 101;
        [SerializeField, Min(1)] private int enemiesPerWave = 5;
        [SerializeField, Min(0f)] private float spawnInterval = 1f;
        [SerializeField] Transform spawnPoint = null;

        private static int nextEntityId = -1000000;
        private readonly Dictionary<int, EnemyEntityData> pending = new Dictionary<int, EnemyEntityData>();
        private readonly List<EnemyEntity> enemies = new List<EnemyEntity>();
        private LevelController levelController;
        private int remainingToSpawn;
        private float spawnTimer;

        /// <summary>
        /// 注册关卡及实体加载事件，加载成功前不计入存活敌人。
        /// </summary>
        private void OnEnable()
        {
            levelController = GetComponent<LevelController>();
            levelController.WaveStarted += StartWave;
            levelController.LevelEnded += OnLevelEnded;
            if (GameEntry.Event != null)
            {
                GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
                GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
            }
            if (GameEntry.Event == null || GameEntry.Entity == null)
            {
                Log.Error("EnemySpawner 需要先初始化 GameFramework。");
                enabled = false;
                return;
            }
            if (levelController.State == LevelState.Fighting)
                StartWave(levelController.CurrentWave);
        }

        /// <summary>
        /// 禁用时结束本局并取消本刷怪器持有的实体，避免后台继续生成。
        /// </summary>
        private void OnDisable()
        {
            levelController.WaveStarted -= StartWave;
            levelController.LevelEnded -= OnLevelEnded;
            levelController.FailLevel();
            ClearEnemies();
            if (GameEntry.Event == null) return;
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
        }

        private void StartWave(int wave)
        {
            Debug.Log("波次开始，当前波数：" + wave);
            remainingToSpawn = Mathf.Max(1, enemiesPerWave);
            spawnTimer = 0f;
        }

        /// <summary>
        /// 按间隔发送生成请求，回收死亡敌人，并等待全部异步请求完成后判定清场。
        /// </summary>
        private void Update()
        {
            if (levelController.State != LevelState.Fighting) return;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyEntity enemy = enemies[i];
                if (enemy != null && enemy.Available && !enemy.CanBeRecycle) continue;
                enemies.RemoveAt(i);
                if (enemy != null && enemy.Available)
                    GameEntry.Entity.HideEntity(enemy.Entity);
            }

            spawnTimer -= Time.deltaTime;
            if (remainingToSpawn > 0 && spawnTimer <= 0f)
            {
                remainingToSpawn--;
                spawnTimer = Mathf.Max(0f, spawnInterval);
                SpawnEnemy();
            }
            if (remainingToSpawn == 0 && pending.Count == 0 && enemies.Count == 0)
                levelController.CompleteWave(levelController.CurrentWave);
        }

        /// <summary>
        /// 使用现有实体表与 Character 分组生成敌人，出生点必须位于导航网格附近。
        /// </summary>
        private void SpawnEnemy()
        {
            DREntity row = GameEntry.DataTable?.GetDataTable<DREntity>()?.GetDataRow(enemyTypeId);
            if (row == null || string.IsNullOrEmpty(row.AssetName) ||
                !GameEntry.Entity.HasEntityGroup("Character"))
            {
                Log.Error("无法生成敌人 {0}：请检查 Entity 表与 Character 实体组。", enemyTypeId);
                levelController.FailLevel();
                return;
            }
            if (!NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Log.Error("EnemySpawner 出生点附近没有可用导航网格。请将刷怪器放在已烘焙区域内。");
                levelController.FailLevel();
                return;
            }

            int id = GenerateEntityId();
            DRCharacter crow = GameEntry.DataTable?.GetDataTable<DRCharacter>()?.GetDataRow(characterId);

            var data = new EnemyEntityData(crow.MoveSpeed, 1000f, 0.5f, crow.MaxHP, id, enemyTypeId)
            {
                Position = hit.position,
                Rotation = transform.rotation
            };
            pending.Add(id, data);
            GameEntry.Entity.ShowEntity<EnemyEntity>(id,
                AssetUtility.GetEntityAsset(row.AssetName), "Character", data);
        }

        /// <summary>
        /// 分配未被已显示实体或异步请求占用的本地实体编号。
        /// </summary>
        private static int GenerateEntityId()
        {
            while (true)
            {
                if (nextEntityId == int.MinValue) nextEntityId = -1000000;
                int id = nextEntityId--;
                if (!GameEntry.Entity.HasEntity(id) && !GameEntry.Entity.IsLoadingEntity(id))
                    return id;
            }
        }

        /// <summary>
        /// 加载成功后放置敌人并恢复复用状态；只处理本刷怪器发出的请求。
        /// </summary>
        private void OnShown(object sender, GameEventArgs args)
        {
            var e = (ShowEntitySuccessEventArgs)args;
            if (!pending.TryGetValue(e.Entity.Id, out EnemyEntityData data) ||
                !ReferenceEquals(data, e.UserData)) return;
            pending.Remove(e.Entity.Id);

            var enemy = e.Entity.Logic as EnemyEntity;
            var agent = enemy != null ? enemy.GetComponent<NavMeshAgent>() : null;
            if (levelController.State != LevelState.Fighting || agent == null ||
                !agent.enabled || !agent.Warp(data.Position))
            {
                GameEntry.Entity.HideEntity(e.Entity);
                Log.Error("敌人 {0} 无法放置到出生点，本局停止生成。", e.Entity.Id);
                levelController.FailLevel();
                return;
            }

            enemy.transform.rotation = data.Rotation;
            // enemy.GetComponent<Health>().ResetHealth();
            // Collider collider = enemy.GetComponent<Collider>();
            // if (collider != null) collider.enabled = true;
            // Animator animator = enemy.GetComponentInChildren<Animator>();
            // if (animator != null) animator.Rebind();
            enemies.Add(enemy);
        }

        /// <summary>
        /// 生成失败时终止本局，避免将加载失败误判为已清场。
        /// </summary>
        private void OnFailed(object sender, GameEventArgs args)
        {
            var e = (ShowEntityFailureEventArgs)args;
            if (!pending.TryGetValue(e.EntityId, out EnemyEntityData data) ||
                !ReferenceEquals(data, e.UserData)) return;
            pending.Remove(e.EntityId);
            Log.Error("敌人 {0} 加载失败：{1}", e.EntityId, e.ErrorMessage);
            levelController.FailLevel();
        }

        private void OnLevelEnded(bool victory)
        {
            ClearEnemies();
        }

        /// <summary>
        /// 取消未完成加载并回收存活实体，不影响其他刷怪器或场景实体。
        /// </summary>
        private void ClearEnemies()
        {
            remainingToSpawn = 0;
            if (GameEntry.Entity != null)
            {
                foreach (int id in pending.Keys)
                {
                    if (GameEntry.Entity.IsLoadingEntity(id) || GameEntry.Entity.HasEntity(id))
                        GameEntry.Entity.HideEntity(id);
                }
                foreach (EnemyEntity enemy in enemies)
                {
                    if (enemy != null && enemy.Available)
                        GameEntry.Entity.HideEntity(enemy.Entity);
                }
            }
            pending.Clear();
            enemies.Clear();
        }
    }
}
