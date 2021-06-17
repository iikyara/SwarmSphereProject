using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PrimitiveParamater
{
  public Vector3 Size;
  public Vector3 Position;
  public Quaternion Quaternion;
  public int Sep_h;
  public int Sep_v;
}

/// <summary>
/// ボロノイ図をゲームオブジェクトとして扱うクラス．
/// VoronoiPartitionFastのラッパーみたいな感じ？
/// </summary>
public class Primitive
{
  public List<(Agent, ParticleObject)> Agents;
  public VoronoiPartitionFast VoronoiPartition;
  public GameObject ParentObject;
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material DisableParticleMaterial;

  public LayerMask Layer;

  public bool IsVisibleParticle;
  public bool IsVisibleDerauneyLine;
  public bool IsVisibleVoronoiLine;
  public bool IsVisibleVoronoiMesh;

  public static GameObject base_ParentObject = null;
  public static Material base_ExistentialParticleMaterial = null;
  public static Material base_NonExistentialParticleMaterial = null;
  public static Material base_DisableParticleMaterial = null;

  private GameObject particles;
  private GameObject derauney;
  private GameObject voronoi;

  public static VoronoiPartitionFast CombinedVoronoiPartition = new VoronoiPartitionFast();

  public Primitive()
  {
    this.Agents = new List<(Agent, ParticleObject)>();
    this.particles = new GameObject("Particles");
    this.derauney = new GameObject("Derauney");
    this.voronoi = new GameObject("Voronoi");
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
    this.VoronoiPartition.SetParent(this.derauney, this.voronoi);
    SetParent(base_ParentObject);
    SetMaterial(base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial, base_DisableParticleMaterial);
    SetVisible(false, false, false, true);
    SetLayer(0);
  }

  public Primitive(List<(Agent, ParticleObject)> agents) : this()
  {
    this.Agents = agents;
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
  }

  public Primitive(float size, Vector3 position, Quaternion quaternion, int sep_h, int sep_v) : this()
  {
    this.Agents = AddAgentsLocatedAsSphere(size, position, quaternion, sep_h, sep_v);
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
  }

  public Primitive(Vector3 size, Vector3 position, Quaternion quaternion, int sep_h, int sep_v) : this()
  {
    this.Agents = AddAgentsLocatedAsSphere(size, position, quaternion, sep_h, sep_v);
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
  }

  public Primitive(PrimitiveParamater pp) : this(pp.Size, pp.Position, pp.Quaternion, pp.Sep_h, pp.Sep_v)
  {

  }

  /// <summary>
  /// コピーコンストラクタ
  /// </summary>
  /// <param name="basePrimitive">基となるインスタンス</param>
  public Primitive(Primitive basePrimitive) : this()
  {
    var agents = new List<(Agent, ParticleObject)>();
    foreach(var agent in basePrimitive.Agents)
    {
      Agent item1 = (Agent)agent.Item1.Clone();
      ParticleObject item2 = new ParticleObject(agent.Item2);
      item2.Agent = item1;
      agents.Add((item1, item2));
    }
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
    SetVisible(false, false, false, true);
  }

  public static void SetBaseParent(GameObject parent)
  {
    base_ParentObject = parent;
  }

  public void SetParent(GameObject parent)
  {
    //SetBaseParent(parent);
    this.ParentObject = parent;
    //Debug.Log("b, " + parent);
    Utils.SetParent(this.ParentObject, this.particles);
    Utils.SetParent(this.ParentObject, this.derauney);
    Utils.SetParent(this.ParentObject, this.voronoi);
    this.VoronoiPartition.SetParent(parent);
  }

  public static void SetBaseMaterial(Material existential, Material nonexistential, Material disable)
  {
    base_ExistentialParticleMaterial = existential;
    base_NonExistentialParticleMaterial = nonexistential;
    base_DisableParticleMaterial = disable;
  }

