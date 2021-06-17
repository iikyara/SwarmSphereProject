using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public enum TestScript
{
  RandomBool,
  RandomRangeInt,
  RandomRangeFloat,
  RandomRangeVector3,
  GPGPUTest,
  GPGPUTextureCopy,
  GPGPUTextureCopy2,
  GPGPUMaskCopy,
  AlphaJoin,
  AlphaJoin2,
  RenderTextureToTexture2D,
  CountNonTransparentPixel_CPUParallel,
  VoronoiPartition,
  QuaternionSpecification,
  Count_CPUParallel,
  TextureScaling,
  CreateQuad,
  ErrorPrimitive,
  WorldToScreen,
  ColorCode,
  DrawPoint,
  VoronoiPartitionFast,
  VoronoiPartition_AddContinuouslyPoint
}

public class ScriptTestController : MonoBehaviour
{
  public TestScript TestScript;
  public Texture2D Texture1;
  public Texture2D Texture2;
  public SpriteRenderer OutputSR;
  public Camera RenderTextureCamera;

  [Header("AlphaJoin2")]
  public float Attenuation;

  [Header("GPGPUTextureCopy2")]
  public RawImage ImageIns;

  [Header("QuaternionSpecification")]
  public GameObject RotationObject;
  public bool IsCW;

  [Header("TextureScaling")]
  public Vector2Int Resolution;
  public Utils.ScalingMode ScalingMode;

  [Header("ErrorPrimitive")]
  public SIP Sip;
  public PrimitiveParamater PP;
  public GameObject CameraPreviewParentObject;

  [Header("WorldToScreen")]
  public Camera ViewCamera;
  public GameObject TargetObject;

  [Header("ColorCode")]
  public string ColorCode;

  [Header("DrawPoint")]
  public Texture2D PenTexture;
  public Vector2Int TextureResolution;
  public Vector2Int Point;

  [Header("VoronoiPartitionFast, VoronoiPartition_AddContinuouslyPoint")]
  public int[] AgentNumList;
  public int LoopNum;
  public int AgentNum;
  [Path]
  public string ResultCSVPath;

  private MethodInfo mi;

