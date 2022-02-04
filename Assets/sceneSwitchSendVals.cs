using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneSwitchSendVals : MonoBehaviour
{
  public void switchToCombat()
  {
    SceneManager.LoadScene("HomePage");
  }
}
