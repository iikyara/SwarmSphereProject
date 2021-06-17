using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeChange : MonoBehaviour
{
  public UIManager UIManager;

  public Text ChangeModeButtonText;

  public void SetMode(MainMode mm)
  {
    if(mm == MainMode.Sketch)
    {
      ChangeModeButtonText.text = "3Dモードに切り替える";
    }
    else if (mm == MainMode.Generate)
    {
      ChangeModeButtonText.text = "直近のスケッチモードに切り替える";
    }
  }

  public void OnClick_ChangeModeButton()
  {
    this.UIManager.OnClick_ChangeModeButton();
  }
}
