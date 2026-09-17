using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "New InputReader", menuName = "InputReader/New InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> Move = delegate { };
    public event Action<Vector2, bool> Look = delegate { };
    public event Action EnableMouseControlCamera = delegate { };
    public event Action DisableMouseControlCamera = delegate { };
    public event Action<bool> Sprint = delegate { };
    public event Action Roll = delegate { };
    public event Action Interact = delegate { };
    public event Action Attack = delegate { };
    public event Action<bool> AttackHeld = delegate { };
    public event Action Reload = delegate { };


    PlayerInputActions inputActions;
    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

    void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
    }
    /// <summary>
    /// 启用包括UI操作在内的所有Action
    /// </summary>
    public void EnablePlayerActions()
    {
        inputActions.Enable();
        Debug.Log("玩家输入已启用");
    }

    public void EnablePlayerControl()
    {
        inputActions.Player.Enable();
    }
    public void DisablePlayerAction()
    {
        inputActions.Player.Disable();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Debug.Log("玩家输入攻击");
            AttackHeld.Invoke(true);
            Attack.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            AttackHeld.Invoke(false);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Reload.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Interact.Invoke();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Roll.Invoke();
        }
    }

    //持续冲刺
    public void OnSprint(InputAction.CallbackContext context)
    {
        Debug.Log("OnSprint" + context.phase);
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Sprint.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Sprint.Invoke(false);
                break;
        }
    }

}
