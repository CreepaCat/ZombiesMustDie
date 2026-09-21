using UnityEngine;

namespace ZombiesMustDie
{
    public class Enemy_Death : EnemyStateBase
    {
        public Enemy_Death(EnemyEntity enemy) : base(enemy)
        {
        }
        public override void OnEnter()
        {
            Debug.Log("OnEnter Enemy_Death");
            Enemy.ProcesseDie();
        }

        public override void OnLogicUpdate()
        {

        }

        public override void OnExit()
        {

        }


    }
}
