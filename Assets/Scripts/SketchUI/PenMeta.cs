using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ペンの種類の定義
/// </summary>
public enum PenType
{
  Brush,
  Eracer,
  Bucket
}

/// <summary>
/// 描画オブジェクトの管理をする
/// </summary>
public class SketchPenManager
{
  public static PenSetManager PenSetManager;

  /// <summary>
  /// 描画オブジェクトのリスト
  /// </summary>
  public List<IPen> PenTools;

  /// <summary>
  /// 現在の描画オブジェクトのインデックス
  /// </summary>
  private int _currentPenIndex;
  public int CurrentPenIndex {
    get { return _currentPenIndex; }
    set {
      if (value >= 0 && value < PenTools.Count) _currentPenIndex = value;
      else throw new System.Exception("Bad Index. - value : " + value + ", PenTools.Count : " + PenTools.Count);
    }
  }

  /// <summary>
  /// 現在の描画オブジェクト
  /// </summary>
  public IPen CurrentPen { get { return PenTools[CurrentPenIndex]; } }

  public SketchPenManager(List<PenType> penList, PenSetManager penSetManager)
  {
    SketchPenManager.PenSetManager = penSetManager;
    this.PenTools = new List<IPen>(PenTypeToPen(penList));
    this.CurrentPenIndex = 0;
  }

  /// <summary>
  /// 現在の描画オブジェクトを変更する
  /// </summary>
  /// <param name="penId">描画オブジェクトのID</param>
  public void SetCurrentPen(int penId)
  {
    this.CurrentPenIndex = penId;
  }

  /// <summary>
  /// 指定した描画オブジェクトの設定を変更する
  /// </summary>
  /// <param name="penId">描画オブジェクトのID</param>
  /// <param name="settings">設定</param>
  public void SetSetting(int penId, (string, object) settings)
  {
    this.PenTools[penId].SetSetting(settings);
  }

  /// <summary>
  /// ペンタイプから描画オブジェクトに変換する．
  /// </summary>
  /// <param name="pts"></param>
  /// <returns></returns>
  List<IPen> PenTypeToPen(List<PenType> pts)
  {
    List<IPen> pens = new List<IPen>();
    for (int i = 0; i < pts.Count; i++) pens.Add(SketchPenManager.CreatePenObject(i, pts[i]));
    return pens;
  }

  /// <summary>
  /// ペンタイプに応じた描画オブジェクトを生成する．
  /// </summary>
  /// <param name="id">描画オブジェクトのID</param>
  /// <param name="pt">描画オブジェクトのタイプ</param>
  /// <returns></returns>
  public static IPen CreatePenObject(int id, PenType pt)
  {
    if (pt == PenType.Brush) return new Brush(id, pt, SketchPenManager.PenSetManager.Pens[0]);
    else if (pt == PenType.Eracer) return new Eracer(id, pt, SketchPenManager.PenSetManager.Pens[0]);
    else if (pt == PenType.Bucket) return new Bucket(id, pt);
    else return null;
  }
}

/// <summary>
/// 何かを描画するクラス
/// 描画オブジェクトの実体を持つ
/// </summary>
public interface IPen
{
  int Id { get; set; }

  string Name { get; set; }

  PenType PenType { get; set; }

  void SetSetting((string, object) settings);

  void OnPenDown(Vector2 point, DrawableSketch sketch);

  void OnPenMove(Vector2 p1, Vector2 p2, DrawableSketch sketch);

  void OnPenUp(Vector2 point, DrawableSketch sketch);
}

