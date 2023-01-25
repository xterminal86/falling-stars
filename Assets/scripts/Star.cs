using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;

using PairF = System.Collections.Generic.KeyValuePair<float, float>;

using static Constants;

public class Star : MonoBehaviour
{
  public Transform InnerObject;

  public ParticleSystem Shine;
  public ParticleSystem Trail;

  public GameObject ExplosionPrefab;

  public Sprite SpriteRed;
  public Sprite SpriteGreen;
  public Sprite SpriteYellow;
  public Sprite SpriteSilver;
  public Sprite SpriteCyan;

  Vector2 _direction = Vector2.zero;
  float _speed = 0.0f;
  float _angleSpeed = 0.0f;

  const float _starSpinSpeed = 125.0f;

  Color _color = Color.magenta;

  Main _mainRef;

  SpriteRenderer _spriteRenderer;
  CircleCollider2D _collider;
  void Awake()
  {
    var objects = GameObject.FindGameObjectsWithTag("MainScript");
    if (objects.Length != 0)
    {
      _mainRef = objects[0].GetComponent<Main>();
    }
    else
    {
      Debug.LogWarning("MainScript not found!");
    }

    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    _collider = GetComponentInChildren<CircleCollider2D>();
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

  StarType _starType = StarType.SILVER;
  public StarType GetStarType()
  {
    return _starType;
  }

  void RandomizeTrajectory()
  {
    switch(_starTrajectory)
    {
      case StarTrajectory.WAVE:
        _wobbleSpeed = Random.Range(1, 6);
        _waveWidth = Random.Range(1.0f, 2.0f);
        break;

      case StarTrajectory.CIRCLE:
        _rotationSpeed = Random.Range(2, 7);
        _radius = Random.Range(0.25f, 1.0f);
        break;

      case StarTrajectory.LINE:
        // Do nothing
        break;
    }
  }

  int _additionalScore = 0;
  public int GetAdditionalScore()
  {
    _additionalScore = 0;

    switch(_starTrajectory)
    {
      case StarTrajectory.WAVE:
        int ww = Mathf.RoundToInt(_waveWidth);
        _additionalScore = (_wobbleSpeed + ww);
        break;

      case StarTrajectory.CIRCLE:
        int r = Mathf.RoundToInt(_radius);
        _additionalScore = (_rotationSpeed + r);
        break;

      case StarTrajectory.LINE:
        // Do nothing
        break;
    }

    if (_additionalScore != 0)
    {
      _additionalScore = Mathf.RoundToInt((float)_additionalScore * Constants.StarAdditionalScoreMultiplierByType[_starType]);
    }

    return _additionalScore;
  }

  StarTrajectory _starTrajectory;
  public void Init(StarType type,
                    float angle,
                    Vector2 dir,
                    float speed,
                    StarTrajectory trajectory = StarTrajectory.LINE)
  {
    _starType   = type;
    _direction  = dir;
    _speed      = speed;

    _starTrajectory = trajectory;

    RandomizeTrajectory();

    _angleSpeed = Mathf.Sign(angle) * _speed * _starSpinSpeed;

    _color = Constants.StarColorsByType[type];

    switch (type)
    {
      case StarType.BAD:
        _spriteRenderer.sprite = SpriteRed;
        break;

      case StarType.GREEN:
        _spriteRenderer.sprite = SpriteGreen;
        break;

      case StarType.CYAN:
        _spriteRenderer.sprite = SpriteCyan;
        break;

      case StarType.SILVER:
        _spriteRenderer.sprite = SpriteSilver;
        break;

      case StarType.YELLOW:
        _spriteRenderer.sprite = SpriteYellow;
        break;
    }

    //_spriteRenderer.color = _color;

    AdjustParticleSystemColor(Shine);
    AdjustParticleSystemColor(Trail);
  }

  float _adjustedBorderLeft = 0.0f;
  float _adjustedBorderRight = 0.0f;
  void AdjustBorders()
  {
    switch (_starTrajectory)
    {
      case StarTrajectory.WAVE:
        _adjustedBorderLeft  = _borders.Key + _waveWidth;
        _adjustedBorderRight = _borders.Value - _waveWidth;
        break;

      case StarTrajectory.CIRCLE:
        _adjustedBorderLeft  = _borders.Key + _radius;
        _adjustedBorderRight = _borders.Value - _radius;
        break;

      case StarTrajectory.LINE:
        _adjustedBorderLeft = _borders.Key;
        _adjustedBorderRight = _borders.Value;
        break;
    }
  }

  bool _exploded = false;
  public bool Exploded
  {
    get { return _exploded; }
  }

  Vector3 _tmpPos = Vector3.zero;
  PairF _borders;
  void CheckBorders()
  {
    _borders = _mainRef.Borders;

    AdjustBorders();

    if (transform.position.x < _adjustedBorderLeft)
    {
      _tmpPos = transform.position;
      _tmpPos.x = _adjustedBorderLeft;
      transform.position = _tmpPos;
      _direction.x *= -1;
    }
    else if (transform.position.x > _adjustedBorderRight)
    {
      _tmpPos = transform.position;
      _tmpPos.x = _adjustedBorderRight;
      transform.position = _tmpPos;
      _direction.x *= -1;
    }

    if (InnerObject.position.y < Constants.BottomBorder)
    {
      _spriteRenderer.enabled = false;
      _collider.enabled = false;

      if (!_exploded)
      {
        var go = Instantiate(ExplosionPrefab, InnerObject.position, Quaternion.identity);

        Explosion ec = go.GetComponent<Explosion>();
        if (ec != null)
        {
          ec.Explode(_color);
        }

        _exploded = true;

        if (_starType != StarType.BAD && !_mainRef.IsGameOver)
        {
          _mainRef.DecrementLives();
        }

        if (_starType == StarType.BAD)
        {
          SoundManager.Instance.PlaySound("star-douse-eq", 0.5f);
        }
        else
        {
          float pitch = 1.0f * Constants.StarSpeedScaleByType[_starType];
          float volume = 0.4f * (pitch * 0.25f);
          SoundManager.Instance.PlaySound("star-break", volume, pitch);
        }
      }

      Shine.Stop();
      Trail.Stop();
      Destroy(gameObject, 3.0f);
    }
  }
    
  bool _starCaught = false;
  public void ProcessHit()
  {    
    _starCaught = true;
    _spriteRenderer.enabled = false;
    _collider.enabled = false;
    _speed = 0.0f;
    _angleSpeed = 0.0f;
    Shine.Stop();
    Trail.Stop();
    Destroy(gameObject, 1.0f);
  }

  Vector3 _innerObjectPos = Vector3.zero;

  int _angle = 0;

  float _sinResult = 0.0f;
  float _cosResult = 0.0f;

  int _rotationSpeed = 4;
  float _radius = 1.5f;
  void AddCircle()
  {
    _innerObjectPos = InnerObject.localPosition;

    _sinResult = Mathf.Sin(_angle * Mathf.Deg2Rad);
    _cosResult = Mathf.Cos(_angle * Mathf.Deg2Rad);

    _angle += _rotationSpeed;
    _angle %= 360;

    _innerObjectPos.x = (_sinResult * _radius);
    _innerObjectPos.y = (_cosResult * _radius);

    InnerObject.localPosition = _innerObjectPos;
  }

  float _waveWidth = 1.0f;
  int _wobbleSpeed = 10;
  void AddWave()
  {
    _innerObjectPos = InnerObject.localPosition;

    _sinResult = Mathf.Sin(_angle * Mathf.Deg2Rad);

    _angle += _wobbleSpeed;
    _angle %= 360;

    _innerObjectPos.x = (_sinResult * _waveWidth);

    InnerObject.localPosition = _innerObjectPos;
  }

  Vector3 _holderPos = Vector3.zero;
  Vector3 _angles = Vector3.zero;
  void UpdatePosition()
  {
    _holderPos = transform.position;
    _angles    = InnerObject.eulerAngles;

    switch (_starTrajectory)
    {
      case StarTrajectory.WAVE:
        AddWave();
        break;

      case StarTrajectory.CIRCLE:
        AddCircle();
        break;

      case StarTrajectory.LINE:
        // Pass through
        break;
    }

    _holderPos.x += _direction.x * _speed * Time.smoothDeltaTime;
    _holderPos.y += _direction.y * _speed * Time.smoothDeltaTime;

    _angles.z += _angleSpeed * Time.smoothDeltaTime;

    InnerObject.localRotation = Quaternion.Euler(_angles);

    transform.position = _holderPos;
  }

  void Update()
  {
    if (_exploded || _starCaught)
    {
      return;
    }

    CheckBorders();
    UpdatePosition();
  }
}
