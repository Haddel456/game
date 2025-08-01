using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausMenu;
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject homeButton;

    public void Pause()
    {
        pausMenu.SetActive(true);
        pauseButton.SetActive(false);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        pausMenu.SetActive(false);
        pauseButton.SetActive(true);
        Time.timeScale = 1;
    }


    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryLevel();
        }
        Time.timeScale = 1;
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1; 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            // Fallback in case GameManager isn't available
            SceneManager.LoadScene("Main");
        }
    }

}
