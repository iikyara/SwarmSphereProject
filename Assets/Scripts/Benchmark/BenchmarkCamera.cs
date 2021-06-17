using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkCamera : MonoBehaviour
{
  public string Name;

  public SketchInitilizationParam SIP;

  private Camera bCamera;

  private void Start()
  {
    //SIPからカメラを生成する
    GameObject cameraGO = new GameObject("BenchmarkCamera");
    Utils.SetParent(this.transform.gameObject, cameraGO);
    bCamera = cameraGO.AddComponent<Camera>();
    bCamera.clearFlags = CameraClearFlags.SolidColor;
    bCamera.backgroundColor = Color.clear;
    bCamera.transform.position = SIP.CameraPosition;
    bCamera.transform.rotation = Quaternion.Euler(SIP.CameraRotation);
    bCamera.orthographic = SIP.CameraProjection == CameraProjection.Orthographic;
    bCamera.orthographicSize = SIP.Size;
    bCamera.fieldOfView = SIP.FOV;
    bCamera.aspect = (float)SIP.Resolution.x / SIP.Resolution.y;
    bCamera.enabled = false;
  }

  public SketchInitilizationParam Capture(GTData data)
  {
    var go_active = data.GameObject.activeSelf;
    var go_layer = data.GameObject.layer;

    //もし前回の撮影画像が残っていたら消す
    if (SIP.SketchImage != null) MonoBehaviour.Destroy(SIP.SketchImage);
    //前処理
    bCamera.cullingMask = 1 << LayerMask.NameToLayer("ForDrawable");
    data.GameObject.SetActive(true);
    data.GameObject.layer = LayerMask.NameToLayer("ForDrawable");
    //撮影
    SIP.SketchImage = Utils.CaptureCameraImage(bCamera, SIP.Resolution.x, SIP.Resolution.y);
    //後処理
    bCamera.cullingMask = 1;
    data.GameObject.SetActive(go_active);
    data.GameObject.layer = go_layer;
    return SIP;
  }
}
