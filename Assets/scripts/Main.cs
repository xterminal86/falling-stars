using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;
using UnityEngine.SceneManagement;
// =================================
using TMPro;
using Unity.Burst.CompilerServices;

using PairF = System.Collections.Generic.KeyValuePair<float, float>;

public class Main : MonoBehaviour
{
  int _difficulty = 1;
  int _score = 0;

  public GameObject GroundCellPrefab;
  public GameObject StarPrefab;
  public GameObject MouseClickEffectPrefab;
  public GameObject ExplosionPrefab;

  public Transform ObjectsHolder;

  public Animator ScoreAnimator;

  public TMP_Text ScoreText;
  public TMP_Text DifficultyText;

  public List<Heart> Hearts;
  
  public GameObject StartWindow;
  public GameObject RestartWindow;
  public GameObject TitleText;
  public GameObject BtnInfo;
  public GameObject BtnHighScores;

  public GameObject TouchEffectPrefab;

  public ScreenShake ScreenShaker;
  public Config AppConfig;

  float _spawnTimeout = 0.0f;
    
  public void OnPlusButton()
  {
    _difficulty++;

    _difficulty = Mathf.Clamp(_difficulty, 1, 10);

    DifficultyText.text = _difficulty.ToString();
  }

  public void OnMinusButton()
  {
    _difficulty--;

    _difficulty = Mathf.Clamp(_difficulty, 1, 10);

    DifficultyText.text = _difficulty.ToString();
  }

  public void OnStartButton()
  {
    //StartWindow.SetActive(false);
    TitleText.SetActive(false);
    BtnInfo.SetActive(false);
    BtnHighScores.SetActive(false);

    StartGame();
  }

  public void OnRestartButton()
  {
    RestartWindow.SetActive(false);

    SceneManager.LoadScene("main");
  }

  int _lives = 5;
  public void DecrementLives()
  {
    int guiIndex = Hearts.Count - _lives;
    Hearts[guiIndex].FadeAway();

    _lives--;

    if (_lives == 0)
    {
      _isGameOver = true;
      RestartWindow.SetActive(true);
      SoundManager.Instance.PlaySound("rekt", 0.5f);
      AppConfig.AddHighscore(_score);
    }
  }

  bool _isGameOver = false;
  public bool IsGameOver
  {
    get { return _isGameOver; }
  }

  bool _gameStarted = false;
  void StartGame()
  {
    //float k = (float)_difficulty / 10.0f;
    //_spawnTimeout = Constants.SpawnTimeoutInit - (Constants.SpawnTimeoutInit - 0.5f) * k;
    _spawnTimeout = Constants.SpawnTimeoutInit;

    //Debug.Log("spawn timeout: " + _spawnTimeout);

    _gameStarted = true;
  }

  PairF _borders;
  PairF _spawnX;
  float _spawnY;

  public PairF Borders
  {
    get { return _borders; }
  }

  Vector2 _groundSpan = Vector2.zero;
  void CreateGround()
  {
    float groundPosY = -(Camera.main.orthographicSize - 0.5f);

    Vector3 pos = Vector3.zero;
    for (float x = _groundSpan.x; x < _groundSpan.y; x++)
    {
      pos.x = x;
      pos.y = groundPosY;

      Instantiate(GroundCellPrefab, pos, Quaternion.identity, ObjectsHolder);
    }
  }

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

    groundSpanX = Mathf.Round(groundSpanX) + 2.0f;

    _groundSpan.x = -groundSpanX;
    _groundSpan.y = groundSpanX;

    CreateGround();

    SoundManager.Instance.Initialize();

    DifficultyText.text = _difficulty.ToString();
    ScoreText.text = _score.ToString();

    SpriteRenderer sr = StarPrefab.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
      float spriteWidth = sr.bounds.size.x;
      float offset      = spriteWidth / 2.0f;

