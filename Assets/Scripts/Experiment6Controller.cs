using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyGeneticAlgorithm;

public class Experiment6Controller : MonoBehaviour
{
  private AgentLocationProblemSolver lpSolver;
  private List<AgentLocationProblemSolver> lpSolverArchive;

  //public GameObject MainCamera;
  public GameObject ParentObject;
  public GameObject CameraPreviewParentObject;

  public bool viewAgent;
  public bool viewDelaunayLine;
  public bool viewVoronoiLine;
  public bool viewVoronoiMesh;
  public bool viewVoronoiSolidMesh;

  private bool previousViewAgent;
  private bool previousViewDelaunayLine;
  private bool previousViewVoronoiLine;
  private bool previousViewVoronoimesh;
  private bool previousViewVoronoiSolidMesh;

  /*public int NumEpisode;
  public int NumGeneration;
  public int PopulationSize;
  public int NumSavedElete;
  public bool RemoveInvalidIndividual;
  public float MutationRate;
  public SelectionMethod SelectionMethod;
  public CrossoverMethod CrossoverMethod;*/
  //public SketchInitilizationParam[] Sips;
  public SIP Sip;
  /*public float Attenuation;
  public float AllowedStickOutPercentage;
  public int MaxPrimitiveNum;*/
  private Sketch[] sketches;

  //public Vector2Int CameraViewSize;

  [Path]
  public string LogFileName;

  public bool ExecOneStep;
  public bool ContinuousExec;
  public bool CreateCombinedPrimitives;

  /*private int[] NumGenerations = new int[] { 10, 30 };
  private int[] PopulationSizes = new int[] { 40 };
  private int[] NumSavedEletes = new int[] { 1 };
  private bool[] RemoveInvalidIndividuals = new bool[] { false };
  private float[] MutationRates = new float[] { 0.1f, 0.5f, 0.9f };
  private SelectionMethod[] SelectionMethods = new SelectionMethod[] { SelectionMethod.Roulette };
  private CrossoverMethod[] CrossoverMethods = new CrossoverMethod[] { CrossoverMethod.Uniform };
  private float[] Attenuations = new float[] { 0.1f };
  private float[] AllowedStickOutPercentages = new float[] { 0.5f };
  private int[] MaxPrimitiveNums = new int[] { 6 };*/
  /*  private int[] NumGenerations = new int[] { 5 };
    private int[] PopulationSizes = new int[] { 10 };
    private int[] NumSavedEletes = new int[] { 1 };
    private bool[] RemoveInvalidIndividuals = new bool[] { false };
    private float[] MutationRates = new float[] { 1f };
    private SelectionMethod[] SelectionMethods = new SelectionMethod[] { SelectionMethod.Roulette };
    private CrossoverMethod[] CrossoverMethods = new CrossoverMethod[] { CrossoverMethod.Uniform };
    private float[] Attenuations = new float[] { 0.1f };
    private float[] AllowedStickOutPercentages = new float[] { 0.2f };
    private int[] MaxPrimitiveNums = new int[] { 10 };*/
  private int[] NumGenerations = new int[] { 50 };
  private int[] PopulationSizes = new int[] { 20 };
  private int[] NumSavedEletes = new int[] { 1 };
  private bool[] RemoveInvalidIndividuals = new bool[] { false };
  private float[] MutationRates = new float[] { 0.2f };
  private SelectionMethod[] SelectionMethods = new SelectionMethod[] { SelectionMethod.Roulette };
  private CrossoverMethod[] CrossoverMethods = new CrossoverMethod[] { CrossoverMethod.Uniform };
  private float[] Attenuations = new float[] { 0.8f };
  private float[] AllowedStickOutPercentages = new float[] { 0.2f };
  private int[] MaxPrimitiveNums = new int[] { 500 };
  private int[] NumAgentPerGAs = new int[] { 1 };

