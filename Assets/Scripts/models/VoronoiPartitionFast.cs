using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 点群→ドロネー図→ボロノイ図→3Dモデル出力を行うクラス．
/// 内部では
///   点群→ドロネー線→3Dモデル（ボロノイ図）
/// という順で生成している．
/// よって，ボロノイ面と出力3Dモデルは同じものを示す．
/// </summary>
public class VoronoiPartitionFast
{
  /* 生成アルゴリズム用 */
  //private List<(Agent, ParticleObject)> agents;
  //private List<(Agent, ParticleObject)> thinOutAgents;
  //private List<Vector3WithAgent> points;
  //private List<Vector3WithAgent> thinOutPoints;
  //private List<Tetrahedron> t_list;

  #region Property
  /* 全要素 */
  /// <summary>
  /// ソース点群
  /// </summary>
  private List<DPPoint> agents;
  /// <summary>
  /// ソース点群から不要な点を抜いたもの
  /// </summary>
  private List<DPPoint> thinOutAgents;
  /// <summary>
  /// ドロネー点
  /// </summary>
  private List<DPPoint> DPPoints;
  /// <summary>
  /// 逐次添加法の最初の四面体を作る仮の点
  /// </summary>
  private List<DPPoint> FirstPoints;
  /// <summary>
  /// ドロネー辺
  /// </summary>
  private List<DPEdge> DPEdges;
  /// <summary>
  /// ドロネー面
  /// </summary>
  private List<DPFace> DPFaces;
  /// <summary>
  /// ドロネー四面体
  /// </summary>
  private List<DPTetrahedron> DPTetrahedrons;
  /// <summary>
  /// ボロノイ辺
  /// </summary>
  private List<VPEdge> VPEdges;
  /// <summary>
  /// ボロノイ面
  /// </summary>
  private List<VPFace> VPFaces;

  /* プロパティ */
  /// <summary>
  /// ドロネー図とボロノイ図が作られたか
  /// </summary>
  public bool IsCreated;
  /// <summary>
  /// ドロネー図が作られたか
  /// </summary>
  public bool DeraunayIsCreated;
  /// <summary>
  /// ボロノイ図が作られたか
  /// </summary>
  public bool VoronoiIsCreated;

  /* ビュー用 */
  /// <summary>
  /// ドロネー線のゲームオブジェクトリスト
  /// </summary>
  private List<GameObject> delaunayLine;
  /// <summary>
  /// ボロノイ線のゲームオブジェクトリスト
  /// </summary>
  private List<GameObject> voronoiLine;
  /// <summary>
  /// ボロノイ面（境界のみ保持）のゲームオブジェクト
  /// </summary>
  public GameObject voronoiMesh;
  /// <summary>
  /// ドロネー線の総数
  /// </summary>
  private int delaunayLine_index;
  /// <summary>
  /// ボロノイ線の総数
  /// </summary>
  private int voronoiLine_index;

  /* 可視性 */
  /// <summary>
  /// ドロネー線を生成するかどうか
  /// </summary>
  public bool isVisibleDelaunayLine;
  /// <summary>
  /// ボロノイ線を生成するかどうか
  /// </summary>
  public bool isVisibleVoronoiLine;
  /// <summary>
  /// ボロノイ面を生成するかどうか
  /// </summary>
  public bool isVisibleVoronoiMesh;

  /* 親オブジェクト（クラス内で生成） */
  /// <summary>
  /// edgeParentとmeshParentとvoronoiMeshの親オブジェクト
  /// </summary>
  private GameObject Parent;
  /// <summary>
  /// ドロネー線の親オブジェクト
  /// </summary>
  private GameObject edgeParent;
  /// <summary>
  /// ボロノイ線の親オブジェクト
  /// </summary>
  private GameObject meshParent;

  /// <summary>
  /// デフォルトのparent
  /// </summary>
  public static GameObject base_Parent = null;
  /// <summary>
  /// デフォルトのedgeParent
  /// </summary>
  public static GameObject base_edgeParent = null;
  /// <summary>
  /// デフォルトのmeshParent
  /// </summary>
  public static GameObject base_meshParent = null;

  /* マテリアル関連 */
  /// <summary>
  /// ドロネー線のマテリアル
  /// </summary>
  public Material relationEdgeMaterial;
  /// <summary>
  /// ボロノイ線のマテリアル
  /// </summary>
  public Material edgeMaterial;
  /// <summary>
  /// ボロノイ面（出力3Dモデル）のマテリアル
  /// </summary>
  public Material meshMaterial;
  /// <summary>
  /// 存在点のマテリアル
  /// </summary>
  public Material ExistentialParticleMaterial;
  /// <summary>
  /// 非存在点のマテリアル
  /// </summary>
  public Material NonExistentialParticleMaterial;
  /// <summary>
  /// 無効化された点のマテリアル
  /// </summary>
  public Material DisabledParticleMaterial;

  /// <summary>
  /// デフォルトのドロネー線のマテリアル
  /// </summary>
  public static Material base_relationEdgeMaterial = null;
  /// <summary>
  /// デフォルトのボロノイ線のマテリアル
  /// </summary>
  public static Material base_edgeMaterial = null;
  /// <summary>
  /// デフォルトのボロノイ面（出力3Dモデル）のマテリアル
  /// </summary>
  public static Material base_meshMaterial = null;
  /// <summary>
  /// デフォルトの存在点のマテリアル
  /// </summary>
  public static Material base_ExistentialParticleMaterial = null;
  /// <summary>
  /// デフォルトの非存在点のマテリアル
  /// </summary>
  public static Material base_NonExistentialParticleMaterial = null;
  /// <summary>
  /// デフォルトの無効化された点のマテリアル
  /// </summary>
  public static Material base_DisabledParticleMaterial = null;

  /* オプション */
  /// <summary>
  /// 出力3Dモデルのレイヤー
  /// </summary>
  public LayerMask Layer;

  #endregion Property


