using System;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 单局关卡的运行阶段。
    /// </summary>
    public enum LevelState
    {
        Idle,
        Preparing,
        Fighting,
        Victory,
        Defeat
    }

    /// <summary>
    /// 管理准备倒计时、波次推进和胜负，具体刷怪与清场统计由外部负责。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float preparationDuration = 5f;

        public LevelState State { get; private set; }
        public int CurrentWave { get; private set; }
        public int TotalWaves { get; private set; }
        public float PreparationRemaining { get; private set; }

        /// <summary>
        /// 准备结束时请求生成当前波敌人，波次从一开始。
        /// </summary>
        public event Action<int> WaveStarted;

        private void Start()
        {
            StartLevel();
        }

        /// <summary>
        /// 准备时间耗尽后进入战斗，并通知外部开始刷怪。
        /// </summary>
        private void Update()
        {
            if (State != LevelState.Preparing)
            {
                return;
            }

            PreparationRemaining = Mathf.Max(0f, PreparationRemaining - Time.deltaTime);
            if (PreparationRemaining > 0f)
            {
                return;
            }

            State = LevelState.Fighting;
            WaveStarted?.Invoke(CurrentWave);
        }

        /// <summary>
        /// 启动本局；重复调用不会重置战斗，重新开局通过重新加载场景完成。
        /// </summary>
        public void StartLevel()
        {
            if (State != LevelState.Idle)
            {
                return;
            }

            if (!LoadWaveConfiguration())
            {
                State = LevelState.Defeat;
                GameEntry.Event.Fire(this, LevelEndedEventArgs.Create(false));
                return;
            }

            CurrentWave = 1;
            PrepareWave();
        }

        /// <summary>
        /// 外部确认本波已全部生成、无待完成生成请求且存活数为零后调用。
        /// 波次校验用于忽略重复通知和上一波的延迟通知。
        /// </summary>
        public void CompleteWave(int wave)
        {
            if (State != LevelState.Fighting || wave != CurrentWave)
            {
                return;
            }

            if (CurrentWave >= TotalWaves)
            {
                State = LevelState.Victory;
                GameEntry.Event.Fire(this, LevelEndedEventArgs.Create(true));
                return;
            }

            CurrentWave++;
            PrepareWave();
        }

        /// <summary>
        /// 基地被摧毁等失败条件成立时由外部调用，本局只结算一次。
        /// </summary>
        public void FailLevel()
        {
            if (State != LevelState.Preparing && State != LevelState.Fighting)
            {
                return;
            }

            PreparationRemaining = 0f;
            State = LevelState.Defeat;
            GameEntry.Event.Fire(this, LevelEndedEventArgs.Create(false));
        }

        /// <summary>
        /// 开局校验波次连续性、数量、间隔和角色实体引用，并从配置取得总波数。
        /// </summary>
        private bool LoadWaveConfiguration()
        {
            var table = GameEntry.DataTable?.GetDataTable<DREnemyWave>();
            var characters = GameEntry.DataTable?.GetDataTable<DRCharacter>();
            var entities = GameEntry.DataTable?.GetDataTable<DREntity>();
            if (table == null || characters == null || entities == null || table.Count == 0)
            {
                Log.Error("关卡配置缺失：请预加载 EnemyWave、Character 和 Entity 表，且波次表不能为空。");
                return false;
            }

            DREnemyWave[] rows = table.GetAllDataRows();
            Array.Sort(rows, (a, b) => a.Wave.CompareTo(b.Wave));
            int lastWave = 0;
            foreach (DREnemyWave row in rows)
            {
                DRCharacter character = characters.GetDataRow(row.CharacterId);
                DREntity entity = character != null ? entities.GetDataRow(character.EntityId) : null;
                if (row.Wave < 1 || row.Wave > lastWave + 1 || row.Count <= 0 ||
                    float.IsNaN(row.SpawnInterval) || float.IsInfinity(row.SpawnInterval) ||
                    row.SpawnInterval < 0f || entity == null || string.IsNullOrEmpty(entity.AssetName))
                {
                    Log.Error("EnemyWave 配置行 {0} 无效：检查连续波次、数量、生成间隔和角色实体引用。", row.Id);
                    return false;
                }
                lastWave = row.Wave;
            }
            TotalWaves = lastWave;
            return true;
        }

        private void PrepareWave()
        {
            State = LevelState.Preparing;
            PreparationRemaining = Mathf.Max(0f, preparationDuration);
        }

        private void OnValidate()
        {
            preparationDuration = Mathf.Max(0f, preparationDuration);
        }
    }
}
