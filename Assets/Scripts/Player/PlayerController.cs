using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float fallDeathY = -2f;

    [Header("New Input System Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool isDead;
    private bool isInteracting;
    private Coroutine interactCoroutine;

    // Animator parameter hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    public bool IsDead => isDead;
    public bool IsInteracting => isInteracting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        SetupDefaultInputBindings();
    }

    // Sets up default input bindings if none are assigned in the inspector
    private void SetupDefaultInputBindings()
    {
        if (moveAction == null || moveAction.bindings.Count == 0)
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
        }

        if (jumpAction == null || jumpAction.bindings.Count == 0)
        {
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    private void Update()
    {
        if (isDead) return;

        if (transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        HandleMovementAndJump();
    }

    private void HandleMovementAndJump()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        Vector3 moveDir = Vector3.zero;

        if (!isInteracting)
        {
            Vector2 inputVector = moveAction.ReadValue<Vector2>();
            Vector3 inputDir = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

            if (inputDir.sqrMagnitude >= 0.01f)
            {
                Vector3 camForward = cameraTransform != null
                    ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                    : Vector3.forward;
                Vector3 camRight = cameraTransform != null
                    ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized
                    : Vector3.right;

                moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (jumpAction.WasPressedThisFrame() && isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                TriggerAnimator(JumpHash);
            }
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        Vector3 finalVelocity = moveDir * moveSpeed + verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        UpdateAnimator(moveDir.magnitude, controller.isGrounded);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (hit.collider.GetComponentInParent<HazardSurface>() != null)
        {
            Die();
        }
    }

    public void PlayInteractAnimation()
    {
        if (isDead || isInteracting) return;

        if (interactCoroutine != null)
        {
            StopCoroutine(interactCoroutine);
        }
        interactCoroutine = StartCoroutine(InteractLockRoutine());
    }

    private IEnumerator InteractLockRoutine()
    {
        isInteracting = true;
        UpdateAnimator(0f, controller.isGrounded);
        TriggerAnimator(InteractHash);

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            float enterWait = 0f;
            while (enterWait < 0.35f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Interact"))
            {
                enterWait += Time.deltaTime;
                yield return null;
            }

            while (!isDead && (animator.GetCurrentAnimatorStateInfo(0).IsName("Interact") || animator.IsInTransition(0)))
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        isInteracting = false;
        interactCoroutine = null;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        isInteracting = false;
        if (interactCoroutine != null)
        {
            StopCoroutine(interactCoroutine);
            interactCoroutine = null;
        }
        UpdateAnimator(0f, controller.isGrounded);
        TriggerAnimator(DeathHash);
    }

    private void UpdateAnimator(float speed, bool grounded)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetFloat(SpeedHash, speed);
        animator.SetBool(IsGroundedHash, grounded);
    }

    private void TriggerAnimator(int triggerHash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetTrigger(triggerHash);
    }
}
