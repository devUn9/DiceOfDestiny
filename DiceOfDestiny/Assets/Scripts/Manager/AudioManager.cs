using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-100)]
public class AudioManager : Singletone<AudioManager>
{
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
    private bool _initialized;

    protected override void Awake()
    {
        base.Awake();
        EnsureInitialized();        
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        var safeBgm = bgmClips ?? Array.Empty<AudioClip>();
        var safeSfx = sfxClips ?? Array.Empty<AudioClip>();
        bgmDict = safeBgm.ToDictionary(c => c.name, c => c);
        sfxDict = safeSfx.ToDictionary(c => c.name, c => c);

        if (bgmSource == null) Debug.LogError("[AudioManager] bgmSource 미할당");
        if (sfxSource == null) Debug.LogError("[AudioManager] sfxSource 미할당");
        if (audioMixer == null) Debug.LogError("[AudioManager] audioMixer 미할당");

        ApplySavedAudioSettings();
        _initialized = true;
    }

    public void SetVolume(string exposedParam, float linearValue)
    {
        float dB = Mathf.Approximately(linearValue, 0f) ? -80f : Mathf.Log10(linearValue) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(exposedParam, linearValue);
    }

    public void SetMasterMute(bool mute)
    {
        if (audioMixer == null)
        {
            if (bgmSource != null) bgmSource.mute = mute;
            if (sfxSource != null) sfxSource.mute = mute;
            return;
        }

        float masterVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 1f)) * 20f;
        float bgmVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("BGMVolume", 1f)) * 20f;
        float sfxVolume = mute ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 1f)) * 20f;

        audioMixer.SetFloat("MasterVolume", masterVolume);
        audioMixer.SetFloat("BGMVolume", bgmVolume);
        audioMixer.SetFloat("SFXVolume", sfxVolume);

        if (bgmSource != null) bgmSource.mute = mute;
        if (sfxSource != null) sfxSource.mute = mute;
    }


    public bool PlayBGM(string name, bool loop = true)
    {
        EnsureInitialized();
        if (!bgmDict.TryGetValue(name, out var clip) || clip == null)
        {
            Debug.LogWarning($"[AudioManager] BGM '{name}'을(를) 찾지 못했습니다.");
            return false;
        }
        if (bgmSource == null)
        {
            Debug.LogError("[AudioManager] bgmSource가 비어 있어 재생할 수 없습니다.");
            return false;
        }
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
        return true;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name)
    {
        EnsureInitialized();
        if (!sfxDict.TryGetValue(name, out var clip) || clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX '{name}'을(를) 찾지 못했습니다.");
            return;
        }
        if (sfxSource == null)
        {
            Debug.LogError("[AudioManager] sfxSource가 비어 있어 재생할 수 없습니다.");
            return;
        }
        sfxSource.PlayOneShot(clip);
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
