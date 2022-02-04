using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using TMPro;

public class SwitchToCombat : MonoBehaviour
{
    //public TextMeshPro scoreText;
    
    void Start()
  {
    //numericalScore.GetComponent<UnityEngine.UI.Text>().text = "Score : " + scoreManager.score; 
    Text myText;
    myText = GameObject.Find("Header").GetComponent<Text>();
    myText.text = "Score: " + PlayerPrefs.GetString("score");
  }
    public void switchToCombat()
    {
      SceneManager.LoadScene("Demo");
    }

    
}
