using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 玩家移动脚本，控制玩家的移动和旋转
    /// </summary>
    public class PlayerLocomotion : MonoBehaviour
    {
        private Player player;
        private InputReader input => player.Input;

        private void Awake()
        {
            player = Player.GetInstance();
        }

        private void Start()
        {
            input.EnablePlayerActions();
        }



    }
}
