using System;
using System.Xml;
using UnityEngine;

namespace ZombieLand.Materials.Prototype
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerLocomotion : MonoBehaviour
    {
        Rigidbody _rb;
        PlayerInput _input;
        Transform _tr;
        CapsuleCollider _col;

        Vector3 velocity;
        Vector3 _desiredVelocity;
        Vector3 _moveInput;

        [SerializeField, Range(0f, 100f)]
        float _maxSpeed = 20f;

        [SerializeField, Range(0f, 100f)]
        float _maxAcceleration = 20f;

        [SerializeField, Range(0f, 50f)]
        float _jumpHeight = 5f;

        [SerializeField, Range(0f, 100f)]
        float _maxAirAcceleration = 5f;

        [SerializeField, Range(1f, 5f)]
        float _jumpSpeedMultiplier = 1f;

        [SerializeField, Range(1f, 5f)]
        float _fallSpeedMultiplier = 1f;

        bool _onGround;
        
        float _cachedGravity;
        float _baseJumpSpeed;

        int _raycastMask;
        RaycastHit _hitInfo;

        [Header("Collider Settings")]
        [SerializeField, Range(0f, 1f)]
        float _stepHeightRatio = 0.1f;
        [SerializeField]
        float _colliderThickness = 1f;
        [SerializeField]
        float _colliderHeight = 2f;
        [SerializeField]
        Vector3 _colliderOffset = Vector3.zero;

        Vector3 _raycastOrigin;
        Vector3 _raycastDirection;
        float _raycastLength;
        [SerializeField]
        bool _useRaycastDebug = false;

        void Awake()
        {
            _tr = transform;
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInput>();
            _col = GetComponent<CapsuleCollider>();
            _cachedGravity = Physics.gravity.y;
        }

        void Start()
        {
            // Calculate base jump speed needed for the desired height
            _baseJumpSpeed = Mathf.Sqrt(-2f * _cachedGravity * _jumpHeight);
            CalculateRaycastMaskForCollision();
            SetupCollider();
        }

        void CalculateRaycastMaskForCollision()
        {
            int objectLayer = gameObject.layer;
            int allLayersCount = SortingLayer.layers.Length; 
            int layerMask = Physics.AllLayers; // -1
            for (int i = 0; i < allLayersCount; i++)
            {
                if (Physics.GetIgnoreLayerCollision(objectLayer, i))
                {
                    layerMask &= ~(1 << i);
                }
            }
            
            _raycastMask = layerMask;
        }
        void SetupCollider()
        {
            if (_col == null)
            {
                _col = GetComponent<CapsuleCollider>();
            }
            _col.height = _colliderHeight * (1f - _stepHeightRatio);
            _col.radius = _colliderThickness * 0.5f;
            // Adds Y-axis offset bases on the step height ratio
            _col.center = _colliderOffset +
                          new Vector3(0f,
                              _stepHeightRatio * _colliderHeight * 0.5f, 0f);

            RecalibrateRaycast();
        }

        void RecalibrateRaycast()
        {
            _raycastOrigin = _col.bounds.center;
            _raycastDirection = Vector3.down;
            float mainColliderRadius = _colliderHeight * (1f - _stepHeightRatio) * 0.5f;
            float stepHeight = _colliderHeight * _stepHeightRatio;
            _raycastLength = mainColliderRadius + stepHeight;
        }

        void CheckForGround()
        {
            Physics.Raycast(_raycastOrigin,
                _raycastDirection,
                out _hitInfo,
                _raycastLength, _raycastMask,
                QueryTriggerInteraction.Ignore);
            
            _onGround = _hitInfo.collider != null;
        }
        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                SetupCollider();
            }
        }

        void Update()
        {
            _moveInput =
                new Vector3(_input.MoveInput.x, 0f, _input.MoveInput.y);
            _moveInput.Normalize();
            _desiredVelocity = _moveInput * _maxSpeed;
        }

        void LateUpdate()
        {
            if (_useRaycastDebug)
            {
                Debug.DrawRay(_raycastOrigin,
                    _raycastDirection * _raycastLength,
                    Color.red, Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            CheckForGround();
            HandleHorizontalMovement();

            if (_input.JumpInput)
            {
                Jump();
                _onGround = false;
            }

            HandleVerticalMovement();
            _rb.MovePosition(
                transform.position + velocity * Time.fixedDeltaTime);
        }

        void HandleHorizontalMovement()
        {
            var acceleration =
                _onGround ? _maxAcceleration : _maxAirAcceleration;

            float maxSpeedChange = Time.fixedDeltaTime * acceleration;

            velocity.x = Mathf.MoveTowards(velocity.x,
                _desiredVelocity.x,
                maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z,
                _desiredVelocity.z,
                maxSpeedChange);
        }

        #region Air Control

        void Jump()
        {
            if (_onGround)
            {
                velocity.y = _jumpSpeedMultiplier > 1 ? _baseJumpSpeed * _jumpSpeedMultiplier : _baseJumpSpeed;
            }
        }

        void HandleVerticalMovement()
        {
            if (_onGround && velocity.y < Mathf.Epsilon)
            {
                velocity.y = 0;
                return;
            }
            // Apply stronger gravity during the jump to maintain the same height
            float gravityMultiplier = velocity.y > 0
                ? _jumpSpeedMultiplier * _jumpSpeedMultiplier
                : _fallSpeedMultiplier;
            velocity.y += _cachedGravity * gravityMultiplier *
                          Time.fixedDeltaTime;
        }

        #endregion
    }
}