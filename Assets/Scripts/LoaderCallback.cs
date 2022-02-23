using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
  private bool isDoneUpdating = false;
  private int counter = 0;
  private int updates_to_wait = 500;

  private void Update()
  {
    counter += 1;
    if (counter > updates_to_wait)
    {
      isDoneUpdating = true;
      //its possible the init values need to be reset at this point
    }
    if (isDoneUpdating)
    {
      SceneManager.LoaderCallback();
    }
  }

  private void Start()
  {
  }

}
