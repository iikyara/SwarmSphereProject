using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MGP : MonoBehaviour
{
  public ModelGenerater TargetModelGenerater;

  public abstract object GetParam();

  public abstract string GetCSVHeader();

  public abstract string ToCSV();
}
