using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public string gameSceneName = "StartScreen"; // Name of your gameplay scene

    public void StartGameNow()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}