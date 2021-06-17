using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGeneticAlgorithm;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// AgentLocationProblemSolverで必要なパラメータの構造体
/// </summary>
public struct ALPSParams
{
  /// <summary>
  /// GAのエピソード数
  /// 基本的に１でＯＫ
  /// </summary>
  public int numEpisode;
  /// <summary>
  /// GAの世代数
  /// </summary>
  public int numGeneration;
  /// <summary>
  /// GAの集団サイズ
  /// </summary>
  public int populationSize;
  /// <summary>
  /// GAのエリート保存数
  /// </summary>
  public int numSavedElete;
  /// <summary>
  /// GAで制約を満たさない個体を集団から除外するかを指定する．
  /// 現在は使われていない．
  /// </summary>
  public bool removeInvalidIndividual;
  /// <summary>
  /// GAの突然変異確率
  /// </summary>
  public float mutationRate;
  /// <summary>
  /// GAの親選択メソッド
  /// </summary>
  public SelectionMethod selectionMethod;
  /// <summary>
  /// GAの交叉メソッド
  /// </summary>
  public CrossoverMethod crossoverMethod;
  /// <summary>
  /// 入力スケッチの配列
  /// </summary>
  public Sketch[] sketches;
  /// <summary>
  /// LocationProblemSolverの名残
  /// プリミティブが重なるにつれて評価値を減衰させるためのパラメータ
  /// AgentLocationProblemSolverでは使われない．
  /// </summary>
  public float attenuation;
  /// <summary>
  /// LocationProblemSolverの名残
  /// プリミティブがスケッチからどれだけはみ出すのを許容するかのパラメータ
  /// AgentLocationProblemSolverでは使われない．
  /// </summary>
  public float allowedStickOutPercentage;
  /// <summary>
  /// AgentLocationProblemSolverでは，点エージェントを配置する数．
  /// LocationProblemSolverでは，プリミティブエージェントを配置する数．
  /// </summary>
  public int maxPrimitiveNum;
  /// <summary>
  /// 一回のGAで配置するエージェントの数
  /// </summary>
  public int numAgentPerGA;
}

/// <summary>
/// 複数のスケッチから最適なエージェント配置を見つける
/// エージェントと個体を混同して使っているが，同じ意味を示す．
/// </summary>
public class AgentLocationProblemSolver
{
  /// <summary>
  /// 入力スケッチの配列
  /// </summary>
  public Sketch[] Sketches;
  //public Texture2D[] EvaluationMap;
  //public List<Primitive> LocatedPrimitives;
  /// <summary>
  /// 配置した個体のリスト
  /// </summary>
  public List<ALPIndividual> LocatedIndividual;

  /// <summary>
  /// 配置した個体から生成される3Dモデル
  /// </summary>
  public Primitive Result;

  /// <summary>
  /// 既に配置された場所に対しての評価値の減衰値
  /// </summary>
  public float Attenuation;

  /// <summary>
  /// 何割まではみ出しを許可するか
  /// </summary>
  public float AllowedStickOutPercentage;

  /// <summary>
  /// プリミティブを配置する最大の個数
  /// </summary>
  public int MaxPrimitiveNum;

  /// <summary>
  /// 一回のGA毎に配置するエージェントの数
  /// </summary>
  public int NumAgentPerGA;

  /// <summary>
  /// 現在配置済みのプリミティブ数
  /// </summary>
  public int NumOfLocatedPrimitives { get { return LocatedIndividual.Count; } }

  /// <summary>
  /// 3Dモデル生成が終了したかどうか
  /// </summary>
  public bool IsFinished { get { return NumOfLocatedPrimitives >= MaxPrimitiveNum; } }

  /// <summary>
  /// 3Dモデル化にかかった時間を計測
  /// </summary>
  public MyStopwatch msw;

  /// <summary>
  /// 親オブジェクト
  /// 生成したすべてのオブジェクトがこの下にネストされる
  /// </summary>
  public GameObject Parent;
  /// <summary>
  /// ALPSのマスターオブジェクト
  /// ALPS一回分のオブジェクトをこの下にネストする．
  /// </summary>
  public GameObject Master;
  /// <summary>
  /// GAで点を仮置きする際にこのオブジェクトの下にネストする．
  /// </summary>
  public GameObject ChildrenParent;
  /// <summary>
  /// LocatedIndividualに入れられた個体のオブジェクトをこの下にネストして，表示する．
  /// </summary>
  private GameObject EleteParent;
  /// <summary>
  /// GAでの各世代ごとにオブジェクトを作成し，その下に各世代での個体を記録する．
  /// </summary>
  private List<GameObject> children;

  /// <summary>
  /// GAインスタンス
  /// </summary>
  public GeneticAlgorithm<ALPEnvironment, ALPIndividual> GA;
  /// <summary>
  /// GAで用いる環境インスタンス
  /// </summary>
  public ALPEnvironment Environment;

  /// <summary>
  /// 3Dモデルをスケッチに投影する際に設定するマテリアル
  /// 不透明なものでないと，上手く描画できないのでOpaqueにしましょう．
  /// </summary>
  public static Material CaptureMaterial;
  /// <summary>
  /// ユーザが実際に見る3Dモデルのマテリアル
  /// こちらは透明でも不透明でもよい．
  /// 見やすいマテリアルを設定しよう
  /// </summary>
  public static Material PreviewMaterial;

