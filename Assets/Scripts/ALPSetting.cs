using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ALPSetting : MonoBehaviour
{
  public Vector3 IndividualPositionMin;
  public Vector3 IndividualPositionMax;
  public Vector3 IndividualPositionMutationRange;

  private void Awake()
  {
    SetALPIndividual();
  }

  private void SetALPIndividual()
  {
    ModelGenerateController.SetAgentSetting(
      IndividualPositionMin,
      IndividualPositionMax,
      IndividualPositionMutationRange
    );
  }
}
