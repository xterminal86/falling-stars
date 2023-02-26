using UnityEngine;

using PairF = System.Collections.Generic.KeyValuePair<float, float>;

using static Constants;

public class Star : MonoBehaviour
{
  public CircleCollider2D ColliderComponent;

  public Transform InnerObject;

  public ParticleSystem Shine;
  public ParticleSystem Trail;

  public Explosion ExplosionPrefab;

  public Sprite SpriteRed;
  public Sprite SpriteGreen;
  public Sprite SpriteYellow;
  public Sprite SpriteSilver;
  public Sprite SpriteCyan;
  public Sprite SpriteHeart;

  Vector2 _direction = Vector2.zero;
  float _speed = 0.0f;
  float _angleSpeed = 0.0f;

  const float _starSpinSpeed = 125.0f;

  Color _color = Color.magenta;

  Main _mainRef;

  bool _exploded = false;
  public bool Exploded
  {
    get { return _exploded; }
  }

  SpriteRenderer _spriteRenderer;
  CircleCollider2D _collider;
  void Awake()
  {
    var mainRef = GameObject.FindGameObjectWithTag("MainScript");
    if (mainRef != null)
    {
      _mainRef = mainRef.GetComponent<Main>();
    }
    else
    {
      Debug.LogError("MainScript not found!");
    }

    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    _collider = GetComponentInChildren<CircleCollider2D>();
  }

  // ===========================================================================

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

  // ===========================================================================

  StarType _starType = StarType.SILVER;
  public StarType GetStarType()
  {
    return _starType;
  }

  // ===========================================================================

  void RandomizeTrajectory()
  {
    switch(_starTrajectory)
    {
      case StarTrajectory.WAVE:
        //_wobbleSpeed = Random.Range(1, 6);
        _waveWidth = Random.Range(1.0f, 2.0f);
        break;

      case StarTrajectory.CIRCLE:
        //_rotationSpeed = Random.Range(2, 7);
        //_radius = Random.Range(0.25f, 1.0f);
        _radius = Random.Range(0.5f, 2.0f);
        break;

      case StarTrajectory.LINE:
        // Do nothing
        break;
    }
  }

  // ===========================================================================

  int _additionalScore = 0;
  public int GetAdditionalScore()
  {
    _additionalScore = 0;

    switch(_starTrajectory)
    {
      case StarTrajectory.WAVE:
        int ww = Mathf.RoundToInt(_waveWidth);
        //_additionalScore = (_wobbleSpeed + ww);
        _additionalScore = ww;
        break;

      case StarTrajectory.CIRCLE:
        int r = Mathf.RoundToInt(_radius);
        //_additionalScore = (_rotationSpeed + r);
        _additionalScore = (r < 1) ? 1 : r;
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

  // ===========================================================================

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

    // Because circular movement adds up with direction
    // thus making overall movement too fast and accelerated.
    if (_starTrajectory == StarTrajectory.CIRCLE)
    {
      _speed = 1.0f;
    }

    RandomizeTrajectory();

    _angleSpeed = Mathf.Sign(angle) * _speed * _starSpinSpeed;

    if (_starType == StarType.HEART)
    {
      _color = Constants.StarColorsByType[type];
      _spriteRenderer.color = Color.white;
      InnerObject.localScale = new Vector3(1.5f, 1.5f, 1.5f);
      ColliderComponent.radius = 0.5f;
    }
    else
    {
      _color = Constants.StarColorsByType[type];
    }

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

      case StarType.HEART:
        _spriteRenderer.sprite = SpriteHeart;
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

  // ===========================================================================

  PairF _borders;
  void CheckBorders()
  {
    _borders = _mainRef.Borders;

    AdjustBorders();

    bool toggleDirection =
      (_direction.x < 0 && transform.position.x < _adjustedBorderLeft)
   || (_direction.x > 0 && transform.position.x > _adjustedBorderRight);

    if (toggleDirection)
    {
      _direction.x *= -1;
    }

    if (InnerObject.position.y < Constants.BottomBorder)
    {
      _spriteRenderer.enabled = false;
      _collider.enabled = false;

      if (!_exploded)
      {
        Explosion ec = Instantiate(ExplosionPrefab, InnerObject.position, Quaternion.identity);
        ec.Explode(_color);

        _exploded = true;

        if (_starType != StarType.BAD
         && _starType != StarType.HEART
         && !_mainRef.IsGameOver)
        {
          _mainRef.DecrementLives();
          _mainRef.ShatterOverlay(_starType);
        }

        if (_starType == StarType.BAD || _starType == StarType.HEART)
        {
          SoundManager.Instance.PlaySound("star-douse-eq", 0.5f);
        }
        else
        {
          //float pitch = 1.0f * Constants.StarSpeedScaleByType[_starType];
          //float volume = 0.4f * (pitch * 0.25f);
          SoundManager.Instance.PlaySound("star-break", 0.3f, 1.0f);
        }

        if (_starType == StarType.HEART)
        {
          _mainRef.HeartWasSpawned = false;
        }
      }

      Shine.Stop();
      Trail.Stop();
      Destroy(gameObject, 3.0f);
    }
  }

  // ===========================================================================

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

    if (_starType == StarType.HEART)
    {
      _mainRef.HeartWasSpawned = false;
    }
  }

  // ===========================================================================

  Vector3 _innerObjectPos = Vector3.zero;

  float _angleIncreaseScale = 200.0f;

  //int _angle = 0;
  float _angleF = 0.0f;

  float _sinResult = 0.0f;
  float _cosResult = 0.0f;

  //int _rotationSpeed = 4;
  float _radius = 3.0f;
  void AddCircle()
  {
    _innerObjectPos = InnerObject.localPosition;

    //_sinResult = Mathf.Sin(_angle * Mathf.Deg2Rad);
    //_cosResult = Mathf.Cos(_angle * Mathf.Deg2Rad);

    _sinResult = Mathf.Sin(_angleF * Mathf.Deg2Rad);
    _cosResult = Mathf.Cos(_angleF * Mathf.Deg2Rad);

    _angleF += Time.smoothDeltaTime * _angleIncreaseScale;

    if (_angleF > 360.0f)
    {
      _angleF -= 360.0f;
    }

    //_angle += _rotationSpeed;
    //_angle %= 360;

    _innerObjectPos.x = (_sinResult * _radius);
    _innerObjectPos.y = (_cosResult * _radius);

    InnerObject.localPosition = _innerObjectPos;
  }

  // ===========================================================================

  float _waveWidth = 1.0f;
  //int _wobbleSpeed = 10;
  void AddWave()
  {
    _innerObjectPos = InnerObject.localPosition;

    //_sinResult = Mathf.Sin(_angle * Mathf.Deg2Rad);
    _sinResult = Mathf.Sin(_angleF * Mathf.Deg2Rad);

    _angleF += Time.smoothDeltaTime * _angleIncreaseScale;

    if (_angleF > 360.0f)
    {
      _angleF -= 360.0f;
    }

    //_angle += _wobbleSpeed;
    //_angle %= 360;

    _innerObjectPos.x = (_sinResult * _waveWidth);

    InnerObject.localPosition = _innerObjectPos;
  }

  // ===========================================================================

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

  // ===========================================================================

  void Update()
  {
    if (_exploded || _starCaught || _mainRef.TimeStopped)
    {
      return;
    }

    CheckBorders();
    UpdatePosition();
  }
}
