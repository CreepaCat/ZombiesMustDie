using UnityEngine;
using UnityEngine.AI;

namespace ZombiesMustDie
{
    /// <summary>
    /// 敌人自动寻路脚本
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyNavigation : MonoBehaviour
    {
        [Tooltip("每隔一定时间更新寻路目标位置")]
        [SerializeField]
        private float destinationChangeTimeout = 0.2f;

        //CACHE
        private NavMeshAgent m_NavMeshAgent;
        private Animator m_Animator;
        private Player m_Player;

        private Vector3 _desiredDestination = Vector3.zero;
        private float _destinationChangeTimer;


        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponentInChildren<Animator>();
            m_Player = Player.GetInstance();
        }

        public void InitAgent(float moveSpeed, float rotationSpeed, float stoppingDistance)
        {
            m_NavMeshAgent.speed = moveSpeed;
            m_NavMeshAgent.angularSpeed = rotationSpeed;
            m_NavMeshAgent.stoppingDistance = stoppingDistance;
        }

        void Update()
        {
            m_Animator.SetFloat(EnemyAnimationParamConfig.Param_Speed, m_NavMeshAgent.velocity.magnitude);

            UpdateDestination();
        }

        private void UpdateDestination()
        {
            _destinationChangeTimer += Time.deltaTime;
            if (_destinationChangeTimer > destinationChangeTimeout)
            {
                _destinationChangeTimer = 0f;
                m_NavMeshAgent.SetDestination(_desiredDestination);

            }
        }
        public float DistanceToPlayer()
        {
            if (m_Player == null) return float.MaxValue;
            return Vector3.Distance(transform.position, m_Player.transform.position);
        }

        internal void SetDestination(Vector3 destination)
        {
            _desiredDestination = destination;
        }

        internal void StopMoving()
        {
            m_NavMeshAgent.isStopped = true;
        }

        public void RestoreMoving()
        {
            m_NavMeshAgent.isStopped = false;
        }
    }
}