  /// <summary>
  /// デフォルトコンストラクタ
  /// </summary>
  public AgentLocationProblemSolver() : this(null) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="sketches">入力スケッチ</param>
  public AgentLocationProblemSolver(Sketch[] sketches) : this(100, 100, 10, 1, true, 0.2f, SelectionMethod.Roulette, CrossoverMethod.Uniform, sketches, 0.5f, 0.1f, 10, 1) { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="aLPSParams">ALPSパラメータ</param>
  public AgentLocationProblemSolver(ALPSParams aLPSParams) : this(
    aLPSParams.numEpisode, aLPSParams.numGeneration, aLPSParams.populationSize, aLPSParams.numSavedElete,
    aLPSParams.removeInvalidIndividual, aLPSParams.mutationRate, aLPSParams.selectionMethod, aLPSParams.crossoverMethod,
    aLPSParams.sketches, aLPSParams.attenuation, aLPSParams.allowedStickOutPercentage, aLPSParams.maxPrimitiveNum,
    aLPSParams.numAgentPerGA
  ){}

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="numEpisode">エピソード数</param>
  /// <param name="numGeneration">世代数</param>
  /// <param name="populationSize">集団サイズ</param>
  /// <param name="numSavedElete">エリート保存数</param>
  /// <param name="removeInvalidIndividual">違反個体の排除</param>
  /// <param name="mutationRate">突然変異確率</param>
  /// <param name="selectionMethod">親選択メソッド</param>
  /// <param name="crossoverMethod">交叉メソッド</param>
  /// <param name="sketches">入力スケッチ</param>
  /// <param name="attenuation">評価マップ減衰係数</param>
  /// <param name="allowedStickOutPercentage">スケッチ外許容割合</param>
  /// <param name="maxPrimitiveNum">配置エージェント数</param>
  /// <param name="numAgentPerGA">１度のGAで配置するエージェント数</param>
  public AgentLocationProblemSolver(
    int numEpisode, int numGeneration, int populationSize, int numSavedElete, bool removeInvalidIndividual,
    float mutationRate, SelectionMethod selectionMethod, CrossoverMethod crossoverMethod, Sketch[] sketches,
    float attenuation, float allowedStickOutPercentage, int maxPrimitiveNum, int numAgentPerGA
  )
  {
    //ストップウォッチ計測開始
    msw = new MyStopwatch();
    msw.Start();

    //スケッチインスタンスへ各種マップを設定
    this.Sketches = sketches;
    for (int i = 0; i < this.Sketches.Length; i++)
    {
      this.Sketches[i].SetEvaluationMap(GPGPUUtils.GPGPUTextureCopy(this.Sketches[i].SketchImage)); //このマップは使用しないが一応生成
      this.Sketches[i].ScanedImage = Utils.CreateMonocolorTexture(Color.clear, this.Sketches[i].SketchImage.width, this.Sketches[i].SketchImage.height);
    }

    //初期化やパラメータ保存
    this.LocatedIndividual = new List<ALPIndividual>();
    this.Attenuation = attenuation;
    this.AllowedStickOutPercentage = allowedStickOutPercentage;
    this.MaxPrimitiveNum = maxPrimitiveNum;
    this.NumAgentPerGA = numAgentPerGA;

    //環境インスタンスの生成
    ALPEnvironment environment = new ALPEnvironment(sketches, null, attenuation, allowedStickOutPercentage);
    this.Environment = environment;
    environment.Attenuation = attenuation;

    //GAインスタンスの生成
    this.GA = new GeneticAlgorithm<ALPEnvironment, ALPIndividual>(
      numEpisode, numGeneration, populationSize, numSavedElete, removeInvalidIndividual,
      mutationRate, selectionMethod, crossoverMethod, environment
    );

    //各オブジェクトの生成とネスト設定
    this.Master = new GameObject(ToString());
    this.ChildrenParent = new GameObject("Populations");
    Utils.SetParent(this.Master, this.ChildrenParent);
    this.children = new List<GameObject>();
    this.children.Add(new GameObject("First Population"));
    foreach (var child in this.children)
      Utils.SetParent(this.ChildrenParent, child);
    this.EleteParent = new GameObject("Elete");
    Utils.SetParent(this.Master, this.EleteParent);
    ALPIndividual.SetBaseParent(this.children[0]);

    //結果の格納場所
    this.Result = new Primitive();

    //探索対象を初期化　現在は使ってないかも
    ALPIndividual.SearchExistential = true;

    //初期個体として非存在エージェントを遠点６方向に配置
    ALPIndividual[] alpis = new ALPIndividual[6];
    alpis[0] = new ALPIndividual(); alpis[0].Position = new Vector3(30.04f, 0.12f, 0.15f); alpis[0].IsExistential = false;
    alpis[1] = new ALPIndividual(); alpis[1].Position = new Vector3(-30.05f, 0.23f, 0.14f); alpis[1].IsExistential = false;
    alpis[2] = new ALPIndividual(); alpis[2].Position = new Vector3(0.05f, 30.1f, 0.11f); alpis[2].IsExistential = false;
    alpis[3] = new ALPIndividual(); alpis[3].Position = new Vector3(0.02f, -30.2f, 0.15f); alpis[3].IsExistential = false;
    alpis[4] = new ALPIndividual(); alpis[4].Position = new Vector3(0.03f, 0.13f, 30.12f); alpis[4].IsExistential = false;
    alpis[5] = new ALPIndividual(); alpis[5].Position = new Vector3(0.04f, 0.09f, -30.05f); alpis[5].IsExistential = false;
    foreach (var ind in alpis)
    {
      ind.CreateChildObject();
      ind.SetParent(this.EleteParent);
      ind.Prepare();
      this.LocatedIndividual.Add(ind);
    }
    CreateResult(); //一旦，3Dモデル化

    //GAの初期化
    this.GA.Discard();
    this.GA.Initialize();
  }

  /// <summary>
  /// 親を変更する
  /// </summary>
  /// <param name="parent">親オブジェクト</param>
  public void SetParent(GameObject parent)
  {
    this.Parent = parent;
    Utils.SetParent(parent, this.Master);
  }

  /// <summary>
  /// GAを1世代ずつ実行
  /// </summary>
  /// <returns></returns>
  public bool Update()
  {
    if (RunGAOneStep()) return PrepareNextStep();
    return false;
  }

  /// <summary>
  /// GAを実行する．
  /// </summary>
  public void RunGA()
  {
    this.GA.Exec();
    //while (!RunGAOneStep()) { }
  }

  /// <summary>
  /// GAを一世代分実行する．
  /// </summary>
  /// <returns></returns>
  public bool RunGAOneStep()
  {
    //子オブジェクトにまとめる
    var child = new GameObject("Population - E" + this.GA.EpisodeCount + "G" + this.GA.GenerationCount);
    this.children.Add(child);
    ALPIndividual.SetBaseParent(child);
    Utils.SetParent(this.ChildrenParent, child);
    //GA実行
    this.GA.Update();
    //終了したか
    return this.GA.IsFinished;
  }

  /// <summary>
  /// １度の実行でMaxPrimitiveNumに達するまでエージェントを配置する．
  /// 重くてUnityが固まるので非推奨
  /// </summary>
  public void Run()
  {
    while (RunOneStep()) { }
  }

  /// <summary>
  /// GAを一度実行して，numAgentPerGA分，エージェントを配置する．
  /// </summary>
  /// <returns></returns>
  public bool RunOneStep()
  {
    //GAで配置を決める
    RunGA();

    //次の準備
    return PrepareNextStep();
  }

  /// <summary>
  /// 次のALPSを用意する．
  /// </summary>
  /// <returns></returns>
  public bool PrepareNextStep()
  {
    //配置の記録
    ALPIndividual[] eletes = this.GA.Population.SelectElete(this.NumAgentPerGA);
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();

    //エリートを追加する
    foreach (var elete_tmp in eletes)
    {
      var elete = (ALPIndividual)elete_tmp.Clone();
      if (elete == null)
      {
        Debug.Log("エリート個体がnullです");
        //GAをリフレッシュ
        this.GA.Discard();
        continue;
      }
      //エリート個体が制約を満たしているか確認
      if (!elete.MeetConstraint)
      {
        Debug.Log("この個体は制約を満たしていません！プンプン！ : " + elete.ToString());
        //GAをリフレッシュ
        this.GA.Discard();
        continue;
      }
      //同じ位置には置かないようにする
      bool flag = false;
      foreach(var ind in this.LocatedIndividual)
      {
        if (ind.Position == elete.Position)
        {
          flag = true;
          break;
        }
      }
      if (flag) continue;

      //エリート個体をビューに追加
      elete.CreateChildObject();
      elete.SetParent(this.EleteParent);
      elete.Prepare();
      this.LocatedIndividual.Add(elete);
      //CreateResultWithAddAgent(elete.Agent);
      agents.Add(elete.Agent);
    }

    //ボロノイ図を作成
    //CreateResult();
    CreateResultWithAddAgents(agents);

    //評価マップを更新
    PrepareToCapture();
    for (int i = 0; i < this.Sketches.Length; i++)
    {
      this.Sketches[i].Capture();
      //this.Sketches[i].EvaluationMap = GPGPUUtils.AlphaJoin2(this.Sketches[i].EvaluationMap, this.Sketches[i].ScanedImage, Attenuation);
      //this.Sketches[i].SetEvaluationMap(GPGPUUtils.AlphaJoin2(this.Sketches[i].EvaluationMap, this.Sketches[i].ScanedImage, Attenuation));
      //this.Sketches[i].SetEvaluationMap(GPGPUUtils.GPGPUTextureCopy(this.Sketches[i].ScanedImage));
    }
    AfterCapture();

    //スケッチの状態を表示
    //Debug.Log(Utils.GetArrayString(this.Sketches));

    //GAをリフレッシュ
    this.GA.Discard();

    //探索対象を切り換え
    //ALPIndividual.SearchExistential = !ALPIndividual.SearchExistential;
    //ALPIndividual.SearchExistential = this.LocatedIndividual.Count % 2 == 0;
    ALPIndividual.SearchExistential = this.LocatedIndividual.Count < (this.MaxPrimitiveNum / 2);

    //return this.NumOfLocatedPrimitives < this.MaxPrimitiveNum;
    return true;
  }

  /// <summary>
  /// 配置された点を使用して，3Dモデル化する
  /// </summary>
  public void CreateResult()
  {
    if (LocatedIndividual.Count < 4) return;
    //if (this.Result != null) this.Result.Discard();
    List<(Agent, ParticleObject)> agents = new List<(Agent, ParticleObject)>();
    foreach (var ind in this.LocatedIndividual) agents.Add(ind.Agent);
    this.Result.SetAgents(agents);
    this.Result.SetParent(this.Master);
    this.Result.RecreateVoronoi();
    this.Environment.VPModel = this.Result.VoronoiPartition;
  }

  /// <summary>
  /// 点を追加して，3Dモデル化する
  /// </summary>
  /// <param name="agent">点</param>
  public void CreateResultWithAddAgent((Agent, ParticleObject) agent)
  {
    this.Result.AddAgent(agent);
    this.Result.SetParent(this.Master);
  }

  /// <summary>
  /// 点の集合を追加して，3Dモデル化する
  /// </summary>
  /// <param name="agents">点の集合</param>
  public void CreateResultWithAddAgents(List<(Agent, ParticleObject)> agents)
  {
    this.Result.AddAgents(agents);
    this.Result.SetParent(this.Master);
  }

  /// <summary>
  /// 可視性に関するプロパティ
  /// </summary>
  bool isVisibleParticle;
  bool isVisibleDerauneyLine;
  bool isVisibleVoronoiLine;
  bool isVisibleVoronoiMesh;
  /// <summary>
  /// スケッチに3Dモデルと投影する準備をする．
  /// </summary>
  public void PrepareToCapture()
  {
    this.Result.SetLayer(LayerMask.NameToLayer("ForDrawable"));
    isVisibleParticle = this.Result.IsVisibleParticle;
    isVisibleDerauneyLine = this.Result.IsVisibleDerauneyLine;
    isVisibleVoronoiLine = this.Result.IsVisibleVoronoiLine;
    isVisibleVoronoiMesh = this.Result.IsVisibleVoronoiMesh;
    this.Result.SetVisible(false, false, false, true);
    this.Result.VoronoiPartition.SetMaterialVoronoiMesh(CaptureMaterial);
  }

  /// <summary>
  /// 3Dモデルを投影する前に状態を戻す．
  /// </summary>
  public void AfterCapture()
  {
    this.Result.SetLayer(0);
    this.Result.SetVisible(isVisibleParticle, isVisibleDerauneyLine, isVisibleVoronoiLine, isVisibleVoronoiMesh);
    this.Result.VoronoiPartition.SetMaterialVoronoiMesh(PreviewMaterial);
  }

  /// <summary>
  /// 環境にパラメータを設定する
  /// </summary>
  /// <param name="onSketchC">評価軸「スケッチ上」の重み</param>
  /// <param name="interferenceC">評価軸「3Dモデル（２次元投影）」の重み</param>
  /// <param name="existentialC">評価軸「存在か」の重み</param>
  /// <param name="onSketchAndInterferenceC">評価軸「スケッチ＆3Dモデル」の重み</param>
  /// <param name="changeVPC">評価軸「3Dモデル（３次元）」の重み</param>
  public void SetEnvironmentParam(float onSketchC, float interferenceC, float existentialC, float onSketchAndInterferenceC, float changeVPC)
  {
    this.Environment.OnSketchC = onSketchC;
    this.Environment.InterferenceC = interferenceC;
    this.Environment.ExistentialC = existentialC;
    this.Environment.OnSketchAndInterferenceC = onSketchAndInterferenceC;
    this.Environment.ChangeVPC = changeVPC;
  }

  /// <summary>
  /// マテリアルを設定
  /// </summary>
  /// <param name="capture">投影用マテリアル（不透明を推奨）</param>
  /// <param name="preview">描画用マテリアル（自由）</param>
  public static void SetMaterial(Material capture, Material preview)
  {
    AgentLocationProblemSolver.CaptureMaterial = capture;
    AgentLocationProblemSolver.PreviewMaterial = preview;
  }

  /// <summary>
  /// 3Dモデル化の結果を文字列で返す
  /// </summary>
  /// <returns>結果のまとめ</returns>
  public string GetResult()
  {
    //ストップウォッチ計測終了
    msw.Stop();
    float sumFitness = 0f;

    //評価値の合計の計算
    for(int i = 6; i < LocatedIndividual.Count; i++)
    {
      sumFitness += LocatedIndividual[i].Fitness;
    }

    //結果の作成
    string result = "【実行結果】\n";
    //評価値の表示
    result += "平均評価値：" + (sumFitness / LocatedIndividual.Count) + "\n";
    //実行時間の表示
    result += msw.GetResultString("実行時間") + "\n";
    //今回の条件の表示
    result += "【パラメータ】\n";
    result += "エピソード数　　　：" + this.GA.NumEpisode + "\n";
    result += "世代数　　　　　　：" + this.GA.NumGeneration + "\n";
    result += "集団サイズ　　　　：" + this.GA.PopulationSize + "\n";
    result += "エリート保存数　　：" + this.GA.NumSavedElete + "\n";
    result += "不正な個体の排除　：" + this.GA.RemoveInvalidIndividual + "\n";
    result += "突然変異確率　　　：" + this.GA.MutationRate + "\n";
    result += "親選択メソッド　　：" + System.Enum.GetName(typeof(SelectionMethod), this.GA.SelectionMethod) + "\n";
    result += "交叉メソッド　　　：" + System.Enum.GetName(typeof(CrossoverMethod), this.GA.CrossoverMethod) + "\n";
    result += "スケッチ数　　　　：" + this.Sketches.Length + "\n";
    result += "減衰率　　　　　　：" + this.Attenuation + "\n";
    result += "はみ出し許可割合　：" + this.AllowedStickOutPercentage + "\n";
    result += "プリミティブ配置数：" + this.MaxPrimitiveNum + "\n";
    //配置されたプリミティブの情報
    result += "【配置されたプリミティブ情報】\n";
    foreach (var individual in LocatedIndividual)
    {
      result += individual.GetGeneInfo() + "\n";
    }
    return result;
  }

  /// <summary>
  /// クラスのパラメータを文字列化
  /// </summary>
  /// <returns>パラメータ一覧</returns>
  public override string ToString()
  {
    string result = "LPSolver:";
    result += "NE-" + this.GA.NumEpisode + ", ";
    result += "NG-" + this.GA.NumGeneration + ", ";
    result += "PS-" + this.GA.PopulationSize + ", ";
    result += "NSE-" + this.GA.NumSavedElete + ", ";
    result += "RII-" + this.GA.RemoveInvalidIndividual + ", ";
    result += "MR-" + this.GA.MutationRate + ", ";
    result += "SM-" + System.Enum.GetName(typeof(SelectionMethod), this.GA.SelectionMethod) + ", ";
    result += "CM-" + System.Enum.GetName(typeof(CrossoverMethod), this.GA.CrossoverMethod) + ", ";
    result += "SL-" + this.Sketches.Length + ", ";
    result += "Att-" + this.Attenuation + ", ";
    result += "ASOP-" + this.AllowedStickOutPercentage + ", ";
    result += "MPN-" + this.MaxPrimitiveNum;
    return result;
  }
}

/// <summary>
/// AgentLocationProblemの環境を定義
/// 存在点と非存在点の位置を評価する．
/// </summary>
public class ALPEnvironment : MyGeneticAlgorithm.Environment
{
  //フィールド
  /// <summary>
  /// 入力スケッチ
  /// </summary>
  public Sketch[] Sketches;
  /// <summary>
  /// 3Dモデル
  /// </summary>
  public VoronoiPartitionFast VPModel;

