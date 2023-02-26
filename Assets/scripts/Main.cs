using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// =================================
using TMPro;

using PairF = System.Collections.Generic.KeyValuePair<float, float>;
using static Constants;

public class Main : MonoBehaviour
{
  [Header("Main Objects")]
  public Star StarPrefab;
  public Explosion ExplosionPrefab;
  public ScoreBubble ScoreBubblePrefab;
  public ImageBubble ImageBubblePrefab;
  public Troll TrollPrefab;

  [Header("Scene Props")]
  public GameObject GroundCellPrefab;
  public GameObject MouseClickEffectPrefab;

  [Header("Transforms")]
  public Transform ObjectsHolder;
  public Transform TrollCurtain;

  [Header("Animators")]
  public Animator ScoreAnimator;
  public Animator ClockAnimator;
  public Animator ZaWarudoAnimator;

  [Header("Clock")]
  public GameObject SpawnMeterHolder;

  [Header("TMP_Text")]
  public TMP_Text ScoreText;
  public TMP_Text DifficultyText;
  public TMP_Text StoppedTimeLeftText;

  // DEBUG
  [Header("Debug")]
  public TMP_Text CyanScore;
  public TMP_Text SilverScore;
  public TMP_Text GreenScore;
  public TMP_Text YellowScore;

  [Header("Clock bar")]
  public Image SpawnMeter;

  [Header("Star shatter cue")]
  public Image ShatterEffect;

  [Header("Player lives")]
  public List<Heart> Hearts;

  [Header("GUI")]
  public GameObject StartWindow;
  public RectTransform RestartWindow;
  public RectTransform TimeStopLeftHolder;
  public RectTransform EvilStar;
  public RectTransform TheWorldHolder;
  public Image TimeStopClockIcon;
  public Image TheWorldReadyImage;
  public Image TheWorldReadyBorder;
  public GameObject TitleText;
  public GameObject BtnInfo;
  public GameObject BtnHighScores;
  public Slider SpawnMeterSlider;

  [Header("Touch effect")]
  public GameObject TouchEffectPrefab;

  [Header("Screen shaker")]
  public ScreenShake ScreenShaker;

  [Header("App config")]
  public Config AppConfig;

  [Header("Audio filters")]
  public AudioReverbFilter ReverbFilter;
  public AudioLowPassFilter LowPassFilter;

  [Header("Time stop clouds")]
  public SpriteRenderer Clouds;

  int _difficulty = 1;
  int _score = 0;

  float _spawnTimeout = 0.0f;
  float _trollTimeout = 0.0f;

  float _spawnRateNormalized = 0.0f;

#if UNITY_EDITOR
  [Header("Editor only")]
  public bool GodMode = false;
#endif

  Dictionary<StarType, int> _scoreByStarType = new Dictionary<StarType, int>()
  {
    { StarType.CYAN,   0 },
    { StarType.GREEN,  0 },
    { StarType.SILVER, 0 },
    { StarType.YELLOW, 0 }
  };

  Dictionary<StarType, TMP_Text> _scoreFieldByStarType = new Dictionary<StarType, TMP_Text>();

  #region PROPERTIES
  bool _isGameOver = false;
  public bool IsGameOver
  {
    get { return _isGameOver; }
  }

  bool _heartWasSpawned = false;
  public bool HeartWasSpawned
  {
    get { return _heartWasSpawned;  }
    set { _heartWasSpawned = value; }
  }

  PairF _borders;
  public PairF Borders
  {
    get { return _borders; }
  }

  PairF _spawnX;
  PairF _spawnTrajX;

  float _spawnY;
  public float SpawnY
  {
    get { return _spawnY; }
  }

  bool _timeStopped = false;
  public bool TimeStopped
  {
    get { return _timeStopped; }
  }
  #endregion

  // ===========================================================================

  public void OnPlusButton()
  {
    _difficulty++;

    _difficulty = Mathf.Clamp(_difficulty, 1, 10);

    DifficultyText.text = _difficulty.ToString();
  }

