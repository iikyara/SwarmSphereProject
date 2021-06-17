using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PercentField : FieldValue
{
  public InputField Field;

  public override object GetField()
  {
    float value = 0f;
    try
    {
      int v_int = int.Parse(Field.text);
      value = (float)v_int / 100f;
    }
    catch
    {
      Debug.Log("Int Parse Error : " + Field.text);
    }

    return value;
  }

  public override void SetField(object value)
  {
    Field.text = ((float)value * 100).ToString();
  }
}
