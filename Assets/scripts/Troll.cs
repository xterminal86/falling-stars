using System.Collections;
using UnityEngine;

public class Troll : MonoBehaviour
{
  public Animator AnimatorComponent;

  public SpriteRenderer Trollface;

  public Collider2D ColliderComponent;

  float _posX = 0.0f;
  float _posY = -5.0f;
  float _posYMax = -3.65f;

  float _trollSpeed = 3.0f;

  bool _trolling = false;
  IEnumerator TrollRoutine()
  {
    ColliderComponent.enabled = true;

    Vector3 pos = transform.localPosition;

    pos.x = _posX;

    float y = _posY;
    while (y < _posYMax)
    {
      y += Time.smoothDeltaTime * _trollSpeed;
      y = Mathf.Clamp(y, _posY, _posYMax);

      pos.y = y;

      transform.localPosition = pos;

      yield return null;
    }

    AnimatorComponent.Play("troll-brows");

    yield return new WaitForSeconds(0.5f);

    while (y > _posY)
    {
      y -= Time.smoothDeltaTime * _trollSpeed;
      y = Mathf.Clamp(y, _posY, _posYMax);

      pos.y = y;

      transform.localPosition = pos;

      yield return null;
    }

    AnimatorComponent.Play("troll-idle");

    _mainRef.SpawnTrollStars(_playerLost);

    _trolling = false;

    ColliderComponent.enabled = false;

    gameObject.SetActive(false);

    yield return null;
  }

  bool _playerLost = false;

  Coroutine _trollRoutine;
  public void TrollPlayer(float posX, bool playerLost = false)
  {
    _playerLost = playerLost;

    if (!_trolling && !_isPunched)
    {
      Trollface.flipX = (posX > 1.0f);

      gameObject.SetActive(true);
      _posX = posX;
      _trolling = true;
      _trollRoutine = StartCoroutine(TrollRoutine());
    }
  }

  float _flySpeed = 20.0f;
  float _spinSpeed = 5.0f;

  Vector3 _flyDir = Vector3.zero;

  IEnumerator TrollFlyRoutine()
  {
    ColliderComponent.enabled = false;

    float dirX = Random.Range(1.0f, 4.0f);

    int dirSign = (Random.Range(0, 2) == 0) ? 1 : -1;

    dirX *= dirSign;

    _flyDir.x = dirX;
    _flyDir.y = 1.0f;

    _flyDir.Normalize();

    Vector3 pos = transform.localPosition;

    Vector3 rotation = transform.eulerAngles;

    bool spinCCW = (Random.Range(0, 2) == 0);

    while (pos.y < _mainRef.SpawnY)
    {
      pos.x += _flyDir.x * _flySpeed * Time.smoothDeltaTime;
      pos.y += _flyDir.y * _flySpeed * Time.smoothDeltaTime;

      if (spinCCW)
      {
        rotation.z += _spinSpeed; //_spinSpeed * Time.smoothDeltaTime;
      }
      else
      {
        rotation.z += -_spinSpeed; //_spinSpeed * -Time.smoothDeltaTime;
      }

      bool switchDir = (pos.x < _mainRef.Borders.Key && _flyDir.x < 0.0f)
                     || (pos.x > _mainRef.Borders.Value && _flyDir.x > 0.0f);

      if (switchDir)
      {
        _flyDir.x *= -1;
      }

      transform.position = pos;
      transform.eulerAngles = rotation;

      yield return null;
    }

    AnimatorComponent.Play("troll-idle");

    gameObject.SetActive(false);

    pos.x = _posX;
    pos.y = _posY;

    rotation.z = 0.0f;

    transform.position    = pos;
    transform.eulerAngles = rotation;

    _isPunched = false;

    yield return null;
  }

  Coroutine _flyRoutine;

  bool _isPunched = false;
  public void PunchTroll()
  {
    if (!_isPunched)
    {
      if (_trolling)
      {
        StopCoroutine(_trollRoutine);
        _trolling = false;
      }

      SoundManager.Instance.PlaySound("punch", 1.0f, 1.0f, false);

      AnimatorComponent.Play("troll-sad");

      _isPunched = true;
      _flyRoutine = StartCoroutine(TrollFlyRoutine());
    }
  }

  Main _mainRef;
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
  }
}

