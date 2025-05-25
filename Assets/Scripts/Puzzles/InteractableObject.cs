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

    public void TryInteract()
    {
        if (hasBeenUsed && isOneTimeUse) return;

        bool canInteract = true;

        // Check for required item
        if (requiresItem)
        {
            if (playerCarrySystem == null) return;

            if (!playerCarrySystem.IsCarrying() ||
                playerCarrySystem.GetCarriedItemName() != requiredItemName)
            {
                Debug.Log($"Need {requiredItemName} to interact with {displayName}");
                return;
            }

            // Consume the key if it's a door
            if (objectType == InteractableType.Door)
            {
                playerCarrySystem.ForceDropItem();
            }
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
                    item.SetActive(true);

                    // If it's a CarryableItem, make sure it's set up properly
                    CarryableItem carryable = item.GetComponent<CarryableItem>();
                    if (carryable != null)
                    {
                        carryable.OnPlayerLeave(); // Reset visual state
                    }
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
        // Handle door opening animation
        if (hasAnimation && animatedObject != null)
        {
            isOpen = true;
            targetPosition = endPosition;
        }

        // If this is a room transition door
        if (objectType == InteractableType.Door)
        {
            // Record door interaction for shadow playback
            MovementRecorder.Instance?.RecordButtonPress(objectId);

            // Handle required key
            if (requiresItem && playerCarrySystem != null &&
                playerCarrySystem.IsCarrying() &&
                playerCarrySystem.GetCarriedItemName() == requiredItemName)
            {
                // Consume the key - this was missing!
                playerCarrySystem.ForceDropItem();

                // Mark as unlocked if it was a one-time use
                if (isOneTimeUse)
                {
                    hasBeenUsed = true;
                }
            }

            // Find the destination room
            string roomName = gameObject.name; // Default - use door name as room name

            // Or parse from targetScene if you're using that field
            if (!string.IsNullOrEmpty(targetScene))
            {
                roomName = targetScene;
            }

            // Switch room after a short delay for door animation
            Invoke("TransitionToNextRoom", 0.5f);
        }
    }

    void TransitionToNextRoom()
    {
        // Get RoomManager
        RoomManager roomManager = RoomManager.Instance;
        if (roomManager == null)
        {
            Debug.LogError("RoomManager not found! Make sure it exists in scene.");
            return;
        }

        // Switch camera and teleport player
        roomManager.SwitchToRoom(targetScene);
        roomManager.TeleportPlayer(spawnPosition);
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
}
