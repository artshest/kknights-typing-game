using KK.App.Typing;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KK.Infrastructure.Input
{
    public class TextInputHandler : ITypingInput
    {
        public event Action<char> OnTextInput;
        private Keyboard _keyboard;

        public TextInputHandler()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;
            if (Keyboard.current == null)
            {
                Debug.LogWarning("There is no current Keyboard");
                return;
            }

            SubscribeOnKeyboardInput(Keyboard.current);
        }

        private void ValidateInput(char ch)
        {
            if (Char.IsControl(ch))
                return;

            OnTextInput?.Invoke(ch);
        }

        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (device is not Keyboard keyboard)
                return;

            if (change == InputDeviceChange.Removed && keyboard == _keyboard)
            {
                UnsubscribeFromKeyboardInput();

                // Keyboard.current здесь ещё null: новую текущую клавиатуру Input System назначит только после слушателей
                Keyboard remaining = InputSystem.GetDevice<Keyboard>();
                if (remaining != null)
                    SubscribeOnKeyboardInput(remaining);
            }

            if (change == InputDeviceChange.Added && _keyboard == null)
                SubscribeOnKeyboardInput(keyboard);
        }

        private void SubscribeOnKeyboardInput(Keyboard keyboard)
        {
            _keyboard = keyboard;
            _keyboard.onTextInput += ValidateInput;
        }

        private void UnsubscribeFromKeyboardInput()
        {
            if (_keyboard != null)
                _keyboard.onTextInput -= ValidateInput;
            _keyboard = null;
        }

        public void Dispose()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;
            UnsubscribeFromKeyboardInput();
        }
    }
}
