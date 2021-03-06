using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataPassing : MonoBehaviour
{
    public static GlobalDataPassing Instance;
    private bool isPlayerDataChanged = false;
    int playerHealth;
    float playerHarmony;
    float playerPosture;
    int curLevel = 1;
    List<int> AliveEnemiesInSections;
    int playerSection;

    public void ResetSections()
    {
        AliveEnemiesInSections = new List<int>();
        playerSection = 0;
    }

    public int GetPlayerHealth()
    {
        return playerHealth;
    }

    public float GetPlayerHarmony()
    {
        if (curLevel == 6) return 0;
        return playerHarmony;
    }

    public float GetPlayerPosture()
    {
        return playerPosture;
    }

    public int GetCurrentLevel()
    {
        return curLevel;
    }

    public List<int> GetAliveEnemiesInSections()
    {
        return AliveEnemiesInSections;
    }

    /*
    public int GetAliveEnemiesInCurrentSection()
    {
        return playerSection <= AliveEnemiesInSections.Count && AliveEnemiesInSections[playerSection];
    }
    */

    public bool EnemiesCleared()
    {
        return playerSection < AliveEnemiesInSections.Count && AliveEnemiesInSections[playerSection] == 0;
    }

    public int GetPlayerSection()
    {
        return playerSection;
    }

    public string GetLevelString()
    {
        if (curLevel < 7 && curLevel >= 1) return Constants.Floors[curLevel - 1];
        return "Error";
    }

    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
    }

    public void SetPlayerHarmony(float harmony)
    {
        playerHarmony = harmony;
    }

    public void SetPlayerPosture(float posture)
    {
        playerPosture = posture;
    }
    
    public void SetCurrentLevel(int levelNum)
    {
        curLevel = levelNum;
    }

    public void IncreaseLevel()
    {
        curLevel += 1;
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

    public void SetPlayerData(int health, float harmony, float posture)
    {
        SetPlayerHealth(health);
        SetPlayerHarmony(harmony);
        SetPlayerPosture(posture);
    }

    public bool IsFirstLevel()
    {
        if(!isPlayerDataChanged && !(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Loading"))
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
            curLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            if (curLevel == 0 || curLevel > 6) curLevel = 1;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
