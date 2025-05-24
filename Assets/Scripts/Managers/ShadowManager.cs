using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    public static ShadowManager Instance;

    [Header("Shadow Settings")]
    public GameObject shadowPrefab; // Assign your shadow prefab here

    private GameObject currentShadow;

    void Awake()
    {
        Instance = this;
    }

    public void CreateShadow()
    {
        // Remove previous shadow
        if (currentShadow != null)
        {
            Destroy(currentShadow);
        }

        // Get previous day's recording
        int previousDay = GameManager.Instance.currentDay - 2; // -2 because currentDay already incremented
        var recording = MovementRecorder.Instance.GetRecording(previousDay);

        if (recording != null && recording.Count > 0)
        {
            Debug.Log($"Creating shadow with {recording.Count} recorded positions");

            // Create new shadow
            currentShadow = Instantiate(shadowPrefab);
            ShadowPlayback playback = currentShadow.GetComponent<ShadowPlayback>();

            if (playback != null)
            {
                playback.Initialize(recording);
            }
        }
        else
        {
            Debug.Log("No recording found for previous day");
        }
    }
}