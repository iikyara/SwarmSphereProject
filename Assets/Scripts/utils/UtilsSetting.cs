using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsSetting : MonoBehaviour
{
  public ComputeShader UtilsComputeShader;

  /*// Start is called before the first frame update
  void Start()
  {
    Debug.Log(this.UtilsComputeShader);
    GPGPUUtils.UtilsComputeShader = this.UtilsComputeShader;
    Debug.Log(GPGPUUtils.UtilsComputeShader);
  }*/

  private void Awake()
  {
    //Debug.Log(this.UtilsComputeShader);
    GPGPUUtils.UtilsComputeShader = this.UtilsComputeShader;
    //Debug.Log(GPGPUUtils.UtilsComputeShader);
  }
}
