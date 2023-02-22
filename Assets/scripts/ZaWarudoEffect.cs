using System.Collections;
using UnityEngine;

public class ZaWarudoEffect : MonoBehaviour
{
  public SpriteRenderer Clouds;

  Color _color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

  IEnumerator ShowCloudsRoutine()
  {
    float t = 0.0f;

    float value = 0.0f;
    while (value < 1.0f)
    {
      value = Mathf.Lerp(0.0f, 1.0f, t);

      _color.a = Constants.CloudsAlpha * value;
      Clouds.color = _color;

      t += Time.unscaledDeltaTime;

      yield return null;
    }

    _color.a = Constants.CloudsAlpha;
    Clouds.color = _color;

    yield return null;
  }

  public void ShowClouds()
  {
    StartCoroutine(ShowCloudsRoutine());
  }
}
