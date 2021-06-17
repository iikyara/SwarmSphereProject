using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ViewController : MonoBehaviour
{
  public GameObject ImageObject;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void CreateImageObject(RenderTexture rtex)
  {
    var ri = ImageObject.GetComponent<RawImage>();
  }
}
