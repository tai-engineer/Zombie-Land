using UnityEngine;

namespace ZombieLand.Materials.Prototype
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        public Vector2 MoveInput { get; private set; }
        public bool JumpInput { get; private set; }
        void OnEnable()
        {
            _inputReader.OnMoveEvent += (value) => MoveInput = value;
            _inputReader.OnJumpEvent += () => JumpInput = true;
            _inputReader.OnJumpCanceledEvent += () => JumpInput = false;
        }

        void OnDisable()
        {
            _inputReader.OnMoveEvent -= (value) => MoveInput = value;
            _inputReader.OnJumpEvent -= () => JumpInput = true;
            _inputReader.OnJumpCanceledEvent -= () => JumpInput = false;
        }
    }
}