using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// Player总脚本，管理挂在Player身上的所有脚本
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(CombatController), typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerInteraction))]
    public class Player : MonoBehaviour
    {
        [SerializeField] InputReader input;
        private static Player m_Instance = null;
        private Health m_Health;

        //Getters
        public InputReader Input => input;
        public PlayerAnimation Animation { get; private set; }

        public CharacterController Controller { get; private set; }
        public CombatController Combat { get; private set; }
        public PlayerCombat PlayerCombat { get; private set; }
        public PlayerInteraction Interaction { get; private set; }

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
            Animation = GetComponent<PlayerAnimation>();
            Controller = GetComponent<CharacterController>();
            m_Health = GetComponent<Health>();
            Combat = GetComponent<CombatController>();
            PlayerCombat = GetComponent<PlayerCombat>();
            Interaction = GetComponent<PlayerInteraction>();
            if (Animation == null)
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
            Animation.PlayTargetAnimation(PlayerAnimationParamConfig.Clip_Death, true);
            //玩家死亡关卡失败
            GameObject.FindWithTag("LevelManager").GetComponent<LevelController>().FailLevel();
        }




    }
}
