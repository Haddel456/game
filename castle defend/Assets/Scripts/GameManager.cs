using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "Main";
    [SerializeField] private string gameScene = "Game";   // For level 1 and 2 
    [SerializeField] private string game1Scene = "Game1";    // For level 3
    [SerializeField] private string game2Scene = "Game2";    // For level 4
    [SerializeField] private string levelCompleteScene = "NextLevel";
    [SerializeField] private string gameOverScene = "GameOver";
    [SerializeField] private string gameWinScene = "GameWin";

    [Header("Game Settings")]
    [SerializeField] private int maxLevels = 4;
    [SerializeField] private WarriorDataSO warriorDataSO; 

    // Static variables to persist between scenes
    public static int currentLevel = 1;
    public static bool gameWon = false;
    public static bool gameLost = false;

    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // If this is the first time, start from level 1
        if (currentLevel <= 0)
            currentLevel = 1;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Debug.Log($"Scene loaded: {scene.name}, Current Level: {currentLevel}");

        // Check if the loaded scene is any of our game scenes
        if (scene.name == gameScene || scene.name == game1Scene || scene.name == game2Scene)
        {
            StartCoroutine(InitializeGameSceneDelayed());
        }
        else if (scene.name == levelCompleteScene)
        {
            InitializeLevelCompleteScene();
        }
        else if (scene.name == gameOverScene)
        {
            InitializeGameOverScene();
        }
        else if (scene.name == gameWinScene)
        {
            InitializeGameWinScene();
        }
    }

    private IEnumerator InitializeGameSceneDelayed()
    {
        // Wait a frame to ensure all scene objects are loaded
        yield return new WaitForEndOfFrame();

        // Additional small delay to ensure everything is properly initialized
        yield return new WaitForSeconds(0.1f);

        InitializeGameScene();
    }

    private void InitializeGameScene()
    {
        // Find the WarriorSpawner and start the current level
        WarriorSpawner spawner = FindObjectOfType<WarriorSpawner>();
        if (spawner != null)
        {
            // Check if we have WarriorDataSO
            if (warriorDataSO == null)
            {
                Debug.LogError("WarriorDataSO is not assigned in GameManager!");
                return;
            }

            // Pass the WarriorDataSO to the spawner
            spawner.SetWarriorDataSO(warriorDataSO);

            // Subscribe to level completion events
            WarriorSpawner.OnLevelCompleted += OnLevelCompleted;
            WarriorSpawner.OnGameLost += OnGameLost;
            WarriorSpawner.OnGameWon += OnGameWon;

            // Start the current level
            Debug.Log($"Starting level {currentLevel} via GameManager");
            spawner.StartLevel(currentLevel);
        }
        else
        {
            Debug.LogError("WarriorSpawner not found in game scene!");
        }
    }

    private void InitializeLevelCompleteScene()
    {
        // Find and setup level complete UI
        LevelCompleteUI levelCompleteUI = FindObjectOfType<LevelCompleteUI>();
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetLevelNumber(currentLevel);
        }
    }

    private void InitializeGameOverScene()
    {
        // Find and setup game over UI
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.SetLevelNumber(currentLevel);
        }
    }

    private void InitializeGameWinScene()
    {
        // Game completed successfully
        Debug.Log("🎉 ALL LEVELS COMPLETED! 🎉");
    }

    // Event handlers for level completion
    private void OnLevelCompleted(int levelNumber)
    {
        Debug.Log($"Level {levelNumber} completed!");

        // Unsubscribe from events
        UnsubscribeFromSpawnerEvents();

        // Check if this was the last level
        if (levelNumber >= maxLevels)
        {
            // Game won - go to win scene
            LoadScene(gameWinScene);
        }
        else
        {
            // Increment level for next time
            //currentLevel++;
            // Go to level complete scene
            LoadScene(levelCompleteScene);
            
        }
    }

    private void OnGameLost()
    {
        Debug.Log("Game Lost - Castle Destroyed!");
        gameLost = true;

        // Unsubscribe from events
        UnsubscribeFromSpawnerEvents();

        // Go to game over scene
        LoadScene(gameOverScene);
    }

    private void OnGameWon()
    {
        Debug.Log("Game Won - All Levels Completed!");
        gameWon = true;

        // Unsubscribe from events
        UnsubscribeFromSpawnerEvents();

        // Go to game win scene
        LoadScene(gameWinScene);
    }

    private void UnsubscribeFromSpawnerEvents()
    {
        WarriorSpawner.OnLevelCompleted -= OnLevelCompleted;
        WarriorSpawner.OnGameLost -= OnGameLost;
        WarriorSpawner.OnGameWon -= OnGameWon;
    }


    private string GetGameSceneForCurrentLevel()
    {
        switch (currentLevel)
        {
            case 1:
            case 2:
                return gameScene;
            case 3:
                return game1Scene;
            case 4:
                return game2Scene;
            default:
                Debug.LogWarning($"Unexpected level {currentLevel}, defaulting to {gameScene}");
                return gameScene;
        }
    }

    // Update all scene-loading methods to use GetGameSceneForCurrentLevel()
    public void StartGame()
    {
        currentLevel = 1;
        gameWon = false;
        gameLost = false;
        LoadScene(GetGameSceneForCurrentLevel());
    }

    public void NextLevel()
    {
        if (currentLevel < maxLevels)
        {
            currentLevel++;
            LoadScene(GetGameSceneForCurrentLevel());
        }
        else if (currentLevel == maxLevels)
        {
            // This handles the case where player completes level 4
            LoadScene(gameWinScene);
        }
    }

    public void RetryLevel()
    {
        gameLost = false;
        LoadScene(GetGameSceneForCurrentLevel());
    }

    public void GoToMainMenu()
    {
        currentLevel = 1;
        gameWon = false;
        gameLost = false;
        LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // Helper method to load scenes
    private void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    // Getter methods for UI
    public static int GetCurrentLevel() => currentLevel;
    public static int GetMaxLevels() => Instance != null ? Instance.maxLevels : 4;
    public static bool IsGameWon() => gameWon;
    public static bool IsGameLost() => gameLost;
}