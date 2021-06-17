using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorField : FieldValue
{
  public InputField Field;

  public override object GetField()
  {
    return Utils.ColorCodeToColor(Field.text);
  }

  public override void SetField(object value)
  {
    Color newColor = (Color)value;
    Field.text = Utils.ColorToColorCode(newColor);
  }
}
