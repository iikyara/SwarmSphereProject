using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アタッチしたオブジェクトをワイヤーフレームで描画する．
/// 若干辺が減ることがあるが気にするな！
/// </summary>
public class Wireframe : MonoBehaviour
{
  /// <summary>
  /// つけたいマテリアル
  /// 設定しなかったらデフォルトのまま
  /// </summary>
  public Material mat;
  // Start is called before the first frame update
  void Start()
  {
    MeshFilter meshFilter = GetComponent<MeshFilter>();
    meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Lines, 0);
    if(mat != null)
    {
      GetComponent<MeshRenderer>().material = mat;
    }
  }
}
