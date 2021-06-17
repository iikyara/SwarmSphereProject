using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkCameraSet : MonoBehaviour
{
  public List<BenchmarkCamera> CameraList;

  private void Awake()
  {
    CameraList = new List<BenchmarkCamera>();
    foreach (Transform ct in this.GetComponentInChildren<Transform>())
    {
      var cgo = ct.gameObject;
      if (cgo)
      {
        var bc = cgo.GetComponent<BenchmarkCamera>();
        if (bc != null) CameraList.Add(bc);
      }
    }
  }

  /// <summary>
  /// 全てのカメラで対象オブジェクトを撮影する
  /// </summary>
  public SketchInitilizationParam[] CaptureAllCamera(GTData data)
  {
    SketchInitilizationParam[] sips = new SketchInitilizationParam[CameraList.Count];
    for (int i = 0; i < sips.Length; i++)
    {
      sips[i] = CameraList[i].Capture(data);
    }
    return sips;
  }

  public override string ToString()
  {
    string result =
      $"CameraSet : {this.CameraList[0]}";
    for(int i = 1; i < this.CameraList.Count; i++)
    {
      result += "|" + this.CameraList[i].Name;
    }
    return result + base.ToString();
  }

  public string GetCSVHeader()
  {
    string header = $"CameraSet";
    return header;
  }

  public string ToCSV()
  {
    string csv = $"{this.CameraList[0].Name}";
    for (int i = 1; i < this.CameraList.Count; i++)
    {
      csv += "|" + this.CameraList[i].Name;
    }
    return csv;
  }
}
