using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolListView : MonoBehaviour
{
  /// <summary>
  /// ツールボタンのアイコンリスト
  /// </summary>
  public GameObject ToolButtons;
  /// <summary>
  /// UIManagerインスタンス
  /// </summary>
  public UIManager UIManager;

  private Dictionary<PenType, GameObject> ButtonBaseObjects;

  // Start is called before the first frame update
  void Start()
  {
    //ボタンのコピー元オブジェクトを辞書に格納
    this.ButtonBaseObjects = new Dictionary<PenType, GameObject>();
    foreach(Transform childTransform in this.ToolButtons.transform)
    {
      GameObject child = childTransform.gameObject;
      (bool s, PenType pt) = Utils.StringToEnum<PenType>(typeof(PenType), child.name);
      if (!s) continue;
      this.ButtonBaseObjects.Add(pt, child);
    }
  }

  /// <summary>
  /// ツールリストを生成する
  /// </summary>
  /// <param name="pts">描画オブジェクトの種類</param>
  public void LoadToolList(List<PenType> pts)
  {
    for(int i = 0; i < pts.Count; i++)
    {
      //オブジェクトのコピー
      GameObject newToolButton = Object.Instantiate(ButtonBaseObjects[pts[i]]) as GameObject;
      newToolButton.name = "ToolButton";
      //親の変更
      Utils.SetParent(this.transform.gameObject, newToolButton);
      //IDの更新
      ToolButton tb = newToolButton.GetComponent<ToolButton>();
      tb.PenId = i;
      //ボタンのイベントリスナーを更新
      Button b = newToolButton.GetComponent<Button>();
      b.onClick.AddListener(tb.OnClick);
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  /// <summary>
  /// ボタンクリックのイベントをUIManagerに伝える
  /// </summary>
  /// <param name="penId">どのボタンがクリックされたか</param>
  public void OnClick_AnyToolButton(int penId)
  {
    this.UIManager.OnClick_ToolList_AnyButton(penId);
  }
}
