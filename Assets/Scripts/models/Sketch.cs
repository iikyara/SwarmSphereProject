using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SketchInitilizationParam
{
  public Texture2D SketchImage;
  public Vector2Int Resolution;
  public Vector3 CameraPosition;
  public Vector3 CameraRotation;
  public CameraProjection CameraProjection;
  public float FOV;
  public float Size;
}

public enum CameraProjection
{
  Perspective,
  Orthographic
}

/// <summary>
/// スケッチ画像1枚と描かれた視点を記録
/// </summary>
public class Sketch
{
  //public RawImage image;
  private Texture2D _scanedImage;
  private Texture2D _sketchImage;
  private Texture2D _evaluationMap;
  /// <summary>
  /// スケッチ視点から描画した画像
  /// </summary>
  public Texture2D ScanedImage
  {
    get { return _scanedImage; }
    set
    {
      if (_scanedImage) MonoBehaviour.Destroy(_scanedImage);
      _scanedImage = value;
      if(this.ScanedBoard) Utils.ChangeTexture(this.ScanedBoard, value);
    }
  }
  /// <summary>
  /// スケッチの元の画像
  /// </summary>
  public Texture2D SketchImage
  {
    get { return _sketchImage; }
    set
    {
      if (_sketchImage) MonoBehaviour.Destroy(_sketchImage);
      _sketchImage = value;
    }
  }
  /// <summary>
  /// スケッチの評価用マップ
  /// </summary>
  public Texture2D EvaluationMap
  {
    get { return _evaluationMap; }
    set
    {
      if (_evaluationMap) MonoBehaviour.Destroy(_evaluationMap);
      _evaluationMap = value;
    }
  }

  /// <summary>
  /// スケッチの物体がある部分のピクセル数
  /// </summary>
  public int SketchPixelSum;
  /// <summary>
  /// スケッチの物体がある部分のアルファ値の合計
  /// </summary>
  public float SketchAlphaSum;
  /// <summary>
  /// 評価マップの物体がある部分のピクセル数
  /// </summary>
  public int EMPixelSum;
  /// <summary>
  /// 評価マップの物体がある部分のアルファ値の合計
  /// </summary>
  public float EMAlphaSum;
  //public RenderTexture image;

  public GameObject Parent;
  public GameObject ScanedBoard;
  public GameObject SketchBoardParent;
  public GameObject SketchBoard;
  public GameObject SketchBoardBG;
  public GameObject CameraObject;

  public Camera camera;
  private CanvasRenderer cr;
  private SpriteRenderer sr;
  private SketchInitilizationParam sip;
  private bool isCreateView;
  //Image im;

  public static float SketchBoardDistance = 20f;

  public Sketch(bool createView = true)
  {
    this.isCreateView = createView;
    if(createView)
    {
      SketchBoardParent = new GameObject("Sketch");
      var tex = Utils.CreateMonocolorTexture(new Color(0.5f, 0.5f, 0.5f, 1f), 10, 10);
      SketchBoard = Utils.CreateQuad("Board", tex);
      Utils.SetParent(SketchBoardParent, SketchBoard);
      SketchBoardBG = Utils.CreateQuad("Background", tex);
      SketchBoardBG.transform.position = new Vector3(0f, 0f, 0.001f);
      Utils.SetParent(SketchBoardParent, SketchBoardBG);
      /*if (ScanedBoard == null)
      {
        ScanedBoard = new GameObject("ScanedBoard");
        this.cr = ScanedBoard.AddComponent<CanvasRenderer>();
        this.sr = ScanedBoard.AddComponent<SpriteRenderer>();
        ScanedBoard.transform.position = new Vector3(0f, 10f, 0f);
      }*/
      //im = Board.AddComponent<Image>();
      //var mr = Board.AddComponent<MeshRenderer>();
      //var mf = Board.AddComponent<MeshFilter>();
      ScanedBoard = Utils.CreateQuad("Scaned", tex);
      ScanedBoard.transform.position = new Vector3(0f, 1.2f, 0f);
      Utils.SetParent(SketchBoardParent, ScanedBoard);
    }

    if (this.CameraObject == null)
    {
      this.CameraObject = new GameObject("SketchCamera");
      this.camera = CameraObject.AddComponent<Camera>();
      this.camera.clearFlags = CameraClearFlags.SolidColor;
      this.camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
      //this.camera.enabled = false;
    }
    if(this.SketchImage == null)
    {
      //this.SketchImage = new Texture2D(1000, 1000);
      SetSketch(new Texture2D(1000, 1000));
    }
    this.ScanedImage = null;

    SetAspect(this.SketchImage.width, this.SketchImage.height);
  }

