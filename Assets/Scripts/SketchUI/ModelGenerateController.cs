using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyGeneticAlgorithm;

[System.Serializable]
public struct ModelGenerateParam
{
  public int NumEpisode;
  public int NumGeneration;
  public int PopulationSize;
  public int NumSavedElete;
  public bool RemoveInvalidIndividual;
  public float MutationRate;
  public SelectionMethod SelectionMethod;
  public CrossoverMethod CrossoverMethod;
  public float Attenuation;
  public float AllowedStickOutPercentage;
  public int MaxPrimitiveNum;
  public int NumAgentPerGA;
  public float OnSketchC;
  public float InterferenceC;
  public float ExistentialC;
  public float OnSketchAndInterferenceC;
  public float ChangeVPC;

  public ModelGenerateParam(
    int numEpisode,
    int numGeneration,
    int populationSize,
    int numSavedElete,
    bool removeInvalidIndividual,
    float mutationRate,
    SelectionMethod selectionMethod,
    CrossoverMethod crossoverMethod,
    float attenuation,
    float allowedStickOutPercentage,
    int maxPrimitiveNum,
    int numAgentPerGA,
    float onSketchC,
    float interferenceC,
    float existentialC,
    float onSketchAndInterferenceC,
    float changeVPC
  )
  {
    this.NumEpisode = numEpisode;
    this.NumGeneration = numGeneration;
    this.PopulationSize = populationSize;
    this.NumSavedElete = numSavedElete;
    this.RemoveInvalidIndividual = removeInvalidIndividual;
    this.MutationRate = mutationRate;
    this.SelectionMethod = selectionMethod;
    this.CrossoverMethod = crossoverMethod;
    this.Attenuation = attenuation;
    this.AllowedStickOutPercentage = allowedStickOutPercentage;
    this.MaxPrimitiveNum = maxPrimitiveNum;
    this.NumAgentPerGA = numAgentPerGA;
    this.OnSketchC = onSketchC;
    this.InterferenceC = interferenceC;
    this.ExistentialC = existentialC;
    this.OnSketchAndInterferenceC = onSketchAndInterferenceC;
    this.ChangeVPC = changeVPC;
  }
}

public class ModelGenerateController : ModelGenerater
{
  /// <summary>
  /// 3Dモデルが配置される場所
  /// </summary>
  public GameObject ParentObject;

  /// <summary>
  /// スケッチ初期化パラメータ
  /// </summary>
  [System.NonSerialized]
  public SketchInitilizationParam[] Sips;
  /// <summary>
  /// lpSolverのパラメータ
  /// </summary>
  public ModelGenerateParam MGP;
  /// <summary>
  /// スケッチ
  /// </summary>
  private Sketch[] sketches;
  /// <summary>
  /// lpSolverのインスタンス
  /// </summary>
  private AgentLocationProblemSolver lpSolver;
  /// <summary>
  /// 生成中かどうか
  /// trueの時，lpSolver.Update()が呼ばれる．
  /// </summary>
  //private bool isGenerating;

  [Path]
  public string LogFileName;

  [Header("可視性")]
  public bool IsVisibleAgent;
  public bool IsVisibleDeraunayLine;
  public bool IsVisibleVoronoiLine;
  public bool IsVisibleVoronoiMesh;
  private bool previousIsVisibleAgent;
  private bool previousIsVisibleDeraunayLine;
  private bool previousIsVisibleVoronoiLine;
  private bool previousIsVisibleVoronoiMesh;

  // Start is called before the first frame update
  void Start()
  {
    //ログファイルの準備
    System.IO.File.AppendAllText(LogFileName, "\n\n" + System.DateTime.Now.ToString() + "実行開始\n");

    //セットアップ
    SetupVisible();
  }

  public void ExecuteModelGenerate(DrawableSketch[] dSketches)
  {
    var camera = Camera.main;
    DiscardOldLPSolver();
    this.sketches = SCIPtoSketch(dSketches);
    PrepareNextLPSolver();
    IsGenerating = true;
    Utils.SetMainCamera(camera);
  }

  public Sketch[] SCIPtoSketch(DrawableSketch[] dSketches)
  {
    Sketch[] sketches = new Sketch[dSketches.Length];

    for(int i = 0; i < dSketches.Length; i++)
    {
      SketchAndCameraInitializationParam scip = dSketches[i].SCIP;
      DrawableSketch dSketch = dSketches[i];
      sketches[i] = dSketch.ToSketch(scip);
      sketches[i].SetParent(ParentObject);
    }

    return sketches;
  }

