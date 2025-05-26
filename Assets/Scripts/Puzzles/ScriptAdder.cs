using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableObject))]
public class AutoPuzzleBinder : MonoBehaviour
{
    void Awake()
    {
        InteractableObject io = GetComponent<InteractableObject>();
        TVPuzzle tv = GetComponent<TVPuzzle>();
        if (io == null || tv == null) return;          // not a TV puzzle

        UnityAction call = tv.AttemptInteract;

        if (!io.OnInteract.GetPersistentEventCount().Equals(0)) return;  // already wired by designer
        io.OnInteract.AddListener(call);
    }
}
