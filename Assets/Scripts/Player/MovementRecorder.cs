using UnityEngine;
using System.Collections.Generic;

public enum ActionType
{
    Movement,
    SceneTransition,
    ItemPickup,
    ItemDrop,  // Add this line
    ButtonPress
}

[System.Serializable]
public struct PositionData
{
    public float time;
    public Vector2 position;
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
                position = position
            };

            currentRecording.Add(data);
            lastRecordTime = currentTime;
        }
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Debug.Log($"Stopped recording. Recorded {currentRecording.Count} positions");

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

    public void RecordItemPickup(string itemName, Vector2 itemPosition)
    {
        if (!isRecording) return;

        PositionData data = new PositionData
        {
            time = Time.time - dayStartTime,
            position = transform.position,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
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
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            actionType = ActionType.ItemDrop,
            actionData = itemName + "|" + dropPosition.x + "|" + dropPosition.y
        };

        currentRecording.Add(data);
        Debug.Log($"Recorded item drop: {itemName} at {dropPosition}");
    }
}