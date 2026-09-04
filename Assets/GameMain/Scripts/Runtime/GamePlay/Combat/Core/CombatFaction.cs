namespace ZombiesMustDie
{
    /// <summary>
    /// 战斗单位所属的阵营。
    /// </summary>
    public enum CombatFaction
    {
        /// <summary>
        /// 中立阵营，不会与玩家或敌人建立敌对关系。
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// 玩家阵营。
        /// </summary>
        Player = 1,

        /// <summary>
        /// 敌人阵营。
        /// </summary>
        Enemy = 2
    }

    /// <summary>
    /// 两个战斗阵营之间的关系。
    /// </summary>
    public enum CombatRelation
    {
        /// <summary>
        /// 中立关系，双方不会互相造成战斗伤害。
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// 友方关系，双方属于同一阵营。
        /// </summary>
        Friendly = 1,

        /// <summary>
        /// 敌对关系，双方可以互相造成战斗伤害。
        /// </summary>
        Hostile = 2
    }

    /// <summary>
    /// 提供阵营关系判定的扩展方法。
    /// </summary>
    public static class CombatFactionExtensions
    {
        /// <summary>
        /// 获取当前阵营与目标阵营之间的关系。
        /// </summary>
        /// <param name="faction">发起关系判断的阵营。</param>
        /// <param name="other">需要比较的目标阵营。</param>
        /// <returns>两个阵营之间的关系。</returns>
        public static CombatRelation GetRelationTo(this CombatFaction faction, CombatFaction other)
        {
            // 只要任意一方为中立阵营，双方关系就是中立。
            if (faction == CombatFaction.Neutral || other == CombatFaction.Neutral)
            {
                return CombatRelation.Neutral;
            }

            // 相同的非中立阵营互为友方。
            if (faction == other)
            {
                return CombatRelation.Friendly;
            }

            // 当前只有玩家与敌人之间属于敌对关系。
            bool isPlayerEnemyPair =
                faction == CombatFaction.Player && other == CombatFaction.Enemy ||
                faction == CombatFaction.Enemy && other == CombatFaction.Player;

            return isPlayerEnemyPair ? CombatRelation.Hostile : CombatRelation.Neutral;
        }

        /// <summary>
        /// 判断当前阵营是否与目标阵营敌对。
        /// </summary>
        /// <param name="faction">发起判断的阵营。</param>
        /// <param name="other">需要比较的目标阵营。</param>
        /// <returns>双方敌对时返回 true，否则返回 false。</returns>
        public static bool IsHostileTo(this CombatFaction faction, CombatFaction other)
        {
            return faction.GetRelationTo(other) == CombatRelation.Hostile;
        }
    }
}
