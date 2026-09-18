using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZombiesMustDie
{
    public class EnemyStateMachine
    {
        private EnemyStateBase _currentState;
        private HashSet<EnemyStateBase> _stateSet;
        private Dictionary<System.Type, EnemyStateBase> _states;

        public EnemyStateBase CurrentState => _currentState;

        public void Init(System.Type startState)
        {
            _states = new();

            foreach (var state in _stateSet)
            {
                _states.Add(state.GetType(), state);
                if (state.GetType() == startState)
                {
                    _currentState = state;
                    _currentState.OnEnter();
                }
            }

        }

        public void AddState(EnemyStateBase stateToAdd)
        {
            _stateSet ??= new();
            _stateSet.Add(stateToAdd);
        }
        public void LoagicUpdate()
        {
            _currentState.OnLogicUpdate();
        }

        public void ChangeState(System.Type stateType)
        {
            if (_states.ContainsKey(stateType))
            {
                _currentState.OnExit();
                _currentState = _states[stateType];
                _currentState.OnEnter();
            }
            else
            {
                Debug.LogError($"EnemyStateMachine中没有{stateType}状态");
            }
        }
    }
}
