using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;

public class Star : MonoBehaviour
{
  public ParticleSystem Shine;
  public ParticleSystem Trail;

  public GameObject ExplosionPrefab;

  Vector2 _direction = Vector2.zero;
  float _speed = 0.0f;
  float _startAngle = 0.0f;
  float _angleSpeed = 0.0f;

  const float _starSpinSpeed = 500.0f;

  Color _color = Color.magenta;

  Main _mainRef;

  Transform _transform;
  SpriteRenderer _spriteRenderer;
  CircleCollider2D _collider;
  void Awake()
  {
    var objects = GameObject.FindGameObjectsWithTag("MainScript");
    if (objects.Length != 0)
    {
      _mainRef = objects[0].GetComponent<Main>();
    }

    _transform = GetComponent<Transform>();
    _spriteRenderer = GetComponent<SpriteRenderer>();
    _collider = GetComponent<CircleCollider2D>();
  }

  void AdjustParticleSystemColor(ParticleSystem ps)
  {
    var sm = ps.main;
    sm.startColor = _color;

    var col = ps.colorOverLifetime;

    Gradient g = new Gradient();
    g.SetKeys(new GradientColorKey[]
    {
      new GradientColorKey(_color, 0.0f),
      new GradientColorKey(_color, 1.0f)
    },
    new GradientAlphaKey[]
    {
      new GradientAlphaKey(1.0f, 0.0f),
      new GradientAlphaKey(0.0f, 1.0f)
    });

    col.color = g;
  }

  Constants.StarType _starType = Constants.StarType.SILVER;
  public Constants.StarType GetStarType()
  {
    return _starType;
  }

  public void Init(Constants.StarType type,
                    float angle,
                    Vector2 dir,
                    float speed)
  {
    _starType = type;
    _direction = dir;
    _speed = speed;
    _startAngle = angle;

    _angleSpeed = _starSpinSpeed * (_startAngle / -Constants.StarFallSpreadAngle);

    _color = Constants.StarColorsByType[type];

    _spriteRenderer.color = _color;

    AdjustParticleSystemColor(Shine);
    AdjustParticleSystemColor(Trail);
  }

  bool _exploded = false;
  void CheckBorders()
  {
    if (_transform.position.x < _mainRef.Borders.Key)
    {
      Vector3 pos = _transform.position;
      pos.x = _mainRef.Borders.Key;
      _transform.position = pos;
      _direction.x *= -1;
      _angleSpeed *= -1;
    }
    else if (_transform.position.x > _mainRef.Borders.Value)
    {
      Vector3 pos = _transform.position;
      pos.x = _mainRef.Borders.Value;
      _transform.position = pos;
      _direction.x *= -1;
      _angleSpeed *= -1;
    }

    if (_transform.position.y < Constants.BottomBorder)
    {
      _spriteRenderer.enabled = false;
      _collider.enabled = false;

      if (!_exploded)
      {
        var go = Instantiate(ExplosionPrefab, _transform.position, Quaternion.identity);

        Explosion ec = go.GetComponent<Explosion>();
        if (ec != null)
        {
          ec.Explode(_color);
        }

        _exploded = true;

        if (_starType != Constants.StarType.BAD && !_mainRef.IsGameOver)
        {
          _mainRef.DecrementLives();
        }

        if (_starType == Constants.StarType.BAD)
        {
          SoundManager.Instance.PlaySound("grenade");
        }
        else
        {
          int index = Random.Range(1, 4);
          string soundName = string.Format("glass{0}", index);
          SoundManager.Instance.PlaySound(soundName, 0.5f);
        }
      }

      Destroy(gameObject, 3.0f);
    }
  }

  public void ProcessHit()
  {
    _spriteRenderer.enabled = false;
    _collider.enabled = false;
    _speed = 0.0f;
    _angleSpeed = 0.0f;
    Shine.Stop();
    Trail.Stop();
    Destroy(gameObject, 1.0f);
  }

  void Update()
  {
    Vector3 pos = _transform.position;
    pos.y += _direction.y * (_speed * Time.smoothDeltaTime);
    pos.x += _direction.x * (_speed * Time.smoothDeltaTime);

    _transform.position = pos;

    Vector3 angles = _transform.rotation.eulerAngles;
    angles.z += _angleSpeed * Time.smoothDeltaTime;

    _transform.rotation = Quaternion.Euler(angles);

    CheckBorders();
  }
}
