using UnityEngine;

public class Explosion : MonoBehaviour
{
  public SpriteRenderer SpriteRenderer_;

  public void Explode(Color c)
  {
    SpriteRenderer_.color = c;
    SpriteRenderer_.enabled = true;

    Destroy(gameObject, 3.0f);
  }
}
