using TMPro;
using UnityEngine;

public class DiamondScoreManager : MonoBehaviour
{
    public static DiamondScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text diamondScoreText;

    private int diamondCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        diamondCount = 0;
        UpdateScoreText();
    }

    public void AddDiamond(int amount)
    {
        if (amount <= 0)
            return;

        diamondCount += amount;
        UpdateScoreText();
    }

    public void ResetScore()
    {
        diamondCount = 0;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (diamondScoreText != null)
        {
            diamondScoreText.text = "× " + diamondCount;
        }
    }
}