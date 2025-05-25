using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    bool isDeath = false;
    bool isMoving = false;
void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    spriteRenderer = GetComponent<SpriteRenderer>();
}

void Update()
{
    // Get input
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    movement = movement.normalized;

        
    isMoving = movement.magnitude > 0.01f;
    animator.SetBool("isMoving", isMoving);

    if (isMoving)
    {
        MovementRecorder.Instance?.RecordPosition(transform.position);

        // Flip sprite based on direction
        if (movement.x > 0)
            spriteRenderer.flipX = true;
        else if (movement.x < 0)
            spriteRenderer.flipX = false;
    }
}

    void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}