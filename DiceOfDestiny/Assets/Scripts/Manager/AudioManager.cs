using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class AudioManager : Singletone<AudioManager>
{
    // public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    protected override void Awake()
    {
        base.Awake();
        if (bgmSource == null || sfxSource == null || audioMixer == null)
        {
            Debug.LogError("AudioManager is missing required components. Please assign them in the inspector.");
            return;
        }

        bgmDict = bgmClips.ToDictionary(c => c.name, c => c);
        sfxDict = sfxClips.ToDictionary(c => c.name, c => c);

        ApplySavedAudioSettings();
    }

    public void SetVolume(string exposedParam, float linearValue)
    {
        float dB = Mathf.Approximately(linearValue, 0f) ? -80f : Mathf.Log10(linearValue) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(exposedParam, linearValue);
    }

    public void SetMasterMute(bool mute)
    {
        float masterVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 1f)) * 20f;
        float bgmVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("BGMVolume", 1f)) * 20f;
        float sfxVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 1f)) * 20f;

        audioMixer.SetFloat("MasterVolume", masterVolume);
        audioMixer.SetFloat("BGMVolume", bgmVolume);
        audioMixer.SetFloat("SFXVolume", sfxVolume);

        if (bgmSource != null)
            bgmSource.mute = mute;
        if (sfxSource != null)
            sfxSource.mute = mute;
    }


public void PlayBGM(string name, bool loop = true)
    {
        if (bgmDict.TryGetValue(name, out var clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{name}' not found.");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out var clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' not found.");
        }
    }


    public void ApplySavedAudioSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        
        SetVolume("MasterVolume", master);
        SetVolume("BGMVolume", bgm);
        SetVolume("SFXVolume", sfx);
        SetMasterMute(isMuted);
    }

}
