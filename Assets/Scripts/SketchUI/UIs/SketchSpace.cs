using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SketchMouse
{
  left,
  right,
  middle,
  wheel,
  other
}

public interface ISketchSpace
{
  void SketchSpace_MouseDown(Vector3 mousePos, SketchMouse sm);

  void SketchSpace_MouseUp(Vector3 mousePos, SketchMouse sm);

  void SketchSpace_MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm);

  void SketchSpace_MouseWheel(Vector2 delta);
}

public class SketchSpace : MonoBehaviour
{
  /// <summary>
  /// UIManagerのインスタンス
  /// </summary>
  public ISketchSpace UIManager;

  public List<System.Type> Clickables;
  /// <summary>
  /// マウスドラッグ中，1フレーム前のマウス位置を格納
  /// </summary>
  private Vector3 previousMousePosition;
  /// <summary>
  /// 何かしらのボタンが押されているか
  /// </summary>
  private bool isAnyDown;

  private void Start()
  {
    isAnyDown = false;
  }

  private void Update()
  {
    if (isAnyDown) OnMouseDragEvent();
  }

  public void OnMouseDownEvent()
  {
    //Debug.Log($"Down : {Input.mousePosition}");

    SketchMouse sm;
    if (Input.GetMouseButtonDown(0)) sm = SketchMouse.left;
    else if (Input.GetMouseButtonDown(1)) sm = SketchMouse.right;
    else if (Input.GetMouseButtonDown(2)) sm = SketchMouse.middle;
    else sm = SketchMouse.other;

    this.UIManager.SketchSpace_MouseDown(Input.mousePosition, sm);
    this.previousMousePosition = Input.mousePosition;

    //スケッチビュークリックイベント
    if (Input.GetMouseButtonDown(0))
    {
      clickJudgeOnMouseDown();
    }

    this.isAnyDown = true;
  }

  public void OnMouseUpEvent()
  {
    //Debug.Log($"Up : {Input.mousePosition}");

    SketchMouse sm;
    if (Input.GetMouseButtonUp(0)) sm = SketchMouse.left;
    else if (Input.GetMouseButtonUp(1)) sm = SketchMouse.right;
    else if (Input.GetMouseButtonUp(2)) sm = SketchMouse.middle;
    else sm = SketchMouse.other;

    this.UIManager.SketchSpace_MouseUp(Input.mousePosition, sm);

    //スケッチビュークリックイベント
    if (Input.GetMouseButtonUp(0))
    {
      clickJudgeOnMouseUp();
    }
    
    this.isAnyDown = false;
  }

  public void OnMouseDragEvent()
  {
    //Debug.Log($"Drag : {previousMousePosition} -> {Input.mousePosition}");

    SketchMouse sm;
    if (Input.GetMouseButton(0)) sm = SketchMouse.left;
    else if (Input.GetMouseButton(1)) sm = SketchMouse.right;
    else if (Input.GetMouseButton(2)) sm = SketchMouse.middle;
    else sm = SketchMouse.other;

    this.UIManager.SketchSpace_MouseDrag(previousMousePosition, Input.mousePosition, sm);
    previousMousePosition = Input.mousePosition;
  }

  public void OnWheelEvent()
  {
    this.UIManager.SketchSpace_MouseWheel(Input.mouseScrollDelta);
  }

  private Clickable judgedObj;

  private void clickJudgeOnMouseDown()
  {
    var obj = getClickObject();
    this.judgedObj = null;
    if (obj)
    {
      this.judgedObj = convertObj2Clikable(obj);
    }
  }

  private void clickJudgeOnMouseUp()
  {
    if (judgedObj == null) return;
    var obj = getClickObject();
    if (obj)
    {
      var upObj = convertObj2Clikable(obj);
      if (upObj == judgedObj)
      {
        this.judgedObj.OnClick();
      }
    }

  }

  private Clickable convertObj2Clikable(GameObject obj)
  {
    Clickable c = obj.GetComponent<Clickable>();
    return c;
  }

  private GameObject getClickObject()
  {
    GameObject result = null;
    // 左クリックされた場所のオブジェクトを取得
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit = new RaycastHit();
    if (Physics.Raycast(ray, out hit))
    {
      result = hit.collider.gameObject;
    }
    return result;
  }
}