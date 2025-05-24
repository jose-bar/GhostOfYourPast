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
        if (currentShadow != null)
        {
            Destroy(currentShadow);
        }
        
        // Get previous day's recording
        int previousDay = GameManager.Instance.currentDay - 2;
        var recording = MovementRecorder.Instance.GetRecording(previousDay);
        
        if (recording != null && recording.Count > 0)
        {
            Debug.Log($"Creating shadow with {recording.Count} recorded actions");
            
            // Create new shadow
            currentShadow = Instantiate(shadowPrefab);
            
            // Make sure shadow has required components
            ShadowPlayback playback = currentShadow.GetComponent<ShadowPlayback>();
            ShadowCarrySystem carrySystem = currentShadow.GetComponent<ShadowCarrySystem>();
            
            if (playback == null)
            {
                Debug.LogError("Shadow prefab missing ShadowPlayback component!");
                return;
            }
            
            if (carrySystem == null)
            {
                Debug.LogWarning("Shadow prefab missing ShadowCarrySystem component! Adding one...");
                carrySystem = currentShadow.AddComponent<ShadowCarrySystem>();
            }
            
            playback.Initialize(recording);
        }
        else
        {
            Debug.Log("No recording found for previous day");
        }
    }
}