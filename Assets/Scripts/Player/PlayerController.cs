using UnityEngine;
using System.Collections;
using Yarn.Unity;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Vector2 respawnPosition;
    public float fadeDuration = 1.5f;
    public float deathPauseTime = 1.0f;

    private DialogueRunner dialogueRunner;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ScreenFader screenFader;

    bool isMoving = false;
    bool isPaused = false;
    bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        screenFader = FindObjectOfType<ScreenFader>();
        dialogueRunner = FindObjectOfType<DialogueRunner>();
    }

    void Update()
    {
         if (isPaused || isDead || (dialogueRunner != null && dialogueRunner.IsDialogueRunning))
    {
        movement = Vector2.zero;
        animator.SetBool("isMoving", false);
        return;
    }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        isMoving = movement.magnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            MovementRecorder.Instance?.RecordPosition(transform.position);

            if (movement.x > 0)
                spriteRenderer.flipX = true;
            else if (movement.x < 0)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void KillPlayer()
    {
        if (isDead) return;

        isDead = true;
        movement = Vector2.zero;
        animator.SetBool("isDead", true);
        animator.SetBool("isMoving", false);

        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        if (screenFader != null)
            yield return screenFader.FadeToBlack(fadeDuration);

        yield return new WaitForSeconds(deathPauseTime);

        Respawn(respawnPosition);

        if (screenFader != null)
            yield return screenFader.FadeFromBlack(fadeDuration);
    }


    public void Respawn(Vector2 position)
    {
        isDead = false;
        transform.position = position;
        animator.SetBool("isDead", false);
    }

    public void PausePlayer()
    {
        isPaused = true;
        movement = Vector2.zero;
        animator.SetBool("isMoving", false);
    }

    public void ResumePlayer()
    {
        isPaused = false;
    }
    
    
}
