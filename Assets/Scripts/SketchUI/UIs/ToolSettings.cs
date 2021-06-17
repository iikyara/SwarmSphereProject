using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolSettings : MonoBehaviour
{
  /// <summary>
  /// UIManagerインスタンス
  /// </summary>
  public UIManager UIManager;

  /* ツール設定のゲームオブジェクト */
  /* Brush */
  public FieldValue Brush_Color_EL;
  public FieldValue Brush_Transparency_EL;
  public FieldValue Brush_Thick_EL;
  public FieldValue Brush_PointInterval_EL;

  /* Eracer */
  public FieldValue Eracer_Transparency_EL;
  public FieldValue Eracer_Thick_EL;

  /* Bucket */
  public FieldValue Bucket_Color_EL;
  public FieldValue Bucket_Transparency_EL;

  //ペンのタイプに対応した設定オブジェクトを保持しておく
  private Dictionary<PenType, GameObject> SettingObjects;

  // Start is called before the first frame update
  void Start()
  {
    //ボタンのコピー元オブジェクトを辞書に格納
    this.SettingObjects = new Dictionary<PenType, GameObject>();
    foreach (Transform childTransform in this.transform)
    {
      GameObject child = childTransform.gameObject;
      (bool s, PenType pt) = Utils.StringToEnum<PenType>(typeof(PenType), child.name);
      if (!s) continue;
      this.SettingObjects.Add(pt, child);
    }
  }
  /* フィールド更新用メソッド */
  /// <summary>
  /// ペンタイプに応じた設定画面に変更する
  /// </summary>
  /// <param name="pt">ペンタイプ</param>
  public void ChangeToolSetting(PenType pt)
  {
    //Debug.Log("設定画面を更新 - PenType : " + pt);
    InactivateAllChild();
    this.SettingObjects[pt].SetActive(true);
  }

  public void UpdateValueOfAllField(object sketchPenInstance)
  {
    if(sketchPenInstance is Brush)
    {
      Brush pen = (Brush)sketchPenInstance;
      Brush_Color_EL.SetField(pen.Color);
      Brush_Transparency_EL.SetField(pen.Transparency);
      Brush_Thick_EL.SetField(pen.Thick);
      Brush_PointInterval_EL.SetField(pen.PointInterval);
    }
    else if(sketchPenInstance is Eracer)
    {
      Eracer pen = (Eracer)sketchPenInstance;
      Eracer_Transparency_EL.SetField(pen.Transparency);
      Eracer_Thick_EL.SetField(pen.Thick);
    }
    else if(sketchPenInstance is Bucket)
    {
      Bucket pen = (Bucket)sketchPenInstance;
      Bucket_Color_EL.SetField(pen.Color);
      Bucket_Transparency_EL.SetField(pen.Transparency);
    }
  }

  /* フィールドイベントリスナー */
  public void OnChange_AnySetting()
  {
    GameObject e = EventSystem.current.currentSelectedGameObject;
    Debug.Log("OnChange: " + e.name);
  }

  public void OnChange_Brush_Color()
  {
    this.UIManager.OnChange_ToolSettings_Any((Brush_Color_EL.Name, Brush_Color_EL.GetField()));
  }

  public void OnChange_Brush_Transparency()
  {
    this.UIManager.OnChange_ToolSettings_Any((Brush_Transparency_EL.Name, Brush_Transparency_EL.GetField()));
  }

  public void OnChange_Brush_Thick()
  {
    this.UIManager.OnChange_ToolSettings_Any((Brush_Thick_EL.Name, Brush_Thick_EL.GetField()));
  }

  public void OnChange_Brush_PointInterval()
  {
    this.UIManager.OnChange_ToolSettings_Any((Brush_PointInterval_EL.Name, Brush_PointInterval_EL.GetField()));
  }

  public void OnChange_Eracer_Transparency()
  {
    this.UIManager.OnChange_ToolSettings_Any((Eracer_Transparency_EL.Name, Eracer_Transparency_EL.GetField()));
  }

  public void OnChange_Eracer_Thick()
  {
    this.UIManager.OnChange_ToolSettings_Any((Eracer_Thick_EL.Name, Eracer_Thick_EL.GetField()));
  }

  public void OnChange_Bucket_Color()
  {
    this.UIManager.OnChange_ToolSettings_Any((Bucket_Color_EL.Name, Bucket_Color_EL.GetField()));
  }

  public void OnChange_Bucket_Transparency()
  {
    this.UIManager.OnChange_ToolSettings_Any((Bucket_Transparency_EL.Name, Bucket_Transparency_EL.GetField()));
  }

  /* プライベートメソッド */
  private void InactivateAllChild()
  {
    foreach (Transform childTransform in this.transform)
    {
      GameObject child = childTransform.gameObject;
      child.SetActive(false);
    }
  }
}