  public void SetMaterial(Material existential, Material nonexistential, Material disable)
  {
    SetBaseMaterial(existential, nonexistential, disable);
    this.ExistentialParticleMaterial = existential;
    this.NonExistentialParticleMaterial = nonexistential;
    this.DisableParticleMaterial = disable;
  }

  public void SetVisible(bool particle, bool derauneyLine, bool voronoiLine, bool voronoiMesh)
  {
    SetVisibleParticle(particle);
    SetVisibleDerauneyLine(derauneyLine);
    SetVisibleVoronoiLine(voronoiLine);
    SetVisibleVoronoiMesh(voronoiMesh);
  }

  public void SetVisibleParticle(bool visible)
  {
    this.IsVisibleParticle = visible;
    foreach(var agent in this.Agents)
    {
      agent.Item2.SetActive(visible);
    }
  }

  public void SetVisibleDerauneyLine(bool visible)
  {
    this.IsVisibleDerauneyLine = visible;
    this.VoronoiPartition.SetVisibleDelaunayLine(visible);
  }

  public void SetVisibleVoronoiLine(bool visible)
  {
    this.IsVisibleVoronoiLine = visible;
    this.VoronoiPartition.SetVisibleVoronoiLine(visible);
  }

  public void SetVisibleVoronoiMesh(bool visible)
  {
    this.IsVisibleVoronoiMesh = visible;
    this.VoronoiPartition.SetVisibleVoronoiMesh(visible);
  }

  public static void SetVisibleCombinedDerauneyLine(bool visible)
  {
    CombinedVoronoiPartition.SetVisibleDelaunayLine(visible);
  }

  public static void SetVisibleCombinedVoronoiLine(bool visible)
  {
    CombinedVoronoiPartition.SetVisibleVoronoiLine(visible);
  }

  public static void SetVisibleCombinedVoronoiMesh(bool visible)
  {
    CombinedVoronoiPartition.SetVisibleVoronoiMesh(visible);
  }

  public void SetLayer(LayerMask layer)
  {
    this.Layer = layer;
    this.VoronoiPartition.SetLayer(layer);
  }

  public void Update()
  {
    UpdateAgent();
    UpdateParticleObject();
  }

  public void UpdateAgent()
  {
    foreach (var agent in this.Agents)
    {
      agent.Item1.Update();
    }
  }

  public void UpdateParticleObject()
  {
    foreach(var agent in this.Agents)
    {
      agent.Item2.UpdateSphere();
    }
  }

  public void SetAgents(List<(Agent, ParticleObject)> agents)
  {
    this.Agents = agents;
  }

  public void RecreateVoronoi()
  {
    if (VoronoiPartition != null) this.VoronoiPartition.Discard();
    MonoBehaviour.Destroy(this.particles);
    MonoBehaviour.Destroy(this.derauney);
    MonoBehaviour.Destroy(this.voronoi);
    this.particles = new GameObject("Particles");
    this.derauney = new GameObject("Derauney");
    this.voronoi = new GameObject("Voronoi");
    this.VoronoiPartition = new VoronoiPartitionFast(this.Agents);
    this.VoronoiPartition.SetParent(this.derauney, this.voronoi);
    CreateVoronoi();
  }

  //[System.Obsolete]
  public void CreateVoronoi()
  {
    //this.VoronoiPartition.CreateDelaunayWithThinOutPoints();
    this.VoronoiPartition.Create();
  }

  public void AddAgent((Agent, ParticleObject) agent)
  {
    this.Agents.Add(agent);
    this.VoronoiPartition.AddAgent(agent);
  }

  public void AddAgents(List<(Agent, ParticleObject)> agents)
  {
    this.Agents.AddRange(agents);
    this.VoronoiPartition.AddAgents(agents);
  }

