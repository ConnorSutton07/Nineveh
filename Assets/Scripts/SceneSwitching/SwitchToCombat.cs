using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchToCombat : MonoBehaviour
{
    public void switchToCombat()
    {
      SceneManager.LoadScene("Demo");
    }
}
