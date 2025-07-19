using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class WarriorData
{
    [Header("Warrior Identity")]
    public string warriorName;
    public WarriorType type;

    [Header("Visual")]
    public GameObject prefab;
    public Sprite warriorSprite;

    [Header("Stats")]
    public float health = 100f;
    public float damage = 10f;
    public float baseSpeed = 1f;
    public float attackRate = 1f;

    [Header("Mouse Interaction")]
    public float killHeightPercentage = 0.6f;
    public float dragResistance = 1f; // Higher = harder to drag
}

[System.Serializable]
public enum WarriorType
{
    Basic,      // No clothes
    Soldier,    // Basic clothes  
    Armored     // Advanced with armor
}

[System.Serializable]
public class LevelConfiguration
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;

    [Header("Warrior Spawn Configuration")]
    public List<WarriorSpawnData> warriorSpawns = new List<WarriorSpawnData>();

    [Header("Level Modifiers")]
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;

    [Header("Spawn Settings")]
    public float minSpawnDelay = 0f;
    public float maxSpawnDelay = 2f;
    public Vector2 spawnYRange = new Vector2(-13f, 0f);
    public float spawnXPosition = -27f;
}

[System.Serializable]
public class WarriorSpawnData
{
    public WarriorType warriorType;
    public int count;
}

[CreateAssetMenu(fileName = "New Warrior Data", menuName = "Castle Defense/Warrior Data")]
public class WarriorDataSO : ScriptableObject
{
    [Header("All Warrior Types")]
    public List<WarriorData> allWarriors = new List<WarriorData>();

    [Header("Level Configurations")]
    public List<LevelConfiguration> levels = new List<LevelConfiguration>();

    // Dictionary for quick lookup
    private Dictionary<WarriorType, WarriorData> warriorDict;

    void OnEnable()
    {
        BuildWarriorDictionary();
    }

    void BuildWarriorDictionary()
    {
        warriorDict = new Dictionary<WarriorType, WarriorData>();
        foreach (var warrior in allWarriors)
        {
            if (!warriorDict.ContainsKey(warrior.type))
            {
                warriorDict.Add(warrior.type, warrior);
            }
        }
    }

    public WarriorData GetWarriorData(WarriorType type)
    {
        if (warriorDict == null)
            BuildWarriorDictionary();

        return warriorDict.ContainsKey(type) ? warriorDict[type] : null;
    }

    public LevelConfiguration GetLevelConfiguration(int levelNumber)
    {
        return levels.Find(level => level.levelNumber == levelNumber);
    }
}