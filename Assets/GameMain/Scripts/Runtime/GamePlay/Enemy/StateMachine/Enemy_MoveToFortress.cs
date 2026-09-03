using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 向总部基地移动
    /// </summary>
    public class Enemy_MoveToFortress : EnemyStateBase
    {
        public Enemy_MoveToFortress(Enemy enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("OnEnter Enemy_MoveToFortress");
            Enemy.Navigation.RestoreMoving();
            Enemy.Navigation.SetDestination(HeadQuarter.GetPosition());
        }
        public override void OnLogicUpdate()
        {
            if (Enemy.Animation.IsInteracting) return;
            if (Enemy.TargetDetector.GetDistanceToPlayer() < Constant.Enemy.ChaseRange)
            {
                StateMachine.ChangeState(typeof(Enemy_ChasePlayer));
            }
        }

        public override void OnExit()
        {
            Debug.Log("OnExit Enemy_MoveToFortress");
        }


    }
}
