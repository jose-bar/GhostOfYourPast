using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [System.Serializable]
    public class PuzzleStep
    {
        public string stepID;
        public string description;
        public bool isCompleted;
    }

    [System.Serializable]
    public class DayPuzzle
    {
        public int dayNumber;
        public List<PuzzleStep> steps;
        public string completionMessage;
    }

    public List<DayPuzzle> dayPuzzles = new List<DayPuzzle>();
    public Text objectiveText;

    private DayPuzzle currentPuzzle;

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

        SetupPuzzles();
    }

    void Start()
    {
        SetCurrentDayPuzzle(GameManager.Instance.currentDay);
        UpdateObjectiveText();
    }

    void SetupPuzzles()
    {
        // Day 1: Escape bedroom
        DayPuzzle day1 = new DayPuzzle
        {
            dayNumber = 1,
            steps = new List<PuzzleStep>
            {
                new PuzzleStep { stepID = "open_drawer", description = "Find the key", isCompleted = false },
                new PuzzleStep { stepID = "unlock_door", description = "Unlock bedroom door", isCompleted = false },
                new PuzzleStep { stepID = "drink_water", description = "Find something in kitchen", isCompleted = false }
            },
            completionMessage = "You passed out after drinking water..."
        };

        // Day 2: Follow shadow
        DayPuzzle day2 = new DayPuzzle
        {
            dayNumber = 2,
            steps = new List<PuzzleStep>
            {
                
            },
            completionMessage = "..."
        };

        dayPuzzles.Add(day1);
        dayPuzzles.Add(day2);

        // ...
    }

    public void SetCurrentDayPuzzle(int day)
    {
        currentPuzzle = dayPuzzles.Find(p => p.dayNumber == day);

        if (currentPuzzle == null)
        {
            Debug.LogWarning($"No puzzle defined for day {day}");
        }
    }

    public void CompleteStep(string stepID)
    {
        if (currentPuzzle == null) return;

        foreach (var step in currentPuzzle.steps)
        {
            if (step.stepID == stepID)
            {
                step.isCompleted = true;
                Debug.Log($"Completed puzzle step: {stepID}");
                break;
            }
        }

        UpdateObjectiveText();
        CheckPuzzleCompletion();
    }

    void CheckPuzzleCompletion()
    {
        if (currentPuzzle == null) return;

        bool allComplete = true;
        foreach (var step in currentPuzzle.steps)
        {
            if (!step.isCompleted)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            Debug.Log($"Day {currentPuzzle.dayNumber} puzzle complete!");
            Debug.Log(currentPuzzle.completionMessage);

            // Auto-complete the puzzle after a delay
            Invoke("TriggerPuzzleCompletion", 1.5f);
        }
    }

    void TriggerPuzzleCompletion()
    {
        GameManager.Instance.CompletePuzzle();
    }

    void UpdateObjectiveText()
    {
        if (objectiveText == null || currentPuzzle == null) return;

        string text = $"Day {currentPuzzle.dayNumber} Objectives:\n";

        foreach (var step in currentPuzzle.steps)
        {
            string checkmark = step.isCompleted ? "✓" : "□";
            text += $"{checkmark} {step.description}\n";
        }

        objectiveText.text = text;
    }
}
