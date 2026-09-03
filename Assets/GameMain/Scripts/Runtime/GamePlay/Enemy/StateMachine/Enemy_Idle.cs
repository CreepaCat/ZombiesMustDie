using UnityEngine;

namespace ZombiesMustDie
{
    public class Enemy_Idle : EnemyStateBase
    {
        //todo:转换条件
        public Enemy_Idle(Enemy enemy) : base(enemy)
        {

        }
        public override void OnEnter()
        {
            Debug.Log("OnEnter Enemy Idle");

            Enemy.Navigation.StopMoving();
        }
        public override void OnLogicUpdate()
        {
            if (Enemy.Animation.IsInteracting) return;
            // StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
        }

        public override void OnExit()
        {
            Debug.Log("OnExit Enemy Idle");
        }


    }
}
