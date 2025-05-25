using UnityEngine;

public class DayActive : MonoBehaviour, IResettable
{
    [Tooltip("Activate object only on these day numbers (1-based). Leave empty = always on.")]
    public int[] activeDays;

    bool originalState;

    void Awake()
    {
        originalState = gameObject.activeSelf;
        DayResetManager.Instance.Register(this);
        Apply();
    }

    public void Apply()
    {
        if (activeDays == null || activeDays.Length == 0)
        {
            gameObject.SetActive(originalState);
            return;
        }

        int d = GameManager.Instance.currentDay;
        bool on = false;
        foreach (int n in activeDays) if (n == d) { on = true; break; }
        gameObject.SetActive(on);
    }

    public void ResetState() => Apply();
}