  // Start is called before the first frame update
  void Start()
  {
    ViewSetup();

    var Sips = this.Sip.Sips;

    //スケッチのセットアップ
    sketches = new Sketch[Sips.Length];
    for (int i = 0; i < Sips.Length; i++)
    {
      //sketches[i] = new Sketch(Sips[i].SketchImage, Sips[i].CameraPosition, Quaternion.Euler(Sips[i].CameraRotation));
      sketches[i] = new Sketch(Sips[i]);
      //GUI上にカメラ視点を入れる
      var rtex = new RenderTexture(Sips[i].SketchImage.width, Sips[i].SketchImage.height, 0, RenderTextureFormat.ARGBFloat);
      rtex.enableRandomWrite = true;
      rtex.Create();
      var camera = sketches[i].CameraObject.GetComponent<Camera>();
      camera.targetTexture = rtex;
      var cameraView = new GameObject("Camera View");
      var rt = cameraView.AddComponent<RectTransform>();
      var cr = cameraView.AddComponent<CanvasRenderer>();
      var ir = cameraView.AddComponent<RawImage>();
      ir.texture = rtex;
      Utils.SetParent(CameraPreviewParentObject, cameraView);
      rt.localScale = new Vector3(1, 1, 1);
      rt.pivot = new Vector2(0, 1);
    }

    Debug.Log(Utils.GetArrayString(sketches));

    //LPSolverのセットアップ
    lpSolverArchive = new List<AgentLocationProblemSolver>();
    PrepareNextLPSolver();

    //ログファイルの準備
    System.IO.File.AppendAllText(LogFileName, "\n\n" + System.DateTime.Now.ToString() + "実行開始\n");
  }

  int count = 0;
  public bool PrepareNextLPSolver()
  {
    int[] indices = new int[11];
    int n = 1;
    indices[0] = NumGenerations.Length;
    indices[1] = PopulationSizes.Length;
    indices[2] = NumSavedEletes.Length;
    indices[3] = RemoveInvalidIndividuals.Length;
    indices[4] = MutationRates.Length;
    indices[5] = SelectionMethods.Length;
    indices[6] = CrossoverMethods.Length;
    indices[7] = Attenuations.Length;
    indices[8] = AllowedStickOutPercentages.Length;
    indices[9] = MaxPrimitiveNums.Length;
    indices[10] = NumAgentPerGAs.Length;

    //全部の積を計算
    for (int i = 0; i < indices.Length; i++)
    {
      n *= indices[i];
    }
    int maxNum = n;

    //チェック
    if (count >= maxNum)
    {
      //Debug.Log(count);
      Debug.Log(allResult);
      return false;
    }

    //それぞれのインデックスを計算
    int ctmp = count;
    for (int i = indices.Length - 1; i >= 0; i--)
    {
      var r = Utils.Div(ctmp, indices[i]);
      ctmp = r.Item1;
      indices[i] = r.Item2;
    }

    //代入
    var numEpisode = 1;
    var numGeneration = NumGenerations[indices[0]];
    var populationSize = PopulationSizes[indices[1]];
    var numSavedElete = NumSavedEletes[indices[2]];
    var removeInvalidIndividual = RemoveInvalidIndividuals[indices[3]];
    var mutationRate = MutationRates[indices[4]];
    var selectionMethod = SelectionMethods[indices[5]];
    var crossoverMethod = CrossoverMethods[indices[6]];
    var attenuation = Attenuations[indices[7]];
    var allowedStickOutPercentage = AllowedStickOutPercentages[indices[8]];
    var maxPrimitiveNum = MaxPrimitiveNums[indices[9]];
    var numAgentPerGA = NumAgentPerGAs[indices[10]];

    Debug.Log(string.Format("array:{11}, count:{12}\nNE-{0}, NG-{1}, PS-{2}, NSE-{3}, RII-{4}, MR-{5}, SM-{6}, CM-{7}, Att-{8}, ASOP-{9}, MPN-{10}",
      numEpisode, numGeneration, populationSize, numSavedElete,
      removeInvalidIndividual, mutationRate, selectionMethod, crossoverMethod,
      attenuation, allowedStickOutPercentage, maxPrimitiveNum, Utils.GetArrayStringNonReturn(indices), count
    ));

    //前回のlpSolverの親要素を描画しないようにする
    if (lpSolver != null) lpSolver.Master.SetActive(false);

    //lpSolverの更新
    lpSolver = new AgentLocationProblemSolver(
      numEpisode, numGeneration, populationSize, numSavedElete, removeInvalidIndividual,
      mutationRate, selectionMethod, crossoverMethod, sketches,
      attenuation, allowedStickOutPercentage, maxPrimitiveNum, numAgentPerGA
    );
    lpSolver.SetParent(ParentObject);

    //lpSolverの保存
    lpSolverArchive.Add(lpSolver);

    count++;
    return true;
  }

