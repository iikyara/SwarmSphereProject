using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwarmAgent : Agent
{
  public Swarm Swarm;
  private int _numParticles;
  private SwarmFitnessFunction _function;
  public List<SwarmAgent> NearAgents;
  public bool is_exist = true;

  public SwarmAgent() : this(10)
  {
  }

  public SwarmAgent(
    int numParticles
  ) : base(name : "Swarm Agent")
  {
    this._function = new SwarmFitnessFunction();
    this.Swarm = new Swarm(numParticles, this._function);
    this._numParticles = numParticles;
    this.Size = 0.1f;
    this.NearAgents = new List<SwarmAgent>();
  }

  public SwarmAgent(
    int numParticles,
    bool is_exist
  ) : this(numParticles: numParticles)
  {
    this.is_exist = is_exist;
  }

  public SwarmAgent(
    int numParticles,
    bool is_exist,
    SwarmFitnessFunction swarmFitnessFunction
  ) : this(numParticles : numParticles, is_exist : is_exist)
  {
    this._function = swarmFitnessFunction;
  }

  /// <summary>
  /// コピーコンストラクタ
  /// </summary>
  /// <param name="baseSwarmAgent"></param>
  public SwarmAgent(SwarmAgent baseSwarmAgent)
  {
    this.Swarm = new Swarm(baseSwarmAgent.Swarm);
    this._numParticles = baseSwarmAgent._numParticles;
    this._function = baseSwarmAgent._function;
    this.NearAgents = new List<SwarmAgent>(baseSwarmAgent.NearAgents);
    this.is_exist = baseSwarmAgent.is_exist;
  }

  public override void Update()
  {
    this.Swarm.Update();
    this.Position = this.Swarm.BestPosition;
  }

  public override void Reset()
  {
    this.Swarm = new Swarm(this._numParticles, this._function);
  }

  public override object Clone()
  {
    return new SwarmAgent(this);
  }

  public static void FindNearAgentsTuple(List<(Agent, ParticleObject)> agents)
  {
    List<SwarmAgent> swarmAgents = new List<SwarmAgent>();
    foreach((Agent, ParticleObject) agent in agents)
    {
      if(agent.Item1.Name.Equals("Swarm Agent"))
      {
        swarmAgents.Add((SwarmAgent)agent.Item1);
      }
    }
    SwarmAgent.FindNearAgents(swarmAgents);
  }

  public static void FindNearAgents(List<SwarmAgent> agents)
  {
    SwarmAgent[] sorted = agents.OrderBy(p => p.Position.x).ToArray();
    for (int i = 0; i < sorted.Length; i++)
    {
      sorted[i].NearAgents.Clear();
    }

    for (int i = 0; i < sorted.Length - 1; i++)
    {
      for(int j = i + 1; j < sorted.Length; j++)
      {
        float d = (sorted[j].Position - sorted[i].Position).sqrMagnitude;
        if(d < SwarmSetting.RANGE_OF_DISTANCE_BETWEEN_PARTICLES)
        {
          sorted[i].NearAgents.Add(sorted[j]);
          sorted[j].NearAgents.Add(sorted[i]);
        }
      }
      sorted[i]._function.nearAgents = sorted[i].NearAgents;
    }
  }
}
