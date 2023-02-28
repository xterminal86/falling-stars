using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickEffect : MonoBehaviour, IPoolObject
{
  Animator _animator;
  void Awake()
  {
    _animator = GetComponent<Animator>();
  }

  public void Prepare()
  {
    _animator.Play("mouse-click");
  }

  public void ResetState()
  {
  }
}
