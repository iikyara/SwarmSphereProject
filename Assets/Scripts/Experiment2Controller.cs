using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentTarget
{
  test,
  monkey,
  toras,
  question,
  SpheresCube,
  SpheresCube2,
  Sphere,
  Cube,
  Conical,
  Triangle,
  Option
}

public class Experiment2Controller : MonoBehaviour
{
  //public List<Agent> Agents;
  public List<(Agent, ParticleObject)> Agents;
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material RelationEdgeMaterial;
  public Material EdgeMaterial;
  public Material MeshMaterial;

  public Material BaseMeshMaterial;
  public Material DisabledParticleMaterial;
  public GameObject ParentObject;

  public bool viewAgent;
  public bool viewDelaunayLine;
  public bool viewVoronoiLine;
  public bool viewVoronoiMesh;

  public bool viewBaseDelaunayLines;
  public bool viewBaseVoronoiLines;
  public bool viewBaseVoronoiMeshs;

  public bool CreateVoronoi;

  public AgentTarget agentTarget;

  private bool previousViewAgent;
  private bool previousViewDelaunayLine;
  private bool previousViewVoronoiLine;
  private bool previousViewVoronoimesh;

  private bool previousViewBaseDelaunayLines;
  private bool previousViewBaseVoronoiLines;
  private bool previousViewBaseVoronoiMeshs;

  private VoronoiPartition voronoiPartition;
  private List<VoronoiPartition> baseVoronoiPartition;
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

    this.previousViewAgent = !this.viewAgent;
    this.previousViewDelaunayLine = !this.viewDelaunayLine;
    this.previousViewVoronoiLine = !this.viewVoronoiLine;
    this.previousViewVoronoimesh = !this.viewVoronoiMesh;
    this.previousViewBaseDelaunayLines = !this.viewBaseDelaunayLines;
    this.previousViewBaseVoronoiLines = !this.viewBaseVoronoiLines;
    this.previousViewBaseVoronoiMeshs = !this.viewBaseVoronoiMeshs;

    this.particles = new GameObject("Particles");
    this.derauney = new GameObject("Derauney");
    this.voronoi = new GameObject("Voronoi");
    Utils.SetParent(this.ParentObject, this.particles);
    Utils.SetParent(this.ParentObject, this.derauney);
    Utils.SetParent(this.ParentObject, this.voronoi);

