using UnityEngine;

public class ShadowCarrySystem : MonoBehaviour
{
    [Header("Shadow Carry Settings")]
    public Transform shadowCarryPoint;
    public Material shadowItemMaterial;

    [Header("Debug")]
    public bool showDebugMessages = true;

    private GameObject currentShadowItem = null;

    void Start()
    {
        // Create carry point for shadow
        if (shadowCarryPoint == null)
        {
            GameObject carryPointObj = new GameObject("ShadowCarryPoint");
            carryPointObj.transform.SetParent(transform);
            carryPointObj.transform.localPosition = Vector3.up * 1f;
            shadowCarryPoint = carryPointObj.transform;
        }

        if (showDebugMessages)
        {
            Debug.Log($"🎭 ShadowCarrySystem initialized on {gameObject.name}");
        }
    }

    public void OnShadowPickupItem(string itemName, Vector2 originalPosition)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow trying to pick up: '{itemName}' at {originalPosition}");
        }

        // Remove any existing shadow item
        if (currentShadowItem != null)
        {
            Destroy(currentShadowItem);
        }

        // Find the original item in the scene to copy its appearance
        CarryableItem originalItem = FindOriginalItem(itemName);

        if (originalItem != null)
        {
            CreateShadowItem(originalItem);
            if (showDebugMessages)
            {
                Debug.Log($"✅ Shadow successfully created phantom: '{itemName}'");
            }
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.LogWarning($"❌ Could not find original item: '{itemName}' in scene");
            }
        }
    }

    public void OnShadowDropItem(string itemName, Vector2 dropPosition)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow dropping phantom: '{itemName}' at {dropPosition}");
        }

        // Destroy the shadow copy
        if (currentShadowItem != null)
        {
            Destroy(currentShadowItem);
            currentShadowItem = null;
        }
    }

    CarryableItem FindOriginalItem(string itemName)
    {
        // ONLY look for real, *active* items
        foreach (CarryableItem item in FindObjectsOfType<CarryableItem>(true))  // include inactive = true
        {
            if (!item.gameObject.activeInHierarchy) continue;   // ignore hidden / consumed originals
            if (item.itemName == itemName) return item;
        }
        return null;
    }

    void CreateShadowItem(CarryableItem originalItem)
    {
        // Create shadow copy
        currentShadowItem = new GameObject($"👻{originalItem.itemName}");

        // Copy the sprite renderer
        SpriteRenderer originalRenderer = originalItem.GetComponent<SpriteRenderer>();
        if (originalRenderer != null)
        {
            SpriteRenderer shadowRenderer = currentShadowItem.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = originalRenderer.sprite;

            // Make it look ghostly - MORE VISIBLE for debugging
            if (shadowItemMaterial != null)
            {
                shadowRenderer.material = shadowItemMaterial;
            }
            else
            {
                // Make it purple and semi-transparent for visibility
                Color ghostColor = Color.magenta;
                ghostColor.a = 0.7f; // More opaque for debugging
                shadowRenderer.color = ghostColor;
            }

            // Make it slightly larger for visibility
            shadowRenderer.transform.localScale = originalRenderer.transform.localScale * 1.2f;
        }

        // Position it at carry point
        currentShadowItem.transform.position = shadowCarryPoint.position;
        currentShadowItem.transform.SetParent(shadowCarryPoint);

        if (showDebugMessages)
        {
            Debug.Log($"👻 Created phantom item at position: {shadowCarryPoint.position}");
        }
    }

    void Update()
    {
        // Keep shadow item at carry point
        if (currentShadowItem != null && shadowCarryPoint != null)
        {
            currentShadowItem.transform.position = shadowCarryPoint.position;
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (shadowCarryPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(shadowCarryPoint.position, 0.3f);
        }

        if (currentShadowItem != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentShadowItem.transform.position);
        }
    }
}