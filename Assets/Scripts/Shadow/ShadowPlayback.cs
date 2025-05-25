using UnityEngine;
using System.Collections.Generic;

public class ShadowPlayback : MonoBehaviour
{
    enum LastProcessed { None, Movement, SceneTransition, Other }

    private List<PositionData> recordingData;
    private int currentIndex = 0;
    private float playbackStartTime;
    private bool isPlaying = false;
    private string currentScene;
    private ShadowCarrySystem carrySystem;
    private Rigidbody2D rb;                   // NEW
    Dictionary<string, InteractableObject> idMap;
    LastProcessed lastProcessed = LastProcessed.None;

    Collider2D myCol;               // cache once
    int skipPhysicsFrames;   // 0 = normal, 1 = ignore self collision

    const float INTERACT_RADIUS = 1.2f;

    [Header("Debug")]
    public bool showDebugMessages = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();     // NEW
        myCol = GetComponent<Collider2D>();          // NEW
        if (rb != null) rb.isKinematic = true;
        // Ask RoomManager – falls back to Unity scene if RoomManager not ready
        currentScene = (RoomManager.Instance != null && RoomManager.Instance.GetCurrentRoom() != null)
                       ? RoomManager.Instance.GetCurrentRoom().roomName
                       : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        carrySystem = GetComponent<ShadowCarrySystem>();