  #region Constracter
  /// <summary>
  /// コンストラクタ
  /// </summary>
  public VoronoiPartitionFast() : this(
    new List<(Agent, ParticleObject)>(), base_relationEdgeMaterial, base_edgeMaterial, base_meshMaterial,
    base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial,
    base_DisabledParticleMaterial, null, null, null
  ) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="agents">点群</param>
  public VoronoiPartitionFast(List<(Agent, ParticleObject)> agents) : this(
    agents, base_relationEdgeMaterial, base_edgeMaterial, base_meshMaterial,
    base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial,
    base_DisabledParticleMaterial, null, null, null
  ) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="agents">点群</param>
  /// <param name="edgeParent">ドロネー線の親オブジェクト</param>
  /// <param name="meshParent">ボロノイ線の親オブジェクト</param>
  public VoronoiPartitionFast(List<(Agent, ParticleObject)> agents, GameObject edgeParent, GameObject meshParent) : this(
    agents, base_relationEdgeMaterial, base_edgeMaterial, base_meshMaterial,
    base_ExistentialParticleMaterial, base_NonExistentialParticleMaterial,
    base_DisabledParticleMaterial, edgeParent, meshParent, null
  )
  { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="agents">点群</param>
  /// <param name="relEdgeMat">ドロネー線のマテリアル</param>
  /// <param name="edgeMat">ボロノイ線のマテリアル</param>
  /// <param name="meshMat">出力3Dモデルのマテリアル</param>
  /// <param name="parent">edgeParentとmeshParentとvoronoiMeshの親オブジェクト</param>
  public VoronoiPartitionFast(List<(Agent, ParticleObject)> agents,
    Material relEdgeMat, Material edgeMat, Material meshMat, GameObject parent
  ) : this(
    agents, relEdgeMat, edgeMat, meshMat, base_ExistentialParticleMaterial,
    base_NonExistentialParticleMaterial, base_DisabledParticleMaterial, null, null, null
  ){ }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="agents">点群</param>
  /// <param name="relEdgeMat">ドロネー線のマテリアル</param>
  /// <param name="edgeMat">ボロノイ線のマテリアル</param>
  /// <param name="meshMat">出力3Dモデルのマテリアル</param>
  /// <param name="edgeParent">ドロネー線の親オブジェクト</param>
  /// <param name="meshParent">ボロノイ線の親オブジェクト</param>
  public VoronoiPartitionFast(List<(Agent, ParticleObject)> agents,
    Material relEdgeMat, Material edgeMat, Material meshMat, GameObject edgeParent, GameObject meshParent
  ) : this(
    agents, relEdgeMat, edgeMat, meshMat, base_ExistentialParticleMaterial,
    base_NonExistentialParticleMaterial, base_DisabledParticleMaterial, edgeParent, meshParent, null
  ) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="agents">点群</param>
  /// <param name="relEdgeMat">ドロネー線のマテリアル</param>
  /// <param name="edgeMat">ボロノイ線のマテリアル</param>
  /// <param name="meshMat">出力3Dモデルのマテリアル</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disable">無効化された点のマテリアル</param>
  /// <param name="edgeParent">ドロネー線の親オブジェクト</param>
  /// <param name="meshParent">ボロノイ線の親オブジェクト</param>
  /// <param name="parent">edgeParentとmeshParentとvoronoiMeshの親オブジェクト</param>
  public VoronoiPartitionFast(List<(Agent, ParticleObject)> agents, Material relEdgeMat, Material edgeMat, Material meshMat, Material existential, Material nonexistential, Material disable, GameObject edgeParent, GameObject meshParent, GameObject parent)
  {
    //ドロネー&ボロノイ分割用
    /*this.points = new List<Vector3WithAgent>();
    //this.thinOutPoints = new List<Vector3WithAgent>();
    this.thinOutAgents = new List<(Agent, ParticleObject)>();
    this.t_list = new List<Tetrahedron>();*/
    this.agents = DPPoint.ListToVPPoints(agents);
    this.thinOutAgents = new List<DPPoint>();
    this.DPPoints = new List<DPPoint>();
    this.FirstPoints = new List<DPPoint>();
    this.DPEdges = new List<DPEdge>();
    this.DPFaces = new List<DPFace>();
    this.DPTetrahedrons = new List<DPTetrahedron>();
    this.VPEdges = new List<VPEdge>();
    this.VPFaces = new List<VPFace>();
    //描画用オブジェクト
    this.delaunayLine = new List<GameObject>();
    this.voronoiLine = new List<GameObject>();
    //描画用オブジェクト削減用インデックス
    this.delaunayLine_index = 0;
    this.voronoiLine_index = 0;
    //初期化
    this.DeraunayIsCreated = false;
    this.VoronoiIsCreated = false;
    //描画を設定
    SetVisible(false, false, true);
    //デフォルトのマテリアルと親を設定
    SetMaterials(relEdgeMat, edgeMat, meshMat, existential, nonexistential, disable);
    SetParent(edgeParent, meshParent);
    SetParent(parent);
    SetLayer(0);
  }

  #endregion Constracter

  /// <summary>
  /// ドロネー図およびボロノイ図を破棄する．
  /// </summary>
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

  /* 可視性のセッター */
  #region SetterVisible
  /// <summary>
  /// 可視性を設定する．
  /// 可視性がfalseのものは生成されなくなる（時間短縮）
  /// </summary>
  /// <param name="delaunay">ドロネー線の可視性</param>
  /// <param name="voronoiLine">ボロノイ線の可視性</param>
  /// <param name="voronoiMesh">ボロノイ面（出力3Dモデル）の可視性</param>
  public void SetVisible(bool delaunay, bool voronoiLine, bool voronoiMesh)
  {
    SetVisibleDelaunayLine(delaunay);
    SetVisibleVoronoiLine(voronoiLine);
    SetVisibleVoronoiMesh(voronoiMesh);
  }

  /// <summary>
  /// ドロネー線の可視性を設定
  /// </summary>
  /// <param name="delaunay">ドロネー線の可視性</param>
  public void SetVisibleDelaunayLine(bool delaunay)
  {
    this.isVisibleDelaunayLine = delaunay;
    foreach (GameObject obj in this.delaunayLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = this.isVisibleDelaunayLine;
    }
  }

  /// <summary>
  /// ボロノイ線の可視性を設定
  /// </summary>
  /// <param name="voronoiLine">ボロノイ線の可視性</param>
  public void SetVisibleVoronoiLine(bool voronoiLine)
  {
    this.isVisibleVoronoiLine = voronoiLine;
    foreach (GameObject obj in this.voronoiLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.enabled = this.isVisibleVoronoiLine;
    }
  }

  /// <summary>
  /// ボロノイ面（出力3Dモデル）の可視性を設定
  /// </summary>
  /// <param name="voronoiMesh">ボロノイ面（出力3Dモデル）の可視性</param>
  public void SetVisibleVoronoiMesh(bool voronoiMesh)
  {
    this.isVisibleVoronoiMesh = voronoiMesh;
    if (this.voronoiMesh) this.voronoiMesh.SetActive(this.isVisibleVoronoiMesh);
  }
  #endregion SetterVisible

  /* 親オブジェクトのセッター */
  #region SetterParent
  /// <summary>
  /// デフォルトの親オブジェクトを設定
  /// </summary>
  /// <param name="parent">デフォルトの親オブジェクト</param>
  public static void SetBaseParent(GameObject parent)
  {
    VoronoiPartitionFast.base_Parent = parent;
  }

  /// <summary>
  /// デフォルトのedgeParentとmeshParentを設定
  /// </summary>
  /// <param name="edgeParent">デフォルトのedgeParent</param>
  /// <param name="meshParent">デフォルトのmeshParent</param>
  public static void SetBaseParent(GameObject edgeParent, GameObject meshParent)
  {
    VoronoiPartitionFast.base_edgeParent = edgeParent;
    VoronoiPartitionFast.base_meshParent = meshParent;
  }

  /// <summary>
  /// 親オブジェクトを設定
  /// このクラスで生成される全てのオブジェクトの親となる．
  /// </summary>
  /// <param name="parent">親オブジェクト</param>
  public void SetParent(GameObject parent)
  {
    //Debug.Log("c, " + parent);
    this.Parent = parent;
    Utils.SetParent(parent, this.edgeParent);
    Utils.SetParent(parent, this.meshParent);
    if (this.voronoiMesh) Utils.SetParent(parent, this.voronoiMesh);
  }

  /// <summary>
  /// ドロネー線とボロノイ線の親オブジェクトを設定
  /// これらのオブジェクトはparentオブジェクトの子となる．
  /// </summary>
  /// <param name="edgeParent">ドロネー線の親オブジェクト</param>
  /// <param name="meshParent">ボロノイ線の親オブジェクト</param>
  public void SetParent(GameObject edgeParent, GameObject meshParent)
  {
    SetBaseParent(edgeParent, meshParent);
    this.edgeParent = edgeParent;
    this.meshParent = meshParent;
  }

  /// <summary>
  /// ドロネー線に親オブジェクトを設定
  /// </summary>
  /// <param name="child">設定されるドロネー線のオブジェクト</param>
  private void SetEdgeParent(GameObject child)
  {
    if (this.edgeParent != null)
    {
      Utils.SetParent(this.edgeParent, child);
      //child.transform.parent = this.edgeParent.transform;
    }
  }

  /// <summary>
  /// ボロノイ線に親オブジェクトを設定
  /// </summary>
  /// <param name="child">設定されるボロノイ線のオブジェクト</param>
  private void SetMeshParent(GameObject child)
  {
    if (this.meshParent != null)
    {
      Utils.SetParent(this.meshParent, child);
      //child.transform.parent = this.meshParent.transform;
    }
  }
  #endregion SetterParent

  /* マテリアル関連のセッター */
  #region SetterMaterial
  /// <summary>
  /// デフォルトのマテリアルを設定する．
  /// </summary>
  public void LoadBaseMaterials()
  {
    this.ExistentialParticleMaterial = base_ExistentialParticleMaterial;
    this.NonExistentialParticleMaterial = base_NonExistentialParticleMaterial;
    this.DisabledParticleMaterial = base_DisabledParticleMaterial;
    this.relationEdgeMaterial = base_relationEdgeMaterial;
    this.edgeMaterial = base_edgeMaterial;
    this.meshMaterial = base_meshMaterial;
  }

  /// <summary>
  /// デフォルトのマテリアルを設定する．
  /// </summary>
  /// <param name="relEdgeMat">デフォルトのドロネー線のマテリアル</param>
  /// <param name="edgeMat">デフォルトのボロノイ線のマテリアル</param>
  /// <param name="meshMat">デフォルトのボロノイ面のマテリアル</param>
  /// <param name="existential">デフォルトの存在点のマテリアル</param>
  /// <param name="nonexistential">デフォルトの非存在点のマテリアル</param>
  /// <param name="disable">デフォルトの無効化された点のマテリアル</param>
  public static void SetBaseMaterials(Material relEdgeMat, Material edgeMat, Material meshMat, Material existential, Material nonexistential, Material disable)
  {
    VoronoiPartitionFast.base_ExistentialParticleMaterial = existential;
    VoronoiPartitionFast.base_NonExistentialParticleMaterial = nonexistential;
    VoronoiPartitionFast.base_DisabledParticleMaterial = disable;
    VoronoiPartitionFast.base_relationEdgeMaterial = relEdgeMat;
    VoronoiPartitionFast.base_edgeMaterial = edgeMat;
    VoronoiPartitionFast.base_meshMaterial = meshMat;
  }

  /// <summary>
  /// マテリアルを設定する．
  /// </summary>
  /// <param name="relEdgeMat">ドロネー線のマテリアル</param>
  /// <param name="edgeMat">ボロノイ線のマテリアル</param>
  /// <param name="meshMat">ボロノイ面のマテリアル</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disable">無効化された点のマテリアル</param>
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

  /// <summary>
  /// ドロネー線のマテリアルを設定する．
  /// </summary>
  /// <param name="mat">ドロネー線のマテリアル</param>
  public void SetMaterialDelaunayLine(Material mat)
  {
    this.relationEdgeMaterial = mat;
    foreach (GameObject obj in this.delaunayLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.material = mat;
    }
  }

  /// <summary>
  /// ボロノイ線のマテリアルを設定する．
  /// </summary>
  /// <param name="mat">ボロノイ線のマテリアル</param>
  public void SetMaterialVoronoiLine(Material mat)
  {
    this.edgeMaterial = mat;
    foreach (GameObject obj in this.voronoiLine)
    {
      LineRenderer lRend = obj.GetComponent<LineRenderer>();
      lRend.material = mat;
    }
  }

  /// <summary>
  /// ボロノイ面（出力3Dモデル）のマテリアルを設定する．
  /// </summary>
  /// <param name="mat">ボロノイ面のマテリアル</param>
  public void SetMaterialVoronoiMesh(Material mat)
  {
    this.meshMaterial = mat;
    if (this.voronoiMesh) this.voronoiMesh.GetComponent<MeshRenderer>().material = mat;
  }
  #endregion SetterMaterial

  /* オプションのセッター */
  #region SetterOption
  /// <summary>
  /// 出力3Dモデルのレイヤーを設定する．
  /// </summary>
  /// <param name="layer">出力3Dモデルのレイヤー</param>
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
  #endregion SetterOption

  /* publicメソッド群 */
  #region OtherPublicMethod
  /// <summary>
  /// クラスの状態をリセットする
  /// </summary>
  public void Reset()
  {
    this.IsCreated = false;
    this.DeraunayIsCreated = false;
    this.VoronoiIsCreated = false;
    this.agents = new List<DPPoint>();
    this.DPTetrahedrons = new List<DPTetrahedron>();
    ResetLine();
  }

  /// <summary>
  /// 新しい点群から3Dモデルを再生成する．
  /// </summary>
  /// <param name="agents">点群</param>
  public void Recreate(List<(Agent, ParticleObject)> agents)
  {
    this.agents = DPPoint.ListToVPPoints(agents);
    Recreate();
  }

  /// <summary>
  /// 3Dモデルを再生する．
  /// </summary>
  public void Recreate()
  {
    ResetLine();
    Create();
  }

  /// <summary>
  /// 3Dモデルを生成する．
  /// </summary>
  public void Create()
  {
    var sw = new MyStopwatch();
    string sw_result = "VoronoiPartitionFast - Create\n";
    sw.Start();

    CreateDelaunay();

    sw.Stop();
    sw_result += sw.GetResultString("CreateDelaunay") + "\n";
    sw.Restart();

    CreateVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateVoronoi") + "\n";
    sw.Restart();

    DrawDelaunay();
    DrawVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateLine&Mesh");

    Debug.Log(this + sw_result);

    this.IsCreated = true;
  }

  /// <summary>
  /// 新しい点群から無駄な点を省いた後に点を生成する．
  /// </summary>
  /// <param name="agents"></param>
  public void CreateDelaunayWithThinOutPoints(List<(Agent, ParticleObject)> agents)
  {
    this.agents = DPPoint.ListToVPPoints(agents);
    CreateDelaunayWithThinOutPoints();
  }

  /// <summary>
  /// 無駄な点を省いた後に点を生成する．
  /// </summary>
  public void CreateDelaunayWithThinOutPoints()
  {
    //スタックとレースを表示
    //Debug.Log("Object : " + this.Parent + "\n" + Utils.GetStackTrace());

    ResetLine();
    var sw = new MyStopwatch();
    string sw_result = "VoronoiPartitionFast - CreateDelaunayWithThinOutPoints\n";
    sw.Start();

    ThinOutPoints();

    sw.Stop();
    sw_result += sw.GetResultString("点間引き") + "\n";

    Utils.Swap<List<DPPoint>>(ref this.agents, ref this.thinOutAgents);

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
    DrawVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateLine&Mesh") + "\n";

    Utils.Swap<List<DPPoint>>(ref this.agents, ref this.thinOutAgents);

    Debug.Log(this + sw_result);

    this.IsCreated = true;
  }

  /// <summary>
  /// 点を追加する．（再生成より処理が軽い）
  /// </summary>
  /// <param name="agent">追加する点</param>
  public void AddAgent((Agent, ParticleObject) agent)
  {
    //ビューをリセット
    ResetLine();

    //DPPoint化
    DPPoint p = new DPPoint(agent.Item1, agent.Item2);

    //エージェントを記録
    this.agents.Add(p);

    var sw = new MyStopwatch();
    string sw_result = "VoronoiPartitionFast - CreateDelaunayWithThinOutPoints\n";
    sw.Start();

    //ドロネー点追加
    AddDelaunayPoint(p);

    sw.Stop();
    sw_result += sw.GetResultString("CreateDelaunay") + "\n";
    sw.Restart();

    //ボロノイ生成
    CreateVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateVoronoi") + "\n";
    sw.Restart();

    //ビュー生成
    DrawDelaunay();
    DrawVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateLine&Mesh");

    Debug.Log(this + sw_result);

    this.IsCreated = true;
  }

  /// <summary>
  /// 点群を追加する．（再生成より処理が軽い）
  /// </summary>
  /// <param name="agents">追加する点群</param>
  public void AddAgents(List<(Agent, ParticleObject)> agents)
  {
    //ビューをリセット
    ResetLine();

    //DPPoint化
    List<DPPoint> ps = DPPoint.ListToVPPoints(agents);
    //DPPoint p = new DPPoint(agent.Item1, agent.Item2);

    //エージェントを記録
    this.agents.AddRange(ps);

    var sw = new MyStopwatch();
    string sw_result = "VoronoiPartitionFast - CreateDelaunayWithThinOutPoints\n";
    sw.Start();

    //ドロネー点追加
    foreach(var p in ps) AddDelaunayPoint(p);

    sw.Stop();
    sw_result += sw.GetResultString("CreateDelaunay") + "\n";
    sw.Restart();

    //ボロノイ生成
    CreateVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateVoronoi") + "\n";
    sw.Restart();

    //ビュー生成
    DrawDelaunay();
    DrawVoronoi();

    sw.Stop();
    sw_result += sw.GetResultString("CreateLine&Mesh");

    Debug.Log(this + sw_result);

    this.IsCreated = true;
  }

  /// <summary>
  /// 生成後の後処理をする．
  /// </summary>
  public void Complete()
  {
    //ビューをリセット
    ResetLine();
    CompleteDelaunay();
    CreateVoronoi();
    DrawDelaunay();
    DrawVoronoi();
  }

  /// <summary>
  /// 仮に点を追加した時の，周囲の存在性の異なる点の数を返す．
  /// ドロネー線で繋がった点のみを数える．
  /// </summary>
  /// <param name="agent"></param>
  /// <returns></returns>
  public int CountAroundPoint((Agent, ParticleObject) agent)
  {
    DPPoint point = new DPPoint(agent.Item1, agent.Item2);
    return countAroundPoint(point);
  }

  /// <summary>
  /// 文字列化
  /// </summary>
  /// <returns>インスタンスの情報</returns>
  public override string ToString()
  {
    string info =
      $"Agents : {this.agents.Count}\n" +
      $"thinOutAgents : {this.thinOutAgents.Count}\n" +
      $"DPPoints : {this.DPPoints.Count}\n" +
      $"DPEdges : {this.DPEdges.Count}\n" +
      $"DPFaces : {this.DPFaces.Count}\n" +
      $"DPTetrahedrons : {this.DPTetrahedrons.Count}\n" +
      $"VPEdges : {this.VPEdges.Count}\n" +
      $"VPFaces : {this.VPFaces.Count}\n";

    return info + base.ToString();
  }

  #endregion OtherPublicMethod

  /* ドロネー図生成メソッド群 */
  #region Deraunay
  /// <summary>
  /// ドロネー図を生成する．
  /// </summary>
  private void CreateDelaunay()
  {
    InitializeDelaunay();

    for (int i = 0; i < agents.Count; i++)
    {
      DPPoint select_point = agents[i];

      //ドロネー点追加
      _addDelaunayPoint(select_point);
    }

    CompleteDelaunay();

    this.DeraunayIsCreated = true;
  }

  /// <summary>
  /// ドロネー図を作成する準備をする．
  /// </summary>
  private void InitializeDelaunay()
  {
    //リストを初期化
    this.DPPoints.Clear();
    this.FirstPoints.Clear();
    this.DPEdges.Clear();
    this.DPFaces.Clear();
    this.DPTetrahedrons.Clear();

    this.DeraunayIsCreated = false;

    DPPoint p1 = new DPPoint(new Vector3(0f, 0f, 800f));
    DPPoint p2 = new DPPoint(new Vector3(0f, 1000f, -200f));
    DPPoint p3 = new DPPoint(new Vector3(866.66f, -500f, -200f));
    DPPoint p4 = new DPPoint(new Vector3(-866.66f, -500f, -200f));

    //Debug.Log($"p1 : {p1}, p2 : {p2}, p3 : {p3}, p4 : {p4}");

    DPTetrahedron tetrahedron = new DPTetrahedron(p1, p2, p3, p4);
    FirstPoints.Add(p1);
    FirstPoints.Add(p2);
    FirstPoints.Add(p3);
    FirstPoints.Add(p4);
    DPTetrahedrons.Add(tetrahedron);

    //Debug.Log(tetrahedron);
  }

  /// <summary>
  /// 逐次法でドロネー点を追加する．
  /// </summary>
  /// <param name="select_point">追加する点</param>
  private void AddDelaunayPoint(DPPoint select_point)
  {
    _addDelaunayPoint(select_point);
    CompleteDelaunay();
  }

  /// <summary>
  /// 逐次添加法でドロネー点を追加する．
  /// main algorithm
  /// https://qiita.com/kkttm530/items/d32bad84a6a7f0d8d7e7
  /// </summary>
  /// <param name="select_point"></param>
  private void _addDelaunayPoint(DPPoint select_point)
  {
    this.DPPoints.Add(select_point);

    List<DPTetrahedron> temp_t_list = new List<DPTetrahedron>();
    for (int j = 0; j < DPTetrahedrons.Count; j++)
    {
      DPTetrahedron tri = DPTetrahedrons[j];
      if (tri.CheckPointIncludeCircumsphere(select_point))
      {
        /*var sep_data = tri.Separate(select_point);
        this.DPEdges.AddRange(sep_data.NewEdges);
        this.DPFaces.AddRange(sep_data.NewFaces);
        temp_t_list.AddRange(sep_data.NewTetrahedrons);*/
        temp_t_list.Add(tri);
        DPTetrahedrons.RemoveAt(j--);
      }
    }
    var sep_data = DPTetrahedron.Separate(temp_t_list, select_point);
    this.DPEdges.AddRange(sep_data.NewEdges);
    this.DPFaces.AddRange(sep_data.NewFaces);
    DPTetrahedrons.AddRange(sep_data.NewTetrahedrons);
    /*    var del_insts = new List<DPTetrahedron>[temp_t_list.Count];
        Parallel.For(0, temp_t_list.Count, k =>
        {
          DPTetrahedron tetra = temp_t_list[k];
          del_insts[k] = new List<DPTetrahedron>();
          foreach (DPTetrahedron tetra_check in temp_t_list)
          {
            if (ReferenceEquals(tetra, tetra_check)) continue;
            if (tetra.Radius == tetra_check.Radius && tetra.Center == tetra_check.Center)
              del_insts[k].Add(tetra_check);
          }
        });
        for (var k = 0; k < del_insts.Length; k++)
        {
          foreach (DPTetrahedron del_inst in del_insts[k])
          {
            int index = temp_t_list.IndexOf(del_inst);
            if (index != -1)
            {
              temp_t_list[index].Enabled = false;
              temp_t_list.RemoveAt(index);
            }
          }
        }
    DPTetrahedrons.AddRange(temp_t_list);
    */
  }

  /// <summary>
  /// ドロネー図生成の後処理を行う．
  /// </summary>
  private void CompleteDelaunay()
  {
    //最初の点を含む四面体を無効にする
    foreach (var p in this.FirstPoints)
      foreach (var t in p.RelTetrahedrons)
        t.Enabled = false;

    /*foreach (var t in DPTetrahedron.GetEnabledDPTetrahedrons(this.DPTetrahedrons))
      Utils.CreateTetrahedron(t);*/
    
    //無効な辺・面を削除
    List<DPEdge> nes = new List<DPEdge>();
    List<DPFace> nfs = new List<DPFace>();
    foreach(var e in this.DPEdges)
    {
      //Debug.Log(e.RelTetraCounter);
      if(e.RelTetraCounter > 0)
      {
        nes.Add(e);
      }
    }
    foreach(var f in this.DPFaces)
    {
      if(f.RelTetraCounter > 0)
      {
        nfs.Add(f);
      }
    }
    this.DPEdges.Clear();
    this.DPFaces.Clear();
    this.DPEdges = nes;
    this.DPFaces = nfs;
  }

  private int countAroundPoint(DPPoint point)
  {
    List<DPTetrahedron> temp_t_list = new List<DPTetrahedron>();
    for (int j = 0; j < DPTetrahedrons.Count; j++)
    {
      DPTetrahedron tri = DPTetrahedrons[j];
      if (tri.CheckPointIncludeCircumsphere(point))
      {
        temp_t_list.Add(tri);
      }
    }
    //隣接点を見つける
    HashSet<DPPoint> points = new HashSet<DPPoint>();
    //共通している面以外の面を四面体情報と共に保持
    List<DPFace> exFaces = new List<DPFace>();
    foreach (var t in temp_t_list)
    {
      for (int i = 0; i < t.Faces.Length; i++)
      {
        //exFacesの中にすでに同じ面がないかチェック
        int index = -1;
        for (int j = 0; j < exFaces.Count; j++)
        {
          //同じ面があったら削除
          if (t.Faces[i] == exFaces[j])
          {
            index = j;
            exFaces.RemoveAt(j);
            break;
          }
        }

        //なかった場合
        if (index == -1)
        {
          exFaces.Add(t.Faces[i]);
        }
      }
    }
    foreach(var f in exFaces)
    {
      foreach(var p in f.Points)
      {
        points.Add(p);
      }
    }
    int count = 0;
    foreach(var p in points)
    {
      if(((SwarmAgent)p.Agent).is_exist != ((SwarmAgent)point.Agent).is_exist)
      {
        count++;
      }
    }

    return count;
  }
  #endregion Deraunay

  /* ボロノイ図生成メソッド群 */
  #region Voronoi
  /// <summary>
  /// ボロノイ図を生成する．
  /// </summary>
  private void CreateVoronoi()
  {
    InitializeVoronoi();
    //ボロノイ辺を生成
    foreach(var dpf in this.DPFaces)
    {
      dpf.CreateVP();
      if (dpf.VPEdge != null) this.VPEdges.Add(dpf.VPEdge);
    }
    //ボロノイ面を生成
    for(int i = 0; i < this.DPEdges.Count; i++)
    {
      //存在点と非存在点の間にのみ面を生成
      SwarmAgent agent1 = (SwarmAgent)this.DPEdges[i].Points[0].Agent;
      SwarmAgent agent2 = (SwarmAgent)this.DPEdges[i].Points[1].Agent;
      if (agent1.is_exist == agent2.is_exist) continue;
      //面を生成
      this.DPEdges[i].CreateVP();
      if(this.DPEdges[i].VPFace != null) this.VPFaces.Add(this.DPEdges[i].VPFace);
    }
    this.VoronoiIsCreated = true;
  }

  /// <summary>
  /// ボロノイ図生成前の初期化処理を行う．
  /// </summary>
  private void InitializeVoronoi()
  {
    this.VoronoiIsCreated = false;
    this.VPEdges.Clear();
    this.VPFaces.Clear();
  }

  #endregion Voronoi

  /* VP結合メソッド */
  #region VPJoinMethod
  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．
  /// 方法１
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  public void JoinVoronoiPartition(VoronoiPartitionFast vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition2(VoronoiPartitionFast vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition3(VoronoiPartitionFast vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4(VoronoiPartitionFast vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }

  //[System.Obsolete]
  public void JoinVoronoiPartition4_parallel(VoronoiPartitionFast vp)
  {
    JoinVoronoiPartition(vp, this.ExistentialParticleMaterial, this.NonExistentialParticleMaterial, this.DisabledParticleMaterial);
  }


  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．
  /// 方法１
  ///   もう一方の四面体内に点が含まれている場合，その点を無効化する
  ///   存在点はそのまま追加する．
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disabledMat">無効化された点のマテリアル</param>
  //[System.Obsolete]
  public void JoinVoronoiPartition(VoronoiPartitionFast vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<DPPoint> agents = new List<DPPoint>();
    this.ThinOutPoints();
    Utils.Swap(ref this.agents, ref this.thinOutAgents);
    this.CreateDelaunay();
    Utils.Swap(ref this.agents, ref this.thinOutAgents);
    vp.ThinOutPoints();
    Utils.Swap(ref vp.agents, ref vp.thinOutAgents);
    vp.CreateDelaunay();
    Utils.Swap(ref vp.agents, ref vp.thinOutAgents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in this.agents)
    {
      if (!vp.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    this.agents = agents;
  }

  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．
  /// 方法２
  ///   両方の点をそのまま追加する．
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disabledMat">無効化された点のマテリアル</param>
  public void JoinVoronoiPartition2(VoronoiPartitionFast vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<DPPoint> agents = new List<DPPoint>();
    foreach (var agent in vp.agents)
    {
      agents.Add(agent);
      if (((SwarmAgent)agent.Agent).is_exist)
      {
        agent.ParticleObject.SetMaterial(existential);
      }
      else
      {
        agent.ParticleObject.SetMaterial(nonexistential);
      }
    }
    foreach (var agent in this.agents)
    {
      agents.Add(agent);
      if (((SwarmAgent)agent.Agent).is_exist)
      {
        agent.ParticleObject.SetMaterial(existential);
      }
      else
      {
        agent.ParticleObject.SetMaterial(nonexistential);
      }
    }
    this.agents = agents;
  }

  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．
  /// 方法３
  ///   もう一方の外接円内に点が含まれている場合，その点を無効化する
  ///   存在点はそのまま追加する．
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disabledMat">無効化された点のマテリアル</param>
  //[System.Obsolete]
  public void JoinVoronoiPartition3(VoronoiPartitionFast vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<DPPoint> agents = new List<DPPoint>();
    List<DPPoint> temp = new List<DPPoint>(this.agents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInCircumsphere(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in temp)
    {
      if (!vp.IsIncludedInCircumsphere(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    this.DPTetrahedrons.AddRange(vp.DPTetrahedrons);
    this.agents = agents;
  }

  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．
  /// 方法４（１より高速化）
  ///   もう一方の四面体内に点が含まれている場合，その点を無効化する
  ///   存在点はそのまま追加する．
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disabledMat">無効化された点のマテリアル</param>
  //[System.Obsolete]
  public void JoinVoronoiPartition4(VoronoiPartitionFast vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<DPPoint> agents = new List<DPPoint> ();
    List<DPPoint> temp = new List<DPPoint>(this.agents);
    foreach (var agent in vp.agents)
    {
      if (!this.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    foreach (var agent in temp)
    {
      if (!vp.IsIncludedInTetrahedron(agent) || ((SwarmAgent)agent.Agent).is_exist)
      {
        agents.Add(agent);
        if (((SwarmAgent)agent.Agent).is_exist)
        {
          agent.ParticleObject.SetMaterial(existential);
        }
        else
        {
          agent.ParticleObject.SetMaterial(nonexistential);
        }
      }
      else
      {
        agent.ParticleObject.SetMaterial(disabledMat);
      }
    }
    this.DPTetrahedrons.AddRange(vp.DPTetrahedrons);
    this.agents = agents;
  }

  /// <summary>
  /// 別のボロノイ図をこのボロノイ図に結合する．並列化して高速化
  /// 方法４と同じ
  ///   もう一方の四面体内に点が含まれている場合，その点を無効化する
  ///   存在点はそのまま追加する．
  /// </summary>
  /// <param name="vp">別のボロノイ図</param>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  /// <param name="disabledMat">無効化された点のマテリアル</param>
  //[System.Obsolete]
  public void JoinVoronoiPartition4_parallel(VoronoiPartitionFast vp, Material existential, Material nonexistential, Material disabledMat)
  {
    List<DPPoint> existential_agents = new List<DPPoint>();
    List<DPPoint> nonexistential_agents = new List<DPPoint>();
    List<DPPoint> temp = new List<DPPoint>(this.agents);
    Parallel.ForEach(vp.agents, agent =>
    {
      if (((SwarmAgent)agent.Agent).is_exist || !this.IsIncludedInTetrahedron(agent))
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
      if (((SwarmAgent)agent.Agent).is_exist || !vp.IsIncludedInTetrahedron(agent))
      {
        lock (existential_agents) existential_agents.Add(agent);
      }
      else
      {
        lock (nonexistential_agents) nonexistential_agents.Add(agent);
      }
    });
    this.DPTetrahedrons.AddRange(vp.DPTetrahedrons);
    this.agents = existential_agents;
    foreach (var agent in existential_agents)
    {
      Debug.Log(agent);
      if (((SwarmAgent)agent.Agent).is_exist)
      {
        agent.ParticleObject.SetMaterial(existential);
      }
      else
      {
        agent.ParticleObject.SetMaterial(nonexistential);
      }
    }
    foreach (var agent in nonexistential_agents)
    {
      agent.ParticleObject.SetMaterial(disabledMat);
    }
  }

  /// <summary>
  /// 四面体の中に点が含まれているかどうか調べる．
  /// </summary>
  /// <param name="agent">指定の点</param>
  /// <returns>四面体の中に点が含まれているかどうか</returns>
  public bool IsIncludedInTetrahedron(DPPoint agent)
  {
    foreach (var tetra in this.DPTetrahedrons)
    {
      if (tetra.CheckPointIncludeTetrahedron(agent))
      {
        return true;
      }
    }
    return false;
  }

  /// <summary>
  /// 四面体の外接球内の中に点が含まれているかどうか調べる．
  /// </summary>
  /// <param name="agent">指定の点</param>
  /// <returns>四面体の外接球内の中に点が含まれているかどうか</returns>
  public bool IsIncludedInCircumsphere(DPPoint agent)
  {
    foreach (var tetra in this.DPTetrahedrons)
    {
      if (tetra.CheckPointIncludeCircumsphere(agent))
      {
        return true;
      }
    }
    return false;
  }
  #endregion VPJoinMethod

  /* その他 privateメソッド群 */
  #region OtherPrivateMethod
  /// <summary>
  /// ドロネー線のゲームオブジェクトを生成する．
  /// </summary>
  private void DrawDelaunay()
  {
    if (this.isVisibleDelaunayLine)
    {
      foreach (var edge in this.DPEdges)
      {
        DrawDelaunayLine(edge);
      }
    }
  }

  /// <summary>
  /// ドロネー線を引く
  /// </summary>
  /// <param name="edge"></param>
  private void DrawDelaunayLine(DPEdge edge)
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
      lRend.positionCount = 2;
      lRend.startWidth = 0.01f;
      lRend.endWidth = 0.01f;
      SetEdgeParent(line);
    }
    lRend.enabled = this.isVisibleDelaunayLine;
    lRend.SetPosition(0, edge.Points[0].vec);
    lRend.SetPosition(1, edge.Points[1].vec);

    this.delaunayLine_index++;
  }

  /// <summary>
  /// ボロノイ線とボロノイ面のゲームオブジェクトを生成する．
  /// </summary>
  private void DrawVoronoi()
  {
    if (this.isVisibleVoronoiLine)
    {
      //線を描画
      foreach (var e in this.VPEdges)
      {
        DrawVoronoiLine(e);
      }
    }
    if (this.isVisibleVoronoiMesh)
    {
      //面を描画
      CreateVoronoiGameObject(this.VPFaces);
    }
  }

  /// <summary>
  /// ボロノイ線のゲームオブジェクトを生成する．
  /// </summary>
  /// <param name="edge"></param>
  private void DrawVoronoiLine(VPEdge edge)
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
      lRend.positionCount = 2;
      lRend.startWidth = 0.01f;
      lRend.endWidth = 0.01f;
      SetMeshParent(line);
    }
    lRend.enabled = this.isVisibleVoronoiLine;
    lRend.SetPosition(0, edge.Points[0].Point);
    lRend.SetPosition(1, edge.Points[1].Point);

    this.voronoiLine_index++;
  }

  /// <summary>
  /// 線のゲームオブジェクトをリセットする．
  /// </summary>
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

  /// <summary>
  /// ボロノイ面のゲームオブジェクトを生成する．
  /// </summary>
  /// <param name="faces"></param>
  private void CreateVoronoiGameObject(List<VPFace> faces)
  {
    //Debug.Log("CreateVoronoiGameObject");
    if (voronoiMesh == null)
    {
      this.voronoiMesh = new GameObject("VoronoiMesh");
      this.voronoiMesh.AddComponent<MeshRenderer>();
      this.voronoiMesh.AddComponent<MeshFilter>();
    }
    MonoBehaviour.Destroy(this.voronoiMesh.GetComponent<MeshFilter>().sharedMesh);
    this.voronoiMesh.GetComponent<MeshFilter>().sharedMesh = CreateVoronoiMesh(faces);
    this.voronoiMesh.GetComponent<MeshRenderer>().material = this.meshMaterial;
    this.voronoiMesh.SetActive(this.isVisibleVoronoiMesh);
    Utils.SetParent(this.Parent, this.voronoiMesh);
  }

  /// <summary>
  /// ボロノイ面をメッシュ化する．
  /// 必要な面（存在点と非存在点の境界の面）のみメッシュ化する．
  /// </summary>
  /// <param name="faces">ボロノイ面の集合</param>
  /// <returns>メッシュ化したボロノイ面</returns>
  private Mesh CreateVoronoiMesh(List<VPFace> faces)
  {
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    foreach(var face in faces)
    {
      //点を格納
      foreach(var point in face.Points)
      {
        point.VertexIndex = vertices.Count; //頂点インデックスを格納
        vertices.Add(point.Point);
      }

      //面を設定
      VPPoint center = face.Points[0];  //この点を基準にポリゴンを生成
      DPPoint front = face.RelDPEdge.Points[0]; //この点が正面となる（is_existがtrueの時は背面）
      foreach(var edge in face.Edges)
      {
        var p1 = edge.Points[0];
        var p2 = edge.Points[1];

        //centerを含む辺は飛ばす
        if (p1 == center || p2 == center) continue;

        //面の向きを計算
        if(Utils.JudgeFrontOrBack(center.Point, p1.Point, p2.Point, front.vec) == ((SwarmAgent)front.Agent).is_exist)
        {
          triangles.Add(center.VertexIndex);
          triangles.Add(p1.VertexIndex);
          triangles.Add(p2.VertexIndex);
        }
        else
        {
          triangles.Add(center.VertexIndex);
          triangles.Add(p2.VertexIndex);
          triangles.Add(p1.VertexIndex);
        }
      }
    }

    //メッシュオブジェクト作成
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    vertices.Clear();
    triangles.Clear();

    return mesh;
  }

  /// <summary>
  /// 必要な点（存在点と非存在点が相互に関係している点）以外を除いて，thinOutAgentsに格納する．
  /// </summary>
  private void ThinOutPoints()
  {
    //ドロネー分析
    CreateDelaunay();

    //必要な点を確認
    foreach(var e in this.DPEdges)
    {
      var agent1 = (SwarmAgent)e.Points[0].Agent;
      var agent2 = (SwarmAgent)e.Points[1].Agent;
      //存在点と非存在点が相互に関係している点のみ残す
      if(agent1.is_exist != agent2.is_exist)
      {
        e.Points[0].isNeeded = true;
        e.Points[1].isNeeded = true;
      }
    }

    //必要な点を保持
    this.thinOutAgents = new List<DPPoint>();
    foreach(var p in this.DPPoints)
    {
      if (p.isNeeded) this.thinOutAgents.Add(p);
    }
  }

  #endregion OtherPrivateMethod
}

#region DeraunayPartitionDataClass
/// <summary>
/// ドロネー点のクラス．
/// </summary>
public class DPPoint
{
  /* データ */
  public Agent Agent;
  public ParticleObject ParticleObject;

  /* 関係 */
  public int RelTetraCounter;
  public List<DPEdge> RelEdges;
  public List<DPFace> RelFaces;
  public List<DPTetrahedron> RelTetrahedrons;
  public List<DPTetrahedron> RelEnabledTetrahedrons
  {
    get
    {
      return DPTetrahedron.GetEnabledDPTetrahedrons(RelTetrahedrons);
    }
  }

  //点を減らすときに使用
  public bool isNeeded;

  public DPPoint()
  {
    this.RelTetraCounter = 0;
    this.RelEdges = new List<DPEdge>();
    this.RelFaces = new List<DPFace>();
    this.RelTetrahedrons = new List<DPTetrahedron>();
    this.isNeeded = false;
  }

  public DPPoint(Agent agent, ParticleObject pObj) : this()
  {
    this.Agent = agent;
    this.ParticleObject = pObj;
  }

  public DPPoint(Vector3 pos) : this()
  {
    this.Agent = new SwarmAgent();
    this.Agent.Position = pos;
  }

  public Vector3 vec
  {
    set { this.Agent.Position = value; }
    get { return this.Agent.Position; }
  }

  public float x
  {
    set { this.Agent.Position.x = value; }
    get { return this.Agent.Position.x; }
  }

  public float y
  {
    set { this.Agent.Position.y = value; }
    get { return this.Agent.Position.y; }
  }

  public float z
  {
    set { this.Agent.Position.z = value; }
    get { return this.Agent.Position.z; }
  }

  /// <summary>
  /// DPEdge，DPFace，DPTetrahedronを参照に持つかを確認する
  /// </summary>
  /// <param name="target">検索対象</param>
  /// <returns>参照に持つか</returns>
  public bool Have(object target)
  {
    IEnumerable array;
    if(target.GetType() == typeof(DPEdge))
      array = this.RelEdges;
    else if(target.GetType() == typeof(DPFace))
      array = this.RelFaces;
    else if(target.GetType() == typeof(DPTetrahedron))
      array = this.RelTetrahedrons;
    else
      return false;

    foreach (var elem in array)
      if (elem == target) return true;
    return false;
  }

  /// <summary>
  /// 指定された点に関連するボロノイ点を返す．
  /// </summary>
  /// <param name="agents">点</param>
  /// <returns>関連するボロノイ点のリスト</returns>
  public static List<DPPoint> ListToVPPoints(List<(Agent, ParticleObject)> agents)
  {
    List<DPPoint> sets = new List<DPPoint>();

    foreach(var agent in agents)
    {
      sets.Add(new DPPoint(agent.Item1, agent.Item2));
    }

    return sets;
  }
}

/// <summary>
/// ドロネー辺のクラス
/// </summary>
public class DPEdge
{
  /* データ */
  public DPPoint[] Points;

  /* 関連 */
  public int RelTetraCounter;
  public List<DPFace> RelFaces;
  public List<DPTetrahedron> RelTetrahedrons;
  public List<DPTetrahedron> RelEnabledTetrahedrons
  {
    get
    {
      return DPTetrahedron.GetEnabledDPTetrahedrons(RelTetrahedrons);
    }
  }

  /* VPの関連 */
  public VPFace VPFace;

  public DPEdge()
  {
    this.RelTetraCounter = 0;
    this.RelFaces = new List<DPFace>();
    this.RelTetrahedrons = new List<DPTetrahedron>();
  }

  public DPEdge(DPPoint p1, DPPoint p2) : this()
  {
    this.Points = new DPPoint[2];

    this.Points[0] = p1;
    this.Points[1] = p2;

    //関連を登録
    foreach (var p in this.Points) p.RelEdges.Add(this);
  }

  /// <summary>
  /// DPEdge，DPFace，DPTetrahedronを参照に持つかを確認する
  /// </summary>
  /// <param name="target"></param>
  /// <returns></returns>
  public bool Have(object target)
  {
    IEnumerable array;
    if (target.GetType() == typeof(DPPoint))
      array = this.Points;
    else if (target.GetType() == typeof(DPFace))
      array = this.RelFaces;
    else if (target.GetType() == typeof(DPTetrahedron))
      array = this.RelTetrahedrons;
    else
      return false;

    foreach (var elem in array)
      if (elem == target) return true;
    return false;
  }

  /* VP関連 */
  /// <summary>
  /// ボロノイ面を生成する．
  /// </summary>
  public void CreateVP()
  {
    //未生成なら生成
    if(this.VPFace == null) this.VPFace = VPFace.Create(this);
  }
}

/// <summary>
/// ドロネー面のクラス
/// </summary>
public class DPFace
{
  /* データ */
  public DPEdge[] Edges;

  /* 関連 */
  public DPPoint[] Points;
  /*  private int _relTetraCounter;
    public int RelTetraCounter {
      get { return _relTetraCounter; }
      set {
        _relTetraCounter = value;
        //もし関係している四面体が二つ以下になったらボロノイ辺を削除
        if (value < 2) this.VPEdge = null;
      }
    } //参照中の四面体がいくつあるか保持*/
  public int RelTetraCounter;
  public List<DPTetrahedron> RelTetrahedrons;
  public List<DPTetrahedron> RelEnabledTetrahedrons
  {
    get
    {
      return DPTetrahedron.GetEnabledDPTetrahedrons(RelTetrahedrons);
    }
  }

  /* VPとの関連 */
  public VPEdge VPEdge;

  public DPFace()
  {
    this.Edges = new DPEdge[3];
    this.Points = new DPPoint[3];
    this.RelTetraCounter = 0;
    this.RelTetrahedrons = new List<DPTetrahedron>();
  }

  public DPFace(DPEdge e1, DPEdge e2, DPEdge e3) : this()
  {
    this.Edges[0] = e1;
    this.Edges[1] = e2;
    this.Edges[2] = e3;

    //点を解析
    this.Points[0] = DPTetrahedron.GetPointSharedBetweenEdges(e1, e2);
    this.Points[1] = DPTetrahedron.GetPointSharedBetweenEdges(e2, e3);
    this.Points[2] = DPTetrahedron.GetPointSharedBetweenEdges(e3, e1);

    //関連を登録
    foreach (var e in this.Edges)
    {
      e.RelFaces.Add(this);
    }
    foreach(var p in this.Points)
    {
      p.RelFaces.Add(this);
    }
  }

  /// <summary>
  /// DPEdge，DPFace，DPTetrahedronを参照に持つかを確認する
  /// </summary>
  /// <param name="target"></param>
  /// <returns></returns>
  public bool Have(object target)
  {
    IEnumerable array;
    if (target.GetType() == typeof(DPEdge))
      array = this.Edges;
    else if (target.GetType() == typeof(DPTetrahedron))
      array = this.RelTetrahedrons;
    else
      return false;

    foreach (var elem in array)
      if (elem == target) return true;
    return false;
  }

  /* VP関連 */
  /// <summary>
  /// ボロノイ辺を生成する．
  /// </summary>
  public void CreateVP()
  {
    //未生成なら生成
    if(this.VPEdge == null) this.VPEdge = VPEdge.Create(this);
  }
}

/// <summary>
/// ドロネー四面体のクラス
/// </summary>
public class DPTetrahedron
{
  /* データ */
  public DPFace[] Faces;

  public bool _enabled;
  /// <summary>
  /// 削除された場合はEnabledをFalseにする．
  /// </summary>
  public bool Enabled
  {
    get { return _enabled; }
    set {
      //有効→無効
      if(_enabled && !value)
      {
        //関連を登録 & 関連を保存
        foreach (var f in this.Faces)
        {
          f.RelTetraCounter--;
          f.VPEdge = null;
        }
        foreach (var e in this.Edges)
        {
          e.RelTetraCounter--;
          e.VPFace = null;
        }
        foreach (var p in this.Points)
          p.RelTetraCounter--;
      }
      //無効→有効
      else if(!_enabled && value)
      {
        foreach (var f in this.Faces)
        {
          f.RelTetraCounter++;
          f.VPEdge = null;
        }
        foreach (var e in this.Edges)
        {
          e.RelTetraCounter++;
          e.VPFace = null;
        }
        foreach (var p in this.Points)
          p.RelTetraCounter++;
      }
      _enabled = value;
    }
  }

  /* 関連 */
  public DPPoint[] Points;
  public DPEdge[] Edges;

  //プロパティ
  public Vector3 Center { get; private set; }
  public float Radius { get; private set; }

  /* VP関連 */
  public VPPoint VPPoint;

  public DPTetrahedron()
  {
    this._enabled = false;  //初期設定（一応）

    this.Points = new DPPoint[4];
    this.Edges = new DPEdge[6];
    this.Faces = new DPFace[4];
  }

  public DPTetrahedron(DPFace f1, DPFace f2, DPFace f3, DPFace f4) : this()
  {
    this.Faces[0] = f1;
    this.Faces[1] = f2;
    this.Faces[2] = f3;
    this.Faces[3] = f4;

    //各辺を見つける
    this.Edges[0] = GetEdgeSharedBetweenFaces(this.Faces[0], this.Faces[1]);
    this.Edges[1] = GetEdgeSharedBetweenFaces(this.Faces[0], this.Faces[2]);
    this.Edges[2] = GetEdgeSharedBetweenFaces(this.Faces[1], this.Faces[2]);
    this.Edges[3] = GetEdgeSharedBetweenFaces(this.Faces[0], this.Faces[3]);
    this.Edges[4] = GetEdgeSharedBetweenFaces(this.Faces[1], this.Faces[3]);
    this.Edges[5] = GetEdgeSharedBetweenFaces(this.Faces[2], this.Faces[3]);

    //各点を見つける
    this.Points[0] = GetPointSharedBetweenEdges(this.Edges[0], this.Edges[1]);
    this.Points[1] = GetPointSharedBetweenEdges(this.Edges[0], this.Edges[3]);
    this.Points[2] = GetPointSharedBetweenEdges(this.Edges[1], this.Edges[3]);
    this.Points[3] = GetPointSharedBetweenEdges(this.Edges[2], this.Edges[4]);

    //データチェック
    foreach (var e in this.Edges) if (e == null) Debug.LogError($"Edge was not found.\n{this}");
    foreach (var p in this.Points) if (p == null) Debug.LogError($"Point was not found.\n{this}");

    //関連を登録
    foreach (var f in this.Faces)
      f.RelTetrahedrons.Add(this);
    foreach (var e in this.Edges)
      e.RelTetrahedrons.Add(this);
    foreach (var p in this.Points)
      p.RelTetrahedrons.Add(this);
    this.Enabled = true;

    //四面体の外接球の中心と半径を計算
    CalcCenterAndRadius();
  }

  public DPTetrahedron(
    DPPoint p1, DPPoint p2, DPPoint p3, DPPoint p4,
    DPEdge e1, DPEdge e2, DPEdge e3, DPEdge e4, DPEdge e5, DPEdge e6,
    DPFace f1, DPFace f2, DPFace f3, DPFace f4
  ) : this()
  {
    this.Points[0] = p1;
    this.Points[1] = p2;
    this.Points[2] = p3;
    this.Points[3] = p4;

    this.Edges[0] = e1;
    this.Edges[1] = e2;
    this.Edges[2] = e3;
    this.Edges[3] = e4;
    this.Edges[4] = e5;
    this.Edges[5] = e6;

    this.Faces[0] = f1;
    this.Faces[1] = f2;
    this.Faces[2] = f3;
    this.Faces[3] = f4;

    //関連を登録
    foreach (var f in this.Faces)
      f.RelTetrahedrons.Add(this);
    foreach (var e in this.Edges)
      e.RelTetrahedrons.Add(this);
    foreach (var p in this.Points)
      p.RelTetrahedrons.Add(this);
    this.Enabled = true;

    //四面体の外接球の中心と半径を計算
    CalcCenterAndRadius();
  }

  public DPTetrahedron(DPPoint p1, DPPoint p2, DPPoint p3, DPPoint p4) : this()
  {
    this.Points[0] = p1;
    this.Points[1] = p2;
    this.Points[2] = p3;
    this.Points[3] = p4;

    this.Edges[0] = new DPEdge(p1, p2);
    this.Edges[1] = new DPEdge(p1, p3);
    this.Edges[2] = new DPEdge(p1, p4);
    this.Edges[3] = new DPEdge(p2, p3);
    this.Edges[4] = new DPEdge(p2, p4);
    this.Edges[5] = new DPEdge(p3, p4);

    this.Faces[0] = new DPFace(this.Edges[0], this.Edges[1], this.Edges[3]);
    this.Faces[1] = new DPFace(this.Edges[0], this.Edges[4], this.Edges[2]);
    this.Faces[2] = new DPFace(this.Edges[1], this.Edges[2], this.Edges[5]);
    this.Faces[3] = new DPFace(this.Edges[3], this.Edges[5], this.Edges[4]);

    //関連を登録
    foreach (var f in this.Faces)
      f.RelTetrahedrons.Add(this);
    foreach (var e in this.Edges)
      e.RelTetrahedrons.Add(this);
    foreach (var p in this.Points)
      p.RelTetrahedrons.Add(this);
    this.Enabled = true;

    //四面体の外接球の中心と半径を計算
    CalcCenterAndRadius();
  }

  /// <summary>
  /// 四面体を点で分割する．
  /// </summary>
  /// <param name="p">新しい点</param>
  /// <returns>分割情報</returns>
  public TetrahedronPartitionData Separate(DPPoint p)
  {
    DPEdge[] newEdges = new DPEdge[4];
    DPFace[] newFaces = new DPFace[6];
    DPTetrahedron[] newTetrahedrons = new DPTetrahedron[4];

    //新しい辺を生成
    newEdges[0] = new DPEdge(p, this.Points[1]);
    newEdges[1] = new DPEdge(p, this.Points[2]);
    newEdges[2] = new DPEdge(p, this.Points[0]);
    newEdges[3] = new DPEdge(p, this.Points[3]);

    //新しい面を生成
    newFaces[0] = new DPFace(newEdges[0], newEdges[1], this.Edges[3]);
    newFaces[1] = new DPFace(newEdges[0], newEdges[2], this.Edges[0]);
    newFaces[2] = new DPFace(newEdges[1], newEdges[2], this.Edges[1]);
    newFaces[3] = new DPFace(newEdges[2], newEdges[3], this.Edges[2]);
    newFaces[4] = new DPFace(newEdges[1], newEdges[3], this.Edges[5]);
    newFaces[5] = new DPFace(newEdges[0], newEdges[3], this.Edges[4]);

    //新しい四面体を生成
    newTetrahedrons[0] = new DPTetrahedron(
      p, this.Points[2], this.Points[1], this.Points[0],
      newEdges[1], newEdges[0], newEdges[2], this.Edges[3], this.Edges[1], this.Edges[0],
      newFaces[0], newFaces[2], newFaces[1], this.Faces[0]
    );
    newTetrahedrons[1] = new DPTetrahedron(
      p, this.Points[0], this.Points[1], this.Points[3],
      newEdges[2], newEdges[0], newEdges[3], this.Edges[0], this.Edges[2], this.Edges[4],
      newFaces[1], newFaces[3], newFaces[5], this.Faces[1]
    );
    newTetrahedrons[2] = new DPTetrahedron(
      p, this.Points[2], this.Points[0], this.Points[3],
      newEdges[1], newEdges[2], newEdges[3], this.Edges[1], this.Edges[5], this.Edges[2],
      newFaces[2], newFaces[4], newFaces[3], this.Faces[2]
    );
    newTetrahedrons[3] = new DPTetrahedron(
      p, this.Points[1], this.Points[2], this.Points[3],
      newEdges[0], newEdges[1], newEdges[3], this.Edges[3], this.Edges[4], this.Edges[5],
      newFaces[0], newFaces[5], newFaces[4], this.Faces[3]
    );

    //新規分割データを準備
    TetrahedronPartitionData sep_data = new TetrahedronPartitionData();
    sep_data.ParentTetrahedron = new List<DPTetrahedron>();
    sep_data.ParentTetrahedron.Add(this);
    sep_data.NewEdges = new List<DPEdge>(newEdges);
    sep_data.NewFaces = new List<DPFace>(newFaces);
    sep_data.NewTetrahedrons = new List<DPTetrahedron>(newTetrahedrons);

    //分割成功
    this.Enabled = false;

    return sep_data;
  }

  /// <summary>
  /// 外接球内に点を含む四面体を同時に分割する．
  /// </summary>
  /// <param name="tetras">点pを外接球内に含む四面体のリスト</param>
  /// <param name="p">新しい点</param>
  /// <returns>分割情報</returns>
  public static TetrahedronPartitionData Separate(List<DPTetrahedron> tetras, DPPoint p)
  {
    //共通している面以外の面を四面体情報と共に保持
    List<(DPTetrahedron, int)> exFaces = new List<(DPTetrahedron, int)>();
    foreach (var t in tetras)
    {
      for (int i = 0; i < t.Faces.Length; i++)
      {
        //exFacesの中にすでに同じ面がないかチェック
        int index = -1;
        for (int j = 0; j < exFaces.Count; j++)
        {
          //同じ面があったら削除
          if (t.Faces[i] == exFaces[j].Item1.Faces[exFaces[j].Item2])
          {
            index = j;
            exFaces.RemoveAt(j);
            break;
          }
        }

        //なかった場合
        if (index == -1)
        {
          exFaces.Add((t, i));
        }
      }
    }

    //各面から点に向かって四面体を生成（共通辺や共通面なども考慮）
    Dictionary<DPPoint, DPEdge> newEdges = new Dictionary<DPPoint, DPEdge>(); //新しい辺
    Dictionary<DPEdge, DPFace> newFaces = new Dictionary<DPEdge, DPFace>(); //新しい面
    List<DPTetrahedron> newTetrahedrons = new List<DPTetrahedron>();

    //各面から点に向けて四面体を生成
    (int[], int[])[] tetra_config = new (int[], int[])[]
    {
      //辺のインデックス，点のインデックス
      (new int[]{ 0, 3, 1 }, new int[]{ 0, 1, 2 }),
      (new int[]{ 2, 4, 0 }, new int[]{ 0, 3, 1 }),
      (new int[]{ 1, 5, 2 }, new int[]{ 0, 2, 3 }),
      (new int[]{ 4, 5, 3 }, new int[]{ 1, 3, 2 }),
    };
    foreach (var f_ti in exFaces)
    {
      //面情報
      DPTetrahedron t = f_ti.Item1;
      DPFace f = f_ti.Item1.Faces[f_ti.Item2];

      //点のループと辺のループを見つける
      DPPoint[] pointLoop = new DPPoint[3];
      DPEdge[] edgeLoop = new DPEdge[3];

      int[] pIndices = tetra_config[f_ti.Item2].Item2;
      int[] eIndices = tetra_config[f_ti.Item2].Item1;
      for (int i = 0; i < 3; i++)
      {
        pointLoop[i] = t.Points[pIndices[i]];
        edgeLoop[i] = t.Edges[eIndices[i]];
      }

      //面からみてターゲット点は表にあるはず（反対ならループを逆にする）
      if (!Utils.JudgeFrontOrBack(pointLoop[0].vec, pointLoop[1].vec, pointLoop[2].vec, p.vec))
      {
        //Debug.LogError("なんと追加点が面の後ろ側にあります．" + f_ti.Item1 + "" + p.vec);
        Utils.Swap(ref pointLoop[0], ref pointLoop[2]);
        Utils.Swap(ref edgeLoop[0], ref edgeLoop[1]);
      }

      //新しい辺と面を用意
      DPEdge[] nEdges = new DPEdge[3];
      DPFace[] nFaces = new DPFace[3];
      for (int i = 0; i < nEdges.Length; i++)
      {
        //辺がない時，辺を生成
        if (!newEdges.ContainsKey(pointLoop[i]))
        {
          newEdges[pointLoop[i]] = new DPEdge(pointLoop[i], p);
        }
        nEdges[i] = newEdges[pointLoop[i]];
      }
      for (int i = 0; i < nFaces.Length; i++)
      {
        //面がない時，面を生成
        if (!newFaces.ContainsKey(edgeLoop[i]))
        {
          newFaces[edgeLoop[i]] = new DPFace(nEdges[i], nEdges[(i + 1) % nEdges.Length], edgeLoop[i]);
        }
        nFaces[i] = newFaces[edgeLoop[i]];
      }

      //四面体を生成
      DPTetrahedron nTetra = new DPTetrahedron(
        p, pointLoop[0], pointLoop[2], pointLoop[1],
        nEdges[0], nEdges[2], nEdges[1], edgeLoop[2], edgeLoop[0], edgeLoop[1],
        nFaces[2], nFaces[0], nFaces[1], f
      );

      //追加した四面体をデータに保存
      newTetrahedrons.Add(nTetra);
    }

   /*List<DPEdge> newEdges = new List<DPEdge>();
   List<DPFace> newFaces = new List<DPFace>();
   List<DPTetrahedron> newTetrahedrons = new List<DPTetrahedron>();
   //共通している面以外の面を四面体情報と共に保持
   List<(DPTetrahedron, int)> exFaces = new List<(DPTetrahedron, int)>();
   foreach(var t in tetras)
   {
     for(int i = 0; i < t.Faces.Length; i++)
     {
       //exFacesの中にすでに同じ面がないかチェック
       int index = -1;
       for(int j = 0; j < exFaces.Count; j++)
       {
         //同じ面があったら削除
         if(t.Faces[i] == exFaces[j].Item1.Faces[exFaces[j].Item2])
         {
           index = j;
           exFaces.RemoveAt(j);
           break;
         }
       }

       //なかった場合
       if(index == -1)
       {
         exFaces.Add((t, i));
       }
     }
   }

   //各面から点に向けて四面体を生成
   (int[], int[])[] tetra_config = new (int[], int[])[]
   {
     //辺のインデックス，点のインデックス
     (new int[]{ 0, 3, 1 }, new int[]{ 0, 1, 2 }),
     (new int[]{ 2, 4, 0 }, new int[]{ 0, 3, 1 }),
     (new int[]{ 1, 5, 2 }, new int[]{ 0, 2, 3 }),
     (new int[]{ 4, 5, 3 }, new int[]{ 1, 3, 2 }),
   };
   foreach (var f_ti in exFaces)
   {
     //面情報
     DPTetrahedron t = f_ti.Item1;
     DPFace f = f_ti.Item1.Faces[f_ti.Item2];

     //点のループと辺のループを見つける
     DPPoint[] pointLoop = new DPPoint[3];
     DPEdge[] edgeLoop = new DPEdge[3];

     int[] pIndices = tetra_config[f_ti.Item2].Item2;
     for (int i = 0; i < pointLoop.Length; i++)
     {
       pointLoop[0] = t.Points[pIndices[i]];
     }
     int[] eIndices = tetra_config[f_ti.Item2].Item1;
     for (int i = 0; i < edgeLoop.Length; i++)
     {
       edgeLoop[0] = t.Edges[eIndices[i]];
     }

     //面からみてターゲット点は表にあるはず（なかったらエラー）
     if(!Utils.JudgeFrontOrBack(pointLoop[0].vec, pointLoop[1].vec, pointLoop[2].vec, p.vec))
     {
       Debug.LogError("なんと追加点が面の後ろ側にあります．" + f_ti.Item1 + "" + p.vec);
       continue;
     }

     //新しい辺と面を用意
     DPEdge[] nEdges = new DPEdge[3];
     DPFace[] nFaces = new DPFace[3];
     for(int i = 0; i < nEdges.Length; i++)
     {
       nEdges[i] = new DPEdge(pointLoop[i], p);
     }
     for(int i = 0; i < nFaces.Length; i++)
     {
       nFaces[i] = new DPFace(nEdges[i], nEdges[(i + 1) % nEdges.Length], edgeLoop[i]);
     }

     //四面体を生成
     DPTetrahedron nTetra = new DPTetrahedron(nFaces[0], nFaces[2], nFaces[1], f);

     //追加した分をデータに保存
     newEdges.AddRange(nEdges);
     newFaces.AddRange(nFaces);
     newTetrahedrons.Add(nTetra);
   }*/

   /*//共通している面以外の面（一番外側の面）を取得
   List<DPFace> exFaces = new List<DPFace>();

   //Dictionary<>
   foreach(var t in tetras)
   {
     foreach(var f in t.Faces)
     {
       int index = exFaces.IndexOf(f);
       if (index != -1)
       {
         exFaces.Add(f);
       }
       else
       {
         exFaces.RemoveAt(index);  //共通している面を排除
       }
     }
   }

   //各面から点に向けて四面体を生成
   foreach(var f in exFaces)
   {
     List<DPEdge> es = new List<DPEdge>(f.Edges);

     //点のループと辺のループを見つける
     DPPoint[] pointLoop = new DPPoint[3];
     DPEdge[] edgeLoop = new DPEdge[3];
     int swap = 0;
     for(int i = 0; i < es.Count; i++)
     {
       pointLoop[i] = es[i].Points[swap];
       edgeLoop[i] = es[i];
       for(int j = i + 1; j < es.Count; j++)
       {
         if(es[i].Points[swap] == es[j].Points[0])
         {
           var t = es[i];
           es[i] = es[j + 1];
           es[j + 1] = t;
           swap = 0;
           break;
         }
         else if(es[i].Points[swap] == es[j].Points[1])
         {
           var t = es[i];
           es[i] = es[j + 1];
           es[j + 1] = t;
           swap = 1;
         }
       }
     }

     //面のループとターゲット点の位置から四面体を生成する．
     if(Utils.JudgeFrontOrBack(pointLoop[0].vec, pointLoop[1].vec, pointLoop[2].vec, p.vec))
     {
       //表にある場合

     }
     else
     {
       //裏にある場合
     }
   }*/

    //新規分割データを準備
    TetrahedronPartitionData sep_data = new TetrahedronPartitionData();
    sep_data.ParentTetrahedron = new List<DPTetrahedron>(tetras);
    sep_data.NewEdges = new List<DPEdge>(newEdges.Values);
    sep_data.NewFaces = new List<DPFace>(newFaces.Values);
    sep_data.NewTetrahedrons = new List<DPTetrahedron>(newTetrahedrons);

    //分割成功（元の四面体を無効にする）
    foreach (var t in tetras) t.Enabled = false;

    exFaces.Clear();
    newEdges.Clear();
    newFaces.Clear();
    newTetrahedrons.Clear();

    return sep_data;
  }

  /// <summary>
  /// 点がこの四面体の外接球内にあるか調べる．
  /// </summary>
  /// <param name="point">対象の点</param>
  /// <returns>点がこの四面体の外接球内にあるか</returns>
  public bool CheckPointIncludeCircumsphere(DPPoint point)
  {
    float distance = (point.vec - this.Center).magnitude;
    //Debug.Log(string.Format("center: {0}, radius: {1}, distance: {2}", this.center, this.radius, distance));
    return this.Radius != -1f && distance < this.Radius;
  }

  /// <summary>
  /// 点がこの四面体内にあるか調べる．
  /// http://steve.hollasch.net/cgindex/geometry/ptintet.html
  /// </summary>
  /// <param name="point">対象の点</param>
  /// <returns>点がこの四面体内にあるか</returns>
  public bool CheckPointIncludeTetrahedron(DPPoint point)
  {
    DPPoint p1 = this.Points[0];
    DPPoint p2 = this.Points[1];
    DPPoint p3 = this.Points[2];
    DPPoint p4 = this.Points[3];
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
  /// 指定された辺と関係のある面を返す
  /// </summary>
  /// <param name="">ドロネー辺</param>
  /// <returns>関連するドロネー面のリスト</returns>
  public DPFace[] GetRelFacesOfDPEdge(DPEdge dpEdge)
  {
    var fs = new DPFace[2];
    int fsIndex = 0;
    foreach (var f in this.Faces)
    {
      foreach (var e in f.Edges)
      {
        if (e == dpEdge)
        {
          fs[fsIndex++] = f;
          break;
        }
      }
      //面が2つ見つかったらok
      if (fsIndex == 2) break;
    }
    //エラー処理
    if (fsIndex != 2) Debug.LogError("VPFace - Bad Relation : " + this + "" + dpEdge);
    return fs;
  }

  /// <summary>
  /// この四面体の外接球の中心座標と半径を計算する．
  /// 計算結果はCenterとRadiusに格納する．
  /// </summary>
  private void CalcCenterAndRadius()
  {
    DPPoint p1 = this.Points[0];
    DPPoint p2 = this.Points[1];
    DPPoint p3 = this.Points[2];
    DPPoint p4 = this.Points[3];

    float[][] A = new float[][]
    {
      new float[]{p2.x - p1.x, p2.y - p1.y, p2.z - p1.z},
      new float[]{p3.x - p1.x, p3.y - p1.y, p3.z - p1.z},
      new float[]{p4.x - p1.x, p4.y - p1.y, p4.z - p1.z}
    };
    float[] b = new float[]
    {
      0.5f * (p2.x * p2.x - p1.x * p1.x + p2.y * p2.y - p1.y * p1.y + p2.z * p2.z - p1.z * p1.z),
      0.5f * (p3.x * p3.x - p1.x * p1.x + p3.y * p3.y - p1.y * p1.y + p3.z * p3.z - p1.z * p1.z),
      0.5f * (p4.x * p4.x - p1.x * p1.x + p4.y * p4.y - p1.y * p1.y + p4.z * p4.z - p1.z * p1.z)
    };
    float[] x = new float[3];
    if (gauss(A, b, ref x) == 0)
    {
      this.Radius = -1f;
    }
    else
    {
      this.Center = new Vector3((float)x[0], (float)x[1], (float)x[2]);
      this.Radius = (this.Center - p1.vec).magnitude;
    }
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

    for (int k = 0; k < n; k++)
    {
      ip[k] = k;
      float u = 0;
      for (int j = 0; j < n; j++)
      {
        float t = Mathf.Abs(a[k][j]);
        if (t > u) u = t;
      }
      if (u == 0) return 0f;
      weight[k] = 1f / u;
    }
    float det = 1f;
    for (int k = 0; k < n; k++)
    {
      float u = -1f;
      int m = 0;
      for (int i = k; i < n; i++)
      {
        int ii = ip[i];
        float t = Mathf.Abs(a[ii][k]) * weight[ii];
        if (t > u)
        {
          u = t;
          m = i;
        }
      }
      int ik = ip[m];
      if (m != k)
      {
        ip[m] = ip[k];
        ip[k] = ik;
        det = -det;
      }
      u = a[ik][k];
      det *= u;
      if (u == 0) return 0;
      for (int i = k + 1; i < n; i++)
      {
        int ii = ip[i];
        float t = (a[ii][k] /= u);
        for (int j = k + 1; j < n; j++)
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
    for (int i = 0; i < n; i++)
    {
      int ii = ip[i];
      float t = b[ii];
      for (int j = 0; j < i; j++)
      {
        t -= a[ii][j] * x[j];
      }
      x[i] = t;
    }
    for (int i = n - 1; i >= 0; i--)
    {
      float t = x[i];
      int ii = ip[i];
      for (int j = i + 1; j < n; j++)
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

    if (det != 0)
    {
      solve(a, b, ip, ref x);
    }
    return det;
  }

  /// <summary>
  /// 文字列化
  /// </summary>
  /// <returns>クラス情報</returns>
  public override string ToString()
  {
    //objectに参照があればo，nullならxを返す関数
    string f(object o) => (o != null) ? "o" : "x";
    string g(object o) => (o != null) ? "OK" : "NG";
    string h(bool b) => b ? "OK" : "NG";

    string relation =
      $"【各インスタンス存在状況】\n" +
      $"{f(Points[0])}-{f(Edges[2])}-{f(Points[3])}-{f(Edges[2])}-{f(Points[0])}\n" +
      $"|{f(Faces[2])} /|{f(Faces[1])} /|\n" +
      $"{f(Edges[1])} {f(Edges[5])} {f(Edges[4])} {f(Edges[0])} {f(Edges[1])}\n" +
      $"|/ {f(Faces[3])}|/ {f(Faces[0])}|\n" +
      $"{f(Points[2])}-{f(Edges[3])}-{f(Points[1])}-{f(Edges[3])}-{f(Points[2])}\n\n";

    //各要素がそれぞれの参照を持っているかを確認（必要な分のみ表示）
    Dictionary<string, object>[] PRels = new Dictionary<string, object>[]
    {
      //P1
      new Dictionary<string, object>()
      {
        {"E1", Edges[0]}, {"E2", Edges[1]}, {"E3", Edges[2]},
        {"F1", Faces[0]}, {"F2", Faces[1]}, {"F3", Faces[2]},
        {"T", this}
      },
      //P2
      new Dictionary<string, object>()
      {
        {"E1", Edges[0]}, {"E4", Edges[3]}, {"E5", Edges[4]},
        {"F1", Faces[0]}, {"F2", Faces[1]}, {"F4", Faces[3]},
        {"T", this}
      },
      //P3
      new Dictionary<string, object>()
      {
        {"E2", Edges[1]}, {"E4", Edges[3]}, {"E6", Edges[5]},
        {"F1", Faces[0]}, {"F3", Faces[2]}, {"F4", Faces[3]},
        {"T", this}
      },
      //P4
      new Dictionary<string, object>()
      {
        {"E3", Edges[2]}, {"E5", Edges[4]}, {"E6", Edges[5]},
        {"F2", Faces[1]}, {"F3", Faces[2]}, {"F4", Faces[3]},
        {"T", this}
      }
    };
    Dictionary<string, object>[] ERels = new Dictionary<string, object>[]
    {
      //E1
      new Dictionary<string, object>()
      {
        {"P1", Points[0]}, {"P2", Points[1]},
        {"F1", Faces[0]}, {"F2", Faces[1]},
        {"T", this}
      },
      //E2
      new Dictionary<string, object>()
      {
        {"P1", Points[0]}, {"P3", Points[2]},
        {"F1", Faces[0]}, {"F3", Faces[2]},
        {"T", this}
      },
      //E3
      new Dictionary<string, object>()
      {
        {"P1", Points[0]}, {"P4", Points[3]},
        {"F2", Faces[1]}, {"F3", Faces[2]},
        {"T", this}
      },
      //E4
      new Dictionary<string, object>()
      {
        {"P2", Points[1]}, {"P3", Points[2]},
        {"F1", Faces[0]}, {"F4", Faces[3]},
        {"T", this}
      },
      //E5
      new Dictionary<string, object>()
      {
        {"P2", Points[1]}, {"P4", Points[3]},
        {"F2", Faces[1]}, {"F4", Faces[3]},
        {"T", this}
      },
      //E6
      new Dictionary<string, object>()
      {
        {"P3", Points[2]}, {"P4", Points[3]},
        {"F3", Faces[2]}, {"F4", Faces[3]},
        {"T", this}
      }
    };
    Dictionary<string, object>[] FRels = new Dictionary<string, object>[]
    {
      //F1
      new Dictionary<string, object>()
      {
        {"E1", Edges[0]}, {"E2", Edges[1]}, {"E4", Edges[3]},
        {"T", this}
      },
      //F2
      new Dictionary<string, object>()
      {
        {"E1", Edges[0]}, {"E3", Edges[2]}, {"E5", Edges[4]},
        {"T", this}
      },
      //F3
      new Dictionary<string, object>()
      {
        {"E2", Edges[1]}, {"E3", Edges[2]}, {"E6", Edges[5]},
        {"T", this}
      },
      //F4
      new Dictionary<string, object>()
      {
        {"E4", Edges[3]}, {"E5", Edges[4]}, {"E6", Edges[5]},
        {"T", this}
      }
    };
    string eachElemInfo = "【接続状況】\n";
    //Points
    for(int i = 0; i < this.Points.Length; i++)
    {
      eachElemInfo += $"P{i + 1}({g(Points[i])}) : ";
      if(Points[i] != null)
      {
        foreach (var item in PRels[i])
        {
          eachElemInfo += $"{item.Key}({h(Points[i].Have(item.Value))}), ";
        }
      }
      eachElemInfo += "\n";
    }
    //Edges
    for (int i = 0; i < this.Edges.Length; i++)
    {
      eachElemInfo += $"E{i + 1}({g(Edges[i])}) : ";
      if (Edges[i] != null)
      {
        foreach (var item in ERels[i])
        {
          eachElemInfo += $"{item.Key}({h(Edges[i].Have(item.Value))}), ";
        }
      }
      eachElemInfo += "\n";
    }
    //Faces
    for (int i = 0; i < this.Faces.Length; i++)
    {
      eachElemInfo += $"F{i + 1}({g(Faces[i])}) : ";
      if (Faces[i] != null)
      {
        foreach (var item in FRels[i])
        {
          eachElemInfo += $"{item.Key}({h(Faces[i].Have(item.Value))}), ";
        }
      }
      eachElemInfo += "\n";
    }
    eachElemInfo += "\n";

    return relation + eachElemInfo + base.ToString();
  }

  /* VP関連 */
  /// <summary>
  /// ボロノイ点を生成する．
  /// </summary>
  public void CreateVP()
  {
    this.VPPoint = VPPoint.Create(this);
  }

  /* クラスメソッド */
  /// <summary>
  /// ２つのドロネー面で共有されているドロネー辺を返す．
  /// </summary>
  /// <param name="f1">ドロネー面１</param>
  /// <param name="f2">ドロネー面２</param>
  /// <returns>２つのドロネー面で共有されているドロネー辺</returns>
  public static DPEdge GetEdgeSharedBetweenFaces(DPFace f1, DPFace f2)
  {
    foreach(var e1 in f1.Edges)
    {
      foreach(var e2 in f2.Edges)
      {
        if (ReferenceEquals(e1, e2)) return e1;
      }
    }
    return null;
  }

  /// <summary>
  /// ２つのドロネー辺で共有されているドロネー点を返す．
  /// </summary>
  /// <param name="e1">ドロネー辺１</param>
  /// <param name="e2">ドロネー辺２</param>
  /// <returns>２つのドロネー辺で共有されているドロネー点</returns>
  public static DPPoint GetPointSharedBetweenEdges(DPEdge e1, DPEdge e2)
  {
    foreach(var p1 in e1.Points)
    {
      foreach(var p2 in e2.Points)
      {
        if (ReferenceEquals(p1, p2)) return p1;
      }
    }
    return null;
  }

  /// <summary>
  /// 有効な四面体のリストを返す．
  /// </summary>
  /// <param name="dpTetras">全ての四面体のリスト</param>
  /// <returns>有効な四面体のリスト</returns>
  public static List<DPTetrahedron> GetEnabledDPTetrahedrons(List<DPTetrahedron> dpTetras)
  {
    var enabledTetras = new List<DPTetrahedron>();
    foreach (var dpt in dpTetras) if (dpt.Enabled) enabledTetras.Add(dpt);
    return enabledTetras;
  }
}

/// <summary>
/// 四面体の分割情報
/// </summary>
public struct TetrahedronPartitionData
{
  /* 親四面体 */
  /// <summary>
  /// 分割前の親四面体のリスト
  /// （複数の親が同時に分割される可能性がある）
  /// </summary>
  public List<DPTetrahedron> ParentTetrahedron;

  /* 新データ */
  //public List<VPPoint> NewPoints;
  /// <summary>
  /// 新しく生成された辺
  /// </summary>
  public List<DPEdge> NewEdges;
  /// <summary>
  /// 新しく生成された面
  /// </summary>
  public List<DPFace> NewFaces;
  /// <summary>
  /// 新しく生成された四面体
  /// </summary>
  public List<DPTetrahedron> NewTetrahedrons;
}
#endregion DeraunayPartitionDataClass

#region VoronoiPartitionDataClass
/// <summary>
/// ボロノイ点のクラス
/// </summary>
public class VPPoint
{
  public Vector3 Point;

  /// <summary>
  /// ボロノイ平面をメッシュ化するときに使用
  /// </summary>
  public int VertexIndex;

  public VPPoint(Vector3 p)
  {
    this.Point = p;
  }

  public static VPPoint Create(DPTetrahedron dpTetra)
  {
    var p = new VPPoint(dpTetra.Center);
    //dpTetra.VPPoint = p;
    return p;
  }
}

/// <summary>
/// ボロノイ辺
/// </summary>
public class VPEdge
{
  public VPPoint[] Points;

  public VPEdge()
  {
    this.Points = new VPPoint[2];
  }

  public VPEdge(VPPoint p1, VPPoint p2) : this()
  {
    this.Points[0] = p1;
    this.Points[1] = p2;
  }

  public static VPEdge Create(DPFace dpFace)
  {
    var ps = dpFace.RelEnabledTetrahedrons;

    if(ps.Count != 2)
    {
      //Debug.LogWarning($"VPEdge - Create : ps.Count={ps.Count}, relCount={dpFace.RelTetraCounter}, tetra.Count={dpFace.RelTetrahedrons.Count}\n{ps[0]}");
      return null;
    }

    ps[0].CreateVP();
    ps[1].CreateVP();

    VPEdge inst = new VPEdge(
      ps[0].VPPoint,
      ps[1].VPPoint
    );

    //dpFace.VPEdge = inst;

    return inst;
  }
}

/// <summary>
/// ボロノイ面
/// </summary>
public class VPFace
{
  /* データ */
  public List<VPPoint> Points;  //時計回りor反時計回りに順番に
  public List<VPEdge> Edges;  //時計回りor反時計回りに順番に (p1->p2(p1)->p2)

  /* 関連 */
  public DPEdge RelDPEdge;

  public VPFace()
  {
    this.Points = new List<VPPoint>();
    this.Edges = new List<VPEdge>();
    this.RelDPEdge = null;
  }

/*  public VPFace(List<VPEdge> edges) : this()
  {
    this.Edges = edges;
  }*/

  public static VPFace Create(DPEdge dpEdge)
  {
    string info = "VPFace - Create\n";

    VPFace inst = new VPFace();

    inst.RelDPEdge = dpEdge;

    List<DPTetrahedron> tetras = dpEdge.RelEnabledTetrahedrons;

    info += $"tetras : {tetras.Count}\n";

    if (tetras.Count == 0) return null;

    //関係する全面を取得
    HashSet<DPFace> faces_hs = new HashSet<DPFace>();
    foreach(var t in tetras)
      foreach (var f in t.GetRelFacesOfDPEdge(dpEdge))
        faces_hs.Add(f);
    List<DPFace> dpFaces = new List<DPFace>(faces_hs);

    info += $"dpFaces : {dpFaces.Count}\n";

    //面をチェック
    foreach(var f in dpFaces)
    {
      //変な面を取得
      if (f.RelEnabledTetrahedrons.Count != 2)
      {
        /*info += $"relTetra : {f.RelEnabledTetrahedrons.Count}\n";
        foreach (var t in f.RelEnabledTetrahedrons)
        {
          info += $"{t}\n";
          //Utils.CreateTetrahedron(t);
        }*/
          
        //Debug.Log(info);
        return null;
      }
    }

    //辺が3に満たない場合は面を生成できない
    if (dpFaces.Count < 3) return null;

    //全ての辺を格納
    foreach (var e in dpFaces)
      inst.Edges.Add(e.VPEdge);

    //点を抽出して格納
    HashSet<VPPoint> vpPoints = new HashSet<VPPoint>();
    foreach (var e in inst.Edges)
    {
      vpPoints.Add(e.Points[0]);
      vpPoints.Add(e.Points[1]);
    }
    inst.Points = new List<VPPoint>(vpPoints);

    /*//VPFaceを時計回りor反時計回りにソートしてDPPointを逐次生成
    List<VPPoint> vpPoints = new List<VPPoint>();
    int swap = 0;
    
    for (int i = 0; i < dpFaces.Count; i++)
    {
      //決定したところから頂点リストに追加
      dpFaces[i].RelEnabledTetrahedrons[swap].CreateVP();
      vpPoints.Add(dpFaces[i].RelEnabledTetrahedrons[swap].VPPoint);
      //ソート
      for (int j = i + 1; j < dpFaces.Count; j++)
      {
        if(dpFaces[i].RelEnabledTetrahedrons[swap] == dpFaces[j].RelEnabledTetrahedrons[0])
        {
          var t = dpFaces[i];
          dpFaces[i] = dpFaces[j];
          dpFaces[j] = t;
          swap = 0;
          break;
        }
        else if (dpFaces[i].RelEnabledTetrahedrons[swap] == dpFaces[j].RelEnabledTetrahedrons[1])
        {
          var t = dpFaces[i];
          dpFaces[i] = dpFaces[j];
          dpFaces[j] = t;
          swap = 1;
          break;
        }
      }
    }*/

    //辺を生成
    /*List<VPEdge> vpEdges = new List<VPEdge>();
    for (int i = 0; i < vpPoints.Count; i++)
      vpEdges.Add(new VPEdge(vpPoints[i], vpPoints[(i + 1) % vpPoints.Count]));*/

    //inst.Points = vpPoints;
    //inst.Edges = vpEdges;

    /*//面情報から辺を生成(点情報と一緒に格納)
    List<VPEdge> vpEdges = new List<VPEdge>();
    foreach (var dpf in dpFaces)
      vpEdges.Add(VPEdge.Create(dpf));*/
    /*//初期値を設定
    DPFace[] relFaces = tetras[0].GetRelFacesOfDPEdge(dpEdge);
    DPFace relFace = relFaces[0];
    do
    {
      //面から四面体の候補を取得
      List<DPTetrahedron> candTetras = relFace.GetEnabledDPTetrahedrons();
      info += $"candTetras : {candTetras.Count}\n";
      //候補四面体の中から，tetrasに残っている四面体を含む四面体を見つける
      DPTetrahedron candTetra = null;
      foreach (var t in candTetras)
      {
        if (tetras.IndexOf(t) != -1)
        {
          candTetra = t;
          break;
        }
      }
      info += $"candTetra : {candTetra}\n";
      //もしcandTetraが見つからなければnullを返す
      if (candTetra == null)
      {
        return null;
      }
      //点を登録
      inst.Points.Add(new VPPoint(candTetra.Center));
      //処理済みの四面体を削除
      tetras.Remove(candTetra);
      //次の面を見つける
      DPFace[] nextRelFaces = candTetra.GetRelFacesOfDPEdge(dpEdge);
      //次の面は前回の面ではない方を選ぶ
      relFace = (relFace != nextRelFaces[0]) ? nextRelFaces[0] : nextRelFaces[1];
    } while (tetras.Count > 0);

    //辺を生成
    for (int i = 0; i < inst.Points.Count; i++)
    {
      inst.Edges.Add(new VPEdge(inst.Points[i], inst.Points[(i + 1) % inst.Points.Count]));
    }

    if (inst.Points.Count < 3 || inst.Edges.Count < 3)
    {
      Debug.Log($"less points - points : {inst.Points.Count}, edges : {inst.Edges.Count}");
      return null;
    }*/

    //Debug.Log(info);

    //dpEdge.VPFace = inst;

    tetras.Clear();
    faces_hs.Clear();
    dpFaces.Clear();
    vpPoints.Clear();

    return inst;
  }
}
#endregion VoronoiPartitionDataClass