  // ===========================================================================

  public void OnMinusButton()
  {
    _difficulty--;

    _difficulty = Mathf.Clamp(_difficulty, 1, 10);

    DifficultyText.text = _difficulty.ToString();
  }

  // ===========================================================================

  public void OnStartButton()
  {
    //StartWindow.SetActive(false);
    TitleText.SetActive(false);
    BtnInfo.SetActive(false);
    BtnHighScores.SetActive(false);
    //SpawnMeterHolder.SetActive(true);
    SpawnMeterSlider.gameObject.SetActive(true);
    EvilStar.gameObject.SetActive(true);
    TheWorldHolder.gameObject.SetActive(true);

    StartGame();
  }

  // ===========================================================================

  public void OnRestartButton()
  {
    RestartWindow.gameObject.SetActive(false);

    SceneManager.LoadScene("main");
  }

  // ===========================================================================

  Vector3 _restartWindowPos = Vector3.zero;
  IEnumerator GameOverScreenSlideRoutine()
  {
    float minPos = -64.0f;
    float yStep = 32.0f;
    float yStepQ = yStep * 0.25f;

    int sh = Screen.height;

    _restartWindowPos = RestartWindow.localPosition;

    _restartWindowPos.y = sh;

    RestartWindow.localPosition = _restartWindowPos;

    RestartWindow.gameObject.SetActive(true);

    while (_restartWindowPos.y > minPos)
    {
      _restartWindowPos.y -= yStep;

      _restartWindowPos.y = Mathf.Clamp(_restartWindowPos.y, minPos, Mathf.Infinity);

      RestartWindow.localPosition = _restartWindowPos;

      yield return null;
    }

    while (_restartWindowPos.y < 0.0f)
    {
      _restartWindowPos.y += yStepQ;

      _restartWindowPos.y = Mathf.Clamp(_restartWindowPos.y, minPos, 0.0f);

      RestartWindow.localPosition = _restartWindowPos;

      yield return null;
    }

    yield return null;
  }

  // ===========================================================================

  IEnumerator ShowOverlayRoutine(StarType st)
  {
    Color overlayColor = Color.red; //StarColorsByType[st];
    overlayColor.a = 0.5f;

    while (overlayColor.a > 0.0f)
    {
      overlayColor.a -= Time.unscaledDeltaTime;
      ShatterEffect.color = overlayColor;
      yield return null;
    }

    overlayColor.a = 0.0f;
    ShatterEffect.color = overlayColor;

    yield return null;
  }

  // ===========================================================================

  Color _none = new Color(0.0f, 0.0f, 0.0f, 0.0f);
  Coroutine _coro;
  public void ShatterOverlay(StarType st)
  {
    if (_lives > 0)
    {
      if (_coro != null)
      {
        StopCoroutine(_coro);
        ShatterEffect.color = _none;
        _coro = null;
      }

      var coro = ShowOverlayRoutine(st);
      _coro = StartCoroutine(coro);
    }
  }

  // ===========================================================================

  int _lives = Constants.MaxLives;
  public void DecrementLives()
  {
    int guiIndex = Hearts.Count - _lives;
    Hearts[guiIndex].FadeOut();

#if UNITY_EDITOR
    if (!GodMode)
    {
      _lives--;
    }
#else
    _lives--;
#endif

    if (_lives == 0)
    {
      if (_timeStopped)
      {
        StopCoroutine(_timeStoppedCoro);
        TimeStopLeftHolder.gameObject.SetActive(false);
        _timeStopped = false;

        if (!_timeResumeWorking)
        {
          var coro = TimeResumeRoutine();
          _timeResumeCoro = StartCoroutine(coro);
        }
      }

      _isGameOver = true;
      ClockAnimator.enabled = false;
      StartCoroutine(GameOverScreenSlideRoutine());
      SoundManager.Instance.PlaySound("game-over", 0.5f, 1.0f, false);
      AppConfig.AddHighscore(_score);
      TrollPrefab.TrollPlayer(0.0f, true);
    }
  }

