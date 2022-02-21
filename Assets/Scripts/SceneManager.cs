using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManager
{
  public enum Scene
  {
    Start,
    Arena,
    Tower,
    Loading
  }

  private static Action onLoaderCallback;

  public static void Load(Scene scene)
  {
    Debug.Log("call SceneManager Load");
    //set lodaer callback action to load target scene
    onLoaderCallback = () =>
    {
      Debug.Log("Should go to called scene");
      UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString());
    };

    //loads loading screen

    LoadScreenDelay();
  }

  private static void LoadScreenDelay()
  {
    //generated so that invoke can be called to delay time 
    UnityEngine.SceneManagement.SceneManager.LoadScene(Scene.Loading.ToString());
  }

  public static void LoaderCallback()
  {
    //necessary for delay of loading screen lad
    if (onLoaderCallback != null)
    {
      onLoaderCallback();
      onLoaderCallback = null;
    }
  }

  public static void Exit()
  {
    Application.Quit();
  }

}