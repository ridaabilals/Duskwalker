using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject levelCompleteCanvas; 
    public string nextSceneName = "Level2"; 

    [Header("Audio Settings")]
    public AudioSource levelAudioSource; // Drag your AudioSource here
    public AudioClip winSound;           // Drag your sound clip here

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        // 1. Show the UI
        if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(true); 

        // 2. Play the finish sound
        if (levelAudioSource != null && winSound != null)
        {
            levelAudioSource.PlayOneShot(winSound);
        }

        // 3. Pause the game
        Time.timeScale = 0f; 
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f; // FIXED: Must be 1f so the next level isn't frozen!
        SceneManager.LoadScene(nextSceneName);
    }
}