  // ===========================================================================

  public void IncrementLives()
  {
    if (_lives == Constants.MaxLives)
    {
      return;
    }

    _lives++;

    int guiIndex = Hearts.Count - _lives;
    Hearts[guiIndex].FadeIn();
  }

  // ===========================================================================

  bool _gameStarted = false;
  void StartGame()
  {
    if (_gameStarted)
    {
      return;
    }

    _spawnTimeout = Constants.SpawnTimeoutInit;
    _trollTimeout = Constants.TrollTimeoutInit;

    _spawnRateNormalized = GetNormalizedValue(_spawnTimeout,
                                               Constants.SpawnTimeoutMax,
                                               Constants.SpawnTimeoutInit);

    SpawnMeter.fillAmount = 1.0f - _spawnRateNormalized;

    SpawnMeterSlider.value = 1.0f - _spawnRateNormalized;

    //Debug.Log("spawn timeout: " + _spawnTimeout);

    _gameStarted = true;
  }

  // ===========================================================================

  Vector2 _groundSpan = Vector2.zero;
  void CreateGround()
  {
    float groundPosY = -(Camera.main.orthographicSize - 0.5f);

    Vector3 pos = Vector3.zero;
    for (float x = _groundSpan.x - 1; x < _groundSpan.y + 1; x++)
    {
      pos.x = x;
      pos.y = groundPosY;

      Instantiate(GroundCellPrefab, pos, Quaternion.identity, ObjectsHolder);
    }
  }

  // ===========================================================================

  StarType GetRandomGoodStar()
  {
    int maxType = (int)StarType.SILVER;
    int starType = Random.Range(0, maxType + 1);
    return (StarType)starType;
  }

  // ===========================================================================

  StarTrajectory GetRandomTrajectory()
  {
    int r = Random.Range(0, (int)StarTrajectory.CIRCLE + 1);
    return (StarTrajectory)r;
  }

  // ===========================================================================

  void Awake()
  {
    Input.backButtonLeavesApp = true;
    Application.targetFrameRate = 90;

    _spawnY = Camera.main.orthographicSize + 1.0f;

    float aspect = (float)Screen.width / (float)Screen.height;
    float hBorder = Camera.main.orthographicSize * aspect;

    float groundSpanX = hBorder;

    _spawnX = new PairF(-Mathf.Round(groundSpanX) + 2.0f,
                         Mathf.Round(groundSpanX) - 2.0f);

    _spawnTrajX = new PairF(-Mathf.Round(groundSpanX) + 4.0f,
                            Mathf.Round(groundSpanX) - 4.0f);

    //Debug.Log(string.Format("SpawnX: {0}", _spawnX));
    //Debug.Log(string.Format("SpawnTrajX: {0}", _spawnTrajX));

    groundSpanX = Mathf.Round(groundSpanX) + 2.0f;

    Vector3 curtainScale = TrollCurtain.localScale;
    TrollCurtain.localScale = new Vector3(groundSpanX * 2.0f,
                                           curtainScale.y,
                                           curtainScale.z);

    //Debug.Log(string.Format("groundSpanX: {0}", groundSpanX));

    _groundSpan.x = -groundSpanX;
    _groundSpan.y = groundSpanX;

    CreateGround();

    SoundManager.Instance.Initialize();

    DifficultyText.text = _difficulty.ToString();
    ScoreText.text = _score.ToString();

    SpriteRenderer sr = StarPrefab.GetComponentInChildren<SpriteRenderer>();
    if (sr != null)
    {
      float spriteWidth = sr.bounds.size.x;
      float offset = spriteWidth / 2.0f;

      _borders = new PairF(-hBorder + offset,
                           hBorder - offset);
    }

    //Debug.Log(string.Format("Borders: {0}", _borders));
    //Debug.Log(string.Format("{0} {1}", Screen.width, Screen.height));

    AppConfig.ReadConfig();

    _scoreFieldByStarType[StarType.CYAN] = CyanScore;
    _scoreFieldByStarType[StarType.SILVER] = SilverScore;
    _scoreFieldByStarType[StarType.GREEN] = GreenScore;
    _scoreFieldByStarType[StarType.YELLOW] = YellowScore;

    //
    // NOTE:
    //
    // TMPro 3D text causes lag spike on first instantiation,
    // so to fix this you have to create TMPro text UI element
    // somewhere in the scene and type all characters
    // you're going to use in it.
    // To hide this element you can set alpha of text color to 0.
    //
  }

