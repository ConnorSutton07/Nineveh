using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
  public void StartGame()
  {
        //load first level of game
        //SceneManager.Load(SceneManager.Scene.Floor1);
        SceneManager.LoadImmediate(SceneManager.Scene.Floor1);
  }

  public void Exit()
  {
    SceneManager.Exit();
  }

}