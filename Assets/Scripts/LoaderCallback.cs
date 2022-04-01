using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderCallback : MonoBehaviour
{
    [SerializeField] public GameObject background;
    [SerializeField] public GameObject tipCharacter;
    [SerializeField] public GameObject tipText;
    [SerializeField] public GameObject tipCharName;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float loadingDuration;

    private void Start()
    {
        int levelNum = GlobalDataPassing.Instance.GetCurrentLevel();
        /* //change backround darkness to 
        Color32 temp = background.GetComponent<Image>().color;
        background.GetComponent<Image>().color = new Color32((byte)(temp[0] + (byte)(levelNum * 25)),
          (byte)(temp[1] + (byte)(levelNum * 25)), (byte)(temp[2] + (byte)(levelNum * 25)), temp[3]);
        */
        //change tipText
        string tip = GetRandomTip();
        tipText.GetComponent<Text>().text = tip;

        //change tipCharacter
        SetCharacterDisplay();
        StartCoroutine(WaitForTip(Time.time));
    }
        
    IEnumerator WaitForTip(float startTime)
    {
        while (Time.time < startTime + loadingDuration) { yield return null; }
        SceneManager.LoaderCallback();
    }


    private string GetRandomTip()
    {
        string[] strTips = new string[] { "You can deflect arrows by blocking.",
                                          "Dashing makes you invulnerable for its duration.",
                                          "Attacking with high Harmony allows you to steal some life from your enemies.",
                                          "The light of the sun has little influence in the Overworld.",
                                          "Etemenanki is grounded on the breast of the netherworld, while its top vies with the heavens.",
                                          "If your posture is broken, you will be momentarily stunned.",
                                          "Your posture will be broken when the bar beneathy your health fills up.", 
                                          "Breaking your opponents posture leaves them open to a finishing blow.",
                                          "A deflect considerably reduces the opponent's posture.",
                                          "Harmony diminishes over time.",
                                          "Increasing harmony with Ashur grants you more power.",
                                          "As your harmony increases, the red cloud of Ashur's hunger grows.", 
                                          "Increase harmony with succesful attacks and deflects."};

        int randomEntry = Random.Range(0, strTips.Length);
        return strTips[randomEntry];
    }
  
    private void SetCharacterDisplay()
    {
        int randomCharacter = Random.Range(0, sprites.Length); ; //Random.Range(0, sprites.Length);
        if (randomCharacter == 0 || randomCharacter == 5)//jake either as hawaiian shirt or not
        {
          if (System.DateTime.Now.DayOfWeek == System.DayOfWeek.Wednesday) randomCharacter = 5;
          else randomCharacter = 0;
        }
        tipCharacter.GetComponent<Image>().sprite = sprites[randomCharacter];
        tipCharName.GetComponent<Text>().text = sprites[randomCharacter].name;
        // if (sprites[randomCharacter].name == "Dr. Johnson") tipText.GetComponent<Text>().fontSize = 45;
    }
}
