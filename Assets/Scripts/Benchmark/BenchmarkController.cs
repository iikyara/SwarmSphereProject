using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkController : MonoBehaviour
{
  public GroundTruthDataManager GroundTruthDataManager;
  public BenchmarkCameraManager BenchmarkCameraManager;
  public BenchmarkParamaterManager BenchmarkParamaterManager;
  public List<Benchmark> Benchmarks;
  //private BenchmarkResultManager BenchmarkResultManager;
  private Dictionary<ModelGenerater, BenchmarkResultManager> BenchmarkResultManagers;
  //public List<ModelGenerater> Generaters;

  public int NumOfBenchmark = 1;

  [Header("UI関連")]
  public BenchmarkUIManager UIManager;
  public SketchCameraController SketchCameraController;


  [Header("Save Setting")]
  [Path]
  public string CSV_Path;
  private string ResultPath;

  /* for setup */
  private int setupCounter;

  /* state flags */
  private bool IsBenchmarkRunning;
  private bool IsModelGenerated;
  private bool IsFinished;
  private bool IsReadyToNext;
  
  /* current elems */
  private GTData currentGTData;
  private ModelGenerater currentModelGenerater;
  private MGP currentMGP;
  private BenchmarkCameraSet currentBenchmarkCameraSet;

  private void Start()
  {
    this.BenchmarkResultManagers = new Dictionary<ModelGenerater, BenchmarkResultManager>();
    ResultPath = System.IO.Path.Combine(CSV_Path, System.DateTime.Now.ToString("yyMMddHHmmss"));
    System.IO.Directory.CreateDirectory(ResultPath);
    //Debug.Log(ResultPath);

    resetState();
    setupCounter = 0;
  }

  private void Update()
  {
    UpdateBenchmark();
  }

  private void resetState()
  {
    IsBenchmarkRunning = false;
    IsModelGenerated = false;
    IsReadyToNext = false;
  }

  /*/// <summary>
  /// 全データに対してベンチマークを実行する
  /// </summary>
  private void ExecBenchmark()
  {
    //全データに対してベンチマークを実行
    var dataList = GroundTruthDataManager.DataList;
    for (int i = 0; i < dataList.Count; i++)
    {
      var data = dataList[i];
      Benchmark.SetGTData(data);
      Benchmark.Exec();
    }
  }*/

  /// <summary>
  /// ベンチマークの実行をフレームごとに行う
  /// </summary>
  private void UpdateBenchmark()
  {
    //全て終了したら更新しない
    if (IsFinished) return;
    //実行していなかったらセットアップして実行しようとする．
    if(!IsBenchmarkRunning)
    {
      Debug.Log("<<<Start>>>");
      //セットアップ
      if (!SetUpSetting())
      {
        //最後まで終了したら保存して終わり
        //SaveResult();
        Debug.Log("Finish!");
        IsFinished = true;
        return;
      }

      //モデル撮影
      CaptureGTModel(currentGTData);

      //パラメータセット
      currentModelGenerater.SetParamater(currentMGP);

      //スケッチ生成
      var camSet = currentBenchmarkCameraSet.CameraList;
      Sketch[] sketches = new Sketch[camSet.Count];
      for(int i = 0; i < camSet.Count; i++)
      {
        sketches[i] = new Sketch(camSet[i].SIP, true);
      }

      //生成開始
      currentModelGenerater.StartGenerating(sketches);
      IsBenchmarkRunning = true;
    }
    if (IsBenchmarkRunning)
    {
      //生成が終わるまで待つ
      if (!IsModelGenerated)
      {
        //Debug.Log("<<<GeneratingUpdate>>>");
        IsModelGenerated = currentModelGenerater.IsGenerated;
      }
      //ベンチマークメソッドでスコアを算出
      if (IsModelGenerated)
      {
        Debug.Log("<<<Benchmark>>>");
        var generated = new GData(currentModelGenerater.GetGeneratedMesh());
        //全ベンチマーク実行
        foreach(var bm in this.Benchmarks)
        {
          bm.SetGTData(currentGTData, generated);
          bm.Exec();
        }
        //結果を保存
        var result = new BenchmarkResult(currentGTData, currentBenchmarkCameraSet, currentMGP, this.Benchmarks);
        if (!this.BenchmarkResultManagers.ContainsKey(result.ModelGenerater))
        {
          this.BenchmarkResultManagers.Add(result.ModelGenerater, new BenchmarkResultManager());
        }
        this.BenchmarkResultManagers[result.ModelGenerater].Results.Add(result);
        SaveResult(result);
        IsBenchmarkRunning = false;
      }
    }
  }

  /// <summary>
  /// 対象データ，対象生成手法，手法のパラメータ，カメラ情報を設定してセット
  /// 対象データ，カメラ情報，パラメータの組み合わせを全て実行
  /// </summary>
  private bool SetUpSetting()
  {
    resetState(); //フラグをリセット

    var datas = this.GroundTruthDataManager.DataList;
    var cameras = this.BenchmarkCameraManager.CameraSetList;
    var paramaters = this.BenchmarkParamaterManager.Paramaters;

    int[] indices = new int[3];
    int n = 1;
    indices[0] = datas.Count;
    indices[1] = cameras.Count;
    indices[2] = paramaters.Count;

    //全部の積を計算
    for (int i = 0; i < indices.Length; i++)
    {
      n *= indices[i];
    }
    int maxNum = n;

    //終了チェック
    if (setupCounter / NumOfBenchmark >= maxNum) return false;

    //それぞれのインデックスを計算
    int ctmp = setupCounter / NumOfBenchmark;
    for (int i = indices.Length - 1; i >= 0; i--)
    {
      var r = Utils.Div(ctmp, indices[i]);
      ctmp = r.Item1;
      indices[i] = r.Item2;
    }

    //フィールド更新
    this.currentGTData = datas[indices[0]];
    this.currentBenchmarkCameraSet = cameras[indices[1]];
    this.currentModelGenerater = paramaters[indices[2]].TargetModelGenerater;
    this.currentMGP = paramaters[indices[2]];

    //残り時間みたいなのを表示
    Debug.Log($"現在：{(float)maxNum / NumOfBenchmark * setupCounter * 100}%");

    setupCounter++;
    IsReadyToNext = true;

    return true;
  }

  private void SaveResultAll()
  {
    Debug.Log(Utils.GetDictionaryString(this.BenchmarkResultManagers));
  }

  /// <summary>
  /// 1行保存
  /// </summary>
  /// <param name="br"></param>
  private void SaveResult(BenchmarkResult br)
  {
    Debug.Log(br);
    string filename = System.IO.Path.Combine(ResultPath, br.ModelGenerater.Name) + ".csv";
    if (!System.IO.File.Exists(filename))
    {
      System.IO.File.AppendAllText(filename, br.GetCSVHeader() + "\n");
    }
    System.IO.File.AppendAllText(filename, br.ToCSV() + "\n");
  }

  private void CaptureGTModel(GTData targetGT)
  {
    var camSet = currentBenchmarkCameraSet.CameraList;
    foreach(var cam in camSet)
    {
      cam.Capture(targetGT);
    }
  }

  /* イベントリスナー */

  public void MouseDown(Vector3 mousePos, SketchMouse sm)
  {
    //updateAll();
  }

  public void MouseUp(Vector3 mousePos, SketchMouse sm)
  {
    //updateAll();
  }

  public void MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    this.SketchCameraController.MouseDrag(preMousePos, curMousePos, sm);
    updateAll();
  }

  public void MouseWheel(Vector2 delta)
  {
    this.SketchCameraController.MouseWheel(delta);
    updateAll();
  }

  private void updateAll()
  {

  }
}
