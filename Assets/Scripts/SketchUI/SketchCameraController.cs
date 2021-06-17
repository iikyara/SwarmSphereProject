using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sketch and Camera Initialize Paramater
/// </summary>
[System.Serializable]
public struct SketchAndCameraInitializationParam
{
  /* Sketch Params */
  public Vector2Int Resolution;

  /* Camera Params */
  public Vector3 Position;
  public Vector3 LookAt;
  public Vector3 Up;
  public CameraProjection Projection;
  public float FOV;
  public float Size;

  public SketchAndCameraInitializationParam(
    Vector2Int resolution, Vector3 position, Vector3 lookAt,
    Vector3 up, CameraProjection cp, float fov, float size
  )
  {
    this.Resolution = resolution;
    this.Position = position;
    this.LookAt = lookAt;
    this.Up = up;
    this.Projection = cp;
    this.FOV = fov;
    this.Size = size;
  }
}

public class SketchCameraController : MonoBehaviour
{
  public GameObject PanObject;
  public GameObject TiltObject;

  public bool Pan_inversed = false;
  public bool Tilt_inversed = true;
  public bool Scale_inversed = true;

  public float rotateSpeed = 1.0f;
  public float scaleSpeed_Wheel = 1.0f;
  public float scaleSpeed_Drag = 1.0f;
  public float moveSpeed = 1.0f;

  private Camera mainCamera;

  /*private Vector3 targetPos;
  private Vector3 lookAt;
  private Vector3 up;
  private CameraProjection cp;
  private float fov;
  private float size;*/

  [SerializeField]
  private SketchAndCameraInitializationParam _scip;
  public SketchAndCameraInitializationParam SCIP {
    get
    {
      return _scip;
    }
    set
    {
      SetUp(value.Up);
      SetPosition(value.Position);
      SetLookAt(value.LookAt);
      SetProjection(value.Projection);
      SetFOV(value.FOV);
      SetSize(value.Size);
      this._scip = value;
    }
  }

  public SketchAndCameraInitializationParam InitialSCIP;

  // Start is called before the first frame update
  void Start()
  {
    this.mainCamera = GetComponent<Camera>();
    this.InitialSCIP = SCIP;
    UpdateSCIPofTransform();
  }

  private void Update()
  {
    
  }

  public void MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    //Ctrl＋左ドラッグ
    if(sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
      //Debug.Log("s1 : " + sm.ToString());

      ScaleCamera((curMousePos.y - preMousePos.y) * scaleSpeed_Drag);
    }
    //Shift＋左ドラッグ
    else if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
    {
      //Debug.Log("s2 : " + sm.ToString());

      MoveCamera(preMousePos, curMousePos);
    }
    //左ドラッグ
    else if (sm == SketchMouse.left)
    {
      //Debug.Log("s3 : " + sm.ToString());

      RotateCamera(preMousePos, curMousePos);
    }
    //中ドラッグ
    else if (sm == SketchMouse.middle)
    {
      //Debug.Log("s4 : " + sm.ToString());

      MoveCamera(preMousePos, curMousePos);
    }
    //右ドラッグ
    else if (sm == SketchMouse.right)
    {
      //Debug.Log("s5 : " + sm.ToString());

      ScaleCamera((curMousePos.y - preMousePos.y) * scaleSpeed_Drag);
    }

    UpdateSCIPofTransform();
    //Debug.Log($"pos : {_scip.Position}, lookat : {_scip.LookAt}, up : {_scip.Up}");
  }

  public void MouseWheel(Vector2 delta)
  {
    //Debug.Log($"wheel : {delta}");
    ScaleCamera(delta.y * scaleSpeed_Wheel);
  }

  public void RotateCamera(Vector3 preMousePos, Vector3 curMousePos)
  {
    float dx = curMousePos.x - preMousePos.x;
    float dy = curMousePos.y - preMousePos.y;

    float pan = dx * rotateSpeed * (Pan_inversed ? -1 : 1);
    float tilt = dy * rotateSpeed * (Tilt_inversed ? -1 : 1);

    PanObject.transform.Rotate(new Vector3(0, pan, 0));
    TiltObject.transform.Rotate(new Vector3(tilt, 0, 0));
  }

  public void MoveCamera(Vector3 preMousePos, Vector3 curMousePos)
  {
    float dx = curMousePos.x - preMousePos.x;
    float dy = curMousePos.y - preMousePos.y;

    Vector4 move = new Vector4(-dx, -dy, 0f, 1f);

    move = Matrix4x4.Rotate(mainCamera.transform.rotation) * move;

    Vector3 m = new Vector3(move.x, move.y, move.z);

    PanObject.transform.position += m * moveSpeed;
  }

  public void ScaleCamera(float scale)
  {
    //スケーリング
    this.mainCamera.transform.Translate(new Vector3(
      0,
      0,
      this.mainCamera.transform.localPosition.z * scale * (Scale_inversed ? -1 : 1)
    ));
  }

  public void UpdateSCIPofTransform()
  {
    _scip.Position = mainCamera.transform.position - PanObject.transform.position;
    _scip.LookAt = PanObject.transform.position;
    _scip.Up = mainCamera.transform.up;
  }

  public void SetPosition(Vector3 pos)
  {
    mainCamera.transform.position = pos;
    _scip.Position = pos;
  }

  public void SetLookAt(Vector3 lookAtVec)
  {
    mainCamera.transform.LookAt(lookAtVec, mainCamera.transform.up);
    _scip.LookAt = lookAtVec;
  }

  public void SetUp(Vector3 upVec)
  {
    mainCamera.transform.up = upVec;
    _scip.Up = upVec;
  }

  public void SetProjection(CameraProjection cp)
  {
    /*if (cp == CameraProjection.Perspective)
    {
      mainCamera.orthographic = false;
    }
    else
    {
      mainCamera.orthographic = true;
    }*/
    _scip.Projection = cp;
    recalcProjection();
  }

  public void SetFOV(float fov)
  {
    //mainCamera.fieldOfView = fov;
    _scip.FOV = fov;
    recalcProjection();
  }

  public void SetSize(float size)
  {
    //mainCamera.orthographicSize = size;
    _scip.Size = size;
    recalcProjection();
  }

  private void recalcProjection()
  {
    if (SCIP.Projection == CameraProjection.Orthographic)
    {
      this.mainCamera.orthographic = true;
      this.mainCamera.orthographicSize = SCIP.Size;
    }
    else if (SCIP.Projection == CameraProjection.Perspective)
    {
      this.mainCamera.orthographic = false;
      this.mainCamera.fieldOfView = SCIP.FOV;
    }
  }

  private void SmoothTransform()
  {

  }

  private void SmoothMoveFloatParam(float current, float target, float smoothness)
  {

  }

  private void SmoothMoveVector3Param(Vector3 current, Vector3 target, float smoothness)
  {

  }
}
