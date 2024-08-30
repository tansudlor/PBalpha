using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace com.playbux.input
{
    public class SkillSlotInputController : CustomReturnTypeInputControllerBase<PlayerControls>
    {
        public override bool IsEnabled => controls.SkillSlot.enabled;

        public SkillSlotInputController(PlayerControls controls) : base(controls)
        {
            Assert.IsNotNull(controls);
            this.controls.SkillSlot.Slot1.started += OnStarted;
            this.controls.SkillSlot.Slot1.performed += OnPerformed;
            this.controls.SkillSlot.Slot1.canceled += OnCanceled;
        }

        public override void LateDispose()
        {
            controls.UI.Inventory.started -= OnStarted;
            controls.UI.Inventory.performed -= OnPerformed;
            controls.UI.Inventory.canceled -= OnCanceled;
        }
        public override void OnStarted(InputAction.CallbackContext context)
        {
            isPressed = true;
            OnPressed?.Invoke(context.action.name);
        }
        public override void OnPerformed(InputAction.CallbackContext context)
        {
            isHold = isPressed;
            OnHold?.Invoke(context.action.name);
        }
        public override void OnCanceled(InputAction.CallbackContext context)
        {
            isPressed = isHold = false;
            OnReleased?.Invoke(context.action.name);
        }
    }

    public class InventoryInputController : InputControllerBase<PlayerControls>
    {
        public override bool IsEnabled => controls.UI.Inventory.enabled;

        public InventoryInputController(PlayerControls controls) : base(controls)
        {
            Assert.IsNotNull(controls);
            this.controls.UI.Inventory.started += OnStarted;
            this.controls.UI.Inventory.performed += OnPerformed;
            this.controls.UI.Inventory.canceled += OnCanceled;
        }

        public override void LateDispose()
        {
            controls.UI.Inventory.started -= OnStarted;
            controls.UI.Inventory.performed -= OnPerformed;
            controls.UI.Inventory.canceled -= OnCanceled;
        }
    }

    public class QuestInputController : InputControllerBase<PlayerControls>
    {
        public override bool IsEnabled => controls.UI.Quest.enabled;

        public QuestInputController(PlayerControls controls) : base(controls)
        {
            Assert.IsNotNull(controls);
            this.controls.UI.Quest.started += OnStarted;
            this.controls.UI.Quest.performed += OnPerformed;
            this.controls.UI.Quest.canceled += OnCanceled;
        }

        public override void LateDispose()
        {
            controls.UI.Quest.started -= OnStarted;
            controls.UI.Quest.performed -= OnPerformed;
            controls.UI.Quest.canceled -= OnCanceled;
        }
    }

    public class PlayerMovementInputController : InputControllerBase<Vector2, PlayerControls>
    {
        public override bool IsEnabled => controls.Movement.Direction.enabled;

        public PlayerMovementInputController(PlayerControls controls) : base(controls)
        {
            Assert.IsNotNull(controls);
            this.controls.Movement.Direction.started += OnStarted;
            this.controls.Movement.Direction.performed += OnPerformed;
            this.controls.Movement.Direction.canceled += OnCanceled;
        }

        public override void LateDispose()
        {
            controls.Movement.Direction.started -= OnStarted;
            controls.Movement.Direction.performed -= OnPerformed;
            controls.Movement.Direction.canceled -= OnCanceled;
            // controls.Dispose();
        }

        protected override Vector2 ReadValue() => controls.Movement.Direction.ReadValue<Vector2>();
    }
}