      _borders = new PairF(-hBorder + offset,
                           hBorder - offset);
    }

    AppConfig.ReadConfig();
  }

  void SpawnStar()
  {
    int maxType = (int)Constants.StarType.BAD;

    int starType = Random.Range(0, maxType + 1);

    float x = Random.Range(_spawnX.Key, _spawnX.Value);
    float angle = Random.Range(-Constants.StarFallSpreadAngle,
                                 Constants.StarFallSpreadAngle);

    float s = Mathf.Sin(angle * Mathf.Deg2Rad);
    float c = Mathf.Cos(angle * Mathf.Deg2Rad);

    //Debug.Log($"angle = { angle }");
    //Debug.Log($"sin = {s} cos = {c}");

    Vector2 dir = new Vector2(s, -c);

    GameObject go = Instantiate(StarPrefab,
      new Vector3(x, _spawnY, 0.0f),
      Quaternion.identity,
      ObjectsHolder);

    Star star = go.GetComponent<Star>();
    if (star != null)
    {
      star.Init((Constants.StarType)starType, angle, dir, _difficulty);
    }
  }

  IEnumerator CaughtBadStarExplosionRoutine(Vector3 pointOfCreation)
  {
    const float deltaMin = 0.2f;
    const float delta = 0.4f;

    Vector3 randomPos = pointOfCreation;

    float dx = Random.Range(-delta, delta);
    float dy = Random.Range(-delta, delta);

    if (dx < 0) { dx = Mathf.Clamp(dx, -delta, -deltaMin); }
    if (dx > 0) { dx = Mathf.Clamp(dx, deltaMin, delta);   }
    if (dy < 0) { dy = Mathf.Clamp(dy, -delta, -deltaMin); }
    if (dy > 0) { dy = Mathf.Clamp(dy, deltaMin, delta);   }

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

      var go = Instantiate(ExplosionPrefab, randomPos, Quaternion.identity);

      Explosion ec = go.GetComponent<Explosion>();
      if (ec != null)
      {
        ec.Explode(Color.red);
      }

      yield return new WaitForSeconds(0.2f);
    }

    yield return null;
  }

  void InstantiateAtMousePos(GameObject prefab)
  {
    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    wp.z = 0.0f;
    var go = Instantiate(prefab, wp, Quaternion.identity, ObjectsHolder);
    Destroy(go, 3.0f);
  }

  void CheckMouse()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

      int mask = LayerMask.GetMask("stars");

      RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, mask);

      if (hit.collider != null)
      {
        Star s = hit.collider.gameObject.GetComponent<Star>();
        if (s != null)
        {
          Vector3 objPos = hit.collider.gameObject.transform.position;

          var starType = s.GetStarType();
          switch (starType)
          {
            case Constants.StarType.BAD:
              DecrementLives();
              break;

            default:
              _score += Constants.StarScoreByType[starType];
              ScoreText.text = _score.ToString();
              break;
          }

          s.ProcessHit();

          Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
          wp.z = 0.0f;

          if (starType != Constants.StarType.BAD)
          {
            var go = Instantiate(MouseClickEffectPrefab, objPos, Quaternion.identity, ObjectsHolder);
            Destroy(go, 3.0f);

            float rndPitch = Random.Range(0.6f, 1.4f);
            SoundManager.Instance.PlaySound("pop", 1.0f, rndPitch);
            ScoreAnimator.Play("score-pop", -1, 0.0f);
          }
          else
          {
            int index = Random.Range(1, 5);
            string soundName = string.Format("explode{0}", index);
            SoundManager.Instance.PlaySound(soundName);
            IEnumerator coro = CaughtBadStarExplosionRoutine(objPos);
            StartCoroutine(coro);
            ScreenShaker.ShakeScreen();
          }
        }
      }
      else
      {
        InstantiateAtMousePos(TouchEffectPrefab);
      }
    }
  }

  float _timer = 0.0f;
  void Update()
  {
    if (!_gameStarted || _isGameOver)
    {
      return;
    }

    CheckMouse();

    _timer += Time.smoothDeltaTime;

    if (_timer > _spawnTimeout)
    {
      SpawnStar();
      _timer = 0.0f;
    }
  }
}
