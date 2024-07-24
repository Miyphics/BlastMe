using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : MonoBehaviour
{
    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;
    public delegate void PauseCancelledEvent();
    public event PauseCancelledEvent OnPauseCancelled;

    private PlayerActions inputActions;
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerActions();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        inputActions.Touch.TouchPress.started += ctx => StartTouch(ctx);
        inputActions.Touch.TouchPress.canceled += ctx => EndTouch(ctx);

        inputActions.Touch.Pause.canceled += ctx => PauseCancelled(ctx);
    }

    private void PauseCancelled(InputAction.CallbackContext ctx)
    {
        OnPauseCancelled?.Invoke();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        TouchSimulation.Enable();
        EnhancedTouchSupport.Enable();

        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
    }


    private void OnDisable()
    {
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;

        inputActions.Disable();
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        OnStartTouch?.Invoke(inputActions.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.startTime);
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(inputActions.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.time);
    }

    private void FingerDown(Finger finger)
    {
        OnStartTouch?.Invoke(finger.screenPosition, Time.time);
    }
}
