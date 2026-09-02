using UnityEditor.EditorTools;
using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 玩家移动脚本，控制玩家的移动和旋转
    /// </summary>
    public class PlayerLocomotion : MonoBehaviour
    {

        [Tooltip("根运动移动倍数")]
        [SerializeField] private float rootMotionMoveMultiplier = 1.5f;
        [Tooltip("后退移动速度")]
        [SerializeField] private float backwardMoveSpeed = 3f; //根据根动画的移动速度算出

        [SerializeField, Min(0f)]
        private float moveInputChangeSpeed = 6f;



        [Tooltip("玩家在地面上的贴地速度")]
        [SerializeField, Min(0.0f)] private float groundStickSpeed = 10.0f;
        [SerializeField] private Transform cameraRoot;

        private Player player;


        private Vector2 moveInput; //移动输入
        private Vector2 targetMoveInput; //目标移动输入，用于平滑速度变化

        private InputReader input => player.Input;

        private void Awake()
        {
            player = Player.GetInstance();

        }

        private void OnEnable()
        {
            input.Move += OnMove;
        }

        private void OnDisable()
        {
            input.Move -= OnMove;
        }

        private void Start()
        {
            input.EnablePlayerActions();
        }

        private void Update()
        {

            moveInput = Vector2.MoveTowards(
        moveInput,
        targetMoveInput,
        moveInputChangeSpeed * Time.deltaTime);

            player.Animator.SetFloat("Velocity", moveInput.sqrMagnitude);
            player.Animator.SetFloat("SpeedX", moveInput.x);
            player.Animator.SetFloat("SpeedZ", moveInput.y);

            Vector3 cameraForward = cameraRoot.forward;
            cameraForward.y = 0.0f;

            if (cameraForward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }

        /// <summary>
        /// 接收移动输入值
        /// </summary>
        /// <param name="value"></param>
        private void OnMove(Vector2 value)
        {
            targetMoveInput = Vector2.ClampMagnitude(value, 1f);
        }

        private void OnAnimatorMove()
        {
            //如果玩家没有移动输入，则不进行根运动

            if (moveInput.sqrMagnitude < 0.001f)
            {
                player.Controller.Move(
                    Vector3.down * groundStickSpeed * Time.deltaTime);
                return;
            }
            //后续在应用roll时，可能需要在roll时也进行根运动，需要额外flag判断
            Vector3 delta = player.Animator.deltaPosition * rootMotionMoveMultiplier;
            delta.y = -groundStickSpeed * Time.deltaTime;

            if (moveInput.y >= 0.0f)
            {
                player.Controller.Move(delta);
            }
            else
            {
                //斜后退时根动画混合不一致，导致卡在原地，所以需要手动计算
                Vector3 deltaBackwardMove = (transform.forward * moveInput.y + transform.right * moveInput.x)
                * backwardMoveSpeed * rootMotionMoveMultiplier * Time.deltaTime;
                deltaBackwardMove.y = -groundStickSpeed * Time.deltaTime;

                player.Controller.Move(deltaBackwardMove);
            }

        }

    }
}
