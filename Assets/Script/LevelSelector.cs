using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    // This is the function we will use for your specific scene names
    public void OpenLevelByName(string levelName)
    {
        // This loads the scene by the text name you type in the inspector
        SceneManager.LoadScene(levelName);
    }
}