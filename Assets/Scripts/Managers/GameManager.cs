// ==================== STEP 1: BASIC SCRIPTS ====================

// 1. GameManager.cs - Attach to empty GameObject called "GameManager"
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentDay = 1;
    public float dayTimer = 60f; // 1 minute for testing
    public bool puzzleCompleted = false;

    [Header("UI References")]
    public Text dayText;
    public Text timerText;

    private float startTime;

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
        StartNewDay();
    }

    void Update()
    {
        if (!puzzleCompleted)
        {
            dayTimer -= Time.deltaTime;
            UpdateUI();

            if (dayTimer <= 0)
            {
                // Time's up, restart day
                RestartDay();
            }
        }
    }

    public void CompletePuzzle()
    {
        puzzleCompleted = true;
        MovementRecorder.Instance.StopRecording();

        Debug.Log("Puzzle Complete! Moving to next day...");

        // Wait a moment, then advance day
        Invoke("AdvanceDay", 2f);
    }

    void AdvanceDay()
    {
        currentDay++;
        StartNewDay();
    }

    void RestartDay()
    {
        Debug.Log("Time's up! Restarting day...");
        StartNewDay();
    }

    void StartNewDay()
    {
        dayTimer = 60f; // Reset timer
        puzzleCompleted = false;

        // Start recording new day
        MovementRecorder.Instance.StartNewDay();

        // Create shadow from previous day
        if (currentDay > 1)
        {
            ShadowManager.Instance.CreateShadow();
        }

        UpdateUI();

        // Reset player position
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero; // or your spawn point
        }
    }

    void UpdateUI()
    {
        if (dayText != null)
            dayText.text = "Day: " + currentDay;

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(dayTimer);
            timerText.text = "Time: " + seconds;
        }
    }
}