public abstract class PenMeta : IPen
{
  /// <summary>
  /// 描画オブジェクトのID
  /// </summary>
  public int Id { get; set; }
  /// <summary>
  /// 描画オブジェクトの名前
  /// </summary>
  public string Name { get; set; }
  /// <summary>
  /// ペンのタイプ
  /// </summary>
  public PenType PenType { get; set; }
  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="id"></param>
  public PenMeta(int id, PenType pt)
  {
    this.Id = id;
    this.Name = "描画オブジェクト";
    this.PenType = pt;
  }
  /// <summary>
  /// スケッチにペンが下ろされたときの処理
  /// </summary>
  /// <param name="point"></param>
  /// <param name="sketch"></param>
  public abstract void OnPenDown(Vector2 point, DrawableSketch sketch);
  /// <summary>
  /// スケッチ上でペンがドラッグされた時の処理
  /// </summary>
  /// <param name="p1"></param>
  /// <param name="p2"></param>
  /// <param name="sketch"></param>
  public abstract void OnPenMove(Vector2 p1, Vector2 p2, DrawableSketch sketch);
  /// <summary>
  /// スケッチからペンが上がったときの処理
  /// </summary>
  /// <param name="point"></param>
  /// <param name="sketch"></param>
  public abstract void OnPenUp(Vector2 point, DrawableSketch sketch);

  /// <summary>
  /// パラメータに設定を保存する
  /// </summary>
  /// <param name="settings"></param>
  public abstract void SetSetting((string, object) settings);

  /* 汎用メソッド */
  protected Vector2 sketchPointToUV(Vector2 sketchPoint, Vector2Int resolution)
  {
    Vector2 fp = new Vector2(
      sketchPoint.x / resolution.x + 0.5f,
      sketchPoint.y / resolution.y + 0.5f
    );

    return fp;
  }

  protected Vector2Int culcCenter(Vector2Int penPoint, Vector2Int penResolution)
  {
    Vector2Int center = penPoint - new Vector2Int(penResolution.x / 2, penResolution.y / 2);
    return center;
  }
}

public class Brush : PenMeta
{
  /// <summary>
  /// ペンの形状
  /// </summary>
  public PenSet PenSet { get; set; }
  /// <summary>
  /// 色
  /// </summary>
  public Color Color {
    get { return _color; }
    set { _color = new Color(value.r, value.g, value.b, _color.a); }
  }
  private Color _color;
  /// <summary>
  /// 太さ
  /// </summary>
  public int Thick { get; set; }
  /// <summary>
  /// 透明度
  /// </summary>
  public float Transparency
  {
    get { return Color.a; }
    set { _color = new Color(Color.r, Color.g, Color.b, value); }
  }
  /// <summary>
  /// 1ピクセルあたり何回描画するか
  /// </summary>
  public float PointInterval { get; set; }

  private CoordinateCalc coordCalc;

  public Brush(int id, PenType pt, PenSet penSet) : base(id, pt)
  {
    this.Name = "ブラシ";
    this.PenSet = penSet;

    this.Color = new Color();
    this.Thick = 3;
    this.Transparency = 1f;
    this.PointInterval = 0f;

    this.coordCalc = new CoordinateCalc(3);
  }

  public override void SetSetting((string, object) settings)
  {
    this.GetType().GetProperty(settings.Item1).SetValue(this, settings.Item2);
  }

  public override void OnPenDown(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //ペンテクスチャを取得
    var penTex = this.PenSet.GetResizedPenTexture(this.Thick);
    //描画位置を計算
    Vector2Int p = culcCenter(
      new Vector2Int((int)point.x, (int)point.y),
      new Vector2Int(penTex.width, penTex.height)
    );
    //描画マスクを加算に設定
    sketch.PaintLayer.BlendMode = BlendMode.Add;
    //描画
    GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p, this.Thick);
    //色を保存
    sketch.PaintLayer.Color = this.Color;
    //CoordCalcをセットアップ
    SetupCoordCalc();
    coordCalc.PushPoint(p);