  public void PrepareNextLPSolver()
  {
    //代入
    var numEpisode = MGP.NumEpisode;
    var numGeneration = MGP.NumGeneration;
    var populationSize = MGP.PopulationSize;
    var numSavedElete = MGP.NumSavedElete;
    var removeInvalidIndividual = MGP.RemoveInvalidIndividual;
    var mutationRate = MGP.MutationRate;
    var selectionMethod = MGP.SelectionMethod;
    var crossoverMethod = MGP.CrossoverMethod;
    var attenuation = MGP.Attenuation;
    var allowedStickOutPercentage = MGP.AllowedStickOutPercentage;
    var maxPrimitiveNum = MGP.MaxPrimitiveNum;
    var numAgentPerGA = MGP.NumAgentPerGA;

    Debug.Log(string.Format("NE-{0}, NG-{1}, PS-{2}, NSE-{3}, RII-{4}, MR-{5}, SM-{6}, CM-{7}, Att-{8}, ASOP-{9}, MPN-{10}, NAPG-{11}",
      numEpisode, numGeneration, populationSize, numSavedElete,
      removeInvalidIndividual, mutationRate, selectionMethod, crossoverMethod,
      attenuation, allowedStickOutPercentage, maxPrimitiveNum, numAgentPerGA
    ));

    //前回のlpSolverの親要素を描画しないようにする
    if (lpSolver != null) MonoBehaviour.Destroy(lpSolver.Master);

    //lpSolverの生成
    lpSolver = new AgentLocationProblemSolver(
      numEpisode, numGeneration, populationSize, numSavedElete, removeInvalidIndividual,
      mutationRate, selectionMethod, crossoverMethod, sketches,
      attenuation, allowedStickOutPercentage, maxPrimitiveNum, numAgentPerGA
    );
    lpSolver.SetParent(ParentObject);

    //環境パラメータ設定
    lpSolver.SetEnvironmentParam(
      MGP.OnSketchC,
      MGP.InterferenceC,
      MGP.ExistentialC,
      MGP.OnSketchAndInterferenceC,
      MGP.ChangeVPC
    );
  }

  /// <summary>
  /// 古いLPSolverを処理する．
  /// </summary>
  public void DiscardOldLPSolver()
  {
    if (lpSolver == null) return;
    //前回のオブジェクトを全て破棄
    if (lpSolver.Result != null) lpSolver.Result.Discard();
    if (lpSolver.GA != null) lpSolver.GA.Discard();
    if (Primitive.CombinedVoronoiPartition != null) Primitive.CombinedVoronoiPartition.Discard();
    foreach(Sketch sketch in this.sketches)
    {
      sketch.Discard();
    }
    this.sketches = null;
    lpSolver = null;
  }

  string allResult = "終了\n";
  // Update is called once per frame
  void Update()
  {
    MyStopwatch ms = new MyStopwatch();
    if (IsGenerating && !lpSolver.IsFinished)
    {
      ms.Reset();
      ms.Start();
      //lpSolver.RunOneStep();
      while (!lpSolver.Update()) { }
      //lpSolver.Update();
      ms.Stop();
      ms.ShowResult("合計時間[ModelGenerating]");

      //結果の表示
      if (lpSolver.IsFinished)
      {
        var result = lpSolver.GetResult();
        allResult += result + "\n\n";
        Debug.Log(result);

        //ログをファイルに書き込み
        System.IO.File.AppendAllText(LogFileName, result);

        IsGenerating = false;

        //for Benchmark 
        this.GeneratedTime = lpSolver.msw.Elapsed;
        IsGenerated = true;
      }
    }

    UpdateVisible();
  }

  public void SetupVisible()
  {
    this.previousIsVisibleAgent = !this.IsVisibleAgent;
    this.previousIsVisibleDeraunayLine = !this.IsVisibleDeraunayLine;
    this.previousIsVisibleVoronoiLine = !this.IsVisibleVoronoiLine;
    this.previousIsVisibleVoronoiMesh = !this.IsVisibleVoronoiMesh;
  }

