using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ZombieLand.Materials.Prototype
{
    [CreateAssetMenu(fileName = "Input Reader", menuName = "Input/Input Reader", order = 0)]
    public class InputReader : ScriptableObject, InputControl.IPlayerActions
    {
        InputControl _inputControl;
        public event UnityAction<Vector2> OnMoveEvent;
        public event UnityAction OnJumpEvent;
        public event UnityAction OnJumpCanceledEvent;

        void OnEnable()
        {
            if (_inputControl == null)
            {
                _inputControl = new InputControl();
                _inputControl.Player.SetCallbacks(this);
            }
            
            _inputControl.Player.Enable();
        }

        void OnDisable()
        {
            _inputControl?.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
                OnJumpEvent?.Invoke();
            else if(context.phase == InputActionPhase.Canceled)
                OnJumpCanceledEvent?.Invoke();
        }
    }
}