  // ===========================================================================

  Vector2 _rndDir = Vector2.zero;
  public Vector2 GetRandomDir()
  {
    float angle = Random.Range(-Constants.StarFallSpreadAngle,
                                 Constants.StarFallSpreadAngle);

    float s = Mathf.Sin(angle * Mathf.Deg2Rad);
    float c = Mathf.Cos(angle * Mathf.Deg2Rad);

    _rndDir.x = s;
    _rndDir.y = -c;

    return _rndDir;
  }

  // ===========================================================================

  Vector3 _starDir = Vector3.zero;
  Vector3 _trollStarSpawnPos = Vector3.zero;
  public void SpawnTrollStars(bool playerLost = false)
  {
    bool shouldBeGood = playerLost ? true : (Random.Range(0, 2) == 0);

    float speed = shouldBeGood ? 1.0f : Random.Range(2.0f, 4.0f);

    bool allTrajsRandom = playerLost ? true : (Random.Range(0, 2) == 0);

    bool allDirsRandom = playerLost ? true : (Random.Range(0, 2) == 0);

    StarTrajectory traj = GetRandomTrajectory();

    float angle = Random.Range(-Constants.StarFallSpreadAngle,
                                 Constants.StarFallSpreadAngle);

    if (Random.Range(0, 2) == 0)
    {
      float s = Mathf.Sin(angle * Mathf.Deg2Rad);
      float c = Mathf.Cos(angle * Mathf.Deg2Rad);

      _starDir.x = s;
      _starDir.y = -c;
    }
    else
    {
      _starDir.x = 0.0f;
      _starDir.y = -1.0f;
    }

    _trollStarSpawnPos.x = _spawnTrajX.Key;
    _trollStarSpawnPos.y = _spawnY;

    float step = shouldBeGood ? 2.0f : 1.0f;

    for (float x = _spawnTrajX.Key; x <= _spawnTrajX.Value; x += step)
    {
      _trollStarSpawnPos.x = x;

      Star star = Instantiate(StarPrefab,
                              _trollStarSpawnPos,
                              Quaternion.identity,
                              ObjectsHolder);

      StarType st = shouldBeGood ? GetRandomGoodStar() : StarType.BAD;

      star.Init(st,
                angle,
                allDirsRandom ? GetRandomDir() : _starDir,
                speed,
                allTrajsRandom ? GetRandomTrajectory() : traj);
    }
  }

  // ===========================================================================

