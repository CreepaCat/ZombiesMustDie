
using UnityEngine;

namespace ZombiesMustDie
{

    /// <summary>
    /// 控制 CameraRoot 跟随玩家，并根据观察输入更新水平与俯仰角度。
    /// 实际相机位置、平滑和遮挡由 CinemachineThirdPersonFollow 负责。
    /// </summary>
    public class PlayerCameraRotator : MonoBehaviour
    {
        [Header("相机")]
        [Tooltip("相机follow目标")]
        public GameObject CinemachineCameraTarget;
        public InputReader input;
        [SerializeField] float offsetY = 1.37f;


        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private Vector2 look;
        private bool isDeviceMouse;
        private Player player;

        private void Awake()
        {
            player = Player.GetInstance();

            Vector3 currentRotation = CinemachineCameraTarget.transform.eulerAngles;
            _cinemachineTargetYaw = currentRotation.y;
            _cinemachineTargetPitch = NormalizeAngle(currentRotation.x);
        }

        private void OnEnable()
        {
            input.Look += OnLook;
        }
        private void OnDisable()
        {
            input.Look -= OnLook;
        }

        private void OnLook(Vector2 look, bool isDeviceMouse)
        {
            this.look = look;
            this.isDeviceMouse = isDeviceMouse;

        }

        private void LateUpdate()
        {
            FollowPlayer();
            CameraRotation();
        }

        private void FollowPlayer()
        {
            Vector3 followPosition = GetFollowPos();
            followPosition.y += offsetY;
            transform.position = followPosition;
        }

        private Vector3 GetFollowPos()
        {
            if (player == null)
            {
                player = Player.GetInstance();
            }
            return player.transform.position;
        }


        private void CameraRotation()
        {
            if (look.sqrMagnitude > 0.01f)
            {
                float deltaTimeMultiplier = isDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = NormalizeAngle(_cinemachineTargetYaw);
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch,
                _cinemachineTargetYaw,
                0.0f);
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle + 180.0f, 360.0f) - 180.0f;
        }

    }
}
