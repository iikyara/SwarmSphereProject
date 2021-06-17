using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 複数のスケッチを管理するクラス
/// 各スケッチのカメラ情報もSIPで保持する．
/// </summary>
public class SketchManager
{
  /// <summary>
  /// スケッチのリスト
  /// </summary>
  public List<DrawableSketch> Sketches;

  /// <summary>
  /// 現在のスケッチ
  /// </summary>
  public DrawableSketch CurrentSketch;

  /// <summary>
  /// デフォルトコンストラクタ
  /// </summary>
  public SketchManager()
  {
    this.Sketches = new List<DrawableSketch>();
  }

  /// <summary>
  /// スケッチを追加する．
  /// </summary>
  /// <param name="sip">視点情報</param>
  /// <returns>追加したスケッチの参照</returns>
  public DrawableSketch AddSketch(SketchAndCameraInitializationParam scip)
  {
    //スケッチ追加
    DrawableSketch newSketch = new DrawableSketch(scip);
    Sketches.Add(newSketch);
    return newSketch;
  }

  /// <summary>
  /// 対象スケッチを切り替える
  /// </summary>
  /// <param name="sketch">切り替えるスケッチ</param>
  /// <returns>切り換え先のスケッチ</returns>
  public DrawableSketch SwitchSketch(DrawableSketch sketch)
  {
    this.CurrentSketch = sketch;
    return sketch;
  }

  /// <summary>
  /// 指定されたスケッチを削除する
  /// </summary>
  /// <param name="sketch">削除するスケッチ</param>
  public void RemoveSketch(DrawableSketch sketch)
  {

  }

}
