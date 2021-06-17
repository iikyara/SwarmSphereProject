using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkParamaterManager : MonoBehaviour
{
  public List<MGP> Paramaters;

  private void Awake()
  {
    Paramaters = new List<MGP>();
    foreach (var cgo in Utils.GetAllChildren(this.transform.gameObject))
    {
      //if (!cgo.activeSelf) continue;
      if (!cgo.activeInHierarchy) continue;
      if (cgo)
      {
        var bc = cgo.GetComponent<MGP>();
        if (bc != null) Paramaters.Add(bc);
      }
    }
  }
}
