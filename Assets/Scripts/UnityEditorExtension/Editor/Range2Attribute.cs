using UnityEngine;

public class Range2Attribute : PropertyAttribute
{
  public float min;
  public float max;

  public Range2Attribute(float min, float max)
  {
    this.min = min;
    this.max = max;
  }
}
