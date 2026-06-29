using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Game Over Settings")]
    public GameObject gameOverPanel;

    [Header("Navigation Settings")]
    public string levelSelectSceneName = "LevelScene"; 
    public string mainMenuSceneName = "FirstScene";

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        Time.timeScale = 1f; 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void GoToLevelsScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSelectSceneName);
    }

    // --- NEW QUIT LOGIC ---
    public void QuitGame()
    {
        Debug.Log("Quit Game Requested"); // This confirms it works in the Console
        Application.Quit(); // This closes the actual .exe or build
    }
}