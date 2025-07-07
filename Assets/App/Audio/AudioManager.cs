using System.Collections.Generic;
using App.Config;
using Cysharp.Threading.Tasks;
using GSDev.Singleton;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using App.LocalData;

public class AudioInfo
{
    private readonly AudioConfig _config;
    private AudioClip _clip = null;

    public bool IsReady =>
        _waitTime == 0
        || Time.realtimeSinceStartup - _waitTime >= _config.Interval;

    public float Volume => _config.Volume;
    private float _waitTime = 0;

    public AudioInfo(AudioConfig config)
    {
        _config = config;
    }

    public void Waiting()
    {
        if (_config.Interval == 0) return;
        _waitTime = Time.realtimeSinceStartup;
    }

    public async UniTask<AudioClip> GetClip()
    {
        if (_clip)
            return _clip;
        _clip = await _config.Asset.LoadAsync<AudioClip>();
        return _clip;
    }
}

public class AudioManager : MonoSingleton<AudioManager>
{
    private const int AUDIO_POOL_COUNT = 5;
    private Dictionary<string, AudioInfo> _audioInfos;

    public bool IsMusicOn { get; private set; }
    public bool IsSoundOn { get; private set; }
    private string _sKeyMusic = "";
    private int _curPlayingIndex = -1;
    private readonly IList<AudioSource> _asSounds = new List<AudioSource>();
    private readonly IDictionary<string, AudioSource> _asLoops = new Dictionary<string, AudioSource>();
    private AudioSource _asMusic;

    protected override void Init()
    {
        Initialize();
        gameObject.AddComponent<AudioListener>();
        _asMusic = gameObject.AddComponent<AudioSource>();
        for (var i = 0; i < AUDIO_POOL_COUNT; i++)
        {
            _asSounds.Add(gameObject.AddComponent<AudioSource>());
        }

        var baseSetting = LocalDataManager.Instance.GetBaseSetting();
        IsMusicOn = baseSetting.IsMusicOn;
        IsSoundOn = baseSetting.IsMusicOn;
    }

    private void Initialize()
    {
        var configs = ConfigManager.Instance.GetConfig<AudioConfigTable>().Rows;
        _audioInfos ??= new Dictionary<string, AudioInfo>();
        foreach (var (key, value) in configs)
        {
            var info = new AudioInfo(value);
            if (value.Load) info.GetClip();
            _audioInfos.Add(key, info);
        }
    }
    public void SetMusicOn(bool bOpen)
    {
        if (IsMusicOn == bOpen) return;
        IsMusicOn = bOpen;
        if (!bOpen) PauseMusic();
        else ResumeMusic();
    }
    public void SetSoundOn(bool bOpen)
    {
        if (IsSoundOn == bOpen) return;
        IsSoundOn = bOpen;
    }
    private AudioSource GetLoopAudioSource(string key)
    {
        if (_asLoops.ContainsKey(key))
        {
            return _asLoops[key];
        }
        // var obj = new GameObject(key);
        // obj.transform.SetParent(transform, false);
        var audioSource = gameObject.AddComponent<AudioSource>();
        _asLoops[key] = audioSource;
        return audioSource;
    }
    //////////////////////////////////////////////////////////////////

    private async void Play(AudioSource audioSource, AudioInfo info, bool loop = false, float delay = 0)
    {
        audioSource.clip = await info.GetClip();
        audioSource.loop = loop;
        audioSource.time = 0;
        audioSource.volume = info.Volume;
        if (delay > 0)
            audioSource.PlayDelayed(delay);
        else
            audioSource.Play();
    }
    private async void PlayLoop(AudioSource audioSource, AudioInfo info, int loop = -1, float delay = 0, string sAlias = null)
    {
        if (loop == -1)
        {
            Play(audioSource, info, true, delay);
            return;
        }

        var clip = await info.GetClip();
        
        FuncPlay(loop, delay);
        void FuncPlay(int param_count, float param_delay)
        {
            if (param_count == 0)
            {
                StopSoundLoop(sAlias);
                return;
            }
            Play(audioSource, info, false, param_delay);
            audioSource.DelayCall(clip.length + param_delay, () =>
            {
                FuncPlay(param_count - 1, 0);
            });
        }
    }

    private void PlayMusic()
    {
        if (_audioInfos.ContainsKey(_sKeyMusic) == false)
        {
            // MDebug.LogError($"play music not key:{_sKeyMusic}");
            return;
        }
        if (IsMusicOn == false) return;
        Play(_asMusic, _audioInfos[_sKeyMusic], true);
    }

    #region static

    public static void PlayMusic(string sKey)
    {
        if (Instance._sKeyMusic == sKey) return;
        Instance._sKeyMusic = sKey;
        Instance.PlayMusic();
    }

    public static void PlaySound(string sKey, float delay = 0)
    {
        if (Instance.IsSoundOn == false) return;
        if (!Instance._audioInfos.TryGetValue(sKey, out var info))
        {
            Debug.Log($"play sound not key:{sKey}");
            return;
        }
        if (!info.IsReady) return;

        if (++Instance._curPlayingIndex == AUDIO_POOL_COUNT)
            Instance._curPlayingIndex = 0;
        var audioSource = Instance._asSounds[Instance._curPlayingIndex];
        if (audioSource.isPlaying) { audioSource.Stop(); }

        Instance.Play(audioSource, info, false, delay);
        info.Waiting();
    }
    public static void PlaySoundLoop(string sKey, int loop, float delay = 0, string sAlias = null)
    {
        if (Instance.IsSoundOn == false) return;
        if (Instance._audioInfos.ContainsKey(sKey) == false)
        {
            Debug.Log($"play sound not key:{sKey}");
            return;
        }
        sAlias ??= sKey;
        var audioSource = Instance.GetLoopAudioSource(sAlias);
        Instance.PlayLoop(audioSource, Instance._audioInfos[sKey], loop, delay, sAlias);
    }

    public static void PlaySoundLoop(string sKey, string sAlias = null)
    {
        PlaySoundLoop(sKey, -1, 0, sAlias);
    }

    public static void StopSoundLoop(string sAlias)
    {
        if (Instance._asLoops.ContainsKey(sAlias) == false) return;
        var audioSource = Instance._asLoops[sAlias];
        if (audioSource.isPlaying) { audioSource.Stop(); }

        audioSource.DOKill();
        Destroy(audioSource);
        Instance._asLoops.Remove(sAlias);
    }
    public static void StopSoundAll()
    {
        foreach (var audioSource in Instance._asSounds)
        {
            if (audioSource.isPlaying) { audioSource.Stop(); }
        }
        foreach (var (_, value) in Instance._asLoops)
        {
            if (value.isPlaying) { value.Stop(); }
        }
    }
    public static void StopMusic()
    {
        Instance._sKeyMusic = "";
        if (!Instance._asMusic.isPlaying) return;
        Instance._asMusic.Stop();
    }
    public static void PauseMusic()
    {
        Instance._asMusic.Pause();
        StopMusicVolume();
    }
    public static void ResumeMusic()
    {
        if (!Instance._asMusic.isPlaying) 
            Instance.PlayMusic();
        else
        {
            Instance._asMusic.UnPause();
            ResumeMusicVolume();
        }
    }
    public static void StopMusicVolume()
    {
        Instance._asMusic.volume = 0;
    }
    public static void ResumeMusicVolume()
    {
        if (Instance._audioInfos.ContainsKey(Instance._sKeyMusic))
        {
            var info = Instance._audioInfos[Instance._sKeyMusic];
            Instance._asMusic.volume = info.Volume;
        }
        else Instance._asMusic.volume = 1;
    }

    #endregion
}