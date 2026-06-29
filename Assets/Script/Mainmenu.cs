using UnityEngine;
using UnityEngine.SceneManagement; // Essential for changing scenes

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // This must match the name of your scene exactly
        SceneManager.LoadScene("LevelScene");
    }
}