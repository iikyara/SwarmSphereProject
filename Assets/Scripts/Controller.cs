using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
  //public List<Agent> Agents;
  public List<(Agent, ParticleObject)> Agents;
  public Material ParticleMaterial;
  public GameObject ParentObject;

  public int NumSwarmAgent = 5;

  private SwarmViewer view;

  // Start is called before the first frame update
  void Start()
  {
    //this.View = new SwarmViewer();
    //this.Agents = new List<Agent>();
    this.Agents = new List<(Agent, ParticleObject)>();

    this.AddAnyKindAgent<SwarmAgent>(NumSwarmAgent);
  }

  // Update is called once per frame
  void Update()
  {
    Debug.Log("Controller : Update");
    foreach((Agent agent, ParticleObject particleObject) agent in this.Agents)
    {
      agent.agent.Update();
      agent.particleObject.UpdateSphere();
    }
    //SwarmAgent.FindNearAgentsTuple(this.Agents);
    Debug.Log(((SwarmAgent)(this.Agents[0].Item1)).Swarm.ToStringReflection());
  }

  private void AddAnyKindAgent<Type>(int num) where Type : Agent, new()
  {
    for(int i = 0; i < num; i++)
    {
      Agent agent = (Agent)new Type();
      ParticleObject particleObject = new ParticleObject(agent, this.ParticleMaterial, this.ParentObject);
      this.Agents.Add((agent, particleObject));
    }
  }
}
