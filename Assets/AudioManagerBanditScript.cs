using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound2
{
    public string m_name;
    public AudioClip[] m_clips;

    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(0f, 1.5f)]
    public float pitch = 1.0f;
    public Vector2 m_VolumeRange = new Vector2(1.0f, 1.0f);
    public Vector2 m_PitchRange = new Vector2(1.0f, 1.0f);

    private AudioSource m_source;

    public void SetSource(AudioSource source)
    {
        m_source = source;
        int randomClip = Random.Range(0, m_clips.Length - 1);
        m_source.clip = m_clips[randomClip];
    }

    public void Play()
    {
        if (m_clips.Length > 1)
        {
            int randomClip = Random.Range(0, m_clips.Length);
            m_source.clip = m_clips[randomClip];
        }
        //m_source.volume = volume * Random.Range(m_VolumeRange.x, m_VolumeRange.y);
        //m_source.pitch = pitch * Random.Range(m_PitchRange.x, m_PitchRange.y);
        m_source.Play();
    }
}

public class AudioManagerBanditScript : MonoBehaviour
{
    // Make it a singleton class that can be accessible everywhere
    public static AudioManagerBanditScript instance;

    [SerializeField]
    Sound2[] m_sounds;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one AudioManger in scene");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        for (int i = 0; i < m_sounds.Length; i++)
        {
            GameObject go = new GameObject("Sound_" + i + "_" + m_sounds[i].m_name);
            go.transform.SetParent(transform);
            m_sounds[i].SetSource(go.AddComponent<AudioSource>());
        }
    }

    public void PlaySound(string name)
    {
        for (int i = 0; i < m_sounds.Length; i++)
        {
            if (m_sounds[i].m_name == name)
            {
                m_sounds[i].Play();
                return;
            }
        }

        Debug.LogWarning("AudioManager: Sound name not found in list: " + name);
    }
}
