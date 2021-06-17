using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SIP : MonoBehaviour
{
  public SketchInitilizationParam[] Sips;

  public GameObject CameraPreviewParentObject;
  public bool CreateSketch;

  public void Start()
  {
    if (CreateSketch)
    {
      foreach(var sip in Sips)
      {
        //sketches[i] = new Sketch(Sips[i].SketchImage, Sips[i].CameraPosition, Quaternion.Euler(Sips[i].CameraRotation));
        var sketch = new Sketch(sip);
        //GUI上にカメラ視点を入れる
        var rtex = new RenderTexture(sip.SketchImage.width, sip.SketchImage.height, 0, RenderTextureFormat.ARGBFloat);
        rtex.enableRandomWrite = true;
        rtex.Create();
        var camera = sketch.CameraObject.GetComponent<Camera>();
        camera.targetTexture = rtex;
        var cameraView = new GameObject("Camera View");
        var rt = cameraView.AddComponent<RectTransform>();
        var cr = cameraView.AddComponent<CanvasRenderer>();
        var ir = cameraView.AddComponent<RawImage>();
        ir.texture = rtex;
        Utils.SetParent(CameraPreviewParentObject, cameraView);
        rt.localScale = new Vector3(1, 1, 1);
        rt.pivot = new Vector2(0, 1);
      }
    }
  }
}
