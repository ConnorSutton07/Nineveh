using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataPassing : MonoBehaviour
{

    public static GlobalDataPassing Instance;
    private bool isPlayerDataChanged = false;
    int playerHealth = 80;
    int playerHarmony = 40;
    int playerPosture = 40;
    
    public int GetPlayerHealth()
    {
        return playerHealth;
    }

    public int GetPlayerHarmony()
    {
      return playerHarmony;
    }

    public int GetPlayerPosture()
    {
      return playerPosture;
    }

    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
    }

    public void SetPlayerHarmony(int harmony)
    {
        playerHarmony = harmony;
    }

    public void SetPlayerPosture(int posture)
    {
        playerPosture = posture;
    }


    public void SetPlayerData(int health, int harmony, int posture)
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
