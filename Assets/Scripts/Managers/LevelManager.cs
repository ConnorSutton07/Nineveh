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

        switch (levelString)
        {
            case "Tower Bridge":
                StartCoroutine(StartFloor1(Time.time, 3f));
                break;
            default: return;
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
        player.Freeze(freezeDuration);
        PlaySound("Thunder");
        PerlinShake();
    }

    IEnumerator StartFloor1(float startTime, float delay)
    {
        while (Time.time < startTime + delay) { yield return null; }
        Floor1();
    }

    void Floor2()
    {

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