using UnityEngine;

public class SimplePuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public bool isCompleted = false;
    public Color normalColor = Color.red;
    public Color completedColor = Color.green;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    void OnMouseDown()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            UpdateVisuals();

            // Check if all puzzles are complete
            CheckAllPuzzlesComplete();
        }
    }

    void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isCompleted ? completedColor : normalColor;
        }
    }

    void CheckAllPuzzlesComplete()
    {
        SimplePuzzle[] allPuzzles = FindObjectsOfType<SimplePuzzle>();

        foreach (SimplePuzzle puzzle in allPuzzles)
        {
            if (!puzzle.isCompleted)
            {
                return; // Not all complete yet
            }
        }

        // All puzzles complete!
        Debug.Log("All puzzles completed!");
        GameManager.Instance.CompletePuzzle();
    }
}