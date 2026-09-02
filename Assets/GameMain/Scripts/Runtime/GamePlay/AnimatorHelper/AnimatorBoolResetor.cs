using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 动画布尔值重置器。
    /// </summary>
    public class AnimatorBoolResetor : StateMachineBehaviour
    {

        [System.Serializable]
        public struct AnimBoolInfo
        {
            public string animBoolName;
            public bool status;
        }
        [Tooltip("需要重置的动画布尔值信息")]
        [SerializeField]
        private AnimBoolInfo[] animBoolInfos;

        ///OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            for (int i = 0; i < animBoolInfos.Length; i++)
            {
                animator.SetBool(animBoolInfos[i].animBoolName, animBoolInfos[i].status);
            }

        }
    }
}
