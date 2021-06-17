using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Build.Reporting;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// 遺伝的アルゴリズムをまとめた俺的クラス
/// </summary>
namespace MyGeneticAlgorithm
{
  /// <summary>
  /// 親選択メソッド群
  /// </summary>
  public enum SelectionMethod
  {
    Roulette,   //ルーレット選択
    Tournament, //トーナメント選択
    Ranking     //ランキング選択
  }

  /// <summary>
  /// 交叉メソッド群
  /// </summary>
  public enum CrossoverMethod
  {
    OnePoint,   //一点交叉
    TwoPoint,   //二点交叉
    Uniform     //一様交叉
  }

  /// <summary>
  /// 遺伝的アルゴリズムを実行するクラス
  /// </summary>
  /// <typeparam name="TEnvironment">環境クラスの子クラス</typeparam>
  /// <typeparam name="TIndividual">個体クラスの子クラス</typeparam>
  public class GeneticAlgorithm <TEnvironment, TIndividual>
    where TEnvironment : Environment, new()
    where TIndividual : Individual, new()
  {
    /// <summary>
    /// 実行するエピソード数
    /// </summary>
    public int NumEpisode;
    /// <summary>
    /// １エピソード当たりの世代数
    /// </summary>
    public int NumGeneration;
    /// <summary>
    /// １世代あたりの個体数
    /// </summary>
    public int PopulationSize;
    /// <summary>
    /// １世代あたりに選択する個体数
    /// </summary>
    //public int NumSelectePopulation;
    /// <summary>
    /// １世代あたりに保存するエリート数
    /// </summary>
    public int NumSavedElete;
    /// <summary>
    /// 制約を見たいしていない個体を集団から排除するかどうか
    /// </summary>
    public bool RemoveInvalidIndividual;
    /// <summary>
    /// 突然変異する確率
    /// </summary>
    public float MutationRate;
    /// <summary>
    /// 親選択メソッド
    /// </summary>
    public SelectionMethod SelectionMethod;
    /// <summary>
    /// 交叉メソッド
    /// </summary>
    public CrossoverMethod CrossoverMethod;

    /// <summary>
    /// 環境
    /// </summary>
    public TEnvironment Environment;
    /// <summary>
    /// 集団
    /// </summary>
    public Population<TIndividual> Population;

    /// <summary>
    /// 現在のエピソード数
    /// </summary>
    public int EpisodeCount { get; private set; }
    /// <summary>
    /// 現在の世代数
    /// </summary>
    public int GenerationCount { get; private set; }
    /// <summary>
    /// GAで１エピソードが終了するタイミングでTrue
    /// </summary>
    public bool IsTerminated { get; private set; }

    /// <summary>
    /// GAが終了したか
    /// </summary>
    public bool IsFinished { get { return this.EpisodeCount >= this.NumEpisode; } }
    /// <summary>
    /// 優秀な個体の記録
    /// </summary>
    public List<TIndividual> ExcellentIndividuals { get; private set; }

    /// <summary>
    /// デフォルトコンストラクタ
    /// たぶん使うことはない
    /// </summary>
    public GeneticAlgorithm()
    {
      this.EpisodeCount = 0;
      this.ExcellentIndividuals = new List<TIndividual>();
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="numEpisode">エピソード数</param>
    /// <param name="numGeneration">世代数</param>
    /// <param name="populationSize">集団サイズ</param>
    /// <param name="numSavedElete">エリート保存数</param>
    /// <param name="mutationRate">突然変異率</param>
    /// <param name="selectionMethod">親選択メソッド</param>
    /// <param name="crossoverMethod">交叉メソッド</param>
    public GeneticAlgorithm(
      int numEpisode, int numGeneration, int populationSize, int numSavedElete, bool removeInvalidIndividual,
      float mutationRate, SelectionMethod selectionMethod, CrossoverMethod crossoverMethod
    ) : this(numEpisode, numGeneration, populationSize, numSavedElete, removeInvalidIndividual,
      mutationRate, selectionMethod, crossoverMethod, new TEnvironment()){ }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="numEpisode">エピソード数</param>
    /// <param name="numGeneration">世代数</param>
    /// <param name="populationSize">集団サイズ</param>
    /// <param name="numSavedElete">エリート保存数</param>
    /// <param name="removeInvalidIndividual"></param>
    /// <param name="mutationRate">突然変異率</param>
    /// <param name="selectionMethod">親選択メソッド</param>
    /// <param name="crossoverMethod">交叉メソッド</param>
    /// <param name="environment">環境インスタンス</param>
    public GeneticAlgorithm(
      int numEpisode, int numGeneration, int populationSize, int numSavedElete, bool removeInvalidIndividual,
      float mutationRate, SelectionMethod selectionMethod, CrossoverMethod crossoverMethod, TEnvironment environment
    ) : this()
    {
      //パラメータの設定
      this.NumEpisode = numEpisode;
      this.NumGeneration = numGeneration;
      this.PopulationSize = populationSize;
      this.NumSavedElete = numSavedElete;
      this.RemoveInvalidIndividual = removeInvalidIndividual;
      this.MutationRate = mutationRate;
      this.SelectionMethod = selectionMethod;
      this.CrossoverMethod = crossoverMethod;
      //インスタンスの生成
      this.Environment = environment;
      this.Population = new Population<TIndividual>(populationSize);
      //初期化
      Initialize();
    }

    /// <summary>
    /// エピソード開始時の初期化
    /// </summary>
    public void Initialize()
    {
      //環境・集団の初期化
      this.Environment.Initialize();
      this.Population.Initialize();
      //カウンタの初期化
      this.GenerationCount = 0;
      this.IsTerminated = false;
    }

    /// <summary>
    /// 経過を観察するためのアップデート関数
    /// 1世代進める
    /// </summary>
    public void Update()
    {
      //全エピソードが終了したか
      if (this.EpisodeCount >= this.NumEpisode)
      {
        Reset();
        //Debug.Log("episode - " + this.EpisodeCount);
      }
      if (this.IsTerminated)
      {
        Initialize();
      }
      //1世代実行
      ExecOneGeneration();
      //エピソードが終了しているかどうか
      if (this.IsTerminated)
      {
        SaveExcellentIndividual();
        this.EpisodeCount++;
        this.IsTerminated = true;
        //Debug.Log(this.EpisodeCount + "世代：エリート個体\n" + Utils.GetArrayString<TIndividual>(this.ExcellentIndividuals));
      }
    }

    public void Reset()
    {
      this.EpisodeCount = 0;
      ClearExcellentIndividual();
    }

    /// <summary>
    /// GAを実行する
    /// </summary>
    public void Exec()
    {
      //前回の記録の破棄
      Reset();
      //全エピソードを実行
      for(int i = 0; i < this.NumEpisode; i++)
      {
        ExecOneEpisode();
        Debug.Log("episode - " + i);
      }
    }

    /// <summary>
    /// 集団を破棄する
    /// </summary>
    public void Discard()
    {
      this.Population.Discard();
    }

    /// <summary>
    /// 1エピソード実行する
    /// </summary>
    private void ExecOneEpisode()
    {
      Initialize();
      for(int i = 0; i < NumGeneration; i++)
      {
        ExecOneGeneration();
        Debug.Log("generation - " + i + " is terminated : " + this.IsTerminated);
        if (this.IsTerminated) break;
      }
      SaveExcellentIndividual();
      //Utils.PrintArray<TIndividual>(this.ExcellentIndividuals);
      //エピソード数のカウント
      this.EpisodeCount++;
      Debug.Log(this.EpisodeCount + "世代：エリート個体\n" + Utils.GetArrayString<TIndividual>(this.ExcellentIndividuals));
    }

    /// <summary>
    /// 1世代実行する
    /// </summary>
    private void ExecOneGeneration()
    {
      string result = "1世代の実行時間\n";
      MyStopwatch allmsw = new MyStopwatch();
      MyStopwatch onemsw = new MyStopwatch();
      allmsw.Start();

      //評価
      onemsw.Start();
      foreach(var Individual in this.Population.Individuals)
      {
        //Individual.Prepare();
        Individual.SetFitness(Environment.GetFitness(Individual));
      }
      onemsw.Stop();
      result += onemsw.GetResultString("評価") + "\n";

      //個体チェック
      onemsw.Restart();
      foreach (var individual in this.Population.Individuals)
      {
        individual.MeetConstraint = this.Environment.Check(individual);
      }
      onemsw.Stop();
      result += onemsw.GetResultString("個体チェック") + "\n";

      //Debug.Log("E" + this.EpisodeCount + "G" + this.GenerationCount + " - 現在の個体\n" + this.Population.GetIndividualsInfo());

      //終了チェック
      onemsw.Restart();
      this.IsTerminated = this.Environment.Termination(this.Population.GetTopIndividual());
      onemsw.Stop();
      result += onemsw.GetResultString("終了") + "\n";
      if (this.IsTerminated || this.NumGeneration - 1 <= this.GenerationCount)
      {
        //Debug.Log("世代終了");
        this.IsTerminated = true;
        return;
      }

      //選択
      onemsw.Restart();
      TIndividual[] selected_individuals = this.Population.Selection(this.SelectionMethod, this.CrossoverMethod);
      onemsw.Stop();
      result += onemsw.GetResultString("選択" + selected_individuals.Length) + "\n";

      //Debug.Log("選択された個体\n" + Utils.GetArrayString(selected_individuals));

      //交叉
      onemsw.Restart();
      var next_individuals = new List<TIndividual>();
      //Debug.Log("ni : " + next_individuals.Count + ", si : " + selected_individuals.Length);
      //Utils.PrintArray<TIndividual>(selected_individuals);
      for(int i = 0; i < selected_individuals.Length; i++)
      {
        for(int j = i + 1; j < selected_individuals.Length; j++)
        {
          //next_individuals.AddRange((IEnumerable<TIndividual>)selected_individuals[i].Crossover(selected_individuals[j], this.CrossoverMethod));
          next_individuals.AddRange(
            (IEnumerable<TIndividual>)selected_individuals[i].CrossoverWithMutation(
              selected_individuals[j], this.CrossoverMethod, this.MutationRate
            )
          );
        }
      }
      onemsw.Stop();
      result += onemsw.GetResultString("交叉") + "\n";

      //突然変異
      for(int i = 0; i < next_individuals.Count; i++)
      {
        next_individuals[i].Mutation(this.MutationRate);
      }

      //突然変異
/*      onemsw.Restart();
      foreach (var individual in next_individuals)
      {
        individual.Mutation(this.MutationRate);
      }
      onemsw.Stop();
      result += onemsw.GetResultString("突然変異") + "\n";*/

      //エリート保存
      onemsw.Restart();
      TIndividual[] elete_individuals = this.Population.SelectElete(this.NumSavedElete);
      onemsw.Stop();
      result += onemsw.GetResultString("エリート保存") + "\n";

      /*
      //個体チェック
      onemsw.Restart();
      foreach (var individual in next_individuals)
      {
        individual.MeetConstraint = this.Environment.Check(individual);
      }
      onemsw.Stop();
      result += onemsw.GetResultString("個体チェック" + next_individuals.Count) + "\n";
      */

      //数合わせ
      onemsw.Restart();

      //もし次世代個体数が集団サイズ-保存エリート数に満たなかったらエラー
      if (next_individuals.Count < this.PopulationSize - this.NumSavedElete)
        throw new System.Exception(
          "GeneticAlgorithm : 生成された個体が足りません\n" +
          "Population.Individuals : " + this.Population.Individuals.Length + "\n" +
          "selected_individuals : " + selected_individuals.Length + "\n" +
          "next_individuals : " + next_individuals.Count
        );

      //次世代個体を集団サイズ-保存エリート数まで減らす．
      while(next_individuals.Count > this.PopulationSize - this.NumSavedElete)
      {
        next_individuals.RemoveAt(Utils.RandomRange(0, next_individuals.Count));
      }
      onemsw.Stop();
      result += onemsw.GetResultString("数合わせ" + next_individuals.Count) + "\n";

      //エリート個体を追加する
      onemsw.Restart();
      foreach (var elete in elete_individuals)
      {
        next_individuals.Add((TIndividual)elete.Clone());
      }

      //世代数のカウント
      this.GenerationCount++;

      //世代交代
      this.Population.Discard();  //集団の破棄
      this.Population = new Population<TIndividual>(this.GenerationCount, next_individuals.ToArray());
      onemsw.Stop();
      result += onemsw.GetResultString("その他" + next_individuals.Count) + "\n";
      allmsw.Stop();
      result += allmsw.GetResultString("1世代の時間合計");
      //Debug.Log(result);
      //Debug.Log(this.EpisodeCount + "エピソード目，" + this.GenerationCount + "世代\n" + this.Population.GetIndividualsInfo());
    }

    /// <summary>
    /// 現在一番優秀な個体を記録する
    /// </summary>
    private void SaveExcellentIndividual()
    {
      this.ExcellentIndividuals.Add(this.Population.GetTopIndividual());
    }

    /// <summary>
    /// 個体の記録を破棄する
    /// </summary>
    private void ClearExcellentIndividual()
    {
      this.ExcellentIndividuals = new List<TIndividual>();
    }

    public TIndividual GetMostEleteIndividual()
    {
      float max = float.MinValue;
      TIndividual elete = ExcellentIndividuals[0];
      foreach(var individual in this.ExcellentIndividuals)
      {
        individual.Fitness = Environment.GetFitness(individual);
        if(individual.Fitness > max)
        {
          max = individual.Fitness;
          elete = individual;
        }
      }
      return elete;
    }
  }

  /// <summary>
  /// 環境クラス
  /// </summary>
  public abstract class Environment
  {
    /// <summary>
    /// エピソード開始時の初期化
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// 評価関数
    /// </summary>
    /// <param name="individual">個体インスタンス</param>
    /// <returns>評価値</returns>
    public abstract float GetFitness(Individual individual);

    /// <summary>
    /// 個体が制約を満たしているかチェックする
    /// </summary>
    /// <param name="individual">個体インスタンス</param>
    /// <returns>制約を満たしているかどうか</returns>
    public abstract bool Check(Individual individual);

    /// <summary>
    /// 遺伝的アルゴリズムの終了条件を記述
    /// </summary>
    /// <param name="individual">その世代の最も評価の高い個体</param>
    /// <returns>終了かどうか</returns>
    public abstract bool Termination(Individual individual);
  }

  /// <summary>
  /// 集団クラス
  /// </summary>
  /// <typeparam name="TIndividual">個体クラスの子クラス</typeparam>
  public class Population<TIndividual>
    where TIndividual : Individual, new()
  {
    public int Generation;
    public TIndividual[] Individuals;
    public int PopulationSize;

    /// <summary>
    /// デフォルトコンストラクタ
    /// 使うことはない
    /// </summary>
    public Population()
    {
      this.Generation = 0;
    }

    /// <summary>
    /// コンストラクタ
    /// 初期生成を同時にする
    /// </summary>
    /// <param name="populationSize">集団サイズ</param>
    public Population(int populationSize) : this()
    {
      this.PopulationSize = populationSize;
      GenerateIndividuals(populationSize);
    }

    /// <summary>
    /// コンストラクタ
    /// 新しい世代用
    /// </summary>
    /// <param name="generation">何世代目か</param>
    /// <param name="individuals">新しい世代の個体</param>
    public Population(int generation, TIndividual[] individuals)
    {
      this.Generation = generation;
      this.Individuals = individuals;
      this.PopulationSize = individuals.Length;
      foreach(var individual in individuals)
      {
        individual.Prepare();
      }
    }

    /// <summary>
    /// エピソード開始時の初期化
    /// </summary>
    public void Initialize()
    {
      //Discard();
      this.Generation = 0;
      foreach(var individual in this.Individuals)
      {
        individual.Initialize();
        individual.Prepare();
      }
    }

    /// <summary>
    /// 集団の終了処理をする
    /// </summary>
    public void Discard()
    {
      foreach (var individual in this.Individuals)
      {
        individual.Discard();
      }
    }

    /// <summary>
    /// 指定されたサイズの集団を生成する
    /// </summary>
    /// <param name="populationSize">集団サイズ</param>
    public void GenerateIndividuals(int populationSize)
    {
      this.Individuals = new TIndividual[populationSize];
      for (int i = 0; i < populationSize; i++)
      {
        this.Individuals[i] = new TIndividual();
      }
    }

    /// <summary>
    /// 親選択をする
    /// 交叉方法によって選択数が異なるため，交叉方法も引数でとる
    /// </summary>
    /// <param name="selectionMethod">親選択メソッド</param>
    /// <param name="crossoverMethod">交叉メソッド</param>
    /// <returns>親個体の集合</returns>
    public TIndividual[] Selection(SelectionMethod selectionMethod, CrossoverMethod crossoverMethod)
    {
      List<TIndividual> parents = new List<TIndividual>();

      int select_num = 1;
      int genCrssovr = (new TIndividual()).NumBornIndividual[crossoverMethod];  //これしか方法が無かったんや…

      //選択数を計算
      select_num = Mathf.CeilToInt((1f + Mathf.Pow(1f + 8f * this.PopulationSize / genCrssovr, 0.5f)) / 2f);

      //制約を満たしている子の集合を作成
      var mcInds = new List<TIndividual>();
      foreach (var individual in this.Individuals)
        if (individual.MeetConstraint)
          mcInds.Add(individual);
      var meetConstraintIndividuals = mcInds.ToArray();

      //上記の集合が0の場合しょうがないので全体から選択する
      if (meetConstraintIndividuals.Length == 0)
        meetConstraintIndividuals = this.Individuals;

      //親選択
      if (selectionMethod == SelectionMethod.Roulette)
        parents.AddRange(RouletteSelection(meetConstraintIndividuals, select_num));
      else if (selectionMethod == SelectionMethod.Tournament)
        parents.AddRange(TournamentSelection(meetConstraintIndividuals, select_num));
      else if (selectionMethod == SelectionMethod.Ranking)
        parents.AddRange(RankingSelection(meetConstraintIndividuals, select_num));
      else
        parents.AddRange(RouletteSelection(meetConstraintIndividuals, select_num));

      //親が足りない場合，適当に追加する
      if (parents.Count < select_num)
        Debug.Log("追加します : " + parents.Count + " -> " + select_num);
      while(parents.Count < select_num)
      {
        parents.Add(this.Individuals[Utils.RandomRange(0, this.Individuals.Length)]);
      }

      return parents.ToArray();
    }

    /// <summary>
    /// ルーレット選択による親選択(未実装)
    /// </summary>
    /// <param name="individuals">親選択をする個体の集合</param>
    /// <param name="select_num">選択する親の数</param>
    /// <returns>親個体</returns>
    public TIndividual[] RouletteSelection(TIndividual[] individuals, int select_num)
    {
      TIndividual[] parents = new TIndividual[select_num];
      //評価値の合計を求める
      float fitSum = 0f;
      foreach (var individual in individuals)
        fitSum += individual.Fitness;
      if (fitSum == 0f) fitSum = 1;
      //親の選択
      for (int i = 0; i < select_num; i++)
      {
        //ルーレット開始
        float roulette = Utils.RandomRange(0f, 1f);
        //評価値の合計が0の場合ルーレットが止まらなくなるので，初期値を設定
        parents[i] = individuals[0];
        //どこに止まったか確認
        foreach(var individual in individuals)
        {
          roulette -= individual.Fitness / fitSum;
          if (roulette <= 0)
          {
            parents[i] = individual;  //親の決定
            break;
          }
        }
      }
      return parents;
    }

    /// <summary>
    /// トーナメント選択による親選択(未実装)
    /// </summary>
    /// <param name="individuals">親選択をする個体の集合</param>
    /// <param name="select_num">選択する親の数</param>
    /// <returns>親個体</returns>
    public TIndividual[] TournamentSelection(TIndividual[] individuals, int select_num)
    {
      TIndividual[] parents = new TIndividual[select_num];
      List<TIndividual> inds = new List<TIndividual>(individuals);
      //個体集合をランダムにシャッフル
      inds = inds.OrderBy(a => Guid.NewGuid()).ToList();
      for (int i = 0; i < select_num; i++)
      {
        //select_num分割する（一部少なくなる区画もある）
        int s = Mathf.RoundToInt((float) i      / (select_num + 1) * inds.Count);  //分割開始地点
        int e = Mathf.RoundToInt((float)(i + 1) / (select_num + 1) * inds.Count);  //分割終了地点
        List<TIndividual> tournament = inds.GetRange(s, e - s);
        //トーナメント中の最も良い個体を親とする
        parents[i] = tournament.OrderByDescending(a => a.Fitness).First();
      }

      return this.Individuals;
    }

    /// <summary>
    /// ランキング選択による親選択(未実装)
    /// </summary>
    /// <param name="individuals">親選択をする個体の集合</param>
    /// <param name="select_num">選択する親の数</param>
    /// <returns>親個体</returns>
    public TIndividual[] RankingSelection(TIndividual[] individuals, int select_num)
    {
      return this.Individuals;
    }

    /// <summary>
    /// エリート選択をするメソッド
    /// 個体の中から最も良い評価を持つものをselect_num個取り出す．
    /// </summary>
    /// <param name="select_num">取り出すエリート数</param>
    /// <returns>指定個数のエリート個体</returns>
    public TIndividual[] SelectElete(int select_num)
    {
      List<TIndividual> temp = new List<TIndividual>(this.Individuals);
      //降順にソート
      temp.Sort((a, b) =>
      {
        if (!a.MeetConstraint && b.MeetConstraint)
          return 1;
        else if (a.MeetConstraint && !b.MeetConstraint)
          return -1;

        else if (a.Fitness < b.Fitness)
          return 1;
        else if (a.Fitness > b.Fitness)
          return -1;
        else
          return 0;
      });
      //Utils.PrintArray(temp);
      //先頭から指定個数抽出
      return temp.GetRange(0, select_num).ToArray();
    }

    /// <summary>
    /// 一番優秀な個体を返す
    /// </summary>
    /// <returns>最も評価の高い個体</returns>
    public TIndividual GetTopIndividual()
    {
      return SelectElete(1)[0];
    }

    /// <summary>
    /// 個体の情報を一覧で表示する．
    /// </summary>
    /// <returns></returns>
    public string GetIndividualsInfo()
    {
      string result = "";
      foreach(var individual in this.Individuals)
      {
        result += individual.GetGeneInfo() + "\n";
      }
      return result;
    }
  }

  /// <summary>
  /// 各交叉メソッドで生まれる個体数を返すプロパティを保証
  /// </summary>
  public interface IIndividual
  {
    Dictionary<CrossoverMethod, int> NumBornIndividual { get; }
  }

  /// <summary>
  /// 個体クラス
  /// </summary>
  public abstract class Individual : IIndividual
  {
    /// <summary>
    /// 個体の評価(適応度)
    /// </summary>
    public float Fitness;
    /// <summary>
    /// この個体が制約を満たしているかどうか
    /// </summary>
    public bool MeetConstraint;

    /// <summary>
    /// 各交叉メソッドで生まれる個体数を返す
    /// </summary>
    public Dictionary<CrossoverMethod, int> NumBornIndividual {
      get {
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
    public Individual()
    {
      this.Fitness = float.MinValue;
      this.MeetConstraint = true;
      //Initialize();
    }

    /// <summary>
    /// エピソード開始時の初期化
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// コピーインスタンスを返す
    /// </summary>
    public abstract object Clone();

    /// <summary>
    /// フィットネス値のセッタ
    /// </summary>
    /// <param name="fitness">更新値</param>
    public void SetFitness(float fitness)
    {
      this.Fitness = fitness;
    }

    /// <summary>
    /// 世代開始時の準備
    /// </summary>
    public abstract void Prepare();

    /// <summary>
    /// 個体の破棄をする
    /// </summary>
    public abstract void Discard();

    /// <summary>
    /// 交叉させる
    /// 突然変異を伴う
    /// </summary>
    /// <param name="individual">パートナー個体</param>
    /// <param name="crossoverMethod">交叉方法</param>
    /// <param name="mutationRate">突然変異確率</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public Individual[] CrossoverWithMutation(Individual individual, CrossoverMethod crossoverMethod, float mutationRate)
    {
      if (crossoverMethod == CrossoverMethod.OnePoint)
        return OnePointCrossoverWithMutation(individual, mutationRate);
      else if (crossoverMethod == CrossoverMethod.TwoPoint)
        return TwoPointCrossoverWithMutation(individual, mutationRate);
      else if (crossoverMethod == CrossoverMethod.Uniform)
        return UniformCrossoverWithMutation(individual, mutationRate);
      else
        return UniformCrossoverWithMutation(individual, mutationRate);
    }

    /// <summary>
    /// 一点交叉させる
    /// 突然変異を伴う
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <param name="mutationRate">突然変異確率</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] OnePointCrossoverWithMutation(Individual parent, float mutationRate);

    /// <summary>
    /// 二点交叉させる
    /// 突然変異を伴う
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <param name="mutationRate">突然変異確率</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] TwoPointCrossoverWithMutation(Individual parent, float mutationRate);

    /// <summary>
    /// 一様交叉させる
    /// 突然変異を伴う
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <param name="mutationRate">突然変異確率</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] UniformCrossoverWithMutation(Individual parent, float mutationRate);

    /// <summary>
    /// 交叉させる
    /// </summary>
    /// <param name="individual">パートナー個体</param>
    /// <param name="crossoverMethod">交叉方法</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public Individual[] Crossover(Individual individual, CrossoverMethod crossoverMethod)
    {
      if (crossoverMethod == CrossoverMethod.OnePoint)
        return OnePointCrossover(individual);
      else if (crossoverMethod == CrossoverMethod.TwoPoint)
        return TwoPointCrossover(individual);
      else if (crossoverMethod == CrossoverMethod.Uniform)
        return UniformCrossover(individual);
      else
        return UniformCrossover(individual);
    }

    /// <summary>
    /// 一点交叉させる
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] OnePointCrossover(Individual parent);

    /// <summary>
    /// 二点交叉させる
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] TwoPointCrossover(Individual parent);

    /// <summary>
    /// 一様交叉させる
    /// </summary>
    /// <param name="parent">パートナー個体</param>
    /// <returns>交叉してできた子個体の集合</returns>
    public abstract Individual[] UniformCrossover(Individual parent);

    /// <summary>
    /// 個体を突然変異させる
    /// </summary>
    public abstract void Mutation(float mutationRate);

    /// <summary>
    /// 遺伝子パラメータの一覧表示を文字列で返す．
    /// </summary>
    /// <returns></returns>
    public abstract string GetGeneInfo();
  }
}

