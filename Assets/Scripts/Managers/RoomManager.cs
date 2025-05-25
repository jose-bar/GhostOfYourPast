using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [System.Serializable]
    public class Room
    {
        public string roomName;
        public Transform cameraPosition;
        public Vector2 playerSpawnPoint;
    }

    public Room[] rooms;
    public Camera mainCamera;

    private Room currentRoom;

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
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Set initial room based on player's starting position
        SetInitialRoom();
    }

    void SetInitialRoom()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && rooms.Length > 0)
        {
            // Default to first room
            SwitchToRoom(rooms[0].roomName, false); // Don't notify shadow during initial setup

            // Try to find the room player is in
            foreach (Room room in rooms)
            {
                SwitchToRoom(room.roomName, false); // Don't notify shadow during initial setup
                break;
            }
        }
    }

    // Overloaded method to control shadow notification
    public void SwitchToRoom(string roomName, bool notifyShadow = true)
    {
        foreach (Room room in rooms)
        {
            if (room.roomName == roomName)
            {
                currentRoom = room;

                Debug.Log($"Switched to room: {roomName}");

                // Only notify shadow if requested (prevents interference during player-initiated transitions)
                if (notifyShadow)
                {
                    GameObject shadow = GameObject.FindWithTag("Shadow");
                    if (shadow != null)
                    {
                        ShadowPlayback shadowScript = shadow.GetComponent<ShadowPlayback>();
                        if (shadowScript != null)
                        {
                            shadowScript.OnSceneChanged(roomName);
                        }
                    }
                }

                return;
            }
        }

        Debug.LogWarning($"Room not found: {roomName}");
    }

    // Original method maintained for backward compatibility
    public void SwitchToRoom(string roomName)
    {
        SwitchToRoom(roomName, true);
    }

    public void TeleportPlayer(Vector2 position)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log($"Teleporting player from {player.transform.position} to {position}");

            // Force position directly with multiple approaches for robustness
            player.transform.position = position;

            // Also update rigidbody if it exists
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = position;
            }

            Debug.Log($"Player teleportation complete - now at {player.transform.position}");
        }
        else
        {
            Debug.LogError("Player not found! Cannot teleport.");
        }
    }

    // Method specifically for player-initiated room transitions (won't interfere with shadow)
    public void PlayerTransitionToRoom(string roomName, Vector2 position)
    {
        Debug.Log($"Player transitioning to room: {roomName}");

        // Switch room without notifying shadow (shadow manages its own scene transitions)
        SwitchToRoom(roomName, false);

        // Teleport player
        TeleportPlayer(position);
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
}