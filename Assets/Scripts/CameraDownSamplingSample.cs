using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDownSamplingSample : MonoBehaviour
{
  /// <summary>
  /// デフォルト解像度のカメラ
  /// </summary>
  public Camera camera1;
  /// <summary>
  /// 解像度をResolutionに合わせるカメラ
  /// </summary>
  public Camera camera2;

  /// <summary>
  /// カメラ1の解像度
  /// </summary>
  public Vector2Int Resolution1;
  /// <summary>
  /// カメラ2の解像度
  /// </summary>
  public Vector2Int Resolution2;

  /// <summary>
  /// プレビューが表示される場所
  /// </summary>
  public GameObject CameraPreviewParentObject;

  // Start is called before the first frame update
  void Start()
  {
    CreateViewBoard(Resolution1.x, Resolution1.y, camera1, CameraPreviewParentObject);
    CreateViewBoard(Resolution2.x, Resolution2.y, camera2, CameraPreviewParentObject);
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  public GameObject CreateViewBoard(int width, int height, Camera camera, GameObject root)
  {
    //GUI上にカメラ視点を入れる
    var rtex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    rtex.enableRandomWrite = true;
    rtex.Create();
    camera.targetTexture = rtex;
    camera.aspect = width / height;
    var cameraView = new GameObject("Camera View");
    var rt = cameraView.AddComponent<RectTransform>();
    var cr = cameraView.AddComponent<CanvasRenderer>();
    var ir = cameraView.AddComponent<RawImage>();
    ir.texture = rtex;
    Utils.SetParent(root, cameraView);
    rt.localScale = new Vector3(1, 1, 1);
    rt.pivot = new Vector2(0, 1);
    return cameraView;
  }
}
