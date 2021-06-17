using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment1Controller : MonoBehaviour
{
  //public List<Agent> Agents;
  public List<(Agent, ParticleObject)> Agents;
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material RelationEdgeMaterial;
  public Material EdgeMaterial;
  public Material MeshMaterial;
  public GameObject ParentObject;

  public GameObject TargetObject;

  public bool viewAgent;
  public bool viewDelaunayLine;
  public bool viewVoronoiLine;
  public bool viewVoronoiMesh;

  public bool isUpdate;
  public bool CreateVoronoi;
  
  public int NumAgent = 100;

  private bool previousViewAgent;
  private bool previousViewDelaunayLine;
  private bool previousViewVoronoiLine;
  private bool previousViewVoronoimesh;

  public bool CreateVoronoiThinOutPoints;
  public bool viewVoronoiThinOutPointsMesh;
  private bool previousViewVoronoiThinOutPointsMesh;

  private VoronoiPartition voronoiPartition;
  private VoronoiPartition voronoiPartitionThinOutPoints;
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
    this.previousViewVoronoiThinOutPointsMesh = !this.viewVoronoiThinOutPointsMesh;

    this.particles = new GameObject("Particles");
    this.derauney = new GameObject("Derauney");
    this.voronoi = new GameObject("Voronoi");
    this.particles.transform.parent = this.ParentObject.transform;
    this.derauney.transform.parent = this.ParentObject.transform;
    this.voronoi.transform.parent = this.ParentObject.transform;
    this.TargetObject.layer = LayerMask.NameToLayer("Experiment1Target");

    //this.AddAnyKindAgent<SwarmAgent>(NumAgent, ExistentialParticleMaterial);
    //this.AddAgentsLocatedAsCube(1f, new Vector3(0, 1, 2));
    this.AddAgentsLocatedAsSphere(1f, new Vector3(-5, 0, 0), 4, 3);
    this.AddAgentsLocatedAsSphere(1f, new Vector3(0, 0, -5), 5, 3);
    this.AddAgentsLocatedAsSphere(1f, new Vector3(0, 0, 0), 3, 3);
    this.AddAgentsLocatedAsSphere(1f, new Vector3(5, 0, 0), 6, 6);
    this.AddAgentsLocatedAsSphere(1f, new Vector3(0, 0, 5), 12, 12);

    this.voronoiPartition = new VoronoiPartition(this.Agents, RelationEdgeMaterial, EdgeMaterial, MeshMaterial, derauney, voronoi);
    this.voronoiPartitionThinOutPoints = new VoronoiPartition(this.Agents, RelationEdgeMaterial, EdgeMaterial, MeshMaterial, derauney, voronoi);

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
    foreach(var agent in this.Agents)
    {
      agent.Item1.Position = new Vector3(
        Random.Range(-depth / 2, depth / 2),
        Random.Range(-width / 2, width / 2),
        Random.Range(-height / 2, height / 2)
      );
    }
  }

  private void AddAgentsLocatedAsCube(float size, Vector3 position)
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
    TransformPositions(ref existential, size, position);
    TransformPositions(ref nonexistential, size, position);
    AddAgentsLocated(existential, nonexistential);
  }

  private void AddAgentsLocatedAsSphere(float size, Vector3 position, int sep_h, int sep_v)
  {
    List<Vector3> existential = new List<Vector3> {
      new Vector3(0f, 0f, 0f)
    };
    List<Vector3> nonexistential = new List<Vector3>();
    nonexistential.Add(new Vector3(0f, 1f, 0f));
    for(int i = 1; i < sep_v - 1; i++)
    {
      for(int j = 0; j < sep_h; j++)
      {
        float thita = i * Mathf.PI / (sep_v - 1);
        float phi = j * 2 * Mathf.PI / sep_h;
        nonexistential.Add(new Vector3(
          Mathf.Sin(phi) * Mathf.Sin(thita),
          Mathf.Cos(thita),
          Mathf.Cos(phi) * Mathf.Sin(thita)
        ));
      }
    }
    nonexistential.Add(new Vector3(0f, -1f, 0f));

    TransformPositions(ref existential, size, position);
    TransformPositions(ref nonexistential, size, position);
    AddAgentsLocated(existential, nonexistential);
  }

  private void AddAgentsLocated(List<Vector3> existential, List<Vector3> nonexistential)
  {
    AddAnyAgentsLocated<ExistentialAgent>(existential, ExistentialParticleMaterial);
    AddAnyAgentsLocated<NonExistentialAgent>(nonexistential, NonExistentialParticleMaterial);
  }

  private void TransformPositions(ref List<Vector3> positions, float size, Vector3 position)
  {
    for (int i = 0; i < positions.Count; i++)
    {
      positions[i] *= size;
      positions[i] += position;
      positions[i] = new Vector3(
        positions[i].x + Random.Range(-0.01f, 0.01f),
        positions[i].y + Random.Range(-0.01f, 0.01f),
        positions[i].z + Random.Range(-0.01f, 0.01f)
      );
    }
  }

  private void AddAnyAgentsLocated<Type>(List<Vector3> positions, Material mat) where Type : Agent, new()
  {
    int offset = this.Agents.Count;
    AddAnyKindAgent<Type>(positions.Count, mat);
    for(int i = 0; i < positions.Count; i++)
    {
      this.Agents[offset + i].Item1.Position = positions[i];
    }
  }

  /// <summary>
  /// ターゲットモデルの内外でエージェントの種類を分類
  /// </summary>
  private void ClassificationPoints()
  {
    foreach (var agent in this.Agents)
    {
      SwarmAgent sa = (SwarmAgent)agent.Item1;
      Vector3 pos = sa.Position;
      if (Utils.IsIncludedInCollision(TargetObject, pos))
      {
        sa.is_exist = true;
        agent.Item2.SetMaterial(this.ExistentialParticleMaterial);
      }
      else
      {
        sa.is_exist = false;
        agent.Item2.SetMaterial(this.NonExistentialParticleMaterial);
      }
    }
  }

  // Update is called once per frame
  [System.Obsolete]
  void Update()
  {
    Debug.Log("Controller : Update");

    //エージェントの更新
    foreach ((Agent agent, ParticleObject particleObject) agent in this.Agents)
    {
      if (isUpdate)
      {
        agent.agent.Update();
      }
      agent.particleObject.UpdateSphere();
    }

    //ドロネー図の作成
    if (CreateVoronoi)
    {
      var sw = new MyStopwatch();
      sw.Start();

      this.voronoiPartition.Recreate(this.Agents);
      CreateVoronoi = false;

      sw.Stop();
      sw.ShowResult("ドロネー図");
    }

    //点を間引いたドロネー図の作成
    if (CreateVoronoiThinOutPoints)
    {
      var sw = new MyStopwatch();
      sw.Start();

      this.voronoiPartitionThinOutPoints.CreateDelaunayWithThinOutPoints(this.Agents);
      CreateVoronoiThinOutPoints = false;

      sw.Stop();
      sw.ShowResult("ドロネー図（点間引き）");
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

    if(this.previousViewDelaunayLine != this.viewDelaunayLine)
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

    if (this.previousViewVoronoiThinOutPointsMesh != this.viewVoronoiThinOutPointsMesh)
    {
      this.voronoiPartitionThinOutPoints.SetVisible(false, false, false);
      Debug.Log("change view voronoi mesh : " + this.viewVoronoiThinOutPointsMesh.ToString());
      this.voronoiPartitionThinOutPoints.SetVisibleVoronoiMesh(this.viewVoronoiThinOutPointsMesh);
      this.previousViewVoronoiThinOutPointsMesh = this.viewVoronoiThinOutPointsMesh;
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
