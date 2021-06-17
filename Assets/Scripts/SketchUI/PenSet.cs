using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenSet : MonoBehaviour
{
  public Texture2D PenTexture;

  private int currentThick;
  private Texture2D _resizedPenTexture;
  public  Texture2D ResizedPenTexture
  {
    get { return _resizedPenTexture; }
    set
    {
      if (!_resizedPenTexture) MonoBehaviour.Destroy(_resizedPenTexture);
      _resizedPenTexture = value;
    }
  }

  public void Awake()
  {
    this.currentThick = PenTexture.width;
    this.ResizedPenTexture = PenTexture;
  }

  public Texture2D GetResizedPenTexture(int thick)
  {
    if (thick != this.currentThick)
    {
      //テクスチャをリサイズ
      this.ResizedPenTexture = GPGPUUtils.GPGPUTextureCopy(this.PenTexture);
      TextureScale.Bilinear(this._resizedPenTexture, thick, thick);
      //更新
      this.currentThick = thick;
    }
    return this.ResizedPenTexture;
  }
}
