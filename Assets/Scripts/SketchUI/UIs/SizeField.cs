using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeField : FieldValue
{
  public IntField WidthField;
  public IntField HeightField;

  public override object GetField()
  {
    return new Vector2Int((int)WidthField.GetField(), (int)HeightField.GetField());
  }

  public override void SetField(object value)
  {
    WidthField.SetField(((Vector2Int)value).x);
    HeightField.SetField(((Vector2Int)value).y);
  }
}
