using TMPro;
using System;
using Mirror;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.input;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatInputController : IInitializable, ILateDisposable
    {
        public event Action OnCanceled;
        public event Action OnWokeUp;
        public event Action<string> OnTypeSubmitted;

        private bool isTyping;
        private bool isSubmitPressed;
        private bool isCanceledPressed;
        private bool isNavigationPressed;
        private int historyPointer = -1;
        private int navigationVector = 0;
        private List<string> messageHistory = new List<string>();

        private readonly Button button;
        private readonly PlayerControls controls;
        private readonly TMP_InputField inputField;

        public ChatInputController(Button button, PlayerControls controls, TMP_InputField inputField)
        {
            this.button = button;
            this.controls = controls;
            this.inputField = inputField;
        }

        public void Initialize()
        {
            button.onClick.AddListener(OnSubmitButtonPressed);

            controls.Chat.Cancel.started += OnCancelPressed;
            controls.Chat.Cancel.canceled += OnCancelReleased;

            controls.Chat.Submit.started += OnSubmitButtonPressed;
            controls.Chat.Submit.canceled += OnSubmitButtonReleased;

            controls.Chat.Navigation.started += OnNavigationPressed;
            controls.Chat.Navigation.canceled += OnNavigationReleased;

            inputField.onSelect.AddListener(OnInputFieldSelect);
            inputField.onDeselect.AddListener(OnInputFieldDeselect);
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);

            controls.Chat.Enable();
        }
        public void LateDispose()
        {
            button.onClick.RemoveListener(OnSubmitButtonPressed);

            controls.Chat.Cancel.started -= OnCancelPressed;
            controls.Chat.Cancel.canceled -= OnCancelReleased;

            controls.Chat.Submit.started -= OnSubmitButtonPressed;
            controls.Chat.Submit.canceled -= OnSubmitButtonReleased;

            controls.Chat.Navigation.started -= OnNavigationPressed;
            controls.Chat.Navigation.canceled -= OnNavigationReleased;

            inputField.onSelect.RemoveListener(OnInputFieldSelect);
            inputField.onDeselect.RemoveListener(OnInputFieldDeselect);
            inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);

            controls.Chat.Disable();
        }

        public void Enable()
        {
            controls.Chat.Enable();
            button.interactable = true;
            inputField.interactable = true;
        }

        public void Disable()
        {
            controls.Chat.Disable();
            button.interactable = false;
            inputField.interactable = false;
        }

        private void OnCancelPressed(InputAction.CallbackContext context)
        {
            isCanceledPressed = true;
        }

        private void OnCancelReleased(InputAction.CallbackContext context)
        {
            if (!isCanceledPressed)
                return;

            isTyping = false;
            isCanceledPressed = false;
            EventSystem.current.SetSelectedGameObject(null);
            OnCanceled?.Invoke();
            controls.UI.Enable();
            controls.Movement.Enable();
        }

        private void OnInputFieldSelect(string value)
        {
            if (!isTyping)
                OnWokeUp?.Invoke();

            isTyping = true;
            controls.UI.Disable();
            controls.Movement.Disable();
        }

        private void OnInputFieldDeselect(string value)
        {
            isTyping = false;
            isSubmitPressed = false;
            controls.UI.Enable();
            controls.Movement.Enable();
        }

        private void OnInputFieldValueChanged(string value)
        {

        }

        private void OnNavigationPressed(InputAction.CallbackContext context)
        {
            if (!isTyping)
                return;

            isNavigationPressed = true;

            var input = context.ReadValue<Vector2>();

            int valueY = 0;

            switch (input.y)
            {
                case 0:
                    return;
                case > 0:
                    navigationVector = -1;
                    break;
                case < 0:
                    navigationVector = 1;
                    break;
            }
        }

        private void OnNavigationReleased(InputAction.CallbackContext context)
        {
            if (!isNavigationPressed)
                return;

            isNavigationPressed = false;

            if (!isTyping)
                return;

            if (messageHistory.Count <= 0)
                return;

            if (historyPointer == -1)
                historyPointer = messageHistory.Count;

            historyPointer += navigationVector;

            if (historyPointer < 0)
                historyPointer = messageHistory.Count - 1;

            if (historyPointer >= messageHistory.Count)
                historyPointer = 0;

            inputField.text = messageHistory[historyPointer];
        }

        private void OnSubmitButtonPressed(InputAction.CallbackContext context)
        {
            isSubmitPressed = true;
        }

        private void OnSubmitButtonReleased(InputAction.CallbackContext context)
        {
            if (!isSubmitPressed)
                return;

            isSubmitPressed = false;

            if (!isTyping)
            {
                isTyping = true;
                OnWokeUp?.Invoke();
                inputField.Select();
                controls.UI.Disable();
                controls.Movement.Disable();
                return;
            }

            isTyping = false;
            controls.UI.Enable();
            controls.Movement.Enable();
            EventSystem.current.SetSelectedGameObject(null);

            if (string.IsNullOrEmpty(inputField.text))
                return;

            if (messageHistory.Count + 1 > NetworkClient.snapshotSettings.bufferLimit)
                messageHistory.RemoveAt(0);

            historyPointer = -1;

            string lastMessage = messageHistory.Count > 0 ? messageHistory[^1] : "";

            if (lastMessage != inputField.text)
                messageHistory.Add(inputField.text);

            OnTypeSubmitted?.Invoke(inputField.text);
            inputField.text = "";
            controls.UI.Enable();
            controls.Movement.Enable();
        }

        private void OnSubmitButtonPressed()
        {
            isTyping = false;
            controls.UI.Enable();
            controls.Movement.Enable();
            EventSystem.current.SetSelectedGameObject(null);

            if (string.IsNullOrEmpty(inputField.text))
                return;

            if (messageHistory.Count + 1 > NetworkClient.snapshotSettings.bufferLimit)
                messageHistory.RemoveAt(0);

            historyPointer = -1;

            string lastMessage = messageHistory.Count > 0 ? messageHistory[^1] : "";

            if (lastMessage != inputField.text)
                messageHistory.Add(inputField.text);

            OnTypeSubmitted?.Invoke(inputField.text);
            inputField.text = "";
            controls.UI.Enable();
            controls.Movement.Enable();
        }

        public void Cancel()
        {
            if (!isCanceledPressed)
                return;

            isTyping = false;
            isCanceledPressed = false;
            inputField.DeactivateInputField();
            OnCanceled?.Invoke();
            controls.UI.Enable();
            controls.Movement.Enable();
        }
    }
}