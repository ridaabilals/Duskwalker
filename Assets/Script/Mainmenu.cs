using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "levelone";

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }
}