    //Debug.Log($"Down:{point}");
  }

  public override void OnPenMove(Vector2 point1, Vector2 point2, DrawableSketch sketch)
  {
    //uvを計算
    //Vector2 uv1 = sketchPointToUV(point1, sketch.SCIP.Resolution);
    //Vector2 uv2 = sketchPointToUV(point2, sketch.SCIP.Resolution);

    //ペンテクスチャを取得
    var penTex = this.PenSet.GetResizedPenTexture(this.Thick);
    Vector2Int p1 = culcCenter(new Vector2Int((int)point1.x, (int)point1.y), new Vector2Int(penTex.width, penTex.height));
    Vector2Int p2 = culcCenter(new Vector2Int((int)point2.x, (int)point2.y), new Vector2Int(penTex.width, penTex.height));

    MyStopwatch msw = new MyStopwatch();
    msw.Start();

    //CoordCalcで点を計算
    coordCalc.PushPoint(p2);  //p1は前回のループで追加済み
    foreach (var p in coordCalc.NextPointsDim2())
    {
      GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p, this.Thick);
    }

    //GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p2, this.Thick);

    msw.Stop();
    //msw.ShowResult($"Move:{p1}->{p2}");

    //Debug.Log($"Move:{p1}->{p2}");
  }

  public override void OnPenUp(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //適用・描画レイヤーリセット
    sketch.ApplyPaintLayer();
  }

  private void SetupCoordCalc()
  {
    this.coordCalc.Clear();
    this.coordCalc.Interval = this.PointInterval;
    this.coordCalc.PointSize = this.Thick;
  }
}

public class Eracer : PenMeta
{
  /// <summary>
  /// ペンテクスチャ
  /// </summary>
  public PenSet PenSet;
  /// <summary>
  /// 太さ
  /// </summary>
  public int Thick { get; set; }
  /// <summary>
  /// 透明度
  /// </summary>
  public float Transparency { get; set; }
  /// <summary>
  /// 1ピクセルあたり何回描画するか
  /// </summary>
  public float PointInterval { get; set; }

  private CoordinateCalc coordCalc;

  public Eracer(int id, PenType pt, PenSet penSet) : base(id, pt)
  {
    this.Name = "ブラシ";
    this.PenSet = penSet;

    this.Thick = 3;
    this.Transparency = 1f;
    this.PointInterval = 0f;

    this.coordCalc = new CoordinateCalc(3);
  }

  public override void SetSetting((string, object) settings)
  {
    this.GetType().GetProperty(settings.Item1).SetValue(this, settings.Item2);
  }

  public override void OnPenDown(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //ペンテクスチャを取得
    var penTex = this.PenSet.GetResizedPenTexture(this.Thick);
    //描画位置を計算
    Vector2Int p = culcCenter(
      new Vector2Int((int)point.x, (int)point.y),
      new Vector2Int(penTex.width, penTex.height)
    );
    //描画マスクを加算に設定
    sketch.PaintLayer.BlendMode = BlendMode.Erase;
    //描画
    GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p, this.Thick);
    //色を保存
    sketch.PaintLayer.Color.a = this.Transparency;
    //CoordCalcをセットアップ
    SetupCoordCalc();
    coordCalc.PushPoint(p);

    //Debug.Log($"Down:{point}");
  }

  public override void OnPenMove(Vector2 point1, Vector2 point2, DrawableSketch sketch)
  {
    //Vector2 uv1 = sketchPointToUV(p1, sketch.SCIP.Resolution);
    //Vector2 uv2 = sketchPointToUV(p2, sketch.SCIP.Resolution);

    //ペンテクスチャを取得
    var penTex = this.PenSet.GetResizedPenTexture(this.Thick);
    Vector2Int p1 = culcCenter(new Vector2Int((int)point1.x, (int)point1.y), new Vector2Int(penTex.width, penTex.height));
    Vector2Int p2 = culcCenter(new Vector2Int((int)point2.x, (int)point2.y), new Vector2Int(penTex.width, penTex.height));

    MyStopwatch msw = new MyStopwatch();
    msw.Start();

    //CoordCalcで点を計算
    coordCalc.PushPoint(p2);  //p1は前回のループで追加済み
    foreach (var p in coordCalc.NextPointsDim2())
    {
      GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p, this.Thick);
    }

    //GPGPUUtils.GPGPUDrawPointOnMask(penTex, ref sketch.PaintLayer.Image, p2, this.Thick);

    msw.Stop();
    msw.ShowResult($"Move:{p1}->{p2}");

    //Debug.Log($"Move:{p1}->{p2}");
  }

  public override void OnPenUp(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //適用・描画レイヤーリセット
    sketch.ApplyPaintLayer();
  }

  private void SetupCoordCalc()
  {
    this.coordCalc.Clear();
    this.coordCalc.Interval = this.PointInterval;
    this.coordCalc.PointSize = this.Thick;
  }
}

