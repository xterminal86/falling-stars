using System.Collections;
using System.Collections.Generic;
// =================================
using UnityEngine;

public static class Constants
{
  public static readonly float BottomBorder = -4.0f;

  public static readonly float TrollTimeoutInit = 30.0f;
  public static readonly float TrollTimeoutMax = 5.0f;
  public static readonly float SpawnTimeoutInit = 2.0f;
  public static readonly float SpawnTimeoutMax = 0.25f;
  public static readonly float SpawnTimeoutDecrementScale = 0.0125f;
  public static readonly float StartSpeed = 2.0f;
  public static readonly float CloudsAlpha = 0.1f;
  public static readonly float StarFallSpreadAngle = 60.0f;

  public static readonly int StoppedTimeDuration = 8;
  public static readonly float TheWorldRecharge = 30.0f;

  public enum StarType
  {
    YELLOW = 0,
    CYAN,
    GREEN,
    SILVER,
    BAD
  };

  public enum StarTrajectory
  {
    LINE = 0,
    WAVE,
    CIRCLE
  }

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
    { StarType.YELLOW, 4 },
    { StarType.CYAN,   3 },
    { StarType.GREEN,  2 },
    { StarType.SILVER, 1 }
  };

  public static readonly Dictionary<StarType, float> StarSpeedScaleByType = new Dictionary<StarType, float>()
  {
    { StarType.YELLOW, 2.0f  },
    { StarType.CYAN,   1.75f },
    { StarType.GREEN,  1.25f },
    { StarType.SILVER, 1.0f  }
  };

  public static readonly Dictionary<StarType, float> StarAdditionalScoreMultiplierByType = new Dictionary<StarType, float>()
  {
    { StarType.YELLOW, 1.3f },
    { StarType.CYAN,   1.2f },
    { StarType.GREEN,  1.1f },
    { StarType.SILVER, 1.0f }
  };
}
