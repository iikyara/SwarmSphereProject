using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkUIManager : MonoBehaviour, ISketchSpace
{
  public BenchmarkController Controller;

  public SketchSpace SketchSpace;

  // Start is called before the first frame update
  void Start()
  {
    this.SketchSpace.UIManager = this;
  }

  public void SketchSpace_MouseDown(Vector3 mousePos, SketchMouse sm)
  {
    this.Controller.MouseDown(mousePos, sm);
  }

  public void SketchSpace_MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    this.Controller.MouseDrag(preMousePos, curMousePos, sm);
  }

  public void SketchSpace_MouseUp(Vector3 mousePos, SketchMouse sm)
  {
    this.Controller.MouseUp(mousePos, sm);
  }

  public void SketchSpace_MouseWheel(Vector2 delta)
  {
    this.Controller.MouseWheel(delta);
  }
}