  public Sketch(Vector3 cam_position, Quaternion cam_rotate, bool createView = true) : this(createView)
  {
    //this.camera.transform.position = cam_position;
    //this.camera.transform.rotation = cam_rotate;
    SetCameraTransform(cam_position, cam_rotate);
  }

  public Sketch(Vector3 cam_position, Quaternion cam_rotate, GameObject parent, bool createView = true) : this(cam_position, cam_rotate, createView)
  {
    SetParent(parent);
  }

  public Sketch(Texture2D sketchImage, bool createView = true) : this(sketchImage, new Vector3(), new Quaternion(), createView) { }

  public Sketch(Texture2D sketchImage, Vector3 cam_position, Quaternion cam_rotate, bool createView = true) : this(sketchImage, cam_position, cam_rotate, null, createView) { }

  public Sketch(Texture2D sketchImage, Vector3 cam_position, Quaternion cam_rotate, GameObject parent, bool createView = true) : this(cam_position, cam_rotate, createView)
  {
    //this.SketchImage = sketchImage;
    SetSketch(sketchImage);
    //var sr = this.SketchBoard.GetComponent<SpriteRenderer>();
    //sr.sprite = Sprite.Create(sketchImage, new Rect(0, 0, sketchImage.width, sketchImage.height), new Vector2());
    if (this.SketchBoard) Utils.ChangeTexture(this.SketchBoard, sketchImage);
    SetAspect(sketchImage.width, sketchImage.height);
    CorrectBoardTransform();
  }

  public Sketch(Texture2D sketchImage, Vector2Int Resolution, Vector3 cam_position, Quaternion cam_rotate, bool createView = true) : this(cam_position, cam_rotate, createView)
  {
    var imageTex = Utils.TextureScaling(sketchImage, Resolution.x, Resolution.y, Utils.ScalingMode.Bilinear);
    SetSketch(imageTex);
    //var sr = this.SketchBoard.GetComponent<SpriteRenderer>();
    //sketchImage = imageTex;
    //sr.sprite = Sprite.Create(sketchImage, new Rect(0, 0, sketchImage.width, sketchImage.height), new Vector2());
    if(this.SketchBoard) Utils.ChangeTexture(this.SketchBoard, sketchImage);
    SetAspect(sketchImage.width, sketchImage.height);
    CorrectBoardTransform();
  }

  public Sketch(SketchInitilizationParam sip, bool createView = true) : this(
    sip.SketchImage, sip.Resolution, sip.CameraPosition, sip.CameraRotation,
    sip.CameraProjection, sip.FOV, sip.Size, null, createView
  )
  {
    this.sip = sip;
    /* this.sip = sip;
    this.camera.fieldOfView = sip.FOV;
    if(sip.CameraProjection == CameraProjection.Orthographic)
    {
      // 平行投影のProjectionMatrixを計算する
      var aspectRatio = this.camera.aspect;
      var orthoWidth = sip.Size * aspectRatio;
      var projMatrix = Matrix4x4.Ortho(orthoWidth * -1, orthoWidth, sip.Size * -1, sip.Size, 0, 1000);

      // カメラのProjectionMatrixを上書き
      this.camera.projectionMatrix = projMatrix;
    }
    CorrectBoardTransform();*/
    CorrectBoardTransform();
  }

  public Sketch(
    Texture2D sketchImage, Vector2Int resolution, Vector3 cameraPosition, Vector3 cameraRotation,
    CameraProjection cameraProjection, float fov, float size, GameObject parent, bool isCreateView = true
  )
  {
    //カメラ生成
    this.CameraObject = new GameObject("SketchCamera");
    this.camera = CameraObject.AddComponent<Camera>();
    this.camera.clearFlags = CameraClearFlags.SolidColor;
    this.camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
    this.camera.fieldOfView = fov;
    float aspect = (float)resolution.x / resolution.y;
    Utils.CalcAndSetProjection(aspect, size, fov, cameraProjection, ref this.camera);
    /*if (cameraProjection == CameraProjection.Orthographic)
    {
      // 平行投影のProjectionMatrixを計算する
      float aspectRatio = (float)resolution.x / resolution.y;
      float orthoWidth = size * aspectRatio;
      var projMatrix = Matrix4x4.Ortho(orthoWidth * -1, orthoWidth, size * -1, size, 0, 1000);

      // カメラのProjectionMatrixを上書き
      this.camera.projectionMatrix = projMatrix;
    }
    else if(cameraProjection == CameraProjection.Perspective)
    {
      float aspect = (float)resolution.x / resolution.y;
      var projMatrix = Matrix4x4.Perspective(fov, aspect, 0.3f, 1000f);
      this.camera.projectionMatrix = projMatrix;
    }*/
    SetCameraTransform(cameraPosition, Quaternion.Euler(cameraRotation));
    //スケッチ生成
    if (sketchImage == null)
    {
      sketchImage = new Texture2D(1000, 1000);
    }
    sketchImage = Utils.TextureScaling(sketchImage, resolution.x, resolution.y, Utils.ScalingMode.Bilinear);
    this.ScanedImage = null;

    SetSketch(sketchImage);
    SetAspect(sketchImage.width, sketchImage.height);

    //ビュー生成
    this.isCreateView = isCreateView;
    if (isCreateView)
    {
      createView();
      CorrectBoardTransform();
    }
    if (this.SketchBoard) Utils.ChangeTexture(this.SketchBoard, sketchImage);

    //親設定
    SetParent(parent);
  }

