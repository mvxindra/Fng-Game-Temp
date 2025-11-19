using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("main scene");
    }

    public void OpenSettings()
    {
        // Implement settings menu logic here
        Debug.Log("Settings menu opened");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
        Debug.Log("Game Quit");
    }
}