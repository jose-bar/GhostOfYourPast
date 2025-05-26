using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    public static ShadowManager Instance;
    
    [Header("Shadow Settings")]
    public GameObject shadowPrefab; // Make sure this has ShadowCarrySystem!
    
    private GameObject currentShadow;
    
    void Awake()
    {
        Instance = this;
    }

    public void CreateShadow()
    {
        // Remove previous shadow
        if (currentShadow != null) Destroy(currentShadow);

        // 1) which recording do we want? → **the last one saved**
        int lastIndex = MovementRecorder.Instance.GetRecordingCount() - 1;
        if (lastIndex < 0)
        {
            Debug.Log("No recording yet – skipping shadow");
            return;
        }

        var recording = MovementRecorder.Instance.GetRecording(lastIndex);

        if (recording == null || recording.Count == 0)
        {
            Debug.LogWarning("Recording empty – cannot build shadow");
            return;
        }

        Debug.Log($"Creating shadow from try #{lastIndex} – {recording.Count} actions");

        // 2) Instantiate and initialise
        currentShadow = Instantiate(shadowPrefab);

        ShadowPlayback playback = currentShadow.GetComponent<ShadowPlayback>();
        ShadowCarrySystem carry = currentShadow.GetComponent<ShadowCarrySystem>();
        if (playback == null)
        {
            Debug.LogError("Shadow prefab missing ShadowPlayback!");
            return;
        }
        if (carry == null) carry = currentShadow.AddComponent<ShadowCarrySystem>();

        playback.Initialize(recording);
    }

}