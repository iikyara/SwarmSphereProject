using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SketchSetting : MonoBehaviour
{
  public UIManager UIManager;

  /// <summary>
  /// 背景色
  /// </summary>
  public FieldValue BGColor_EL;
  public FieldValue BGTransparency_EL;
  public FieldValue ModelTransparency_EL;
  public FieldValue Resolution_EL;

  private void Start()
  {
    Resolution_EL.SetField(new Vector2Int(1920, 1080));
  }

  public void UpdateValues(DrawableSketch dSketch)
  {
    SetField_BGColor(dSketch.BGColor);
    SetField_BGTransparency(dSketch.BGTransparency);
    SetField_ModelTransparency(dSketch.ModelTransparency);
    Debug.Log(dSketch.BGColor);
  }

  public void SetField_BGColor(Color bGColor)
  {
    BGColor_EL.SetField(bGColor);
  }

  public void SetField_BGTransparency(float transparency)
  {
    BGTransparency_EL.SetField(transparency);
  }

  public void SetField_ModelTransparency(float transparency)
  {
    ModelTransparency_EL.SetField(transparency);
  }

  public void OnChange_BGColor()
  {
    Color bGColor = (Color)BGColor_EL.GetField();
    this.UIManager.SketchSetting_ChangeBGColor(bGColor);
  }

  public void OnChange_BGTransparency()
  {
    float bGTransparency = (float)BGTransparency_EL.GetField();
    this.UIManager.SketchSetting_ChangeBGTransparency(bGTransparency);
  }

  public void OnChange_ModelTransprency()
  {
    float modelTransparency = (float)ModelTransparency_EL.GetField();
    this.UIManager.SketchSetting_ChangeModelTransparency(modelTransparency);
  }

  public void OnClick_NewCanvasButton()
  {
    Vector2Int resolution = (Vector2Int)Resolution_EL.GetField();
    SketchAndCameraInitializationParam sip = new SketchAndCameraInitializationParam();
    sip.Resolution = resolution;
    this.UIManager.SketchSetting_ClickNewCanvasButton(sip);
  }
}
