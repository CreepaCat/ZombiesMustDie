using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 攻击
    /// </summary>
    public class Enemy_Attack : EnemyStateBase
    {
        public Enemy_Attack(Enemy enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("OnEnter Enemy_Attack");
            Enemy.Navigation.StopMoving();
        }
        public override void OnLogicUpdate()
        {

            if (Enemy.TargetDetector.GetDistanceToPlayer() > Constant.Enemy.ChaseRange)
            {
                StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
                return;
            }
            if (Enemy.TargetDetector.GetDistanceToPlayer() > Constant.Enemy.AttackRange)
            {
                StateMachine.ChangeState(typeof(Enemy_ChasePlayer));
                return;
            }
        }

        public override void OnExit()
        {
            //NOOP
        }


    }
}
