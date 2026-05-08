using Animancer;
using GameAnimation.Core;
using GameAnimation.Data;
using GameAnimation.UnityBridge;
using Tools.Log;
using UnityEngine;

namespace GameAnimation.Samples
{
    /// <summary>
    /// Self-contained animation test setup. Add to a GameObject with a mesh to test animation system.
    /// Creates all required components and profile at runtime.
    /// </summary>
    public class AnimationTestSetup : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material material;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 180f;

        private CharacterAnimationActor animationActor;
        private CharacterAnimationProfile runtimeProfile;
        private float currentSpeed;
        private bool isAttacking;
        private bool isJumping;

        private void Awake()
        {
            SetupComponents();
            SetupProfile();
        }

        private void SetupComponents()
        {
            // Ensure we have required components
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = null; // We use Animancer, no controller needed
            }

            var animancer = GetComponent<AnimancerComponent>();
            if (animancer == null)
                animancer = gameObject.AddComponent<AnimancerComponent>();

            animationActor = GetComponent<CharacterAnimationActor>();
            if (animationActor == null)
                animationActor = gameObject.AddComponent<CharacterAnimationActor>();

            var rootMotionDriver = GetComponent<RootMotionDriver>();
            if (rootMotionDriver == null)
                rootMotionDriver = gameObject.AddComponent<RootMotionDriver>();

