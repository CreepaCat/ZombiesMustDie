using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 攻击
    /// </summary>
    public class Enemy_Attack : EnemyStateBase
    {
        //攻击间隔
        float attackTimeout = 1f;
        float attackTimer;

        public Enemy_Attack(Enemy enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("OnEnter Enemy_Attack");
            Enemy.Navigation.StopMoving();

            attackTimer = 0f;

        }
        public override void OnLogicUpdate()
        {

            if (Enemy.Animation.IsInteracting) return;

            if (CheckChangeState())
            {
                return;
            }

            //todo:平滑转向
            Enemy.transform.rotation = Quaternion.LookRotation(Enemy.GetDirectionToPlayer(), Vector3.up);
            //攻击间隔
            attackTimer += Time.deltaTime;
            if (attackTimer > attackTimeout)
            {
                attackTimer = 0f;
                Enemy.Attack();
            }
        }

        private bool CheckChangeState()
        {
            if (Enemy.TargetDetector.GetDistanceToPlayer() > Constant.Enemy.ChaseRange)
            {
                StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
                return true;
            }
            if (Enemy.TargetDetector.GetDistanceToPlayer() > Constant.Enemy.AttackRange)
            {
                StateMachine.ChangeState(typeof(Enemy_ChasePlayer));
                return true;
            }

            return false;
        }

        public override void OnExit()
        {
            //NOOP
        }


    }
}
