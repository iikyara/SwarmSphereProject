using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VoxelSystem;

/// <summary>
/// オレオレ関数群
/// </summary>
public class Utils
{
  /// <summary>
  /// 乱数生成器
  /// </summary>
  private static System.Random rand = new System.Random();

  /// <summary>
  /// 指定された点がゲームオブジェクトのコリジョン内部にあるかを判定
  /// </summary>
  /// <param name="collision_object">対象のコライダーのついたゲームオブジェクト</param>
  /// <param name="target_point">調べる点</param>
  /// <returns>内部にあればTrue</returns>
  public static bool IsIncludedInCollision(GameObject collision_object, Vector3 target_point)
  {
    Vector3 Point;
    Vector3 Start = new Vector3(0, 100, 0); // This is defined to be some arbitrary point far away from the collider.
    Vector3 Goal = target_point; // This is the point we want to determine whether or not is inside or outside the collider.
    Vector3 Direction = Goal - Start; // This is the direction from start to goal.
    Collider collider = collision_object.GetComponent<Collider>();
    int layermask = 1 << collision_object.layer;
    Direction.Normalize();
    int Itterations = 0; // If we know how many times the raycast has hit faces on its way to the target and back, we can tell through logic whether or not it is inside.
    Point = Start;


    while (Point != Goal) // Try to reach the point starting from the far off point.  This will pass through faces to reach its objective.
    {
      RaycastHit hit;
      if (Physics.Linecast(Point, Goal, out hit, layermask)) // Progressively move the point forward, stopping everytime we see a new plane in the way.
      {
        Itterations++;
        Point = hit.point + (Direction / 100.0f); // Move the Point to hit.point and push it forward just a touch to move it through the skin of the mesh (if you don't push it, it will read that same point indefinately).
      }
      else
      {
        Point = Goal; // If there is no obstruction to our goal, then we can reach it in one step.
      }
    }
    while (Point != Start) // Try to return to where we came from, this will make sure we see all the back faces too.
    {
      RaycastHit hit;
      if (Physics.Linecast(Point, Start, out hit, layermask))
      {
        Itterations++;
        Point = hit.point + (-Direction / 100.0f);
      }
      else
      {
        Point = Start;
      }
    }

    return Itterations % 2 == 1;
  }

  /// <summary>
  /// target点がp1，p2，p3で構成される面のどちら側にいるかを判定する．
  /// 時計回りを表とする．
  /// 表がtrue
  /// </summary>
  /// <param name="p1">点１</param>
  /// <param name="p2">点２</param>
  /// <param name="p3">点３</param>
  /// <param name="target">ターゲット点</param>
  /// <returns>表がtrue，裏がfalse，面上にある時はtrue</returns>
  public static bool JudgeFrontOrBack(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 target)
  {
    Vector3 a = p2 - p1;
    Vector3 b = p3 - p1;
    Vector3 c = target - p1;
    Vector3 bcrossa = Vector3.Cross(b, a);
    // ↓本当は / (bcrossa.magnitude * c.magnitude)が必要だが，符号チェックのみなので計算省略
    float cos = Vector3.Dot(bcrossa, c);
    return cos >= 0f;
  }

  /// <summary>
  /// テクスチャを貼れるような板ポリを作成する
  /// 大きさはテクスチャの長い方を1としてアスペクト比はテクスチャと同じ
  /// </summary>
  /// <param name="name">オブジェクト名</param>
  /// <param name="texture">テクスチャ</param>
  /// <returns>テクスチャが貼られた板ポリオブジェクト</returns>
  public static GameObject CreateQuad(string name, Texture texture)
  {
    GameObject obj = new GameObject(name);
    Material mat = new Material(Shader.Find("UI/Unlit/Transparent"));
    Vector2 size = new Vector2(
      (float)texture.width / texture.height,
      1f
    );
    //mat.renderQueue = 2000;

    /*if(texture.width > texture.height)
    {
      size = new Vector2(
        1f,
        (float)texture.height / texture.width
      );
    }
    else
    {
      size = new Vector2(
        (float)texture.width / texture.height,
        1f
      );
    }*/

    Mesh mesh = new Mesh();
    mesh.vertices = new Vector3[] {
        new Vector3 (-size.x / 2, -size.y / 2, 0),
        new Vector3 (-size.x / 2,  size.y / 2, 0),
        new Vector3 ( size.x / 2, -size.y / 2, 0),
        new Vector3 ( size.x / 2,  size.y / 2, 0),
    };

    mesh.uv = new Vector2[] {
        new Vector2 (0, 0),
        new Vector2 (0, 1),
        new Vector2 (1, 0),
        new Vector2 (1, 1),
    };

    mesh.triangles = new int[] {
        0, 1, 2,
        1, 3, 2,
    };

    mat.SetTexture("_MainTex", texture);
    mesh.RecalculateNormals();

    obj.AddComponent<MeshFilter>().sharedMesh = mesh;
    obj.AddComponent<MeshRenderer>().material = mat;

    return obj;
  }