            // Create visual child object if not assigned
            if (meshFilter == null)
            {
                var visualChild = transform.Find("Visual");
                if (visualChild == null)
                {
                    var visualGo = new GameObject("Visual");
                    visualGo.transform.SetParent(transform);
                    visualGo.transform.localPosition = new Vector3(0, 0.75f, 0);
                    visualGo.transform.localRotation = Quaternion.identity;
                    visualGo.transform.localScale = Vector3.one;

                    var childFilter = visualGo.AddComponent<MeshFilter>();
                    childFilter.mesh = CreateTestMesh();

                    var childRenderer = visualGo.AddComponent<MeshRenderer>();
                    childRenderer.material = CreateTestMaterial();
                }
            }
        }

        private void SetupProfile()
        {
            runtimeProfile = TestAnimationProfileFactory.CreateTestProfile();

            // Use reflection to set profile since it's a serialized field
            var profileField = typeof(CharacterAnimationActor).GetField("profile",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            profileField?.SetValue(animationActor, runtimeProfile);

            animationActor.AnimationEventTriggered += OnAnimationEvent;

            HLog.Log(this, "Animation test setup complete. Profile created at runtime.");
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

            var moveDir = Vector3.zero;
            if (keyboard.wKey.isPressed) moveDir.z += 1;
            if (keyboard.sKey.isPressed) moveDir.z -= 1;
            if (keyboard.aKey.isPressed) moveDir.x -= 1;
            if (keyboard.dKey.isPressed) moveDir.x += 1;

            currentSpeed = 0f;
            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();
                currentSpeed = keyboard.shiftKey.isPressed ? moveSpeed * 2f : moveSpeed;

                transform.position += moveDir * (currentSpeed * Time.deltaTime);
                var targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }

            if (keyboard.spaceKey.wasPressedThisFrame && !isJumping)
                TryJump();

            if (keyboard.jKey.wasPressedThisFrame && !isAttacking)
                TryAttack();
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

                transform.position += moveDir * (currentSpeed * Time.deltaTime);
                var targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
                TryJump();

            if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
                TryAttack();
#endif
        }

        private void UpdateLocomotion()
        {
            if (isAttacking || isJumping) return;

            var input = AnimationLocomotionInput.FromWorldVelocity(
                transform.forward * currentSpeed,
                transform,
                isGrounded: true,
                isLockedOn: false);

            animationActor.SetLocomotionInput(input);
        }

        private void TryAttack()
        {
            isAttacking = true;
            var result = animationActor.PlayAnimation(TestAnimationProfileFactory.AttackAnimationId);
            if (!result.Accepted)
            {
                HLog.LogW(this, $"Failed to play attack: {result.FailureReason}");
                isAttacking = false;
            }
        }

        private void TryJump()
        {
            isJumping = true;
            var result = animationActor.PlayAnimation(TestAnimationProfileFactory.JumpAnimationId);
            if (!result.Accepted)
            {
                HLog.LogW(this, $"Failed to play jump: {result.FailureReason}");
                isJumping = false;
            }
        }

        private void OnAnimationEvent(AnimationEventContext context)
        {
            HLog.Log(this, $"Animation Event: {context.EventType}, AnimationId: {context.AnimationId}");

            if (context.EventType == AnimationEventType.ActionComplete)
            {
                if (context.AnimationId == TestAnimationProfileFactory.AttackAnimationId)
                {
                    isAttacking = false;
                }
                else if (context.AnimationId == TestAnimationProfileFactory.JumpAnimationId)
                {
                    isJumping = false;
                }
            }
        }

        private static Mesh CreateTestMesh()
        {
            var mesh = new Mesh();
            mesh.name = "TestCharacterMesh";

            // Create a capsule-like mesh (cylinder with rounded ends approximation)
            // Using Unity's built-in capsule mesh would be better, but we create a simple one
            const int segments = 12;
            const int rings = 8;
            const float radius = 0.25f;
            const float height = 1.5f;

            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();

            // Create cylinder body
            for (var ring = 0; ring <= rings; ring++)
            {
                var y = (float)ring / rings * height;
                for (var seg = 0; seg < segments; seg++)
                {
                    var angle = (float)seg / segments * Mathf.PI * 2f;
                    var x = Mathf.Cos(angle) * radius;
                    var z = Mathf.Sin(angle) * radius;
                    vertices.Add(new Vector3(x, y, z));
                }
            }

            // Create triangles for cylinder
            for (var ring = 0; ring < rings; ring++)
            {
                for (var seg = 0; seg < segments; seg++)
                {
                    var current = ring * segments + seg;
                    var next = ring * segments + (seg + 1) % segments;
                    var above = (ring + 1) * segments + seg;
                    var aboveNext = (ring + 1) * segments + (seg + 1) % segments;

                    triangles.Add(current);
                    triangles.Add(above);
                    triangles.Add(next);

                    triangles.Add(next);
                    triangles.Add(above);
                    triangles.Add(aboveNext);
                }
            }

            // Add top cap (hemisphere approximation)
            var topCenter = vertices.Count;
            vertices.Add(new Vector3(0, height + radius * 0.5f, 0));

            for (var seg = 0; seg < segments; seg++)
            {
                var angle = (float)seg / segments * Mathf.PI * 2f;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, height, z));
            }

            for (var seg = 0; seg < segments; seg++)
            {
                triangles.Add(topCenter);
                triangles.Add(topCenter + 1 + seg);
                triangles.Add(topCenter + 1 + (seg + 1) % segments);
            }

            // Add bottom cap
            var bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0, -radius * 0.5f, 0));

            for (var seg = 0; seg < segments; seg++)
            {
                var angle = (float)seg / segments * Mathf.PI * 2f;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, 0, z));
            }

            for (var seg = 0; seg < segments; seg++)
            {
                triangles.Add(bottomCenter);
                triangles.Add(bottomCenter + 1 + (seg + 1) % segments);
                triangles.Add(bottomCenter + 1 + seg);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static Material CreateTestMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                HLog.LogW("AnimationTestSetup", "URP Lit shader not found, using Standard");
                shader = Shader.Find("Standard");
            }

            var mat = new Material(shader);
            mat.color = new Color(0.3f, 0.5f, 0.8f);
            mat.name = "TestCharacterMaterial_Runtime";
            return mat;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 280));
            GUILayout.Label("<b>Animation Test (Runtime Setup)</b>");
            GUILayout.Label("WASD - Move");
            GUILayout.Label("Shift - Run");
            GUILayout.Label("J - Attack");
            GUILayout.Label("Space - Jump");
            GUILayout.Space(10);

            GUILayout.Label($"Current Speed: {currentSpeed:F1}");
            GUILayout.Label($"Is Attacking: {isAttacking}");
            GUILayout.Label($"Is Jumping: {isJumping}");

            if (animationActor != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Profile: {(runtimeProfile != null ? runtimeProfile.name : "None")}");
            }

            GUILayout.EndArea();
        }
    }
}