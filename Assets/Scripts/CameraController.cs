using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  //properties
  public GameObject Pan;
  public GameObject Tilt;
  public GameObject Camera;

  public bool Pan_inversed = false;
  public bool Tilt_inversed = true;
  public bool Scale_inversed = true;

  public float speed = 1.0f;
  public float scale_speed = 1.0f;

  private Camera thisCamera;

  // Start is called before the first frame update
  void Start()
  {
    thisCamera = GetComponent<Camera>();
  }

  // Update is called once per frame
  void Update()
  {
    // rotate
    if (Input.GetButton("Fire1"))
    {
      float pan = Input.GetAxisRaw("Mouse X") * speed * (Pan_inversed ? -1 : 1);
      float tilt = Input.GetAxisRaw("Mouse Y") * speed * (Tilt_inversed ? -1 : 1);

      Pan.transform.Rotate(new Vector3(0, pan, 0));
      Tilt.transform.Rotate(new Vector3(tilt, 0, 0));
      /*
      Debug.Log(Tilt.transform.localRotation.eulerAngles.x);

      if (180 > Tilt.transform.localRotation.eulerAngles.x
        && Tilt.transform.localRotation.eulerAngles.x > 85)
      {
        Tilt.transform.localRotation.eulerAngles.Set(
          85,
          Tilt.transform.localRotation.eulerAngles.y,
          Tilt.transform.localRotation.eulerAngles.z
        );

        Debug.Log("180 > x > 85");
      }
      if (180 < Tilt.transform.localRotation.eulerAngles.x
        && Tilt.transform.localRotation.eulerAngles.x < 275)
      {
        Tilt.transform.localRotation.eulerAngles.Set(
          275,
          Tilt.transform.localRotation.eulerAngles.y,
          Tilt.transform.localRotation.eulerAngles.z
        );
        Debug.Log("180 < x < 275");
      }
      */
    }

    float scale = Input.GetAxisRaw("Mouse ScrollWheel");

    Camera.transform.Translate(new Vector3(
      0,
      0,
      Camera.transform.localPosition.z * scale_speed * scale * (Scale_inversed ? -1 : 1)
    ));
  }
}
