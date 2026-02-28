using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallGravityScale = 2.5f;
    [SerializeField] private float normalGravityScale = 1.5f;

    [Header("Ground Detection - Raycast")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private int groundRayCount = 3; 
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
   
    [SerializeField] private InputActionAsset inputActionsAsset;
    private InputAction moveAction;
    private InputAction jumpAction;

    private float moveInput;
    private float currentVelocityX;
    private bool isGrounded;
    private bool jumpPressed;

    // Fall/death tracking
    [Header("Fall / Death")]
    [SerializeField] private float fallDeathDistance = 7f;  // fall distance that causes death on landing
    [SerializeField] private float fallDeathY = -20f;      // world Y threshold that causes immediate death
    private float fallStartY;
    private bool isFalling;

    // New: UI and alive state
    [Header("UI")]
    [SerializeField] private GameObject deathPanel; // assign your deactivated "You Died" panel here
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Find actions inside the provided InputActionAsset (set this in the Inspector).
        if (inputActionsAsset != null)
        {
            // The action names in your .inputactions are "Move" and "Jump" (not "Player/Move")
            moveAction = inputActionsAsset.FindAction("Move");
            jumpAction = inputActionsAsset.FindAction("Jump");
        }

        // Ensure death panel is hidden at start (if assigned)
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // subscribe to actions if found
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
            moveAction.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
            jumpAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
    }

    private void OnDestroy()
    {
        // No generated wrapper to dispose; just clear references.
        moveAction = null;
        jumpAction = null;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Read 1D axis as float
        moveInput = context.ReadValue<float>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = 0f;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Record the jump input; the actual jump will only be performed in FixedUpdate when grounded.
        jumpPressed = true;
    }

    private void FixedUpdate()
    {
        if (isDead) return; // stop processing movement when dead

        // Track previous grounded state to detect transitions
        bool wasGrounded = isGrounded;

        // Check if grounded using raycast
        CheckGroundedWithRaycast();

        // Detect start falling (left ground)
        if (wasGrounded && !isGrounded)
        {
            isFalling = true;
            fallStartY = transform.position.y;
        }

        // Detect landing
        if (!wasGrounded && isGrounded && isFalling)
        {
            float fallDistance = fallStartY - transform.position.y;
            isFalling = false;

            if (fallDistance > fallDeathDistance)
            {
                Die();
                return; // early out - avoid further processing
            }
        }

        // Immediate death if below world threshold
        if (transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        // Handle horizontal movement
        HandleMovement();

        // Handle jumping
        HandleJump();

        // Apply X velocity while preserving Y using Rigidbody2D.velocity
        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

        // Adjust gravity scale based on vertical velocity
        rb.gravityScale = rb.linearVelocity.y < 0 ? fallGravityScale : normalGravityScale;
    }

    private void CheckGroundedWithRaycast()
    {
        isGrounded = false;

        
        Bounds bounds = col.bounds;
        float boundsWidth = bounds.size.x;
        float bottomY = bounds.min.y;
        float leftX = bounds.min.x;

        // Ensure we don't divide by zero if groundRayCount == 1
        int steps = Mathf.Max(1, groundRayCount);
        float denom = (steps > 1) ? (steps - 1) : 1f;

        // Small upward offset so ray origin is slightly inside the collider top edge (avoids starting the ray inside other colliders)
        float skinOffset = 0.02f;

        // Cast multiple rays across the bottom of the collider
        for (int i = 0; i < steps; i++)
        {
            // Calculate position along the bottom of the collider
            float t = (denom == 0f) ? 0f : (i / denom);
            float xPos = leftX + boundsWidth * t;
            Vector2 rayOrigin = new Vector2(xPos, bottomY + skinOffset);

            // Cast ray downward
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance + skinOffset, groundLayer);

            // Debug visualization
            Debug.DrawRay(rayOrigin, Vector2.down * (groundCheckDistance + skinOffset), hit.collider != null ? Color.green : Color.red);

            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void HandleMovement()
    {
        float targetVelocity = moveInput * moveSpeed;
        float accel = moveInput != 0 ? acceleration : deceleration;

        currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocity, accel * Time.fixedDeltaTime);

        // Flip sprite based on movement direction but preserve original scale magnitude
        if (moveInput > 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x); // keep magnitude
            transform.localScale = s;
        }
        else if (moveInput < 0)
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x); // keep magnitude, flip sign
            transform.localScale = s;
        }
    }

    private void HandleJump()
    {
        // Perform jump only if input was pressed and player is grounded
        if (jumpPressed && isGrounded)
        {
            jumpPressed = false;
            // Reset Y velocity via velocity property
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // If jump was pressed but not grounded yet, keep the flag so it can be handled in next FixedUpdate
        // (This provides a basic jump buffer behavior without extra timers.)
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop physics and input so the player stays still and cannot control character
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();

        // Show death UI (assign the panel in the Inspector). Panel should contain your Restart button.
        if (deathPanel != null)
        {
            // Activate the panel (this activates the parent, but child GameObjects might still be individually deactivated)
            deathPanel.SetActive(true);

            // Ensure all children (including ones set inactive) are activated
            foreach (Transform t in deathPanel.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
            }

            // If there's a Button under the panel, attach the RestartGame listener at runtime.
            Button restartButton = deathPanel.GetComponentInChildren<Button>(true);
            if (restartButton != null)
            {
                // Remove previous listeners to avoid duplicates and add our restart handler
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }

            // Pause the game so the player doesn't keep falling / animations running (optional).
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log("Player died from a long fall. (deathPanel not assigned)");
        }
    }

    // New: Restart method for the UI button to call
    public void RestartGame()
    {
        // Restore time scale in case it was paused
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsGrounded => isGrounded;
}
