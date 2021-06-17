using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エージェントの基底クラス
/// 当初は球体エージェントを想定していたためSizeパラメータが存在するが，現在は使っていない．
/// Particle Objectへのサイズ指定に使っているぐらい．
/// </summary>
public abstract class Agent : System.ICloneable
{
  /// <summary>
  /// エージェント名
  /// </summary>
  public string Name;
  /// <summary>
  /// エージェントが存在する位置
  /// </summary>
  public Vector3 Position;
  /// <summary>
  /// エージェントの大きさ
  /// </summary>
  public float Size;

  /// <summary>
  /// デフォルトコンストラクタ
  /// </summary>
  public Agent() : this("Agent", new Vector3(), 1.0f) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="name">エージェント名</param>
  public Agent(string name) : this(name, new Vector3(), 1.0f) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="name">エージェント名</param>
  /// <param name="position">エージェントが存在する位置</param>
  /// <param name="size">エージェントの大きさ</param>
  public Agent(
    string name,
    Vector3 position,
    float size
  )
  {
    this.Name = name;
    this.Position = position;
    this.Size = size;
  }

  /// <summary>
  /// エージェントの状態を更新する
  /// </summary>
  public abstract void Update();

  /// <summary>
  /// エージェントの状態をリセットする
  /// </summary>
  public abstract void Reset();

  /// <summary>
  /// エージェントを複製する（ディープコピー）
  /// </summary>
  /// <returns></returns>
  public abstract object Clone();
}
