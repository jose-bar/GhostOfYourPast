using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class RemoteToTVLink : MonoBehaviour
{
    public TVPuzzle tv;                        // drag the TV here in Inspector

    public void PressRemote()                  // called by both player & shadow
    {
        tv?.EnableWindow();
    }
}
