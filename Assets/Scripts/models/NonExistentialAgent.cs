using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonExistentialAgent : SwarmAgent
{
  public NonExistentialAgent()
  {
    this.Name = "Non-Existential Agent";
    this.is_exist = false;
  }

  public NonExistentialAgent(NonExistentialAgent instanse) : base((SwarmAgent)instanse)
  {

  }

  public override object Clone()
  {
    return new NonExistentialAgent(this);
  }
}
