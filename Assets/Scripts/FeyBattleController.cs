using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FeyBattleController : MonoBehaviour
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
    [SerializeField] private AttackHitBox quickAttackHitBox;
    [SerializeField] private AttackHitBox comboAttackHitBox;
    [SerializeField] private LayerMask enemyLayer;
    private Vector2 facingDirection = Vector2.left;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;
    [SerializeField] private Image[] lifeIcons;
    [SerializeField] private Sprite aliveSprite;
    [SerializeField] private Sprite deadSprite;
    //[Header("BattleControls")]
    private BattleSystem battleSystem; 
    private BattleControls controls;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        battleSystem = GameObject.FindFirstObjectByType<BattleSystem>();

        controls = new BattleControls();
        controls.Player.QuickAttack.performed += ctx => {
            animator.SetTrigger(quickAttackHitBox.animationTrigger);
            quickAttackHitBox.TriggerAttack(transform, facingDirection, enemyLayer, battleSystem);
        };
        controls.Player.Combo1.performed += ctx => {
            animator.SetTrigger(comboAttackHitBox.animationTrigger);
            comboAttackHitBox.TriggerAttack(transform, facingDirection, enemyLayer, battleSystem);
        };

    }
    void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }
    void OnDisable() => controls.Disable();


    void Start()
    {

        Transform healthZone = GameObject.Find("Health Bar Zone").transform;

        lifeIcons = new Image[3];

        for (int i = 0; i < healthZone.childCount; i++)
        {
            Transform slot = healthZone.GetChild(i);
            if (i < 3)
            {
                lifeIcons[i] = slot.GetComponent<Image>();
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }


        currentLives = maxLives;
        UpdateLivesUI();
        Debug.Log("BattleSystem found: " + battleSystem);
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Animator found: " + animator);
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
        {
            spriteRenderer.flipX = true;
            facingDirection = Vector2.right;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = Vector2.left;
        }
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
    public void LoseLife()
    {
        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Debug.Log("FEY SUCCUMBED TO DESPAIR");
        }
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].sprite = i < currentLives ? aliveSprite : deadSprite;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            LoseLife(); // WILL NEED TO CHANGE THIS WHEN ADDIGN ENEMY UI
        }
    }
}