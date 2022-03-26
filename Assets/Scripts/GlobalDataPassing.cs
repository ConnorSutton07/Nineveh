using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataPassing : MonoBehaviour
{

    public static GlobalDataPassing Instance;
    private bool isPlayerDataChanged = false;
    int playerHealth;
    float playerHarmony;
    int playerPosture;
    int currLevel = 1;
    List<int> AliveEnemiesInSections = new List<int>();
    int playerSection = 0;

    public int GetPlayerHealth()
    {
        return playerHealth;
    }

    public float GetPlayerHarmony()
    {
        return playerHarmony;
    }

    public int GetPlayerPosture()
    {
        return playerPosture;
    }

    public int GetCurrentLevel()
    {
        return currLevel;
    }

    public List<int> GetAliveEnemiesInSections()
    {
        return AliveEnemiesInSections;
    }

    public int GetAliveEnemiesInCurrentSection()
    {
        return AliveEnemiesInSections[playerSection];
    }

    public int GetPlayerSection()
    {
        return playerSection;
    }

    public string GetLevelString()
    { 
        switch(currLevel)
        {
          case (1):
            return "Tower Bridge";
          case (2):
            return "The Foundation of Heaven and Earth";
          case (3):
            return "Temple Gardens";
          case (4):
            return "Astrologer's Court";
          case (5):
            return "The Haunt of Communion";
          case (6):
            return "The Overworld";
          default:
            return "ERROR";
    }
    }

    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
    }

    public void SetPlayerHarmony(float harmony)
    {
        playerHarmony = harmony;
    }

    public void SetPlayerPosture(int posture)
    {
        playerPosture = posture;
    }
    
    public void IncreaseLevel()
    {
        currLevel += 1;
    }

    public void AppendAliveEnemiesInSections(int numEnemies)
    {
        AliveEnemiesInSections.Add(numEnemies);
    }

    public void IncrementPlayerSection()
    {
        playerSection++;
    }

    public void DecrementEnemiesInCurrentSection()
    {
        AliveEnemiesInSections[playerSection]--;
    }

    public void SetPlayerData(int health, float harmony, int posture)
    {
        SetPlayerHealth(health);
        SetPlayerHarmony(harmony);
        SetPlayerPosture(posture);
    }

    public bool IsFirstLevel()
    {
        if(!isPlayerDataChanged)
        {
          isPlayerDataChanged = true;
          return true;
        }
        //else return false as its no longer the first level
        return false;
    }

    //control the instance of the class such that it remains constant
    //from call to call
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
