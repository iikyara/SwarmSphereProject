using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 見た目に関わることを頑張るクラス
/// ボタン等のUIの生成
/// スケッチオブジェクトの生成
/// イベントの管理をする
/// イベントはコントローラに通知される．
/// </summary>
public class UIManager : MonoBehaviour, ISketchSpace
{
  public SketchViewManager SketchViewManager;
  public SketchZone SketchZone;
  public ToolListView ToolListView;
  public ToolSettings ToolSettings;
  public SketchSpace SketchSpace;
  public CameraSetting CameraSetting;
  public SketchSetting SketchSetting;
  public ModeChange ModeChange;

  public SketchController SketchController;

  private void Start()
  {
    //自分のインスタンスを渡す
    SketchSpace.UIManager = this;
  }

  /// <summary>
  /// スケッチを生成
  /// </summary>
  public void CreateSketch()
  {
    //スケッチの生成
  }

  /// <summary>
  /// 描画ツールの種類に応じて動的にツールリストを生成する
  /// </summary>
  /// <param name="pts"></param>
  public void LoadToolList(List<PenType> pts)
  {
    this.ToolListView.LoadToolList(pts);
  }

  /* 更新メソッド */

  public void UpdateView_AddSketch(DrawableSketch dSketch)
  {
    SketchViewManager.AddSketch(dSketch);
  }

  public void UpdateView_RemoveSketch(DrawableSketch dSketch)
  {
    SketchViewManager.RemoveSketch(dSketch);
  }

  public void UpdateView_ApplySketch(DrawableSketch dSketch)
  {
    SketchViewManager.ApplySketchChange(dSketch);
  }

  /// <summary>
  /// 描画オブジェクトの種類に応じてツール設定の表示を変更する．
  /// </summary>
  /// <param name="penMeta">描画オブジェクト</param>
  public void UpdateView_ToolSettings(IPen penMeta)
  {
    this.ToolSettings.ChangeToolSetting(penMeta.PenType);
  }

  /// <summary>
  /// ツール設定の値を更新する
  /// </summary>
  /// <param name="sketchPenInstance"></param>
  public void UpdateValue_ToolSettings(object sketchPenInstance)
  {
    this.ToolSettings.UpdateValueOfAllField(sketchPenInstance);
  }

  /// <summary>
  /// スケッチを更新する．
  /// </summary>
  public void UpdateView_SketchZone(DrawableSketch sketch)
  {
    this.SketchZone.UpdateView(sketch);
  }

  /// <summary>
  /// UIのモードをスケッチに変える
  /// </summary>
  public void ChangeMainMode_Sketch(DrawableSketch sketch)
  {
    this.SketchZone.SetActive(true);
    this.SketchViewManager.ChangeToSketchMode(sketch);
  }

  /// <summary>
  /// UIのモードを3DViewに変える
  /// </summary>
  public void ChangeMainMode_Generate()
  {
    this.SketchZone.SetActive(false);
    this.SketchViewManager.ChangeToGenerateMode();
  }

  public void UpdateValue_CameraSetting(SketchAndCameraInitializationParam cip)
  {
    this.CameraSetting.SetValue(cip);
  }

  public void UpdateValue_SketchSetting(DrawableSketch dSketch)
  {
    this.SketchSetting.UpdateValues(dSketch);
  }

  /* ここからイベントビューワ */
  public void OnClick_SketchView(DrawableSketch dSketch)
  {
    this.SketchController.ClickSketchView(dSketch);
  }

  public void OnClick_ToolList_AnyButton(int penId)
  {
    this.SketchController.ClickToolListButton(penId);
  }

  public void OnChange_ToolSettings_Any((string, object) setting)
  {
    this.SketchController.ChangeCurrentSketchPenSetting(setting);
  }

  public void SketchSpace_MouseDown(Vector3 mousePos, SketchMouse sm)
  {
    this.SketchController.MouseDown(mousePos, sm);
  }

  public void SketchSpace_MouseUp(Vector3 mousePos, SketchMouse sm)
  {
    this.SketchController.MouseUp(mousePos, sm);
  }

  public void SketchSpace_MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    this.SketchController.MouseDrag(preMousePos, curMousePos, sm);
  }

  public void SketchSpace_MouseWheel(Vector2 delta)
  {
    this.SketchController.MouseWheel(delta);
  }

  public void CameraSetting_ChangeProjection(CameraProjection cp)
  {
    this.SketchController.ChangeCameraProjection(cp);
  }

  public void CameraSetting_ChangeFOV(float fov)
  {
    this.SketchController.ChangeCameraFOV(fov);
  }

  public void CameraSetting_ChangeSize(float size)
  {
    this.SketchController.ChangeCameraSize(size);
  }

  public void CameraSetting_ClickToDefaultButton()
  {
    this.SketchController.ClickToDefaultCamera();
  }

  public void SketchSetting_ChangeBGColor(Color bGColor)
  {
    this.SketchController.ChangeSketchBGColor(bGColor);
  }

  public void SketchSetting_ChangeBGTransparency(float bGTransparency)
  {
    this.SketchController.ChangeSketchBGTransparency(bGTransparency);
  }

  public void SketchSetting_ChangeModelTransparency(float modelTransparency)
  {
    this.SketchController.ChangeSketchModelTransparency(modelTransparency);
  }

  public void SketchSetting_ClickNewCanvasButton(SketchAndCameraInitializationParam sip)
  {
    this.SketchController.ClickNewCanvasButton(sip);
  }

  public void OnClick_Generate3DModelButton()
  {
    SketchController.ClickGenerateModelButton();
  }

  public void OnClick_ChangeModeButton()
  {
    this.SketchController.ClickChangeModeButton();
  }
}
