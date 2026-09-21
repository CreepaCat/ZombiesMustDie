using System;
using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>选择附近目标，并将输入、提示和高亮绑定到同一个目标。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Player))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float interactionRadius = 3f;
        [SerializeField] private LayerMask interactionMask = ~0;
        private Collider[] colliders = new Collider[16];
        private Player player;
        private Interactable currentTarget;

        public Interactable CurrentTarget => currentTarget;
        public string CurrentPrompt => currentTarget != null ? currentTarget.Prompt : string.Empty;
        public event Action<Interactable> TargetChanged;

        private bool CanSearch => player != null && !player.IsDead && Time.timeScale > 0f &&
            player.Input != null && player.Input.PlayerControlEnabled;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            if (player.Input != null) player.Input.Interact += OnInteract;
        }

        private void OnDisable()
        {
            if (player.Input != null) player.Input.Interact -= OnInteract;
            SetTarget(null);
        }

        private void Update()
        {
            SetTarget(CanSearch ? FindTarget() : null);
        }

        private Interactable FindTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius,
                colliders, interactionMask, QueryTriggerInteraction.Collide);
            // 缓冲区满时扩容重查，避免遗漏真正最近的目标。
            while (count == colliders.Length)
            {
                Array.Resize(ref colliders, colliders.Length * 2);
                count = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius,
                    colliders, interactionMask, QueryTriggerInteraction.Collide);
            }

            Interactable nearest = null;
            float nearestDistance = interactionRadius * interactionRadius;
            for (int i = 0; i < count; i++)
            {
                Interactable candidate = colliders[i].GetComponentInParent<Interactable>();
                colliders[i] = null;
                if (candidate == null || !candidate.isActiveAndEnabled || !candidate.CanInteract) continue;
                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance > nearestDistance) continue;
                if (nearest != null && distance == nearestDistance && candidate != currentTarget) continue;
                nearest = candidate;
                nearestDistance = distance;
            }
            return nearest;
        }

        private void SetTarget(Interactable target)
        {
            // 使用引用比较，使已销毁的目标也能发出清空通知。
            if (ReferenceEquals(currentTarget, target)) return;
            if (currentTarget != null) currentTarget.SetHighlighted(false);
            currentTarget = target;
            if (currentTarget != null) currentTarget.SetHighlighted(true);
            Debug.Log("PlayerInteraction SetTarget " + currentTarget);
            TargetChanged?.Invoke(currentTarget);
        }

        private void OnInteract()
        {
            if (!isActiveAndEnabled) return;
            SetTarget(CanSearch ? FindTarget() : null);
            if (currentTarget == null) return;
            currentTarget.Interact();
            // 界面可能在回调中禁用输入，立即撤销世界交互提示。
            if (!CanSearch || currentTarget == null || !currentTarget.isActiveAndEnabled || !currentTarget.CanInteract)
                SetTarget(null);
        }
    }
}