public class Bucket : PenMeta
{
  /// <summary>
  /// 色
  /// </summary>
  public Color Color
  {
    get { return _color; }
    set { _color = new Color(value.r, value.g, value.b, _color.a); }
  }
  private Color _color;
  /// <summary>
  /// 透明度
  /// </summary>
  public float Transparency
  {
    get { return Color.a; }
    set { _color = new Color(Color.r, Color.g, Color.b, value); }
  }

  public Bucket(int id, PenType pt) : base(id, pt)
  {
    this.Name = "バケツ";
    this._color = new Color(0f, 0f, 0f, 1f);
  }

  public override void SetSetting((string, object) settings)
  {
    this.GetType().GetProperty(settings.Item1).SetValue(this, settings.Item2);
  }

  public override void OnPenDown(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //描画位置を計算
    Vector2Int p = new Vector2Int((int)point.x, (int)point.y);
    //描画マスクを加算に設定
    sketch.PaintLayer.BlendMode = BlendMode.Add;
    //描画
    FillArea.Fill(p, this._color, sketch.View.Image, ref sketch.PaintLayer.Image);
    //色を保存
    sketch.PaintLayer.Color = this._color;
  }

  public override void OnPenMove(Vector2 p1, Vector2 p2, DrawableSketch sketch)
  {
    //Vector2 uv1 = sketchPointToUV(p1, sketch.SCIP.Resolution);
    //Vector2 uv2 = sketchPointToUV(p2, sketch.SCIP.Resolution);
  }

  public override void OnPenUp(Vector2 point, DrawableSketch sketch)
  {
    //Vector2 uv = sketchPointToUV(point, sketch.SCIP.Resolution);
    //適用・描画レイヤーリセット
    sketch.ApplyPaintLayer();
  }
}

/// <summary>
/// 筆跡の座標を計算するクラス
/// </summary>
public class CoordinateCalc
{
  public Stack<Vector2Int> Points;

  public float PointSize;
  public float Interval;
  public float Offset;

  public int Dimension;

  public CoordinateCalc(int dimension)
  {
    this.Dimension = dimension;
    this.Points = new Stack<Vector2Int>(dimension);
    this.PointSize = 1f;
    this.Interval = 0f;
    this.Offset = 0f;
  }

  public void Clear()
  {
    this.Points.Clear();
    this.Offset = 0f;
  }

  public void PushPoint(Vector2Int point)
  {
    this.Points.Push(point);
  }

  public List<Vector2Int> NextPointsDim2()
  {
    List<Vector2Int> resPoints = new List<Vector2Int>();

    var points = this.Points.ToArray();

    float d = (this.PointSize - 1) * this.Interval + 1;

    Vector2 direction = points[0] - points[1];

    //Debug.Log($"dim : {points[1]} -> {points[0]}");

    float length = Mathf.Pow(direction.x * direction.x + direction.y * direction.y, 0.5f);

    int n = (int)(length / d);

    Vector2 norm = direction / length;
    
    for(int i = 0; i < n; i++)
    {
      Vector2 pos = direction * (d * i / (float)n) + points[1];
      resPoints.Add(new Vector2Int((int)pos.x, (int)pos.y));
    }

    this.Offset = length - d * n;

    return resPoints;
  }

