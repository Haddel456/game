using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("UI Text")]
    [SerializeField] private string gameOverMessage = "Game Over!";
    [SerializeField] private string levelFailedText = "Failed at Level {0}";
    [SerializeField] private string castleDestroyedText = "Your castle has been destroyed!";

    void Start()
    {
        SetupButtons();
        SetupGameOverText();
    }

    private void SetupButtons()
    {
        // Setup button click events
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void SetupGameOverText()
    {
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }

        // Show additional failure message
        if (levelText != null)
        {
            levelText.text = castleDestroyedText;
        }
    }

    public void SetLevelNumber(int levelNumber)
    {
        if (levelText != null)
        {
            string failureMessage = string.Format(levelFailedText, levelNumber);
            levelText.text = $"{failureMessage}\n{castleDestroyedText}";
        }

        // Update retry button text
        if (retryButton != null)
        {
            TextMeshProUGUI buttonText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Retry Level {levelNumber}";
            }
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

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}