using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WarriorWave
{
    public GameObject warriorPrefab;
    public int count;
    public float damage;
}

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public List<WarriorWave> warriorWaves;
    public float spawnDelay = 2f; // Delay between each warrior spawn
    public float waveDelay = 5f;  // Delay between different warrior types
}

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private List<LevelData> levels;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int currentLevel = 0;

    [Header("UI References")]
    [SerializeField] private GameObject levelCompleteUI;
    [SerializeField] private GameObject gameOverUI;

    private bool levelInProgress = false;
    private int totalWarriorsInLevel = 0;
    private int warriorsSpawned = 0;
    private int warriorsKilled = 0;

    void Start()
    {
        StartLevel(currentLevel);
    }

    public void StartLevel(int levelIndex)
    {
        if (levelIndex >= levels.Count)
        {
            Debug.Log("All levels completed!");
            return;
        }

        currentLevel = levelIndex;
        levelInProgress = true;
        warriorsSpawned = 0;
        warriorsKilled = 0;

        // Calculate total warriors for this level
        totalWarriorsInLevel = 0;
        foreach (var wave in levels[currentLevel].warriorWaves)
        {
            totalWarriorsInLevel += wave.count;
        }

        Debug.Log($"Starting Level {currentLevel + 1} with {totalWarriorsInLevel} warriors");

        StartCoroutine(SpawnLevelWaves());
    }

    private IEnumerator SpawnLevelWaves()
    {
        LevelData currentLevelData = levels[currentLevel];

        foreach (var wave in currentLevelData.warriorWaves)
        {
            // Spawn all warriors of this type
            for (int i = 0; i < wave.count; i++)
            {
                SpawnWarrior(wave.warriorPrefab, wave.damage);
                warriorsSpawned++;
                yield return new WaitForSeconds(currentLevelData.spawnDelay);
            }

            // Wait before next wave
            if (wave != currentLevelData.warriorWaves[currentLevelData.warriorWaves.Count - 1])
            {
                yield return new WaitForSeconds(currentLevelData.waveDelay);
            }
        }

        Debug.Log($"All {totalWarriorsInLevel} warriors spawned for level {currentLevel + 1}");
    }

    private void SpawnWarrior(GameObject prefab, float damage)
    {
        if (prefab != null && spawnPoint != null)
        {
            GameObject warrior = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            WarriorController controller = warrior.GetComponent<WarriorController>();

            if (controller != null)
            {
                controller.SetDamageAmount(damage);
                // Subscribe to warrior death event
                //controller.OnWarriorDeath += OnWarriorKilled;
            }
        }
    }

    public void OnWarriorKilled()
    {
        warriorsKilled++;
        Debug.Log($"Warrior killed! {warriorsKilled}/{totalWarriorsInLevel}");

        // Check if level is complete
        if (warriorsKilled >= totalWarriorsInLevel && levelInProgress)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        levelInProgress = false;
        Debug.Log($"Level {currentLevel + 1} completed!");

        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }
    }

    public void NextLevel()
    {
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }

        currentLevel++;
        StartLevel(currentLevel);
    }

    public void RestartLevel()
    {
        // Destroy all remaining warriors
        WarriorController[] remainingWarriors = FindObjectsOfType<WarriorController>();
        foreach (var warrior in remainingWarriors)
        {
            Destroy(warrior.gameObject);
        }

        StartLevel(currentLevel);
    }

    public void GameOver()
    {
        levelInProgress = false;
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }
}