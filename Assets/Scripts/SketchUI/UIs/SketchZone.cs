using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SketchZone : MonoBehaviour
{
  public UIManager UIManager;

  public RawImage BackGround;
  public RawImage Sketch;
  public RawImage ModelView;

  /// <summary>
  /// 3Dモデルを映すカメラ
  /// </summary>
  public ModelViewCamera ModelViewCamera;

  private RenderTexture modelViewTexture;
  
  /// <summary>
  /// 現在のスケッチインスタンスを保持
  /// </summary>
  private DrawableSketch current;

  private RectTransform rectTransform;

  private void Start()
  {
    this.rectTransform = this.GetComponent<RectTransform>();
  }

  public Color GetBGColor()
  {
    return BackGround.color;
  }

  public float GetSize()
  {
    return this.rectTransform.localScale.x;
  }

  public RectTransform GetRectTransform()
  {
    return this.rectTransform;
  }

  /* 更新メソッド */

  public void SetSketch(RenderTexture sketch)
  {
    Sketch.texture = sketch;
  }

  public void SetBGColor(Color bGColor)
  {
    BackGround.color = bGColor;
  }

  public void SetModelViewTransparency(float transparency)
  {
    ModelView.color = new Color(ModelView.color.r, ModelView.color.g, ModelView.color.b, transparency);
  }

  public void SetActive(bool isActive)
  {
    this.transform.gameObject.SetActive(isActive);
  }

  public void SetSize(float size)
  {
    this.rectTransform.localScale = new Vector3(size, size, size);
  }

  public void UpdateView(DrawableSketch sketch)
  {
    //テクスチャを更新（スケッチが変わっていた場合のみ）
    /*if (this.current == null || !object.ReferenceEquals(sketch.View.Image, this.current.View.Image))
    {
      this.Sketch.texture = sketch.View.Image;
    }*/
    this.Sketch.texture = sketch.View.Image;
    //this.Sketch.texture = sketch.PaintLayer.Image;
    //this.Sketch.texture = sketch.CurrentLayer.Image;
    //背景色を更新
    this.SetBGColor(sketch.BGColor);
    //3Dモデルの透明度を更新
    this.SetModelViewTransparency(sketch.ModelTransparency);
    //レンダーテクスチャを生成してカメラと3Dモデルに適用
    if(this.modelViewTexture == null
      || sketch.View.Image.width != this.modelViewTexture.width
      || sketch.View.Image.height != this.modelViewTexture.height
    )
    {
      MonoBehaviour.Destroy(this.modelViewTexture);
      this.modelViewTexture = new RenderTexture(sketch.View.Image.width, sketch.View.Image.height, 0, RenderTextureFormat.ARGB32);
      this.modelViewTexture.enableRandomWrite = true;
      this.modelViewTexture.Create();
    }
    this.ModelView.texture = this.modelViewTexture;
    this.ModelViewCamera.thisCamera.targetTexture = this.modelViewTexture;
    this.ModelViewCamera.SCIP = sketch.SCIP;
    this.ModelViewCamera.thisCamera.enabled = true;

    //現在のスケッチを更新
    this.current = sketch;
  }
}
