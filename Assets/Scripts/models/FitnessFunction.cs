using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FitnessFunction
{
  /*
  public float getFitness(Particle particle)
  {
    Vector3 point = new Vector3(10f,10f,10f);
    Vector3 point2 = new Vector3(-5f, 5f, -5f);
    //return Random.Range(0.0f, 1.0f);
    return -(particle.Position - point).magnitude
      - (particle.Position - point2).magnitude;
  }
  */
  public abstract float getFitness(Particle particle);
}
