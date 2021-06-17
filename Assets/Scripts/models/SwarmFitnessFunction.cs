using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmFitnessFunction : FitnessFunction
{
  public List<SwarmAgent> nearAgents = new List<SwarmAgent>();

  public override float getFitness(Particle particle)
  {
    
    Vector3 point = new Vector3(5f, 5f, 5f);
    Vector3 point2 = new Vector3(-5f, -5f, -5f);
    return Random.Range(0.0f, 1.0f);
    //return -(particle.Position - point).magnitude - (particle.Position - point2).magnitude;
    /*
    float sum = 0f;
    foreach(SwarmAgent agent in nearAgents)
    {
      sum += -(particle.Position - agent.Position).magnitude;
    }
    return sum;
    */
  }
}
