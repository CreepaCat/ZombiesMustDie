using System;
using UnityEngine;

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
        [SerializeField, Min(1)] private int totalWaves = 3;
        [SerializeField, Min(0f)] private float preparationDuration = 5f;

        public LevelState State { get; private set; }
        public int CurrentWave { get; private set; }
        public int TotalWaves => totalWaves;
        public float PreparationRemaining { get; private set; }

        /// <summary>
        /// 准备结束时请求生成当前波敌人，波次从一开始。
        /// </summary>
        public event Action<int> WaveStarted;

        /// <summary>
        /// 本局结束时触发，参数表示是否获胜。
        /// </summary>
        public event Action<bool> LevelEnded;

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

            if (CurrentWave >= totalWaves)
            {
                State = LevelState.Victory;
                LevelEnded?.Invoke(true);
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
            LevelEnded?.Invoke(false);
        }

        private void PrepareWave()
        {
            State = LevelState.Preparing;
            PreparationRemaining = Mathf.Max(0f, preparationDuration);
        }

        private void OnValidate()
        {
            totalWaves = Mathf.Max(1, totalWaves);
            preparationDuration = Mathf.Max(0f, preparationDuration);
        }
    }
}
