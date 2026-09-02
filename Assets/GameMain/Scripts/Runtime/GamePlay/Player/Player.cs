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
        private CharacterController m_Controller;


        //Getters
        public InputReader Input => input;

        public Animator Animator => m_Animator;

        public CharacterController Controller => m_Controller;

        public static Player GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            }
            return m_Instance;
        }

        void Awake()
        {
            m_Instance = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            m_Animator = GetComponentInChildren<Animator>();
            m_Controller = GetComponent<CharacterController>();
            if (m_Animator == null)
            {
                Debug.LogError("Player Animator is null");
            }
        }


    }
}
