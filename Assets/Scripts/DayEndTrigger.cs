using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DayEndTrigger : MonoBehaviour
{
    [Header("Activation")]
    [Tooltip("1-based day number.  -1 = every day")]
    public int dayNumber = 1;
    public bool checkPuzzleSolved = true;

    [Header("Result")]
    public bool advancesDay = true;   // false = failure / repeat
    public float cooldownSeconds = 3f;

    float lastFire = -Mathf.Infinity;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (Time.time - lastFire < cooldownSeconds) return;

        if (dayNumber > 0 && dayNumber != GameManager.Instance.currentDay) return;
        if (checkPuzzleSolved && !GameManager.Instance.HasPuzzleBeenSolved()) return;

        lastFire = Time.time;

        if (advancesDay)
            GameManager.Instance.EndDaySuccess();
        else GameManager.Instance.EndDayFailure();
    }
}