  public void UpdateVisible()
  {
    if(this.previousIsVisibleAgent != this.IsVisibleAgent)
    {
      SetVisible_Agent(this.IsVisibleAgent);
      this.previousIsVisibleAgent = this.IsVisibleAgent;
    }
    if (this.previousIsVisibleDeraunayLine != this.IsVisibleDeraunayLine)
    {
      SetVisible_DelaunayLine(this.IsVisibleDeraunayLine);
      this.previousIsVisibleDeraunayLine = this.IsVisibleDeraunayLine;
    }
    if(this.previousIsVisibleVoronoiLine != this.IsVisibleVoronoiLine)
    {
      SetVisible_VoronoiLine(this.IsVisibleVoronoiLine);
      this.previousIsVisibleVoronoiLine = this.IsVisibleVoronoiLine;
    }
    if(this.previousIsVisibleVoronoiMesh != this.IsVisibleVoronoiMesh)
    {
      SetVisible_VoronoiMesh(this.IsVisibleVoronoiMesh);
      this.previousIsVisibleVoronoiMesh = this.IsVisibleVoronoiMesh;
    }
  }

  /// <summary>
  /// 点の可視を変更する．
  /// </summary>
  /// <param name="visible"></param>
  public void SetVisible_Agent(bool visible)
  {
    Debug.Log("change view agent : " + visible.ToString());
    lpSolver?.Result?.SetVisibleParticle(visible);
    foreach (var ind in GetALPIndividuals(this.lpSolver))
    {
      ind.Agent.Item2.SetActive(visible);
    }
  }

  /// <summary>
  /// ドロネー線の可視を変更する．
  /// </summary>
  /// <param name="visible"></param>
  public void SetVisible_DelaunayLine(bool visible)
  {
    Debug.Log("change view delaunay line : " + visible.ToString());
    lpSolver?.Result?.SetVisibleDerauneyLine(visible);
  }

  /// <summary>
  /// ボロノイ線の可視を変更する．
  /// </summary>
  /// <param name="visible"></param>
  public void SetVisible_VoronoiLine(bool visible)
  {
    Debug.Log("change view voronoi line : " + visible.ToString());
    lpSolver?.Result?.SetVisibleVoronoiLine(visible);
  }

  /// <summary>
  /// ボロノイ図の可視を変更する．
  /// </summary>
  /// <param name="visible"></param>
  public void SetVisible_VoronoiMesh(bool visible)
  {
    Debug.Log("change view voronoi mesh : " + visible.ToString());
    lpSolver?.Result?.SetVisibleVoronoiMesh(visible);
  }

  /// <summary>
  /// ボロノイ図（ソリッドモデル）の可視を変更する．
  /// 使わない予定．
  /// </summary>
  /// <param name="visible"></param>
  public void SetVisible_VoronoiSolidMesh(bool visible)
  {
    Debug.Log("change view voronoi solid mesh : " + visible.ToString());
    Primitive.CombinedVoronoiPartition.SetVisibleVoronoiMesh(visible);
  }

  public IEnumerable<ALPIndividual> GetALPIndividuals(AgentLocationProblemSolver alpSolver)
  {
    if (alpSolver == null) yield break;
    foreach (var ind in alpSolver.LocatedIndividual)
    {
      if (ind == null || ind.Agent.Item2 == null) continue;
      yield return ind;
    }
    foreach (var ind in alpSolver.GA.Population.Individuals)
    {
      if (ind == null || ind.Agent.Item2 == null) continue;
      yield return ind;
    }
  }

  public static void SetAgentSetting(
    Vector3 position_min,
    Vector3 position_max,
    Vector3 position_mutation_range
  )
  {
    ALPIndividual.POSITION_MIN = position_min;
    ALPIndividual.POSITION_MAX = position_max;
    ALPIndividual.POSITION_MUTATION_RANGE = position_mutation_range;
  }

  /* ベンチマーク用メソッド */
  public override void SetParamater(MGP mgp)
  {
    this.MGP = (ModelGenerateParam)(mgp.GetParam());
  }

  public override void StartGenerating(Sketch[] sketches)
  {
    var camera = Camera.main;
    DiscardOldLPSolver();
    this.sketches = sketches;
    PrepareNextLPSolver();
    IsGenerating = true;
    IsGenerated = false;
    Utils.SetMainCamera(camera);
  }

  public override Mesh GetGeneratedMesh()
  {
    return this.lpSolver?.Result?.VoronoiPartition?.voronoiMesh?.GetComponent<MeshFilter>()?.sharedMesh;
  }

  private void Discard()
  {
    if (lpSolver == null) return;
    //前回のオブジェクトを全て破棄
    if (lpSolver.Result != null) lpSolver.Result.Discard();
    if (lpSolver.GA != null) lpSolver.GA.Discard();
    if (Primitive.CombinedVoronoiPartition != null) Primitive.CombinedVoronoiPartition.Discard();
    this.sketches = null;
    lpSolver = null;
  }
}
