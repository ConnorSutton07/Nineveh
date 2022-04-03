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
    Floor1,
    Floor2,
    Floor3,
    Floor4,
    Floor5,
    Floor6,
    Loading
  }

  private static Action onLoaderCallback;

  public static void Load(Scene scene)
  {
    //set lodaer callback action to load target scene
    onLoaderCallback = () =>
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString());
    };

    //loads loading screen
    LoadScreenDelay();
  }

    public static void LoadImmediate(Scene scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString());
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