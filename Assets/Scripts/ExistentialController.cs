using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocateAgent
{
  Sphere,
  Square
}

public class ExistentialController : MonoBehaviour
{
  //public List<Agent> Agents;
  public List<(Agent, ParticleObject)> Agents;
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material RelationEdgeMaterial;
  public Material EdgeMaterial;
  public Material MeshMaterial;
  public GameObject ParentObject;

  public bool viewDelaunayLine;
  public bool viewVoronoiLine;
  public bool viewVoronoiMesh;

  public bool isUpdate;

  public int NumExistentialAgent = 5;
  public int NumNonExistentialAgent = 5;

  public LocateAgent locateAgent = LocateAgent.Sphere;

  private SwarmViewer view;
  private VoronoiPartition voronoiPartition;
  private GameObject particles;
  private GameObject derauney;
  private GameObject voronoi;

  // Start is called before the first frame update
  [System.Obsolete]
  void Start()
  {
    //this.View = new SwarmViewer();
    //this.Agents = new List<Agent>();
    this.Agents = new List<(Agent, ParticleObject)>();

    this.particles = new GameObject("Particles");
    this.derauney = new GameObject("Derauney");
    this.voronoi = new GameObject("Voronoi");
    this.particles.transform.parent = this.ParentObject.transform;
    this.derauney.transform.parent = this.ParentObject.transform;
    this.voronoi.transform.parent = this.ParentObject.transform;

    /*this.AddAnyKindAgent<ExistentialAgent>(NumExistentialAgent, ExistentialParticleMaterial);
    this.AddAnyKindAgent<NonExistentialAgent>(NumNonExistentialAgent, NonExistentialParticleMaterial);*/
    this.AddAnyKindAgent<NonExistentialAgent>(13, NonExistentialParticleMaterial);
    this.AddAnyKindAgent<ExistentialAgent>(1, ExistentialParticleMaterial);
    this.AddAnyKindAgent<NonExistentialAgent>(13, NonExistentialParticleMaterial);

    this.voronoiPartition = new VoronoiPartition(this.Agents, RelationEdgeMaterial, EdgeMaterial, MeshMaterial, derauney, voronoi);
    
    if (locateAgent == LocateAgent.Sphere) LocateAgentLikeSphere(3.0f);
    if (locateAgent == LocateAgent.Square) LocateAgentLikeSquare(3.0f);
  }

  private void LocateAgentLikeSquare(float r)
  {
    int count = 0;
    foreach ((Agent, ParticleObject) agent in this.Agents)
    {
      agent.Item1.Position.x = r * (float)(count % 3 - 1f) + Random.Range(-0.01f, 0.01f);
      agent.Item1.Position.y = r * (float)(count / 9 - 1f) + Random.Range(-0.01f, 0.01f);
      agent.Item1.Position.z = r * (float)(count / 3 % 3 - 1f) + Random.Range(-0.01f, 0.01f);
      count++;
    }
  }

  private void LocateAgentLikeSphere(float r)
  {
    int count = 0;
    foreach ((Agent, ParticleObject) agent in this.Agents)
    {
      float x = count % 3 - 1f;
      float y = count / 9 - 1f;
      float z = count / 3 % 3 - 1f;
      float n = new Vector3(x, y, z).magnitude + 0.001f;
      agent.Item1.Position.x = r * x / n + Random.Range(-0.01f, 0.01f);
      agent.Item1.Position.y = r * y / n + Random.Range(-0.01f, 0.01f);
      agent.Item1.Position.z = r * z / n + Random.Range(-0.01f, 0.01f);
      count++;
    }
  }

  // Update is called once per frame
  [System.Obsolete]
  void Update()
  {
    Debug.Log("Controller : Update");
    foreach ((Agent agent, ParticleObject particleObject) agent in this.Agents)
    {
      if(isUpdate) agent.agent.Update();
      agent.particleObject.UpdateSphere();
    }
    //this.voronoiPartition.Clear();
    //this.voronoiPartition = new VoronoiPartition(this.Agents, EdgeMaterial, MeshMaterial, derauney, voronoi);
    this.voronoiPartition.SetVisible(this.viewDelaunayLine, this.viewVoronoiLine, this.viewVoronoiMesh);
    this.voronoiPartition.Recreate(this.Agents);
    //SwarmAgent.FindNearAgentsTuple(this.Agents);
    /*Debug.Log(((SwarmAgent)(this.Agents[0].Item1)).Swarm.ToStringReflection());*/
  }

  private void AddAnyKindAgent<Type>(int num, Material mat) where Type : Agent, new()
  {
    for (int i = 0; i < num; i++)
    {
      Agent agent = (Agent)new Type();
      ParticleObject particleObject = new ParticleObject(agent, mat, this.particles);
      this.Agents.Add((agent, particleObject));
    }
  }
}
