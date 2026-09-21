using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 处理动画时间
    /// </summary>
    public class EnemyAnimationEventHandler : MonoBehaviour
    {

        EnemyEntity enemy;
        //敌人出生动画播放完毕后调用

        void Start()
        {
            enemy = GetComponentInParent<EnemyEntity>();
        }
        private void BornOver(AnimationEvent animationEvent)
        {
            //Debug.Log("敌人出生动画播放完毕,animationEvent.animatorClipInfo.weight=" + animationEvent.animatorClipInfo.weight);
            enemy.OnBornOver();
        }

        private void AtkEvent(AnimationEvent animationEvent)
        {
            enemy.OnAtkHit();
        }
    }
}
