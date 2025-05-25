using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [Header("Identification")]
    public string objectId = "object_01";
    public string displayName = "Object";

    [Header("Interaction Settings")]
    public float interactionRange = 1f;
    public KeyCode interactKey = KeyCode.E;
    public string interactionPrompt = "Press E to interact";
    public bool isOneTimeUse = false;
    public bool canBeTriggereByShadow = true;
    public enum InteractableType { Generic, Door, Drawer, Item, Puzzle, Switch }
    public InteractableType objectType = InteractableType.Generic;

    [Header("Item Reveal")]
    public GameObject[] itemsToReveal;
    public bool revealItemsOnInteract = false;

    [Header("Item Requirements")]
    public bool requiresItem = false;
    public string requiredItemName = "";

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color disabledColor = Color.gray;
    public bool showPrompt = true;

    [Header("Sound")]
    public AudioClip interactionSound;

    [Header("Animation")]
    public bool hasAnimation = false;
    public Transform animatedObject;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = Vector3.zero;
    public float animationSpeed = 3f;

    [Header("Day Management")]
    public bool completesDayOnInteract = false;

    [Header("Door Settings")]
    public string targetScene = "";
    public Vector3 spawnPosition = Vector3.zero;
    public Collider2D doorCollider;  // Assign the door's collider
    public bool removeCollisionOnOpen = true;


    [Header("Events")]
    public UnityEvent OnInteract;
    public UnityEvent OnPlayerEnterRange;
    public UnityEvent OnPlayerExitRange;
    public UnityEvent OnShadowInteract;

    // State tracking
    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;
    private bool isOpen = false;
    private PlayerCarrySystem playerCarrySystem;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animatedObject != null && hasAnimation)
        {
            animatedObject.localPosition = startPosition;
            targetPosition = startPosition;
        }

        UpdateVisuals();
    }

    void Update()
    {
        // Input handling
        if (playerInRange && Input.GetKeyDown(interactKey) && !hasBeenUsed)
        {
            TryInteract();
        }

        // Handle animation
        if (hasAnimation && animatedObject != null)
        {
            animatedObject.localPosition = Vector3.Lerp(
                animatedObject.localPosition,
                targetPosition,
                Time.deltaTime * animationSpeed
            );
        }
    }

    public void TryInteract(PlayerCarrySystem externalCarrySystem = null)
    {
        if (hasBeenUsed && isOneTimeUse) return;

        // *** FIX: Use provided carry system or the one from trigger ***
        PlayerCarrySystem carrySystem = externalCarrySystem ?? playerCarrySystem;

        bool canInteract = true;

        // Check for required item
        if (requiresItem)
        {
            if (carrySystem == null)
            {
                Debug.LogError($"No PlayerCarrySystem available for {displayName}");
                return;
            }

            if (!carrySystem.IsCarrying() ||
                carrySystem.GetCarriedItemName() != requiredItemName)
            {
                Debug.Log($"Need {requiredItemName} to interact with {displayName}");
                return;
            }

            // Consume the key
            Debug.Log($"Using {requiredItemName} on {displayName}");
            carrySystem.ForceDropItem();
        }

        if (canInteract)
        {
            Interact(true);
        }
    }

    public void Interact(bool isPlayer = true)
    {
        if (hasBeenUsed && isOneTimeUse) return;

        // Mark as used if one-time
        if (isOneTimeUse)
        {
            hasBeenUsed = true;
        }

        // Record for shadow playback if player is interacting
        if (isPlayer)
        {
            MovementRecorder.Instance?.RecordButtonPress(objectId);
        }

        // Reveal items if specified
        if (revealItemsOnInteract && itemsToReveal != null)
        {
            foreach (GameObject item in itemsToReveal)
            {
                if (item != null)
                {
                    // Make sure item is active
                    item.SetActive(true);

                    // Ensure the item has a collider
                    Collider2D itemCollider = item.GetComponent<Collider2D>();
                    if (itemCollider == null)
                    {
                        Debug.LogWarning($"Item {item.name} has no collider! Adding one...");
                        BoxCollider2D newCollider = item.AddComponent<BoxCollider2D>();
                        newCollider.isTrigger = true;
                        newCollider.size = new Vector2(0.5f, 0.5f);
                    }
                    else
                    {
                        // Make sure the collider is enabled
                        itemCollider.enabled = true;
                        itemCollider.isTrigger = true;  // Must be trigger to be picked up
                    }

                    // Make sure item has CarryableItem component
                    CarryableItem carryableItem = item.GetComponent<CarryableItem>();
                    if (carryableItem == null)
                    {
                        Debug.LogWarning($"Item {item.name} has no CarryableItem component! Adding one...");
                        carryableItem = item.AddComponent<CarryableItem>();
                        carryableItem.itemName = item.name;
                    }

                    // Force SpriteRenderer to be visible
                    SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                        renderer.color = Color.white; // Ensure full visibility
                    }
                    else
                    {
                        Debug.LogWarning($"Item {item.name} has no SpriteRenderer! Items need sprites.");
                    }

                    Debug.Log($"Successfully revealed and set up item: {item.name}");
                }
            }
        }

        // Handle type-specific behavior
        HandleTypeSpecificBehavior();

        // Play sound if available
        if (interactionSound != null)
        {
            AudioSource.PlayClipAtPoint(interactionSound, transform.position);
        }

        // Update visuals
        UpdateVisuals();

        // Trigger events
        if (isPlayer)
        {
            OnInteract?.Invoke();

            // Complete the day if specified
            if (completesDayOnInteract)
            {
                GameManager.Instance.CompletePuzzle();
            }
        }
        else
        {
            OnShadowInteract?.Invoke();
        }
    }

    void HandleTypeSpecificBehavior()
    {
        switch (objectType)
        {
            case InteractableType.Door:
                HandleDoorInteraction();
                break;

            case InteractableType.Drawer:
                ToggleDrawer();
                break;

            case InteractableType.Switch:
                ToggleSwitch();
                break;

            case InteractableType.Puzzle:
                // Handle in OnInteract events
                break;
        }
    }

    void HandleDoorInteraction()
    {
        Debug.Log($"DOOR INTERACTION for {objectId} - starting door handling");

        // *** IMMEDIATE FIX: Disable door collider RIGHT AWAY ***
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
            Debug.Log($"SUCCESS: Door collider disabled: {doorCollider.name}");
        }
        else
        {
            Debug.LogWarning($"WARNING: Door collider not assigned for {objectId}! Using fallback...");

            // Fallback: Try to find and disable ANY collider on this object
            Collider2D[] colliders = GetComponents<Collider2D>();
            if (colliders.Length > 0)
            {
                foreach (Collider2D col in colliders)
                {
                    col.enabled = false;
                    Debug.Log($"Disabled collider via fallback: {col.name}");
                }
            }
            else
            {
                Debug.LogError($"CRITICAL ERROR: No colliders found on door {objectId}!");
            }
        }

        // Handle door animation
        if (hasAnimation && animatedObject != null)
        {
            isOpen = true;
            targetPosition = endPosition;
        }

        // If this is a room transition, trigger it
        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"Planning room transition to {targetScene} at {spawnPosition}");
            Invoke("TransitionToRoom", 0.5f);
        }
    }

    void ToggleDrawer()
    {
        if (!hasAnimation || animatedObject == null) return;

        isOpen = !isOpen;
        targetPosition = isOpen ? endPosition : startPosition;
    }

    void ToggleSwitch()
    {
        // Similar to drawer but might have different visuals
        if (!hasAnimation || animatedObject == null) return;

        isOpen = !isOpen;
        targetPosition = isOpen ? endPosition : startPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCarrySystem = other.GetComponent<PlayerCarrySystem>();

            UpdateVisuals();
            OnPlayerEnterRange?.Invoke();

            if (showPrompt)
            {
                Debug.Log(interactionPrompt);
                // Show UI prompt here
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerCarrySystem = null;

            UpdateVisuals();
            OnPlayerExitRange?.Invoke();

            // Hide UI prompt here
        }
    }

    void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        if (hasBeenUsed && isOneTimeUse)
        {
            spriteRenderer.color = disabledColor;
        }
        else if (playerInRange)
        {
            spriteRenderer.color = highlightColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }

    // Called by ShadowPlayback
    public void ShadowInteract()
    {
        if (!canBeTriggereByShadow) return;

        Debug.Log($"Shadow interacted with {displayName}");
        Interact(false);
    }

    void TransitionToRoom()
    {
        Debug.Log($"EXECUTING TRANSITION to {targetScene} at {spawnPosition}");

        if (RoomManager.Instance != null)
        {
            // First switch room for camera
            RoomManager.Instance.SwitchToRoom(targetScene);

            // Then teleport player to new position 
            RoomManager.Instance.TeleportPlayer(spawnPosition);

            Debug.Log($"Room transition complete!");

            // Notify shadow system
            MovementRecorder.Instance?.RecordSceneTransition(targetScene);
        }
        else
        {
            Debug.LogError("CRITICAL: RoomManager.Instance is null! Room transition failed.");
        }
    }

}
