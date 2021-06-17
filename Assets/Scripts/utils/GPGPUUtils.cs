using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ThreadSize
{
  public int x;
  public int y;
  public int z;

  public ThreadSize(uint x, uint y, uint z)
  {
    this.x = (int)x;
    this.y = (int)y;
    this.z = (int)z;
  }
}

public static class GPGPUUtils
{
  /// <summary>
  /// GPGPU用コンピュートシェーダ
  /// </summary>
  public static ComputeShader UtilsComputeShader
  {
    get { return _UtilsComputeShader; }
    set
    {
      //値のセット
      _UtilsComputeShader = value;
      //カーネル番号の取得
      kernelIndex_TestMethod = _UtilsComputeShader.FindKernel("TestMethod");
      kernelIndex_TestTextureCopy = _UtilsComputeShader.FindKernel("TestTextureCopy");
      kernelIndex_MaskCopy = _UtilsComputeShader.FindKernel("MaskCopy");
      kernelIndex_AlphaJoin = _UtilsComputeShader.FindKernel("AlphaJoin");
      kernelIndex_AlphaJoin2 = _UtilsComputeShader.FindKernel("AlphaJoin2");
      kernelIndex_DrawPoint = _UtilsComputeShader.FindKernel("DrawPoint");
      kernelIndex_CompositLayer_Add = _UtilsComputeShader.FindKernel("CompositLayer_Add");
      kernelIndex_DrawPointOnMask = _UtilsComputeShader.FindKernel("DrawPointOnMask");
      kernelIndex_ApplyDraw = _UtilsComputeShader.FindKernel("ApplyDraw");
      kernelIndex_ApplyDraw_Erace = _UtilsComputeShader.FindKernel("ApplyDraw_Erace");
      //カーネルのスレッド数を取得
      uint x, y, z;
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_TestMethod, out x, out y, out z);
      kernelThreadSize_TestMethod= new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_TestTextureCopy, out x, out y, out z);
      kernelThreadSize_TestTextureCopy = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_MaskCopy, out x, out y, out z);
      kernelThreadSize_MaskCopy = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_AlphaJoin, out x, out y, out z);
      kernelThreadSize_AlphaJoin = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_AlphaJoin2, out x, out y, out z);
      kernelThreadSize_AlphaJoin2 = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_DrawPoint, out x, out y, out z);
      kernelThreadSize_DrawPoint = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_CompositLayer_Add, out x, out y, out z);
      kernelThreadSize_CompositLayer_Add = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_DrawPointOnMask, out x, out y, out z);
      kernelThreadSize_DrawPointOnMask = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_ApplyDraw, out x, out y, out z);
      kernelThreadSize_ApplyDraw = new ThreadSize(x, y, z);
      _UtilsComputeShader.GetKernelThreadGroupSizes(kernelIndex_ApplyDraw_Erace, out x, out y, out z);
      kernelThreadSize_ApplyDraw_Erace = new ThreadSize(x, y, z);
    }
  }
  private static ComputeShader _UtilsComputeShader;
  private static ThreadSize kernelThreadSize_TestMethod;
  private static ThreadSize kernelThreadSize_TestTextureCopy;
  private static ThreadSize kernelThreadSize_MaskCopy;
  private static ThreadSize kernelThreadSize_AlphaJoin;
  private static ThreadSize kernelThreadSize_AlphaJoin2;
  private static ThreadSize kernelThreadSize_DrawPoint;
  private static ThreadSize kernelThreadSize_CompositLayer_Add;
  private static ThreadSize kernelThreadSize_DrawPointOnMask;
  private static ThreadSize kernelThreadSize_ApplyDraw;
  private static ThreadSize kernelThreadSize_ApplyDraw_Erace;
  private static int kernelIndex_TestMethod;
  private static int kernelIndex_TestTextureCopy;
  private static int kernelIndex_MaskCopy;
  private static int kernelIndex_AlphaJoin;
  private static int kernelIndex_AlphaJoin2;
  private static int kernelIndex_DrawPoint;
  private static int kernelIndex_CompositLayer_Add;
  private static int kernelIndex_DrawPointOnMask;
  private static int kernelIndex_ApplyDraw;
  private static int kernelIndex_ApplyDraw_Erace;
  private static ComputeBuffer TestMethod_intBuffer;
  private static ComputeBuffer AlphaJoin_TextureBuffer;
  private static ComputeBuffer AlphaJoin2_TextureBuffer;
  private static ComputeBuffer DrawPoint_TextureBuffer;

  /// <summary>
  /// 2つのテクスチャのアルファ値をGPU演算を使用して合成する．
  /// </summary>
  /// <param name="texture1">テクスチャ１</param>
  /// <param name="texture2">テクスチャ２</param>
  /// <returns>アルファ合成後のテクスチャ</returns>
  public static Texture2D AlphaJoin(Texture2D texture1, Texture2D texture2)
  {
    // 結果格納用テクスチャの作成
    int width = Mathf.Min(texture1.width, texture2.width);
    int height = Mathf.Min(texture1.height, texture2.height);
    var rtex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    rtex.enableRandomWrite = true;
    rtex.Create();

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin, "TextureBuffer", rtex);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin, "Texture1", texture1);
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin, "Texture2", texture2);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_AlphaJoin, width / kernelThreadSize_AlphaJoin.x, height / kernelThreadSize_AlphaJoin.y, 1);

    var result = Utils.RenderTextureToTexture2D(rtex);
    MonoBehaviour.Destroy(rtex);
    return result;
  }

  /// <summary>
  /// 2つのテクスチャのアルファ値を減衰値付きでGPU演算を使用して合成する．
  /// 2つ目のテクスチャのアルファ値がある部分を減衰する．
  /// </summary>
  /// <param name="texture1">テクスチャ１</param>
  /// <param name="texture2">テクスチャ２</param>
  /// <param name="attenuation">減衰値</param>
  /// <returns>アルファ合成後のテクスチャ</returns>
  public static Texture2D AlphaJoin2(Texture2D texture1, Texture2D texture2, float attenuation)
  {
    // 結果格納用テクスチャの作成
    int width = Mathf.Min(texture1.width, texture2.width);
    int height = Mathf.Min(texture1.height, texture2.height);
    var rtex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    rtex.enableRandomWrite = true;
    rtex.Create();

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin2, "TextureBuffer", rtex);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin2, "Texture1", texture1);
    UtilsComputeShader.SetTexture(kernelIndex_AlphaJoin2, "Texture2", texture2);
    //入力値の設定
    UtilsComputeShader.SetFloat("Attenuation", attenuation);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_AlphaJoin2, width / kernelThreadSize_AlphaJoin2.x, height / kernelThreadSize_AlphaJoin2.y, 1);

    var result = Utils.RenderTextureToTexture2D(rtex);
    MonoBehaviour.Destroy(rtex);
    return result;
  }

  /// <summary>
  /// GPGPUのテストメソッド
  /// </summary>
  public static void GPGPUTest()
  {
    //出力用バッファの準備
    TestMethod_intBuffer = new ComputeBuffer(4, sizeof(int)); //<- 要素数, ストライド（間隔）
    UtilsComputeShader.SetBuffer(kernelIndex_TestMethod, "intBuffer", TestMethod_intBuffer);
    //入力値の設定
    UtilsComputeShader.SetInt("intValue", 1);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_TestMethod, 1, 1, 1); //<-カーネル番号, グループ数x, y, z
    //結果の取得
    int[] result = new int[4];
    TestMethod_intBuffer.GetData(result);
    //バッファーの開放
    TestMethod_intBuffer.Release();
    //結果の表示
    string disp = "結果 : ";
    for (int i = 0; i < 4; i++) disp += "int[" + i + "] : " + result[i] + ", ";
    Debug.Log(disp);
  }

  public static Texture2D GPGPUTextureCopy(Texture texture)
  {
    // 結果格納用テクスチャの作成
    int width = texture.width;
    int height = texture.height;
    var rtex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    rtex.enableRandomWrite = true;
    rtex.Create();

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "TextureBuffer", rtex);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "Texture1", texture);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_TestTextureCopy, width / kernelThreadSize_TestTextureCopy.x, height / kernelThreadSize_TestTextureCopy.y, 1);

    var result = Utils.RenderTextureToTexture2D(rtex);
    MonoBehaviour.Destroy(rtex);
    return result;
  }

  public static void GPGPUTextureCopy(Texture2D texture, ref RenderTexture dist)
  {
    // 結果格納用テクスチャの作成
    int width = texture.width;
    int height = texture.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "TextureBuffer", dist);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "Texture1", texture);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_TestTextureCopy, width / kernelThreadSize_TestTextureCopy.x, height / kernelThreadSize_TestTextureCopy.y, 1);
  }

  public static void GPGPUTextureCopy(RenderTexture src, ref RenderTexture dist)
  {
    // 結果格納用テクスチャの作成
    int width = dist.width;
    int height = dist.height;
    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "TextureBuffer", dist);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_TestTextureCopy, "Texture1", src);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_TestTextureCopy, width / kernelThreadSize_TestTextureCopy.x, height / kernelThreadSize_TestTextureCopy.y, 1);
  }

  public static void GPGPUMaskCopy(Texture2D src, ref RenderTexture dist)
  {
    // 結果格納用テクスチャの作成
    int width = dist.width;
    int height = dist.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_MaskCopy, "MaskBuffer", dist);
    //入力テクスチャをセット
    UtilsComputeShader.SetTexture(kernelIndex_MaskCopy, "Texture1", src);
    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_MaskCopy, width / kernelThreadSize_MaskCopy.x, height / kernelThreadSize_MaskCopy.y, 1);
  }

  public static void GPGPUDrawPoint(Texture2D penTexture,
    Vector2Int Point, ref RenderTexture drawBuffer
  )
  {
    // 結果格納用テクスチャの作成
    int width = drawBuffer.width;
    int height = drawBuffer.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_DrawPoint, "TextureBuffer", drawBuffer);
    //各GPU変数をセット
    UtilsComputeShader.SetInts("TextureSize", new int[] { drawBuffer.width, drawBuffer.height });
    UtilsComputeShader.SetTexture(kernelIndex_DrawPoint, "PenTexture", penTexture);
    UtilsComputeShader.SetInts("PenTextureSize", new int[] { penTexture.width, penTexture.height });
    UtilsComputeShader.SetInts("Point", Utils.Vector2IntToIntArray(Point));

    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_DrawPoint, width / kernelThreadSize_DrawPoint.x, height / kernelThreadSize_DrawPoint.y, 1);
  }

  public static void GPGPUCompositLayer_Add(RenderTexture otherLayer, ref RenderTexture layer)
  {
    // 結果格納用テクスチャの作成
    int width = layer.width;
    int height = layer.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_CompositLayer_Add, "TextureBuffer", layer);
    //各GPU変数をセット
    UtilsComputeShader.SetInts("TextureSize", new int[] { width, height });
    UtilsComputeShader.SetTexture(kernelIndex_CompositLayer_Add, "LayerTexture", otherLayer);
    UtilsComputeShader.SetInts("PenTextureSize", new int[] { otherLayer.width, otherLayer.height });

    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_CompositLayer_Add, width / kernelThreadSize_CompositLayer_Add.x, height / kernelThreadSize_CompositLayer_Add.y, 1);
  }

  /// <summary>
  /// 1次元テクスチャ上に
  /// </summary>
  /// <param name="penTexture"></param>
  /// <param name="drawBuffer"></param>
  public static void GPGPUDrawPointOnMask(Texture2D penTexture, ref RenderTexture drawBuffer, Vector2Int point, float thick)
  {
    // 結果格納用テクスチャの作成
    int width = drawBuffer.width;
    int height = drawBuffer.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_DrawPointOnMask, "MaskBuffer", drawBuffer);
    //各GPU変数をセット
    UtilsComputeShader.SetTexture(kernelIndex_DrawPointOnMask, "PenTexture", penTexture);
    UtilsComputeShader.SetInts("PenTextureSize", new int[] { penTexture.width, penTexture.height });
    UtilsComputeShader.SetInts("Point", Utils.Vector2IntToIntArray(point));
    UtilsComputeShader.SetFloat("Thick", thick);

    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_DrawPointOnMask, width / kernelThreadSize_DrawPointOnMask.x, height / kernelThreadSize_DrawPointOnMask.y, 1);
  }

  public static void GPGPUApplyDraw(RenderTexture drawBuffer, ref RenderTexture layer, Color color)
  {
    // 結果格納用テクスチャの作成
    int width = layer.width;
    int height = layer.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_ApplyDraw, "TextureBuffer", layer);
    //各GPU変数をセット
    UtilsComputeShader.SetTexture(kernelIndex_ApplyDraw, "MaskTexture", drawBuffer);
    UtilsComputeShader.SetFloats("Color", Utils.ColorToFloatArray(color));

    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_ApplyDraw, width / kernelThreadSize_ApplyDraw.x, height / kernelThreadSize_ApplyDraw.y, 1);
  }

  public static void GPGPUApplyDraw_Erace(RenderTexture drawBuffer, ref RenderTexture layer, float transparency)
  {
    // 結果格納用テクスチャの作成
    int width = layer.width;
    int height = layer.height;

    //出力バッファの設定
    UtilsComputeShader.SetTexture(kernelIndex_ApplyDraw_Erace, "TextureBuffer", layer);
    //各GPU変数をセット
    UtilsComputeShader.SetTexture(kernelIndex_ApplyDraw_Erace, "MaskTexture", drawBuffer);
    UtilsComputeShader.SetFloat("Transparency", transparency);

    //カーネルの実行
    UtilsComputeShader.Dispatch(kernelIndex_ApplyDraw_Erace, width / kernelThreadSize_ApplyDraw_Erace.x, height / kernelThreadSize_ApplyDraw_Erace.y, 1);
  }
}
