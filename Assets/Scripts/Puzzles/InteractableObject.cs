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

            // IMPORTANT: Consume the key BEFORE the interaction
            // This fixes the key drop issue
            if (objectType == InteractableType.Door)
            {
                // Record key was used
                Debug.Log($"Using {requiredItemName} on {displayName}");
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
                    Debug.Log($"Revealed item: {item.name}");
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
        // Handle door animation
        if (hasAnimation && animatedObject != null)
        {
            isOpen = true;
            targetPosition = endPosition;
        }

        // If door has a collider, disable it when open
        if (doorCollider != null && removeCollisionOnOpen)
        {
            doorCollider.enabled = false;
        }

        // If this is a room transition, trigger it
        if (!string.IsNullOrEmpty(targetScene))
        {
            // Trigger room transition after short animation delay
            Invoke("TransitionToRoom", 0.5f);
        }
    }

    void TransitionToRoom()
    {
        // If we have a room manager, use it
        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.SwitchToRoom(targetScene);
            roomManager.TeleportPlayer(spawnPosition);

            // If the player had a key, make sure it's actually dropped
            if (requiresItem && isOneTimeUse)
            {
                // Key is consumed (handled in TryInteract)
            }

            // Record scene transition for shadow
            MovementRecorder.Instance?.RecordSceneTransition(targetScene);
            return;
        }

        // Fallback to old scene loading if no room manager
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
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