  Vector3 _goodStarLastSpawnPos = Vector3.zero;
  void SpawnStar(bool badStar)
  {
    StarType st = badStar ? StarType.BAD : GetRandomGoodStar();

    float mult = (float)(Constants.MaxLives - _lives) / (float)(Constants.MaxLives - 1);
    float heartChance = (float)Constants.HeartStarMaxChance * mult;
    float roll = Random.Range(0, 100);

    // Debug.Log(string.Format("roll = {0} chance = {1}", roll, heartChance));

    bool shouldSpawnHeart = ((_iterations % Constants.HeartSpawnCheckAfterIterations == 0)
                            && !_heartWasSpawned
                            && !_isGameOver
                            && (roll < heartChance));

    if (shouldSpawnHeart)
    {
      _heartWasSpawned = true;
      st = StarType.HEART;
    }

    int trajType = (st == StarType.BAD) ?
                   Random.Range(1, 3) :
                   Random.Range(0, 3);

    PairF spawnX = (trajType == 0) ? _spawnX : _spawnTrajX;

    float x = badStar ?
              Random.Range(_goodStarLastSpawnPos.x - 0.5f, _goodStarLastSpawnPos.x + 0.5f) :
              Random.Range(spawnX.Key, spawnX.Value);

    float angle = Random.Range(-Constants.StarFallSpreadAngle,
                                Constants.StarFallSpreadAngle);

    float s = Mathf.Sin(angle * Mathf.Deg2Rad);
    float c = Mathf.Cos(angle * Mathf.Deg2Rad);

    //Debug.Log($"angle = { angle }");
    //Debug.Log($"sin = {s} cos = {c}");

    Vector2 dir = new Vector2(s, -c);

    _goodStarLastSpawnPos.x = x;
    _goodStarLastSpawnPos.y = _spawnY;

    Star star = Instantiate(StarPrefab,
                            _goodStarLastSpawnPos,
                            Quaternion.identity,
                            ObjectsHolder);

    StarTrajectory traj = (StarTrajectory)trajType;

    float speedScale = 1.0f;

    if (st == StarType.BAD)
    {
      int rndIndex = Random.Range(0, Constants.StarSpeedScaleByType.Count);
      List<StarType> keyList = new List<StarType>(Constants.StarSpeedScaleByType.Keys);
      StarType k = keyList[rndIndex];
      speedScale = Constants.StarSpeedScaleByType[k];
    }
    else
    {
      speedScale = Constants.StarSpeedScaleByType[st];
    }

    star.Init(st, angle, dir, Constants.StartSpeed * speedScale, traj);
  }

  // ===========================================================================

  IEnumerator CaughtBadStarExplosionRoutine(Vector3 pointOfCreation)
  {
    const float deltaMin = 0.2f;
    const float delta = 0.4f;

    Vector3 randomPos = pointOfCreation;

    float dx = Random.Range(-delta, delta);
    float dy = Random.Range(-delta, delta);

    if (dx < 0) { dx = Mathf.Clamp(dx, -delta, -deltaMin); }
    if (dx > 0) { dx = Mathf.Clamp(dx, deltaMin, delta); }
    if (dy < 0) { dy = Mathf.Clamp(dy, -delta, -deltaMin); }
    if (dy > 0) { dy = Mathf.Clamp(dy, deltaMin, delta); }

    for (int i = 0; i < 3; i++)
    {
      if (i == 2)
      {
        dx = -dx;
        dy = -dy;
      }

      if (i != 0)
      {
        randomPos.x += dx;
        randomPos.y += dy;
      }

      Explosion ec = Instantiate(ExplosionPrefab, randomPos, Quaternion.identity);
      ec.Explode(Color.red);

      yield return new WaitForSecondsRealtime(0.2f);
    }

    yield return null;
  }

  // ===========================================================================

  void InstantiateAtMousePos(GameObject prefab)
  {
    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    wp.z = 0.0f;
    var go = Instantiate(prefab, wp, Quaternion.identity, ObjectsHolder);
    Destroy(go, 1.0f);
  }

  // ===========================================================================

  float _rewindTo = 0.0f;
  void CalculateTimeGracePeriod()
  {
    float grace = Constants.SpawnTimeoutInit - Constants.SpawnTimeoutMax;
    grace *= 0.0625f;

    _spawnTimeout += grace;

    _spawnTimeout = Mathf.Clamp(_spawnTimeout,
                                Constants.SpawnTimeoutMax,
                                Constants.SpawnTimeoutInit);

    _rewindTo = 1.0f - GetNormalizedValue(_spawnTimeout,
                                           Constants.SpawnTimeoutMax,
                                           Constants.SpawnTimeoutInit);
  }

  // ===========================================================================