  public static void ChangeQuadAspect(GameObject quad, int width, int height)
  {
    Vector2 size = new Vector2(
      (float)width / height,
      1f
    );
    //mat.renderQueue = 2000;

    /*if(texture.width > texture.height)
    {
      size = new Vector2(
        1f,
        (float)texture.height / texture.width
      );
    }
    else
    {
      size = new Vector2(
        (float)texture.width / texture.height,
        1f
      );
    }*/

    var mesh = quad.GetComponent<MeshFilter>().sharedMesh;
    mesh.vertices = new Vector3[] {
        new Vector3 (-size.x / 2, -size.y / 2, 0),
        new Vector3 (-size.x / 2,  size.y / 2, 0),
        new Vector3 ( size.x / 2, -size.y / 2, 0),
        new Vector3 ( size.x / 2,  size.y / 2, 0),
    };
  }

  public static GameObject CreateTetrahedron(DPTetrahedron tetra)
  {
    GameObject obj = new GameObject("Tetrahedron");
    Material mat = new Material(Shader.Find("Standard"));

    Mesh mesh = new Mesh();
    mesh.vertices = new Vector3[] {
        tetra.Points[0].vec, tetra.Points[2].vec, tetra.Points[1].vec,
        tetra.Points[0].vec, tetra.Points[1].vec, tetra.Points[3].vec,
        tetra.Points[0].vec, tetra.Points[3].vec, tetra.Points[2].vec,
        tetra.Points[1].vec, tetra.Points[2].vec, tetra.Points[3].vec
    };

    mesh.triangles = new int[] {
        0, 1, 2,
        3, 4, 5,
        6, 7, 8,
        9, 10, 11
    };

    mat.color = Color.blue;

    mesh.RecalculateNormals();

    obj.AddComponent<MeshFilter>().sharedMesh = mesh;
    obj.AddComponent<MeshRenderer>().material = mat;

    return obj;
  }

  public static GameObject CreateVoxel(Mesh mesh, Bounds range, int resolution, out List<Voxel_t> voxels, out float unit, out bool[,,] vMap)
  {
    CPUVoxelizer.Voxelize(mesh, range, resolution, out voxels, out unit, out vMap);

    var vMesh = VoxelMesh.Build(voxels.ToArray(), unit, false);

    //親オブジェクト
    GameObject parent = new GameObject("Voxel Object");
    var mr = parent.AddComponent<MeshRenderer>();
    mr.material = new Material(Shader.Find("Standard"));
    var mf = parent.AddComponent<MeshFilter>();
    mf.sharedMesh = vMesh;

    return parent;
  }

  public static bool[,,] CalcVoxel(Mesh mesh, Bounds range, int resolution)
  {
    bool[,,] vMap;
    CPUVoxelizer.Voxelize(mesh, range, resolution, out _, out _, out vMap);
    return vMap;
  }

  public static GameObject ConvertVoxelMap2VoxelObject(bool[][][] vMap, Bounds size)
  {
    var xDensity = vMap.GetLength(0);
    var yDensity = vMap.GetLength(1);
    var zDensity = vMap.GetLength(2);
    var gridCubeSize = new Vector3(
      size.size.x / xDensity,
      size.size.y / yDensity,
      size.size.z / zDensity
    );
    var worldCentre = size.min + gridCubeSize / 2;
    var voxelRoot = new GameObject("Voxel Object");
    var rootTransform = voxelRoot.transform;

    for(int x = 0; x < xDensity; x++)
      for(int y = 0; y < yDensity; y++)
        for(int z = 0; z < zDensity; z++)
          if (vMap[x][y][z])
          {
            var pos = worldCentre + new Vector3(
              x * gridCubeSize.x,
              y * gridCubeSize.y,
              z * gridCubeSize.z
            );
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localScale = gridCubeSize;
            go.transform.SetParent(rootTransform, true);
          }

    return voxelRoot;
  }

