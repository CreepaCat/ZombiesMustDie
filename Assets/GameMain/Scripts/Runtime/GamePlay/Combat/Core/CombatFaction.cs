namespace ZombiesMustDie
{
    public enum CombatFaction
    {
        Neutral = 0,
        Player = 1,
        Enemy = 2
    }

    public enum CombatRelation
    {
        Neutral = 0,
        Friendly = 1,
        Hostile = 2
    }

    public static class CombatFactionExtensions
    {
        public static CombatRelation GetRelationTo(this CombatFaction faction, CombatFaction other)
        {
            if (faction == CombatFaction.Neutral || other == CombatFaction.Neutral)
            {
                return CombatRelation.Neutral;
            }

            if (faction == other)
            {
                return CombatRelation.Friendly;
            }

            bool isPlayerEnemyPair =
                faction == CombatFaction.Player && other == CombatFaction.Enemy ||
                faction == CombatFaction.Enemy && other == CombatFaction.Player;

            return isPlayerEnemyPair ? CombatRelation.Hostile : CombatRelation.Neutral;
        }

        public static bool IsHostileTo(this CombatFaction faction, CombatFaction other)
        {
            return faction.GetRelationTo(other) == CombatRelation.Hostile;
        }
    }
}
