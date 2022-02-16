using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
  private bool isFirstUpdate = true;

  private void Update()
  {
    if (isFirstUpdate)
    {
      isFirstUpdate = false;
      SceneManager.LoaderCallback();
    }
  }

  private void Start()
  {
    Debug.Log("We have triggered start");
  }
}
