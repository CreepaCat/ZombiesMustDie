using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>交互目标。高亮层必须是专用于交互描边的渲染层。</summary>
    [DisallowMultipleComponent]
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private RenderingLayerMask highlightLayer;
        private Renderer[] renderers;
        private readonly int interactableLayer = LayerMask.NameToLayer("Interactable");

        public abstract bool CanInteract { get; }
        public abstract string Prompt { get; }
        public abstract void Interact();

        protected virtual void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            gameObject.layer = interactableLayer;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (renderers == null) return;
            uint mask = highlightLayer;
            foreach (Renderer target in renderers)
            {
                if (target == null || !(target is MeshRenderer || target is SkinnedMeshRenderer)) continue;
                // 只修改专用描边位，保留其他渲染层。
                target.renderingLayerMask = highlighted
                    ? target.renderingLayerMask | mask
                    : target.renderingLayerMask & ~mask;
            }
        }

        protected virtual void OnDisable()
        {
            SetHighlighted(false);
        }
    }
}
