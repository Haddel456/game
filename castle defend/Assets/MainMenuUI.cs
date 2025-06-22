using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("UI Text")]
    [SerializeField] private string gameTitle = "Defend Your Castle";
    [SerializeField] private string instructions = "Defend your castle from incoming warriors!\nSurvive 4 levels to win!";

    void Start()
    {
        SetupUI();
        SetupButtons();
    }

    private void SetupUI()
    {
        if (titleText != null)
        {
            titleText.text = gameTitle;
        }

        if (instructionsText != null)
        {
            instructionsText.text = instructions;
        }
    }

    private void SetupButtons()
    {
        // Setup button click events
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnStartGameClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
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