  //パラメータ
  /// <summary>
  /// 評価軸「スケッチ上」の重み
  /// </summary>
  public float OnSketchC = 0.2f;
  /// <summary>
  /// 評価軸「3Dモデル（２次元投影）」の重み
  /// </summary>
  public float InterferenceC = 0.4f;
  /// <summary>
  /// 評価軸「存在か」の重み
  /// </summary>
  public float ExistentialC = 0.4f;
  /// <summary>
  /// 評価軸「スケッチ＆3Dモデル」の重み
  /// </summary>
  public float OnSketchAndInterferenceC = 0.8f;
  /// <summary>
  /// 評価軸「3Dモデル（３次元）」の重み
  /// </summary>
  public float ChangeVPC = 1f;

  /// <summary>
  /// 適応度の最終目標
  /// この適応度を超える個体が現れたらGAを強制終了する
  /// 現在は使用していない
  /// </summary>
  public float TerminateFitness = 100000f;
  /// <summary>
  /// 評価マップの減衰係数
  /// 現在は使用していない
  /// </summary>
  public float Attenuation = 0.5f;
  /// <summary>
  /// 3Dモデルがスケッチからはみ出せる許容割合
  /// 現在は使用していない
  /// </summary>
  public float AllowedStickOutPercentage;

  /// <summary>
  /// デフォルトコンストラクタ
  /// </summary>
  public ALPEnvironment() { }

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="sketches">入力スケッチ</param>
  /// <param name="vpm">3Dモデル化（ボロノイ図）のインスタンス</param>
  /// <param name="attenuation">評価マップの減衰係数</param>
  /// <param name="allowedStickOutPercentage">3Dモデルがスケッチからはみ出せる許容割合</param>
  public ALPEnvironment(Sketch[] sketches, VoronoiPartitionFast vpm, float attenuation, float allowedStickOutPercentage)
  {
    this.Sketches = sketches;
    this.VPModel = vpm;
    this.Attenuation = attenuation;
    this.AllowedStickOutPercentage = allowedStickOutPercentage;
  }

