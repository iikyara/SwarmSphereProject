using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveField : FieldValue
{
  public Dropdown Field;

  public override object GetField()
  {
    CameraProjection value = (CameraProjection)Field.value;
    Debug.Log(value);
    return value;
  }

  public override void SetField(object value)
  {
    Field.value = (int)(CameraProjection)value;
  }
}
