using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolButton : MonoBehaviour
{
  /// <summary>
  /// ツールリストの
  /// </summary>
  public ToolListView ToolListView;
  /// <summary>
  /// 対象とするペンのID
  /// </summary>
  public int PenId;

  private void Start()
  {

  }

  /// <summary>
  /// ボタンがクリックされたときの処理
  /// </summary>
  public void OnClick()
  {
    this.ToolListView.OnClick_AnyToolButton(this.PenId);
  }
}