  /// <summary>
  /// 個体が制約に違反していないかチェックする．
  /// </summary>
  /// <param name="individual">チェックする個体</param>
  /// <returns>個体が制約を満たしているか．true:遵守，false:違反</returns>
  public override bool Check(Individual individual)
  {
    ALPIndividual aLPIndividual = (ALPIndividual)individual;
    //制約１：存在エージェントはスケッチの外側に出てはいけない
    if (aLPIndividual.IsExistential)
    {
      //各スケッチでチェック
      foreach(var sketch in this.Sketches)
      {
        var width = sketch.SketchImage.width;
        var height = sketch.SketchImage.height;

        //座標をカメラ座標に変換
        var point = Utils.WorldToScreen(aLPIndividual.Position, sketch.camera, width, height);
        int x = (int)point.x;
        int y = (int)point.y;

        //スケッチが存在するか確認
        bool isOnSketch = sketch.EvaluationMap.GetPixel(x, y).a > 0.01f;

        //スケッチが存在しなかったらOUT!
        if (!isOnSketch) return false;
      }
    }
    return true;
  }

  /// <summary>
  /// 個体の適応度を算出
  /// </summary>
  /// <param name="individual">個体</param>
  /// <returns>個体の適応度</returns>
  public override float GetFitness(Individual individual)
  {
    //時間計測
    MyStopwatch msw = new MyStopwatch();
    int cnt = 0;
    string result = "評価関数の実行時間\n";

    float fitness = 0f;
    ALPIndividual aLPIndividual = (ALPIndividual)individual;
    //aLPIndividual.PrepareToCapture();
    for (int i = 0; i < this.Sketches.Length; i++)
    {
      var sketch = this.Sketches[i];
      var width = sketch.SketchImage.width;
      var height = sketch.SketchImage.height;

      //座標をカメラ座標に変換
      var point = Utils.WorldToScreen(aLPIndividual.Position, sketch.camera, width, height);
      int x = (int)point.x;
      int y = (int)point.y;

      //描画マップに点を追加（デバッグ用）
      //sketch.ScanedImage = Utils.CreateMonocolorTexture(Color.white, sketch.camera.pixelWidth, sketch.camera.pixelHeight);
      //sketch.ScanedImage.SetPixel(x, y, Color.red);
      //Debug.Log(x + ", " + y);

      //評価軸「スケッチ上」：スケッチが存在するか確認
      bool isOnSketch = sketch.EvaluationMap.GetPixel(x, y).a > 0.01f;
      float os = OnSketchC * ((isOnSketch == aLPIndividual.IsExistential) ? 1f : 0f);

      //評価軸「3Dモデル（２次元投影）」：既に面が貼れてるor貼れてない場合評価
      bool isInterference = sketch.ScanedImage.GetPixel(x, y).a > 0.01f;
      float intf = InterferenceC * ((isInterference != aLPIndividual.IsExistential) ? 1f : 0f);

      //評価軸「スケッチ＆3Dモデル」：スケッチが存在し未だ面が貼れていないorスケッチが存在しないが面が貼られている
      float osandintf = OnSketchAndInterferenceC * ((
        !isOnSketch & isInterference & !aLPIndividual.IsExistential |
        isOnSketch & !isInterference & aLPIndividual.IsExistential
      ) ? 1f: 0f);

      //評価軸「存在か」：存在エージェントなら評価
      float e = ExistentialC * (aLPIndividual.IsExistential ? 1f : 0f);

      //線との距離を評価値に入れる．同じ距離に置きまくりたい．（未実装）

      //fitnessの計算
      fitness += os / this.Sketches.Length;
      fitness += intf / this.Sketches.Length;
      fitness += osandintf / this.Sketches.Length;
      fitness += e / this.Sketches.Length;
    }
    aLPIndividual.AfterCapture();

    //評価軸「3Dモデル（３次元）」：ボロノイ図に影響する点の場合評価
    if (ChangeVPC != 0f)
    {
      float cvp = ChangeVPC * (this.VPModel.CountAroundPoint(aLPIndividual.Agent) > 0 ? 1f : 0f);
      fitness += cvp;
    }

    //もし評価値が負になっていたら0にする
    if (fitness < 0) fitness = 0;
    //値が不正な場合もとりあえず0にする．
    if (float.IsPositiveInfinity(fitness) || float.IsNegativeInfinity(fitness)) fitness = 0;

    //Debug.Log(result);
    return fitness;
  }

