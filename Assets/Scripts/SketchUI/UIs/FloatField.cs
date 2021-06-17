using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatField : FieldValue
{
  public InputField Field;

  public override object GetField()
  {
    float value = -1;
    try
    {
      value = float.Parse(Field.text);
    }
    catch
    {
      Debug.Log("Float Parse Error : " + Field.text);
    }

    return value;
  }

  public override void SetField(object value)
  {
    Field.text = value.ToString();
  }
}
