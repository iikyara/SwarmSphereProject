using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelViewCamera : MonoBehaviour
{
  private SketchAndCameraInitializationParam _scip;
  public SketchAndCameraInitializationParam SCIP
  {
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

  public Camera thisCamera;

  private void Start()
  {
    this.thisCamera = this.GetComponent<Camera>();
  }

  public void SetPosition(Vector3 pos)
  {
    this.transform.position = pos;
    _scip.Position = pos;
  }

  public void SetLookAt(Vector3 lookAtVec)
  {
    this.transform.LookAt(lookAtVec, this.transform.up);
    _scip.LookAt = lookAtVec;
  }

  public void SetUp(Vector3 upVec)
  {
    this.transform.up = upVec;
    _scip.Up = upVec;
  }

  public void SetProjection(CameraProjection cp)
  {
    _scip.Projection = cp;
    recalcProjection();
  }

  public void SetFOV(float fov)
  {
    _scip.FOV = fov;
    recalcProjection();
  }

  public void SetSize(float size)
  {
    _scip.Size = size;
    recalcProjection();
  }

  private void recalcProjection()
  {
    if (SCIP.Projection == CameraProjection.Orthographic)
    {
      this.thisCamera.orthographic = true;
      this.thisCamera.orthographicSize = SCIP.Size;
    }
    else if (SCIP.Projection == CameraProjection.Perspective)
    {
      this.thisCamera.orthographic = false;
      this.thisCamera.fieldOfView = SCIP.FOV;
    }
  }
}
