using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentDay = 1;
    public float dayTimer = 60f;
    public bool puzzleCompleted = false;

    [Header("UI References")]
    public Text dayText;
    public Text timerText;

    [Header("Debug")]
    public bool showDebugMessages = true;

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
                RestartDay();
            }
        }
    }

    public void CompletePuzzle()
    {
        puzzleCompleted = true;

        if (showDebugMessages)
        {
            Debug.Log($"🎯 Puzzle completed on day {currentDay}!");
        }

        // Stop recording BEFORE advancing day
        MovementRecorder.Instance.StopRecording();

        if (showDebugMessages)
        {
            Debug.Log("⏰ Waiting 2 seconds before advancing day...");
        }

        Invoke("AdvanceDay", 2f);
    }

    void AdvanceDay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"📅 Advancing from day {currentDay} to day {currentDay + 1}");
        }

        currentDay++;
        StartNewDay();
    }

    void RestartDay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"⏰ Time's up! Restarting day {currentDay}");
        }
        StartNewDay();
    }

    void StartNewDay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"🌅 Starting day {currentDay}");
        }

        dayTimer = 60f;
        puzzleCompleted = false;

        // Start recording new day
        MovementRecorder.Instance.StartNewDay();

        // Create shadow from previous day (only if we have recordings)
        if (currentDay > 1)
        {
            if (showDebugMessages)
            {
                Debug.Log($"🎭 Attempting to create shadow for day {currentDay}...");
            }
            ShadowManager.Instance.CreateShadow();
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.Log("🎭 Day 1 - no shadow to create yet");
            }
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


    public void FreezeGameForSeconds(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    private System.Collections.IEnumerator FreezeCoroutine(float duration)
    {
        if (showDebugMessages)
            Debug.Log($"⏸️ Freezing game for {duration} seconds...");

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;

        if (showDebugMessages)
            Debug.Log("▶️ Game resumed");
    }
}

