using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CameraShake;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Player player;

    [Header ("UI")]
    [SerializeField] public GameObject levelText;
    [SerializeField] float textFadeRate;

    [Header("Dialogue")]
    [SerializeField] Dialogue dialogue;

    [Header ("Thunder")]
    [SerializeField] PerlinShake.Params shakeParams;
    [SerializeField] float freezeDuration;

    AudioManager audioManager;
    DialogueManager dialogueManager;
    Text text;
    int level;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        text = levelText.GetComponent<Text>();
    }

    void Start()
    {
        string levelString = GlobalDataPassing.Instance.GetLevelString();
        level = GlobalDataPassing.Instance.GetCurrentLevel();
        levelText.GetComponent<Text>().text = levelString;
        StartCoroutine(FadeText(Time.time));
    }

    void LevelEvent()
    {
        switch (level)
        {
            case 1:
                Floor1();
                break;
            case 2:
                StartCoroutine(StartFloor2(Time.time, 1.5f));
                break;
            case 3:
                Floor3();
                break;
            case 4:
                Floor4();
                break;
            case 5:
                Floor5();
                break;
            case 6:
                Overworld();
                break;
            default: break;
        }
    }

    #region Title Text

    IEnumerator FadeText(float startTime)
    {
        while (Time.time < startTime + 2.0f) { yield return null; }
        Color c = text.color;
        for (float alpha = 1f; alpha >= 0; alpha -= textFadeRate)
        {
            c.a = alpha;
            text.color = c;
            yield return new WaitForSeconds(0.1f);
        }
        LevelEvent();
    }

    #endregion

    #region Floor-Dependent Methods

    void Floor1()
    {
        player.DisableUI();
        dialogueManager.StartDialogue(dialogue);
    }

    IEnumerator StartFloor2(float startTime, float delay)
    {
        while (Time.time < startTime + delay) { yield return null; }
        Floor2();
    }

    void Floor2()
    {
        player.Freeze(freezeDuration);
        PlaySound("Thunder");
        PerlinShake();
    }

    void Floor3()
    {

    }

    void Floor4()
    {

    }

    void Floor5()
    {

    }

    void Overworld()
    {
        player.DisableUI();
        GameObject marduk = GameObject.Find("Marduk");
        marduk.SetActive(false);
    }

    #endregion

    #region Screen Shake 

    public void PerlinShake()
    {
        Vector3 sourcePosition = transform.position;

        // Creating new instance of a shake and registering it in the system.
        CameraShaker.Shake(new PerlinShake(shakeParams, sourcePosition: sourcePosition));
    }

    #endregion

    #region Audio

    public void PlaySound(string text)
    {
        audioManager.PlaySound(text);
    }

    #endregion
}