  public List<Vector2Int> NextPointsWithSpline()
  {
    List<Vector2Int> resPoints = new List<Vector2Int>();

    int dim = this.Dimension;
    int i;
    float x, y, yy0, yy1, yy2, yy3;
    float[] h = new float[dim];
    float[] dif1 = new float[dim];
    float[] dif2 = new float[dim];

    var points = this.Points.ToArray();

    if (points.Length < dim) return resPoints;

    h[0] = 0.0f;
    dif2[0] = 0.0f;
    dif2[dim - 1] = 0.0f;

    for(i = 1; i < dim; i++)
    {
      h[i] = points[i].x - points[i - 1].x;
      dif1[i] = (points[i].y - points[i].y) / h[i];
    }

    for(i = 1; i < dim - 1; i++)
    {
      dif2[i] = (dif1[i + 1] - dif1[i]) / (points[i + 1].x - points[i - 1].y);
    }

    i = 1;
    for(x = (float)points[0].x; x < points[dim - 1].x; x += 0.01f)
    {
      if(x < points[i].x)
      {
        yy0 = dif2[i - 1] / (6 * h[i]) * (points[i].x - x)
               * (points[i].x - x) * (points[i].x - x);       //第１項
        yy1 = dif2[i] / (6 * h[i]) * (x - points[i - 1].x)
           * (x - points[i - 1].x) * (x - points[i - 1].x);   //第２項
        yy2 = (points[i - 1].y / h[i] - h[i] * dif2[i - 1] / 6)
           * (points[i].x - x);                    //第３項
        yy3 = (points[i].y / h[i] - h[i] * dif2[i] / 6) *
           (x - points[i - 1].x);                   //第４項
        y = yy0 + yy1 + yy2 + yy3;

        resPoints.Add(new Vector2Int((int)(10 * x), (int)(10 * (40 - y))));
        //g.drawRect(X0 + (int)(10 * x), Y0 + (int)(10 * (40 - y)), 0, 0);
      }
      else i++;
    }

    return resPoints;
  }
}

/// <summary>
/// 塗りつぶしアルゴリズム
/// http://fussy.web.fc2.com/algo/algo3-2.htm
/// </summary>
public class FillArea
{
  public struct BufStr
  {
    public int lx; //領域右端のX座標
    public int rx; //領域右端のX座標
    public int y;  //領域のY座標
    public int oy; //親ラインのY座標

    public BufStr(int lx, int rx, int y, int oy)
    {
      this.lx = lx;
      this.rx = rx;
      this.y = y;
      this.oy = oy;
    }
  }

  static BufStr[] buff;  //シード登録用バッファ
  static int sIdx, eIdx; //buffの先頭・末尾インデックス
  //static Texture2D sourceTex;
  static Color[] source;
  static int width;
  static int height;
  static Texture2D mask;
  static Color[] mask_col;

  public static Color GetPixel(int x, int y)
  {
    return source[y * width + x];
  }

  public static void Fill(Vector2Int point, Color paintCol, RenderTexture sourceTex, ref RenderTexture maskRTex)
  {
    if(point.x < 0 || point.y < 0 || point.x >= sourceTex.width || point.y >= sourceTex.height)
    {
      return;
    }

    MyStopwatch msw = new MyStopwatch();
    string result = "Fill\n";

    msw.Start();

    FillArea.buff = new BufStr[sourceTex.width];

    msw.Stop();
    result += msw.GetResultString("Pre1") + "\n";
    msw.Restart();

    setSourceTexToColors(sourceTex);

    msw.Stop();
    result += msw.GetResultString("Pre2") + "\n";
    msw.Restart();

    createMaskTex(maskRTex.width, maskRTex.height);

    msw.Stop();
    result += msw.GetResultString("Pre3") + "\n";
    msw.Restart();

    paint(point.x, point.y, paintCol);

    msw.Stop();
    result += msw.GetResultString("Proc") + "\n";
    msw.Restart();

    copyT2MaskToRMask(mask, ref maskRTex);

    msw.Stop();
    result += msw.GetResultString("Post") + "\n";
    //Debug.Log(result);
  }

  /// <summary>
  /// 線分からシードを探索してバッファに登録する
  /// </summary>
  /// <param name="lx">線分のX座標の範囲</param>
  /// <param name="rx">線分のX座標の範囲</param>
  /// <param name="y">線分のY座標</param>
  /// <param name="oy">親ラインのY座標</param>
  /// <param name="col">領域色</param>
  private static void scanLine(int lx, int rx, int y, int oy, Color col)
  {
    while( lx <= rx )
    {

      /* 非領域色を飛ばす */
      for (; lx < rx; lx++)
        if (isInColorRange(col, lx, y)) break;
      if (!isInColorRange(col, lx, y)) break;

      buff[eIdx].lx = lx;

      /* 領域色を飛ばす */
      for (; lx <= rx; lx++)
        if (!isInColorRange(col, lx, y)) break;

      buff[eIdx].rx = lx - 1;
      buff[eIdx].y = y;
      buff[eIdx].oy = oy;

      if (++eIdx == width - 1)
        eIdx = 0;
    }
  }