  bool _clockRewinding = false;
  IEnumerator RewindClockRoutine()
  {
    _clockRewinding = true;

    //ClockAnimator.Play("clock-reverse");

    //float currentMeter = SpawnMeter.fillAmount;
    float currentMeter = SpawnMeterSlider.value;

    CalculateTimeGracePeriod();

    while (currentMeter > _rewindTo)
    {
      currentMeter -= Time.unscaledDeltaTime * 0.125f;
      currentMeter = Mathf.Clamp(currentMeter, 0.0f, 1.0f);

      //SpawnMeter.fillAmount = currentMeter;
      SpawnMeterSlider.value = currentMeter;

      yield return null;
    }

    //ClockAnimator.Play("clock");

    _clockRewinding = false;

    yield return null;
  }

  // ===========================================================================

  void PunchTroll(Troll t)
  {
    t.PunchTroll();
  }

  // ===========================================================================

  void ProcessStar(Star s, Vector3 hitPos)
  {
    Vector3 objPos = hitPos;

    int totalScore = 0;

    var starType = s.GetStarType();
    switch (starType)
    {
      case StarType.BAD:
        DecrementLives();
        break;

      case StarType.HEART:
        IncrementLives();
        break;

      default:
        totalScore = Constants.StarScoreByType[starType] + s.GetAdditionalScore();
        _score += totalScore;
        ScoreText.text = _score.ToString();

#if UNITY_EDITOR
        _scoreByStarType[starType] += totalScore;
        _scoreFieldByStarType[starType].text = _scoreByStarType[starType].ToString();
#endif

        break;
    }

    s.ProcessHit();

    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    wp.z = 0.0f;

    switch (starType)
    {
      case StarType.BAD:
      {
        if (_lives != 0)
        {
          if (!_clockRewinding)
          {
            IEnumerator rewindTime = RewindClockRoutine();
            StartCoroutine(rewindTime);
          }
          else
          {
            CalculateTimeGracePeriod();
          }
        }

        int index = Random.Range(1, 5);
        string soundName = string.Format("explode{0}", index);
        SoundManager.Instance.PlaySound(soundName);
        IEnumerator coro = CaughtBadStarExplosionRoutine(objPos);
        StartCoroutine(coro);
        ScreenShaker.ShakeScreen();
      }
      break;

      case StarType.HEART:
      {
        var go = Instantiate(MouseClickEffectPrefab, objPos, Quaternion.identity, ObjectsHolder);
        Destroy(go, 1.0f);

        float rndPitch = Random.Range(0.6f, 1.4f);
        SoundManager.Instance.PlaySound("pop", 1.0f, rndPitch, false);
        ScoreAnimator.Play("score-pop", -1, 0.0f);

        _bubblePos.x = objPos.x;
        _bubblePos.y = objPos.y + 0.5f;

        ImageBubble ib = Instantiate(ImageBubblePrefab,
                                      _bubblePos,
                                      Quaternion.identity,
                                      ObjectsHolder);
        ib.Init();
      }
      break;

      default:
      {
        var go = Instantiate(MouseClickEffectPrefab, objPos, Quaternion.identity, ObjectsHolder);
        Destroy(go, 1.0f);

        float rndPitch = Random.Range(0.6f, 1.4f);
        SoundManager.Instance.PlaySound("pop", 1.0f, rndPitch, false);
        ScoreAnimator.Play("score-pop", -1, 0.0f);

        _bubblePos.x = objPos.x;
        _bubblePos.y = objPos.y + 0.5f;

        ScoreBubble sb = Instantiate(ScoreBubblePrefab,
                                    _bubblePos,
                                    Quaternion.identity,
                                    ObjectsHolder);

        sb.Init(starType, totalScore);
      }
      break;
    }
  }

  // ===========================================================================

  Vector3 _bubblePos = Vector3.zero;
  void CheckMouse()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

      int mask = LayerMask.GetMask("stars", "troll", "curtain");

      RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, mask);

      if (hit.collider != null)
      {
        Troll t = hit.collider.gameObject.GetComponentInParent<Troll>();
        if (t != null)
        {
          PunchTroll(t);
        }
        else
        {
          Star s = hit.collider.gameObject.GetComponentInParent<Star>();
          //
          // Prevent scoring via mouse click and losing score due to miss
          // if clicked at the same time as star hit the ground.
          //
          if (s != null && !s.Exploded)
          {
            ProcessStar(s, hit.collider.transform.position);
          }
        }
      }
      else
      {
        InstantiateAtMousePos(TouchEffectPrefab);
      }
    }
  }

  // ===========================================================================

  void ToggleAudioFilters()
  {
    if (_timeStopped)
    {
      ReverbFilter.enabled = true;
      LowPassFilter.enabled = true;
    }
    else
    {
      ReverbFilter.enabled = false;
      LowPassFilter.enabled = false;
    }
  }

  // ===========================================================================

  Coroutine _timeStoppedCoro;
  IEnumerator TimeHasStoppedRoutine()
  {
    float guiCounter = (float)Constants.StoppedTimeDuration;

    //StoppedTimeLeftText.text = string.Format("{0:F2}", guiCounter);

    TimeStopLeftHolder.gameObject.SetActive(true);

    float secondPassed = 0.0f;

    float radialFillValue = 1.0f;

    int secondsPassed = 0;
    while (secondsPassed < Constants.StoppedTimeDuration)
    {
      secondPassed += Time.unscaledDeltaTime;
      if (secondPassed > 1.0f)
      {
        secondPassed = 0.0f;
        secondsPassed++;
      }

      radialFillValue = 1.0f - (((float)Constants.StoppedTimeDuration - guiCounter) / (float)Constants.StoppedTimeDuration);
      TimeStopClockIcon.fillAmount = radialFillValue;

      guiCounter -= Time.unscaledDeltaTime;

      guiCounter = Mathf.Clamp(guiCounter, 0.0f, (float)Constants.StoppedTimeDuration);

      //StoppedTimeLeftText.text = string.Format("{0}", Constants.StoppedTimeDuration - secondsPassed);

      yield return null;
    }

    _timeStopped = false;

    TimeStopLeftHolder.gameObject.SetActive(false);

    var coro = TimeResumeRoutine();
    _timeResumeCoro = StartCoroutine(coro);

    yield return null;
  }

  // ===========================================================================

  Coroutine _stopTimeCoro;
  bool _timeIsStopping = false;
  IEnumerator StopTimeRoutine()
  {
    ToggleAudioFilters();

    _timeIsStopping = true;

    ZaWarudoAnimator.Play("za-warudo");
    SoundManager.Instance.PlaySound("time-stop", 0.2f, 1.0f, false, true);
    Time.timeScale = 0.0f;

    yield return new WaitForSecondsRealtime(2.0f);

    _timeIsStopping = false;

    ZaWarudoAnimator.Play("za-warudo-idle");

    var coro = TimeHasStoppedRoutine();
    _timeStoppedCoro = StartCoroutine(coro);

    yield return null;
  }

  // ===========================================================================

  Coroutine _timeResumeCoro;
  bool _timeResumeWorking = false;
  Color _cloudsColor = Color.white;
  IEnumerator TimeResumeRoutine()
  {
    _timeResumeWorking = true;

    ToggleAudioFilters();

    SoundManager.Instance.PlaySound("time-resume", 0.2f, 1.0f, false, true);

    float cloudsAlpha = Constants.CloudsAlpha;
    _cloudsColor.a = cloudsAlpha;

    while (Time.timeScale < 1.0f)
    {
      cloudsAlpha = Constants.CloudsAlpha - Mathf.Lerp(0.0f, Constants.CloudsAlpha, Time.timeScale);
      _cloudsColor.a = cloudsAlpha;
      Clouds.color = _cloudsColor;

      Time.timeScale += Time.unscaledDeltaTime;
      yield return null;
    }

    _cloudsColor.a = 0.0f;

    Clouds.color = _cloudsColor;

    Time.timeScale = 1.0f;

    _timeResumeWorking = false;

    yield return null;
  }

  // ===========================================================================

  void ZaWarudo()
  {
    var coro = StopTimeRoutine();
    _stopTimeCoro = StartCoroutine(coro);
  }

  // ===========================================================================

  bool _theWorldReady = false;
  public void StopTimeClickHandler()
  {
    if (!_gameStarted || _isGameOver || _timeIsStopping)
    {
      return;
    }

    if (!_timeStopped && _theWorldReady)
    {
      _timeStopped = true;
      _theWorldReady = false;
      _timerTheWorld = 0.0f;
      TheWorldReadyBorder.gameObject.SetActive(false);
      TheWorldReadyImage.fillAmount = 0.0f;
      ZaWarudo();
    }
  }

  // ===========================================================================

  void RechargeTheWorld()
  {
    _theWorldReady = true;
    TheWorldReadyBorder.gameObject.SetActive(true);
  }

  // ===========================================================================

  float GetNormalizedValue(float value, float min, float max)
  {
    return (value - min) / (max - min);
  }

  // ===========================================================================

  float _timer = 0.0f;
  float _timerBad = 0.0f;
  float _timerTroll = 0.0f;
  float _timerTheWorld = 0.0f;

  int _iterations = 0;

  void Update()
  {
    if (!_gameStarted || _isGameOver || _timeIsStopping)
    {
      return;
    }

#if UNITY_EDITOR
    if (Input.GetKeyDown(KeyCode.Space))
    {
      //SpawnTrollStars();
      TrollPrefab.TrollPlayer(0.0f);
    }

    if (!_timeStopped && Input.GetKeyDown(KeyCode.T))
    {
      _timeStopped = true;
      ZaWarudo();
    }

    if (Input.GetKeyDown(KeyCode.S))
    {
      SpawnStar(false);
    }
#endif

    if (!_clockRewinding)
    {
      _spawnTimeout -= Time.smoothDeltaTime * Constants.SpawnTimeoutDecrementScale;

      _spawnTimeout = Mathf.Clamp(_spawnTimeout,
                                Constants.SpawnTimeoutMax,
                                Constants.SpawnTimeoutInit);

      _spawnRateNormalized = GetNormalizedValue(_spawnTimeout,
                                                 Constants.SpawnTimeoutMax,
                                                 Constants.SpawnTimeoutInit);

      SpawnMeter.fillAmount = 1.0f - _spawnRateNormalized;
      SpawnMeterSlider.value = 1.0f - _spawnRateNormalized;
    }

    float t = (1.0f - _spawnRateNormalized);
    _trollTimeout = Mathf.Lerp(Constants.TrollTimeoutInit,
                               Constants.TrollTimeoutMax,
                               t);

    CheckMouse();

    _timer += Time.smoothDeltaTime;
    _timerBad += Time.smoothDeltaTime;
    _timerTroll += Time.smoothDeltaTime;
    _timerTheWorld += Time.smoothDeltaTime;

    if (_timer > _spawnTimeout)
    {
      _iterations++;

      SpawnStar(false);
      _timer = 0.0f;
    }

    if (_timerTroll > _trollTimeout)
    {
      float x = Random.Range(_spawnX.Key, _spawnX.Value);
      TrollPrefab.TrollPlayer(x);
      _timerTroll = 0.0f;
    }

    if (_timerBad > _spawnTimeout)
    {
      if (Random.Range(0, 2) == 0)
      {
        SpawnStar(true);
      }

      _timerBad = 0.0f;
    }

    float fill = GetNormalizedValue(_timerTheWorld, 0.0f, Constants.TheWorldRecharge);
    fill = Mathf.Clamp(fill, 0.0f, 1.0f);
    TheWorldReadyImage.fillAmount = fill;

    if (_timerTheWorld > Constants.TheWorldRecharge)
    {
      RechargeTheWorld();
    }
  }
}
