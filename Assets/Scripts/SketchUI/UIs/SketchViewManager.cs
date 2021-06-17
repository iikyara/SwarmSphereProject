using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SketchViewManager : MonoBehaviour
{
  /// <summary>
  /// UIManager
  /// </summary>
  public UIManager UIManager;

  /// <summary>
  /// スケッチビューを置く親
  /// </summary>
  public GameObject ViewParent;

  /// <summary>
  /// スケッチビューのサイズ
  /// </summary>
  public float Size;
  /// <summary>
  /// 3Dモデルの中心点からスケッチまでの最低距離
  /// </summary>
  public float OffsetDistance;
  /// <summary>
  /// 3Dモデルの中心点からスケッチまでの最高距離
  /// </summary>
  public float MaxDistance;

  /// <summary>
  /// スケッチビューオブジェクトのディクショナリ
  /// </summary>
  Dictionary<DrawableSketch, GameObject> sketchViews;
  DrawableSketch current;

  private void Awake()
  {
    sketchViews = new Dictionary<DrawableSketch, GameObject>();
  }

  /// <summary>
  /// 指定されたスケッチを追加する．
  /// </summary>
  /// <param name="dSketch"></param>
  public void AddSketch(DrawableSketch dSketch)
  {
    sketchViews.Add(dSketch, createSketchView(dSketch));
  }

  /// <summary>
  /// 指定されたスケッチを破棄する．
  /// </summary>
  /// <param name="dSketch"></param>
  public void RemoveSketch(DrawableSketch dSketch)
  {
    MonoBehaviour.Destroy(sketchViews[dSketch]);
    sketchViews.Remove(dSketch);
  }

  /// <summary>
  /// スケッチの変更を適用する．
  /// </summary>
  /// <param name="dSketch"></param>
  public void ApplySketchChange(DrawableSketch dSketch)
  {
    Utils.ChangeTexture(sketchViews[dSketch], dSketch.View.Image);
  }

  public void ChangeToSketchMode(DrawableSketch dSketch)
  {
    ChangeCurrentSketch(dSketch);
  }

  public void ChangeToGenerateMode()
  {
    sketchViews[current].SetActive(true);
  }

  public void ChangeCurrentSketch(DrawableSketch dSketch)
  {
    //スケッチを切り替え
    if (current != null) sketchViews[current].SetActive(true);
    sketchViews[dSketch].SetActive(false);
    current = dSketch;
  }

  public void OnClick_SketchView(DrawableSketch dSketch)
  {
    UIManager.OnClick_SketchView(dSketch);
  }

  /// <summary>
  /// スケッチからビューを生成する．
  /// </summary>
  /// <param name="dSketch"></param>
  /// <returns></returns>
  private GameObject createSketchView(DrawableSketch dSketch)
  {
    GameObject view = Utils.CreateQuad("SketchView", dSketch.View.Image);

    var position = dSketch.SCIP.Position;
    //ベクトル
    Vector3 direction = position.normalized;
    Vector3 sktPos = position - direction * (position.magnitude);
    //スケッチの大きさを求める
    float distanceCamAndSkt = (position - sktPos).magnitude;
    float sprite_height = 1f;
    //https://teratail.com/questions/191194
    float scale = distanceCamAndSkt * Mathf.Tan(dSketch.SCIP.FOV * 0.5f * Mathf.Deg2Rad) * 2 / sprite_height;
    if (dSketch.SCIP.Projection == CameraProjection.Orthographic)
    {
      scale = dSketch.SCIP.Size * 2 / sprite_height;
    }

    var lookat = dSketch.SCIP.LookAt;
    var pos = dSketch.SCIP.Position;

    view.transform.position = lookat + pos;
    view.transform.localScale = new Vector3(scale, scale, scale);
    view.transform.LookAt(lookat);

    //スクリプト（イベントリスナー）を付与
    var sv = view.AddComponent<SketchView>();
    sv.SketchViewManager = this;
    sv.DrawableSketch = dSketch;
    var cl = view.AddComponent<Clickable>();
    cl.Instance = sv;
    var mc = view.AddComponent<MeshCollider>();
    mc.convex = true;
    mc.isTrigger = true;

    Utils.SetParent(ViewParent, view);

    return view;
  }
}
