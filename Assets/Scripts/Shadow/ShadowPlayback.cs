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
        // Only show shadow if it's in the current scene
        if (data.sceneName != currentScene)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);

        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow action: {data.actionType} at {data.position} with data '{data.actionData}'");
        }

        switch (data.actionType)
        {
            case ActionType.Movement:
                transform.position = data.position;
                break;

            case ActionType.SceneTransition:
                gameObject.SetActive(false);
                Debug.Log($"🎭 Shadow transitioned to {data.actionData}");
                break;

            case ActionType.ItemPickup:
                transform.position = data.position;
                HandleShadowItemPickup(data.actionData);
                break;

            case ActionType.ItemDrop:
                transform.position = data.position;
                HandleShadowItemDrop(data.actionData);
                break;

            case ActionType.ButtonPress:
                transform.position = data.position;
                HandleShadowButtonPress(data.actionData);
                Debug.Log($"🎭 Shadow pressed {data.actionData}");
                break;
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
                    Debug.Log($"✅ Called shadow pickup for '{itemName}' at {originalPosition}");
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
                    Debug.Log($"✅ Called shadow drop for '{itemName}' at {dropPosition}");
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
        currentScene = newSceneName;

        // Check if shadow should be visible in this scene
        bool shouldBeVisible = false;
        for (int i = currentIndex; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == newSceneName)
            {
                shouldBeVisible = true;
                break;
            }
        }

        gameObject.SetActive(shouldBeVisible);
    }

    void HandleShadowButtonPress(string objectId)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎭 Shadow trying to interact with: '{objectId}'");
        }

        // Find all interactable objects
        InteractableObject[] interactables = FindObjectsOfType<InteractableObject>();

        foreach (var interactable in interactables)
        {
            if (interactable.objectId == objectId && interactable.canBeTriggereByShadow)
            {
                interactable.ShadowInteract();
                return;
            }
        }
    }
}