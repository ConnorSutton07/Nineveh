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
    //StartCoroutine(delay()); //does not delay in the right spot and
    //caused bug
    Debug.Log("We have triggered start");
  }

  IEnumerator delay()
  {
    Debug.Log("waiting...");
    yield return new WaitForSeconds(2);
    Debug.Log("Done");
  }
}
