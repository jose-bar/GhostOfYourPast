using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance;
    
    [Header("Spawn Data")]
    public Vector2 spawnPosition = Vector2.zero;
    public string fromScene = "";
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    void Awake()
    {
        // Robust singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (showDebugMessages)
            {
                Debug.Log("📍 PlayerSpawnManager initialized and will persist across scenes");
            }
        }
        else if (Instance != this)
        {
            if (showDebugMessages)
            {
                Debug.Log("📍 Duplicate PlayerSpawnManager found, destroying");
            }
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugMessages)
        {
            Debug.Log($"📍 Scene loaded: {scene.name}, spawn position: {spawnPosition}");
        }
        
        // Move player to spawn position when scene loads
        PositionPlayer();
    }
    
    void PositionPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && spawnPosition != Vector2.zero)
        {
            player.transform.position = spawnPosition;
            
            if (showDebugMessages)
            {
                Debug.Log($"📍 Positioned player at: {spawnPosition}");
            }
            
            // Snap camera to player
            CameraController camera = Camera.main?.GetComponent<CameraController>();
            if (camera != null)
            {
                camera.SnapToTarget();
                if (showDebugMessages)
                {
                    Debug.Log("📷 Snapped camera to player");
                }
            }
            
            // Reset spawn position so it doesn't apply again
            spawnPosition = Vector2.zero;
        }
        else if (player == null)
        {
            Debug.LogWarning("❌ Player with 'Player' tag not found in new scene!");
        }
        else if (showDebugMessages)
        {
            Debug.Log("📍 No spawn position set, using default player position");
        }
    }
    
    public void SetSpawnData(Vector2 position, string targetScene)
    {
        spawnPosition = position;
        fromScene = SceneManager.GetActiveScene().name;
        
        if (showDebugMessages)
        {
            Debug.Log($"📍 Set spawn data: position {position}, from scene '{fromScene}' to '{targetScene}'");
        }
    }
    
    // Call this manually if automatic positioning fails
    public void ForcePositionPlayer()
    {
        PositionPlayer();
    }
}