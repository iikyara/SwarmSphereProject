using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModelGenerater : MonoBehaviour
{
  /// <summary>
  /// モデル生成手法名
  /// </summary>
  public string Name;
  /// <summary>
  /// モデルが実行中かどうか
  /// </summary>
  [HideInInspector]
  public bool IsGenerating;
  /// <summary>
  /// モデルが生成されたかどうか
  /// </summary>
  [HideInInspector]
  public bool IsGenerated;
  /// <summary>
  /// 生成時間
  /// </summary>
  public System.TimeSpan GeneratedTime;

  public abstract void SetParamater(MGP mgp);

  /// <summary>
  /// メッシュ生成を開始する．
  /// </summary>
  public abstract void StartGenerating(Sketch[] sketches);

  /// <summary>
  /// 生成したメッシュを取得する．
  /// </summary>
  /// <returns></returns>
  public abstract Mesh GetGeneratedMesh();

  public override string ToString()
  {
    string str =
      $"ModelGeneraterName : {Name}\n";
    return str + base.ToString();
  }

  public string GetCSVHeader()
  {
    return
      $"ModelGeneraterName";
  }

  public string ToCSV()
  {
    string csv =
      $"{Name}";
    return csv;
  }
}
