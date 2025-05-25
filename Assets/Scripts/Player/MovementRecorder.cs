using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct PositionData
{
    public float time;
    public Vector2 position;
    public string sceneName;
    public ActionType actionType;
    public string actionData; // For storing extra info like item names, positions, etc.
}

public enum ActionType
{
    Movement,
    SceneTransition,
    ItemPickup,
    ItemDrop,
    ButtonPress
}

public class MovementRecorder : MonoBehaviour
{
    public static MovementRecorder Instance;

    [Header("Recording Settings")]
    public float recordInterval = 0.1f; // Record every 0.1 seconds

    private List<PositionData> currentRecording = new List<PositionData>();
    private List<List<PositionData>> allRecordings = new List<List<PositionData>>();

    private bool isRecording = false;
    private float lastRecordTime = 0f;
    private float dayStartTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    // Return the currently active ROOM name (falls back to Unity scene)
    string CurrentRoomName
    {
        get
        {
            if (RoomManager.Instance != null && RoomManager.Instance.GetCurrentRoom() != null)
                return RoomManager.Instance.GetCurrentRoom().roomName;

            // fallback – single-scene projects
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }

    public void StartNewDay()
    {
        Debug.Log("Starting new day recording...");

        currentRecording.Clear();
        isRecording = true;
        dayStartTime = Time.time;
        lastRecordTime = 0f;
    }

    public void RecordPosition(Vector2 position)
    {
        if (!isRecording) return;

        float currentTime = Time.time - dayStartTime;

        // Only record if enough time has passed
        if (currentTime - lastRecordTime >= recordInterval)
        {
            PositionData data = new PositionData
            {
                time = currentTime,
                position = position,
                sceneName = CurrentRoomName,
                actionType = ActionType.Movement,
                actionData = ""
            };

            currentRecording.Add(data);
            lastRecordTime = currentTime;
        }
    }

    public void RecordSceneTransition(string targetScene)
    {
        if (!isRecording) return;

        PositionData data = new PositionData
        {
            time = Time.time - dayStartTime,
            position = transform.position,
            sceneName = CurrentRoomName,
            actionType = ActionType.SceneTransition,
            actionData = targetScene
        };

        currentRecording.Add(data);
        Debug.Log($"Recorded scene transition to {targetScene}");
    }

    public void RecordItemPickup(string itemName, Vector2 itemPosition)
    {
        if (!isRecording) return;

        PositionData data = new PositionData
        {
            time = Time.time - dayStartTime,
            position = transform.position,
            sceneName = CurrentRoomName,
            actionType = ActionType.ItemPickup,
            actionData = itemName + "|" + itemPosition.x + "|" + itemPosition.y
        };

        currentRecording.Add(data);
        Debug.Log($"Recorded item pickup: {itemName} at {itemPosition}");
    }

    public void RecordItemDrop(string itemName, Vector2 dropPosition)
    {
        if (!isRecording) return;

        PositionData data = new PositionData
        {
            time = Time.time - dayStartTime,
            position = transform.position,
            sceneName = CurrentRoomName,
            actionType = ActionType.ItemDrop,
            actionData = itemName + "|" + dropPosition.x + "|" + dropPosition.y
        };

        currentRecording.Add(data);
        Debug.Log($"Recorded item drop: {itemName} at {dropPosition}");
    }

    public void RecordButtonPress(string buttonName)
    {
        if (!isRecording) return;

        PositionData data = new PositionData
        {
            time = Time.time - dayStartTime,
            position = transform.position,
            sceneName = CurrentRoomName,
            actionType = ActionType.ButtonPress,
            actionData = buttonName
        };

        currentRecording.Add(data);
        Debug.Log($"Recorded button press: {buttonName}");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Debug.Log($"Stopped recording. Recorded {currentRecording.Count} actions");

        isRecording = false;

        // Save this recording
        allRecordings.Add(new List<PositionData>(currentRecording));
    }

    public List<PositionData> GetRecording(int dayIndex)
    {
        if (dayIndex >= 0 && dayIndex < allRecordings.Count)
        {
            return allRecordings[dayIndex];
        }
        return null;
    }

    public int GetRecordingCount()
    {
        return allRecordings.Count;
    }
}