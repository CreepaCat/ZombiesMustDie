using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// Player总脚本，管理挂在Player身上的所有脚本
    /// </summary>
    public class Player : MonoBehaviour
    {

        private static Player m_Instance = null;
        void Awake()
        {
            m_Instance = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
