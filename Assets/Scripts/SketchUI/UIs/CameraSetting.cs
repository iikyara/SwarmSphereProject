using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting : MonoBehaviour
{
  public UIManager UIManager;

  public FieldValue Projection_EL;
  public FieldValue FOV_EL;
  public FieldValue Size_EL;

  public void SetValue(SketchAndCameraInitializationParam scip)
  {
    Projection_EL.SetField(scip.Projection);
    FOV_EL.SetField(scip.FOV);
    Size_EL.SetField(scip.Size);
  }

  public void OnChange_Projection()
  {
    CameraProjection cp = (CameraProjection)Projection_EL.GetField();
    this.UIManager.CameraSetting_ChangeProjection(cp);
  }

  public void OnChange_FOV()
  {
    float fov = (float)FOV_EL.GetField();
    this.UIManager.CameraSetting_ChangeFOV(fov);
  }

  public void OnChange_Size()
  {
    float size = (float)Size_EL.GetField();
    this.UIManager.CameraSetting_ChangeSize(size);
  }

  public void OnClick_ToDefaultButton()
  {
    this.UIManager.CameraSetting_ClickToDefaultButton();
  }
}
