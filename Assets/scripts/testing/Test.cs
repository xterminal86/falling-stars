using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Test : MonoBehaviour
{
  public enum TrajectoryType
  {
    LINE = 0,
    CIRCLE,
    WAVE
  }

  public TrajectoryType TrajectoryType_;

  public TMP_Text Text;
  public TMP_Text Value;
  public TMP_Text GeneralDir;

  public Transform Holder;
  public Transform Object;

  Vector3 _pos = Vector3.zero;
  Vector3 _holderPos = Vector3.zero;

  Vector2 _generalDir = Vector2.zero;

  void Awake()
  {
    _generalDir.x = 1.0f;//Random.Range(-0.8f, 0.8f);
    _generalDir.y = -1.0f;

    GeneralDir.text = string.Format("{0:F2} {1:F2}", _generalDir.x, _generalDir.y);
  }

  float _waveWidth = 1.0f;
  int _wobbleSpeed = 10;
  void WaveTraj()
  {
    _pos = Object.transform.localPosition;

    float vx = Mathf.Sin(_angle * Mathf.Deg2Rad);

    _angle += _wobbleSpeed;
    _angle %= 360;

    _pos.x = (vx * _waveWidth);

    Object.transform.localPosition = _pos;
  }

  int _angle = 0;
  int _rotationSpeed = 4;
  float _radius = 1.5f;
  void CircleTraj()
  {
    _pos = Object.transform.localPosition;

    float vx = Mathf.Sin(_angle * Mathf.Deg2Rad);
    float vy = Mathf.Cos(_angle * Mathf.Deg2Rad);

    Text.text = string.Format("{0}", _angle);
    Value.text = string.Format("{0:F4} {1:F4}", vx, vy);

    _angle += _rotationSpeed;
    _angle %= 360;

    _pos.x = (vx * _radius);
    _pos.y = (vy * _radius);

    Object.transform.localPosition = _pos;
  }

  void Update()
  {
    _holderPos.x += _generalDir.x * Time.smoothDeltaTime * 2.0f;
    _holderPos.y += _generalDir.y * Time.smoothDeltaTime * 2.0f;

    if (_holderPos.x > 8.0f || _holderPos.x < -8.0f)
    {
      _generalDir.x *= -1.0f;
    }

    if (_holderPos.y < -5.5f)
    {
      _holderPos.y = 5.5f;
    }

    switch (TrajectoryType_)
    {
      case TrajectoryType.CIRCLE:
        CircleTraj();
        break;

      case TrajectoryType.WAVE:
        WaveTraj();
        break;

      case TrajectoryType.LINE:
        break;
    }

    Holder.transform.position = _holderPos;
  }
}
