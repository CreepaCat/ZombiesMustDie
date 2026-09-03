using UnityEngine;

namespace ZombiesMustDie
{
    public abstract class EnemyStateBase : IState
    {
        protected Enemy Enemy;
        protected EnemyStateMachine StateMachine;
        public EnemyStateBase(Enemy enemy)
        {
            Enemy = enemy;
            StateMachine = Enemy.StateMachine;
        }
        public abstract void OnEnter();

        public abstract void OnLogicUpdate();

        public abstract void OnExit();


    }
}
