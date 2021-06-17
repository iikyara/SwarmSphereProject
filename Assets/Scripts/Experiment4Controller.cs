using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment4Controller : MonoBehaviour
{
  Sketch sketch;
  public bool capture;

  public List<Primitive> Primitives;

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
  public bool CreatePrimitive;

  public AgentTarget agentTarget;

  public float OptionSize;
  public Vector3 OptionPosition;
  public Vector3 OptionRotate;
  public int OptionSepH;
  public int OptionSepV;

  private bool previousViewAgent;
  private bool previousViewDelaunayLine;
  private bool previousViewVoronoiLine;
  private bool previousViewVoronoimesh;

  private bool previousViewBaseDelaunayLines;
  private bool previousViewBaseVoronoiLines;
  private bool previousViewBaseVoronoiMeshs;


  // Start is called before the first frame update
  void Start()
  {
    this.Primitives = new List<Primitive>();
    sketch = new Sketch(new Vector3(0f, 0f, -20f), Quaternion.Euler(0f, 0f, 0f));
    ViewSetup();
    MaterialSetup();
  }

  public void ViewSetup()
  {
    this.previousViewAgent = !this.viewAgent;
    this.previousViewDelaunayLine = !this.viewDelaunayLine;
    this.previousViewVoronoiLine = !this.viewVoronoiLine;
    this.previousViewVoronoimesh = !this.viewVoronoiMesh;
    this.previousViewBaseDelaunayLines = !this.viewBaseDelaunayLines;
    this.previousViewBaseVoronoiLines = !this.viewBaseVoronoiLines;
    this.previousViewBaseVoronoiMeshs = !this.viewBaseVoronoiMeshs;
  }

  public void MaterialSetup()
  {
    //Primitive.SetBaseParent()
    Primitive.SetBaseMaterial(ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
    VoronoiPartition.SetBaseMaterials(RelationEdgeMaterial, EdgeMaterial, BaseMeshMaterial, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
    Primitive.CombinedVoronoiPartition.LoadBaseMaterials();
    Primitive.CombinedVoronoiPartition.meshMaterial = MeshMaterial;
    //VoronoiPartition.SetBaseParent()
  }

  public void AddPrimitive(AgentTarget at)
  {
    Primitive p = new Primitive();
    if (at == AgentTarget.test)
    {
      p.AddAgentsTest();
    }
    else if (at == AgentTarget.question)
    {
      p.AddAgentsLocatedAsQuestion();
    }
    else if (at == AgentTarget.toras)
    {
      p.AddAgentsLocatedAsToras();
    }
    else if (at == AgentTarget.monkey)
    {
      p.AddAgentsLocatedAsMonkey();
    }
    else if (at == AgentTarget.SpheresCube)
    {
      p.AddAgentsLocatedAsCube();
    }
    else if (at == AgentTarget.SpheresCube2)
    {
      p.AddAgentsLocatedAsCube2();
    }
    else if (at == AgentTarget.Sphere)
    {
      p.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 20, 10);
    }
    else if (at == AgentTarget.Cube)
    {
      p.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 4, 3);
    }
    else if (at == AgentTarget.Conical)
    {
      p.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 3, 2);
    }
    else if (at == AgentTarget.Triangle)
    {
      p.AddAgentsLocatedAsSphere(5f, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0f, 0f), 3, 3);
    }
    else if (at == AgentTarget.Option)
    {
      p.AddAgentsLocatedAsSphere(OptionSize, OptionPosition, Quaternion.Euler(OptionRotate.x, OptionRotate.y, OptionRotate.z), OptionSepH, OptionSepV);
    }
    else
    {
      p.AddAgentsTest();
    }
    this.Primitives.Add(p);
  }

  // Update is called once per frame
  [System.Obsolete]
  void Update()
  {
    //エージェントアップデート
    foreach (var p in this.Primitives)
    {
      p.UpdateParticleObject();
    }

    //新たなプリミティブの作成
    if (CreatePrimitive)
    {
      AddPrimitive(this.agentTarget);
      CreatePrimitive = false;
    }

    //ドロネー図の作成
    if (CreateVoronoi)
    {
      MyStopwatch sw = new MyStopwatch();
      sw.Start();

      //各プリミティブのボロノイ図を作成
      foreach (var p in this.Primitives)
      {
        p.CreateVoronoi();
      }

      //結合ボロノイ図を作成
      Primitive.CombinePrimitives(this.Primitives.ToArray());
      Primitive.CombinedVoronoiPartition.CreateDelaunayWithThinOutPoints();

      CreateVoronoi = false;

      sw.Stop();
      sw.ShowResult("All");
    }

    SetVisible();

    //撮影
    if (capture)
    {
      capture = !capture;
      Debug.Log(Primitives.Count);
      Primitives[0].SetLayer(LayerMask.NameToLayer("ForDrawable"));
      sketch.Capture();
      Primitives[0].SetLayer(0);
    }
  }

  public void SetVisible()
  {
    //可視化の設定
    if (this.previousViewAgent != this.viewAgent)
    {
      Debug.Log("change view agent : " + this.viewAgent.ToString());
      foreach (var p in this.Primitives)
      {
        foreach (var a in p.Agents)
        {
          a.Item2.SetActive(this.viewAgent);
        }
      }
      this.previousViewAgent = this.viewAgent;
    }

    //可視化設定
    if (this.previousViewDelaunayLine != this.viewDelaunayLine)
    {
      Debug.Log("change view delaunay line : " + this.viewDelaunayLine.ToString());
      Primitive.CombinedVoronoiPartition.SetVisibleDelaunayLine(this.viewDelaunayLine);
      this.previousViewDelaunayLine = this.viewDelaunayLine;
    }
    if (this.previousViewVoronoiLine != this.viewVoronoiLine)
    {
      Debug.Log("change view voronoi line : " + this.viewVoronoiLine.ToString());
      Primitive.CombinedVoronoiPartition.SetVisibleVoronoiLine(this.viewVoronoiLine);
      this.previousViewVoronoiLine = this.viewVoronoiLine;
    }
    if (this.previousViewVoronoimesh != this.viewVoronoiMesh)
    {
      Debug.Log("change view voronoi mesh : " + this.viewVoronoiMesh.ToString());
      Primitive.CombinedVoronoiPartition.SetVisibleVoronoiMesh(this.viewVoronoiMesh);
      this.previousViewVoronoimesh = this.viewVoronoiMesh;
    }
    if (this.previousViewBaseDelaunayLines != this.viewBaseDelaunayLines)
    {
      Debug.Log("change view delaunay line : " + this.viewBaseDelaunayLines.ToString());
      foreach (var p in this.Primitives)
      {
        p.VoronoiPartition.SetVisibleDelaunayLine(this.viewBaseDelaunayLines);
      }
      this.previousViewBaseDelaunayLines = this.viewBaseDelaunayLines;
    }
    if (this.previousViewBaseVoronoiLines != this.viewBaseVoronoiLines)
    {
      Debug.Log("change view voronoi line : " + this.viewBaseVoronoiLines.ToString());
      foreach (var p in this.Primitives)
      {
        p.VoronoiPartition.SetVisibleVoronoiLine(this.viewBaseVoronoiLines);
      }
      this.previousViewBaseVoronoiLines = this.viewBaseVoronoiLines;
    }
    if (this.previousViewBaseVoronoiMeshs != this.viewBaseVoronoiMeshs)
    {
      Debug.Log("change view voronoi mesh : " + this.viewBaseVoronoiMeshs.ToString());
      foreach (var p in this.Primitives)
      {
        p.VoronoiPartition.SetVisibleVoronoiMesh(this.viewBaseVoronoiMeshs);
      }
      this.previousViewBaseVoronoiMeshs = this.viewBaseVoronoiMeshs;
    }
  }
}
