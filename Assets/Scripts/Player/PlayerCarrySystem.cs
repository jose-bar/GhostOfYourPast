using UnityEngine;

public class PlayerCarrySystem : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform carryPoint; // Empty child GameObject positioned in front of player
    public float carryDistance = 1f; // How far in front to carry item
    public KeyCode interactKey = KeyCode.E;
    public LayerMask carryableLayer = -1;

    [Header("Interaction")]
    public float interactionRange = 1.5f;

    private GameObject carriedItem = null;
    private CarryableItem carriedItemScript = null;
    private bool playerInRange = false;
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
            if (carriedItem == null && nearbyItem != null)
            {
                // Pick up item
                PickUpItem(nearbyItem);
            }
            else if (carriedItem != null)
            {
                // Drop item
                DropItem();
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
        // Drop item slightly in front of player based on last movement direction
        Vector2 dropOffset = Vector2.down * 0.5f; // Default drop below player

        // You could enhance this to remember last movement direction
        return dropOffset;
    }

    void UpdateCarriedItemPosition()
    {
        if (carriedItem != null && carryPoint != null)
        {
            carriedItem.transform.position = carryPoint.position;
        }
    }

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

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCircle(transform.position, interactionRange);

        if (carryPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(carryPoint.position, 0.2f);
        }
    }
}
