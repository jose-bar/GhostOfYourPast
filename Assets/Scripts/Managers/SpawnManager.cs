using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    private Vector2 spawnPosition = Vector2.zero;
    private string fromScene = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Move player to spawn position when scene loads
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && spawnPosition != Vector2.zero)
        {
            player.transform.position = spawnPosition;

            // Snap camera to player
            CameraController camera = Camera.main.GetComponent<CameraController>();
            if (camera != null)
            {
                camera.SnapToTarget();
            }
        }
    }

    public void SetSpawnData(Vector2 position, string targetScene)
    {
        spawnPosition = position;
        fromScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}