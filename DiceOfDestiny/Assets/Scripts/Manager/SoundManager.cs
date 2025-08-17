using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool loop = false;
    public float volume = 1f;
    public bool is3D = false;
}

public class SoundManager : Singletone<SoundManager>
{
    [Header("사운드 리스트")]
    public List<Sound> skillSounds;
    public List<Sound> gameSounds;
    

    private Dictionary<string, Sound> soundDict = new();
    private Dictionary<string, AudioSource> loopedSources = new();
    private List<AudioSource> sfxPool = new();
    private AudioSource bgmSource;

    [Header("풀링 옵션")]
    public int poolSize = 10;

    private void Start()
    {
        RegisterSounds(skillSounds);
        RegisterSounds(gameSounds);
      
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        for (int i = 0; i < poolSize; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }

    private void RegisterSounds(List<Sound> list)
    {
        foreach (var sound in list)
        {
            if (!soundDict.ContainsKey(sound.name))
                soundDict[sound.name] = sound;
            else
                Debug.LogWarning($"중복된 사운드 이름: {sound.name}");
        }
    }

    public void Play(string name)
    {
        if (!soundDict.TryGetValue(name, out var sound)) return;

        if (skillSounds.Contains(sound))
        {
            PlayBGM(sound);
            return;
        }

        var src = GetAvailableAudioSource();
        Configure(src, sound);
        src.loop = false;
        src.Play();
    }

    public void PlayLoop(string name)
    {
        if (loopedSources.ContainsKey(name)) return;
        if (!soundDict.TryGetValue(name, out var sound)) return;

        var src = GetAvailableAudioSource();
        Configure(src, sound);
        src.loop = true;
        src.Play();
        loopedSources[name] = src;
    }

    public void Stop(string name)
    {
        if (loopedSources.TryGetValue(name, out var src))
        {
            src.Stop();
            loopedSources.Remove(name);
        }

        if (bgmSource.isPlaying && bgmSource.clip.name == name)
        {
            bgmSource.Stop();
        }
    }


    private void PlayBGM(Sound sound)
    {
        bgmSource.clip = sound.clip;
        bgmSource.volume = sound.volume;
        bgmSource.loop = sound.loop;
        bgmSource.spatialBlend = 0f;
        bgmSource.Play();
    }

    private void Configure(AudioSource src, Sound sound)
    {
        src.clip = sound.clip;
        src.volume = sound.volume;
        src.spatialBlend = sound.is3D ? 1f : 0f;
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var src in sfxPool)
        {
            if (!src.isPlaying)
                return src;
        }

        var newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.playOnAwake = false;
        sfxPool.Add(newSrc);
        return newSrc;
    }
}
