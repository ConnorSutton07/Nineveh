using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    public GameObject levelText; 

    private int countdown = 600;

    void Start()
    {
      levelText.GetComponent<Text>().text = GlobalDataPassing.Instance.GetLevelString();
    }

    // Update is called once per frame
    void Update()
    {
      countdown = countdown - 1;
      if (countdown < 256 && countdown >= 0)
      {
        Color32 temp = levelText.GetComponent<Text>().color;
        levelText.GetComponent<Text>().color = new Color32((byte)(temp[0]),
        (byte)(temp[1]), (byte)(temp[2]), (byte)(countdown));
      }

    }
}
