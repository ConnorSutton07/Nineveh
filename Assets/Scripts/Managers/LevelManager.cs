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

    [Header ("Thunder")]
    [SerializeField] PerlinShake.Params shakeParams;
    [SerializeField] float freezeDuration;

    private int countdown = 600;
    AudioManager audioManager;


    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        string levelString = GlobalDataPassing.Instance.GetLevelString();
        levelText.GetComponent<Text>().text = levelString;
        int level = GlobalDataPassing.Instance.GetCurrentLevel();

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

    #region Floor-Dependent Methods

    void Floor1()
    {

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
