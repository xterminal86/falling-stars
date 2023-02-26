using System;
using System.Collections;
using UnityEngine;

public class ImageBubble : MonoBehaviour
{
  public SpriteRenderer Heart;

  Color _color = Color.white;

  IEnumerator FadeRoutine()
  {
    while (_color.a > 0.0f)
    {
      Vector3 p = transform.position;

      p.y += Time.unscaledDeltaTime;

      transform.position = p;

      _color.a -= Time.unscaledDeltaTime * 0.85f;

      Heart.color = _color;

      yield return null;
    }

    Destroy(gameObject);

    yield return null;
  }

  public void Init()
  {
    _color = Color.white;
    Heart.color = _color;

    StartCoroutine(FadeRoutine());
  }
}
