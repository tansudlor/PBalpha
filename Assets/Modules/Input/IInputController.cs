using System;
using UnityEngine.InputSystem;

namespace com.playbux.input
{
    public interface IInputController
    {
        bool IsEnabled { get; }
        bool IsPressed { get; }
        bool IsHold { get; }
        event Action OnPressed;
        event Action OnHold;
        event Action OnReleased;
        void Enable();
        void Disable();
        void OnStarted(InputAction.CallbackContext context);
        void OnPerformed(InputAction.CallbackContext context);
        void OnCanceled(InputAction.CallbackContext context);
    }

    public interface IInputController<T>
    {
        bool IsEnabled { get; }
        bool IsPressed { get; }
        bool IsHold { get; }
        T Value { get; }
        event Action<T> OnPressed;
        event Action<T> OnHold;
        event Action OnReleased;
        event Action<T> OnValueChanged;
        void Enable();
        void Disable();
        void OnStarted(InputAction.CallbackContext context);
        void OnPerformed(InputAction.CallbackContext context);
        void OnCanceled(InputAction.CallbackContext context);
    }

    public interface ICustomReturnTypeInputController<T>
    {
        bool IsEnabled { get; }
        bool IsPressed { get; }
        bool IsHold { get; }
        T Value { get; }
        Action<T> OnPressed { get; }
        Action<T> OnHold { get; }
        Action<T> OnReleased { get; }
        Action<T> OnValueChanged { get; }
        void Enable();
        void Disable();
        void OnStarted(InputAction.CallbackContext context);
        void OnPerformed(InputAction.CallbackContext context);
        void OnCanceled(InputAction.CallbackContext context);
    }
}