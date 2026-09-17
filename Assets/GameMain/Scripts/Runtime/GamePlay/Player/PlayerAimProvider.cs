using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>挂在玩家根节点，使用玩家正前方作为射击方向。</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAimProvider : MonoBehaviour
    {
        public bool TryGetAttackRequest(out AttackRequest request)
        {
            // Direction only: muzzle position must not introduce convergence toward a camera aim point.
            request = new AttackRequest(Vector3.zero, transform.forward, false);
            return true;
        }
    }
}
