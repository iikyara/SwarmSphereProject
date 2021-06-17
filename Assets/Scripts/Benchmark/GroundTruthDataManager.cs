using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTruthDataManager : MonoBehaviour
{
  public List<GTData> DataList;

  private void Start()
  {
    DataList = new List<GTData>();
    foreach(var cgo in Utils.GetAllChildren(this.transform.gameObject))
    {
      if (!cgo.activeSelf) continue;
      var data = GTData.Create(cgo);
      if (data != null) DataList.Add(data);
    }
  }
}

public class MeshData
{
  public Mesh Mesh;

  public MeshData()
  {
    Mesh = null;
  }
}

/// <summary>
/// 真値のメッシュデータ
/// </summary>
public class GTData : MeshData
{
  /// <summary>
  /// ラベル
  /// </summary>
  public string Name;

  public GameObject GameObject;

  public GTData() : base()
  {
    GameObject = null;
  }

  public static GTData Create(GameObject go)
  {
    var gtdata = new GTData();

    gtdata.Name = go.name;
    gtdata.GameObject = go;

    var mf = go.GetComponent<MeshFilter>();
    if (!(mf && mf.sharedMesh)) return null;

    gtdata.Mesh = mf.sharedMesh;

    return gtdata;
  }

  public override string ToString()
  {
    string str =
      $"GTLabel : {Name}\n";
    return str;
  }

  public string GetCSVHeader()
  {
    return
      $"GTLabel";
  }

  public string ToCSV()
  {
    return
      $"{Name}";
  }
}

/// <summary>
/// 生成されたメッシュデータ
/// </summary>
public class GData : MeshData
{
  public GData(Mesh mesh) : base()
  {
    this.Mesh = mesh;
  }
}