  string allResult = "終了\n";
  // Update is called once per frame
  void Update()
  {
    MyStopwatch ms = new MyStopwatch();
    if ((ExecOneStep || ContinuousExec) && !lpSolver.IsFinished)
    {
      Debug.Log("Loop");
      ExecOneStep = false;

      ms.Reset();
      ms.Start();
      //lpSolver.RunOneStep();
      //while (!lpSolver.Update()) { }
      lpSolver.Update();
      ms.Stop();
      //ms.ShowResult("合計時間");

      //結果の表示
      if (lpSolver.IsFinished)
      {
        var result = lpSolver.GetResult();
        allResult += result + "\n\n";
        Debug.Log(result);

        //内容をファイルに書き込み
        System.IO.File.AppendAllText(LogFileName, result);

        //次のLPSolverの準備
        PrepareNextLPSolver();
      }
    }

    //結合ボロノイ図を作成
    if (CreateCombinedPrimitives)
    {
      var Primitives = new List<Primitive>();

      //プリミティブをまとめる
      Primitives.Add(lpSolver.Result);

      Primitive.CombinePrimitives(Primitives.ToArray());
      Primitive.CombinedVoronoiPartition.CreateDelaunayWithThinOutPoints();

      CreateCombinedPrimitives = false;
    }

    SetVisible();
  }

  public void ViewSetup()
  {
    this.previousViewAgent = !this.viewAgent;
    this.previousViewDelaunayLine = !this.viewDelaunayLine;
    this.previousViewVoronoiLine = !this.viewVoronoiLine;
    this.previousViewVoronoimesh = !this.viewVoronoiMesh;
    this.previousViewVoronoiSolidMesh = !this.viewVoronoiSolidMesh;
  }

  public void SetVisible()
  {
    //可視化の設定
    if (this.previousViewAgent != this.viewAgent)
    {
      Debug.Log("change view agent : " + this.viewAgent.ToString());
      foreach(var alps in this.lpSolverArchive)
      {
        alps.Result.SetVisibleParticle(this.viewAgent);
      }
      foreach (var ind in GetALPIndividuals(this.lpSolverArchive))
      {
        ind.Agent.Item2.SetActive(this.viewAgent);
      }
      this.previousViewAgent = this.viewAgent;
    }

    //可視化設定
    if (this.previousViewDelaunayLine != this.viewDelaunayLine)
    {
      Debug.Log("change view delaunay line : " + this.viewDelaunayLine.ToString());
      foreach (var alps in this.lpSolverArchive) alps.Result.SetVisibleDerauneyLine(this.viewDelaunayLine);
      this.previousViewDelaunayLine = this.viewDelaunayLine;
    }
    if (this.previousViewVoronoiLine != this.viewVoronoiLine)
    {
      Debug.Log("change view voronoi line : " + this.viewVoronoiLine.ToString());
      foreach (var alps in this.lpSolverArchive) alps.Result.SetVisibleVoronoiLine(this.viewVoronoiLine);
      this.previousViewVoronoiLine = this.viewVoronoiLine;
    }
    if (this.previousViewVoronoimesh != this.viewVoronoiMesh)
    {
      Debug.Log("change view voronoi mesh : " + this.viewVoronoiMesh.ToString());
      foreach (var alps in this.lpSolverArchive) alps.Result.SetVisibleVoronoiMesh(this.viewVoronoiMesh);
      this.previousViewVoronoimesh = this.viewVoronoiMesh;
    }
    if (this.previousViewVoronoiSolidMesh != this.viewVoronoiSolidMesh)
    {
      Debug.Log("change view voronoi solid mesh : " + this.viewVoronoiSolidMesh.ToString());
      Primitive.CombinedVoronoiPartition.SetVisibleVoronoiMesh(this.viewVoronoiSolidMesh);
      this.previousViewVoronoiSolidMesh = this.viewVoronoiSolidMesh;
    }
  }

  public IEnumerable<ALPIndividual> GetALPIndividuals(List<AgentLocationProblemSolver> alpSolvers)
  {
    foreach(var alps in alpSolvers)
    {
      foreach(var ind in alps.LocatedIndividual)
      {
        if (ind == null || ind.Agent.Item2 == null) continue;
        yield return ind;
      }
      foreach(var ind in alps.GA.Population.Individuals)
      {
        if (ind == null || ind.Agent.Item2 == null) continue;
        yield return ind;
      }
    }
  }

/*  public IEnumerable<Primitive> GetAllPrimitive(List<AgentLocationProblemSolver> lpSolvers)
  {
    foreach (var lpSolver in lpSolvers)
    {
      foreach (var ind in lpSolver.LocatedIndividual)
      {
        if (ind == null || ind.Primitive == null) continue;
        yield return ind.Primitive;
      }
      foreach (var ind in lpSolver.GA.Population.Individuals)
      {
        if (ind == null || ind.Primitive == null) continue;
        yield return ind.Primitive;
      }
    }
  }*/
}
