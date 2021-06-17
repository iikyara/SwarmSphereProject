using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlendMode
{
  Add,
  Erase
}

public class Layer
{
  private RenderTexture _image;
  /// <summary>
  /// レイヤー画像
  /// </summary>
  public RenderTexture Image {
    get { return _image; }
    set
    {
      //前のテクスチャを開放する．
      if (_image) MonoBehaviour.Destroy(_image);
      //新しいテクスチャをセット
      _image = value;
    }
  }

  public BlendMode BlendMode;

  public Layer(Vector2Int resolution, BlendMode blendMode)
  {
    this._image = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
    this._image.enableRandomWrite = true;
    this._image.filterMode = FilterMode.Point;
    this._image.Create();

    this.BlendMode = blendMode;
  }

  public void Discard()
  {
    this.Image = null;
  }

  public void SetImageWithoutDestroy(RenderTexture rtex)
  {
    this._image = rtex;
  }

  public void CopyImageFromLayer(Layer layer)
  {
    GPGPUUtils.GPGPUTextureCopy(layer.Image, ref this._image);
  }

  /// <summary>
  /// layerのBlendModeに応じてレイヤーを合成する．
  /// </summary>
  /// <param name="layer"></param>
  public void CompositLayer(Layer layer)
  {
    switch (this.BlendMode)
    {
      case BlendMode.Add: GPGPUUtils.GPGPUCompositLayer_Add(layer.Image, ref _image); break;
    }
  }

  /// <summary>
  /// 描画レイヤーを適用する．
  /// </summary>
  /// <param name="drawLayer"></param>
  public void ApplyDrawLayer(MaskLayer drawLayer)
  {
    if(drawLayer.BlendMode == BlendMode.Add) GPGPUUtils.GPGPUApplyDraw(drawLayer.Image, ref _image, drawLayer.Color);
    else if(drawLayer.BlendMode == BlendMode.Erase) GPGPUUtils.GPGPUApplyDraw_Erace(drawLayer.Image, ref _image, drawLayer.Color.a);
  }

  public void ClearLayer()
  {
    var rtex = new RenderTexture(Image.width, Image.height, 0, RenderTextureFormat.ARGBFloat);
    rtex.enableRandomWrite = true;
    this._image.filterMode = FilterMode.Point;
    rtex.Create();
    this.Image = rtex;
  }
}

/// <summary>
/// マスク描画レイヤー
/// </summary>
public class MaskLayer
{
  public RenderTexture Image;

  public BlendMode BlendMode;

  public Color Color;

  public MaskLayer(Vector2Int resolution, BlendMode blendMode)
  {
    this.Image = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RFloat);
    this.Image.enableRandomWrite = true;
    this.Image.filterMode = FilterMode.Point;
    this.Image.Create();

    this.BlendMode = blendMode;

    this.Color = new Color();
  }

  public void Discard()
  {
    SetImage(null);
  }

  public void SetImage(RenderTexture value)
  {
    //前のテクスチャを開放する．
    if (Image) MonoBehaviour.Destroy(Image);
    //新しいテクスチャをセット
    Image = value;
  }

  public void SetImageWithoutDestroy(RenderTexture rtex)
  {
    this.Image = rtex;
  }

  public void ClearLayer()
  {
    var rtex = new RenderTexture(Image.width, Image.height, 0, RenderTextureFormat.RFloat);
    rtex.enableRandomWrite = true;
    this.Image.filterMode = FilterMode.Point;
    rtex.Create();
    SetImage(rtex);
  }
}