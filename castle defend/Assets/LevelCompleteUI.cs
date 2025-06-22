using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI congratsText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("UI Text")]
    [SerializeField] private string levelCompleteText = "Level {0} Complete!";
    [SerializeField] private string congratsMessage = "Excellent! You defended the castle!";

    void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Setup button click events
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Check if next level button should be enabled
        UpdateNextLevelButton();
    }

    public void SetLevelNumber(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = string.Format(levelCompleteText, levelNumber);
        }

        if (congratsText != null)
        {
            congratsText.text = congratsMessage;
        }

        UpdateNextLevelButton();
    }

    private void UpdateNextLevelButton()
    {
        if (nextLevelButton != null)
        {
            // Enable next level button only if there are more levels
            bool hasNextLevel = GameManager.GetCurrentLevel() < GameManager.GetMaxLevels();
            nextLevelButton.interactable = hasNextLevel;

            // Update button text
            TextMeshProUGUI buttonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (hasNextLevel)
                {
                    buttonText.text = $"Next Level ({GameManager.GetCurrentLevel() + 1})";
                }
                else
                {
                    buttonText.text = "Game Complete!";
                }
            }
        }
    }

    private void OnNextLevelClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryLevel();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}