  /// <summary>
  /// 環境の初期化
  /// </summary>
  public override void Initialize() { }

  /// <summary>
  /// 強制終了するべきかを判断
  /// </summary>
  /// <param name="individual">個体</param>
  /// <returns>強制終了するべきか．true:終了，false:続行</returns>
  public override bool Termination(Individual individual)
  {
    ALPIndividual lPIndividual = (ALPIndividual)individual;
    if (lPIndividual.Fitness > this.TerminateFitness)
    {
      return true;
    }
    return false;
  }
}

/// <summary>
/// ALPSで用いる個体クラス
/// 存在点と非存在点の２種類に分けられ，IsExistentialプロパティで表現
/// </summary>
public class ALPIndividual : Individual
{
  /// <summary>
  /// ビュー用エージェント
  /// 3Dモデル化にも使用
  /// </summary>
  public (Agent, ParticleObject) Agent;

  //プロパティ
  /// <summary>
  /// 位置
  /// </summary>
  public Vector3 Position { get; set; }
  /// <summary>
  /// 存在か
  /// </summary>
  public bool IsExistential { get; set; }

  /// <summary>
  /// 親オブジェクト
  /// </summary>
  public GameObject Parent;
  /// <summary>
  /// マスターオブジェクト
  /// </summary>
  private GameObject child;
  /// <summary>
  /// 親オブジェクトが設定されていなかった場合の親オブジェクト
  /// </summary>
  public static GameObject base_Parent;

