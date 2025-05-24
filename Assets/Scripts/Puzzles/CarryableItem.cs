using UnityEngine;

public class CarryableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public bool canBeCarried = true;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public float bobAmount = 0.1f;
    public float bobSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private bool isBeingCarried = false;
    private bool isHighlighted = false;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isBeingCarried && isHighlighted)
        {
            // Gentle bobbing animation when highlighted
            float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public bool CanBePickedUp()
    {
        return canBeCarried && !isBeingCarried;
    }

    public void OnPlayerApproach()
    {
        isHighlighted = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }

        // Show pickup prompt
        Debug.Log($"Press E to pick up {itemName}");
    }

    public void OnPlayerLeave()
    {
        isHighlighted = false;
        if (spriteRenderer != null && !isBeingCarried)
        {
            spriteRenderer.color = normalColor;
        }
    }

    public void OnPickedUp(Transform carrier)
    {
        isBeingCarried = true;
        isHighlighted = false;

        // Disable physics while being carried
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        // Parent to carrier (optional - you might prefer to just move it manually)
        // transform.SetParent(carrier);
    }

    public void OnDropped(Vector2 dropPosition)
    {
        isBeingCarried = false;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (col != null)
        {
            col.isTrigger = false;
        }

        // Set position
        transform.position = dropPosition;
        originalPosition = dropPosition;

        // Unparent if it was parented
        transform.SetParent(null);
    }
}
