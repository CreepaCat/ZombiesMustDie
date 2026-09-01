using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 玩家移动脚本，控制玩家的移动和旋转
    /// </summary>
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("移动和旋转")]
        [SerializeField] float moveSpeed = 5.0f;
        [SerializeField] float sprintSpeed = 8.335f;
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float rotationSmoothTime = 0.1f;
        [SerializeField] float speedChangeRate = 30f; //加速度
    }
}
