using System;
using UnityEngine;
using UnityEngine.AI;

namespace ZombiesMustDie
{
    /// <summary>
    /// 敌人AI控制器
    /// </summary>
    public class AiController : MonoBehaviour
    {
        //CONFIG
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 1000f;
        [SerializeField] private float stoppingDistance = 0.5f;

        //当玩家在一定范围内时，将目标转向玩家，若不在则将目标设为基地
        [SerializeField] private float playerDetectDistance = 10f;

        //CACHE
        private NavMeshAgent m_NavMeshAgent;
        private Animator m_Animator;
        private Player m_Player;

        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponentInChildren<Animator>();
            m_Player = Player.GetInstance();
        }

        void Start()
        {
            InitAgent();
        }

        private void InitAgent()
        {
            m_NavMeshAgent.speed = moveSpeed;
            m_NavMeshAgent.angularSpeed = rotationSpeed;
            m_NavMeshAgent.stoppingDistance = stoppingDistance;
        }

        void Update()
        {
            m_Animator.SetFloat("Speed", m_NavMeshAgent.velocity.magnitude);

            //todo:时间间隔检测，减少性能消耗
            if (DistanceToPlayer() < playerDetectDistance
                && DistanceToPlayer() > m_NavMeshAgent.stoppingDistance + 1f) //避免推挤玩家
            {
                m_NavMeshAgent.SetDestination(m_Player.transform.position);
            }
            else
            {
                m_NavMeshAgent.SetDestination(HeadQuarter.GetPosition());
            }
        }

        //todo:PlayerDetector.cs
        public float DistanceToPlayer()
        {
            if (m_Player == null) return float.MaxValue;
            return Vector3.Distance(transform.position, m_Player.transform.position);
        }

        internal void UpdateDestination(Vector3 destination)
        {
            m_NavMeshAgent.SetDestination(destination);
        }
    }
}
