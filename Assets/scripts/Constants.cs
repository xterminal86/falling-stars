using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PairF = System.Collections.Generic.KeyValuePair<float, float>;

public static class Constants
{
  public static readonly float BottomBorder = -4.0f;

  public static readonly float SpawnTimeoutInit = 0.5f;
  public static readonly float StartSpeed = 2.0f;

  public static readonly float StarFallSpreadAngle = 60.0f;

  public enum StarType
  {
    YELLOW = 0,
    CYAN,
    GREEN,
    SILVER,
    BAD
  };

  public static readonly Dictionary<StarType, Color> StarColorsByType = new Dictionary<StarType, Color>()
  {
    { StarType.YELLOW, new Color(1.0f, 1.0f, 0.0f, 1.0f) },
    { StarType.CYAN,   new Color(0.0f, 1.0f, 1.0f, 1.0f) },
    { StarType.GREEN,  new Color(0.0f, 1.0f, 0.0f, 1.0f) },
    { StarType.SILVER, new Color(1.0f, 1.0f, 1.0f, 1.0f) },
    { StarType.BAD,    new Color(1.0f, 0.0f, 0.0f, 1.0f) }
  };

  public static readonly Dictionary<StarType, int> StarScoreByType = new Dictionary<StarType, int>()
  {
    { StarType.YELLOW, 6 },
    { StarType.CYAN,   4 },
    { StarType.GREEN,  2 },
    { StarType.SILVER, 1 }
  };
}
