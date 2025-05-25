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
        public Collider2D roomBounds;
    }

    public Room[] rooms;
    public Camera mainCamera;

    private Room currentRoom;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
            SwitchToRoom(rooms[0].roomName);

            // Try to find the room player is in
            foreach (Room room in rooms)
            {
                if (room.roomBounds != null &&
                    room.roomBounds.bounds.Contains(player.transform.position))
                {
                    SwitchToRoom(room.roomName);
                    break;
                }
            }
        }
    }

    public void SwitchToRoom(string roomName)
    {
        foreach (Room room in rooms)
        {
            if (room.roomName == roomName)
            {
                currentRoom = room;

                // Move camera to room position
                if (mainCamera != null && room.cameraPosition != null)
                {
                    mainCamera.transform.position = room.cameraPosition.position;

                    // Disable camera following if using CameraController
                    CameraController camController = mainCamera.GetComponent<CameraController>();
                    if (camController != null)
                    {
                        camController.enabled = false;
                    }
                }

                Debug.Log($"Switched to room: {roomName}");
                return;
            }
        }

        Debug.LogWarning($"Room not found: {roomName}");
    }

    public void TeleportPlayer(Vector2 position)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
            Debug.Log($"Player teleported to {position}");
        }
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
}
