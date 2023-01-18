using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System;

public class Config : MonoBehaviour
{
  public List<HighscoreEntry> HighScoreEntries;

  string _playerPrefsConfigKey = "fs-cfg";

  string _highscoreConfigPlaceKey = "e-place";
  string _highscoreConfigScoreKey = "e-score";
  string _highscoreConfigTimestampKey = "e-timestamp";

  public JSONNode DataAsJson = new JSONObject();

  List<HighScoreData> _highScores = new List<HighScoreData>();
  public List<HighScoreData> HighScores
  {
    get { return _highScores; }
  }

  string GetTimestamp()
  {    
    string day = DateTime.Now.ToString("dd");
    string month = DateTime.Now.ToString("MM");
    string year = DateTime.Now.ToString("yyyy");
    string tm = DateTime.Now.ToString("HH:mm");
        
    return string.Format("{0}.{1}.{2} ({3})", day, month, year, tm);
  }

  void SaveHighScores()
  {
    for (int i = 0; i < HighScoreEntries.Count; i++)
    {
      string key = string.Format("entry-{0}", i);
      string json = GetJsonForHighscore(_highScores[i]);
      DataAsJson[key] = json;
    }

    WriteConfig();
  }

  public void AddHighscore(int newScore)
  {
    for (int i = 0; i < _highScores.Count; i++)
    {
      if (newScore > _highScores[i].Score)
      {
        HighScoreData d = new HighScoreData();
        
        d.Place     = i + 1;
        d.Timestamp = GetTimestamp();
        d.Score     = newScore;

        _highScores.Insert(i, d);

        SaveHighScores();

        break;
      }
    }
  }

  void SortHighscores()
  {
    _highScores.Sort((e1, e2) => e1.Score.CompareTo(e2.Score));
  }

  string _defaultFillerScore = new string('.', 12);
  string _defaultFillerTimestamp = new string('.', 18);

  void UpdateWindow()
  {
    for (int i = 0; i < HighScoreEntries.Count; i++)
    {
      if (_highScores[i].Score == -1)
      {
        HighScoreEntries[i].Place.text = string.Format((i != 9) ? " {0}" : "{0}", (i + 1));
        HighScoreEntries[i].Timestamp.text = _defaultFillerTimestamp;
        HighScoreEntries[i].Score.text = _defaultFillerScore;
      }
      else
      {
        HighScoreEntries[i].Place.text = 
          (_highScores[i].Place != 10) ?
          string.Format(" {0}", _highScores[i].Place) :
          string.Format("{0}", _highScores[i].Place);

        HighScoreEntries[i].Timestamp.text = _highScores[i].Timestamp;
        HighScoreEntries[i].Score.text = _highScores[i].Score.ToString();
      }
    }
  }

  public void ReadConfig()
  {
    //PlayerPrefs.DeleteAll();

    _highScores.Clear();

    if (PlayerPrefs.HasKey(_playerPrefsConfigKey))
    {
      string config = PlayerPrefs.GetString(_playerPrefsConfigKey);
      DataAsJson = JSON.Parse(config);

#if UNITY_EDITOR
      Debug.Log("Config loaded:\n" + DataAsJson.ToString(2));
#endif

      for (int i = 0; i < HighScoreEntries.Count; i++)
      {
        string entryKey = string.Format("entry-{0}", i);

        JSONNode n = JSON.Parse(DataAsJson[entryKey]);

        HighScoreData d = new HighScoreData();

        d.Timestamp = n[_highscoreConfigTimestampKey];
        d.Place     = (int)n[_highscoreConfigPlaceKey];
        d.Score     = (int)n[_highscoreConfigScoreKey];

        _highScores.Add(d);
      }
    }
    else
    {
      CreateDefaultConfig();
      WriteConfig();
    }

    SortHighscores();

    UpdateWindow();
  }

  string GetJsonForHighscore(HighScoreData d)
  {
    JSONNode json = new JSONObject();
    json[_highscoreConfigPlaceKey] = d.Place.ToString();
    json[_highscoreConfigScoreKey] = d.Score.ToString();
    json[_highscoreConfigTimestampKey] = d.Timestamp;
    return json.ToString();
  }

  void CreateDefaultConfig()
  {
    string entryKey = string.Empty;

    for (int i = 0; i < HighScoreEntries.Count; i++)
    {
      HighScoreData d = new HighScoreData();

      _highScores.Add(d);

      entryKey = string.Format("entry-{0}", i);

      DataAsJson[entryKey] = GetJsonForHighscore(d);
    }
  }

  public void WriteConfig()
  {
#if UNITY_EDITOR
    Debug.Log("Writing config:\n" + DataAsJson.ToString(2));
#endif

    PlayerPrefs.SetString(_playerPrefsConfigKey, DataAsJson.ToString());
  }
}

public class HighScoreData
{
  public string Timestamp = string.Empty;
  public int Score = -1;
  public int Place = -1;

  public override string ToString()
  {
    return string.Format("{0} {1} {2}", Place, Timestamp, Score);
  }
}
