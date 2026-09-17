using UnityEngine;
using UnityEngine.Events;

namespace ZombiesMustDie
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatController), typeof(PlayerAimProvider))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private WeaponFireMode fireMode = WeaponFireMode.Single;
        [SerializeField] private bool allowAttack = true;
        [SerializeField, Min(0f)] private float reloadDuration = 1.5f;
        [SerializeField] private UnityEvent onFired = new UnityEvent();
        [SerializeField] private UnityEvent onReloadStarted = new UnityEvent();
        private CombatController combat;
        private PlayerAimProvider aim;
        private InputReader subscribedInput;
        private bool fireHeld;
        private int lastAttemptFrame = -1;

        public bool AllowAttack { get => allowAttack; set { allowAttack = value; if (!value) fireHeld = false; } }
        public WeaponFireMode FireMode { get => fireMode; set => fireMode = value; }
        public UnityEvent OnFired => onFired;

        private void Awake()
        {
            combat = GetComponent<CombatController>();
            aim = GetComponent<PlayerAimProvider>();
        }

        private void OnEnable()
        {
            Player player = GetComponent<Player>();
            subscribedInput = input != null ? input : player != null ? player.Input : null;
            if (subscribedInput == null) return;
            subscribedInput.Attack += OnFirePressed;
            subscribedInput.AttackHeld += OnFireHeld;
        }

        private void OnDisable()
        {
            if (subscribedInput != null)
            {
                subscribedInput.Attack -= OnFirePressed;
                subscribedInput.AttackHeld -= OnFireHeld;
            }
            subscribedInput = null;
            fireHeld = false;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) fireHeld = false;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || combat.Owner.IsDead || !allowAttack)
            {
                fireHeld = false;
                return;
            }
            if (fireHeld && fireMode == WeaponFireMode.Automatic) TryFire();
        }

        private void OnFirePressed() => TryFire();
        private void OnFireHeld(bool held) => fireHeld = held;

        public bool TryFire()
        {
            if (!isActiveAndEnabled || !allowAttack || Time.timeScale <= 0f || combat.Owner.IsDead ||
                lastAttemptFrame == Time.frameCount || combat.CurrentWeapon == null) return false;
            lastAttemptFrame = Time.frameCount;
            if (!aim.TryGetAttackRequest(out AttackRequest request) || !combat.TryAttack(in request)) return false;
            // Connect animation, recoil, audio and muzzle flash here; failed attempts emit nothing.
            onFired.Invoke();
            return true;
        }

        // Bind to a UI button or a reload input action; no new input binding is imposed.
        public bool TryReload()
        {
            if (!isActiveAndEnabled || !allowAttack || !combat.TryReload(reloadDuration)) return false;
            onReloadStarted.Invoke();
            return true;
        }

        public void Reload() => TryReload();
    }
}
