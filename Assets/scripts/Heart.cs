using System.Collections;
// =====================
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
  public RectTransform RectTransform_;
  public Image Image_;

  IEnumerator FadeOutRoutine()
  {
    Color c = Image_.color;

    while (c.a > 0.0f)
    {
      Vector2 pos = RectTransform_.anchoredPosition;
      pos.y -= (Time.unscaledDeltaTime * 75.0f);

      RectTransform_.anchoredPosition = pos;

      c = Image_.color;

      c.a -= (Time.unscaledDeltaTime * 2.0f);

      Image_.color = c;

      yield return null;
    }

    _fadingOut = false;

    yield return null;
  }

  IEnumerator FadeInRoutine()
  {
    Vector2 pos = RectTransform_.anchoredPosition;
    pos.y = 0.0f;
    RectTransform_.anchoredPosition = pos;

    Color c = Image_.color;

    while (c.a < 1.0f)
    {
      c.a += (Time.unscaledDeltaTime * 4.0f);

      Image_.color = c;

      yield return null;
    }

    c.a = 1.0f;

    Image_.color = c;

    _fadingIn = false;

    yield return null;
  }

  bool _fadingIn = false;
  Coroutine _fadeInCoro;
  public void FadeIn()
  {
    if (!_fadingIn)
    {
      if (_fadingOut)
      {
        StopCoroutine(_fadeOutCoro);
        _fadingOut = false;
      }

      _fadingIn = true;
      IEnumerator coro = FadeInRoutine();
      _fadeInCoro = StartCoroutine(coro);
    }
  }

  Coroutine _fadeOutCoro;

  bool _fadingOut = false;
  public void FadeOut()
  {
    if (!_fadingOut)
    {
      if (_fadingIn)
      {
        StopCoroutine(_fadeInCoro);
        _fadingIn = false;
      }

      _fadingOut = true;
      IEnumerator coro = FadeOutRoutine();
      _fadeOutCoro = StartCoroutine(coro);
    }
  }
}
