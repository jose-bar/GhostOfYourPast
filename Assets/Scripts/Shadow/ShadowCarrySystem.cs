using UnityEngine;
using System.Collections.Generic;

public class ShadowCarrySystem : MonoBehaviour
{
    [Header("Shadow Carry Settings")]
    public Transform shadowCarryPoint;
    public Material shadowItemMaterial; // Material for shadow copies of items

    private GameObject currentShadowItem = null;
    private Dictionary<string, GameObject> shadowItemPrefabs = new Dictionary<string, GameObject>();

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
    }

    public void OnShadowPickupItem(string itemName, Vector2 originalPosition)
    {
        // Create a shadow copy of the item
        CreateShadowItem(itemName);
        Debug.Log($"Shadow picked up: {itemName}");
    }

    public void OnShadowDropItem(string itemName, Vector2 dropPosition)
    {
        // Destroy the shadow copy
        if (currentShadowItem != null)
        {
            Destroy(currentShadowItem);
            currentShadowItem = null;
        }
        Debug.Log($"Shadow dropped: {itemName}");
    }

    void CreateShadowItem(string itemName)
    {
        // Remove any existing shadow item
        if (currentShadowItem != null)
        {
            Destroy(currentShadowItem);
        }

        // Find the original item to copy its appearance
        CarryableItem[] allItems = FindObjectsOfType<CarryableItem>();
        GameObject originalItem = null;

        foreach (CarryableItem item in allItems)
        {
            if (item.itemName == itemName)
            {
                originalItem = item.gameObject;
                break;
            }
        }

        if (originalItem != null)
        {
            // Create shadow copy
            currentShadowItem = CreateShadowCopy(originalItem);
            currentShadowItem.transform.position = shadowCarryPoint.position;
        }
    }

    GameObject CreateShadowCopy(GameObject original)
    {
        // Create a simplified copy for the shadow
        GameObject shadowCopy = new GameObject($"Shadow_{original.name}");

        // Copy sprite renderer
        SpriteRenderer originalRenderer = original.GetComponent<SpriteRenderer>();
        if (originalRenderer != null)
        {
            SpriteRenderer shadowRenderer = shadowCopy.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = originalRenderer.sprite;

            // Apply shadow material/color
            if (shadowItemMaterial != null)
            {
                shadowRenderer.material = shadowItemMaterial;
            }
            else
            {
                shadowRenderer.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
            }
        }

        // Set as child of shadow carry point
        shadowCopy.transform.SetParent(shadowCarryPoint);

        return shadowCopy;
    }

    void Update()
    {
        // Keep shadow item at carry point
        if (currentShadowItem != null && shadowCarryPoint != null)
        {
            currentShadowItem.transform.position = shadowCarryPoint.position;
        }
    }
}