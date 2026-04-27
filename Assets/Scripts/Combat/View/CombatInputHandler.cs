using UnityEngine;
using UnityEngine.InputSystem;

namespace Combat.View
{
    /// <summary>
    /// 战斗输入处理器 - 处理玩家输入并转换为战斗操作
    /// </summary>
    public class CombatInputHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _playerEntityId = "player";
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _attackBaseDamage = 10f;

        private CombatSceneBridge _bridge;
        private Core.CombatEntity _playerEntity;
        private Vector2 _moveInput;
        private float _blockHoldTime;
        private bool _isBlocking;

        // Input System
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _blockAction;
        private InputAction _dodgeAction;

        public void Initialize(CombatSceneBridge bridge, string playerId)
        {
            _bridge = bridge;
            _playerEntityId = playerId;
            _playerEntity = bridge.GetEntity(playerId);
        }

        private void Awake()
        {
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            var inputActions = new InputActionMap("Combat");

            _moveAction = inputActions.AddAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _attackAction = inputActions.AddAction("Attack", InputActionType.Button);
            _attackAction.AddBinding("<Mouse>/leftButton");

            _blockAction = inputActions.AddAction("Block", InputActionType.Button);
            _blockAction.AddBinding("<Mouse>/rightButton");

            _dodgeAction = inputActions.AddAction("Dodge", InputActionType.Button);
            _dodgeAction.AddBinding("<Keyboard>/space");

            inputActions.Enable();
        }

        private void OnEnable()
        {
            EnableInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void EnableInputActions()
        {
            _moveAction?.Enable();
            _attackAction?.Enable();
            _blockAction?.Enable();
            _dodgeAction?.Enable();

            if (_attackAction != null)
                _attackAction.performed += OnAttack;

            if (_blockAction != null)
            {
                _blockAction.performed += OnBlockStart;
                _blockAction.canceled += OnBlockEnd;
            }

            if (_dodgeAction != null)
                _dodgeAction.performed += OnDodge;
        }

        private void DisableInputActions()
        {
            if (_attackAction != null)
                _attackAction.performed -= OnAttack;

            if (_blockAction != null)
            {
                _blockAction.performed -= OnBlockStart;
                _blockAction.canceled -= OnBlockEnd;
            }

            if (_dodgeAction != null)
                _dodgeAction.performed -= OnDodge;

            _moveAction?.Disable();
            _attackAction?.Disable();
            _blockAction?.Disable();
            _dodgeAction?.Disable();
        }

        private void Update()
        {
            if (_playerEntity == null)
            {
                _playerEntity = _bridge?.GetEntity(_playerEntityId);
                if (_playerEntity == null) return;
            }

            HandleMovement();
            HandleBlockHold();
        }

        private void HandleMovement()
        {
            if (_moveAction == null) return;

            _moveInput = _moveAction.ReadValue<Vector2>();

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                var moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
                _playerEntity.PositionX += moveDir.x * _moveSpeed * Time.deltaTime;
                _playerEntity.PositionZ += moveDir.z * _moveSpeed * Time.deltaTime;
            }
        }

        private void HandleBlockHold()
        {
            if (_isBlocking)
            {
                _blockHoldTime += Time.deltaTime;
            }
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            if (_playerEntity == null || _bridge == null) return;

            // 找最近的敌人作为目标
            var enemies = _bridge.CombatManager.GetAllEnemies();
            if (enemies.Count > 0)
            {
                var target = enemies[0];
                _bridge.ExecuteAttack(_playerEntityId, target.EntityId, _attackBaseDamage);
            }
        }

        private void OnBlockStart(InputAction.CallbackContext ctx)
        {
            if (_playerEntity == null || _bridge == null) return;

            _isBlocking = true;
            _blockHoldTime = 0f;
            _bridge.StartDefense(_playerEntityId);
        }

        private void OnBlockEnd(InputAction.CallbackContext ctx)
        {
            if (_playerEntity == null || _bridge == null) return;

            _isBlocking = false;

            // 检查完美格挡
            if (_blockHoldTime <= 0.15f)
            {
                _bridge.TryPerfectBlock(_playerEntityId, _blockHoldTime);
            }

            _bridge.EndDefense(_playerEntityId);
        }

        private void OnDodge(InputAction.CallbackContext ctx)
        {
            if (_playerEntity == null || _bridge == null) return;

            _bridge.TryDodge(_playerEntityId);
        }
    }
}
