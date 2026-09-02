using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 玩家移动脚本，控制玩家的移动和旋转
    /// </summary>
    public class PlayerLocomotion : MonoBehaviour
    {
        [SerializeField] private float moveSpeedMultiplier = 3.0f;
        [SerializeField, Min(0.0f)] private float groundStickSpeed = 2.0f;
        [SerializeField] private Transform cameraRoot;

        private Player player;
        private Animator animator;
        private CharacterController controller;
        private Vector2 moveInput;

        private InputReader input => player.Input;

        private void Awake()
        {
            player = Player.GetInstance();
            animator = GetComponentInChildren<Animator>();
            controller = GetComponent<CharacterController>();
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

            animator.SetFloat("Velocity", moveInput.sqrMagnitude);
            animator.SetFloat("SpeedX", moveInput.x);
            animator.SetFloat("SpeedZ", moveInput.y);

            Vector3 cameraForward = cameraRoot.forward;
            cameraForward.y = 0.0f;

            if (cameraForward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }

        private void OnMove(Vector2 value)
        {
            moveInput = value;
        }

        private void OnAnimatorMove()
        {
            if (moveInput.sqrMagnitude < 0.001f)
            {
                return;
            }
            Vector3 delta = animator.deltaPosition * moveSpeedMultiplier;
            delta.y = -groundStickSpeed * Time.deltaTime;
            controller.Move(delta);
        }

    }
}
