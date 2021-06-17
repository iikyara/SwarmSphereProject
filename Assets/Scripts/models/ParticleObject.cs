using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleObject
{
  //public params
  public Material Material;
  public Agent Agent;
  public GameObject ParentObject;
  //public Vector3 Position;
  //public float Size;

  public LayerMask Layer;

  public int MaxSep = 10;
  public int MinSep = 1;
  public float MaxSize = 10.0f;
  public float MinSize = 0.1f;

  //private params
  private static Dictionary<int, Mesh> SpheresPrefavs = new Dictionary<int, Mesh>();
  private GameObject Sphere;
  private int current_sep = -1;
  private MeshFilter meshFilter;

  public ParticleObject() : this(null, null, null) { }

  public ParticleObject(Agent agent, Material mat) : this(agent, mat, null) { }

  public ParticleObject(Material mat) : this(null, mat, null) { }

  public ParticleObject(Agent agent, Material mat, GameObject parentObject)
  {
    this.Agent = agent;
    this.Material = mat;
    this.Sphere = this.CreateSphereGameObject(1);
    this.meshFilter = Sphere.GetComponent<MeshFilter>();
    if (agent != null)
    {
      this.Sphere.name = agent.Name;
    }
    this.SetParentObject(parentObject);
    this.SetLayer(0);
  }

  public ParticleObject(ParticleObject instanse) : this(instanse.Agent, instanse.Material)
  {
    GameObject pObj = new GameObject(instanse.ParentObject.name);
    pObj.transform.parent = instanse.ParentObject.transform.parent;
    SetParentObject(pObj);
  }

  public void SetParentObject(GameObject parent)
  {
    this.ParentObject = parent;
    Utils.SetParent(parent, this.Sphere);
  }

  public void SetMaterial(Material mat)
  {
    this.Material = mat;
    this.Sphere.GetComponent<MeshRenderer>().material = mat;
  }

  public void SetActive(bool active)
  {
    this.Sphere.SetActive(active);
  }

  public void SetLayer(LayerMask layer)
  {
    this.Layer = layer;
    this.Sphere.layer = layer;
  }

  public void UpdateSphere()
  {
    //位置の更新
    this.Sphere.transform.position = this.Agent.Position;
    this.Sphere.transform.localScale = new Vector3(this.Agent.Size, this.Agent.Size, this.Agent.Size);

    //分割数の更新
    float Size = this.Agent.Size;
    int sep = MinSep + (int)((MaxSep - MinSep) / (MaxSize - MinSize) * (Size - MinSize));
    if (Size < MinSize)
    {
      sep = MinSep;
    }
    else if (Size > MaxSize)
    {
      sep = MaxSep;
    }
    if(this.current_sep != sep)
    {
      meshFilter.sharedMesh = this.CreateSphereMesh(sep);
    }
  }

  public void Discard()
  {
    Object.Destroy(this.Sphere);
  }

  private GameObject CreateSphereGameObject(int sep)
  {
    var gameobject = new GameObject();
    gameobject.AddComponent<MeshRenderer>();
    gameobject.AddComponent<MeshFilter>();

    var mesh = CreateSphereMesh(sep);
    gameobject.GetComponent<MeshFilter>().sharedMesh = mesh;
    gameobject.GetComponent<MeshRenderer>().material = Material;

    return gameobject;
  }

  private Mesh CreateSphereMesh(int sep)
  {
    if (SpheresPrefavs.ContainsKey(sep))
    {
      return SpheresPrefavs[sep];
    }

    float r = 1f;

    var vertices = new List<Vector3>();
    var triangles = new List<int>();
    var triangles_tmp = new List<Vector3>();

    // メッシュの生成
    var seeks = new Vector3[][]
    {
      new Vector3[]{new Vector3( 0f,  0f,  1f), new Vector3( 1f,  0f,  0f), new Vector3( 0f,  1f,  0f)},
      new Vector3[]{new Vector3( 0f,  0f,  1f), new Vector3( 0f, -1f,  0f), new Vector3( 1f,  0f,  0f)},
      new Vector3[]{new Vector3(-1f,  0f,  0f), new Vector3( 0f, -1f,  0f), new Vector3( 0f,  0f,  1f)},
      new Vector3[]{new Vector3( 0f,  0f, -1f), new Vector3( 0f, -1f,  0f), new Vector3(-1f,  0f,  0f)},
      new Vector3[]{new Vector3( 1f,  0f,  0f), new Vector3( 0f, -1f,  0f), new Vector3( 0f,  0f, -1f)},
      new Vector3[]{new Vector3(-1f,  0f,  0f), new Vector3( 0f,  0f, -1f), new Vector3( 0f, -1f,  0f)}
    };

    for (int i = 0; i < seeks.Length; i++)
    {
      int v_num = vertices.Count;

      //vertices
      var seek_u = seeks[i][0];
      var seek_v = seeks[i][1];
      var norm = seeks[i][2];
      for (int v = 0; v < sep + 2; v++)
      {
        var pos_v = r * seek_v * (v / (float)(sep + 1) - 1f / 2f);
        for (int u = 0; u < sep + 2; u++)
        {
          var pos_u = r * seek_u * (u / (float)(sep + 1) - 1f / 2f);
          var pos = pos_u + pos_v + r / 2 * norm;
          vertices.Add(r * pos.normalized);
        }
      }

      //faces
      for (int v = 0; v < sep + 1; v++)
      {
        for (int u = 0; u < sep + 1; u++)
        {
          int index1 = v_num + v * (sep + 2) + u;
          int index2 = v_num + (v + 1) * (sep + 2) + u;
          //face1
          triangles_tmp.Add(vertices[index1]);
          triangles_tmp.Add(vertices[index1 + 1]);
          triangles_tmp.Add(vertices[index2]);
          //face2
          triangles_tmp.Add(vertices[index1 + 1]);
          triangles_tmp.Add(vertices[index2 + 1]);
          triangles_tmp.Add(vertices[index2]);
        }
      }
    }

    for (int i = 0; i < triangles_tmp.Count; i++)
    {
      triangles.Add(vertices.IndexOf(triangles_tmp[i]));
    }

    //メッシュオブジェクト作成
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    //追加
    SpheresPrefavs[sep] = mesh;

    return mesh;
  }
}
