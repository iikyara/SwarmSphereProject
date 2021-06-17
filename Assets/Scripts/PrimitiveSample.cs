using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveSample : MonoBehaviour
{
  public List<Primitive> Primitives;

  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material RelationEdgeMaterial;
  public Material EdgeMaterial;
  public Material MeshMaterial;

  public Material BaseMeshMaterial;
  public Material DisabledParticleMaterial;
  public GameObject ParentObject;

  public List<GameObject> PrimitiveParents;

  public bool viewAgent;
  public bool viewDelaunayLine;
  public bool viewVoronoiLine;
  public bool viewVoronoiMesh;

  public bool viewBaseDelaunayLines;
  public bool viewBaseVoronoiLines;
  public bool viewBaseVoronoiMeshs;

  public Vector3 Size;

  public Vector2Int SepVRange;
  public Vector2Int SepHRange;

  public bool Rotation;
  public bool ClearRotate;

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
    this.PrimitiveParents = new List<GameObject>();
    ViewSetup();
    MaterialSetup();

    //生成
    //p.AddAgentsLocatedAsSphere(OptionSize, OptionPosition, Quaternion.Euler(OptionRotate.x, OptionRotate.y, OptionRotate.z), OptionSepH, OptionSepV);
    float size = Size.x;
    float d = 2f;
    float offsetH = (SepHRange.y - SepHRange.x + 1) / 2f + SepHRange.x;
    float offsetV = (SepVRange.y - SepVRange.x + 1) / 2f + SepVRange.x;
    for (int i = SepHRange.x; i <= SepHRange.y; i++)
    {
      for(int j = SepVRange.x; j <= SepVRange.y; j++)
      {
        var p = new Primitive(
          Size,
          new Vector3((size * 2 + d) * (j - offsetV), (size * 2 + d) * (i - offsetH), 0f),
          Quaternion.Euler(0, 0, 0),
          j,
          i
        );
        var parent = new GameObject("Primitive");
        Utils.SetParent(this.ParentObject, parent);
        //Debug.Log("a, " + parent);
        p.SetParent(parent);
        parent.transform.position = new Vector3((size * 2 + d) * (j - offsetV), (size * 2 + d) * (i - offsetH), 0f);
        p.UpdateParticleObject();
        this.Primitives.Add(p);
        this.PrimitiveParents.Add(parent);
      }
    }

    MyStopwatch sw = new MyStopwatch();
    sw.Start();

    //各プリミティブのボロノイ図を作成
    foreach (var p in this.Primitives)
    {
      p.CreateVoronoi();
    }

    sw.Stop();
    sw.ShowResult("All");
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

  // Update is called once per frame
  private int t = 0;
  [System.Obsolete]
  void Update()
  {
    if (Rotation)
    {
      foreach (var go in this.PrimitiveParents)
      {
        go.transform.rotation = Quaternion.Euler(t, t, t);
      }
      t++;
    }
    if (ClearRotate)
    {
      foreach (var go in this.PrimitiveParents)
      {
        go.transform.rotation = Quaternion.Euler(0, 0, 0);
      }
      t = 0;
      ClearRotate = false;
    }
    SetVisible();
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
