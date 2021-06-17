using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenSetManager : MonoBehaviour
{
  public List<PenSet> Pens;

  // Start is called before the first frame update
  void Start()
  {
    LoadPenTexture();
  }

  /// <summary>
  /// ペンのテクスチャを読み込み，ペンのセットを作成する．
  /// </summary>
  public void LoadPenTexture()
  {
    
  }
}