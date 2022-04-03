using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CameraShake;
using System;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Player player;

    [Header("UI")]
    [SerializeField] public GameObject levelText;
    [SerializeField] float textFadeRate;

    [Header("Dialogue")]
    [SerializeField] Dialogue dialogue;

    [Header("Thunder")]

    [SerializeField] PerlinShake.Params shakeParams;
    [SerializeField] float thunderDelay;
    [SerializeField] float freezeDuration;

    Text dialogueText;
    AudioManager audioManager;
    DialogueManager dialogueManager;
    PlayerInput GameplayControls;
    PlayerInput UIControls;
    int currentDialogue;

    string[] overworldDialogue =
    {
        "Observe me, as I beckon the stars...",
        "Am I not what you seek?",
        "You return, Ashur, in vulgar, pathetic form.",
        "This realm will not show itself to you. You are forbidden.",
        "Be gone.",
        "",
        "Do not be afraid.",
        "For thou hast entered mine realm...",
        "The Overworld, the Heavens...",
        "And I shall grace thee with guidance.",
        "Like a star, thy shall sing...",
        ""
    };

    [SerializeField] float[] dialogueDelays;

    Text text;
    int level;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        text = levelText.GetComponent<Text>();
        GameplayControls = player.GetComponent<PlayerInput>();
        UIControls = dialogueManager.GetComponent<PlayerInput>();
        InitializeInputManagers();
    }

    void Start()
    {
        string levelString = GlobalDataPassing.Instance.GetLevelString();
        level = GlobalDataPassing.Instance.GetCurrentLevel();
        levelText.GetComponent<Text>().text = levelString;
        player.DisableUI();
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
                Floor2();
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

    IEnumerator DelayCallable(float startTime, float delay, Action callable)
    {
        while (Time.time < startTime + delay) { yield return null; }
        callable();
    }

    void StartDialogue()
    {
        if (!dialogueManager.Used()) { player.EnableUI(); return; }
        SwapInputManagers();
        dialogueManager.StartDialogue(dialogue);
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
        text.enabled = false;
        LevelEvent();
    }

    #endregion

    #region Floor-Dependent Methods

    void Floor1()
    {
        StartDialogue();
    }

    void Floor2Events()
    {
        player.Freeze(freezeDuration);
        PlaySound("Thunder");
        PerlinShake();
        StartCoroutine(DelayCallable(Time.time, freezeDuration, StartDialogue));
    }

    void Floor2()
    {
        StartCoroutine(DelayCallable(Time.time, thunderDelay, Floor2Events));
    }

    void Floor3()
    {
        StartDialogue();
    }

    void Floor4()
    {
        player.Freeze(freezeDuration);
        PlaySound("Big Thunder");
        PerlinShake();
        StartCoroutine(DelayCallable(Time.time, 1.5f, SmallThunder));
        StartCoroutine(DelayCallable(Time.time, freezeDuration, StartDialogue));
    }

    void SmallThunder()
    {
        PlaySound("Thunder");
    }

    void Floor5()
    {
        StartDialogue();
    }

    void Pass() { return; }

    void Overworld()
    {
        player.PermaFreeze();
        currentDialogue = 0;
        dialogueText = GameObject.Find("TextCanvas").transform.Find("Text").GetComponent<Text>();
        player.DisableUI();
        GameObject marduk = GameObject.Find("Marduk");
        Action BeginSequence = GameObject.Find("Overworld Sky").GetComponent<Stars>().BeginSequence;
        StartCoroutine(ChangeText(Time.time, Pass));
        StartCoroutine(DelayCallable(Time.time, 4f, BeginSequence));
    }

    IEnumerator ChangeText(float startTime, Action callable)
    {
        while (Time.time < startTime + dialogueDelays[currentDialogue]) { yield return null; }
        dialogueText.text = overworldDialogue[currentDialogue];
        callable();
        Action action = Pass;
        currentDialogue++;
        if (currentDialogue == 4) action = GameObject.Find("Marduk").GetComponent<Marduk>().Reject;
        else if (currentDialogue == 5) action = GameObject.Find("Marduk").GetComponent<Marduk>().Relocate;
        else if (currentDialogue == 10) action = player.ActivateLight;
        if (currentDialogue < overworldDialogue.Length)
        {
            StartCoroutine(ChangeText(Time.time, action));
            yield break;
        }
        else
        {
            dialogueText.text = "";
            player.Unfreeze();
            player.EnableUI();
            GameObject.Find("Marduk").GetComponent<Marduk>().Unfreeze();
            yield break;
        }
    }

    IEnumerator ActivateMarduk(float startTime, float delay, GameObject marduk)
    {
        while (Time.time < startTime + delay) { yield return null; }
        marduk.GetComponent<Marduk>().Unfreeze();
    }

    #endregion

    #region Screen Shake 

    public void PerlinShake()
    {
        Vector3 sourcePosition = transform.position;
        CameraShaker.Shake(new PerlinShake(shakeParams, sourcePosition: sourcePosition));
    }

    #endregion

    #region Audio

    public void PlaySound(string text)
    {
        audioManager.PlaySound(text);
    }

    #endregion

    #region Input Management

    void InitializeInputManagers()
    {
        GameplayControls.enabled = true;
        UIControls.enabled = false;
    }

    public void SwapInputManagers()
    {
        if (GameplayControls.enabled) { player.OnStopMoving(); }
        GameplayControls.enabled = !GameplayControls.enabled;
        UIControls.enabled = !UIControls.enabled;
    }

    #endregion
}
