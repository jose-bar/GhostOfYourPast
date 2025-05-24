using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public string spawnPointName = "DefaultSpawn";
    public bool isDefaultSpawn = true;

    void Start()
    {
        // Hide the spawn point marker in game
        GetComponent<Renderer>()?.material.SetColor("_Color", Color.clear);
    }

    // Method to manually spawn player here
    public void SpawnPlayerHere()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = transform.position;

            // Snap camera
            CameraController camera = Camera.main?.GetComponent<CameraController>();
            if (camera != null)
            {
                camera.SnapToTarget();
            }

            Debug.Log($"📍 Spawned player at {spawnPointName}: {transform.position}");
        }
    }

    void OnDrawGizmos()
    {
        // Draw spawn point in scene view
        Gizmos.color = isDefaultSpawn ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, spawnPointName);
#endif
    }
}