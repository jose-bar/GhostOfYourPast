using UnityEngine;

public class CarryableItem : MonoBehaviour, IResettable
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public bool canBeCarried = true;

    [Header("Visual Feedback")]
    public float bobAmount = 0.1f;
    public float bobSpeed = 2f;

    private Vector3 originalPosition;
    private bool isBeingCarried = false;
    private bool isHighlighted = false;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 spawnPosition;
    private bool wasConsumed = false;     // NEW
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;   // remember original spawn
        DayResetManager.Instance?.Register(this);   // register for reset
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

        // Show pickup prompt
        Debug.Log($"Press E to pick up {itemName}");
    }

    public void OnPlayerLeave()
    {
        isHighlighted = false;
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

    public void OnConsumed()                 // called instead of Destroy
    {
        wasConsumed = true;
        gameObject.SetActive(false);         // hide but keep object
    }

    //  IResettable -------------
    public void ResetState()
    {
        wasConsumed = false;
        transform.position = spawnPosition;
        originalPosition = spawnPosition;
        isBeingCarried = false;
        rb.isKinematic = false;
        col.isTrigger = false;
        gameObject.SetActive(true);
    }

    void OnDestroy() { DayResetManager.Instance?.Unregister(this); }
}
