using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameWinUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI GameOver;
    [SerializeField] private TextMeshProUGUI congratsText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Setup button click events
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
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