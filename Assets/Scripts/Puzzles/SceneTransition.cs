using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string targetSceneName;
    public Vector2 playerSpawnPosition;
    public bool requiresKey = false;
    public string requiredKeyName = "";

    [Header("Visual Feedback")]
    public GameObject doorSprite; // Optional door visual
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    private bool playerInRange = false;
    private SpriteRenderer doorRenderer;

    void Start()
    {
        if (doorSprite != null)
        {
            doorRenderer = doorSprite.GetComponent<SpriteRenderer>();
            UpdateDoorVisual();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryTransition();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
        }
    }

    void TryTransition()
    {
        if (requiresKey)
        {
            // Check if player has required key
            PlayerInventory inventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
            if (inventory == null || !inventory.HasItem(requiredKeyName))
            {
                Debug.Log($"Need {requiredKeyName} to open this door!");
                return;
            }
        }

        // Store spawn position for next scene
        PlayerSpawnManager.Instance.SetSpawnData(playerSpawnPosition, targetSceneName);

        // Record scene transition
        MovementRecorder.Instance?.RecordSceneTransition(targetSceneName);

        // Load new scene
        SceneManager.LoadScene(targetSceneName);
    }

    void ShowPrompt()
    {
        string promptText = requiresKey ? $"Need {requiredKeyName}" : "Press E to enter";
        // You can implement UI prompt here
        Debug.Log(promptText);
    }

    void HidePrompt()
    {
        // Hide UI prompt
    }

    void UpdateDoorVisual()
    {
        if (doorRenderer == null) return;

        if (requiresKey)
        {
            PlayerInventory inventory = GameObject.FindWithTag("Player")?.GetComponent<PlayerInventory>();
            bool hasKey = inventory != null && inventory.HasItem(requiredKeyName);
            doorRenderer.sprite = hasKey ? openDoorSprite : closedDoorSprite;
        }
        else
        {
            doorRenderer.sprite = openDoorSprite;
        }
    }
}