    //this.AddAnyKindAgent<SwarmAgent>(NumAgent, ExistentialParticleMaterial);
    //this.AddAgentsLocatedAsCube(1f, new Vector3(0, 1, 2));
    List<(Agent, ParticleObject)>[] locatedAgents;
    if(this.agentTarget == AgentTarget.test)
    {
      locatedAgents = AddAgentsTest();
    }
    else if (this.agentTarget == AgentTarget.question)
    {
      locatedAgents = AddAgentsLocatedAsQuestion();
    }
    else if(this.agentTarget == AgentTarget.toras)
    {
      locatedAgents = AddAgentsLocatedAsToras();
    }
    else if (this.agentTarget == AgentTarget.monkey)
    {
      locatedAgents = AddAgentsLocatedAsMonkey();
    }
    else if (this.agentTarget == AgentTarget.SpheresCube)
    {
      locatedAgents = AddAgentsLocatedAsCube();
    }
    else if (this.agentTarget == AgentTarget.SpheresCube2)
    {
      locatedAgents = AddAgentsLocatedAsCube2();
    }
    else if (this.agentTarget == AgentTarget.Sphere)
    {
      locatedAgents = new List<(Agent, ParticleObject)>[] { this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 20, 10) };
    }
    else if (this.agentTarget == AgentTarget.Cube)
    {
      locatedAgents = new List<(Agent, ParticleObject)>[] { this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 4, 3) };
    }
    else if (this.agentTarget == AgentTarget.Conical)
    {
      locatedAgents = new List<(Agent, ParticleObject)>[] { this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 3, 2) };
    }
    else if (this.agentTarget == AgentTarget.Triangle)
    {
      locatedAgents = new List<(Agent, ParticleObject)>[] { this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 3, 3) };
    }
    else
    {
      locatedAgents = AddAgentsTest();
    }

    this.baseVoronoiPartition = new List<VoronoiPartition>();
    foreach(var la in locatedAgents)
    {
      this.baseVoronoiPartition.Add(new VoronoiPartition(la, RelationEdgeMaterial, EdgeMaterial, BaseMeshMaterial, derauney, voronoi));
    }

    this.voronoiPartition = new VoronoiPartition(this.Agents, RelationEdgeMaterial, EdgeMaterial, MeshMaterial, derauney, voronoi);

    //ランダムに配置
    /*LocateAgentRandomly(10, 10, 10);
    foreach (var agent in this.Agents)
    {
      agent.Item2.UpdateSphere();
    }*/

    //ClassificationPoints();
  }

  /// <summary>
  /// rect範囲内でランダムにエージェントを配置する．
  /// </summary>
  /// <param name="width"></param>
  /// <param name="height"></param>
  /// <param name="depth"></param>
  private void LocateAgentRandomly(float width, float height, float depth)
  {
    foreach (var agent in this.Agents)
    {
      agent.Item1.Position = new Vector3(
        Random.Range(-depth / 2, depth / 2),
        Random.Range(-width / 2, width / 2),
        Random.Range(-height / 2, height / 2)
      );
    }
  }

  private List<(Agent, ParticleObject)>[] AddAgentsTest()
  {
    return new List<(Agent, ParticleObject)>[]
    {
      //this.AddAgentsLocatedAsSphere(5f, new Vector3(-5, 3, 0), Quaternion.AngleAxis(60f, new Vector3(1,1,0)), 4, 3),
      //this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 3, -5), Quaternion.Euler(60f, 60f, 0f), 5, 3),
      this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), new Quaternion(), 4, 3),
      //this.AddAgentsLocatedAsSphere(5f, new Vector3(5, -3, 0), new Quaternion(), 6, 6),
      //this.AddAgentsLocatedAsSphere(5f, new Vector3(0, -3, 5), new Quaternion(), 12, 12)
    };
  }

  private List<(Agent, ParticleObject)>[] AddAgentsLocatedAsQuestion()
  {
    float r_1 = -57f;
    float d_1 = 0.3f;
    float d_2 = 0.3f;
    float r_3 = 55f;
    float d_3 = 0.2f;
    float r_4 = 78f;
    float d_4 = 0.45f;
    float r_5 = -10f;
    float r_6 = -10f;
    float d_6 = 0.3f;
    float r_7 = 10f;
    float d_7 = 0.3f;
    return new List<(Agent, ParticleObject)>[]
    {
      // 0
      this.AddAgentsLocatedAsSphere(1.5f, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), 4, 3),

      // 1
      //this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.2f, 0.6f, d_1), Quaternion.Euler(0f, 0f, r_1), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(0.5f, 1.0f, d_1), Quaternion.Euler(0f, 0f, r_1), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1f, 1.3f, d_1), Quaternion.Euler(0f, 0f, r_1 + 2), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1.5f, 1.6f, d_1), Quaternion.Euler(0f, 0f, r_1 + 4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(2.0f, 1.9f, d_1), Quaternion.Euler(0f, 0f, r_1 + 4), 4, 3),
      //this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.2f, 0.6f, -d_1), Quaternion.Euler(0f, 0f, r_1), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(0.5f, 1.0f, -d_1), Quaternion.Euler(0f, 0f, r_1), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1f, 1.3f, -d_1), Quaternion.Euler(0f, 0f, r_1 + 2), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1.5f, 1.6f, -d_1), Quaternion.Euler(0f, 0f, r_1 + 4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(2.0f, 1.9f, -d_1), Quaternion.Euler(0f, 0f, r_1 + 4), 4, 3),

      //2
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1.8f, 2.5f, d_2), Quaternion.Euler(0f, 0f, 0f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(2.5f, 2.5f, d_2), Quaternion.Euler(0f, 0f, 0f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(1.8f, 2.5f, -d_2), Quaternion.Euler(0f, 0f, 0f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(2.5f, 2.5f, -d_2), Quaternion.Euler(0f, 0f, 0f), 4, 3),

      //3
      this.AddAgentsLocatedAsSphere(1.0f, new Vector3(2.1f, 3.0f, d_3), Quaternion.Euler(0f, 0f, r_3), 4, 3),
      this.AddAgentsLocatedAsSphere(1.0f, new Vector3(2.1f, 3.0f, -d_3), Quaternion.Euler(0f, 0f, r_3), 4, 3),

      //4
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.7f, 3.5f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.6f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(0.5f, 3.7f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.1f, 3.8f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.7f, 3.9f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      //this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-1.3f, 4.0f, d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.7f, 3.5f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.6f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(0.5f, 3.7f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.1f, 3.8f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.7f, 3.9f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      //this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-1.3f, 4.0f, 0), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.7f, 3.5f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.6f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(0.5f, 3.7f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.1f, 3.8f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-0.7f, 3.9f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),
      //this.AddAgentsLocatedAsSphere(0.5f, new Vector3(-1.3f, 4.0f, -d_4), Quaternion.Euler(0f, 0f, r_4), 4, 3),

      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.5f, 3.1f, d_4), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.5f, d_4), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.5f, 3.1f, 0), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.5f, 0), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.5f, 3.1f, -d_4), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.5f, new Vector3(1.1f, 3.5f, -d_4), Quaternion.Euler(0f, 0f, r_4 - 30f), 4, 3),

      //5
      this.AddAgentsLocatedAsSphere(1.4f, new Vector3(-1.5f, 3.6f, 0f), Quaternion.Euler(0f, 0f, r_5), 4, 3),

      //6
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.9f, 2.8f, d_6), Quaternion.Euler(0f, 0f, r_6 + 20f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.7f, 2.6f, d_6), Quaternion.Euler(0f, 0f, r_6), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.2f, 2.5f, d_6), Quaternion.Euler(0f, 0f, r_6), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.9f, 2.8f, -d_6), Quaternion.Euler(0f, 0f, r_6 + 20f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.7f, 2.6f, -d_6), Quaternion.Euler(0f, 0f, r_6), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-1.2f, 2.5f, -d_6), Quaternion.Euler(0f, 0f, r_6), 4, 3),

      //7
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.8f, 2.6f, d_7), Quaternion.Euler(0f, 0f, r_7 + 50f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.2f, 2.4f, d_7), Quaternion.Euler(0f, 0f, r_7), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.8f, 2.6f, -d_7), Quaternion.Euler(0f, 0f, r_7 + 50f), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.2f, 2.4f, -d_7), Quaternion.Euler(0f, 0f, r_7), 4, 3),

      //8
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.3f, -1.9f, 0.4f), Quaternion.Euler(0f, 0f, 0), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(0.4f, -1.9f, 0.4f), Quaternion.Euler(0f, 0f, 0), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(-0.3f, -1.9f, -0.4f), Quaternion.Euler(0f, 0f, 0), 4, 3),
      this.AddAgentsLocatedAsSphere(0.8f, new Vector3(0.4f, -1.9f, -0.4f), Quaternion.Euler(0f, 0f, 0), 4, 3),
    };
  }

  private List<(Agent, ParticleObject)>[] AddAgentsLocatedAsToras()
  {
    int sep = 8;
    //int sepsep = 3;
    float d = 3.0f;
    float r = 1.5f;
    //float l = 2.5f;
    var result = new List<List<(Agent, ParticleObject)>>();
    for(int i = 0; i < sep; i++)
    {
      float x = d * Mathf.Sin((float)i / sep * 2.0f * Mathf.PI);
      float z = d * Mathf.Cos((float)i / sep * 2.0f * Mathf.PI);
      float rot = (float)i / sep * 360f + 90f;
      result.Add(this.AddAgentsLocatedAsSphere(r, new Vector3(x, 0, z), Quaternion.Euler(90f, rot, 0f), sep, 3));
    }
    return result.ToArray();
  }

  private List<(Agent, ParticleObject)>[] AddAgentsLocatedAsMonkey()
  {
    return new List<(Agent, ParticleObject)>[]
    {
      this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 3, -5), Quaternion.Euler(60f, 60f, 0f), 5, 3),
    };
  }

  private List<(Agent, ParticleObject)>[] AddAgentsLocatedAsCube()
  {
    var result = new List<List<(Agent, ParticleObject)>>();
    int tate = 5;
    int yoko = 5;
    int takasa = 5;
    float r = 0.5f;
    for (int i = 0; i < tate; i++)
    {
      float x = ((float)i / (tate - 1) - 0.5f) * r * tate;
      for(int j = 0; j < yoko; j++)
      {
        float y = ((float)j / (yoko - 1) - 0.5f) * r * yoko;
        for (int k = 0; k < takasa; k++)
        {
          float z = ((float)k / (takasa - 1) - 0.5f) * r * takasa;
          result.Add(this.AddAgentsLocatedAsSphere(r, new Vector3(x, y, z), Quaternion.Euler(0f, 0f, 0f), 10, 5));
        }
      }
    }
    return result.ToArray();
  }

  private List<(Agent, ParticleObject)>[] AddAgentsLocatedAsCube2()
  {
    var result = new List<List<(Agent, ParticleObject)>>();
    int sep = 10;
    float r = 5f;
    int sep_h = 20;
    int sep_v = 10;

    //中心の球
    result.Add(this.AddAgentsLocatedAsSphere(2 * r, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
    for (int i = 0; i < sep - 1; i++)
    {
      float dt = r / sep * (i + 1);
      float rt = r - dt;
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(dt, dt, dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(dt, dt, -dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(dt, -dt, dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(dt, -dt, -dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(-dt, dt, dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(-dt, dt, -dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(-dt, -dt, dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
      result.Add(this.AddAgentsLocatedAsSphere(2 * rt, new Vector3(-dt, -dt, -dt), Quaternion.Euler(0f, 0f, 0f), sep_h, sep_v));
    }
    return result.ToArray();
  }

  private List<(Agent, ParticleObject)> AddAgentsLocatedAsCube(float size, Vector3 position, Quaternion quaternion)
  {
    List<Vector3> existential = new List<Vector3> {
      new Vector3(0f, 0f, 0f)
    };
    List<Vector3> nonexistential = new List<Vector3> {
      new Vector3(1f, 0f, 0f),
      new Vector3(-1f, 0f, 0f),
      new Vector3(0f, 1f, 0f),
      new Vector3(0f, -1f, 0f),
      new Vector3(0f, 0f, 1f),
      new Vector3(0f, 0f, -1f)
    };
    TransformPositions(ref existential, size, position, quaternion);
    TransformPositions(ref nonexistential, size, position, quaternion);
    return AddAgentsLocated(existential, nonexistential);
  }

  private List<(Agent, ParticleObject)> AddAgentsLocatedAsSphere(float size, Vector3 position, Quaternion quaternion, int sep_h, int sep_v)
  {
    List<Vector3> existential = new List<Vector3> {
      new Vector3(0f, 0f, 0f)
    };
    List<Vector3> nonexistential = new List<Vector3>();
    float conicalOffset = 0;
    if(sep_v > 2)
    {
      nonexistential.Add(new Vector3(0f, 1f, 0f));
    }
    else
    {
      sep_v++;
      conicalOffset = size / 7;
    }
    
    for (int i = 1; i < sep_v - 1; i++)
    {
      for (int j = 0; j < sep_h; j++)
      {
        float thita = i * Mathf.PI / (sep_v - 1);
        float phi = j * 2 * Mathf.PI / sep_h;
        nonexistential.Add(new Vector3(
          Mathf.Sin(phi) * Mathf.Sin(thita),
          Mathf.Cos(thita) + conicalOffset,
          Mathf.Cos(phi) * Mathf.Sin(thita)
        ));
      }
    }
    nonexistential.Add(new Vector3(0f, -1f, 0f));

    TransformPositions(ref existential, size, position, quaternion);
    TransformPositions(ref nonexistential, size, position, quaternion);
    return AddAgentsLocated(existential, nonexistential);
  }

  private void TransformPositions(ref List<Vector3> positions, float size, Vector3 position, Quaternion quaternion)
  {
    for (int i = 0; i < positions.Count; i++)
    {
      positions[i] *= size;
      //回転
      var x = quaternion.x;
      var y = quaternion.y;
      var z = quaternion.z;
      var w = quaternion.w;
      positions[i] = new Matrix4x4(
        new Vector4(1-2*y*y-2*z*z, 2*x*y+2*w*z, 2*x*z-2*w*y, 0),
        new Vector4(2*x*y-2*w*z, 1-2*x*x-2*z*z, 2*y*z+2*w*x, 0),
        new Vector4(2*x*z+2*w*y, 2*y*z-2*w*x, 1-2*x*x-2*y*y, 0),
        new Vector4(0, 0, 0, 1)
      ) * positions[i];
      positions[i] += position;
      positions[i] = new Vector3(
        positions[i].x + Random.Range(-0.01f, 0.01f),
        positions[i].y + Random.Range(-0.01f, 0.01f),
        positions[i].z + Random.Range(-0.01f, 0.01f)
      );
    }
  }

  private List<(Agent, ParticleObject)> AddAgentsLocated(List<Vector3> existential, List<Vector3> nonexistential)
  {
    List<(Agent, ParticleObject)> addedAgents = new List<(Agent, ParticleObject)>();
    addedAgents.AddRange(AddAnyAgentsLocated<ExistentialAgent>(existential, ExistentialParticleMaterial));
    addedAgents.AddRange(AddAnyAgentsLocated<NonExistentialAgent>(nonexistential, NonExistentialParticleMaterial));
    return addedAgents;
  }

  private List<(Agent, ParticleObject)> AddAnyAgentsLocated<Type>(List<Vector3> positions, Material mat) where Type : Agent, new()
  {
    List<(Agent, ParticleObject)> addedAgents = new List<(Agent, ParticleObject)>();
    int offset = this.Agents.Count;
    AddAnyKindAgent<Type>(positions.Count, mat);
    for (int i = 0; i < positions.Count; i++)
    {
      this.Agents[offset + i].Item1.Position = positions[i];
      addedAgents.Add(this.Agents[offset + i]);
    }
    return addedAgents;
  }

  // Update is called once per frame
  [System.Obsolete]
  void Update()
  {
    //Debug.Log("Controller : Update");

    //エージェントの更新
    foreach ((Agent agent, ParticleObject particleObject) agent in this.Agents)
    {
      agent.particleObject.UpdateSphere();
    }

    //ドロネー図の作成
    if (CreateVoronoi)
    {
      MyStopwatch sw = new MyStopwatch();
      sw.Start();

      this.voronoiPartition.Reset();

      foreach (var vp in this.baseVoronoiPartition)
      {
        //vp.Recreate();
        vp.CreateDelaunayWithThinOutPoints();
        //this.voronoiPartition.JoinVoronoiPartition(vp, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
        //this.voronoiPartition.JoinVoronoiPartition2(vp, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
        //this.voronoiPartition.JoinVoronoiPartition3(vp, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
        //this.voronoiPartition.JoinVoronoiPartition4(vp, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
        this.voronoiPartition.JoinVoronoiPartition4_parallel(vp, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
      }
      //this.voronoiPartition.Recreate();
      this.voronoiPartition.CreateDelaunayWithThinOutPoints();
      CreateVoronoi = false;

      sw.Stop();
      sw.ShowResult("All");
    }

    //可視化の設定
    if (this.previousViewAgent != this.viewAgent)
    {
      Debug.Log("change view agent : " + this.viewAgent.ToString());
      foreach (var agent in this.Agents)
      {
        agent.Item2.SetActive(this.viewAgent);
      }
      this.previousViewAgent = this.viewAgent;
    }

    if (this.previousViewDelaunayLine != this.viewDelaunayLine)
    {
      Debug.Log("change view delaunay line : " + this.viewDelaunayLine.ToString());
      this.voronoiPartition.SetVisibleDelaunayLine(this.viewDelaunayLine);
      this.previousViewDelaunayLine = this.viewDelaunayLine;
    }
    if (this.previousViewVoronoiLine != this.viewVoronoiLine)
    {
      Debug.Log("change view voronoi line : " + this.viewVoronoiLine.ToString());
      this.voronoiPartition.SetVisibleVoronoiLine(this.viewVoronoiLine);
      this.previousViewVoronoiLine = this.viewVoronoiLine;
    }
    if (this.previousViewVoronoimesh != this.viewVoronoiMesh)
    {
      Debug.Log("change view voronoi mesh : " + this.viewVoronoiMesh.ToString());
      this.voronoiPartition.SetVisibleVoronoiMesh(this.viewVoronoiMesh);
      this.previousViewVoronoimesh = this.viewVoronoiMesh;
    }
    if (this.previousViewBaseDelaunayLines != this.viewBaseDelaunayLines)
    {
      Debug.Log("change view delaunay line : " + this.viewBaseDelaunayLines.ToString());
      foreach(var vp in this.baseVoronoiPartition)
      {
        vp.SetVisibleDelaunayLine(this.viewBaseDelaunayLines);
      }
      this.previousViewBaseDelaunayLines = this.viewBaseDelaunayLines;
    }
    if (this.previousViewBaseVoronoiLines != this.viewBaseVoronoiLines)
    {
      Debug.Log("change view voronoi line : " + this.viewBaseVoronoiLines.ToString());
      foreach (var vp in this.baseVoronoiPartition)
      {
        vp.SetVisibleVoronoiLine(this.viewBaseVoronoiLines);
      }
      this.previousViewBaseVoronoiLines = this.viewBaseVoronoiLines;
    }
    if (this.previousViewBaseVoronoiMeshs != this.viewBaseVoronoiMeshs)
    {
      Debug.Log("change view voronoi mesh : " + this.viewBaseVoronoiMeshs.ToString());
      foreach (var vp in this.baseVoronoiPartition)
      {
        vp.SetVisibleVoronoiMesh(this.viewBaseVoronoiMeshs);
      }
      this.previousViewBaseVoronoiMeshs = this.viewBaseVoronoiMeshs;
    }
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
