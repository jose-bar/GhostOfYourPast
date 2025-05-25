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
    private bool initRequiresItem;                 // PATCH #1
    private bool initDoorIsTrigger;          // NEW
    private bool hasRevealedThisDay = false;

    // run-time only
    private bool isUnlocked = false;               // PATCH #1

    void Start()
    {
        //---- we catch both root AND child sprite renderers now ------------
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();  // PATCH #2

        CaptureInitialState();
        DayResetManager.Instance?.Register(this);
    }

    void CaptureInitialState()
    {
        initIsOpen = isOpen;
        initHasBeenUsed = hasBeenUsed;
        initDoorColliderEnabled = doorCollider ? doorCollider.enabled : true;
        initRequiresItem = requiresItem;    // PATCH #1
        initDoorIsTrigger = doorCollider ? doorCollider.isTrigger : false;   // NEW
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
        isUnlocked = false;                       // PATCH #1
        requiresItem = initRequiresItem;            // PATCH #1
        hasRevealedThisDay = false;

        // rewind visuals / physics
        if (doorCollider)
        {
            doorCollider.enabled = initDoorColliderEnabled;
            doorCollider.isTrigger = initDoorIsTrigger;        // NEW
        }

        // sprite swap back
        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }

    bool CanInteract()
    {
        if (isUnlocked) requiresItem = false;      // PATCH #1
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
                // Debug.LogError($"No PlayerCarrySystem available for {displayName}");
                return;
            }

            if (!carrySystem.IsCarrying() ||
                carrySystem.GetCarriedItemName() != requiredItemName)
            {
                // Debug.Log($"Need {requiredItemName} to interact with {displayName}");
                return;
            }

            // Handle item consumption
            if (consumeItemOnUse)
            {
                // Debug.Log($"Consuming {requiredItemName} on {displayName}");
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
                // Debug.Log($"Using {requiredItemName} on {displayName} (not consuming)");
                carrySystem.ForceDropItem();
            }

            isUnlocked = true;                   // PATCH #1
            requiresItem = false;                  // PATCH #1
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
        if (revealItemsOnInteract && objectType != InteractableType.Drawer &&
            itemsToReveal != null)
        {
            foreach (GameObject item in itemsToReveal)
                RevealItem(item);
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

    void RevealItem(GameObject itemRef)
    {
        if (itemRef == null) return;

        GameObject itemInstance;

        // Is the reference already in the scene?
        if (itemRef.scene.IsValid() && itemRef.scene.isLoaded)
        {
            itemInstance = itemRef;
            itemInstance.SetActive(true);                 // un-hide
        }
        else                 // ? this is the ?instantiate prefab? branch
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.35f;
            itemInstance = Instantiate(itemRef, spawnPos, Quaternion.identity);

            // Tag as temporary so it is removed on the next reset
            itemInstance.AddComponent<TemporaryResettable>();    // NEW
                                                                 // (registration handled by component itself)
        }

        // Detach so sprite-sorting of the drawer never hides it again
        itemInstance.transform.SetParent(null);

        // Register with DayResetManager if it has IResettable
        IResettable reset = itemInstance.GetComponent<IResettable>();
        if (reset != null) DayResetManager.Instance?.Register(reset);

        // Debug.Log($"? Revealed item: {itemInstance.name}");
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
        // Debug.Log($"DOOR INTERACTION for {objectId} - player: {isPlayer}");

        // Disable door collider if this is a permanent door opening
        if (doorCollider != null && removeCollisionOnOpen)
        {
            doorCollider.isTrigger = true;
            // Debug.Log($"Door collider disabled: {doorCollider.name}");
        }

        // CRITICAL FIX: Only teleport player if this interaction came from the player
        if (isPlayer && !string.IsNullOrEmpty(targetScene))
        {
            // swap sprite
            if (openedSprite != null && spriteRenderer != null)
                spriteRenderer.sprite = openedSprite;

            // Debug.Log($"Player transitioning to {targetScene} at {spawnPosition}");
            Invoke("TransitionPlayerToRoom", 0.5f);
        }
        else if (!isPlayer && !string.IsNullOrEmpty(targetScene))
        {
            // 0) flip sprite so the door visibly opens
            if (openedSprite != null && spriteRenderer != null)
                spriteRenderer.sprite = openedSprite;

            /* 1) mark door as unlocked so the player can walk through
                  for the rest of the day                                        */
            requiresItem = false;
            isUnlocked = true;

            /* 2)  DO *NOT* call TransitionToRoom here.
                   The very next line in the recording is a SceneTransition and
                   ShadowPlayback will teleport there without any extra help.     */

            // Debug.Log($"Shadow unlocked door '{displayName}' ? waiting for recorded SceneTransition");
        }

    }

    void ToggleDrawer()
    {
        isOpen = !isOpen;

        if (spriteRenderer != null)
            spriteRenderer.sprite = isOpen ? openedSprite : closedSprite;

        // reveal exactly ONCE per day
        if (isOpen && revealItemsOnInteract && !hasRevealedThisDay && itemsToReveal != null)
        {
            hasRevealedThisDay = true;                           // lock
            foreach (GameObject item in itemsToReveal) RevealItem(item);
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

        // Debug.Log($"Shadow interacted with {displayName}");

        /* 1)  run the same interaction logic the player would use */
        Interact(false);

        /* 2)  if this object normally needed a key, mark it as
               unlocked for the REST OF THE DAY so the player can
               walk through without a real key                                 */
        if (requiresItem)
        {
            requiresItem = false;
            isUnlocked = true;
        }
    }


    // Separate method for player teleportation only
    void TransitionPlayerToRoom()
    {
        // Debug.Log($"EXECUTING PLAYER TRANSITION to {targetScene} at {spawnPosition}");

        if (RoomManager.Instance != null)
        {
            // First switch room for camera
            RoomManager.Instance.SwitchToRoom(targetScene);

            // Then teleport player to new position 
            RoomManager.Instance.TeleportPlayer(spawnPosition);

            // Debug.Log($"Player room transition complete!");

            // Record the transition for the player
            MovementRecorder.Instance?.RecordSceneTransition(targetScene);
        }
        else
        {
            // Debug.LogError("CRITICAL: RoomManager.Instance is null! Room transition failed.");
        }
    }

    void OnDestroy() { DayResetManager.Instance?.Unregister(this); }

}