  public void Discard()
  {
    this.SketchImage = null;
    this.ScanedImage = null;
    this.EvaluationMap = null;
    if (this.SketchBoard) MonoBehaviour.Destroy(this.SketchBoard);
    if (this.ScanedBoard) MonoBehaviour.Destroy(this.ScanedBoard);
    if (this.SketchBoardBG) MonoBehaviour.Destroy(this.SketchBoardBG);
    if (this.SketchBoardParent) MonoBehaviour.Destroy(this.SketchBoardParent);
    if (this.CameraObject) MonoBehaviour.Destroy(this.CameraObject);
  }

  public override string ToString()
  {
    return string.Format("{0}, " +
      "sketch : {1}, " +
      "capture : {2}, " +
      "EMap : {3}, " +
      "SketchPixelSum : {4}, " +
      "SketchAlphaSum : {5}, " +
      "EMPixelSum : {6}," +
      "EMAlphaSum : {7}",
      base.ToString(),
      this.SketchImage != null,
      this.ScanedImage != null,
      this.EvaluationMap != null,
      SketchPixelSum,
      SketchAlphaSum,
      EMPixelSum,
      EMAlphaSum
    );
  }
/*
  /// <summary>
  /// スケッチの物体がある部分のピクセル数
  /// </summary>
  public int SketchPixelSum;
  /// <summary>
  /// スケッチの物体がある部分のアルファ値の合計
  /// </summary>
  public float SketchAlphaSum;
  /// <summary>
  /// 評価マップの物体がある部分のピクセル数
  /// </summary>
  public int EMPixelSum;
  /// <summary>
  /// 評価マップの物体がある部分のアルファ値の合計
  /// </summary>
  public float EMAlphaSum;*/

  public void SetParent(GameObject parent)
  {
    this.Parent = parent;
    if(this.ScanedBoard) Utils.SetParent(parent, this.ScanedBoard);
    Utils.SetParent(parent, this.CameraObject);
    if(this.SketchBoard) Utils.SetParent(parent, this.SketchBoard);
  }

  public void SetAspect(int width, int height)
  {
    //this.camera.pixelRect = new Rect(0, 0, width, height);
    //var rt = this.SketchBoard.GetComponent<RectTransform>();
    //rt.pivot = new Vector2(width / 2, height / 2);
    //this.camera.pixelRect = new Rect(0, 0, width / height, 1);
    this.camera.aspect = (float)width / height;

    if(SketchBoard) Utils.ChangeQuadAspect(SketchBoard, width, height);
    if(ScanedBoard) Utils.ChangeQuadAspect(ScanedBoard, width, height);
    if(SketchBoardBG) Utils.ChangeQuadAspect(SketchBoardBG, width, height);

    //Debug.Log($"SetAspect - width:{width}, height:{height}, aspect:{camera.aspect}");
  }

  public void SetCameraTransform(Vector3 position, Quaternion rotate)
  {
    //カメラに位置を設定
    this.camera.transform.position = position;
    this.camera.transform.rotation = rotate;
  }

  public void SetSketch(Texture2D image)
  {
    this.SketchImage = image;
    this.SketchPixelSum = Utils.CountNonTransparentPixel_CPUParallel(image);
    this.SketchAlphaSum = Utils.SumAlpha_CPUParallel(image);
  }

  public void SetEvaluationMap(Texture2D image)
  {
    this.EvaluationMap = image;
    this.EMPixelSum = Utils.CountNonTransparentPixel_CPUParallel(image);
    this.EMAlphaSum = Utils.SumAlpha_CPUParallel(image);
  }

