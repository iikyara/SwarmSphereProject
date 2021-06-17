using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonoBehavior : MonoBehaviour
{
  public void TestMethod()
  {
    Debug.Log("Hello, World");
  }

  private void OnMouseDown()
  {
    Debug.Log("Test : Mouse Down");
  }
}
