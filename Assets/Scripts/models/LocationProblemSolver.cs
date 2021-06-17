using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGeneticAlgorithm;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;


//複数のスケッチから最適なプリミティブ配置を見つける
public class LocationProblemSolver
{
  public Sketch[] Sketches;
  //public Texture2D[] EvaluationMap;
  //public List<Primitive> LocatedPrimitives;
  public List<LPIndividual> LocatedIndividual;

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
  /// 現在配置済みのプリミティブ数
  /// </summary>
  public int NumOfLocatedPrimitives { get { return LocatedIndividual.Count; } }

  public bool IsFinished { get { return NumOfLocatedPrimitives >= MaxPrimitiveNum; } }

  private MyStopwatch msw;

  public GameObject Parent;
  public GameObject Master;
  public GameObject ChildrenParent;
  private GameObject EleteParent;
  private List<GameObject> children;

  public GeneticAlgorithm<LPEnvironment, LPIndividual> GA;

  public LocationProblemSolver() : this(null) { }

  public LocationProblemSolver(Sketch[] sketches) : this(100, 100, 10, 1, true, 0.2f, SelectionMethod.Roulette, CrossoverMethod.Uniform, sketches, 0.5f, 0.1f, 10) { }

  public LocationProblemSolver(
    int numEpisode, int numGeneration, int populationSize, int numSavedElete, bool removeInvalidIndividual,
    float mutationRate, SelectionMethod selectionMethod, CrossoverMethod crossoverMethod, Sketch[] sketches,
    float attenuation, float allowedStickOutPercentage, int maxPrimitiveNum
  )
  {
    //ストップウォッチ計測開始
    msw = new MyStopwatch();
    msw.Start();

    this.Sketches = sketches;
    for(int i = 0; i < this.Sketches.Length; i++)
    {
      //this.Sketches[i].EvaluationMap = GPGPUUtils.GPGPUTextureCopy(this.Sketches[i].SketchImage);
      this.Sketches[i].SetEvaluationMap(GPGPUUtils.GPGPUTextureCopy(this.Sketches[i].SketchImage));
    }
    //this.LocatedPrimitives = new List<Primitive>();
    this.LocatedIndividual = new List<LPIndividual>();
    this.Attenuation = attenuation;
    this.AllowedStickOutPercentage = allowedStickOutPercentage;
    this.MaxPrimitiveNum = maxPrimitiveNum;
    //環境インスタンスの生成
    LPEnvironment environment = new LPEnvironment(sketches, attenuation, allowedStickOutPercentage);
    /*if(sketches != null)
    {
      environment = new LPEnvironment(sketches);
    }
    else
    {
      environment = new LPEnvironment();
    }*/
    environment.Attenuation = attenuation;
    //GAインスタンスの生成
    this.GA = new GeneticAlgorithm<LPEnvironment, LPIndividual>(
      numEpisode, numGeneration, populationSize, numSavedElete, removeInvalidIndividual,
      mutationRate, selectionMethod, crossoverMethod, environment
    );

    this.Master = new GameObject(ToString());
    this.ChildrenParent = new GameObject("Populations");
    Utils.SetParent(this.Master, this.ChildrenParent);
    this.children = new List<GameObject>();
    this.children.Add(new GameObject("First Population"));
    foreach (var child in this.children)
      Utils.SetParent(this.ChildrenParent, child);
    this.EleteParent = new GameObject("Elete");
    Utils.SetParent(this.Master, this.EleteParent);
    //Debug.Log(this.children[0]);
    LPIndividual.SetBaseParent(this.children[0]);
    this.GA.Discard();
    this.GA.Initialize();
  }

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

  public void RunGA()
  {
    this.GA.Exec();
    //while (!RunGAOneStep()) { }
  }