  private static void paint(int x, int y, Color paintCol)
  {
    int lx, rx;
    int ly;
    int oy;
    int i;
    Color col = GetPixel(x, y);
    Debug.Log($"Col:{col}");
    if (col == paintCol) return;

    sIdx = 0;
    eIdx = 1;
    buff[sIdx].lx = buff[sIdx].rx = x;
    buff[sIdx].y = buff[sIdx].oy = y;

    int cnt = 0;

    do
    {
      lx = buff[sIdx].lx;
      rx = buff[sIdx].rx;
      ly = buff[sIdx].y;
      oy = buff[sIdx].oy;

      int lxsav = lx - 1;
      int rxsav = rx + 1;

      if (++sIdx == buff.Length - 1) sIdx = 0;

      //処理済みのシードなら無視
      if (!isInColorRange(col, lx, ly))
      {
        continue;
      }

      //右方向の境界を探す
      while (rx < width - 1)
      {
        if (!isInColorRange(col, rx + 1, ly)) break;
        rx++;
      }
      //左方向の境界を探す
      while (lx > 0)
      {
        if (!isInColorRange(col, lx - 1, ly)) break;
        lx--;
      }
      //lx - rxの線分を描画
      for (i = lx; i <= rx; i++) mask_col[ly * width + i] = Color.white;

      cnt += rx - lx;

      //真上のスキャンラインを捜査する
      if (ly - 1 >= 0)
      {
        if (ly - 1 == oy)
        {
          scanLine(lx, lxsav, ly - 1, ly, col);
          scanLine(rxsav, rx, ly - 1, ly, col);
        }
        else
        {
          scanLine(lx, rx, ly - 1, ly, col);
        }
      }

      /* 真下のスキャンラインを走査する */
      if (ly + 1 <= height - 1)
      {
        if (ly + 1 == oy)
        {
          scanLine(lx, lxsav, ly + 1, ly, col);
          scanLine(rxsav, rx, ly + 1, ly, col);
        }
        else
        {
          scanLine(lx, rx, ly + 1, ly, col);
        }
      }
    } while (sIdx != eIdx);

    Debug.Log($"{cnt}px塗りつぶしました");
  }

  private static bool isInColorRange(Color color_base, int x, int y)
  {
    //Debug.Log($"mask pixel : {mask_col[y * width + x]}, color_base : {color_base}, pixel : {GetPixel(x, y)}, isEqual : {color_base == GetPixel(x, y)}");
    if (mask_col[y * width + x].a != 0f) return false;  //すでに処理済みの場合，領域色の範囲に含まない
    return color_base == GetPixel(x, y);
  }

  /*private static Texture2D sourceTexToTexture2D(RenderTexture source)
  {
    Texture2D tex = Utils.RenderTextureToTexture2D(source);
    return tex;
  }*/

  private static void setSourceTexToColors(RenderTexture sourceTex)
  {
    Texture2D tex = Utils.RenderTextureToTexture2D(sourceTex);
    source = tex.GetPixels();
    width = tex.width;
    height = tex.height;
    MonoBehaviour.Destroy(tex);
  }

  private static void createMaskTex(int width, int height)
  {
    Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
    mask_col = new Color[width * height];
    //for (int i = 0; i < mask_col.Length / 3; i++) mask_col[i] = new Color(0.1f, 0.3f, 0.5f, 0.1f);
    mask = tex;
  }

  private static void copyT2MaskToRMask(Texture2D t2Mask, ref RenderTexture rMask)
  {
    mask.SetPixels(mask_col);
    mask.Apply();
    GPGPUUtils.GPGPUMaskCopy(t2Mask, ref rMask);
  }
}