using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// Player总脚本，管理挂在Player身上的所有脚本
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(CombatController), typeof(PlayerCombat))]
    public class Player : MonoBehaviour
    {
        [SerializeField] InputReader input;
        private static Player m_Instance = null;

        private PlayerAnimation m_Animation;
        private CharacterController m_Controller;
        private Health m_Health;
        public CombatController Combat { get; private set; }
        public PlayerCombat PlayerCombat { get; private set; }


        //Getters
        public InputReader Input => input;

        public PlayerAnimation Animation => m_Animation;

        public CharacterController Controller => m_Controller;

        public bool IsDead => m_Health.IsDead;

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
            m_Animation = GetComponent<PlayerAnimation>();
            m_Controller = GetComponent<CharacterController>();
            m_Health = GetComponent<Health>();
            Combat = GetComponent<CombatController>();
            PlayerCombat = GetComponent<PlayerCombat>();
            if (m_Animation == null)
            {
                Debug.LogError("Player Animator is null");
            }
        }

        void OnEnable()
        {
            m_Health.Died += OnDie;
        }

        void OnDisable()
        {
            m_Health.Died -= OnDie;
        }

        private void OnDie()
        {
            m_Animation.PlayTargetAnimation(PlayerAnimationParamConfig.Clip_Death, true);
        }




    }
}
