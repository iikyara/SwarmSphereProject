using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawableSketch
{
  /* IDを生成する */
  static int idCounter = 0;
  static int getNextId() { return idCounter++; }

  /// <summary>
  /// スケッチを識別するID
  /// </summary>
  public int Id;

  /// <summary>
  /// ビューレイヤー
  /// </summary>
  public Layer View;

  /// <summary>
  /// レイヤー
  /// </summary>
  public List<Layer> Layers;

  private int _currentLayerIndex;
  /// <summary>
  /// 現在のアクティブレイヤー
  /// </summary>
  public Layer CurrentLayer
  {
    get { return Layers[_currentLayerIndex]; }
    set { _currentLayerIndex = Layers.IndexOf(value); }
  }

  /// <summary>
  /// 描画バッファ
  /// </summary>
  public MaskLayer PaintLayer;

  /// <summary>
  /// 色
  /// </summary>
  public Color BGColor
  {
    get { return _bgcolor; }
    set { _bgcolor = new Color(value.r, value.g, value.b, _bgcolor.a); }
  }
  private Color _bgcolor;

  /// <summary>
  /// 透明度
  /// </summary>
  public float BGTransparency
  {
    get { return BGColor.a; }
    set { _bgcolor = new Color(BGColor.r, BGColor.g, BGColor.b, value); }
  }

  /// <summary>
  /// 3Dモデルの透過度
  /// </summary>
  public float ModelTransparency { get; set; }

  public SketchAndCameraInitializationParam SCIP;

  public DrawableSketch(SketchAndCameraInitializationParam scip)
  {
    this.Id = getNextId();
    this.SCIP = scip;
    this.View = new Layer(scip.Resolution, BlendMode.Add);
    this.Layers = new List<Layer>();
    this._currentLayerIndex = 0;
    this.PaintLayer = new MaskLayer(scip.Resolution, BlendMode.Add);
    AddLayer(scip.Resolution, BlendMode.Add);
    this.BGColor = Color.white;
    this.BGTransparency = 1.0f;
    this.ModelTransparency = 0.3f;
  }

  /// <summary>
  /// レイヤーを全て合成してViewにまとめる
  /// </summary>
  public void UpdateView()
  {
    ClearView();

    for(int i = 0; i < this.Layers.Count; i++)
    {
      Layer layer = this.Layers[i];
      if(i == _currentLayerIndex)
      {
        //レイヤービューを作成
        Layer view = new Layer(this.SCIP.Resolution, layer.BlendMode);
        //レイヤー画像をコピー
        view.CopyImageFromLayer(layer);
        //描画レイヤーを適用
        view.ApplyDrawLayer(this.PaintLayer);
        layer = view;
        //レイヤーを合成
        this.View.CompositLayer(layer);
        //ビューを開放
        view.Discard();
      }
      else
      {
        //レイヤーを合成
        this.View.CompositLayer(layer);
      }
    }
  }

  public void AddLayer(Vector2Int resolution, BlendMode blendMode = BlendMode.Add)
  {
    Layer layer = new Layer(resolution, blendMode);
    this.Layers.Add(layer);
  }

  public void RemoveLayer(int layerIndex)
  {
    this.Layers.RemoveAt(layerIndex);
    if (_currentLayerIndex > layerIndex) _currentLayerIndex--;
  }

  /// <summary>
  /// srcIndexにあるレイヤーをdistIndexの上に移動する．
  /// </summary>
  /// <param name="srcIndex"></param>
  /// <param name="distIndex"></param>
  public void MoveLayer(int srcIndex, int distIndex)
  {
    Layer layer = this.Layers[srcIndex];
    this.Layers.RemoveAt(srcIndex);
    int index = (srcIndex > distIndex) ? distIndex : distIndex - 1;
    this.Layers.Insert(index, layer);
  }

  public void ApplyPaintLayer()
  {
    this.CurrentLayer.ApplyDrawLayer(this.PaintLayer);
    //描画マスクをクリア
    this.PaintLayer.ClearLayer();
  }

  public void ClearPaintLayer()
  {
    PaintLayer.ClearLayer();
  }

  public void ClearView()
  {
    this.View.ClearLayer();
  }

  public override string ToString()
  {
    return string.Format("ID : {0}", this.Id);
  }

  public Sketch ToSketch(SketchAndCameraInitializationParam scip)
  {
    //scpを作成する
    SketchInitilizationParam sip;
    //dSketchをSketchに変換する
    sip.SketchImage = GPGPUUtils.GPGPUTextureCopy(this.View.Image);
    sip.Resolution = scip.Resolution;
    sip.CameraPosition = scip.Position + scip.LookAt;
    sip.CameraRotation = Quaternion.LookRotation(-scip.Position.normalized, scip.Up).eulerAngles;
    sip.CameraProjection = scip.Projection;
    sip.FOV = scip.FOV;
    sip.Size = scip.Size;
    return new Sketch(sip, false);
  }
}
