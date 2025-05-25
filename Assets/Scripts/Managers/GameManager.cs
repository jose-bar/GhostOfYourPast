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

    bool puzzleSolvedThisDay = false;      // internal flag

    public bool HasPuzzleBeenSolved()      // called by DayEndTrigger
    {
        return puzzleSolvedThisDay;
    }

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
        if (Input.GetKeyDown(KeyCode.Q))
            EndDayFailure();

        if (!puzzleCompleted)
        {
            dayTimer -= Time.deltaTime;
            UpdateUI();

            if (dayTimer <= 0)
            {
                EndDayFailure();
            }
        }
    }

    public void CompletePuzzle()
    {
        if (showDebugMessages)
            Debug.Log($"✅ Puzzle completed on day {currentDay}");

        puzzleCompleted = true;
        puzzleSolvedThisDay = true;
    }

    public void EndDaySuccess()          // called by e.g. Bed trigger
    {
        if (showDebugMessages)
            Debug.Log($"🌙  Day {currentDay} SUCCESS → advance");

        MovementRecorder.Instance.StopRecording();
        currentDay++;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController2D player = playerObj.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.KillPlayer();
            }
        }
        StartNewDay();
        ShadowManager.Instance.CreateShadow();
    }

    public void EndDayFailure()          // timer ran out, wrong trigger
    {
        if (showDebugMessages)
            Debug.Log($"💀  Day {currentDay} FAILURE → repeat same day");

        MovementRecorder.Instance.StopRecording();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController2D player = playerObj.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.KillPlayer();
            }
        }
        StartNewDay();                   // same day number
        ShadowManager.Instance.CreateShadow();
    }

    [HideInInspector] public float dayStartRealtime;   // in StartNewDay()

    void StartNewDay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"🌅 Starting day {currentDay}");
        }
        dayStartRealtime = Time.time;

        DayResetManager.Instance?.ResetDay();
        dayTimer = 60f;
        puzzleCompleted = false;
        puzzleSolvedThisDay = false;

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

