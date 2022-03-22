using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderCallback : MonoBehaviour
{
  [SerializeField]
  public GameObject background;
  public GameObject levelText;
  public GameObject tipCharacter;
  public GameObject tipText;
  public Sprite character1;
  public Sprite character2;

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
    int levelNum = GlobalDataPassing.Instance.GetCurrentLevel();
    //change backround darkness to 
    Color32 temp = background.GetComponent<Image>().color;
    background.GetComponent<Image>().color = new Color32((byte)(temp[0]+(byte)(levelNum * 25)), 
      (byte)(temp[1] + (byte)(levelNum * 25)), (byte)(temp[2] + (byte)(levelNum * 25)), temp[3]);

    //add text of level
    levelText.GetComponent<Text>().text = GlobalDataPassing.Instance.GetLevelString();

    //change tipText
    string tip = GetRandomTip();
    tipText.GetComponent<Text>().text = tip;
    //change tipCharacter
    Sprite character = GetRandomCharacter();
    tipCharacter.GetComponent<Image>().sprite = character;
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
                                      "A timed block considerably reduces the opponent's posture.",
                                      "Harmony decreases overtime.",
                                      "Increasing harmony with Asher gives attack and posture damage buffs.",
                                      "As harmony with Asher increases, the character glow will increase.", 
                                      "Increase harmony by hitting successive hits on an enemy in a short period."};

    int randomEntry = Random.Range(0, strTips.Length);
    return strTips[randomEntry];
  }
  
  private Sprite GetRandomCharacter()
  {
    Sprite[] characters = new Sprite[] { character1, character2 };
    int randomCharacter = Random.Range(0, characters.Length);
    return characters[randomCharacter];
  }

}
