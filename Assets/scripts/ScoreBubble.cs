using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;
using TMPro;
using static Constants;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreBubble : MonoBehaviour
{
  public TMP_Text ScoreText;

  Color _textColor = Color.white;

  IEnumerator FadeRoutine()
  {  
    while (_textColor.a > 0.0f)
    {
      Vector3 p = transform.position;

      p.y += Time.smoothDeltaTime;

      transform.position = p;

      _textColor.a -= Time.smoothDeltaTime * 0.85f;

      ScoreText.color = _textColor;

      yield return null;
    }

    Destroy(gameObject);

    yield return null;
  }
    
  public void Init(Constants.StarType starType, int score)
  {
    ScoreText.text = string.Format("+{0}", score);

    _textColor = Constants.StarColorsByType[starType];
    ScoreText.color = _textColor;

    StartCoroutine(FadeRoutine());
  }
}
