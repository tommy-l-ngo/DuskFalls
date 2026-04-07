using UnityEngine;
using UnityEngine.InputSystem;

public class FeyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveDirection;
    private Animator animator;


    [Header("Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool isMoving = false;

    //[Header("BattleControls")]
    private BattleControls controls;

    void Awake()
    {
        controls = new BattleControls();
        controls.Player.QuickAttack.performed += ctx => {
            Debug.Log("Quick Attack fired!");
            animator.SetTrigger("BreakingQuick");
        };
        controls.Player.Combo1.performed += ctx => {
            Debug.Log("Combo fired!");
            animator.SetTrigger("BreakingCombo1");
        };
    }

    void OnEnable() => controls.Enable(); // ADD THIS
    void OnDisable() => controls.Disable(); // ADD THIS


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Set Rigidbody2D constraints
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        // Get WASD input
        moveInput.x = Input.GetAxis("Horizontal"); // A/D (Left/Right)
        moveInput.y = Input.GetAxis("Vertical");   // W/S (Forward/Backward)

        moveDirection = moveInput.normalized;
        isMoving = moveInput.magnitude > 0;

        // Flip sprite based on horizontal movement
        if (moveInput.x > 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = false;


        Debug.Log("IsMoving: " + isMoving + " | Animator: " + animator); // ADD THIS


        // Update animator parameter
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        // Move using direct position change (map vertical input to Z for 3D/top-down)
        Vector3 movement = new Vector3(moveDirection.x, 0f, moveDirection.y);
        transform.position += movement * moveSpeed * Time.deltaTime;
      }
}