  public bool RunGAOneStep()
  {
    //子オブジェクトにまとめる
    var child = new GameObject("Population - E" + this.GA.EpisodeCount + "G" + this.GA.GenerationCount);
    this.children.Add(child);
    LPIndividual.SetBaseParent(child);
    Utils.SetParent(this.ChildrenParent, child);
    //Debug.Log(this.children[this.GA.EpisodeCount * this.GA.GenerationCount + this.GA.GenerationCount + 1]);
    //Utils.PrintArray<GameObject>(this.children);
    //GA実行
    this.GA.Update();
    //終了したか
    //return this.GA.GenerationCount >= this.GA.NumGeneration;
    return this.GA.IsFinished;
  }

  public void Run()
  {
    while (RunOneStep()) { }
  }

  public bool RunOneStep()
  {
    //GAで配置を決める
    RunGA();

    //次の準備
    return PrepareNextStep();
  }

  public bool PrepareNextStep()
  {
    //配置の記録
    var elete = (LPIndividual)(this.GA.GetMostEleteIndividual().Clone());
    //this.LocatedPrimitives.Add(new Primitive(this.GA.GetMostEleteIndividual().Primitive));
    //記録をまとめる
    if (elete == null)
    {
      Debug.Log("エリート個体がnullです");
      //GAをリフレッシュ
      this.GA.Discard();
      return true;
    }
    //エリート個体が制約を見たいしているか確認
    if (!elete.MeetConstraint)
    {
      Debug.Log("この個体は制約を満たしていません！プンプン！ : " + elete.ToString());
      //GAをリフレッシュ
      this.GA.Discard();
      return true;
    }

    //エリート個体をビューに追加
    elete.CreateChildObject();
    elete.SetParent(this.EleteParent);
    this.LocatedIndividual.Add(elete);

    //スケッチの更新
    elete.Prepare();
    elete.PrepareToCapture();
    for (int i = 0; i < this.Sketches.Length; i++)
    {
      this.Sketches[i].Capture();
      //this.Sketches[i].EvaluationMap = GPGPUUtils.AlphaJoin2(this.Sketches[i].EvaluationMap, this.Sketches[i].ScanedImage, Attenuation);
      //this.Sketches[i].SetEvaluationMap(GPGPUUtils.AlphaJoin2(this.Sketches[i].EvaluationMap, this.Sketches[i].ScanedImage, Attenuation));
      this.Sketches[i].SetEvaluationMap(this.Sketches[i].AlphaJoinEMAndScaned2(Attenuation));
    }
    elete.AfterCapture();

    //スケッチの状態を表示
    Debug.Log(Utils.GetArrayString(this.Sketches));

    //GAをリフレッシュ
    this.GA.Discard();

    return this.NumOfLocatedPrimitives < this.MaxPrimitiveNum;
  }

