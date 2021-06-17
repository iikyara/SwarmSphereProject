using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRotation : MonoBehaviour
{
  public GameObject kijun;
  public float depth;
  public Vector2 offset;
  public GameObject CameraObject;
  // Start is called before the first frame update
  void Start()
  {
        
  }

  // Update is called once per frame
  void Update()
  {
    try
    {
      transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 200 + offset.x, Screen.height + offset.y, depth));
      transform.LookAt(new Vector3(0.27f, 0.45f, 10000f));
      /*transform.LookAt(CameraObject.transform.position);
      transform.Rotate(-CameraObject.transform.rotation.eulerAngles);*/
    }
    catch
    {
      //Debug.Log("CubeRotation - Transform error");
    }
  }
}
