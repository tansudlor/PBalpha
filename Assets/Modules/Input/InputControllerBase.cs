using System;
using Zenject;
using UnityEngine;
using UnityEngine.InputSystem;

namespace com.playbux.input
{
    public abstract class CustomReturnTypeInputControllerBase<TControl> : ICustomReturnTypeInputController<string>, ILateDisposable where TControl : IInputActionCollection2
    {
        public abstract bool IsEnabled { get; }
        public bool IsPressed => isPressed;
        public bool IsHold => isHold;

        public string Value => value;
        public Action<string> OnPressed { get; }
        public Action<string> OnHold { get; }
        public Action<string> OnReleased { get; }
        public Action<string> OnValueChanged { get; }

        internal bool isHold;
        internal bool isPressed;
        internal readonly string value;
        internal readonly TControl controls;

        protected CustomReturnTypeInputControllerBase(TControl controls)
        {
            this.controls = controls;
        }

        public abstract void LateDispose();

        public void Enable() => controls.Enable();

        public void Disable() => controls.Disable();

        public abstract void OnStarted(InputAction.CallbackContext context);

        public abstract void OnPerformed(InputAction.CallbackContext context);

        public abstract void OnCanceled(InputAction.CallbackContext context);
    }

    public abstract class InputControllerBase<TControl> : IInputController, ILateDisposable where TControl : IInputActionCollection2
    {
        public abstract bool IsEnabled { get; }
        public bool IsPressed => isPressed;
        public bool IsHold => isHold;

        public event Action OnPressed;
        public event Action OnHold;
        public event Action OnReleased;

        internal readonly TControl controls;

        private bool isHold;
        private bool isPressed;

        protected InputControllerBase(TControl controls)
        {
            this.controls = controls;
        }

        public abstract void LateDispose();

        public void Enable() => controls.Enable();

        public void Disable() => controls.Disable();

        public void OnStarted(InputAction.CallbackContext context)
        {
            isPressed = true;
            OnPressed?.Invoke();
        }

        public void OnPerformed(InputAction.CallbackContext context)
        {
            isHold = isPressed;
            OnHold?.Invoke();
        }

        public void OnCanceled(InputAction.CallbackContext context)
        {
            isPressed = isHold = false;
            OnReleased?.Invoke();
        }
    }
    public abstract class InputControllerBase<TValue, TControl> : IInputController<TValue>, ITickable, ILateDisposable where TValue : struct, IEquatable<Vector2>  where TControl : IInputActionCollection2
    {
        public abstract bool IsEnabled { get; }
        public bool IsPressed => isPressed;
        public bool IsHold => isHold;

        public TValue Value => value;
        public event Action<TValue> OnPressed;
        public event Action<TValue> OnHold;
        public event Action OnReleased;
        public event Action<TValue> OnValueChanged;

        internal readonly TControl controls;

        private bool isHold;
        private bool isPressed;
        private TValue value;
        private TValue previousValue;

        protected InputControllerBase(TControl controls)
        {
            this.controls = controls;
            previousValue = value = default;
        }

        public abstract void LateDispose();

        public void Enable() => controls.Enable();

        public void Disable() => controls.Disable();

        public void OnStarted(InputAction.CallbackContext context)
        {
            isPressed = true;
            var vector = context.ReadValue<TValue>();
            OnPressed?.Invoke(vector);
        }

        public void OnPerformed(InputAction.CallbackContext context)
        {
            isHold = isPressed;
            var vector = context.ReadValue<TValue>();
            OnHold?.Invoke(vector);
        }

        public void OnCanceled(InputAction.CallbackContext context)
        {
            isPressed = isHold = false;
            OnReleased?.Invoke();
        }

        public void Tick()
        {
            value = ReadValue();
            if (value.Equals(previousValue))
                return;

            previousValue = value;
            OnValueChanged?.Invoke(value);
        }

        protected abstract TValue ReadValue();
    }
}