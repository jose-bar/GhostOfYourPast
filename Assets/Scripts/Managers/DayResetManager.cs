using System.Collections.Generic;
using UnityEngine;

public class DayResetManager : MonoBehaviour
{
    public static DayResetManager Instance;

    private readonly List<IResettable> resettables = new List<IResettable>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // Called from Awake / OnEnable of every resettable component
    public void Register(IResettable item)
    {
        if (!resettables.Contains(item)) resettables.Add(item);
    }

    public void Unregister(IResettable item)
    {
        resettables.Remove(item);
    }

    public void ResetDay()
    {
        // work on a copy so collection may change inside ResetState
        foreach (var r in new List<IResettable>(resettables))
            r.ResetState();
    }
}
