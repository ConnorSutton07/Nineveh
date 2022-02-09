using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

public class SaveManager : MonoBehaviour
{
  public static SaveManager instance { get; private set; }
 
    void Awake()
    {
      instance = this; /*singleton class. Can be called from any script*/
      Debug.Log("ran awake and set instance");
    }
 

    public void test()
    {
      Debug.Log("reached SaveManager");
    }
}
