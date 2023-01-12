using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
  Vector3 _cameraPos = new Vector3(0.0f, 0.0f, -10.0f);

  IEnumerator ShakeScreenRoutine()
  {
    float secondsPassed = 0.0f;
    while (secondsPassed < 0.5f)
    {
      float dx = Random.Range(-0.1f, 0.1f);
      float dy = Random.Range(-0.1f, 0.1f);

      _cameraPos.x = dx;
      _cameraPos.y = dy;

      Camera.main.transform.position = _cameraPos;

      secondsPassed += Time.smoothDeltaTime;

      yield return null;
    }

    Camera.main.transform.position = new Vector3(0.0f, 0.0f, -10.0f);

    yield return null;
  }

  public void ShakeScreen()
  {
    StopCoroutine(ShakeScreenRoutine());
    StartCoroutine(ShakeScreenRoutine());
  }
}
