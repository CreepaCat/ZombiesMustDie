using UnityEngine;

namespace ZombiesMustDie
{
    public abstract class EnemyStateBase : IState
    {
        protected EnemyEntity Enemy;
        protected EnemyStateMachine StateMachine;
        public EnemyStateBase(EnemyEntity enemy)
        {
            Enemy = enemy;
            StateMachine = Enemy.StateMachine;
        }
        public abstract void OnEnter();

        public abstract void OnLogicUpdate();

        public abstract void OnExit();


    }
}
