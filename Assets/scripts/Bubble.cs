using System.Collections;

using UnityEngine;
using TMPro;

public class Bubble : MonoBehaviour, IPoolObject
{
  Main _mainRef;

  TMP_Text _text;
  SpriteRenderer _image;

  enum BubbleType
  {
    TEXT = 0,
    IMAGE
  }

  BubbleType _type = BubbleType.TEXT;

  void Awake()
  {
    _text = GetComponentInChildren<TMP_Text>(true);
    if (_text == null)
    {
      Debug.LogError("TMP_Text not found!");
    }

    _image = GetComponentInChildren<SpriteRenderer>(true);
    if (_image == null)
    {
      Debug.LogError("Image subobject not found!");
    }

    var mainRef = GameObject.FindGameObjectWithTag("MainScript");
    if (mainRef != null)
    {
      _mainRef = mainRef.GetComponent<Main>();
    }
    else
    {
      Debug.LogError("MainScript not found!");
    }
  }

  // ===========================================================================

  public void SetText(string text, Color c)
  {
    _type = BubbleType.TEXT;

    _text.text = text;
    _color = c;

    _text.gameObject.SetActive(true);

    StartCoroutine(FadeRoutine());
  }

  // ===========================================================================

  public void SetImage(Sprite sprite, Color c)
  {
    _type = BubbleType.IMAGE;

    _image.sprite = sprite;
    _color = c;

    _image.gameObject.SetActive(true);

    StartCoroutine(FadeRoutine());
  }

  // ===========================================================================

  Color _color = Color.white;
  IEnumerator FadeRoutine()
  {
    while (_color.a > 0.0f)
    {
      Vector3 p = transform.position;

      p.y += Time.unscaledDeltaTime;

      transform.position = p;

      _color.a -= Time.unscaledDeltaTime * 0.85f;

      switch (_type)
      {
        case BubbleType.TEXT:
          _text.color = _color;
          break;

        case BubbleType.IMAGE:
          _image.color = _color;
          break;

        default:
          break;
      }

      yield return null;
    }

    _mainRef.BubblesPool.Return(gameObject);

    yield return null;
  }

  // ===========================================================================

  public void Prepare()
  {
  }

  public void ResetState()
  {
    _color = Color.white;
    _text.gameObject.SetActive(false);
    _image.gameObject.SetActive(false);
  }
}
