using UnityEngine;
using System.Collections.Generic;

public class ShadowPlayback : MonoBehaviour
{
    private List<PositionData> recordingData;
    private int currentIndex = 0;
    private float playbackStartTime;
    private bool isPlaying = false;
    private string currentScene;
    private ShadowCarrySystem carrySystem;
    private Rigidbody2D rb;                   // NEW

    [Header("Debug")]
    public bool showDebugMessages = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();     // NEW
        if (rb != null) rb.isKinematic = true;
        // Ask RoomManager – falls back to Unity scene if RoomManager not ready
        currentScene = (RoomManager.Instance != null && RoomManager.Instance.GetCurrentRoom() != null)
                       ? RoomManager.Instance.GetCurrentRoom().roomName
                       : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        carrySystem = GetComponent<ShadowCarrySystem>();

        if (carrySystem == null)
        {
            Debug.LogError("❌ Shadow missing ShadowCarrySystem! Adding one...");
            carrySystem = gameObject.AddComponent<ShadowCarrySystem>();
        }
    }

    public void Initialize(List<PositionData> data)
    {
        recordingData = new List<PositionData>(data);
        currentIndex = 0;
        playbackStartTime = Time.time;
        isPlaying = true;

        currentScene = (recordingData.Count > 0)
                   ? recordingData[0].sceneName
                   : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;


        Debug.Log($"🎭 Shadow initialized with {recordingData.Count} actions");

        // Count different action types for debugging
        int movements = 0, pickups = 0, drops = 0, scenes = 0, buttons = 0;
        foreach (var action in recordingData)
        {
            switch (action.actionType)
            {
                case ActionType.Movement: movements++; break;
                case ActionType.ItemPickup: pickups++; break;
                case ActionType.ItemDrop: drops++; break;
                case ActionType.SceneTransition: scenes++; break;
                case ActionType.ButtonPress: buttons++; break;
            }
        }

        Debug.Log($"📊 Actions: {movements} moves, {pickups} pickups, {drops} drops, {scenes} scenes, {buttons} buttons");

        // Start at first position in current scene
        // FindFirstPositionInScene();

        // jump to first line (we no longer scan for ‘currentScene’)
        if (recordingData.Count > 0)
        {
            transform.position = recordingData[0].position;   // <-- PATCH 2
        }
    }

    void Update()
    {
        if (!isPlaying || recordingData == null || recordingData.Count == 0)
            return;

        float currentTime = Time.time - playbackStartTime;

        // Process all actions that should have happened by now
        while (currentIndex < recordingData.Count)
        {
            PositionData currentData = recordingData[currentIndex];

            if (currentTime >= currentData.time)
            {
                ProcessAction(currentData);
                currentIndex++;
            }
            else
            {
                break;
            }
        }

        // Stop when we've played all actions
        if (currentIndex >= recordingData.Count)
        {
            isPlaying = false;
            Debug.Log("🎭 Shadow playback complete");
        }
    }

    void ProcessAction(PositionData data)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow action: {data.actionType} in {data.sceneName}");
        }

        switch (data.actionType)
        {
            case ActionType.Movement:
                if (data.sceneName == currentScene)
                {
                    gameObject.SetActive(true);

                    if (rb != null)
                        rb.MovePosition(data.position);   // physics-friendly move
                    else
                        transform.position = data.position;
                }
                break;

            case ActionType.SceneTransition:
                HandleShadowSceneTransition(data.actionData);
                break;

            case ActionType.ItemPickup:
                if (data.sceneName == currentScene)
                {
                    gameObject.SetActive(true);
                    transform.position = data.position;
                    HandleShadowItemPickup(data.actionData);
                }
                break;

            case ActionType.ItemDrop:
                if (data.sceneName == currentScene)
                {
                    gameObject.SetActive(true);
                    transform.position = data.position;
                    HandleShadowItemDrop(data.actionData);
                }
                break;

            case ActionType.ButtonPress:
                if (data.sceneName == currentScene)
                {
                    gameObject.SetActive(true);
                    transform.position = data.position;
                    HandleShadowButtonPress(data.actionData);
                }
                break;
        }
    }

    void HandleShadowSceneTransition(string targetScene)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow scene transition to: {targetScene}");
        }

        // Update the shadow's scene tracking
        currentScene = targetScene;

        // Find the shadow's next position in the target scene
        for (int i = currentIndex; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == targetScene)
            {
                transform.position = recordingData[i].position;
                Debug.Log($"🎭 Shadow positioned at {transform.position} in {targetScene}");
                break;
            }
        }

        // Shadow should always be visible after a scene transition - it will be hidden by other logic if needed
        gameObject.SetActive(true);
    }

    void HandleShadowItemPickup(string actionData)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🔍 Processing shadow pickup: '{actionData}'");
        }

        // Parse actionData: "itemName|x|y"
        string[] parts = actionData.Split('|');
        if (parts.Length >= 3)
        {
            string itemName = parts[0];
            if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y))
            {
                Vector2 originalPosition = new Vector2(x, y);

                if (carrySystem != null)
                {
                    carrySystem.OnShadowPickupItem(itemName, originalPosition);
                    Debug.Log($"✅ Shadow picked up phantom '{itemName}' at {originalPosition}");
                }
                else
                {
                    Debug.LogError("❌ ShadowCarrySystem is null!");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to parse position from '{actionData}'");
            }
        }
        else
        {
            Debug.LogError($"❌ Invalid actionData format: '{actionData}'. Expected 'itemName|x|y'");
        }
    }

    void HandleShadowItemDrop(string actionData)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🔍 Processing shadow drop: '{actionData}'");
        }

        // Parse actionData: "itemName|x|y"
        string[] parts = actionData.Split('|');
        if (parts.Length >= 3)
        {
            string itemName = parts[0];
            if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y))
            {
                Vector2 dropPosition = new Vector2(x, y);

                if (carrySystem != null)
                {
                    carrySystem.OnShadowDropItem(itemName, dropPosition);
                    Debug.Log($"✅ Shadow dropped phantom '{itemName}' at {dropPosition}");
                }
                else
                {
                    Debug.LogError("❌ ShadowCarrySystem is null!");
                }
            }
        }
    }

    // Call this when the scene changes to update shadow visibility
    public void OnSceneChanged(string newSceneName)
    {
        string oldScene = currentScene;
        currentScene = newSceneName;

        if (showDebugMessages)
        {
            Debug.Log($"🎭 Scene changed from {oldScene} to {newSceneName}");
        }

        // Check if shadow should be visible in this scene
        bool shouldBeVisible = false;
        if (recordingData != null)
        {
            // Look ahead from current index to see if there are actions in this scene
            for (int i = currentIndex; i < recordingData.Count; i++)
            {
                if (recordingData[i].sceneName == newSceneName)
                {
                    shouldBeVisible = true;
                    // Position shadow at first action in this scene
                    transform.position = recordingData[i].position;
                    break;
                }
            }
        }

        gameObject.SetActive(shouldBeVisible);

        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow in {newSceneName}: {(shouldBeVisible ? "VISIBLE" : "HIDDEN")}");
        }
    }

    void HandleShadowButtonPress(string objectId)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow trying to interact with: '{objectId}'");
        }

        // Find the specific interactable object by ID
        InteractableObject[] interactables = FindObjectsOfType<InteractableObject>();

        foreach (var interactable in interactables)
        {
            if (interactable.objectId == objectId && interactable.canBeTriggereByShadow)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"🎭 Shadow found and interacting with: {interactable.displayName}");
                }

                // CRITICAL: Call ShadowInteract to ensure isPlayer = false
                interactable.ShadowInteract();
                return;
            }
        }

        if (showDebugMessages)
        {
            Debug.LogWarning($"🎭 Shadow could not find interactable with ID: '{objectId}'");
        }
    }

    public void TransitionToRoom(string roomName, Vector3 pos)
    {
        /* 1)   advance the playback pointer so that the very next action
         *      we process is the FIRST one that belongs to the new room      */
        for (int i = currentIndex; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == roomName)
            {
                currentIndex = i;          // ← jump cursor
                break;
            }
        }

        /* 2)   keep the internal bookkeeping and visibility logic  */
        HandleShadowSceneTransition(roomName);

        if (currentIndex < recordingData.Count)
        {
            playbackStartTime = Time.time - recordingData[currentIndex].time;
        }

        /* 3)   finally snap to the door’s spawn-point so the shadow is
         *      visually in the right place when the new room appears        */
        transform.position = pos;
    }
}