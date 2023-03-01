using UnityEngine;

public class Explosion : MonoBehaviour
{
  public SpriteRenderer SpriteRenderer_;

  Main _mainRef;
  Animator _animator;
  void Awake()
  {
    _animator = GetComponent<Animator>();

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

  public void Explode(Color c)
  {
    _animator.Play("explosion");

    SpriteRenderer_.color = c;

    _mainRef.ExplosionsPool.Return(gameObject, 3.0f, true);
  }
}
