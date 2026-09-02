using UnityEngine;
using UnityEngine.AI;

namespace ZombiesMustDie
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AiController))]
    public class Enemy : MonoBehaviour
    {

        private Animator m_Animator;
        private NavMeshAgent m_Agent;
        private AiController m_AiController;

        public Animator Animator => m_Animator;
        public NavMeshAgent Agent => m_Agent;
        public AiController AiController => m_AiController;
        void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_AiController = GetComponent<AiController>();
        }

        public void SetDestination(Vector3 destination)
        {
            m_AiController.UpdateDestination(destination);
        }
    }
}