  /// <summary>
  /// オブジェクトのテクスチャを変更する
  /// </summary>
  /// <param name="obj">オブジェクト</param>
  /// <param name="texture">テクスチャ</param>
  public static void ChangeTexture(GameObject obj, Texture2D texture)
  {
    var mr = obj.GetComponent<MeshRenderer>();
    if (mr == null) return;
    var mat = mr.material;
    mat.SetTexture("_MainTex", texture);
  }

  /// <summary>
  /// オブジェクトのテクスチャを変更する
  /// </summary>
  /// <param name="obj">オブジェクト</param>
  /// <param name="texture">テクスチャ</param>
  public static void ChangeTexture(GameObject obj, RenderTexture texture)
  {
    var mr = obj.GetComponent<MeshRenderer>();
    if (mr == null) return;
    var mat = mr.material;
    mat.SetTexture("_MainTex", texture);
  }

  public static Texture2D CreateMonocolorTexture(Color color, int width, int height)
  {
    var texture = new Texture2D(width, height);
    Color[] colors = new Color[width * height];
    for(int i = 0; i < width * height; i++)
    {
      colors[i] = color;
    }
    texture.SetPixels(colors);
    return texture;
  }

  public static RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat rtf)
  {
    RenderTexture rtex = new RenderTexture(width, height, 0, rtf);
    rtex.enableRandomWrite = true;
    rtex.filterMode = FilterMode.Point;
    rtex.Create();
    return rtex;
  }

  public static void CalcAndSetProjection(float aspect, float size, float fov, CameraProjection cp, ref Camera camera)
  {
    if (cp == CameraProjection.Orthographic)
    {
      // 平行投影のProjectionMatrixを計算する
      float orthoWidth = size * aspect;
      var projMatrix = Matrix4x4.Ortho(orthoWidth * -1, orthoWidth, size * -1, size, 0, 1000);

      // カメラのProjectionMatrixを上書き
      camera.projectionMatrix = projMatrix;
    }
    else if (cp == CameraProjection.Perspective)
    {
      var projMatrix = Matrix4x4.Perspective(fov, aspect, 0.3f, 1000f);
      camera.projectionMatrix = projMatrix;
    }
  }

  /// <summary>
  /// childの親をparentに設定する
  /// </summary>
  /// <param name="parent">親オブジェクト</param>
  /// <param name="child">子オブジェクト</param>
  public static void SetParent(GameObject parent, GameObject child)
  {
    //Debug.Log(parent + ", " + child);
    if(parent != null) child.transform.parent = parent.transform;
  }

  public static void SetMainCamera(Camera main)
  {
    foreach(var camera in Camera.allCameras)
    {
      camera.enabled = false;
    }
    main.enabled = true;
  }

  public static List<GameObject> GetAllChildren(GameObject obj)
  {
    List<GameObject> all = new List<GameObject>();
    GetChildren(obj, ref all);
    return all;
  }

  public static void GetChildren(GameObject obj, ref List<GameObject> children)
  {
    var cs = obj.GetComponentInChildren<Transform>();
    if (cs.childCount == 0) return;
    foreach(Transform c_o in cs)
    {
      children.Add(c_o.gameObject);
      GetChildren(c_o.gameObject, ref children);
    }
  }

  /// <summary>
  /// aとbの値を入れ替える
  /// </summary>
  /// <typeparam name="Type">なんでも</typeparam>
  /// <param name="a">変数1</param>
  /// <param name="b">変数2</param>
  public static void Swap<Type>(ref Type a, ref Type b)
  {
    Type temp = a;
    a = b;
    b = temp;
  }

  /// <summary>
  /// minとmaxの間でランダムな数値を返す
  /// maxは含まない
  /// </summary>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns>ランダムな数</returns>
  public static float RandomRange(float min, float max)
  {
    return (float)(rand.NextDouble() * (max - min) + min);
  }

  /// <summary>
  /// minとmaxの間でランダムな数値を返す
  /// maxは含まない
  /// </summary>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns>ランダムな数</returns>
  public static int RandomRange(int min, int max)
  {
    return rand.Next(min, max);
  }

  /// <summary>
  /// minとmaxの間でランダムな数値を返す
  /// maxは含まない
  /// ランダムの範囲は各要素を参照
  /// </summary>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns>ランダムな数値のベクトル</returns>
  public static Vector3 RandomRange(Vector3 min, Vector3 max)
  {
    return new Vector3(
      (float)(rand.NextDouble() * (max.x - min.x) + min.x),
      (float)(rand.NextDouble() * (max.y - min.y) + min.y),
      (float)(rand.NextDouble() * (max.z - min.z) + min.z)
    );
  }

  /// <summary>
  /// epsの確率でtrueを返す
  /// </summary>
  /// <param name="eps">trueの確率</param>
  /// <returns>確率epsでのbool値</returns>
  public static bool RandomBool(float eps)
  {
    return (float)(rand.NextDouble()) < eps;
  }

  /// <summary>
  /// 1次元配列の中身を出力する
  /// </summary>
  /// <typeparam name="T">ToStringが定義された適当な型</typeparam>
  /// <param name="array">型Tの1次元配列</param>
  public static void PrintArray<T>(IList<T> array)
  {
    Debug.Log(GetArrayString<T>(array));
  }

  /// <summary>
  /// 1次元配列の中身を文字列にして返す
  /// </summary>
  /// <typeparam name="T">ToStringが定義された適当な型</typeparam>
  /// <param name="array">型Tの1次元配列</param>
  /// <returns>配列を文字列にしたもの</returns>
  public static string GetArrayString<T>(IList<T> array)
  {
    //nullなら空文字列を返す
    if (array == null)
    {
      return "";
    }

    string result = "";
    foreach (T item in array)
    {
      result += item.ToString() + "\n";
    }
    return result;
  }

  /// <returns>配列を文字列にしたもの</returns>
  public static string GetArrayStringNonReturn<T>(IList<T> array)
  {
    //nullなら空文字列を返す
    if(array == null)
    {
      return "";
    }

    string result = "";
    foreach (T item in array)
    {
      result += item.ToString() + ", ";
    }
    return result;
  }

  public static string GetDictionaryString<T1, T2>(IDictionary<T1, T2> dict)
  {
    if (dict == null) return "";

    string result = "";
    foreach(var item in dict)
    {
      result += $"{item.Key} : {item.Value}\n";
    }
    return result;
  }

  /// <summary>
  /// 呼び出し元情報を出力する
  /// </summary>
  public static void PrintStackTrace()
  {
    Debug.Log(System.Environment.StackTrace);
  }

  public static string GetStackTrace()
  {
    return System.Environment.StackTrace;
  }

  /// <summary>
  /// Enumを文字列に変換する
  /// </summary>
  /// <param name="t"></param>
  /// <param name="enumObject"></param>
  /// <returns></returns>
  public static string EnumToString(System.Type t, object enumObject)
  {
    return System.Enum.GetName(t, enumObject);
  }

  /// <summary>
  /// 文字列をEnumに変換する
  /// </summary>
  /// <typeparam name="EnumType">対象のEnum</typeparam>
  /// <param name="t">対象のEnum</param>
  /// <param name="enumString">文字列</param>
  /// <returns></returns>
  public static (bool, EnumType) StringToEnum<EnumType>(System.Type t, string enumString) where EnumType : System.Enum
  {
    bool success = true;
    EnumType result = (EnumType)(System.Enum.GetValues(t).GetValue(0));
    try
    {
      result = (EnumType)System.Enum.Parse(t, enumString);
    }
    catch (System.Exception)
    {
      success = false;
    }
    return (success, result);
    
  }

  /// <summary>
  /// 商と剰余を計算する．
  /// </summary>
  /// <param name="a">割られる数</param>
  /// <param name="b">割る数</param>
  /// <returns>Item1 : 商, Item2 : 剰余</returns>
  public static System.Tuple<int, int> Div(int a, int b)
  {
    int q = a / b;
    int p = a % b;
    return new System.Tuple<int, int>(q, p);
  }

  /// <summary>
  /// Vector2IntをInt配列に変換する．
  /// </summary>
  /// <param name="vec"></param>
  /// <returns></returns>
  public static int[] Vector2IntToIntArray(Vector2Int vec)
  {
    return new int[] { vec.x, vec.y };
  }

  public static float[] ColorToFloatArray(Color color)
  {
    return new float[] { color.r, color.g, color.b, color.a };
  }

  /// <summary>
  /// カメラの映像をキャプチャーしてテクスチャに変換する
  /// 参考：https://qiita.com/tilyakuda/items/67fe8e787ab5e5679ddb
  /// </summary>
  /// <param name="camera">対象のカメラ</param>
  /// <returns>キャプチャ画像</returns>
  public static Texture2D CaptureCameraImage(Camera camera, int width, int height)
  {
    //カメラ映像のレンダリング用にレンダーテクスチャを作成
    var rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
    //保存用テクスチャ
    Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, false);
    //元の設定を保存しておく
    var temp_crt = camera.targetTexture;
    var temp_art = RenderTexture.active;
    //カメラとアクティブレンダーテクスチャにそれぞれレンダリング用テクスチャをセット
    camera.targetTexture = rt;
    camera.Render();  //手動でカメラをレンダリング
    RenderTexture.active = rt;
    //テクスチャに保存
    texture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    texture2D.Apply();
    //設定を元に戻す
    camera.targetTexture = temp_crt;
    RenderTexture.active = temp_art;
    //レンダーテクスチャを開放
    MonoBehaviour.Destroy(rt);

    return texture2D;
  }

  /// <summary>
  /// RenderTextureをTexture2Dに変換する
  /// それなりに遅いらしい
  /// https://light11.hatenadiary.com/entry/2018/04/19/192905
  /// </summary>
  /// <param name="rt"></param>
  /// <returns></returns>
  public static Texture2D RenderTextureToTexture2D(RenderTexture rt, TextureFormat tf = TextureFormat.RGBAFloat)
  {
    // アクティブなレンダーテクスチャをキャッシュしておく
    var currentRT = RenderTexture.active;

    // アクティブなレンダーテクスチャを一時的にTargetに変更する
    RenderTexture.active = rt;

    // Texture2D.ReadPixels()によりアクティブなレンダーテクスチャのピクセル情報をテクスチャに格納する
    var texture = new Texture2D(rt.width, rt.height, tf, false);
    texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    texture.Apply();

    // アクティブなレンダーテクスチャを元に戻す
    RenderTexture.active = currentRT;

    return texture;
  }

  /// <summary>
  /// 2つのテクスチャのアルファ値を合成する．
  /// </summary>
  /// <param name="texture1">テクスチャ１</param>
  /// <param name="texture2">テクスチャ２</param>
  /// <returns>アルファ合成後のテクスチャ</returns>
  public static Texture2D AlphaJoin(Texture2D texture1, Texture2D texture2)
  {
    int width = Mathf.Min(texture1.width, texture2.width);
    int height = Mathf.Min(texture1.height, texture2.height);
    Texture2D texture = new Texture2D(width, height);
    for(int y = 0; y < height; y++)
    {
      for(int x = 0; x < width; x++)
      {
        Color c1 = texture1.GetPixel(x, y);
        Color c2 = texture2.GetPixel(x, y);
        texture.SetPixel(x, y, new Color(0f, 0f, 0f, c1.a * c2.a));
      }
    }
    return texture;
  }

  /// <summary>
  /// テクスチャの透明ではないピクセルの合計とアルファ値の合計を返す．
  /// </summary>
  /// <param name="texture">テクスチャ</param>
  /// <returns>Tuple型(ピクセルの合計，アルファ値の合計)</returns>
  public static System.Tuple<float, int> CountPixelAndSumArea(Texture2D texture)
  {
    //論理プロセッサの数に分割する
    int procCnt = System.Environment.ProcessorCount;
    float[] sums = new float[procCnt];
    int[] counts = new int[procCnt];
    Color[] colors = texture.GetPixels();
    int stripe = Mathf.CeilToInt((float)colors.Length / procCnt);
    //Debug.Log(procCnt + ", " + counts.Length + ", " + colors.Length + ", " + stripe);
    Parallel.For(0, procCnt, i =>
    {
      int offset = i * stripe;
      for (int j = 0; j < stripe; j++)
      {
        if (j + offset >= colors.Length) break;
        if (colors[j + offset].a > 0f) counts[i]++;
        sums[i] += colors[j + offset].a;
      }
    });
    int count = 0;
    float sum = 0f;
    foreach (var c in counts) count += c;
    foreach (var c in sums) sum += c;
    return new System.Tuple<float, int>(sum, count);
  }

  /// <summary>
  /// テクスチャの透明でないピクセルを数える
  /// </summary>
  /// <param name="texture">テクスチャ</param>
  /// <returns>透明でないピクセル数</returns>
  public static int CountNonTransparentPixel_CPUParallel(Texture2D texture)
  {
    //論理プロセッサの数に分割する
    int procCnt = System.Environment.ProcessorCount;
    int[] counts = new int[procCnt];
    Color[] colors = texture.GetPixels();
    int stripe = Mathf.CeilToInt((float)colors.Length / procCnt);
    //Debug.Log(procCnt + ", " + counts.Length + ", " + colors.Length + ", " + stripe);
    Parallel.For(0, procCnt, i =>
    {
      int offset = i * stripe;
      for(int j = 0; j < stripe; j++)
      {
        if (j + offset >= colors.Length) break;
        if (colors[j + offset].a > 0f) counts[i]++;
      }
    });
    int count = 0;
    foreach (var c in counts) count += c;
    return count;
  }

  /// <summary>
  /// テクスチャの透明でないピクセルのα値の合計を計算する
  /// </summary>
  /// <param name="texture">テクスチャ</param>
  /// <returns>アルファ値の合計</returns>
  public static float SumAlpha_CPUParallel(Texture2D texture)
  {
    //論理プロセッサの数に分割する
    int procCnt = System.Environment.ProcessorCount;
    float[] sums = new float[procCnt];
    Color[] colors = texture.GetPixels();
    int stripe = Mathf.CeilToInt((float)colors.Length / procCnt);
    //Debug.Log(procCnt + ", " + counts.Length + ", " + colors.Length + ", " + stripe);
    Parallel.For(0, procCnt, i =>
    {
      int offset = i * stripe;
      for (int j = 0; j < stripe; j++)
      {
        if (j + offset >= colors.Length) break;
        sums[i] += colors[j + offset].a;
      }
    });
    float sum = 0f;
    foreach (var c in sums) sum += c;
    return sum;
  }

  /// <summary>
  /// テクスチャの透明でないピクセルを数える
  /// </summary>
  /// <param name="texture">テクスチャ</param>
  /// <returns>透明でないピクセル数</returns>
  public static int CountNonTransparentPixel(Texture2D texture)
  {
    int count = 0;
    foreach (var pixel in texture.GetPixels())
    {
      if (pixel.a > 0f)
      {
        count++;
      }
    }
    return count;
  }

  /// <summary>
  /// テクスチャの透明でないピクセルを数える
  /// </summary>
  /// <param name="texture">テクスチャ</param>
  /// <returns>透明でないピクセル数</returns>
  public static int CountNonTransparentPixel(float[] texture)
  {
    int count = 0;
    foreach (var pixel in texture)
    {
      if (pixel > 0f)
      {
        count++;
      }
    }
    return count;
  }

  /// <summary>
  /// TextureScaling用
  /// </summary>
  public enum ScalingMode
  {
    Point,
    Bilinear
  }

  /// <summary>
  /// テクスチャの解像度を変更する
  /// </summary>
  /// <param name="tex">テクスチャ</param>
  /// <param name="newWidth">変更後の横解像度</param>
  /// <param name="newHeight">変更後の縦解像度</param>
  /// <param name="sm">解像度変更のモード（Point or Bilinear）</param>
  /// <returns>解像度変更後のテクスチャ</returns>
  public static Texture2D TextureScaling(Texture2D tex, int newWidth, int newHeight, ScalingMode sm)
  {
    var scaledTex = GPGPUUtils.GPGPUTextureCopy(tex);
    if (sm == ScalingMode.Point)
    {
      TextureScale.Point(scaledTex, newWidth, newHeight);
    }
    else if(sm == ScalingMode.Bilinear)
    {
      TextureScale.Bilinear(scaledTex, newWidth, newHeight);
    }
    else
    {
      TextureScale.Bilinear(scaledTex, newWidth, newHeight);
    }
    return scaledTex;
  }

  /// <summary>
  /// カメラ上で点がどの位置にあるかを返す．
  /// カメラのピクセルをwidthとheightで指定する．
  /// </summary>
  /// <param name="position">ワールド座標</param>
  /// <param name="camera">対象カメラ</param>
  /// <param name="width">横幅</param>
  /// <param name="height">縦幅</param>
  /// <returns></returns>
  public static Vector3 WorldToScreen(Vector3 position, Camera camera, int width, int height)
  {
    var wts = WorldToScreen(position, camera);
    float x = wts.x * width;
    float y = wts.y * height;
    var result = new Vector3(x, y, wts.z);
    return result;
  }

  /// <summary>
  /// カメラ上で点がどの位置にいるかを返す．
  /// </summary>
  /// <param name="position">ワールド座標</param>
  /// <param name="camera">対象カメラ</param>
  /// <returns>(-1~1, -1~1, カメラからの距離)の値で返す</returns>
  public static Vector3 WorldToScreen(Vector3 position, Camera camera)
  {
    //var t = camera.projectionMatrix * new Vector4(position.x, position.y, position.z, 1f);
    //return RectTransformUtility.WorldToScreenPoint(camera, position);
    var wtsp = camera.WorldToScreenPoint(position);
    float x = wtsp.x / camera.pixelWidth;
    if (x < 0f) x = 0f;
    else if (x > 1f) x = 1f;
    float y = wtsp.y / camera.pixelHeight;
    if (y < 0f) y = 0f;
    else if (y > 1f) y = 1f;
    float z = wtsp.z;
    var result = new Vector3(x, y, z);
    return result;
  }

  /// <summary>
  /// 撮影用にオブジェクトを設定する
  /// </summary>
  /// <param name="obj">対象オブジェクト</param>
  /// <param name="capture">撮影用マテリアル</param>
  public static void PrepareToCapture(GameObject obj, Material capture)
  {
    obj.layer = LayerMask.NameToLayer("ForDrawable");
    var mr = obj.GetComponent<MeshRenderer>();
    if (mr != null) mr.material = capture;
  }

  /// <summary>
  /// プレビュー用にオブジェクトを設定する
  /// </summary>
  /// <param name="obj">対象オブジェクト</param>
  /// <param name="view">プレビュー用マテリアル</param>
  public static void AfterCapture(GameObject obj, Material view)
  {
    obj.layer = 0;
    var mr = obj.GetComponent<MeshRenderer>();
    if (mr != null) mr.material = view;
  }

  /// <summary>
  /// 「#rrggbb」の形式からColorクラスに変換する
  /// </summary>
  /// <param name="colorCode"></param>
  /// <returns></returns>
  public static Color ColorCodeToColor(string colorCode)
  {
    Color color = new Color();
    if (colorCode.Length == 7)
    {
      float r, g, b;
      string color_str = colorCode.Replace("#", "");
      try
      {
        //Rをパース
        string hex_string = color_str[0].ToString() + color_str[1].ToString();
        int hex = System.Int32.Parse(hex_string, System.Globalization.NumberStyles.HexNumber);
        r = (float)hex / 255f;
        //Gをパース
        hex_string = color_str[2].ToString() + color_str[3].ToString();
        hex = System.Int32.Parse(hex_string, System.Globalization.NumberStyles.HexNumber);
        g = (float)hex / 255f;
        //Bをパース
        hex_string = color_str[4].ToString() + color_str[5].ToString();
        hex = System.Int32.Parse(hex_string, System.Globalization.NumberStyles.HexNumber);
        b = (float)hex / 255f;
        color = new Color(r, g, b);
      }
      catch
      {
        Debug.Log("パース失敗 : " + colorCode);
      }
    }
    return color;
  }

  /// <summary>
  /// Colorクラスから「#rrggbb」の形式に変換する．
  /// </summary>
  /// <param name="color"></param>
  /// <returns></returns>
  public static string ColorToColorCode(Color color)
  {
    string hex = "#";
    //Rを変換
    int value = (int)(color.r * 255f);
    hex += value.ToString("x").PadLeft(2, '0');
    //Gを変換
    value = (int)(color.g * 255f);
    hex += value.ToString("x").PadLeft(2, '0');
    //Bを変換
    value = (int)(color.b * 255f);
    hex += value.ToString("x").PadLeft(2, '0');
    return hex;
  }
}

/// <summary>
/// オレオレストップウォッチ
/// </summary>
public class MyStopwatch : System.Diagnostics.Stopwatch
{
  /// <summary>
  /// ストップウォッチの結果を表示
  /// オプションで前につけるラベルを指定できる．
  /// </summary>
  /// <param name="name">ラベル</param>
  public void ShowResult(params string[] name)
  {
    Debug.Log(GetResultString(name));
  }

  public string GetResultString(params string[] name)
  {
    var ts = Elapsed;
    string msg = "";
    foreach (var n in name) msg += n;
    msg += " : ";
    return $"{msg}{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒";
  }
}