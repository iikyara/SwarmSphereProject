using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour
{
  public IClickable Instance { get; set; }

  public void OnClick()
  {
    //Debug.Log("Clickable is Clicked.");
    if (Instance != null) Instance.OnClick();
  }
}


public interface IClickable
{
  void OnClick();
}