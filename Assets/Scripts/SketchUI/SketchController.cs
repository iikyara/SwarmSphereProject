using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMode
{
  Generate,
  Sketch
}

public class SketchController : MonoBehaviour
{
  public List<PenType> SketchPens;
  /// <summary>
  /// ビュー
  /// </summary>
  public UIManager UIManager;
  /// <summary>
  /// スケッチマネージャ
  /// </summary>
  public SketchManager SketchManager;
  /// <summary>
  /// 描画オブジェクトマネージャ
  /// </summary>
  public SketchPenManager SketchPenManager;
  /// <summary>
  /// ペンテクスチャのマネージャ
  /// </summary>
  public PenSetManager PenSetManager;
  /// <summary>
  /// カメラコントローラ
  /// </summary>
  public SketchCameraController SketchCameraController;
  /// <summary>
  /// スケッチモードのモデル
  /// ビューの役割もある（切り離し方が分からなかった…）
  /// </summary>
  public SketchModeController SketchModeController;
  /// <summary>
  /// 3Dモデル生成のコントローラ
  /// </summary>
  public ModelGenerateController ModelGenerateController;
  /// <summary>
  /// 現在のモード
  /// </summary>
  public MainMode MainMode;

  // Start is called before the first frame update
  void Start()
  {
    this.SketchManager = new SketchManager();
    this.SketchPenManager = new SketchPenManager(this.SketchPens, this.PenSetManager);
    this.MainMode = MainMode.Generate;

    //スケッチモードコントローラにインスタンスを設定
    this.SketchModeController.SketchManager = this.SketchManager;
    this.SketchModeController.SketchPenManager = this.SketchPenManager;

    //コンテキストを実行
    Context();

    //全UIを更新
    updateAll();
  }

  /// <summary>
  /// UI生成
  /// </summary>
  void Context()
  {
    //描画ツール選択ボタンを生成
    this.UIManager.LoadToolList(this.SketchPens);

    //初期描画オブジェクトを設定
    updateCurrentTool(this.SketchPenManager.CurrentPenIndex);
  }

  /* イベントビューワ */
  public void ClickSketchView(DrawableSketch dSketch)
  {
    if(MainMode == MainMode.Generate)
    {
      this.SketchManager.CurrentSketch = dSketch;
      modeChange(MainMode.Sketch);
    }
  }

  public void MouseDown(Vector3 mousePos, SketchMouse sm)
  {
    if(MainMode == MainMode.Sketch)
    {
      this.SketchModeController.MouseDown(mousePos, sm);
    }
    else if(MainMode == MainMode.Generate)
    {
      
    }
    updateAll();
  }

  public void MouseUp(Vector3 mousePos, SketchMouse sm)
  {
    if (MainMode == MainMode.Sketch)
    {
      this.SketchModeController.MouseUp(mousePos, sm);
    }
    else if (MainMode == MainMode.Generate)
    {

    }
    updateAll();
  }

