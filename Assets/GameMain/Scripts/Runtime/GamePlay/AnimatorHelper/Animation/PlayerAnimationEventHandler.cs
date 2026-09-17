using UnityEngine;
namespace ZombiesMustDie
{
    public class PlayerAnimationEventHandler : MonoBehaviour
    {
        Player player;
        void Awake()
        {
            player = GetComponent<Player>();
        }
        private void ShootEvent(AnimationEvent animationEvent)
        {
            Debug.Log("玩家射击动画事件");
        }
    }
}
