using System;
using UnityEngine;

namespace ZombiesMustDie
{
    public enum MenuCameraTurnDirection
    {
        Left = -1,
        Right = 1
    }
    /// <summary>
    /// 菜单相机控制器
    /// </summary>
    public class MenuCameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 originalPosition = new Vector3(216.7f, 30f, 212f);
        [SerializeField] private float originalAngle = 90f;
        private Animator m_Animator;
        private Action m_OnComplete;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            transform.localPosition = originalPosition;
            transform.localEulerAngles = new Vector3(0, originalAngle, 0);
        }

        public void Turn(
            MenuCameraTurnDirection direction,
            Action onComplete)
        {
            m_OnComplete = onComplete;

            string stateName = direction == MenuCameraTurnDirection.Left
                ? "TurnLeft"
                : "TurnRight";

            m_Animator.SetTrigger(stateName);
        }

        // 由动画末尾的 Animation Event 调用
        public void OnTurnAnimationComplete()
        {
            Debug.Log("MenuCameraController.OnTurnAnimationComplete");
            Action callback = m_OnComplete;
            m_OnComplete = null;
            callback?.Invoke();
        }
    }
}