  /// <summary>
  /// 遺伝子となるパラメータの名前リスト
  /// </summary>
  public string[] GeneNames = new string[] { "Position", "IsExistential" };

  //パラメータの上限と下限
  /// <summary>
  /// 位置の下限
  /// </summary>
  public static Vector3 POSITION_MIN = new Vector3(-10f, -10f, -10f);
  /// <summary>
  /// 位置の上限
  /// </summary>
  public static Vector3 POSITION_MAX = new Vector3(10f, 10f, 10f);

  //突然変異で変動するパラメータ値の幅
  /// <summary>
  /// 突然変異で変動する位置の幅
  /// </summary>
  public static Vector3 POSITION_MUTATION_RANGE = new Vector3(3f, 3f, 3f);

  //存在と非存在の探索の切り替え
  /// <summary>
  /// 探索の切り替え
  /// </summary>
  public static bool SearchExistential = true;

  /// <summary>
  /// 存在点のマテリアル
  /// </summary>
  public static Material ExistentialMaterial;
  /// <summary>
  /// 非存在点のマテリアル
  /// </summary>
  public static Material NonExistentialMaterial;

  /// <summary>
  /// 各交叉メソッドで生まれる個体数を返す
  /// </summary>
  public new Dictionary<CrossoverMethod, int> NumBornIndividual
  {
    get
    {
      return new Dictionary<CrossoverMethod, int>
        {
          {CrossoverMethod.OnePoint, 2},
          {CrossoverMethod.TwoPoint, 2},
          {CrossoverMethod.Uniform, 2}
        };
    }
  }

