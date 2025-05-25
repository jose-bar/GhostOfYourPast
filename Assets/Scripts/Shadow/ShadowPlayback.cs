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

    [Header("Debug")]
    public bool showDebugMessages = true;

    void Start()
    {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

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
        FindFirstPositionInScene();
    }

    void FindFirstPositionInScene()
    {
        for (int i = 0; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == currentScene)
            {
                transform.position = recordingData[i].position;
                currentIndex = i;
                Debug.Log($"🎯 Shadow starting at position {recordingData[i].position} in scene {currentScene}");
                break;
            }
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
            Debug.Log($"🎭 Processing shadow action: {data.actionType} at {data.position} in scene {data.sceneName}");
        }

        switch (data.actionType)
        {
            case ActionType.Movement:
                // Only show movement if in current scene
                if (data.sceneName == currentScene)
                {
                    gameObject.SetActive(true);
                    transform.position = data.position;
                }
                else
                {
                    gameObject.SetActive(false);
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
            Debug.Log($"🎭 Shadow transitioning from {currentScene} to {targetScene}");
        }

        // Update shadow's current scene
        string previousScene = currentScene;
        currentScene = targetScene;

        // Check if shadow should be visible in new scene
        bool hasActionsInNewScene = false;
        for (int i = currentIndex; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == targetScene)
            {
                hasActionsInNewScene = true;
                // Position shadow at first action in new scene
                transform.position = recordingData[i].position;
                break;
            }
        }

        if (hasActionsInNewScene)
        {
            // Only show shadow if player is also in this scene
            if (targetScene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                gameObject.SetActive(true);
                Debug.Log($"🎭 Shadow appeared in {targetScene}");
            }
            else
            {
                gameObject.SetActive(false);
                Debug.Log($"🎭 Shadow in {targetScene} but player not there, hiding shadow");
            }
        }
        else
        {
            gameObject.SetActive(false);
            Debug.Log($"🎭 Shadow has no more actions in {targetScene}, hiding");
        }
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

    public void TransitionToRoom(string roomName, Vector3 position)
    {
        Debug.Log($"🎭 Shadow transitioning to room: {roomName}");

        // Update current scene name
        currentScene = roomName;

        // Check if we have actions in this scene
        bool hasActionsInScene = false;

        for (int i = 0; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == roomName)
            {
                hasActionsInScene = true;
                break;
            }
        }

        // If we have actions in this scene, position shadow at the first one
        if (hasActionsInScene)
        {
            // Find first position in this scene
            for (int i = 0; i < recordingData.Count; i++)
            {
                if (recordingData[i].sceneName == roomName)
                {
                    transform.position = recordingData[i].position;
                    currentIndex = i;
                    gameObject.SetActive(true);
                    Debug.Log($"🎭 Shadow appeared in {roomName} at {transform.position}");
                    break;
                }
            }
        }
        else
        {
            // Hide shadow if no actions in this scene
            gameObject.SetActive(false);
            Debug.Log($"🎭 Shadow has no actions in {roomName}, hiding it");
        }
    }
}