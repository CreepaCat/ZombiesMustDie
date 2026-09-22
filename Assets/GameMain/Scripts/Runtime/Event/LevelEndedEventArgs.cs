using GameFramework;
using GameFramework.Event;

namespace ZombiesMustDie
{
    /// <summary>
    /// 关卡结束通知，由关卡控制器更新胜负状态后发布。
    /// </summary>
    public sealed class LevelEndedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LevelEndedEventArgs).GetHashCode();

        public override int Id => EventId;
        public bool IsVictory { get; private set; }

        /// <summary>
        /// 从引用池取得事件参数，发布后由事件系统负责回收。
        /// </summary>
        public static LevelEndedEventArgs Create(bool isVictory)
        {
            LevelEndedEventArgs args = ReferencePool.Acquire<LevelEndedEventArgs>();
            args.IsVictory = isVictory;
            return args;
        }

        public override void Clear()
        {
            IsVictory = false;
        }
    }
}
