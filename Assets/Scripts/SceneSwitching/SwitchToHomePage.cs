using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwitchToHomePage : MonoBehaviour
{
  private string entry;
  private bool changedEntry = false;
  public InputField myIF;

  public void Start()
  {

    myIF = GameObject.Find("InputField").GetComponent<InputField>();
    //add listener to invoke "" method when player types in input field
    myIF.onEndEdit.AddListener(delegate { setTextOnEdit(myIF.text); });
    
  }
  public void switchToHomePage()
  {
    //private int entry = transform.getComponent
    //can not assign a private variable within function?//
    if (!changedEntry)
    {
      entry = "1000";
    }
    PlayerPrefs.SetString("score" , entry);
    SceneManager.LoadScene("HomePage");
  }
  
  public void setTextOnEdit(string str)
  {
    //Debug.Log("reached setTextOnEdit");
    Debug.Log("Entered SetTextOnEdit " + str);
    entry = str;
    changedEntry = true;
    //Debug.Log(entry);
  }
}
