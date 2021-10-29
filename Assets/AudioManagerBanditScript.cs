using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerBanditScript : MonoBehaviour
{
    public static AudioClip runningSound;
    static AudioSource audioSrc;
    // Start is called before the first frame update
    void Start()
    {
        runningSound = Audio.Load<AudioClip>("running");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string name)
    {
        switch (name)
        {
            case "running":
                audioSrc.PlayOneShot(runningSound);
                break;
        }
    }
}