  private bool ExecOnce = false;

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log(this.TestScript.ToString() + "のテストを開始");
    Debug.Log(this.GetType());
    Debug.Log(this.GetType().GetMethod("Test" + this.TestScript.ToString()));
    mi = this.GetType().GetMethod("Test" + this.TestScript.ToString());
  }

  // Update is called once per frame
  void Update()
  {
    mi.Invoke(this, new object[] { });
    ExecOnce = true;
  }

  private int truecount = 0;
  private int count = 0;
  public void TestRandomBool()
  {
    count++;
    bool r = Utils.RandomBool(0.8f);
    if (r) truecount++;
    Debug.Log(r + ", 母数 : " + count + ", 現在のTrue確率：" + ((float)truecount / count));
  }

  private int sum = 0;
  private int sqrtsum = 0;
  private int count2 = 0;
  public void TestRandomRangeInt()
  {
    int r = Utils.RandomRange(-10, 11);
    sum += r;
    sqrtsum += r * r;
    count2++;
    float ave = (float)sum / count2;
    float sqrtave = (float)sqrtsum / count2;
    float s = sqrtave - ave * ave;
    Debug.Log(r + ", 母数 : " + count2 + ", 平均 : " + ave + ", 分散 : " + s);
  }

  private float sum2 = 0;
  private float sqrtsum2 = 0;
  private int count3 = 0;
  public void TestRandomRangeFloat()
  {
    float r = Utils.RandomRange(-10f, 10f);
    sum2 += r;
    sqrtsum2 += r * r;
    count3++;
    float ave = (float)sum2 / count3;
    float sqrtave = (float)sqrtsum2 / count3;
    float s = sqrtave - ave * ave;
    Debug.Log(r + ", 母数 : " + count3 + ", 平均 : " + ave + ", 分散 : " + s);
  }

  private Vector3 sum3 = new Vector3();
  private Vector3 sqrtsum3 = new Vector3();
  private int count4 = 0;
  public void TestRandomRangeVector3()
  {
    Vector3 r = Utils.RandomRange(new Vector3(-10f, -5f, 0f), new Vector3(10f, 5f, 10f));
    sum3 += r;
    sqrtsum3 += new Vector3(r.x * r.x, r.y * r.y, r.z * r.z);
    count4++;
    Vector3 ave = sum3 / count4;
    Vector3 sqrtave = sqrtsum3 / count4;
    Vector3 s = sqrtave - new Vector3(ave.x * ave.x, ave.y * ave.y, ave.z * ave.z);
    Debug.Log(r + ", 母数 : " + count4 + ", 平均 : " + ave + ", 分散 : " + s);
  }

  public void TestGPGPUTest()
  {
    GPGPUUtils.GPGPUTest();
  }

  public void TestAlphaJoin()
  {
    var result = GPGPUUtils.AlphaJoin(Texture1, Texture2);
    var sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2());
    OutputSR.sprite = sprite;
    //OutputSR.sprite.texture.SetPixels(result.GetPixels());
  }

  public void TestAlphaJoin2()
  {
    var result = GPGPUUtils.AlphaJoin2(Texture1, Texture2, Attenuation);
    var sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2());
    OutputSR.sprite = sprite;
    //OutputSR.sprite.texture.SetPixels(result.GetPixels());
  }

  public void TestGPGPUTextureCopy()
  {
    var result = GPGPUUtils.GPGPUTextureCopy(Texture1);
    var sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2());
    OutputSR.sprite = sprite;
  }

  public void TestGPGPUTextureCopy2()
  {
    if(!ExecOnce)
    {
      RenderTexture rtex = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
      rtex.enableRandomWrite = true;
      rtex.Create();
      ImageIns.texture = rtex;
    }
    var tex = (RenderTexture)ImageIns.texture;
    GPGPUUtils.GPGPUTextureCopy(Texture1, ref tex);
  }

  public void TestGPGPUMaskCopy()
  {
    if (!ExecOnce)
    {
      RenderTexture rtex = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RFloat);
      rtex.enableRandomWrite = true;
      rtex.Create();
      ImageIns.texture = rtex;
    }
    var tex = (RenderTexture)ImageIns.texture;
    GPGPUUtils.GPGPUMaskCopy(Texture1, ref tex);
  }

  public void TestRenderTextureToTexture2D()
  {
    var rtex = RenderTextureCamera.targetTexture;
    var result = Utils.RenderTextureToTexture2D(rtex);
    var sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2());
    OutputSR.sprite = sprite;
  }

  public void TestCountNonTransparentPixel_CPUParallel()
  {
    var result = Utils.CountNonTransparentPixel_CPUParallel(Texture1);
    Debug.Log(result);
  }

  public void TestVoronoiPartition()
  {
    var primitive = new Primitive(5f, new Vector3(), new Quaternion(), 6, 3);
    var agents = primitive.Agents;
    var vp1 = new VoronoiPartition();
    vp1.CreateDelaunayWithThinOutPoints(agents);

  }

  Quaternion rotation = Quaternion.Euler(0f, 50f, 0f);
  public void TestQuaternionSpecification()
  {
    if(IsCW) rotation = rotation * Quaternion.Euler(1f, 0f, 0f);
    else rotation = rotation * Quaternion.Euler(-1f, -0f, 0f);
    Debug.Log(rotation.eulerAngles);
    RotationObject.transform.rotation = rotation;
  }

  public void TestCount_CPUParallel()
  {
    string r = "【時間合計】\n";
    var msw = new MyStopwatch();
    msw.Start();
    var result1 = Utils.CountNonTransparentPixel_CPUParallel(Texture1);
    msw.Stop();
    r += msw.GetResultString("CountNonTransparentPixel_CPUParallel");

    msw.Restart();
    var result2 = Utils.SumAlpha_CPUParallel(Texture1);
    msw.Stop();
    r += msw.GetResultString("SumAlpha_CPUParallel");

    msw.Restart();
    var result3 = Utils.CountPixelAndSumArea(Texture1);
    msw.Stop();
    r += msw.GetResultString("CountPixelAndSumArea");

    Debug.Log(string.Format("【結果】CountNonTransparentPixel_CPUParallel : {0}\n" +
      "SumAlpha_CPUParallel : {1}\n" +
      "CountPixelAndSumArea : {2}\n" +
      "{3}",
      result1, result2, result3, r
    ));
  }

  public void TestTextureScaling()
  {
    var msw = new MyStopwatch();
    msw.Start();
    var result = Utils.TextureScaling(Texture1, Resolution.x, Resolution.y, ScalingMode);
    msw.Stop();
    Debug.Log("実行時間 : " + msw.GetResultString());
    var sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2());
    OutputSR.sprite = sprite;
  }

  public void TestCreateQuad()
  {
    if (ExecOnce) return;
    var obj = Utils.CreateQuad("TestQuad", Texture1);
  }

  public void TestErrorPrimitive()
  {
    if (ExecOnce) return;

    var Sips = this.Sip.Sips;

    //スケッチのセットアップ
    var sketches = new Sketch[Sips.Length];
    for (int i = 0; i < Sips.Length; i++)
    {
      //sketches[i] = new Sketch(Sips[i].SketchImage, Sips[i].CameraPosition, Quaternion.Euler(Sips[i].CameraRotation));
      sketches[i] = new Sketch(Sips[i]);
      sketches[i].SetEvaluationMap(GPGPUUtils.GPGPUTextureCopy(sketches[i].SketchImage));
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

    //環境を生成
    LPEnvironment lPEnvironment = new LPEnvironment(sketches, 0.8f, 0.1f);

    //個体を生成
    LPIndividual lPIndividual = new LPIndividual();
    lPIndividual.Size = PP.Size.x;
    lPIndividual.Position = PP.Position;
    lPIndividual.Rotation = PP.Quaternion;
    lPIndividual.SepH = PP.Sep_h;
    lPIndividual.SepV = PP.Sep_v;
    lPIndividual.Prepare();

    var fitness = lPEnvironment.GetFitness(lPIndividual);
    Debug.Log("評価値：" + fitness);
  }

  public void TestWorldToScreen()
  {
    var result = Utils.WorldToScreen(TargetObject.transform.position, ViewCamera, 32, 32);
    Debug.Log(string.Format(
      "World:{0} -> Screen:{1} CameraPixel:({2}, {3})",
      TargetObject.transform.position, result, ViewCamera.pixelWidth, ViewCamera.pixelHeight
    ));
  }

  public void TestColorCode()
  {
    string colorCode = ColorCode;
    Color color = Utils.ColorCodeToColor(colorCode);
    string color_hex = Utils.ColorToColorCode(color);
    Debug.Log(string.Format("source : {0} -> color({1}, {2}, {3}) -> hex : {4}", 
      colorCode, color.r, color.g, color.b, color_hex
    ));
  }

  GameObject sketchCanvas;
  RenderTexture sketchBuffer;
  public void TestDrawPoint()
  {
    if (!ExecOnce)
    {
      //描画バッファ
      int width = this.TextureResolution.x;
      int height = this.TextureResolution.y;
      sketchBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      sketchBuffer.enableRandomWrite = true;
      sketchBuffer.Create();

      //ビュー
      sketchCanvas = Utils.CreateQuad("SketchCanvas", Utils.RenderTextureToTexture2D(sketchBuffer));
    }
    
    //ペンテクスチャ
    var penTex = this.PenTexture;

    MyStopwatch msw = new MyStopwatch();
    msw.Start();
    
    GPGPUUtils.GPGPUDrawPoint(penTex, this.Point, ref sketchBuffer);

    msw.Stop();

    Utils.ChangeTexture(sketchCanvas, sketchBuffer);

    Debug.Log($"{msw.GetResultString("DrawTime")}");
  }

  int vpf_cnt = 0;
  GameObject test_vpf_parent;
  VoronoiPartition test_vpf_vp;
  VoronoiPartitionFast test_vpf_vpf;
  VoronoiPartitionFast test_vpf_vpf_wtop;
  public void TestVoronoiPartitionFast()
  {
    //全ての実行が終わったら終了
    if (vpf_cnt >= LoopNum * AgentNumList.Length) return;

    if (test_vpf_parent != null)
    {
      test_vpf_parent.SetActive(false);
    }
    if (test_vpf_vp != null) test_vpf_vp.Discard();
    if (test_vpf_vpf != null) test_vpf_vpf.Discard();
    if (test_vpf_vpf_wtop != null) test_vpf_vpf_wtop.Discard();

    //点の数を読み込む
    int num = AgentNumList[Mathf.FloorToInt((float)vpf_cnt++ / LoopNum)];
    
    //var primitive = new Primitive(5f, new Vector3(), new Quaternion(), 6, 3);
    var primitive = new Primitive();
    //primitive.AddAgentsLocatedAsQuestion();
    //primitive.LocateAgentRandomly(10f, 10f, 10f);
    primitive.AddAgentLocatedRandomly(num, 10f, 10f, 10f);
    var agents = primitive.Agents;
    primitive.SetVisible(true, true, true, true);
    primitive.UpdateParticleObject();

    //親オブジェクト
    test_vpf_parent = new GameObject("TVPF");
    GameObject vp_parent = new GameObject("VoronoiPartition_wtop");
    GameObject vp_edgeParent = new GameObject("Deraunay");
    GameObject vp_meshParent = new GameObject("Voronoi");
    GameObject vpf_parent = new GameObject("VoronoiPartitionFast");
    GameObject vpf_edgeParent = new GameObject("Deraunay");
    GameObject vpf_meshParent = new GameObject("Voronoi");
    GameObject vpf_wtop_parent = new GameObject("VoronoiPartitionFast_wtop");
    GameObject vpf_wtop_edgeParent = new GameObject("Deraunay");
    GameObject vpf_wtop_meshParent = new GameObject("Voronoi");

    string result = $"{num}, ";
    MyStopwatch msw = new MyStopwatch();
    string msw_result = $"【計測結果】\n{num}点\n";
    msw.Start();

    test_vpf_vp = new VoronoiPartition(agents, vp_edgeParent, vp_meshParent);
    test_vpf_vp.SetVisible(false, false, true);
    //vp.CreateDelaunayWithThinOutPoints();

    msw.Stop();
    msw_result += msw.GetResultString("改善前") + "\n";
    result += $"{msw.Elapsed.TotalSeconds}, ";
    msw.Restart();

    test_vpf_vpf = new VoronoiPartitionFast(agents, vpf_edgeParent, vpf_meshParent);
    test_vpf_vpf.SetVisible(false, false, true);
    test_vpf_vpf.Create();

    msw.Stop();
    msw_result += msw.GetResultString("改善後") + "\n";
    result += $"{msw.Elapsed.TotalSeconds}, ";
    msw.Restart();

    test_vpf_vpf_wtop = new VoronoiPartitionFast(agents, vpf_wtop_edgeParent, vpf_wtop_meshParent);
    test_vpf_vpf_wtop.SetVisible(false, false, true);
    test_vpf_vpf_wtop.CreateDelaunayWithThinOutPoints();

    msw.Stop();
    msw_result += msw.GetResultString("改善後（点間引き有）") + "\n";
    result += $"{msw.Elapsed.TotalSeconds}, ";
    msw.Restart();

    Debug.Log(msw_result);

    //横に並べる
    test_vpf_vp.SetParent(vp_parent);
    test_vpf_vpf.SetParent(vpf_parent);
    test_vpf_vpf_wtop.SetParent(vpf_wtop_parent);
    vp_parent.transform.position = new Vector3(-20f, 0f, 0f);
    vpf_parent.transform.position = new Vector3(0f, 0f, 0f);
    vpf_wtop_parent.transform.position = new Vector3(20f, 0f, 0f);

    //親を設定
    Utils.SetParent(test_vpf_parent, vp_parent);
    Utils.SetParent(test_vpf_parent, vpf_parent);
    Utils.SetParent(test_vpf_parent, vpf_wtop_parent);
    primitive.SetParent(test_vpf_parent);

    //結果を保存
    System.IO.File.AppendAllText(
      ResultCSVPath,
      $"{result}\n"
    );
  }

  VoronoiPartitionFast vp_acp_vpf;
  List<(Agent, ParticleObject)> vp_acp_agents;
  int vp_acp_index;
  MyStopwatch vp_acp_msw;
  bool vp_acp_isfinished;
  public void TestVoronoiPartition_AddContinuouslyPoint()
  {
    if (!ExecOnce)
    {
      //点生成
      var primitive = new Primitive();
      primitive.AddAgentLocatedRandomly(AgentNum, 10f, 10f, 10f);
      vp_acp_agents = primitive.Agents;
      primitive.SetVisible(true, true, true, true);
      primitive.UpdateParticleObject();

      //ペアレント生成
      GameObject vpf_parent = new GameObject("VoronoiPartitionFast_Continuously");
      GameObject vpf_edgeParent = new GameObject("Deraunay");
      GameObject vpf_meshParent = new GameObject("Voronoi");

      //vpf生成
      vp_acp_vpf = new VoronoiPartitionFast();
      vp_acp_vpf.Create();

      //親設定
      vp_acp_vpf.SetParent(vpf_edgeParent, vpf_meshParent);
      vp_acp_vpf.SetParent(vpf_parent);

      //ビュー設定
      vp_acp_vpf.SetVisible(false, false, true);

      //カウンタリセット
      vp_acp_index = 0;
      vp_acp_isfinished = false;

      vp_acp_msw = new MyStopwatch();
      vp_acp_msw.Start();
      /*
      GameObject vpf2_parent = new GameObject("VoronoiPartitionFast");
      GameObject vpf2_edgeParent = new GameObject("Deraunay");
      GameObject vpf2_meshParent = new GameObject("Voronoi");
      var vpf2 = new VoronoiPartitionFast(vp_acp_agents, vpf2_edgeParent, vpf2_meshParent);
      vpf2.SetVisible(false, true, true);
      vpf2.Create();
      vpf2.SetParent(vpf2_parent);*/
    }

    if (vp_acp_isfinished) return;

    //点を配置し終わったら
    if (vp_acp_index >= vp_acp_agents.Count && !vp_acp_isfinished)
    {
      //vp_acp_vpf.Complete();
      vp_acp_msw.Stop();
      vp_acp_msw.ShowResult("Result");
      vp_acp_isfinished = true;
      return;
    }

    //vpfに追加
    vp_acp_vpf.AddAgent(vp_acp_agents[vp_acp_index++]);

    //vp_acp_msw.GetResultString();
  }
}