  public string GetResult()
  {
    //ストップウォッチ計測終了
    msw.Stop();
    float sumFitness = 0f;

    //評価値の合計の計算
    foreach(var individual in LocatedIndividual)
    {
      sumFitness += individual.Fitness;
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
    foreach(var individual in LocatedIndividual)
    {
      result += individual.GetGeneInfo() + "\n";
    }
    return result;
  }

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

public class LPEnvironment : MyGeneticAlgorithm.Environment
{
  public Sketch[] Sketches;
  public float NonTransparentC = 0.4f;
  public float StickOutPenaltyC = 0.6f;
  public float TerminateFitness = 100000f;
  public float Attenuation = 0.5f;
  public float AllowedStickOutPercentage;

  public LPEnvironment() { }

  public LPEnvironment(Sketch[] sketches, float attenuation, float allowedStickOutPercentage)
  {
    this.Sketches = sketches;
    this.Attenuation = attenuation;
    this.AllowedStickOutPercentage = allowedStickOutPercentage;
  }

  public override bool Check(Individual individual)
  {
    LPIndividual lPIndividual = (LPIndividual)individual;
    //はみ出している部分がスケッチ全体のallowedStickOutPercentage以下なら制約クリア
    for (int i = 0; i < this.Sketches.Length; i++)
    {
      var IndPixelArea = lPIndividual.AreaPixels[i];
      var IndPixelSumOnSketch = lPIndividual.PixelSumOnSketch[i];

      Debug.Log(string.Format(
        "IndPixelArea : {0}\n" +
        "IndPixelSumOnSketch : {1}\n" +
        "constraint : {2}",
        IndPixelArea, IndPixelSumOnSketch, (float)(IndPixelArea - IndPixelSumOnSketch) / IndPixelArea
      ));

      if ((float)(IndPixelArea - IndPixelSumOnSketch) / IndPixelArea >= AllowedStickOutPercentage) return false;

      /*var sketch = this.Sketches[i];
      var sketchArea = sketch.EMAlphaSum;
      var areaSize = lPIndividual.AreaSize[i];
      var alphaSumOnSketch = lPIndividual.AlphaSumOnSketch[i];

      if ((areaSize - alphaSumOnSketch) / areaSize >= 0.5 || (areaSize - alphaSumOnSketch) / sketchArea >= AllowedStickOutPercentage)
        Debug.Log(string.Format("SketchArea : {0}\n" +
          "IndAreaSize : {1}\n" +
          "IndAlphaSum : {2}\n" +
          "constraint1 : {3}\n" +
          "constraint2 : {4}",
          sketchArea, areaSize, alphaSumOnSketch,
          (areaSize - alphaSumOnSketch) / areaSize,
          (areaSize - alphaSumOnSketch) / sketchArea
        ));

      //プリミティブがプリミティブの面積と比べて半分以上スケッチから出ていないかチェック
      if ((areaSize - alphaSumOnSketch) / areaSize >= 0.5) return false;

      //プリミティブがスケッチの面積と比べて半分以上スケッチから出ていないかチェック
      if ((areaSize - alphaSumOnSketch) / sketchArea >= AllowedStickOutPercentage) return false;*/
    }
    return true;
  }

  public override float GetFitness(Individual individual)
  {
    MyStopwatch msw = new MyStopwatch();
    int cnt = 0;
    string result = "評価関数の実行時間\n";

    float fitness = 0f;
    float ntp = 0f;
    LPIndividual lPIndividual = (LPIndividual)individual;
    lPIndividual.AreaSize = new float[Sketches.Length];
    lPIndividual.AlphaSumOnSketch = new float[Sketches.Length];
    lPIndividual.AreaPixels = new int[Sketches.Length];
    lPIndividual.PixelSumOnSketch = new int[Sketches.Length];
    lPIndividual.PrepareToCapture();
    for(int i = 0; i < this.Sketches.Length; i++)
    {
      var sketch = this.Sketches[i];

      result += "スケッチ" + ++cnt + "\n";
      msw.Restart();

      sketch.Capture();

      msw.Stop();
      result += msw.GetResultString("プリミティブの描画") + "\n";
      msw.Restart();

      //Texture2D tex = sketch.AlphaJoinSketchAndScaned();
      //Texture2D tex = sketch.AlphaJoin2(Attenuation);
      Texture2D tex = sketch.AlphaJoinEMAndScaned();      

      msw.Stop();
      result += msw.GetResultString("画像のブーリアン結合") + "\n";
      /*msw.Restart();

      ntp = NonTransparentC * Utils.CountNonTransparentPixel(tex);

      msw.Stop();
      result += msw.GetResultString("ピクセル数のカウント(逐次)(value=" + ntp + ")") + "\n";*/
      msw.Restart();

      //ntp = NonTransparentC * Utils.CountNonTransparentPixel_CPUParallel(tex);
      ntp = NonTransparentC * Utils.SumAlpha_CPUParallel(tex) / sketch.EMAlphaSum;
      if(sketch.EMAlphaSum == 0f)
      {
        ntp = 0;
      }

      //チェック用のプロパティの更新
      var scanedArea = Utils.CountPixelAndSumArea(sketch.ScanedImage);
      lPIndividual.AreaSize[i] = scanedArea.Item1;
      lPIndividual.AreaPixels[i] = scanedArea.Item2;
      var areaOnSketch = Utils.CountPixelAndSumArea(tex);
      lPIndividual.AlphaSumOnSketch[i] = areaOnSketch.Item1;
      lPIndividual.PixelSumOnSketch[i] = areaOnSketch.Item2;
/*      lPIndividual.AreaSize[i] = Utils.SumAlpha_CPUParallel(sketch.ScanedImage);
      lPIndividual.AlphaSumOnSketch[i] = Utils.SumAlpha_CPUParallel(tex);
      lPIndividual.AreaPixels[i] = Utils.CountNonTransparentPixel_CPUParallel(sketch.ScanedImage);
      lPIndividual.PixelSumOnSketch[i] = Utils.CountNonTransparentPixel(tex);*/

      msw.Stop();
      result += msw.GetResultString("ピクセル数のカウント(並列)(value=" + ntp + ")") + "\n";

      //はみ出した量，負の報酬を与える
      var IndPixelArea = lPIndividual.AreaPixels[i];
      var IndPixelSumOnSketch = lPIndividual.PixelSumOnSketch[i];
      var sop = StickOutPenaltyC * (float)(IndPixelArea - IndPixelSumOnSketch) / IndPixelArea;
      /*Debug.Log(string.Format(
        "IndPixelArea : {0}\n" +
        "IndPixelSumOnSketch : {1}\n" +
        "ntp : {3}\n" +
        "sop : {2}",
        IndPixelArea, IndPixelSumOnSketch, sop, ntp
      ));*/

      fitness += ntp / this.Sketches.Length;
      fitness -= sop / this.Sketches.Length;

      //テクスチャの開放
      MonoBehaviour.Destroy(tex);
    }
    lPIndividual.AfterCapture();

    //もし評価値が負になっていたら0にする
    if (fitness < 0) fitness = 0;

    //Debug.Log(result);
    return fitness;
  }

  public override void Initialize() { }

  public override bool Termination(Individual individual)
  {
    LPIndividual lPIndividual = (LPIndividual)individual;
    if(lPIndividual.Fitness > this.TerminateFitness)
    {
      return true;
    }
    return false;
  }
}

public class LPIndividual : Individual
{
  public Primitive Primitive;
  public float Size { get; set; }
  public Vector3 Position { get; set; }
  public Quaternion Rotation { get; set; }
  public int SepH { get; set; }
  public int SepV { get; set; }

  //評価用プロパティ
  public float[] AreaSize;          //スケッチ視点ごとのプリミティブのアルファ面積
  public float[] AlphaSumOnSketch;  //スケッチ視点ごとのスケッチ上のアルファ合計値
  public int[] AreaPixels;        //スケッチ視点ごとのプリミティブのピクセル面積
  public int[] PixelSumOnSketch;  //スケッチ視点ごとのスケッチ上のピクセル合計値

  public GameObject Parent;
  private GameObject child;

  public static GameObject base_Parent;

  public string[] GeneNames = new string[] { "Size", "Position", "Rotation", "SepH", "SepV" };

  //パラメータの上限と下限
  /*  public static float SIZE_MIN = 1f;
    public static float SIZE_MAX = 5f;
    public static Vector3 POSITION_MIN = new Vector3(-10f, -10f, -10f);
    public static Vector3 POSITION_MAX = new Vector3( 10f,  10f,  10f);
    public static int SEP_H_MIN = 3;
    public static int SEP_H_MAX = 6;
    public static int SEP_V_MIN = 2;
    public static int SEP_V_MAX = 5;*/
  public static float SIZE_MIN = 2f;
  public static float SIZE_MAX = 4f;
  public static Vector3 POSITION_MIN = new Vector3(-10f, -10f, -10f);
  public static Vector3 POSITION_MAX = new Vector3(10f, 10f, 10f);
  public static int SEP_H_MIN = 6;
  public static int SEP_H_MAX = 6;
  public static int SEP_V_MIN = 6;
  public static int SEP_V_MAX = 6;

  //突然変異で変動するパラメータ値の幅
  public static float SIZE_MUTATION_RANGE = 3f;
  public static Vector3 POSITION_MUTATION_RANGE = new Vector3(3f, 3f, 3f);
  public static Vector3 ROTATION_MUTATION_RANGE = new Vector3(30f, 30f, 30f); //オイラー角度
  public static int SEP_H_MUTATION_RANGE = 2;
  public static int SEP_V_MUTATION_RANGE = 2;

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

  public LPIndividual() : base() { }

  public override void Initialize()
  {
    Discard();
    this.Size = Utils.RandomRange(SIZE_MIN, SIZE_MAX);
    this.Position = Utils.RandomRange(POSITION_MIN, POSITION_MAX);
    this.Rotation = Quaternion.Euler(Utils.RandomRange(new Vector3(0f, 0f, 0f), new Vector3(360f, 360f, 360f)));
    this.SepH = Utils.RandomRange(SEP_H_MIN, SEP_H_MAX + 1);
    this.SepV = Utils.RandomRange(SEP_V_MIN, SEP_V_MAX + 1);
    CreateChildObject();
    SetParent(base_Parent);
    //Prepare();
  }

  public override object Clone()
  {
    LPIndividual instance = new LPIndividual();
    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(LPIndividual).GetProperty(genename);
      gene.SetValue(instance, gene.GetValue(this));
    }
    instance.Fitness = this.Fitness;
    instance.MeetConstraint = this.MeetConstraint;
    return instance;
  }

  public new void SetFitness(float fitness)
  {
    base.SetFitness(fitness);
    SetChildName();
  }

  public void CreateChildObject()
  {
    this.child = new GameObject();
    SetChildName();
  }

  public void SetChildName()
  {
    this.child.name = "Individual - " + this.GetGeneInfo();
  }

  public static void SetBaseParent(GameObject parent)
  {
    LPIndividual.base_Parent = parent;
  }

  public void SetParent(GameObject parent)
  {
    //SetBaseParent(parent);
    this.Parent = parent;
    Utils.SetParent(parent, this.child);
    if(this.Primitive != null) this.Primitive.SetParent(this.child);
  }

  public override void Prepare()
  {
    //Debug.Log(Parent);
    //Debug.Log(base_Parent);
    this.Primitive = new Primitive(this.Size, this.Position, this.Rotation, this.SepH, this.SepV);
    this.Primitive.UpdateParticleObject();
    this.Primitive.CreateVoronoi();
    this.Primitive.SetParent(this.child);
  }

  public override void Discard()
  {
    if(this.Primitive != null) this.Primitive.Discard();
    this.Primitive = null;
    MonoBehaviour.Destroy(this.child);
  }

  public override void Mutation(float mutationRate)
  {
    if (Utils.RandomBool(mutationRate))
    {
      Initialize();
    }
  }

  public void MutationParameter()
  {
    this.Size += Utils.RandomRange(-SIZE_MUTATION_RANGE, SIZE_MUTATION_RANGE);
    if (this.Size > SIZE_MAX) this.Size = SIZE_MAX;
    if (this.Size < SIZE_MIN) this.Size = SIZE_MIN;
    var position = this.Position + Utils.RandomRange(-POSITION_MUTATION_RANGE, POSITION_MUTATION_RANGE);
    if (position.x > POSITION_MAX.x) position.x = POSITION_MAX.x;
    if (position.y > POSITION_MAX.y) position.y = POSITION_MAX.y;
    if (position.z > POSITION_MAX.z) position.z = POSITION_MAX.z;
    if (position.x < POSITION_MIN.x) position.x = POSITION_MIN.x;
    if (position.y < POSITION_MIN.y) position.y = POSITION_MIN.y;
    if (position.z < POSITION_MIN.z) position.z = POSITION_MIN.z;
    this.Position = position;
    this.Rotation = this.Rotation * Quaternion.Euler(Utils.RandomRange(-ROTATION_MUTATION_RANGE, ROTATION_MUTATION_RANGE));
    this.SepH += Utils.RandomRange(-SEP_H_MUTATION_RANGE, SEP_H_MUTATION_RANGE + 1);
    if (this.SepH > SEP_H_MAX) this.SepH = SEP_H_MAX;
    if (this.SepH < SEP_H_MIN) this.SepH = SEP_H_MIN;
    this.SepV += Utils.RandomRange(-SEP_V_MUTATION_RANGE, SEP_V_MUTATION_RANGE + 1);
    if (this.SepV > SEP_V_MAX) this.SepV = SEP_V_MAX;
    if (this.SepV < SEP_V_MIN) this.SepV = SEP_V_MIN;
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
    LPIndividual[] children = new LPIndividual[num];
    for (int i = 0; i < num; i++)
    {
      children[i] = new LPIndividual();
    }

    //親を突然変異させる
    var parent1 = (LPIndividual)this.Clone();
    var parent2 = (LPIndividual)parent.Clone();
    if (Utils.RandomBool(1f))
    {
      //Debug.Log("突然変異" + mutationRate);
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

    foreach (var genename in this.GeneNames)
    {
      var gene = typeof(LPIndividual).GetProperty(genename);
      //Debug.Log(genename + ", " + parent + ", " + individuals[0] + ", " + individuals[1]);
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
    LPIndividual[] children = new LPIndividual[num];
    for(int i = 0; i < num; i++)
    {
      children[i] = new LPIndividual();
    }

    foreach(var genename in this.GeneNames)
    {
      var gene = typeof(LPIndividual).GetProperty(genename);
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
      var gene = typeof(LPIndividual).GetProperty(genename);
      result += genename + " : " + gene.GetValue(this) + ", ";
    }
    result += "Fitness : " + Fitness + ", ";
    result += "MeetConstraint : " + MeetConstraint + ", ";
    result += "AreaSize : " + Utils.GetArrayStringNonReturn(AreaSize) + ", ";
    result += "AlphaSumOnSketch : " + Utils.GetArrayStringNonReturn(AlphaSumOnSketch);
    return result;
  }

  public override string ToString()
  {
    return GetGeneInfo();
  }

  private bool isVisibleParticle;
  private bool isVisibleDerauneyLine;
  private bool isVisibleVoronoiLine;
  private bool isVisibleVoronoiMesh;
  public static Material BaseMeshMaterial;
  public static Material CaptureMaterial;

  public static void SetMaterial(Material baseMesh, Material capture)
  {
    BaseMeshMaterial = baseMesh;
    CaptureMaterial = capture;
  }

  public void PrepareToCapture()
  {
    this.Primitive.SetLayer(LayerMask.NameToLayer("ForDrawable"));
    isVisibleParticle = this.Primitive.IsVisibleParticle;
    isVisibleDerauneyLine = this.Primitive.IsVisibleDerauneyLine;
    isVisibleVoronoiLine = this.Primitive.IsVisibleVoronoiLine;
    isVisibleVoronoiMesh = this.Primitive.IsVisibleVoronoiMesh;
    this.Primitive.SetVisible(false, false, false, true);
    this.Primitive.VoronoiPartition.SetMaterialVoronoiMesh(CaptureMaterial);
  }

  public void AfterCapture()
  {
    this.Primitive.SetLayer(0);
    this.Primitive.SetVisible(isVisibleParticle, isVisibleDerauneyLine, isVisibleVoronoiLine, isVisibleVoronoiMesh);
    this.Primitive.VoronoiPartition.SetMaterialVoronoiMesh(BaseMeshMaterial);
  }
}