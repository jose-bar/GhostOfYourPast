using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableObject))]
public class RemoteToTVLink : MonoBehaviour
{
    [Tooltip("The TV (root) that owns the TVPuzzle component")]
    public TVPuzzle tv;                // drag the TV gameObject here

    InteractableObject io;

    void Awake()
    {
        io = GetComponent<InteractableObject>();
        if (io == null || tv == null) return;

        // Add listeners ONCE so both player and shadow presses work
        UnityAction call = PressRemote;

        io.OnInteract.AddListener(call);        // player
        io.OnShadowInteract.AddListener(call);  // shadow
    }

    // This is what both events call
    public void PressRemote()
    {
        tv.EnableWindow();
    }
}
