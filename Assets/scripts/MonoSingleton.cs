using UnityEngine;
using System.Collections;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
  static T _instance = null;

  static bool _instantiated = false;

  public static T Instance
  {
    get
    {
      if (_instantiated)
      {
        return _instance;
      }

      if (_instance == null)
      {
        bool error = false;

        _instance = GameObject.FindObjectOfType(typeof(T)) as T;

        if (_instance == null)
        {
          string path = string.Format("{0}", typeof(T));

          T resource = Resources.Load<T>(path);

          if (resource != null)
          {
            _instance = Instantiate(resource) as T;
            _instance.name = typeof(T).ToString() + ".Singleton";
          }

          if (_instance == null)
          {
            _instance = new GameObject(typeof(T).ToString() + ".Singleton", typeof(T)).GetComponent<T>();

            if (_instance == null)
            {
              Debug.LogError("Problem during the creation of " + typeof(T).ToString());
              error = true;
            }
          }
        }

        if (!error)
        {
          _instantiated = true;

          DontDestroyOnLoad(_instance.gameObject);
        }
      }

      return _instance;
    }
  }

  void Awake()
  {
    if (Instance != null)
    {
      Instance.Init();
    }
  }

  /// <summary>
  /// Some post Awake initialization (called once)
  /// </summary>
  protected virtual void Init()
  {
  }

  /// <summary>
  /// When user types MySingleton.Instance.Initialize() the Instance property
  /// will be called, which does the actual initialization.
  /// </summary>
  public virtual void Initialize()
  {
  }

  protected void OnDestroy()
  {
    _instantiated = false;
  }

  public static bool isInstantinated { get { return _instantiated; } }
}