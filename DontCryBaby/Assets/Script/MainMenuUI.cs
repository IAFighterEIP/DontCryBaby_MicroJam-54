using UnityEngine;
using UnityEngine.SceneManagement; // Only needed if loading scenes

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Play button clicked!");
        SceneManager.LoadScene("GameMap");
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked!");
        Application.Quit();
    }
}