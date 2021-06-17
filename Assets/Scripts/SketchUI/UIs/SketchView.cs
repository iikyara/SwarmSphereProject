using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SketchView : MonoBehaviour, IClickable
{
  public SketchViewManager SketchViewManager;
  public DrawableSketch DrawableSketch;

  public void OnClick()
  {
    SketchViewManager.OnClick_SketchView(DrawableSketch);
  }
}
