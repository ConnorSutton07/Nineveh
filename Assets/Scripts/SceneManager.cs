using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneManager : MonoBehaviour
{
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tower");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Load()
    {
        Debug.Log("Load game attempted");
        SaveManager.instance.test(); //access function from singleton class
    }
}

