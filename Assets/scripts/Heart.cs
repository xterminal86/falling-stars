using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
  public RectTransform RectTransform_;
  public Image Image_;

  IEnumerator FadeRoutine()
  {
    Color c = Image_.color;

    while (c.a > 0.0f)
    {
      Vector2 pos = RectTransform_.anchoredPosition;
      pos.y -= (Time.smoothDeltaTime * 75.0f);

      RectTransform_.anchoredPosition = pos;

      c = Image_.color;

      c.a -= (Time.smoothDeltaTime * 2.0f);

      Image_.color = c;

      yield return null;
    }

    yield return null;
  }

  bool _isRunning = false;
  public void FadeAway()
  {
    if (!_isRunning)
    {
      StartCoroutine(FadeRoutine());
      _isRunning = true;
    }
  }
}
