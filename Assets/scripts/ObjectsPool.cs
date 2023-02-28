using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsPool : MonoBehaviour
{
  [Range(1, 1000)]
  public int PoolSize = 100;

  public GameObject Prefab;

  Queue<GameObject> _pool = new Queue<GameObject>();
  Dictionary<int, GameObject> _activeObjectsById = new Dictionary<int, GameObject>();

#if UNITY_EDITOR
  public int ActiveObjects
  {
    get
    {
      int activeCount = 0;
      foreach (var kvp in _activeObjectsById)
      {
        if (kvp.Value != null)
        {
          activeCount++;
        }
      }

      return activeCount;
    }
  }
#endif

  Transform _holder;
  void Awake()
  {
    _holder = GetComponent<Transform>();

        for (int i = 0; i < PoolSize; i++)
    {
      GameObject obj = Instantiate(Prefab,
                                   Vector3.zero,
                                   Quaternion.identity,
                                   _holder);
      obj.SetActive(false);
      _pool.Enqueue(obj);
    }
  }

  public GameObject Acquire(Vector3 moveTo)
  {
    if (_pool.Count != 0)
    {
      var go = _pool.Dequeue();
      go.transform.localPosition = moveTo;
      go.SetActive(true);

      go.GetComponent<IPoolObject>()?.Prepare();

      _activeObjectsById[go.GetInstanceID()] = go;

      return _activeObjectsById[go.GetInstanceID()];
    }

    return null;
  }

  public void Return(GameObject objectToReturn)
  {
    if (objectToReturn == null)
    {
      Debug.LogWarning("Trying to return null to pool - doing nothing");
      return;
    }

    int objectId = objectToReturn.GetInstanceID();

    if (_activeObjectsById.ContainsKey(objectId))
    {
      _activeObjectsById[objectId].SetActive(false);

      IPoolObject po = _activeObjectsById[objectId].GetComponent<IPoolObject>();
      po.ResetState();

      _pool.Enqueue(_activeObjectsById[objectId]);
      _activeObjectsById.Remove(objectId);
    }
  }

  IEnumerator DeactivateRoutine(GameObject obj, float seconds)
  {
    yield return new WaitForSeconds(seconds);

    Return(obj);

    yield return null;
  }

  public void Return(GameObject objectToReturn,
                     float deactivateAfterSeconds)
  {
    StartCoroutine(DeactivateRoutine(objectToReturn, deactivateAfterSeconds));
  }
}
