using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// Player总脚本，管理挂在Player身上的所有脚本
    /// </summary>
    public class Player : MonoBehaviour
    {
        [SerializeField] InputReader input;
        private static Player m_Instance = null;

        private Animator m_Animator;
        public InputReader Input => input;

        public Animator Animator => m_Animator;


        void Awake()
        {
            m_Instance = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            m_Animator = GetComponentInChildren<Animator>();
            if (m_Animator == null)
            {
                Debug.LogError("Player Animator is null");
            }
        }

        public static Player GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            }
            return m_Instance;
        }
    }
}
