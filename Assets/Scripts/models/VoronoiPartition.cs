using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VoronoiPartition
{
  private List<GameObject> delaunayLine;
  private List<GameObject> voronoiLine;
  private GameObject voronoiMesh;
  private int delaunayLine_index;
  private int voronoiLine_index;

  public Material relationEdgeMaterial;
  public Material edgeMaterial;
  public Material meshMaterial;
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material DisabledParticleMaterial;

  public LayerMask Layer;

  public bool isVisibleDelaunayLine;
  public bool isVisibleVoronoiLine;
  public bool isVisibleVoronoiMesh;

  public bool IsCreated;

  private GameObject Parent;
  private GameObject edgeParent;
  private GameObject meshParent;

  private List<(Agent, ParticleObject)> agents;
  private List<(Agent, ParticleObject)> thinOutAgents;
  private List<Vector3WithAgent> points;
  //private List<Vector3WithAgent> thinOutPoints;
  private List<Tetrahedron> t_list;

  public static Material base_relationEdgeMaterial = null;
  public static Material base_edgeMaterial = null;
  public static Material base_meshMaterial = null;
  public static Material base_ExistentialParticleMaterial = null;
  public static Material base_NonExistentialParticleMaterial = null;
  public static Material base_DisabledParticleMaterial = null;
  private static GameObject base_Parent = null;
  private static GameObject base_edgeParent = null;
  private static GameObject base_meshParent = null;

  public VoronoiPartition()
  {
    //描画用オブジェクト
    this.delaunayLine = new List<GameObject>();
    this.voronoiLine = new List<GameObject>();
    //描画用オブジェクト削減用インデックス
    this.delaunayLine_index = 0;
    this.voronoiLine_index = 0;
    //ドロネー&ボロノイ分割用
    this.points = new List<Vector3WithAgent>();
    //this.thinOutPoints = new List<Vector3WithAgent>();
    this.thinOutAgents = new List<(Agent, ParticleObject)>();
    this.t_list = new List<Tetrahedron>();
    //初期化
    this.IsCreated = false;
    //描画を設定
    SetVisible(false, false, true);
    //デフォルトのマテリアルと親を設定
    SetMaterials(base_relationEdgeMaterial, base_edgeMaterial, base_meshMaterial, base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial, base_DisabledParticleMaterial);
    SetParent(base_edgeParent, base_meshParent);
    SetParent(base_Parent);
    SetLayer(0);
  }

  public VoronoiPartition(List<(Agent, ParticleObject)> agents) : this()
  {
    this.agents = agents;
  }

  public VoronoiPartition(List<(Agent, ParticleObject)> agents, GameObject edgeParent, GameObject meshParent) : this(agents)
  {
    SetParent(edgeParent, meshParent);
  }

  public VoronoiPartition(List<(Agent, ParticleObject)> agents, Material relEdgeMat, Material edgeMat, Material meshMat, GameObject parent) : this(agents)
  {
    SetMaterials(relEdgeMat, edgeMat, meshMat, base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial, base_DisabledParticleMaterial);
    SetParent(parent);
  }

  public VoronoiPartition(List<(Agent, ParticleObject)> agents, Material relEdgeMat, Material edgeMat, Material meshMat, GameObject edgeParent, GameObject meshParent) : this(agents)
  {
    SetMaterials(relEdgeMat, edgeMat, meshMat, base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial, base_DisabledParticleMaterial);
    SetParent(edgeParent, meshParent);
  }

  public VoronoiPartition(List<(Agent, ParticleObject)> agents, Material relEdgeMat, Material edgeMat, Material meshMat, Material existential, Material nonexistential, Material disable, GameObject edgeParent, GameObject meshParent) : this(agents)
  {
    SetMaterials(relEdgeMat, edgeMat, meshMat, existential, nonexistential, disable);
    SetParent(edgeParent, meshParent);
  }

  public void Discard()
  {
    foreach (var line in this.delaunayLine)
      MonoBehaviour.Destroy(line);
    foreach (var line in this.voronoiLine)
      MonoBehaviour.Destroy(line);
    MonoBehaviour.Destroy(voronoiMesh);
    //全部初期化して開放
    this.delaunayLine = new List<GameObject>();
    this.voronoiLine = new List<GameObject>();
    this.voronoiMesh = null;
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition(VoronoiPartition vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition2(VoronoiPartition vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition3(VoronoiPartition vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4(VoronoiPartition vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4_parallel(VoronoiPartition vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition(VoronoiPartition vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
    this.ThinOutPoints();
    Utils.Swap<List<(Agent, ParticleObject)>>(ref this.agents, ref this.thinOutAgents);
    this.CreateDelaunay();
    Utils.Swap<List<(Agent, ParticleObject)>>(ref this.agents, ref this.thinOutAgents);
    vp.ThinOutPoints();
    Utils.Swap<List<(Agent, ParticleObject)>>(ref vp.agents, ref vp.thinOutAgents);
    vp.CreateDelaunay();
    Utils.Swap<List<(Agent, ParticleObject)>>(ref vp.agents, ref vp.thinOutAgents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in this.agents)
    {
      if (!vp.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    this.agents = agents;
  }

  public void JoinVoronoiPartition2(VoronoiPartition vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
    foreach (var agent in vp.agents)
    {
      agents.Add(agent);
      if (((SwarmAgent)agent.Item1).is_exist)
      {
        agent.Item2.SetMaterial(existential);
      }
      else
      {
        agent.Item2.SetMaterial(nonexistential);
      }
    }
    foreach (var agent in this.agents)
    {
      agents.Add(agent);
      if (((SwarmAgent)agent.Item1).is_exist)
      {
        agent.Item2.SetMaterial(existential);
      }
      else
      {
        agent.Item2.SetMaterial(nonexistential);
      }
    }
    this.agents = agents;
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition3(VoronoiPartition vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
    List<(Agent, ParticleObject)> temp = new List<(Agent, ParticleObject)>(this.agents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInCircumsphere(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in temp)
    {
      if (!vp.IsIncludedInCircumsphere(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    this.t_list.AddRange(vp.t_list);
    this.agents = agents;
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4(VoronoiPartition vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
    List<(Agent, ParticleObject)> temp = new List<(Agent, ParticleObject)>(this.agents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in temp)
    {
      if (!vp.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Item1).is_exist)
        {
          agent.Item2.SetMaterial(existential);
        }
        else
        {
          agent.Item2.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.Item2.SetMaterial(disabledMat);
      }
    }
    this.t_list.AddRange(vp.t_list);
    this.agents = agents;
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4_parallel(VoronoiPartition vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<(Agent, ParticleObject)> existential_agents = new List<(Agent, ParticleObject)>();
    List<(Agent, ParticleObject)> nonexistential_agents = new List<(Agent, ParticleObject)>();
    List<(Agent, ParticleObject)> temp = new List<(Agent, ParticleObject)>(this.agents);
    Parallel.ForEach(vp.agents, agent =>
    {
      if (((SwarmAgent)agent.Item1).is_exist || !this.IsIncludedInTetrahedron(agent))
      {
        lock (existential_agents) existential_agents.Add(agent);
      }
      else
      {
        lock (nonexistential_agents) nonexistential_agents.Add(agent);
      }
    });
    Parallel.ForEach(temp, agent =>
    {
      if (((SwarmAgent)agent.Item1).is_exist || !vp.IsIncludedInTetrahedron(agent))
      {
        lock (existential_agents) existential_agents.Add(agent);
      }
      else
      {
        lock (nonexistential_agents) nonexistential_agents.Add(agent);
      }
    });
    this.t_list.AddRange(vp.t_list);
    this.agents = existential_agents;
    foreach(var agent in existential_agents)
    {
      Debug.Log(agent);
      if (((SwarmAgent)agent.Item1).is_exist)
      {
        agent.Item2.SetMaterial(existential);
      }
      else
      {
        agent.Item2.SetMaterial(nonexistential);
      }
    }
    foreach(var agent in nonexistential_agents)
    {
      agent.Item2.SetMaterial(disabledMat);
    }
  }

  /*  [System.Obsolete]
    public static VoronoiPartition JoinVoronoiPartition(VoronoiPartition vp1, VoronoiPartition vp2)
    {
      List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
      vp1.ThinOutPoints();
      vp1.CreateDelaunay();
      vp2.ThinOutPoints();
      vp2.CreateDelaunay();
      foreach(var agent in vp1.agents)
      {
        if (!vp2.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
        {
          agents.Add(agent);
        }
      }
      foreach (var agent in vp2.agents)
      {
        if (!vp1.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Item1).is_exist)
        {
          agents.Add(agent);
        }
      }

      return new VoronoiPartition(agents, vp1.relationEdgeMaterial, vp1.edgeMaterial, vp1.meshMaterial, vp1.edgeParent, vp1.meshParent);
    }*/

  public bool IsIncludedInTetrahedron((Agent, ParticleObject) agent)
  {
    foreach(Tetrahedron tetra in t_list)
    {
      var point = new Vector3WithAgent(agent.Item1.Position.x, agent.Item1.Position.y, agent.Item1.Position.z);
      if (tetra.CheckPointIncludeTetrahedron(point))
      {
        return true;
      }
    }
    return false;
  }

  public bool IsIncludedInCircumsphere((Agent, ParticleObject) agent)
  {
    foreach (Tetrahedron tetra in t_list)
    {
      var point = new Vector3WithAgent(agent.Item1.Position.x, agent.Item1.Position.y, agent.Item1.Position.z);
      if (tetra.CheckPointIncludeCircumsphere(point))
      {
        return true;
      }
    }
    return false;
  }

  //[Obsolete]
  public void Recreate(List<(Agent, ParticleObject)> agents)
  {
    this.agents = agents;
    Recreate();
  }

  //[System.Obsolete]
  public void Recreate()
  {
    ResetLine();
    Tetrahedron.id_count = 0;

    var sw = new MyStopwatch();
    sw.Start();

    CreateDelaunay();

    sw.Stop();
    sw.ShowResult("CreateDelaunay");

    sw.Restart();

    CreateVoronoi();

    sw.Stop();
    sw.ShowResult("CreateVoronoi");

    sw.Restart();

    DrawDelaunay();

    sw.Stop();
    sw.ShowResult("CreateLine&Mesh");
  }

  public void SetVisible(bool delaunay, bool voronoiLine, bool voronoiMesh)
  {
    SetVisibleDelaunayLine(delaunay);
    SetVisibleVoronoiLine(voronoiLine);
    SetVisibleVoronoiMesh(voronoiMesh);
  }

  public void SetVisibleDelaunayLine(bool delaunay)
  {
    this.isVisibleDelaunayLine = delaunay;
    foreach (GameObject obj in this.delaunayLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = this.isVisibleDelaunayLine;
    }
  }

  public void SetVisibleVoronoiLine(bool voronoiLine)
  {
    this.isVisibleVoronoiLine = voronoiLine;
    foreach (GameObject obj in this.voronoiLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = this.isVisibleVoronoiLine;
    }
  }

  public void SetVisibleVoronoiMesh(bool voronoiMesh)
  {
    this.isVisibleVoronoiMesh = voronoiMesh;
    if(this.voronoiMesh) this.voronoiMesh.SetActive(this.isVisibleVoronoiMesh);
  }

  public void LoadBaseMaterials()
  {
    this.ExistentialParticleMaterial = base_ExistentialParticleMaterial;
    this.NonExistentialParticleMaterial = base_NonExistentialParticleMaterial;
    this.DisabledParticleMaterial = base_DisabledParticleMaterial;
    this.relationEdgeMaterial = base_relationEdgeMaterial;
    this.edgeMaterial = base_edgeMaterial;
    this.meshMaterial = base_meshMaterial;
  }

  public static void SetBaseMaterials(Material relEdgeMat, Material edgeMat, Material meshMat, Material existential, Material nonexistential, Material disable)
  {
    VoronoiPartition.base_ExistentialParticleMaterial = existential;
    VoronoiPartition.base_NonExistentialParticleMaterial = nonexistential;
    VoronoiPartition.base_DisabledParticleMaterial = disable;
    VoronoiPartition.base_relationEdgeMaterial = relEdgeMat;
    VoronoiPartition.base_edgeMaterial = edgeMat;
    VoronoiPartition.base_meshMaterial = meshMat;
  }

  public void SetMaterials(Material relEdgeMat, Material edgeMat, Material meshMat, Material existential, Material nonexistential, Material disable)
  {
    SetBaseMaterials(relEdgeMat, edgeMat, meshMat, existential, nonexistential, disable);
    this.ExistentialParticleMaterial = existential;
    this.NonExistentialParticleMaterial = nonexistential;
    this.DisabledParticleMaterial = disable;
    SetMaterialDelaunayLine(relEdgeMat);
    SetMaterialVoronoiLine(edgeMat);
    SetMaterialVoronoiMesh(meshMat);
    /*this.relationEdgeMaterial = relEdgeMat;
    this.edgeMaterial = edgeMat;
    this.meshMaterial = meshMat;*/
  }

  public void SetMaterialDelaunayLine(Material mat)
  {
    this.relationEdgeMaterial = mat;
    foreach (GameObject obj in this.delaunayLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.material = mat;
    }
  }

  public void SetMaterialVoronoiLine(Material mat)
  {
    this.edgeMaterial = mat;
    foreach (GameObject obj in this.voronoiLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.material = mat;
    }
  }

  public void SetMaterialVoronoiMesh(Material mat)
  {
    this.meshMaterial = mat;
    if (this.voronoiMesh) this.voronoiMesh.GetComponent<MeshRenderer>().material = mat;
  }

  public static void SetBaseParent(GameObject parent)
  {
    VoronoiPartition.base_Parent = parent;
  }

  public static void SetBaseParent(GameObject edgeParent, GameObject meshParent)
  {
    VoronoiPartition.base_edgeParent = edgeParent;
    VoronoiPartition.base_meshParent = meshParent;
  }

  public void SetParent(GameObject parent)
  {
    //Debug.Log("c, " + parent);
    this.Parent = parent;
    Utils.SetParent(parent, this.edgeParent);
    Utils.SetParent(parent, this.meshParent);
    if(this.voronoiMesh) Utils.SetParent(parent, this.voronoiMesh);
  }

  public void SetParent(GameObject edgeParent, GameObject meshParent)
  {
    SetBaseParent(edgeParent, meshParent);
    this.edgeParent = edgeParent;
    this.meshParent = meshParent;
  }

  public void Reset()
  {
    this.IsCreated = false;
    this.agents = new List<(Agent, ParticleObject)>();
    this.t_list = new List<Tetrahedron>();
  }

  private void ResetLine()
  {
    this.delaunayLine_index = 0;
    this.voronoiLine_index = 0;
    foreach (GameObject obj in this.delaunayLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = false;
    }
    foreach (GameObject obj in this.voronoiLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = false;
    }
  }

  /*
  public void Clear()
  {
    foreach(GameObject edge in delaunayLine)
    {
      Destroy(edge);
    }
    Destroy(voronoiMesh);
  }
  */

  private void SetEdgeParent(GameObject child)
  {
    if (this.edgeParent != null)
    {
      Utils.SetParent(this.edgeParent, child);
      //child.transform.parent = this.edgeParent.transform;
    }
  }

  private void SetMeshParent(GameObject child)
  {
    if (this.meshParent != null)
    {
      Utils.SetParent(this.meshParent, child);
      //child.transform.parent = this.meshParent.transform;
    }
  }

  public void SetLayer(LayerMask layer)
  {
    this.Layer = layer;
    foreach (GameObject obj in this.delaunayLine)
    {
      obj.layer = layer;
    }
    foreach (GameObject obj in this.voronoiLine)
    {
      obj.layer = layer;
    }
    if (this.voronoiMesh) this.voronoiMesh.layer = layer;
  }

  //[System.Obsolete]
  private void CreateDelaunay()
  {
    this.t_list.Clear();
    this.points.Clear();

    Vector3WithAgent p1 = new Vector3WithAgent(0f, 0f, 800f);
    Vector3WithAgent p2 = new Vector3WithAgent(0f, 1000f, -200f);
    Vector3WithAgent p3 = new Vector3WithAgent(866.66f, -500f, -200f);
    Vector3WithAgent p4 = new Vector3WithAgent(-866.66f, -500f, -200f);

    Tetrahedron tetrahedron = new Tetrahedron(p1, p2, p3, p4);
    t_list.Add(tetrahedron);

    // main algorithm
    // https://qiita.com/kkttm530/items/d32bad84a6a7f0d8d7e7
    for(int i = 0; i < agents.Count; i++)
    {
      Vector3WithAgent select_point = new Vector3WithAgent(agents[i], i);
      this.points.Add(select_point);

      List<Tetrahedron> temp_t_list = new List<Tetrahedron>();
      for(int j = 0; j < t_list.Count; j++)
      {
        Tetrahedron tri = t_list[j];
        if (tri.CheckPointIncludeCircumsphere(select_point))
        {
          temp_t_list.Add(new Tetrahedron(tri.p1, tri.p2, tri.p3, select_point));
          temp_t_list.Add(new Tetrahedron(tri.p1, tri.p2, tri.p4, select_point));
          temp_t_list.Add(new Tetrahedron(tri.p1, tri.p3, tri.p4, select_point));
          temp_t_list.Add(new Tetrahedron(tri.p2, tri.p3, tri.p4, select_point));
          t_list.RemoveAt(j--);
        }
      }
      var del_insts = new List<Tetrahedron>[temp_t_list.Count];
      Parallel.For(0, temp_t_list.Count, k =>
      {
        Tetrahedron tetra = temp_t_list[k];
        del_insts[k] = new List<Tetrahedron>();
        foreach (Tetrahedron tetra_check in temp_t_list)
        {
          if (ReferenceEquals(tetra, tetra_check)) continue;
          if (tetra.radius == tetra_check.radius && tetra.center == tetra_check.center)
            del_insts[k].Add(tetra_check);
        }
      });
      for(var k = 0; k < del_insts.Length; k++)
      {
        foreach (Tetrahedron del_inst in del_insts[k])
        {
          int index = temp_t_list.IndexOf(del_inst);
          if (index != -1)
          {
            temp_t_list.RemoveAt(index);
          }
        }
      }
      t_list.AddRange(temp_t_list);
    }

    List<Tetrahedron> del_insts2 = new List<Tetrahedron>();
    foreach(Tetrahedron tetra_check in t_list)
    {
      if(
        tetra_check.p1 == p1 || tetra_check.p1 == p2 || tetra_check.p1 == p3 || tetra_check.p1 == p4 ||
        tetra_check.p2 == p1 || tetra_check.p2 == p2 || tetra_check.p2 == p3 || tetra_check.p2 == p4 ||
        tetra_check.p3 == p1 || tetra_check.p3 == p2 || tetra_check.p3 == p3 || tetra_check.p3 == p4 ||
        tetra_check.p4 == p1 || tetra_check.p4 == p2 || tetra_check.p4 == p3 || tetra_check.p4 == p4
      )
      {
        del_insts2.Add(tetra_check);
      }
    }
    foreach (Tetrahedron del_inst in del_insts2)
    {
      int index = t_list.IndexOf(del_inst);
      if (index != -1)
      {
        t_list.RemoveAt(index);
      }
    }
  }

  //[Obsolete]
  private void DrawDelaunay()
  {
    if (isVisibleDelaunayLine)
    {
      // draw
      foreach (Tetrahedron tetra in t_list)
      {
        DrawLine(tetra.p1, tetra.p2);
        DrawLine(tetra.p1, tetra.p3);
        DrawLine(tetra.p1, tetra.p4);
        DrawLine(tetra.p2, tetra.p3);
        DrawLine(tetra.p2, tetra.p4);
        DrawLine(tetra.p3, tetra.p4);
      }
    }
  }

  //[System.Obsolete]
  private void DrawLine(Vector3WithAgent p1, Vector3WithAgent p2)
  {
    GameObject line;
    LineRenderer lRend;
    if (this.delaunayLine_index < this.delaunayLine.Count)
    {
      line = this.delaunayLine[this.delaunayLine_index];
      lRend = line.GetComponent<LineRenderer>();
    }
    else
    {
      line = new GameObject("Delaunay");
      delaunayLine.Add(line);
      lRend = line.AddComponent<LineRenderer>();
      lRend.material = this.relationEdgeMaterial;
      //lRend.SetVertexCount(2);
      //lRend.SetWidth(0.01f, 0.01f);
      lRend.positionCount = 2;
      lRend.startWidth = 0.01f;
      lRend.endWidth = 0.01f;
      SetEdgeParent(line);
    }
    lRend.enabled = this.isVisibleDelaunayLine;
    lRend.SetPosition(0, p1.vec);
    lRend.SetPosition(1, p2.vec);

    this.delaunayLine_index++;
  }

  //[Obsolete]
  private void CreateVoronoi()
  {
    //ボロノイ図を表示しない設定なら飛ばす
    if (!isVisibleVoronoiLine && !isVisibleVoronoiMesh) return;

    int num_points = this.agents.Count;
    List<(Tetrahedron, Tetrahedron)>[,] pols = new List<(Tetrahedron, Tetrahedron)>[num_points, num_points];
    List<(Vector3, Vector3)>[] lines = new List<(Vector3, Vector3)>[t_list.Count];

    //初期化
    for (int i = 0; i < t_list.Count; i++)
      lines[i] = new List<(Vector3, Vector3)>();
    for (int j = 0; j < num_points; j++)
      for (int k = 0; k < num_points; k++)
        pols[j, k] = new List<(Tetrahedron, Tetrahedron)>();

    /*    //各点から四面体を逆参照できるように設定
        foreach(var tetra in t_list)
        {
          tetra.p1.tetra.Add(tetra);
          tetra.p2.tetra.Add(tetra);
          tetra.p3.tetra.Add(tetra);
          tetra.p4.tetra.Add(tetra);
        }

        //並列で面を生成
        Parallel.For(0, num_points, i =>
        {
          for(int j = i + 1; j < num_points; j++)
          {
            if (((SwarmAgent)this.points[i].agent).is_exist == ((SwarmAgent)this.points[j].agent).is_exist)
              continue;
            for(int k = 0; k < this.points[i].tetra.Count; k++)
            {
              Tetrahedron tri1 = this.points[i].tetra[k];
              for (int l = k + 1; l < this.points[j].tetra.Count; l++)
              {
                Tetrahedron tri2 = this.points[j].tetra[l];

                int count = 0;
                bool[] flag = new bool[4];
                flag[0] = tri1.p1.vec == tri2.p1.vec || tri1.p1.vec == tri2.p2.vec || tri1.p1.vec == tri2.p3.vec || tri1.p1.vec == tri2.p4.vec;
                flag[1] = tri1.p2.vec == tri2.p1.vec || tri1.p2.vec == tri2.p2.vec || tri1.p2.vec == tri2.p3.vec || tri1.p2.vec == tri2.p4.vec;
                flag[2] = tri1.p3.vec == tri2.p1.vec || tri1.p3.vec == tri2.p2.vec || tri1.p3.vec == tri2.p3.vec || tri1.p3.vec == tri2.p4.vec;
                flag[3] = tri1.p4.vec == tri2.p1.vec || tri1.p4.vec == tri2.p2.vec || tri1.p4.vec == tri2.p3.vec || tri1.p4.vec == tri2.p4.vec;

                foreach (bool f in flag) if (f) count++;

                if (count == 3)
                {
                  if (flag[0] && flag[1] && (((SwarmAgent)tri1.p1.agent).is_exist != ((SwarmAgent)tri1.p2.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  if (flag[0] && flag[2] && (((SwarmAgent)tri1.p1.agent).is_exist != ((SwarmAgent)tri1.p3.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  if (flag[0] && flag[3] && (((SwarmAgent)tri1.p1.agent).is_exist != ((SwarmAgent)tri1.p4.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  if (flag[1] && flag[2] && (((SwarmAgent)tri1.p2.agent).is_exist != ((SwarmAgent)tri1.p3.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  if (flag[1] && flag[3] && (((SwarmAgent)tri1.p2.agent).is_exist != ((SwarmAgent)tri1.p4.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  if (flag[2] && flag[3] && (((SwarmAgent)tri1.p3.agent).is_exist != ((SwarmAgent)tri1.p4.agent).is_exist)) lock (pols[i, j]) pols[i, j].Add((tri1, tri2));
                  lines[i].Add((tri1.center, tri2.center));
                }
              }
            }
          }
        });*/

    //並列で面を生成
    Parallel.For(0, t_list.Count, i =>
    {
      Tetrahedron tri1 = t_list[i];
      for (int j = i + 1; j < this.t_list.Count; j++)
      {
        Tetrahedron tri2 = t_list[j];

        int count = 0;
        bool[] flag = new bool[4];
        flag[0] = tri1.p1.vec == tri2.p1.vec || tri1.p1.vec == tri2.p2.vec || tri1.p1.vec == tri2.p3.vec || tri1.p1.vec == tri2.p4.vec;
        flag[1] = tri1.p2.vec == tri2.p1.vec || tri1.p2.vec == tri2.p2.vec || tri1.p2.vec == tri2.p3.vec || tri1.p2.vec == tri2.p4.vec;
        flag[2] = tri1.p3.vec == tri2.p1.vec || tri1.p3.vec == tri2.p2.vec || tri1.p3.vec == tri2.p3.vec || tri1.p3.vec == tri2.p4.vec;
        flag[3] = tri1.p4.vec == tri2.p1.vec || tri1.p4.vec == tri2.p2.vec || tri1.p4.vec == tri2.p3.vec || tri1.p4.vec == tri2.p4.vec;

        foreach (bool f in flag) if (f) count++;

        if (count == 3)
        {
          if (flag[0] && flag[1] && (((SwarmAgent)tri1.p1.agent.Item1).is_exist != ((SwarmAgent)tri1.p2.agent.Item1).is_exist)) lock (pols[tri1.p1.id, tri1.p2.id]) pols[tri1.p1.id, tri1.p2.id].Add((tri1, tri2));
          if (flag[0] && flag[2] && (((SwarmAgent)tri1.p1.agent.Item1).is_exist != ((SwarmAgent)tri1.p3.agent.Item1).is_exist)) lock (pols[tri1.p1.id, tri1.p3.id]) pols[tri1.p1.id, tri1.p3.id].Add((tri1, tri2));
          if (flag[0] && flag[3] && (((SwarmAgent)tri1.p1.agent.Item1).is_exist != ((SwarmAgent)tri1.p4.agent.Item1).is_exist)) lock (pols[tri1.p1.id, tri1.p4.id]) pols[tri1.p1.id, tri1.p4.id].Add((tri1, tri2));
          if (flag[1] && flag[2] && (((SwarmAgent)tri1.p2.agent.Item1).is_exist != ((SwarmAgent)tri1.p3.agent.Item1).is_exist)) lock (pols[tri1.p2.id, tri1.p3.id]) pols[tri1.p2.id, tri1.p3.id].Add((tri1, tri2));
          if (flag[1] && flag[3] && (((SwarmAgent)tri1.p2.agent.Item1).is_exist != ((SwarmAgent)tri1.p4.agent.Item1).is_exist)) lock (pols[tri1.p2.id, tri1.p4.id]) pols[tri1.p2.id, tri1.p4.id].Add((tri1, tri2));
          if (flag[2] && flag[3] && (((SwarmAgent)tri1.p3.agent.Item1).is_exist != ((SwarmAgent)tri1.p4.agent.Item1).is_exist)) lock (pols[tri1.p3.id, tri1.p4.id]) pols[tri1.p3.id, tri1.p4.id].Add((tri1, tri2));
          lines[i].Add((tri1.center, tri2.center));
        }
      }
    });

    if (isVisibleVoronoiLine)
    {
      for (int i = 0; i < lines.Length; i++)
        foreach (var line in lines[i])
          DrawPorigon(line.Item1, line.Item2);
    }
    /*var temp_pols = new List<(Tetrahedron, Tetrahedron)>[num_points, num_points];
    for (int i = 0; i < temp_pols.GetLength(0); i++)
      for (int j = 0; j < temp_pols.GetLength(1); j++)
        temp_pols[i, j] = new List<(Tetrahedron, Tetrahedron)>();
    for (int i = 0; i < pols.GetLength(0); i++)
      for (int j = 0; j < pols.GetLength(1); j++)
        for (int k = 0; k < pols.GetLength(2); k++)
          temp_pols[j, k].AddRange(pols[i, j, k]);*/

    if (isVisibleVoronoiMesh)
    {
      CreateVoronoiGameObject(pols);
    }
    
    IsCreated = true;
  }

  //[System.Obsolete]
  private void DrawPorigon(Vector3 p1, Vector3 p2)
  {
    GameObject line;
    LineRenderer lRend;
    if (this.voronoiLine_index < this.voronoiLine.Count)
    {
      line = this.voronoiLine[this.voronoiLine_index];
      lRend = line.GetComponent<LineRenderer>();
    }
    else
    {
      line = new GameObject("Voronoi");
      voronoiLine.Add(line);
      lRend = line.AddComponent<LineRenderer>();
      lRend.material = this.edgeMaterial;
      //lRend.SetVertexCount(2);
      //lRend.SetWidth(0.01f, 0.01f);
      lRend.positionCount = 2;
      lRend.startWidth = 0.01f;
      lRend.endWidth = 0.01f;
      SetMeshParent(line);
    }
    lRend.enabled = this.isVisibleVoronoiLine;
    lRend.SetPosition(0, p1);
    lRend.SetPosition(1, p2);

    this.voronoiLine_index++;
  }

  private void CreateVoronoiGameObject(List<(Tetrahedron, Tetrahedron)>[,] pols)
  {
    //Debug.Log("CreateVoronoiGameObject");
    if(voronoiMesh == null)
    {
      this.voronoiMesh = new GameObject("VoronoiMesh");
      this.voronoiMesh.AddComponent<MeshRenderer>();
      this.voronoiMesh.AddComponent<MeshFilter>();
    }
    this.voronoiMesh.GetComponent<MeshFilter>().sharedMesh = CreateVoronoiMesh(pols);
    this.voronoiMesh.GetComponent<MeshRenderer>().material = this.meshMaterial;
    this.voronoiMesh.SetActive(this.isVisibleVoronoiMesh);
    Utils.SetParent(this.Parent, this.voronoiMesh);
  }

  private Mesh CreateVoronoiMesh(List<(Tetrahedron, Tetrahedron)>[,] pols)
  {
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    int offset = 0;
    for (int i = 0; i < pols.GetLength(0); i++)
    {
      for (int j = i + 1; j < pols.GetLength(1); j++)
      {
        pols[i, j].AddRange(pols[j, i]);
        List<(Tetrahedron, Tetrahedron)> edges = new List<(Tetrahedron, Tetrahedron)>(pols[i, j]);
        if (edges.Count < 3) continue;
        /*string msg = string.Format("edge : {0}, pols[{1}, {2}] : ", edges.Count.ToString(), i, j);*/
        for (int k = 0; k < edges.Count; k++)
        {
          //決定したところから頂点リストに追加
          vertices.Add(edges[k].Item1.center);
          /*msg += string.Format(", ({0}, {1})", edges[k].Item1.center.ToString(), edges[k].Item2.center.ToString());*/
          /*msg += string.Format(", (({0}, {1}, {2}), ({3}, {4}, {5}))",
            edges[k].Item1.x, edges[k].Item1.y, edges[k].Item1.z,
            edges[k].Item2.x, edges[k].Item2.y, edges[k].Item2.z);*/
          for (int l = k + 1; l < edges.Count; l++)
          {
            /*msg += "+";*/
            if (edges[k].Item2 == edges[l].Item1)
            {
              /*msg += string.Format("||nrev({0}, {1}) <> ({2}, {3})||",
                edges[k + 1].Item1.center.ToString(), edges[k + 1].Item2.center.ToString(),
                edges[l].Item1.center.ToString(), edges[l].Item2.center.ToString());*/
              (Tetrahedron, Tetrahedron) t = edges[l];
              edges[l] = edges[k + 1];
              edges[k + 1] = t;
              break;
            }
            if (edges[k].Item2 == edges[l].Item2)
            {
              //msg += string.Format("||rev({0}, {1}) <> ({2}, {3})||",
              //  edges[k + 1].Item1.center.ToString(), edges[k + 1].Item2.center.ToString(),
              //  edges[l].Item1.center.ToString(), edges[l].Item2.center.ToString());
              (Tetrahedron, Tetrahedron) t = edges[l];
              edges[l] = edges[k + 1];
              edges[k + 1] = (t.Item2, t.Item1);
              break;
            }
          }
        }
        //面を設定
        /*Debug.Log(((SwarmAgent)this.agents[i].Item1).is_exist);*/
        Vector3 a = vertices[offset + 1] - vertices[offset + 0];
        Vector3 b = vertices[offset + 2] - vertices[offset + 0];
        Vector3 c = this.agents[i].Item1.Position - vertices[offset + 0];
        Vector3 bcrossa = Vector3.Cross(b, a);
        float cos = Vector3.Dot(bcrossa, c) / (bcrossa.magnitude * c.magnitude);
        //Debug.Log(cos);
        if (((SwarmAgent)this.agents[i].Item1).is_exist != cos > 0f)
        {
          for (int k = 1; k < edges.Count - 1; k++)
          {
            //面
            triangles.Add(offset + 0);
            triangles.Add(offset + k + 1);
            triangles.Add(offset + k);
          }
        }
        else
        {
          for (int k = 1; k < edges.Count - 1; k++)
          {
            //面
            triangles.Add(offset + 0);
            triangles.Add(offset + k);
            triangles.Add(offset + k + 1);
          }
        }

        /*Debug.Log(msg);*/

        offset += edges.Count;
      }
    }

    //メッシュオブジェクト作成
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    /*Debug.Log("vertices num : " + vertices.Count.ToString() + "triangles num : " + triangles.Count.ToString());*/

    return mesh;
  }

  /*
  private Mesh CreateVoronoiMesh(List<(Vector3, Vector3)>[,] pols)
  {
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    int offset = 0;
    for(int i = 0; i < pols.GetLength(0); i++)
    {
      for (int j = i + 1; j < pols.GetLength(1); j++)
      {
        pols[i, j].AddRange(pols[j, i]);
        List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(pols[i, j]);
        string msg = string.Format("edge : {0}, pols[{1}, {2}] : ", edges.Count.ToString(), i, j);
        for(int k = 0; k < edges.Count; k++)
        {
          //決定したところから頂点リストに追加
          vertices.Add(edges[k].Item1);
          //msg += string.Format(", ({0}, {1})", edges[k].Item1.ToString(), edges[k].Item2.ToString());
          msg += string.Format(", (({0}, {1}, {2}), ({3}, {4}, {5}))",
            edges[k].Item1.x, edges[k].Item1.y, edges[k].Item1.z,
            edges[k].Item2.x, edges[k].Item2.y, edges[k].Item2.z);
          for (int l = k + 1; l < edges.Count; l++)
          {
            if((edges[k].Item2 - edges[l].Item1).magnitude < 0.000001)
            {
              (Vector3, Vector3) t = edges[k + 1];
              edges[k + 1] = edges[l];
              edges[l] = t;
              msg += "nrev";
              break;
            }
            if((edges[k].Item2 - edges[l].Item2).magnitude < 0.000001)
            {
              (Vector3, Vector3) t = edges[k + 1];
              edges[k + 1] = (edges[l].Item2, edges[l].Item1);
              edges[l] = t;
              msg += "rev";
              break;
            }
          }
        }
        //面を設定
        for(int k = 1; k < edges.Count - 1; k++)
        {
          //面
          triangles.Add(offset + 0);
          triangles.Add(offset + k);
          triangles.Add(offset + k + 1);
        }

        Debug.Log(msg);

        offset += edges.Count;
      }
    }

    //メッシュオブジェクト作成
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    Debug.Log("vertices num : " + vertices.Count.ToString() + "triangles num : " + triangles.Count.ToString());

    return mesh;
  }
  */

  //[Obsolete]
  public void CreateDelaunayWithThinOutPoints(List<(Agent, ParticleObject)> agents)
  {
    this.agents = agents;
    CreateDelaunayWithThinOutPoints();
  }

  //[Obsolete]
  public void CreateDelaunayWithThinOutPoints()
  {
    //スタックとレースを表示
    //Debug.Log("Object : " + this.Parent + "\n" + Utils.GetStackTrace());

    ResetLine();
    Tetrahedron.id_count = 0;
    var sw = new MyStopwatch();
    string sw_result = "VoronoiPartition - CreateDelaunayWithThinOutPoints\n";
    sw.Start();

    ThinOutPoints();

    sw.Stop();
    sw_result += sw.GetResultString("点間引き") + "\n";

    //Debug.Log("Agents Num. : " + this.agents.Count.ToString() + " => " + this.thinOutAgents.Count.ToString());

    Utils.Swap<List<(Agent, ParticleObject)>>(ref this.agents, ref this.thinOutAgents);

    sw.Restart();
    
    CreateDelaunay();

    sw.Stop();
    sw_result += sw.GetResultString("Delaunay(点間引き)") + "\n";
    sw.Restart();

    CreateVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("Voronoi(点間引き)") + "\n";
    sw.Restart();

    DrawDelaunay();

    sw.Stop();
    sw_result += sw.GetResultString("CreateLine&Mesh") + "\n";

    Utils.Swap<List<(Agent, ParticleObject)>>(ref this.agents, ref this.thinOutAgents);

    Debug.Log(sw_result);
    
  }

  //[Obsolete]
  private void ThinOutPoints()
  {
    //ドロネー分析
    CreateDelaunay();
    
    //必要な点を分析
    foreach (var tetra in this.t_list)
    {
      bool p1_is_exist = ((SwarmAgent)tetra.p1.agent.Item1).is_exist;
      bool p2_is_exist = ((SwarmAgent)tetra.p2.agent.Item1).is_exist;
      bool p3_is_exist = ((SwarmAgent)tetra.p3.agent.Item1).is_exist;
      bool p4_is_exist = ((SwarmAgent)tetra.p4.agent.Item1).is_exist;
      if (p1_is_exist != p2_is_exist) { tetra.p1.isNeeded = true; tetra.p2.isNeeded = true; }
      if (p1_is_exist != p3_is_exist) { tetra.p1.isNeeded = true; tetra.p3.isNeeded = true; }
      if (p1_is_exist != p4_is_exist) { tetra.p1.isNeeded = true; tetra.p4.isNeeded = true; }
      if (p2_is_exist != p3_is_exist) { tetra.p2.isNeeded = true; tetra.p3.isNeeded = true; }
      if (p2_is_exist != p4_is_exist) { tetra.p2.isNeeded = true; tetra.p4.isNeeded = true; }
      if (p3_is_exist != p4_is_exist) { tetra.p3.isNeeded = true; tetra.p4.isNeeded = true; }
    }

    //必要な点を対象とする．
    this.thinOutAgents = new List<(Agent, ParticleObject)>();
    for (int i = 0; i < this.points.Count; i++)
    {
      var point = this.points[i];
      if (point.isNeeded) this.thinOutAgents.Add(point.agent);
    }
  }
}

public class Tetrahedron
{
  public int id;
  public Vector3WithAgent p1;
  public Vector3WithAgent p2;
  public Vector3WithAgent p3;
  public Vector3WithAgent p4;
  public Vector3 center;
  public float radius;
  public TetraFace[] faces;
  public static int id_count = 0;
  public Tetrahedron(Vector3WithAgent p1, Vector3WithAgent p2, Vector3WithAgent p3, Vector3WithAgent p4)
  {
    this.id = id_count++;
    this.p1 = p1;
    this.p2 = p2;
    this.p3 = p3;
    this.p4 = p4;
    CalcCenterAndRadius();
    faces = new TetraFace[4];
  }

  public void CalcTetraFace()
  {

  }

  private void CalcCenterAndRadius()
  {
    /*
    Matrix4x4 a = new Matrix4x4(
      new Vector4(p1.x, p1.y, p1.z, 1f),
      new Vector4(p2.x, p2.y, p2.z, 1f),
      new Vector4(p3.x, p3.y, p3.z, 1f),
      new Vector4(p4.x, p4.y, p4.z, 1f)
    );

    Matrix4x4 d_x = new Matrix4x4(
      new Vector4(Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2) + Mathf.Pow(p1.z, 2), p1.y, p1.z, 1),
      new Vector4(Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2) + Mathf.Pow(p2.z, 2), p2.y, p2.z, 1),
      new Vector4(Mathf.Pow(p3.x, 2) + Mathf.Pow(p3.y, 2) + Mathf.Pow(p3.z, 2), p3.y, p3.z, 1),
      new Vector4(Mathf.Pow(p4.x, 2) + Mathf.Pow(p4.y, 2) + Mathf.Pow(p4.z, 2), p4.y, p4.z, 1)
    );

    Matrix4x4 d_y = new Matrix4x4(
      new Vector4(Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2) + Mathf.Pow(p1.z, 2), p1.x, p1.z, 1),
      new Vector4(Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2) + Mathf.Pow(p2.z, 2), p2.x, p2.z, 1),
      new Vector4(Mathf.Pow(p3.x, 2) + Mathf.Pow(p3.y, 2) + Mathf.Pow(p3.z, 2), p3.x, p3.z, 1),
      new Vector4(Mathf.Pow(p4.x, 2) + Mathf.Pow(p4.y, 2) + Mathf.Pow(p4.z, 2), p4.x, p4.z, 1)
    );

    Matrix4x4 d_z = new Matrix4x4(
      new Vector4(Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2) + Mathf.Pow(p1.z, 2), p1.x, p1.y, 1),
      new Vector4(Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2) + Mathf.Pow(p2.z, 2), p2.x, p2.y, 1),
      new Vector4(Mathf.Pow(p3.x, 2) + Mathf.Pow(p3.y, 2) + Mathf.Pow(p3.z, 2), p3.x, p3.y, 1),
      new Vector4(Mathf.Pow(p4.x, 2) + Mathf.Pow(p4.y, 2) + Mathf.Pow(p4.z, 2), p4.x, p4.y, 1)
    );

    float a_dit = a.determinant;
    float d_x_dit = d_x.determinant;
    float d_y_dit = -(d_y.determinant);
    float d_z_dit = d_z.determinant;

    this.center = new Vector3(
      d_x_dit / (2 * a_dit),
      d_y_dit / (2 * a_dit),
      d_z_dit / (2 * a_dit)
    );
    this.radius = (p1 - this.center).magnitude;
    */
    float[][] A = new float[][]
    {
      new float[]{p2.vec.x - p1.x, p2.y - p1.y, p2.z - p1.z},
      new float[]{p3.vec.x - p1.x, p3.y - p1.y, p3.z - p1.z},
      new float[]{p4.vec.x - p1.x, p4.y - p1.y, p4.z - p1.z}
    };
    float[] b = new float[]
    {
      0.5f * (p2.x * p2.x - p1.x * p1.x + p2.y * p2.y - p1.y * p1.y + p2.z * p2.z - p1.z * p1.z),
      0.5f * (p3.x * p3.x - p1.x * p1.x + p3.y * p3.y - p1.y * p1.y + p3.z * p3.z - p1.z * p1.z),
      0.5f * (p4.x * p4.x - p1.x * p1.x + p4.y * p4.y - p1.y * p1.y + p4.z * p4.z - p1.z * p1.z)
    };
    float[] x = new float[3];
    if(gauss(A, b, ref x) == 0)
    {
      this.radius = -1f;
    }
    else
    {
      this.center = new Vector3((float)x[0], (float)x[1], (float)x[2]);
      this.radius = (this.center - p1.vec).magnitude;
    }
    //this.radius = Mathf.Sqrt(Mathf.Pow(p1.x - this.center.x, 2) + Mathf.Pow(p1.y - this.center.y, 2) + Mathf.Pow(p1.z - this.center.z, 2));

    //Debug.Log(string.Format("p1 : {0}, center : {1}, radius : {2}, magnitude : {3}",p1, center, radius, (p1 - this.center).magnitude));
  }

  public bool CheckPointIncludeCircumsphere(Vector3WithAgent point)
  {
    float distance = (point.vec - this.center).magnitude;
    //Debug.Log(string.Format("center: {0}, radius: {1}, distance: {2}", this.center, this.radius, distance));
    return this.radius != -1f && distance < this.radius;
  }

  /// <summary>
  /// http://steve.hollasch.net/cgindex/geometry/ptintet.html
  /// </summary>
  /// <param name="point"></param>
  /// <returns></returns>
  public bool CheckPointIncludeTetrahedron(Vector3WithAgent point)
  {
    Vector4 v1 = new Vector4(p1.x, p1.y, p1.z, 1f);
    Vector4 v2 = new Vector4(p2.x, p2.y, p2.z, 1f);
    Vector4 v3 = new Vector4(p3.x, p3.y, p3.z, 1f);
    Vector4 v4 = new Vector4(p4.x, p4.y, p4.z, 1f);
    Vector4 p = new Vector4(point.x, point.y, point.z, 1f);
    float d0 = (new Matrix4x4(v1, v2, v3, v4)).determinant;
    float d1 = (new Matrix4x4(p, v2, v3, v4)).determinant;
    float d2 = (new Matrix4x4(v1, p, v3, v4)).determinant;
    float d3 = (new Matrix4x4(v1, v2, p, v4)).determinant;
    float d4 = (new Matrix4x4(v1, v2, v3, p)).determinant;
    return d0 >= 0 && d1 >= 0 && d1 >= 0 && d2 >= 0 && d3 >= 0 && d4 >= 0
      || d0 < 0 && d1 < 0 && d1 < 0 && d2 < 0 && d3 < 0 && d4 < 0;
  }

  /// <summary>
  /// https://www.openprocessing.org/sketch/31295#
  /// </summary>
  /// <param name="a"></param>
  /// <param name="ip"></param>
  /// <returns></returns>
  private float lu(float[][] a, ref int[] ip)
  {
    int n = a.Length;
    float[] weight = new float[n];
    
    for(int k = 0; k < n; k++)
    {
      ip[k] = k;
      float u = 0;
      for(int j = 0; j < n; j++)
      {
        float t = Mathf.Abs(a[k][j]);
        if (t > u) u = t;
      }
      if (u == 0) return 0f;
      weight[k] = 1f / u;
    }
    float det = 1f;
    for(int k = 0; k < n; k++)
    {
      float u = -1f;
      int m = 0;
      for(int i = k; i < n; i++)
      {
        int ii = ip[i];
        float t = Mathf.Abs(a[ii][k]) * weight[ii];
        if(t > u)
        {
          u = t;
          m = i;
        }
      }
      int ik = ip[m];
      if(m != k)
      {
        ip[m] = ip[k];
        ip[k] = ik;
        det = -det;
      }
      u = a[ik][k];
      det *= u;
      if (u == 0) return 0;
      for(int i = k + 1; i < n; i++)
      {
        int ii = ip[i];
        float t = (a[ii][k] /= u);
        for(int j = k + 1; j < n; j++)
        {
          a[ii][j] -= t * a[ik][j];
        }
      }
    }
    return det;
  }

  private void solve(float[][] a, float[] b, int[] ip, ref float[] x)
  {
    int n = a.Length;
    for(int i = 0; i < n; i++)
    {
      int ii = ip[i];
      float t = b[ii];
      for(int j = 0; j < i; j++)
      {
        t -= a[ii][j] * x[j];
      }
      x[i] = t;
    }
    for(int i = n - 1; i >= 0; i--)
    {
      float t = x[i];
      int ii = ip[i];
      for(int j = i + 1; j < n; j++)
      {
        t -= a[ii][j] * x[j];
      }
      x[i] = t / a[ii][i];
    }
  }

  private float gauss(float[][] a, float[] b, ref float[] x)
  {
    int n = a.Length;
    int[] ip = new int[n];
    float det = lu(a, ref ip);

    if(det != 0)
    {
      solve(a, b, ip, ref x);
    }
    return det;
  }
}

public class TetraFace
{
  public Vector3 p1;
  public Vector3 p2;
  public Vector3 p3;
  public List<Tetrahedron> included;

  public TetraFace()
  {
    included = new List<Tetrahedron>();
  }
  public TetraFace(Vector3 p1, Vector3 p2, Vector3 p3) : this()
  {
    this.p1 = p1;
    this.p2 = p2;
    this.p3 = p3;
  }
  public TetraFace(Vector3 p1, Vector3 p2, Vector3 p3, Tetrahedron[] tetra) : this(p1, p2, p3)
  {
    for(int i = 0; i < tetra.Length; i++)
    {
      this.included.Add(tetra[i]);
    }
  }
}

public class Vector3WithAgent
{
  public int id;
  public (Agent, ParticleObject) agent;
  public List<Vector3> vertices;
  public List<(int, int)> edges;
  public List<Tetrahedron> tetra;
  public bool isNeeded;

  public Vector3WithAgent()
  {
    agent = (new SwarmAgent(), null);
    id = -1;
    tetra = new List<Tetrahedron>();
    isNeeded = false;
  }

  public Vector3WithAgent((Agent, ParticleObject) agent, int id) : this()
  {
    this.id = id;
    this.agent = agent;
  }

  public Vector3WithAgent(float x, float y, float z)
  {
    this.id = -1;
    this.agent = (new SwarmAgent(), null);
    this.agent.Item1.Position = new Vector3(x, y, z);
  }

  public Vector3 vec
  {
    set { this.agent.Item1.Position = value; }
    get { return this.agent.Item1.Position; }
  }

  public float x
  {
    set { this.agent.Item1.Position.x = value; }
    get { return this.agent.Item1.Position.x; }
  }

  public float y
  {
    set { this.agent.Item1.Position.y = value; }
    get { return this.agent.Item1.Position.y; }
  }

  public float z
  {
    set { this.agent.Item1.Position.z = value; }
    get { return this.agent.Item1.Position.z; }
  }
}