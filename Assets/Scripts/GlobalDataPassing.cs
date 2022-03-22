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

    public string GetLevelString()
    { 
        switch(currLevel)
        {
          case (1):
            return "1: The Scourge of Babylon";
          case (2):
            return "2: Sennacherib's Ghost";
          case (3):
            return "3: Herodotus' Resting Place";
          case (4):
            return "4: Nimrod''s Appeal";
          case (5):
            return "5: The Folly of Antiochus";
          case (6):
            return "6: Alexander's Plight";
          case (7):
            return "Marduk's Revenge";
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
