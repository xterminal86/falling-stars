using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerInspector : Editor
{
  string _musicPath = "Assets/music";
  string _soundsPath = "Assets/sound";

  SoundManager _sm;

  public override void OnInspectorGUI()
  {
    _sm = target as SoundManager;

    if (_sm == null) return;

    _sm.AudioSourceOneShotPrefab = (AudioSource)EditorGUILayout.ObjectField("Audio Source One Shot Prefab", _sm.AudioSourceOneShotPrefab, typeof(AudioSource), true);

    _sm.SoundVolume = EditorGUILayout.Slider("Sound Volume", _sm.SoundVolume, 0.0f, 1.0f);
    _sm.MusicVolume = EditorGUILayout.Slider("Music Volume", _sm.MusicVolume, 0.0f, 1.0f);    

    if (GUILayout.Button("Generate Music List"))
    {
      BuildMediaList(_sm.MusicTracks, _musicPath, "*.wav");
    }

    if (GUILayout.Button("Clear Music List"))
    {
      _sm.MusicTracks.Clear();
    }

    PrintListContents(_sm.MusicTracks);

    if (GUILayout.Button("Generate Sounds List"))
    {
      BuildMediaList(_sm.SoundEffects, _soundsPath, "*.wav");
    }

    if (GUILayout.Button("Clear Sounds List"))
    {
      _sm.SoundEffects.Clear();
    }

    PrintListContents(_sm.SoundEffects);

    if (GUI.changed)
    {
      EditorUtility.SetDirty(_sm);
      AssetDatabase.SaveAssets();
    }
  }

  void BuildMediaList(List<AudioClip> listToPopulate, string pathToDirWithFiles, string extension)
  {
    listToPopulate.Clear();

    string[] dirs = Directory.GetDirectories(pathToDirWithFiles, "*", SearchOption.AllDirectories);
    if (dirs.Length == 0)
    {
      LoadFiles(listToPopulate, pathToDirWithFiles, extension);
    }
    else
    {
      for (int i = 0; i < dirs.Length; i++)
      {
        string dirSlashesFixed = dirs[i].Replace("\\", "/");

        LoadFiles(listToPopulate, dirSlashesFixed, extension);
      }
    }
  }

  void PrintListContents(List<AudioClip> listToPrint)
  {
    if (listToPrint.Count != 0)
    {
      string text = string.Empty;

      int counter = 0;
      foreach (var item in listToPrint)
      {
        if (item != null)
        {
          text += string.Format("{0}: {1}\n", counter, item.name);
          counter++;
        }
      }

      EditorGUILayout.HelpBox(text, MessageType.None);
    }
  }

  void LoadFiles(List<AudioClip> listToAdd, string path, string filter)
  {
    string[] files = Directory.GetFiles(path, filter);
    for (int j = 0; j < files.Length; j++)
    {      
      string fileSlashFixed = files[j].Replace("\\", "/");

      AudioClip clip = AssetDatabase.LoadAssetAtPath(fileSlashFixed, typeof(AudioClip)) as AudioClip;
      listToAdd.Add(clip);
    }
  }
}