  /// <summary>
  /// プリミティブを破棄する
  /// </summary>
  public void Discard()
  {
    if (this.Agents != null)
    {
      foreach (var agent in this.Agents)
      {
        agent.Item2.Discard();
      }
    }
    if(VoronoiPartition != null) this.VoronoiPartition.Discard();
    MonoBehaviour.Destroy(this.particles);
    MonoBehaviour.Destroy(this.derauney);
    MonoBehaviour.Destroy(this.voronoi);
    //null代入で開放
    this.Agents = null;
    this.VoronoiPartition = null;
    this.particles = null;
    this.derauney = null;
    this.voronoi = null;
  }

  //[System.Obsolete]
  public static void CombinePrimitives(Primitive[] primitives)
  {
    Primitive.CombinedVoronoiPartition.Reset();
    foreach (var p in primitives)
    {
      if(!p.VoronoiPartition.IsCreated)
      {
        p.CreateVoronoi();
      }

      Primitive.CombinedVoronoiPartition.JoinVoronoiPartition4_parallel(p.VoronoiPartition);
    }
  }

  /// <summary>
  /// rect範囲内でランダムにエージェントを配置する．
  /// </summary>
  /// <param name="width"></param>
  /// <param name="height"></param>
  /// <param name="depth"></param>
  public void LocateAgentRandomly(float width, float height, float depth)
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

  public List<(Agent, ParticleObject)>[] AddAgentsTest()
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

  public List<(Agent, ParticleObject)>[] AddAgentLocatedRandomly(int agent_num, float width, float height, float depth)
  {
    int exist_num = agent_num / 2;
    int nonexist_num = agent_num - exist_num;
    Vector3[] exist = new Vector3[exist_num];
    Vector3[] nonexist = new Vector3[nonexist_num];
    var result = new List<(Agent, ParticleObject)>[]
    {
      this.AddAgentsLocated(new List<Vector3>(exist), new List<Vector3>(nonexist)),
    };

    this.LocateAgentRandomly(width, height, depth);
    return result;
  }

  public List<(Agent, ParticleObject)>[] AddAgentsLocatedAsQuestion()
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

  public List<(Agent, ParticleObject)>[] AddAgentsLocatedAsToras()
  {
    int sep = 8;
    //int sepsep = 3;
    float d = 3.0f;
    float r = 1.5f;
    //float l = 2.5f;
    var result = new List<List<(Agent, ParticleObject)>>();
    for (int i = 0; i < sep; i++)
    {
      float x = d * Mathf.Sin((float)i / sep * 2.0f * Mathf.PI);
      float z = d * Mathf.Cos((float)i / sep * 2.0f * Mathf.PI);
      float rot = (float)i / sep * 360f + 90f;
      result.Add(this.AddAgentsLocatedAsSphere(r, new Vector3(x, 0, z), Quaternion.Euler(90f, rot, 0f), sep, 3));
    }
    return result.ToArray();
  }

  public List<(Agent, ParticleObject)>[] AddAgentsLocatedAsMonkey()
  {
    return new List<(Agent, ParticleObject)>[]
    {
      this.AddAgentsLocatedAsSphere(5f, new Vector3(0, 3, -5), Quaternion.Euler(60f, 60f, 0f), 5, 3),
    };
  }

