using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData{
    public int health;
    public int posture;
    public int level;

    public PlayerData (Player player)
    {
        health = player.currentHealth;
        posture = player.currentPosture;
    }
}
