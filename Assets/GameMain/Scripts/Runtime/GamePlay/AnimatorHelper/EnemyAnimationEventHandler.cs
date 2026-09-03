using UnityEngine;

namespace ZombiesMustDie
{
    public class EnemyAnimationEventHandler : MonoBehaviour
    {

        Enemy enemy;
        //敌人出生动画播放完毕后调用

        void Awake()
        {
            enemy = GetComponentInParent<Enemy>();
        }
        private void BornOver(AnimationEvent animationEvent)
        {
            Debug.Log("敌人出生动画播放完毕,animationEvent.animatorClipInfo.weight=" + animationEvent.animatorClipInfo.weight);
            enemy.OnBornOver();
        }
    }
}
