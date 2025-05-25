using UnityEngine;

public class PlayerCarrySystem : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform carryPoint;
    public float carryDistance = 1f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask carryableLayer = -1;

    [Header("Interaction")]
    public float interactionRange = 1.5f;

    private GameObject carriedItem = null;
    private CarryableItem carriedItemScript = null;
    private CarryableItem nearbyItem = null;

    void Start()
    {
        // Create carry point if it doesn't exist
        if (carryPoint == null)
        {
            GameObject carryPointObj = new GameObject("CarryPoint");
            carryPointObj.transform.SetParent(transform);
            carryPointObj.transform.localPosition = Vector3.up * carryDistance;
            carryPoint = carryPointObj.transform;
        }
    }

    void Update()
    {
        CheckForNearbyItems();
        HandleInteraction();
        UpdateCarriedItemPosition();
    }

    void CheckForNearbyItems()
    {
        if (carriedItem != null) return; // Already carrying something

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, carryableLayer);
        CarryableItem closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D col in nearbyColliders)
        {
            CarryableItem item = col.GetComponent<CarryableItem>();
            if (item != null && item.CanBePickedUp())
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        // Update nearby item
        if (nearbyItem != closestItem)
        {
            if (nearbyItem != null)
                nearbyItem.OnPlayerLeave();

            nearbyItem = closestItem;

            if (nearbyItem != null)
                nearbyItem.OnPlayerApproach();
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            // Find nearby interactables
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);
            InteractableObject closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (Collider2D col in nearbyColliders)
            {
                InteractableObject interactable = col.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Priority 1: Use carried item with interactable that requires an item
            if (carriedItem != null && closestInteractable != null && closestInteractable.requiresItem)
            {
                if (closestInteractable.requiredItemName == carriedItemScript.itemName)
                {
                    Debug.Log($"Using {carriedItemScript.itemName} with {closestInteractable.displayName}");
                    closestInteractable.TryInteract(this);
                    return;
                }
                else
                {
                    Debug.Log($"Wrong item! Need {closestInteractable.requiredItemName}, have {carriedItemScript.itemName}");
                    return;
                }
            }

            // ? priority 2 : PICK UP nearby item (if hands are free) --------
            if (carriedItem == null && nearbyItem != null)
            {
                PickUpItem(nearbyItem);
                return;
            }

            // ? priority 3 : interact with objects that don?t need an item --
            if (closestInteractable != null && !closestInteractable.requiresItem)
            {
                closestInteractable.TryInteract(this);
                return;
            }

            // Priority 4: Drop carried item (only if no interactable nearby)
            if (carriedItem != null && closestInteractable == null)
            {
                DropItem();
                return;
            }
        }
    }

    void PickUpItem(CarryableItem item)
    {
        carriedItem = item.gameObject;
        carriedItemScript = item;

        // Record the pickup
        MovementRecorder.Instance?.RecordItemPickup(item.itemName, item.transform.position);

        // Setup item for carrying
        item.OnPickedUp(transform);

        Debug.Log($"Picked up: {item.itemName}");

        // Play pickup sound
        SimpleAudioManager.Instance?.PlayItemPickup();
    }

    void DropItem()
    {
        if (carriedItem == null) return;

        Vector2 dropPosition = (Vector2)transform.position + GetDropOffset();

        // Record the drop
        MovementRecorder.Instance?.RecordItemDrop(carriedItemScript.itemName, dropPosition);

        // Drop the item
        carriedItemScript.OnDropped(dropPosition);

        Debug.Log($"Dropped: {carriedItemScript.itemName}");

        carriedItem = null;
        carriedItemScript = null;
    }

    Vector2 GetDropOffset()
    {
        Vector2 dropOffset = Vector2.down * 0.5f;
        return dropOffset;
    }

    void UpdateCarriedItemPosition()
    {
        if (carriedItem != null && carryPoint != null)
        {
            carriedItem.transform.position = carryPoint.position;
        }
    }

    // Public methods for other systems
    public bool IsCarrying()
    {
        return carriedItem != null;
    }

    public string GetCarriedItemName()
    {
        return carriedItemScript?.itemName ?? "";
    }

    public GameObject GetCarriedItem()
    {
        return carriedItem;
    }

    public void ForceDropItem()
    {
        if (carriedItem != null)
        {
            Vector2 dropPosition = (Vector2)transform.position + GetDropOffset();
            carriedItemScript.OnDropped(dropPosition);

            // Clear references but don't record this as a normal drop
            carriedItem = null;
            carriedItemScript = null;
        }
    }

    // Method to consume/destroy carried item without dropping
    public void ConsumeCarriedItem()
    {
        if (carriedItem != null)
        {
            Debug.Log($"Consuming carried item: {carriedItemScript.itemName}");
            Destroy(carriedItem);
            carriedItem = null;
            carriedItemScript = null;
        }
    }

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCircle(transform.position, interactionRange);

        if (carryPoint != null)
        {
            Gizmos.color = Color.green;
            //Gizmos.DrawWireSphere(carryPoint.position, 0.2f);
        }
    }
}