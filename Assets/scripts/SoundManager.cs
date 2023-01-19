using UnityEngine;
using UnityEngine.UI;
// =================================
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoSingleton<SoundManager>
{
  [Range(0.0f, 1.0f)]
  public float SoundVolume = 1.0f;
  [Range(0.0f, 1.0f)]
  public float MusicVolume = 1.0f;

  public AudioSource AudioSourceOneShotPrefab;

  [SerializeField]
  public List<AudioClip> MusicTracks = new List<AudioClip>();
  [SerializeField]
  public List<AudioClip> SoundEffects = new List<AudioClip>();

  public Dictionary<string, int> LastPlayedFootstepSoundIndexOfActor = new Dictionary<string, int>();

  Dictionary<string, AudioSource> _audioSourcesByName = new Dictionary<string, AudioSource>();
  public Dictionary<string, AudioSource> AudioSourcesByName
  {
    get { return _audioSourcesByName; }
  }

  AudioSource _musicTrack;
  protected override void Init()
  {
    GameObject go = new GameObject("music-track");
    go.transform.parent = transform;
    _musicTrack = go.AddComponent<AudioSource>();
    _musicTrack.playOnAwake = false;
    _musicTrack.volume = MusicVolume;

    //MakeMusicDatabase();
    MakeSoundsDatabase();
  }

  public void RefreshMediaLists()
  {
    MakeSoundsDatabase();
  }

  void MakeSoundsDatabase()
  {
    foreach (var item in SoundEffects)
    {
      if (item == null)
      {
        Debug.LogWarning("Sound effect didn't load (is null) - rebuild media list in Inspector!");
        continue;
      }

      AudioSource s = (AudioSource)Instantiate(AudioSourceOneShotPrefab);
      s.transform.parent = transform;

      s.clip = item;
      s.volume = SoundVolume;
      s.name = item.name;

      _audioSourcesByName.Add(s.name, s);
    }
  }

  public bool IsSoundPlaying(string name)
  {
    if (_audioSourcesByName.ContainsKey(name))
    {
      return _audioSourcesByName[name].isPlaying;
    }

    return false;
  }

  public void PlaySoundLooped(string name, float volume, float pitch = 1.0f)
  {
    if (_audioSourcesByName.ContainsKey(name))
    {
      _audioSourcesByName[name].volume = volume;

      if (!IsSoundPlaying(name))
      {
        _audioSourcesByName[name].loop = true;
        _audioSourcesByName[name].pitch = pitch;
        _audioSourcesByName[name].Play();
      }
      else
      {
        _audioSourcesByName[name].UnPause();
      }
    }
  }

  public void StopLoopedSound(string name)
  {
    if (_audioSourcesByName.ContainsKey(name))
    {
      // To avoid clicking during sound stoppage,
      // stop sound on the next frame
      _audioSourcesByName[name].volume = 0.0f;
      StartCoroutine(StopSoundRoutine(name));
    }
  }

  IEnumerator StopSoundRoutine(string name)
  {
    yield return null;

    _audioSourcesByName[name].Pause();

    yield return null;
  }

  public void PlaySound(string name, Vector3 position, bool is3D, float pitch = 1.0f)
  {
    if (_audioSourcesByName.ContainsKey(name))
    {
      GameObject go = new GameObject("SFX-3D-" + name);
      go.transform.parent = transform;
      go.transform.position = position;
      AudioSource a = go.AddComponent<AudioSource>();
      a.playOnAwake = false;
      a.spatialBlend = is3D ? 1.0f : 0.0f;
      a.volume = is3D ? SoundVolume : 1.0f;
      a.pitch = pitch;
      a.maxDistance = AudioSourceOneShotPrefab.maxDistance;
      a.minDistance = AudioSourceOneShotPrefab.minDistance;
      a.rolloffMode = AudioRolloffMode.Custom;
      var curve = AudioSourceOneShotPrefab.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
      a.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
      a.clip = _audioSourcesByName[name].clip;
      float length = a.clip.length / pitch + 1.0f;
      a.Play();
      Destroy(go, length);
    }
  }

  public void PlaySound(string name, float volume = 1.0f, float pitch = 1.0f, bool instantiate = true)
  {
    if (_audioSourcesByName.ContainsKey(name))
    {
      if (instantiate)
      {
        GameObject go = new GameObject("SFX-one-shot");
        go.transform.parent = transform;
        AudioSource a = go.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.volume = volume * SoundVolume;
        a.clip = _audioSourcesByName[name].clip;
        a.pitch = pitch;
        float length = a.clip.length + 1.0f;
        a.Play();
        Destroy(go, length);
      }
      else
      {
        _audioSourcesByName[name].pitch = pitch;
        _audioSourcesByName[name].volume = volume * SoundVolume;
        _audioSourcesByName[name].Play();
      }
    }
  }

  string _currentPlayingTrack = string.Empty;
  public AudioSource CurrentMusicTrack
  {
    //get { return _audioSourcesByName[_currentPlayingTrack]; }
    get { return _musicTrack; }
  }

  List<string> _loadingMusicStrings = new List<string>()
  {
    " - Loading...",
    " \\ Loading...",
    " | Loading...",
    " / Loading..."
  };

  public void PlayMusicTrack(string trackName, bool showProgress = false)
  {
    if (!_loading)
    {
      StartCoroutine(LoadMusicRoutine(trackName, showProgress));
    }

    /*
    StopMusic();

    string filename = string.Format("music/{0}", trackName);

    var obj = Resources.Load(filename) as AudioClip;

    _musicTrack.clip = obj;
    _musicTrack.Play();

    _currentPlayingTrack = trackName;

    Resources.UnloadUnusedAssets();
    */

    /*
    if (_audioSourcesByName.ContainsKey(trackName))
    {
      if (_currentPlayingTrack != string.Empty && _audioSourcesByName[_currentPlayingTrack].isPlaying)
      {
        _audioSourcesByName[_currentPlayingTrack].Stop();
        _audioSourcesByName[_currentPlayingTrack].timeSamples = 0;
      }

      _audioSourcesByName[trackName].volume = MusicVolume;
      _audioSourcesByName[trackName].Play();
      _currentPlayingTrack = trackName;
    }
    */
  }

  int _stringsIndex = 0;
  bool _loading = false;
  IEnumerator LoadMusicRoutine(string trackName, bool showProgress)
  {
    _loading = true;
    _stringsIndex = 0;

    string filename = string.Format("music/{0}", trackName);

    var res = Resources.LoadAsync(filename);

    _currentPlayingTrack = string.Empty;

    while (res.progress < 0.9f)
    {
      _stringsIndex++;

      if (_stringsIndex > _loadingMusicStrings.Count - 1)
      {
        _stringsIndex = 0;
      }

      yield return null;
    }

    StopMusic();

    AudioClip clip = res.asset as AudioClip;

    _musicTrack.clip = clip;
    _musicTrack.Play();

    _currentPlayingTrack = trackName;

#if !UNITY_EDITOR
    Resources.UnloadUnusedAssets();
#endif

    _loading = false;

    yield return null;
  }

  public void StopMusic()
  {
    if (_musicTrack != null)
    {
      _musicTrack.Stop();
      _musicTrack.timeSamples = 0;
    }
  }

  public void StopAllSounds()
  {
    foreach (var item in _audioSourcesByName)
    {
      item.Value.Stop();
    }
  }
}
