using TMPro;
using UnityEngine;

namespace ZombiesMustDie
{
    public class InteractPomptUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);

        [SerializeField] private string keyHint = "[E]";

        Camera cam;
        Canvas canvas;
        RectTransform canvasRect;
        RectTransform labelRect;
        Interactable lastTarget;
        PlayerInteraction playerInteraction;


        private void Awake()
        {
            cam = Camera.main;
            canvas = label.GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            labelRect = label.GetComponent<RectTransform>();
            playerInteraction = Player.GetInstance().GetComponent<PlayerInteraction>();
            Hide();
        }
        private void OnEnable()
        {
            playerInteraction ??= Player.GetInstance().GetComponent<PlayerInteraction>();
            if (playerInteraction == null)
            {
                Debug.LogWarning("playerInteraction为空");
                return;
            }
            playerInteraction.TargetChanged += OnTargetChanged;
            OnTargetChanged(playerInteraction.CurrentTarget);
        }

        private void OnDisable()
        {
            playerInteraction.TargetChanged -= OnTargetChanged;
        }

        private void LateUpdate()
        {
            //if(target == null) return;
            if (lastTarget == null) return;
            if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);

            //固定显示在屏幕下方，暂不需要跟随玩家位置
            // Vector3 playerPos = playerInteraction.transform.position;
            // playerPos.y = 0;

            // Vector3 worldPos = playerPos + worldOffset;
            // Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            // Camera uiCame = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam;

            // if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCame,
            //         out Vector2 localPos))
            // {
            //     labelRect.anchoredPosition = localPos;
            // }
        }

        private void OnTargetChanged(Interactable target)
        {
            if (target == null) { Hide(); return; }
            if (lastTarget == null || !ReferenceEquals(lastTarget, target))
            {
                Show(target);
            }
        }

        public void Show(Interactable interactable)
        {
            if (interactable == null) return;
            Debug.Log("Show prompt lable");
            label.text = $"{keyHint} {interactable.Prompt}";
            lastTarget = interactable;
            label.gameObject.SetActive(true);
        }

        public void Hide()
        {
            label.gameObject.SetActive(false);
            lastTarget = null;
        }
    }
}
