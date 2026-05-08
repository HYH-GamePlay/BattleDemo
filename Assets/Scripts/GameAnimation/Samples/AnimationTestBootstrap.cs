using GameAnimation.Core;
using GameAnimation.Data;
using GameAnimation.UnityBridge;
using Tools.Log;
using UnityEngine;

namespace GameAnimation.Samples
{
    public class AnimationTestBootstrap : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private CharacterAnimationActor animationActor;
        [SerializeField] private Transform characterTransform;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 180f;

        [Header("Test Animation IDs")]
        [SerializeField] private int idleAnimationId = 1001;
        [SerializeField] private int walkAnimationId = 1002;
        [SerializeField] private int runAnimationId = 1003;
        [SerializeField] private int attackAnimationId = 2001;
        [SerializeField] private int jumpAnimationId = 2002;

        private float currentSpeed;
        private bool isAttacking;
        private bool isJumping;

        private void Start()
        {
            if (animationActor == null)
            {
                HLog.LogE(this, "CharacterAnimationActor not assigned!");
                return;
            }

            animationActor.AnimationEventTriggered += OnAnimationEvent;

            StartIdle();
        }

        private void Update()
        {
            HandleInput();
            UpdateLocomotion();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            // Movement input
            var moveDir = Vector3.zero;
            if (keyboard.wKey.isPressed) moveDir.z += 1;
            if (keyboard.sKey.isPressed) moveDir.z -= 1;
            if (keyboard.aKey.isPressed) moveDir.x -= 1;
            if (keyboard.dKey.isPressed) moveDir.x += 1;

            // Speed control
            currentSpeed = 0f;
            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();
                currentSpeed = keyboard.shiftKey.isPressed ? moveSpeed * 2f : moveSpeed;

                if (characterTransform != null)
                {
                    characterTransform.position += moveDir * (currentSpeed * Time.deltaTime);
                    var targetRotation = Quaternion.LookRotation(moveDir);
                    characterTransform.rotation = Quaternion.RotateTowards(
                        characterTransform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            // Action inputs
            if (keyboard.spaceKey.wasPressedThisFrame && !isJumping)
            {
                TryJump();
            }

            if (keyboard.jKey.wasPressedThisFrame && !isAttacking)
            {
                TryAttack();
            }
#else
            var moveDir = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDir.z += 1;
            if (Input.GetKey(KeyCode.S)) moveDir.z -= 1;
            if (Input.GetKey(KeyCode.A)) moveDir.x -= 1;
            if (Input.GetKey(KeyCode.D)) moveDir.x += 1;

            currentSpeed = 0f;
            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();
                currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * 2f : moveSpeed;

                if (characterTransform != null)
                {
                    characterTransform.position += moveDir * (currentSpeed * Time.deltaTime);
                    var targetRotation = Quaternion.LookRotation(moveDir);
                    characterTransform.rotation = Quaternion.RotateTowards(
                        characterTransform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                TryJump();
            }

            if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
            {
                TryAttack();
            }
#endif
        }

        private void UpdateLocomotion()
        {
            if (isAttacking || isJumping) return;

            var input = AnimationLocomotionInput.FromWorldVelocity(
                characterTransform != null ? characterTransform.forward * currentSpeed : Vector3.zero,
                characterTransform != null ? characterTransform : transform,
                isGrounded: true,
                isLockedOn: false);

            animationActor.SetLocomotionInput(input);
        }

        private void StartIdle()
        {
            var result = animationActor.PlayAnimation(idleAnimationId);
            if (!result.Accepted)
                HLog.LogW(this, $"Failed to play idle: {result.FailureReason}");
        }

        private void TryAttack()
        {
            isAttacking = true;
            var result = animationActor.PlayAnimation(attackAnimationId);
            if (!result.Accepted)
            {
                HLog.LogW(this, $"Failed to play attack: {result.FailureReason}");
                isAttacking = false;
            }
        }

        private void TryJump()
        {
            isJumping = true;
            var result = animationActor.PlayAnimation(jumpAnimationId);
            if (!result.Accepted)
            {
                HLog.LogW(this, $"Failed to play jump: {result.FailureReason}");
                isJumping = false;
            }
        }

        private void OnAnimationEvent(AnimationEventContext context)
        {
            HLog.Log(this, $"Animation Event: {context.EventType}, AnimationId: {context.AnimationId}, EventId: {context.EventId}");

            if (context.EventType == AnimationEventType.ActionComplete)
            {
                if (context.AnimationId == attackAnimationId)
                {
                    isAttacking = false;
                    StartIdle();
                }
                else if (context.AnimationId == jumpAnimationId)
                {
                    isJumping = false;
                    StartIdle();
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.Label("<b>Animation Test</b>");
            GUILayout.Label("WASD - Move");
            GUILayout.Label("Shift - Run");
            GUILayout.Label("J - Attack");
            GUILayout.Label("Space - Jump");
            GUILayout.Space(10);

            GUILayout.Label($"Current Speed: {currentSpeed:F1}");
            GUILayout.Label($"Is Attacking: {isAttacking}");
            GUILayout.Label($"Is Jumping: {isJumping}");

            if (animationActor != null && animationActor.Profile != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Profile: {animationActor.Profile.name}");
            }

            GUILayout.EndArea();
        }
    }
}