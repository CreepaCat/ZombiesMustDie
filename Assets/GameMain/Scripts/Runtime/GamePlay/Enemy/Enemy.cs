using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZombiesMustDie
{
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(EnemyNavigation))]
    [RequireComponent(typeof(EnemyTargetDetector))]
    public class Enemy : MonoBehaviour
    {

        //CONFIG
        [Header("自动寻路")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 1000f;
        [SerializeField] private float stoppingDistance = 0.5f;

        [SerializeField] private string currentState;

        //CACHE
        private EnemyAnimation m_Animation;
        private EnemyNavigation m_Navigation;
        private EnemyStateMachine m_StateMachine;
        private EnemyTargetDetector m_TargetDetector;

        //GETTERS
        public EnemyAnimation Animation => m_Animation;
        public EnemyNavigation Navigation => m_Navigation;

        public EnemyStateMachine StateMachine => m_StateMachine;

        public EnemyTargetDetector TargetDetector => m_TargetDetector;
        void Awake()
        {
            m_Animation = GetComponent<EnemyAnimation>();
            m_Navigation = GetComponent<EnemyNavigation>();
            m_TargetDetector = GetComponent<EnemyTargetDetector>();
        }

        void Start()
        {
            //初始化NavAgent设置
            m_Navigation.InitAgent(moveSpeed, rotationSpeed, stoppingDistance);
            InitStateMachine();

        }

        private void InitStateMachine()
        {
            m_StateMachine = new();
            Enemy_Idle idle = new(this);
            Enemy_MoveToFortress moveToFortress = new(this);
            Enemy_ChasePlayer chasePlayer = new(this);
            Enemy_Attack attack = new(this);
            m_StateMachine.AddState(idle);
            m_StateMachine.AddState(moveToFortress);
            m_StateMachine.AddState(chasePlayer);
            m_StateMachine.AddState(attack);

            m_StateMachine.Init(idle.GetType());
        }

        void Update()
        {
            m_StateMachine.LoagicUpdate();
            currentState = m_StateMachine.CurrentState.ToString();

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                m_Animation.PlayTargetAnimation(EnemyAnimationParamConfig.Clip_Death, true);
            }
        }

        public Vector3 GetDirectionToPlayer()
        {
            var dirToPlayer = (TargetDetector.GetPlayerPosition() - transform.position).normalized;
            dirToPlayer.y = 0f;
            return dirToPlayer;
        }

        public void OnBornOver()
        {
            Debug.Log("将目标点设为玩家总部基地");
            StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
        }

        #region 战斗相关

        internal void Attack()
        {
            Animation.PlayTargetAnimation(EnemyAnimationParamConfig.Clip_Attack01, true);
        }

        internal void OnAtkHit()
        {
            //todo:检查伤害区域是否有玩家
            Debug.Log("OnAtkHit");

            GetComponent<CombatTarget>().MeleeAttack();
        }
        #endregion
    }
}
