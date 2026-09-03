using UnityEngine;

namespace ZombiesMustDie
{
    public interface IState
    {
        void OnEnter();
        void OnLogicUpdate();
        void OnExit();
    }
}
