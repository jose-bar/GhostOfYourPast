using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IResettable
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
    public bool consumeItemOnUse = true; // NEW: Whether to consume the item or just drop it

    [Header("Sound")]
    public AudioClip interactionSound;

    [Header("Sprite Toggle (optional)")]                  // NEW
    public Sprite closedSprite;
    public Sprite openedSprite;

    [Header("Day Management")]
    public bool completesDayOnInteract = false;

    [Header("Door Settings")]
    public string targetScene = "";
    public Vector3 spawnPosition = Vector3.zero;
    public Collider2D doorCollider;
    public bool removeCollisionOnOpen = true;
    public bool isDoorPassage = false; // NEW: Mark hallway doors that shouldn't be one-time use

    [Header("Events")]
    public UnityEvent OnInteract;
    public UnityEvent OnPlayerEnterRange;
    public UnityEvent OnPlayerExitRange;
    public UnityEvent OnShadowInteract;

    // State tracking
    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    private SpriteRenderer spriteRenderer;
    private bool isOpen = false;
    private PlayerCarrySystem playerCarrySystem;

    // -------- private snapshot for reset -------------
    private bool initIsOpen;
    private bool initHasBeenUsed;
    private bool initDoorColliderEnabled;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CaptureInitialState();                            // NEW
        DayResetManager.Instance?.Register(this);         // NEW
    }

    void CaptureInitialState()                            // NEW
    {
        initIsOpen = isOpen;
        initHasBeenUsed = hasBeenUsed;
        initDoorColliderEnabled = doorCollider ? doorCollider.enabled : true;
    }

    void Update()
    {
        // Input handling
        if (playerInRange && Input.GetKeyDown(interactKey) && CanInteract())
        {
            TryInteract();
        }
    }

    public void ResetState()                              // NEW
    {
        isOpen = initIsOpen;
        hasBeenUsed = initHasBeenUsed;

        // rewind visuals / physics
        if (doorCollider) doorCollider.enabled = initDoorColliderEnabled;

        // sprite swap back
        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }

    bool CanInteract()
    {
        // Allow interaction if it's not one-time use, or if it hasn't been used yet
        return !hasBeenUsed || !isOneTimeUse;
    }

    public void TryInteract(PlayerCarrySystem externalCarrySystem = null)
    {
        if (!CanInteract()) return;

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

            // Handle item consumption
            if (consumeItemOnUse)
            {
                Debug.Log($"Consuming {requiredItemName} on {displayName}");
                // Destroy the carried item instead of dropping it
                GameObject carriedItem = carrySystem.GetCarriedItem();
                carrySystem.ForceDropItem(); // This sets carried item to null
                if (carriedItem != null)
                {
                    var ci = carriedItem.GetComponent<CarryableItem>();
                    if (ci != null) ci.OnConsumed();
                    else Destroy(carriedItem);
                }
            }
            else
            {
                Debug.Log($"Using {requiredItemName} on {displayName} (not consuming)");
                carrySystem.ForceDropItem();
            }
        }

        if (canInteract)
        {
            Interact(true);
        }
    }

    public void Interact(bool isPlayer = true)
    {
        if (!CanInteract()) return;

        // Mark as used if one-time use
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
                    RevealItem(item);
                }
            }
        }

        // Handle type-specific behavior
        HandleTypeSpecificBehavior(isPlayer);

        // Play sound if available
        if (interactionSound != null)
        {
            AudioSource.PlayClipAtPoint(interactionSound, transform.position);
        }

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

    void RevealItem(GameObject item)
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
            itemCollider.enabled = true;
            itemCollider.isTrigger = true;
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
            renderer.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"Item {item.name} has no SpriteRenderer! Items need sprites.");
        }

        Debug.Log($"Successfully revealed and set up item: {item.name}");
    }

    void HandleTypeSpecificBehavior(bool isPlayer)
    {
        switch (objectType)
        {
            case InteractableType.Door:
                HandleDoorInteraction(isPlayer);
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

    void HandleDoorInteraction(bool isPlayer)
    {
        Debug.Log($"DOOR INTERACTION for {objectId} - player: {isPlayer}");

        // Disable door collider if this is a permanent door opening
        if (doorCollider != null && removeCollisionOnOpen)
        {
            doorCollider.enabled = false;
            Debug.Log($"Door collider disabled: {doorCollider.name}");
        }

        // CRITICAL FIX: Only teleport player if this interaction came from the player
        if (isPlayer && !string.IsNullOrEmpty(targetScene))
        {
            // swap sprite
            if (openedSprite != null && spriteRenderer != null)
                spriteRenderer.sprite = openedSprite;

            Debug.Log($"Player transitioning to {targetScene} at {spawnPosition}");
            Invoke("TransitionPlayerToRoom", 0.5f);
        }
        else if (!isPlayer)
        {
            Debug.Log($"Shadow opened door {objectId} but did not teleport player");
        }

    }

    void ToggleDrawer()
    {
        isOpen = !isOpen;

        // sprite swap
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOpen ? openedSprite : closedSprite;
        }
    }

    void ToggleSwitch()
    {

        isOpen = !isOpen;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCarrySystem = other.GetComponent<PlayerCarrySystem>();

            OnPlayerEnterRange?.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerCarrySystem = null;

            OnPlayerExitRange?.Invoke();
        }
    }

    // Called by ShadowPlayback
    public void ShadowInteract()
    {
        if (!canBeTriggereByShadow) return;

        Debug.Log($"Shadow interacted with {displayName}");
        Interact(false); // Pass false to indicate this is NOT a player interaction
    }

    // Separate method for player teleportation only
    void TransitionPlayerToRoom()
    {
        Debug.Log($"EXECUTING PLAYER TRANSITION to {targetScene} at {spawnPosition}");

        if (RoomManager.Instance != null)
        {
            // First switch room for camera
            RoomManager.Instance.SwitchToRoom(targetScene);

            // Then teleport player to new position 
            RoomManager.Instance.TeleportPlayer(spawnPosition);

            Debug.Log($"Player room transition complete!");

            // Record the transition for the player
            MovementRecorder.Instance?.RecordSceneTransition(targetScene);
        }
        else
        {
            Debug.LogError("CRITICAL: RoomManager.Instance is null! Room transition failed.");
        }
    }

    void OnDestroy() { DayResetManager.Instance?.Unregister(this); }
}