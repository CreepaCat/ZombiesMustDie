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
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //todo:总部基地地点应由level模块提供，而非直接获取
            enemy.SetDestination(HeadQuarter.GetPosition());
            Debug.Log("将目标点设为玩家总部基地");

            //}
        }
    }
}
