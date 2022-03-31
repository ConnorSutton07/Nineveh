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

    AudioManager audioManager;
    DialogueManager dialogueManager;
    PlayerInput GameplayControls;
    PlayerInput UIControls;


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