  public List<(Agent, ParticleObject)>[] AddAgentsLocatedAsCube()
  {
    var result = new List<List<(Agent, ParticleObject)>>();
    int tate = 5;
    int yoko = 5;
    int takasa = 5;
    float r = 0.5f;
    for (int i = 0; i < tate; i++)
    {
      float x = ((float)i / (tate - 1) - 0.5f) * r * tate;
      for (int j = 0; j < yoko; j++)
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

  public List<(Agent, ParticleObject)>[] AddAgentsLocatedAsCube2()
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

  public List<(Agent, ParticleObject)> AddAgentsLocatedAsCube(float size, Vector3 position, Quaternion quaternion)
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

  public List<(Agent, ParticleObject)> AddAgentsLocatedAsSphere(float size, Vector3 position, Quaternion quaternion, int sep_h, int sep_v)
  {
    return AddAgentsLocatedAsSphere(new Vector3(size, size, size), position, quaternion, sep_h, sep_v);
  }

  public List<(Agent, ParticleObject)> AddAgentsLocatedAsSphere(Vector3 size, Vector3 position, Quaternion quaternion, int sep_h, int sep_v)
  {
    List<Vector3> existential = new List<Vector3> {
      new Vector3(0f, 0f, 0f)
    };
    List<Vector3> nonexistential = new List<Vector3>();
    float conicalOffset = 0;
    if (sep_v > 2)
    {
      nonexistential.Add(new Vector3(0f, 1f, 0f));
    }
    else
    {
      sep_v++;
      conicalOffset = -30f / 180f * Mathf.PI;
    }

    for (int i = 1; i < sep_v - 1; i++)
    {
      for (int j = 0; j < sep_h; j++)
      {
        float thita = i * Mathf.PI / (sep_v - 1) + conicalOffset;
        float phi = j * 2 * Mathf.PI / sep_h;
        nonexistential.Add(new Vector3(
          Mathf.Sin(phi) * Mathf.Sin(thita),
          Mathf.Cos(thita),
          Mathf.Cos(phi) * Mathf.Sin(thita)
        ));
      }
    }
    nonexistential.Add(new Vector3(0f, -1f, 0f));

    TransformPositions(ref existential, size, position, quaternion);
    TransformPositions(ref nonexistential, size, position, quaternion);
    return AddAgentsLocated(existential, nonexistential);
  }

  public void TransformPositions(ref List<Vector3> positions, float size, Vector3 position, Quaternion quaternion)
  {
    TransformPositions(ref positions, new Vector3(size, size, size), position, quaternion);
  }

  public void TransformPositions(ref List<Vector3> positions, Vector3 size, Vector3 position, Quaternion quaternion)
  {
    for (int i = 0; i < positions.Count; i++)
    {
      positions[i] = new Vector3(
        positions[i].x * size.x,
        positions[i].y * size.y,
        positions[i].z * size.z
      );
      //回転
      var x = quaternion.x;
      var y = quaternion.y;
      var z = quaternion.z;
      var w = quaternion.w;
      positions[i] = new Matrix4x4(
        new Vector4(1 - 2 * y * y - 2 * z * z, 2 * x * y + 2 * w * z, 2 * x * z - 2 * w * y, 0),
        new Vector4(2 * x * y - 2 * w * z, 1 - 2 * x * x - 2 * z * z, 2 * y * z + 2 * w * x, 0),
        new Vector4(2 * x * z + 2 * w * y, 2 * y * z - 2 * w * x, 1 - 2 * x * x - 2 * y * y, 0),
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

  public List<(Agent, ParticleObject)> AddAgentsLocated(List<Vector3> existential, List<Vector3> nonexistential)
  {
    List<(Agent, ParticleObject)> addedAgents = new List<(Agent, ParticleObject)>();
    addedAgents.AddRange(AddAnyAgentsLocated<ExistentialAgent>(existential, ExistentialParticleMaterial));
    addedAgents.AddRange(AddAnyAgentsLocated<NonExistentialAgent>(nonexistential, NonExistentialParticleMaterial));
    return addedAgents;
  }

  public List<(Agent, ParticleObject)> AddAnyAgentsLocated<Type>(List<Vector3> positions, Material mat) where Type : Agent, new()
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

  public void AddAnyKindAgent<Type>(int num, Material mat) where Type : Agent, new()
  {
    for (int i = 0; i < num; i++)
    {
      Agent agent = (Agent)new Type();
      ParticleObject particleObject = new ParticleObject(agent, mat, this.particles);
      particleObject.SetActive(IsVisibleParticle);
      this.Agents.Add((agent, particleObject));
    }
  }
}
