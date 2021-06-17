using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntField : FieldValue
{
  public InputField Field;
  
  public override object GetField()
  {
    int value = -1;
    try
    {
      value = int.Parse(Field.text);
    }
    catch
    {
      Debug.Log("Int Parse Error : " + Field.text);
    }
    
    return value;
  }

  public override void SetField(object value)
  {
    Field.text = value.ToString();
  }
}