  /// <summary>
  /// デフォルトコンストラクタ
  /// </summary>
  public ALPIndividual() : base() { }

  /// <summary>
  /// 個体の初期化
  /// </summary>
  public override void Initialize()
  {
    Discard();
    this.Position = Utils.RandomRange(POSITION_MIN, POSITION_MAX);
    this.IsExistential = Utils.RandomBool(0.5f);
    //this.IsExistential = SearchExistential;
    CreateChildObject();
    SetParent(base_Parent);
    //Prepare();
  }

  /// <summary>
  /// 個体の複製（ディープコピー）
  /// </summary>
  /// <returns>複製したインスタンス</returns>
  public override object Clone()
  {
    ALPIndividual instance = new ALPIndividual();
    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(ALPIndividual).GetProperty(genename);
      gene.SetValue(instance, gene.GetValue(this));
    }
    instance.Fitness = this.Fitness;
    instance.MeetConstraint = this.MeetConstraint;
    return instance;
  }

  /// <summary>
  /// 適応度をセット
  /// 記録オブジェクトの名前を更新
  /// </summary>
  /// <param name="fitness">適応度</param>
  public new void SetFitness(float fitness)
  {
    base.SetFitness(fitness);
    SetChildName();
  }

  /// <summary>
  /// マスターオブジェクトを生成
  /// </summary>
  public void CreateChildObject()
  {
    this.child = new GameObject();
    SetChildName();
  }

  /// <summary>
  /// マスターオブジェクトの名前に個体情報を載せる
  /// </summary>
  public void SetChildName()
  {
    this.child.name = "Individual - " + this.GetGeneInfo();
  }

  /// <summary>
  /// 親オブジェクトが設定されてない時に代わりとなる親オブジェクトを設定する．
  /// </summary>
  /// <param name="parent">親オブジェクト</param>
  public static void SetBaseParent(GameObject parent)
  {
    ALPIndividual.base_Parent = parent;
  }

  /// <summary>
  /// 親オブジェクトを設定する
  /// </summary>
  /// <param name="parent">親オブジェクト</param>
  public void SetParent(GameObject parent)
  {
    //SetBaseParent(parent);
    this.Parent = parent;
    Utils.SetParent(parent, this.child);
    if (this.Agent.Item2 != null) this.Agent.Item2.SetParentObject(this.child);
  }

  /// <summary>
  /// GAの評価の前に実行
  /// 各パラメータやインスタンスを生成して，準備する．
  /// </summary>
  public override void Prepare()
  {
    Agent agent;
    ParticleObject obj;
    if (this.IsExistential)
    {
      agent = new ExistentialAgent();
      agent.Position = this.Position;
      obj = new ParticleObject(agent, ExistentialMaterial, this.child);
    }
    else
    {
      agent = new NonExistentialAgent();
      agent.Position = this.Position;
      obj = new ParticleObject(agent, NonExistentialMaterial, this.child);
    }

    obj.UpdateSphere();
    //obj.SetActive(true);
    //インスタンス確保
    this.Agent = (agent, obj);
  }

