using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 追击玩家
    /// </summary>
    public class Enemy_ChasePlayer : EnemyStateBase
    {
        public Enemy_ChasePlayer(Enemy enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {

            Debug.Log("OnEnter Enemy_ChasePlayer");
            Enemy.Navigation.RestoreMoving();
            Enemy.Navigation.SetDestination(Enemy.TargetDetector.GetPlayerPosition());

        }

        public override void OnLogicUpdate()
        {
            if (Enemy.TargetDetector.GetDistanceToPlayer() > Constant.Enemy.ChaseRange)
            {
                StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
                return;
            }

            if (Enemy.TargetDetector.GetDistanceToPlayer() < Constant.Enemy.AttackRange)
            {
                StateMachine.ChangeState(typeof(Enemy_Attack));
                return;
            }

            //更新玩家位置
            //todo:做时间间隔
            Enemy.Navigation.SetDestination(Enemy.TargetDetector.GetPlayerPosition());

        }

        public override void OnExit()
        {
            //NOOP
        }


    }
}
