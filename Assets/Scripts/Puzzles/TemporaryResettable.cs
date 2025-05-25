using UnityEngine;

public class TemporaryResettable : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        Destroy(gameObject);        // one-shot objects vanish each morning
    }

    void OnEnable() { DayResetManager.Instance?.Register(this); }
    void OnDestroy() { DayResetManager.Instance?.Unregister(this); }
}
