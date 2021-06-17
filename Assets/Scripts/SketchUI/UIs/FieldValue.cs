using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FieldValue : MonoBehaviour
{
  /// <summary>
  /// フィールド名
  /// </summary>
  public string Name;

  /// <summary>
  /// フィールドの内容をキャストして返す．
  /// </summary>
  /// <returns></returns>
  public abstract object GetField();

  /// <summary>
  /// フィールドの内容を更新する．
  /// </summary>
  /// <param name="value"></param>
  public abstract void SetField(object value);
}
