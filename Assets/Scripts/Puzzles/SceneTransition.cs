using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string targetSceneName;
    public Vector2 playerSpawnPosition;

    [Header("Door Requirements")]
    public bool requiresItem = false;
    public string requiredItemName = "";

    [Header("Visual Feedback")]
    public GameObject doorSprite;
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    [Header("Debug")]
    public bool showDebugMessages = true;

    private bool playerInRange = false;
    private SpriteRenderer doorRenderer;
    private PlayerCarrySystem playerCarrySystem;

    void Start()
    {
        if (doorSprite != null)
        {
            doorRenderer = doorSprite.GetComponent<SpriteRenderer>();
        }

        ValidateSetup();
        EnsureSpawnManagerExists();
        UpdateDoorVisual();

        if (showDebugMessages)
        {
            Debug.Log($"🚪 Door '{gameObject.name}' initialized. Target: '{targetSceneName}', RequiresItem: {requiresItem}, Item: '{requiredItemName}'");
        }
    }

    void EnsureSpawnManagerExists()
    {
        if (PlayerSpawnManager.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerSpawnManager not found! Creating one...");

            // Create PlayerSpawnManager if it doesn't exist
            GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
            spawnManagerObj.AddComponent<PlayerSpawnManager>();

            Debug.Log("✅ Created PlayerSpawnManager");
        }
        else
        {
            Debug.Log("✅ PlayerSpawnManager found and ready");
        }
    }

    void ValidateSetup()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"SceneTransition '{gameObject.name}' needs a Collider2D component!");
            return;
        }
        if (!col.isTrigger)
        {
            Debug.LogError($"SceneTransition '{gameObject.name}' collider must be set as trigger!");
            return;
        }

        // Check if target scene exists
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == targetSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"❌ Target scene '{targetSceneName}' not found in Build Settings!");
            Debug.LogError("Fix: Go to File > Build Settings and add your scenes!");
        }
        else
        {
            Debug.Log($"✅ Target scene '{targetSceneName}' found in Build Settings");
        }
    }

    void Update()
    {
        UpdateDoorVisual();

        // Fallback: If player presses E while in range (in case direct communication fails)
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (showDebugMessages)
            {
                Debug.Log($"🔑 Fallback E press detected at door '{gameObject.name}'");
            }
            TryTransition(playerCarrySystem); // Use stored reference for fallback
        }
    }

    // NEW: Direct communication method called by PlayerCarrySystem
    public void PlayerTriedInteraction(PlayerCarrySystem carrySystem)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🔑 Player tried interaction with door '{gameObject.name}'");
        }

        // Use the passed carrySystem instead of stored reference
        TryTransition(carrySystem);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugMessages)
        {
            Debug.Log($"Trigger entered by: {other.name}, Tag: {other.tag}");
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCarrySystem = other.GetComponent<PlayerCarrySystem>();

            if (playerCarrySystem == null)
            {
                Debug.LogError("Player does not have PlayerCarrySystem component!");
            }

            ShowPrompt();

            if (showDebugMessages)
            {
                Debug.Log($"🚪 Player entered door trigger for '{gameObject.name}'");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerCarrySystem = null;
            HidePrompt();

            if (showDebugMessages)
            {
                Debug.Log($"🚪 Player left door trigger for '{gameObject.name}'");
            }
        }
    }

    void TryTransition(PlayerCarrySystem carrySystem)
    {
        Debug.Log($"======= DOOR TRANSITION ATTEMPT =======");
        Debug.Log($"🚪 Door: {gameObject.name}");
        Debug.Log($"🎯 Target Scene: '{targetSceneName}'");
        Debug.Log($"🔐 Requires Item: {requiresItem}");
        Debug.Log($"🔑 Required Item: '{requiredItemName}'");
        Debug.Log($"👤 Player In Range: {playerInRange}");

        if (requiresItem)
        {
            if (carrySystem == null)
            {
                Debug.LogError("❌ Passed PlayerCarrySystem is null!");
                return;
            }

            bool isCarrying = carrySystem.IsCarrying();
            string carriedItem = carrySystem.GetCarriedItemName();

            Debug.Log($"👋 Player carrying: {isCarrying}");
            Debug.Log($"📦 Carried item: '{carriedItem}'");
            Debug.Log($"🎯 Required item: '{requiredItemName}'");

            if (!isCarrying)
            {
                Debug.Log($"❌ ACCESS DENIED - Player not carrying anything");
                ShowFailureMessage();
                return;
            }

            if (carriedItem != requiredItemName)
            {
                Debug.Log($"❌ ACCESS DENIED - Wrong item. Need '{requiredItemName}', have '{carriedItem}'");
                ShowFailureMessage();
                return;
            }
        }

        Debug.Log($"✅ ACCESS GRANTED!");

        // Ensure SpawnManager exists before using it
        if (PlayerSpawnManager.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerSpawnManager missing during transition! Creating emergency instance...");
            GameObject emergencySpawnManager = new GameObject("EmergencySpawnManager");
            emergencySpawnManager.AddComponent<PlayerSpawnManager>();
        }

        // Store spawn position for next scene
        if (PlayerSpawnManager.Instance != null)
        {
            PlayerSpawnManager.Instance.SetSpawnData(playerSpawnPosition, targetSceneName);
            Debug.Log($"📍 Set spawn position: {playerSpawnPosition}");
        }
        else
        {
            Debug.LogError("❌ Still no PlayerSpawnManager after emergency creation!");
        }

        // Record scene transition
        MovementRecorder.Instance?.RecordSceneTransition(targetSceneName);

        // Play door sound
        SimpleAudioManager.Instance?.PlayDoorOpen();

        // Load new scene
        try
        {
            Debug.Log($"🌍 LOADING SCENE: '{targetSceneName}'");
            SceneManager.LoadScene(targetSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Failed to load scene '{targetSceneName}': {e.Message}");
        }
    }

    void ShowPrompt()
    {
        string promptText;

        if (requiresItem)
        {
            if (playerCarrySystem != null &&
                playerCarrySystem.IsCarrying() &&
                playerCarrySystem.GetCarriedItemName() == requiredItemName)
            {
                promptText = "Press E to enter";
            }
            else
            {
                promptText = $"Need {requiredItemName} to enter";
            }
        }
        else
        {
            promptText = "Press E to enter";
        }

        Debug.Log($"💬 {promptText}");
    }

    void HidePrompt()
    {
        // Hide UI prompt
    }

    void ShowFailureMessage()
    {
        Debug.Log("🔒 Door is locked!");
    }

    void UpdateDoorVisual()
    {
        if (doorRenderer == null) return;

        if (requiresItem)
        {
            bool canOpen = false;

            if (playerCarrySystem != null &&
                playerCarrySystem.IsCarrying() &&
                playerCarrySystem.GetCarriedItemName() == requiredItemName)
            {
                canOpen = true;
            }

            doorRenderer.sprite = canOpen ? openDoorSprite : closedDoorSprite;
        }
        else
        {
            doorRenderer.sprite = openDoorSprite;
        }
    }

    // Enhanced gizmos
    void OnDrawGizmos()
    {
        // Door area
        Gizmos.color = requiresItem ? Color.red : Color.green;
        //Gizmos.DrawWireCube(transform.position, transform.localScale);

        // Spawn point
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere((Vector3)playerSpawnPosition, 0.5f);

        // Connection line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, (Vector3)playerSpawnPosition);
    }
}