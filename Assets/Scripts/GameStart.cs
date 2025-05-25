using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // Name of your gameplay scene

    public void StartGameNow()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game quit (in editor this does nothing)");
    }
}