  /// <summary>
  /// ビューも含めて個体を破棄する．
  /// </summary>
  public override void Discard()
  {
    if(this.Agent.Item2 != null) this.Agent.Item2.Discard();
    this.Agent.Item2 = null;
    MonoBehaviour.Destroy(this.child);
  }

  /// <summary>
  /// 突然変異
  /// 全パラメータをランダムに初期化
  /// </summary>
  /// <param name="mutationRate">突然変異確率</param>
  public override void Mutation(float mutationRate)
  {
    if (Utils.RandomBool(mutationRate))
    {
      Initialize();
    }
  }

  /// <summary>
  /// 相対的突然変異
  /// パラメータを範囲内で変更する．
  /// </summary>
  public void MutationParameter()
  {
    //
    var position = this.Position + Utils.RandomRange(-POSITION_MUTATION_RANGE, POSITION_MUTATION_RANGE);
    if (position.x > POSITION_MAX.x) position.x = POSITION_MAX.x;
    if (position.y > POSITION_MAX.y) position.y = POSITION_MAX.y;
    if (position.z > POSITION_MAX.z) position.z = POSITION_MAX.z;
    if (position.x < POSITION_MIN.x) position.x = POSITION_MIN.x;
    if (position.y < POSITION_MIN.y) position.y = POSITION_MIN.y;
    if (position.z < POSITION_MIN.z) position.z = POSITION_MIN.z;
    this.Position = position;
    //this.IsExistential = Utils.RandomBool(0.5f);
  }

  public override Individual[] OnePointCrossoverWithMutation(Individual parent, float mutationRate)
  {
    throw new System.NotImplementedException();
  }

  public override Individual[] TwoPointCrossoverWithMutation(Individual parent, float mutationRate)
  {
    throw new System.NotImplementedException();
  }

  public override Individual[] UniformCrossoverWithMutation(Individual parent, float mutationRate)
  {
    int num = 2;
    ALPIndividual[] children = new ALPIndividual[num];
    for (int i = 0; i < num; i++)
    {
      children[i] = new ALPIndividual();
    }

    //親を突然変異させる
    var parent1 = (ALPIndividual)this.Clone();
    var parent2 = (ALPIndividual)parent.Clone();
    if (Utils.RandomBool(1f))
    {
      //どちらか片方を突然変異
      if (Utils.RandomBool(0.5f))
        parent1.MutationParameter();
      else
        parent2.MutationParameter();
      /*Debug.Log(string.Format("突然変異が起こりました．\n{0} --> {1}\n{2} --> {3}",
        this.ToString(),
        parent1.ToString(),
        parent.ToString(),
        parent2.ToString()
      ));*/
    }

    //各遺伝子ごとに一様に交差させる
    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(ALPIndividual).GetProperty(genename);
      if (Utils.RandomBool(0.5f))
      {
        gene.SetValue(children[0], gene.GetValue(parent1));
        gene.SetValue(children[1], gene.GetValue(parent2));
      }
      else
      {
        gene.SetValue(children[0], gene.GetValue(parent2));
        gene.SetValue(children[1], gene.GetValue(parent1));
      }
    }

    return children;
  }

  public override Individual[] OnePointCrossover(Individual parent)
  {
    throw new System.NotImplementedException();
  }

  public override Individual[] TwoPointCrossover(Individual parent)
  {
    throw new System.NotImplementedException();
  }

  public override Individual[] UniformCrossover(Individual parent)
  {
    int num = 2;
    ALPIndividual[] children = new ALPIndividual[num];
    for (int i = 0; i < num; i++)
    {
      children[i] = new ALPIndividual();
    }

    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(ALPIndividual).GetProperty(genename);
      //Debug.Log(genename + ", " + parent + ", " + individuals[0] + ", " + individuals[1]);
      if (Utils.RandomBool(0.5f))
      {
        gene.SetValue(children[0], gene.GetValue(parent));
        gene.SetValue(children[1], gene.GetValue(this));
      }
      else
      {
        gene.SetValue(children[0], gene.GetValue(this));
        gene.SetValue(children[1], gene.GetValue(parent));
      }
    }

    return children;
  }

  public override string GetGeneInfo()
  {
    string result = "";
    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(ALPIndividual).GetProperty(genename);
      result += genename + " : " + gene.GetValue(this) + ", ";
    }
    result += "Fitness : " + Fitness + ", ";
    result += "MeetConstraint : " + MeetConstraint + ", ";
    return result;
  }

  /// <summary>
  /// 遺伝子パラメータの一覧表示を文字列で返す．
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
    return GetGeneInfo();
  }

  /// <summary>
  /// マテリアルを設定
  /// </summary>
  /// <param name="existential">存在点のマテリアル</param>
  /// <param name="nonexistential">非存在点のマテリアル</param>
  public static void SetMaterial(Material existential, Material nonexistential)
  {
    ExistentialMaterial = existential;
    NonExistentialMaterial = nonexistential;
  }

  /// <summary>
  /// 投影前の準備
  /// </summary>
  public void PrepareToCapture()
  {
  }

  /// <summary>
  /// 投影後の処理
  /// </summary>
  public void AfterCapture()
  {
  }
}