  public void MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    if (MainMode == MainMode.Sketch)
    {
      this.SketchModeController.MouseDrag(preMousePos, curMousePos, sm);
    }
    else if (MainMode == MainMode.Generate)
    {
      //カメラ移動
      this.SketchCameraController.MouseDrag(preMousePos, curMousePos, sm);
    }
    updateAll();
  }

  public void MouseWheel(Vector2 delta)
  {
    if (MainMode == MainMode.Sketch)
    {
      this.SketchModeController.MouseWheel(delta);
    }
    else if (MainMode == MainMode.Generate)
    {
      this.SketchCameraController.MouseWheel(delta);
    }
    updateAll();
  }

  /// <summary>
  /// 描画ツールが選択されたときのイベント
  /// </summary>
  /// <param name="penId"></param>
  public void ClickToolListButton(int penId)
  {
    updateCurrentTool(penId);
    //ビュー側を更新
    updateAll();
  }

  /// <summary>
  /// ツール設定が変更されたときのイベント
  /// 現在のペンが対象
  /// </summary>
  /// <param name="setting"></param>
  public void ChangeCurrentSketchPenSetting((string, object) setting)
  {
    ChangeSketchPenSetting(SketchPenManager.CurrentPenIndex, setting);
  }

  /// <summary>
  /// ツール設定が変更されたときのイベント
  /// </summary>
  /// <param name="penId">対象の描画オブジェクトのID</param>
  /// <param name="setting">変更された設定</param>
  public void ChangeSketchPenSetting(int penId, (string, object) setting)
  {
    updateSketchPenSetting(penId, setting);
    //ビュー側を更新
    updateAll();
  }

  public void ChangeCameraProjection(CameraProjection cp)
  {
    this.SketchCameraController.SetProjection(cp);
  }

  public void ChangeCameraFOV(float fov)
  {
    this.SketchCameraController.SetFOV(fov);
  }

  public void ChangeCameraSize(float size)
  {
    this.SketchCameraController.SetSize(size);
  }

  public void ClickToDefaultCamera()
  {
    this.SketchCameraController.SCIP = this.SketchCameraController.InitialSCIP;
  }

  /// <summary>
  /// 現在のスケッチの背景色を変更する．
  /// スケッチモードの時のみ有効
  /// </summary>
  /// <param name="bGColor"></param>
  public void ChangeSketchBGColor(Color bGColor)
  {
    if (MainMode == MainMode.Sketch) this.SketchManager.CurrentSketch.BGColor = bGColor;
    Debug.Log("スケッチ - 背景色変更 : " + bGColor);
    updateAll();
  }

  public void ChangeSketchBGTransparency(float bGTransparency)
  {
    if (MainMode == MainMode.Sketch) this.SketchManager.CurrentSketch.BGTransparency = bGTransparency;
    Debug.Log("スケッチ - 背景透明度変更 : " + bGTransparency);
    updateAll();
  }

  public void ChangeSketchModelTransparency(float modelTransparency)
  {
    if (MainMode == MainMode.Sketch) this.SketchManager.CurrentSketch.ModelTransparency = modelTransparency;
    Debug.Log("スケッチ - 背景透明度変更 : " + modelTransparency);
    updateAll();
  }

  /// <summary>
  /// 指定されたサイズでスケッチを生成
  /// </summary>
  /// <param name="size"></param>
  public void ClickNewCanvasButton(SketchAndCameraInitializationParam sip)
  {
    //カメラ情報をコピー
    SketchAndCameraInitializationParam scip = this.SketchCameraController.SCIP;
    //解像度パラメータ追加
    scip.Resolution = sip.Resolution;
    //スケッチ追加
    var sketch = this.SketchManager.AddSketch(scip);
    //現在のスケッチを変更
    this.SketchManager.SwitchSketch(sketch);
    //スケッチビューを作成
    this.UIManager.UpdateView_AddSketch(sketch);
    //モードをスケッチモードに変更
    this.modeChange(MainMode.Sketch);
    //ビューを更新
    updateAll();
  }

  public void ClickGenerateModelButton()
  {
    //Debug.Log("Click GenerateModelButton");
    ModelGenerateController.ExecuteModelGenerate(this.SketchManager.Sketches.ToArray());
    updateAll();
  }

  public void ClickChangeModeButton()
  {
    if (MainMode == MainMode.Sketch) modeChange(MainMode.Generate);
    else if (MainMode == MainMode.Generate) modeChange(MainMode.Sketch);
    updateAll();
  }

  /* 再利用しそうなメソッド */
  /// <summary>
  /// 現在の描画オブジェクトを変更する
  /// UIもそれに合わせて変更する．
  /// </summary>
  /// <param name="penId">変更後の描画オブジェクトのID</param>
  private void updateCurrentTool(int penId)
  {
    //描画オブジェクトマネージャーを更新
    this.SketchPenManager.CurrentPenIndex = penId;
  }
  /// <summary>
  /// 描画オブジェクトの設定を変更する
  /// </summary>
  /// <param name="penId">対象の描画オブジェクトのID</param>
  /// <param name="setting">変更された設定</param>
  private void updateSketchPenSetting(int penId, (string, object) setting)
  {
    this.SketchPenManager.SetSetting(penId, setting);
  }

  /// <summary>
  /// モード切替の処理
  /// </summary>
  /// <param name="mm"></param>
  private void modeChange(MainMode mm)
  {
    if(mm != MainMode)
    {
      //ビューのモードチェンジ
      if(mm == MainMode.Sketch)
      {
        if (this.SketchManager.CurrentSketch == null)
        {
          Debug.Log("スケッチが存在しないため，モードを切り替えられませんでした．");
          return;
        }
        this.UIManager.UpdateView_SketchZone(this.SketchManager.CurrentSketch);
        this.UIManager.ChangeMainMode_Sketch(this.SketchManager.CurrentSketch);
      }
      else if(mm == MainMode.Generate)
      {
        this.UIManager.ChangeMainMode_Generate();
        this.UIManager.UpdateView_ApplySketch(this.SketchManager.CurrentSketch);
      }
    }

    //メインモードを更新
    this.MainMode = mm;

    Debug.Log("メインモード : " + this.MainMode);
  }

  /// <summary>
  /// 全ての変更をUIに適用する．
  /// </summary>
  private void updateAll()
  {
    //ツール設定パネルの更新
    this.UIManager.UpdateView_ToolSettings(this.SketchPenManager.CurrentPen);
    //ツール設定の変更を反映
    this.UIManager.UpdateValue_ToolSettings(this.SketchPenManager.CurrentPen);
    //カメラ設定の変更を反映
    this.UIManager.UpdateValue_CameraSetting(this.SketchCameraController.SCIP);
    //スケッチモードの場合
    if (this.MainMode == MainMode.Sketch)
    {
      //スケッチゾーンの変更を反映（真ん中のスペース）
      this.UIManager.UpdateView_SketchZone(this.SketchManager.CurrentSketch);
      //スケッチ設定パネルの変更を更新
      this.UIManager.UpdateValue_SketchSetting(this.SketchManager.CurrentSketch);
    }
    //生成モードの場合
    else if (this.MainMode == MainMode.Generate)
    {

    }
  }
}
