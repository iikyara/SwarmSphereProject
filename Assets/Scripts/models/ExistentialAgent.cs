using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistentialAgent : SwarmAgent
{
  public ExistentialAgent()
  {
    this.Name = "Existential Agent";
    this.is_exist = true;
  }

  public ExistentialAgent(ExistentialAgent instanse) : base((SwarmAgent)instanse)
  {

  }

  public override object Clone()
  {
    return new ExistentialAgent(this);
  }
}
