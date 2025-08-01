using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WarriorSpawner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int currentLevel = 1;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnParent; // Parent object for spawned warriors

    [Header("Castle Reference")]
    private GameObject castle; // Reference to your castle
    private HealthComponent castleHealth; // Castle's health component

    private LevelConfiguration currentLevelConfig;
    private List<GameObject> activeWarriors = new List<GameObject>();
    private bool isSpawning = false;
    private bool levelActive = false;
    private int totalWarriorsToSpawn = 0;
    private int warriorsSpawned = 0;
    private WarriorDataSO warriorDataSO;

    // Game State
    public enum GameState
    {
        WaitingToStart,
        LevelActive,
        LevelWon,
        GameOver
    }

    private GameState currentGameState = GameState.WaitingToStart;

    // Events
    public static System.Action<int> OnLevelStarted;
    public static System.Action<int> OnLevelCompleted;
    public static System.Action OnAllWarriorsDefeated;
    public static System.Action OnGameWon; // All levels completed
    public static System.Action OnGameLost; // Castle destroyed

    // Public method to set WarriorDataSO
    public void SetWarriorDataSO(WarriorDataSO data)
    {
        warriorDataSO = data;
        Debug.Log("WarriorDataSO set successfully via GameManager!");
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        // Find castle if not assigned
        if (castle == null)
            castle = GameObject.FindGameObjectWithTag("ourcastle");

        if (castleHealth == null && castle != null)
            castleHealth = castle.GetComponent<HealthComponent>();

        // Subscribe to events
        WarriorController.OnWarriorDeath += OnWarriorDestroyed;

        // Subscribe to castle destruction if it has health component
        if (castleHealth != null)
        {
            castleHealth.OnDeath += OnCastleDestroyed;
        }
    }

    void OnDestroy()
    {
        WarriorController.OnWarriorDeath -= OnWarriorDestroyed;
        if (castleHealth != null)
            castleHealth.OnDeath -= OnCastleDestroyed;
    }

    public void StartLevel(int levelNumber)
    {
        // Make sure WarriorDataSO is loaded before starting level
        if (warriorDataSO == null)
        {
            Debug.LogError("Cannot start level - WarriorDataSO is not loaded!");
            return;
        }

        currentLevel = levelNumber;
        currentLevelConfig = warriorDataSO.GetLevelConfiguration(levelNumber);

        if (currentLevelConfig == null)
        {
            Debug.LogError($"No configuration found for level {levelNumber}");
            OnGameWon?.Invoke(); // No more levels = game won
            return;
        }

        // Reset level state
        currentGameState = GameState.LevelActive;
        levelActive = true;
        isSpawning = false;
        warriorsSpawned = 0;

        // Calculate total warriors for this level
        totalWarriorsToSpawn = 0;
        foreach (var spawnData in currentLevelConfig.warriorSpawns)
        {
            totalWarriorsToSpawn += spawnData.count;
        }

        // Restore castle health to full
        RestoreCastleHealth();

        Debug.Log($"Starting Level {levelNumber}: {currentLevelConfig.levelName} - {totalWarriorsToSpawn} warriors to spawn");
        OnLevelStarted?.Invoke(levelNumber);

        StartCoroutine(SpawnWarriorsForLevel());
    }

    private void RestoreCastleHealth()
    {
        if (castleHealth != null)
        {
            castleHealth.SetMaxHealth(100f);
            castleHealth.RestoreToFullHealth(); 
            Debug.Log("Castle health restored to 100%");
        }
    }

    private IEnumerator SpawnWarriorsForLevel()
    {
        if (currentGameState != GameState.LevelActive) yield break;

        isSpawning = true;

        // Create a list of all warriors to spawn
        List<WarriorType> warriorsToSpawn = new List<WarriorType>();

        foreach (var spawnData in currentLevelConfig.warriorSpawns)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                warriorsToSpawn.Add(spawnData.warriorType);
            }
        }

        // Shuffle the list for random spawn order
        for (int i = 0; i < warriorsToSpawn.Count; i++)
        {
            WarriorType temp = warriorsToSpawn[i];
            int randomIndex = Random.Range(i, warriorsToSpawn.Count);
            warriorsToSpawn[i] = warriorsToSpawn[randomIndex];
            warriorsToSpawn[randomIndex] = temp;
        }

        // Spawn warriors with random delays
        foreach (var warriorType in warriorsToSpawn)
        {
            if (currentGameState != GameState.LevelActive) break; // Stop spawning if game ended

            SpawnWarrior(warriorType);
            warriorsSpawned++;

            float delay = Random.Range(currentLevelConfig.minSpawnDelay, currentLevelConfig.maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }

        isSpawning = false;
        Debug.Log($"Finished spawning {warriorsSpawned}/{totalWarriorsToSpawn} warriors for level {currentLevel}");

        // Check if level should end immediately
        CheckLevelCompletion();
    }

    private void SpawnWarrior(WarriorType type)
    {
        if (currentGameState != GameState.LevelActive) return;

        WarriorData warriorData = warriorDataSO.GetWarriorData(type);
        if (warriorData == null || warriorData.prefab == null)
        {
            Debug.LogError($"No warrior data or prefab found for type {type}");
            return;
        }

        // Calculate spawn position
        float spawnY = Random.Range(currentLevelConfig.spawnYRange.x, currentLevelConfig.spawnYRange.y);
        Vector3 spawnPosition = new Vector3(currentLevelConfig.spawnXPosition, spawnY, 0f);

        // Spawn the warrior
        GameObject warrior = Instantiate(warriorData.prefab, spawnPosition, Quaternion.identity);

        if (spawnParent != null)
            warrior.transform.SetParent(spawnParent);

        // Configure the warrior
        ConfigureWarrior(warrior, warriorData);

        activeWarriors.Add(warrior);
    }

    private void ConfigureWarrior(GameObject warrior, WarriorData data)
    {
        WarriorController controller = warrior.GetComponent<WarriorController>();
        if (controller == null)
        {
            Debug.LogError("Spawned warrior doesn't have WarriorController component!");
            return;
        }

        // Apply base stats with level modifiers
        float modifiedSpeed = data.baseSpeed * currentLevelConfig.speedMultiplier;
        float modifiedDamage = data.damage * currentLevelConfig.damageMultiplier;

        controller.speed = modifiedSpeed;
        controller.SetDamageAmount(modifiedDamage);

        // Set other properties if WarriorController has these methods
        SetWarriorProperty(controller, "killHeightPercentage", data.killHeightPercentage);

        // Configure health if warrior has health component
        HealthComponent health = warrior.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.SetMaxHealth(100f);
        }

        // Set sprite if different from prefab
        if (data.warriorSprite != null)
        {
            SpriteRenderer spriteRenderer = warrior.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = data.warriorSprite;
            }
        }
    }

    private void SetWarriorProperty(WarriorController controller, string propertyName, float value)
    {
        // Use reflection to set properties dynamically
        var field = controller.GetType().GetField(propertyName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(controller, value);
        }
    }

    private void OnWarriorDestroyed()
    {
        if (currentGameState != GameState.LevelActive) return;

        // Clean up the list - Remove all null references
        int beforeCount = activeWarriors.Count;
        activeWarriors.RemoveAll(warrior => warrior == null);
        int afterCount = activeWarriors.Count;


        // Additional safety check - if we have more warriors in list than expected, clean up
        if (activeWarriors.Count > totalWarriorsToSpawn)
        {
            Debug.LogWarning($"More active warriors ({activeWarriors.Count}) than total to spawn ({totalWarriorsToSpawn}). Cleaning up...");
            // Force cleanup of any warriors that might be duplicated
            for (int i = activeWarriors.Count - 1; i >= 0; i--)
            {
                if (activeWarriors[i] == null || !activeWarriors[i].GetComponent<WarriorController>().IsAlive())
                {
                    activeWarriors.RemoveAt(i);
                }
            }
        }
        if (activeWarriors.Count == 1 && !isSpawning)
        {
            ForceCleanupWarriors(); 
        }

        CheckLevelCompletion();

        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (currentGameState != GameState.LevelActive) return;

        // Win condition: All warriors spawned and all warriors defeated
        if (!isSpawning && activeWarriors.Count == 0 && warriorsSpawned >= totalWarriorsToSpawn)
        {
            CompleteLevel(true);
        }
    }

    private void OnCastleDestroyed()
    {
        if (currentGameState != GameState.LevelActive) return;

        Debug.Log("Castle destroyed! Game Over!");
        CompleteLevel(false);
    }

    private void CompleteLevel(bool won)
    {
        if (currentGameState != GameState.LevelActive) return;

        levelActive = false;
        StopAllCoroutines(); // Stop any ongoing spawning

        if (won)
        {
            currentGameState = GameState.LevelWon;
            OnAllWarriorsDefeated?.Invoke();
            OnLevelCompleted?.Invoke(currentLevel);

            Debug.Log($"Level {currentLevel} completed! All {totalWarriorsToSpawn} warriors defeated!");


        }
        else
        {
            currentGameState = GameState.GameOver;
            Debug.Log("💀 GAME OVER - Castle Destroyed! 💀");
            OnGameLost?.Invoke();

            // Clear remaining warriors
            ClearAllWarriors();
        }
    }

    private IEnumerator StartNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Wait 3 seconds before next level

        int nextLevel = currentLevel + 1;
        if (warriorDataSO.GetLevelConfiguration(nextLevel) != null)
        {
            StartLevel(nextLevel);
        }
    }

    // Public methods for manual control
    public void RestartCurrentLevel()
    {
        StopAllCoroutines();
        ClearAllWarriors();
        currentGameState = GameState.WaitingToStart;
        StartLevel(currentLevel);
    }

    public void StartNextLevel()
    {
        if (currentLevel < 4) // Assuming 4 levels max
        {
            StopAllCoroutines();
            ClearAllWarriors();
            currentGameState = GameState.WaitingToStart;
            StartLevel(currentLevel + 1);
        }
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        ClearAllWarriors();
        currentGameState = GameState.WaitingToStart;
        currentLevel = 1;
        StartLevel(1);
    }

    private void ClearAllWarriors()
    {
        foreach (var warrior in activeWarriors)
        {
            if (warrior != null)
                Destroy(warrior);
        }
        activeWarriors.Clear();
        warriorsSpawned = 0;
    }

    // Getter methods for UI or other systems
    public int GetCurrentLevel() => currentLevel;
    public int GetActiveWarriorCount() => activeWarriors.Count;
    public int GetTotalWarriorsToSpawn() => totalWarriorsToSpawn;
    public int GetWarriorsSpawned() => warriorsSpawned;
    public GameState GetCurrentGameState() => currentGameState;
    public bool IsLevelActive() => levelActive;

    // Debug methods
    [ContextMenu("Start Level 1")]
    public void DebugStartLevel1() => StartLevel(1);

    [ContextMenu("Start Level 2")]
    public void DebugStartLevel2() => StartLevel(2);

    [ContextMenu("Start Level 3")]
    public void DebugStartLevel3() => StartLevel(3);

    [ContextMenu("Start Level 4")]
    public void DebugStartLevel4() => StartLevel(4);

    [ContextMenu("Restart Current Level")]
    public void DebugRestartLevel() => RestartCurrentLevel();

    [ContextMenu("Force Win Level")]
    public void DebugForceWin() => CompleteLevel(true);

    [ContextMenu("Force Lose Level")]
    public void DebugForceLose() => CompleteLevel(false);

    [ContextMenu("Force Cleanup Warriors")]
    public void ForceCleanupWarriors()
    {
        for (int i = activeWarriors.Count - 1; i >= 0; i--)
        {
            if (activeWarriors[i] == null)
            {
                Debug.Log($"Removing null warrior at index {i}");
                activeWarriors.RemoveAt(i);
            }
            else
            {
                var controller = activeWarriors[i].GetComponent<WarriorController>();
                if (controller == null)
                {
                    Debug.Log($"Removing warrior with no controller at index {i}: {activeWarriors[i].name}");
                    activeWarriors.RemoveAt(i);
                }
                else if (!controller.IsAlive())
                {
                    Debug.Log($"Removing dead warrior at index {i}: {activeWarriors[i].name}");
                    activeWarriors.RemoveAt(i);
                }
                else
                {
                    Debug.Log($"Keeping alive warrior at index {i}: {activeWarriors[i].name} - Position: {activeWarriors[i].transform.position}");
                }
            }
        }

        CheckLevelCompletion();
    }
}