  public void CorrectBoardTransform()
  {
    if (!isCreateView) return;
    var position = this.camera.transform.position;
    var rotate = this.camera.transform.rotation;
    //スケッチの位置と大きさを設定
    //var rt = this.SketchBoard.GetComponent<RectTransform>();
    //var sr = this.SketchBoard.GetComponent<SpriteRenderer>();
    //ベクトル
    Vector3 direction = position.normalized;
    Vector3 sktPos = position - direction * (position.magnitude + SketchBoardDistance);
    //スケッチの大きさを求める
    float distanceCamAndSkt = (position - sktPos).magnitude;
    //float sprite_height = sr.sprite.texture.height / sr.sprite.pixelsPerUnit;
    //float sprite_width = sr.sprite.texture.width / sr.sprite.pixelsPerUnit;
    float sprite_height = 1f;
    //float sprite_width = 1f;
    //https://teratail.com/questions/191194
    float scale = distanceCamAndSkt * Mathf.Tan(this.camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2 / sprite_height;
    if (sip.CameraProjection == CameraProjection.Orthographic)
    {
      scale = sip.Size * 2 / sprite_height;
    }
    //スケッチの中心座標を求める．
    //Vector2 sktCenter = new Vector2(sprite_width * scale / 2, sprite_height * scale / 2);
    //位置オフセットを求める
    Vector3 offset = new Vector3();
    var up = this.camera.transform.up;
    //offset += up * sktCenter.y;
    //offset += Vector3.Cross(direction, up).normalized * sktCenter.x;

    float height = sprite_height * scale;
    float width = height * this.camera.aspect;
    this.SketchBoardParent.transform.position = sktPos - offset;
    this.SketchBoardParent.transform.rotation = rotate;
    this.SketchBoardParent.transform.localScale = new Vector3(scale, scale, scale);
    //this.SketchBoard.SetActive(false);
  }

  /// <summary>
  /// 現在のカメラを用いて撮影
  /// </summary>
  /// <returns></returns>
  public Texture2D Capture()
  {
    //Debug.Log(camera);
    /*
    var width = this.ScanedImage.width;
    var height = this.ScanedImage.height;
    MonoBehaviour.Destroy(this.ScanedImage);*/
    this.camera.cullingMask = 1 << LayerMask.NameToLayer("ForDrawable");
    this.ScanedImage = Utils.CaptureCameraImage(camera, this.SketchImage.width, this.SketchImage.height);
    this.camera.cullingMask = 1;
    //this.image = Utils.CaptureCameraImage(UnityEngine.Camera.main);
    //cr.SetTexture(image);
    //Debug.Log("image pixel : " + Utils.CountNonTransparentPixel_CPUParallel(this.ScanedImage));
    //Debug.Log("image pixel : " + this.ScanedImage.width + ", " + this.ScanedImage.height);
    //sr.sprite = Sprite.Create(ScanedImage, new Rect(0, 0, ScanedImage.width, ScanedImage.height), new Vector2());
    //Debug.Log($"Capture : {this.ScanedImage.width}, {this.ScanedImage.height}, aspect:{camera.aspect}");
    return this.ScanedImage;
  }

  public Texture2D AlphaJoinSketchAndScaned()
  {
    //return Utils.AlphaJoin(this.SketchImage, this.ScanedImage);
    return GPGPUUtils.AlphaJoin(this.SketchImage, this.ScanedImage);
  }

  public Texture2D AlphaJoinSketchAndScaned2(float attenuation)
  {
    return GPGPUUtils.AlphaJoin2(this.SketchImage, this.ScanedImage, attenuation);
  }

  public Texture2D AlphaJoinEMAndScaned()
  {
    return GPGPUUtils.AlphaJoin(this.EvaluationMap, this.ScanedImage);
  }

  public Texture2D AlphaJoinEMAndScaned2(float attenuation)
  {
    return GPGPUUtils.AlphaJoin2(this.EvaluationMap, this.ScanedImage, attenuation);
  }

  public Color GetPixel(int x, int y)
  {
    return this.ScanedImage.GetPixel(x, y);
  }

  public Vector2Int GetSize()
  {
    return new Vector2Int(this.ScanedImage.width, this.ScanedImage.height);
  }

  private void createView()
  {
    //スケッチビュー
    SketchBoardParent = new GameObject("Sketch");
    var tex = Utils.CreateMonocolorTexture(new Color(0.5f, 0.5f, 0.5f, 1f), 10, 10);
    SketchBoard = Utils.CreateQuad("Board", tex);
    Utils.SetParent(SketchBoardParent, SketchBoard);
    SketchBoardBG = Utils.CreateQuad("Background", tex);
    SketchBoardBG.transform.position = new Vector3(0f, 0f, 0.001f);
    Utils.SetParent(SketchBoardParent, SketchBoardBG);
    ScanedBoard = Utils.CreateQuad("Scaned", tex);
    ScanedBoard.transform.position = new Vector3(0f, 1.2f, 0f);
    Utils.SetParent(SketchBoardParent, ScanedBoard);

    SetAspect(this.SketchImage.width, this.SketchImage.height);
  }
}
