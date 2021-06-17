using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkCameraManager : MonoBehaviour
{
  public List<BenchmarkCameraSet> CameraSetList;

  private void Awake()
  {
    CameraSetList = new List<BenchmarkCameraSet>();
    foreach(Transform ct in this.GetComponentInChildren<Transform>())
    {
      var cgo = ct.gameObject;
      if (!cgo.activeSelf) continue;
      if (cgo)
      {
        var bc = cgo.GetComponent<BenchmarkCameraSet>();
        if (bc != null) CameraSetList.Add(bc);
      }
    }
  }
}