        if (carrySystem == null)
        {
            // Debug.LogError("❌ Shadow missing ShadowCarrySystem! Adding one...");
            carrySystem = gameObject.AddComponent<ShadowCarrySystem>();
        }
    }

    public void Initialize(List<PositionData> data)
    {
        // 0)   keep reference to recording
        recordingData = new List<PositionData>(data);
        currentIndex = 0;
        playbackStartTime = Time.time;
        isPlaying = true;

        // 1)   take the first recorded room as truth
        currentScene = (recordingData.Count > 0)
                       ? recordingData[0].sceneName
                       : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 2)   spawn / show at that spot
        if (recordingData.Count > 0)
            transform.position = recordingData[0].position;

        // 3)   Build fast lookup so we NEVER miss an interactable
        BuildInteractableMap();

        // Debug.Log($"🕯️  Shadow initialised – {recordingData.Count} actions, start in '{currentScene}' @ {transform.position}");
    }

    void BuildInteractableMap()
    {
        idMap = new Dictionary<string, InteractableObject>();

        foreach (var io in FindObjectsOfType<InteractableObject>(true))
        {
            if (!idMap.ContainsKey(io.objectId))
                idMap.Add(io.objectId, io);

            if (!string.IsNullOrEmpty(io.displayName) &&
                !idMap.ContainsKey(io.displayName))
                idMap.Add(io.displayName, io);
        }

        // Debug.Log($"🕯️  Built interactable map with {idMap.Count} entries");
    }

    void Update()
    {
        if (!isPlaying || recordingData == null || recordingData.Count == 0)
            return;

        if (skipPhysicsFrames > 0)
        {
            skipPhysicsFrames--;
            if (skipPhysicsFrames == 0 && myCol != null)
                myCol.enabled = true;
        }

        float currentTime = Time.time - playbackStartTime;
        int processedThisFrame = 0;
        const int MAX_ACTIONS_PER_FRAME = 120;   // safety cap (~2 seconds worth)

        while (currentIndex < recordingData.Count)
        {
            PositionData currentData = recordingData[currentIndex];

            if (currentTime >= currentData.time)
            {
                ProcessAction(currentData);
                currentIndex++;
                processedThisFrame++;

                /*  ❱  Immediately stop for this frame after a scene
                    transition so currentTime will be recomputed
                    on the next Update() call.                              */
                if (lastProcessed == LastProcessed.SceneTransition)
                    break;

                if (processedThisFrame >= MAX_ACTIONS_PER_FRAME)
                    break;                              // emergency throttle
            }
            else
            {
                break;
            }
        }

        if (currentIndex >= recordingData.Count)
            isPlaying = false;
    }


    void ProcessAction(PositionData data)
    {
        switch (data.actionType)
        {
            case ActionType.Movement:
                lastProcessed = LastProcessed.Movement;
                transform.position = data.position;          // <— replace rb.MovePosition
                break;

            case ActionType.SceneTransition:
                lastProcessed = LastProcessed.SceneTransition;
                TransitionToRoom(data.actionData, data.position);
                break;

            case ActionType.ItemPickup:
                lastProcessed = LastProcessed.Other;
                transform.position = data.position;
                HandleShadowItemPickup(data.actionData);
                break;

            case ActionType.ItemDrop:
                lastProcessed = LastProcessed.Other;
                transform.position = data.position;
                HandleShadowItemDrop(data.actionData);
                break;

            case ActionType.ButtonPress:
                lastProcessed = LastProcessed.Other;
                transform.position = data.position;
                HandleShadowButtonPress(data.actionData);
                break;
        }
    }



    void HandleShadowSceneTransition(string targetScene)
    {
        if (showDebugMessages)
        {
            // Debug.Log($"🎭 Shadow scene transition to: {targetScene}");
        }

        // Update the shadow's scene tracking
        currentScene = targetScene;

        // Find the shadow's next position in the target scene
        for (int i = currentIndex; i < recordingData.Count; i++)
        {
            if (recordingData[i].sceneName == targetScene)
            {
                transform.position = recordingData[i].position;
                // Debug.Log($"🎭 Shadow positioned at {transform.position} in {targetScene}");
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
            // Debug.Log($"🔍 Processing shadow pickup: '{actionData}'");
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
                    // Debug.Log($"✅ Shadow picked up phantom '{itemName}' at {originalPosition}");
                }
                else
                {
                    // Debug.LogError("❌ ShadowCarrySystem is null!");
                }
            }
            else
            {
                // Debug.LogError($"❌ Failed to parse position from '{actionData}'");
            }
        }
        else
        {
            // Debug.LogError($"❌ Invalid actionData format: '{actionData}'. Expected 'itemName|x|y'");
        }
    }

    void HandleShadowItemDrop(string actionData)
    {
        if (showDebugMessages)
        {
            // Debug.Log($"🔍 Processing shadow drop: '{actionData}'");
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
                    // Debug.Log($"✅ Shadow dropped phantom '{itemName}' at {dropPosition}");
                }
                else
                {
                    // Debug.LogError("❌ ShadowCarrySystem is null!");
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
            // Debug.Log($"🎭 Scene changed from {oldScene} to {newSceneName}");
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
            // Debug.Log($"🎭 Shadow in {newSceneName}: {(shouldBeVisible ? "VISIBLE" : "HIDDEN")}");
        }
    }

    void HandleShadowButtonPress(string recordedId)
    {
        if (showDebugMessages)
            // Debug.Log($"🕯️  Shadow button press: '{recordedId}'");

        // 1) try quick map (id or displayName)
        if (idMap == null)
        {
            idMap = new Dictionary<string, InteractableObject>();
            foreach (var it in FindObjectsOfType<InteractableObject>(true))
            {
                if (!idMap.ContainsKey(it.objectId)) idMap.Add(it.objectId, it);
                if (!idMap.ContainsKey(it.displayName)) idMap.Add(it.displayName, it);
            }
        }

        if (idMap.TryGetValue(recordedId, out InteractableObject target) && target != null)
        {
            TryShadowInteract(target);
            return;
        }

        // 2)  Fallback – pick the closest interactable in front of us
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, INTERACT_RADIUS);
        float best = float.MaxValue;
        InteractableObject nearest = null;

        foreach (var h in hits)
        {
            var io = h.GetComponent<InteractableObject>();
            if (io == null) continue;

            float d = Vector2.Distance(io.transform.position, transform.position);
            if (d < best)
            {
                best = d;
                nearest = io;
            }
        }

        if (nearest != null)
        {
            if (showDebugMessages) ;
                // Debug.Log($"🕯️  Fallback: using closest interactable '{nearest.displayName}'");

            TryShadowInteract(nearest);
        }
        else
        {
            if (showDebugMessages) ;
                // Debug.LogWarning($"🕯️  No interactable found for '{recordedId}'");
        }
    }

    void TryShadowInteract(InteractableObject io)
    {
        if (!io.canBeTriggereByShadow) return;
        io.ShadowInteract();
    }



    public void TransitionToRoom(string roomName, Vector3 pos)
    {
        /* 1) find FIRST action that belongs to the target room */
        int newIndex = currentIndex;
        while (newIndex < recordingData.Count &&
               recordingData[newIndex].sceneName != roomName)
        {
            newIndex++;
        }

        if (newIndex >= recordingData.Count)
        {
            // Debug.LogWarning($"🕯️  TransitionToRoom: room '{roomName}' not found in recording");
            return;
        }

        /* 2) bookkeeping */
        HandleShadowSceneTransition(roomName);

        /* 3) rewind virtual clock so we continue exactly at newIndex */
        playbackStartTime = Time.time - recordingData[newIndex].time;

        /* 4) position the shadow at the door-spawn */
        transform.position = pos;

        /* 5) IMPORTANT ► we will ++ the index in Update() right after
              ProcessAction returns, therefore we store (newIndex-1) here   */
        currentIndex = (newIndex > currentIndex) ? newIndex - 1 : currentIndex;

        if (myCol != null) { myCol.enabled = false; skipPhysicsFrames = 5; }
    }

}