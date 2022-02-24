using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
  public void StartGame()
  {
    Debug.Log("Call StartUI StartGame");
    SceneManager.Load(SceneManager.Scene.Tower);
  }

  public void Exit()
  {
    SceneManager.Exit();
  }

}