using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SketchModeController : MonoBehaviour
{
  public SketchZone SketchZone;
  public SketchManager SketchManager;
  public SketchPenManager SketchPenManager;
  public Canvas Canvas;
  public GameObject SketchRoot;

  public float MovingSpeed;
  public float ScalingSpeed;

  public void MouseDown(Vector3 mousePos, SketchMouse sm)
  {
    Vector2 sketchPos = screenPointToLocalPointInSketch(mousePos);
    //Ctrl＋左クリック
    if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
      //特になし
    }
    //Shift＋左クリック
    else if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
    {
      //特になし
    }
    //左クリック
    else if (sm == SketchMouse.left)
    {
      DrawSketch_MouseDown(sketchPos);
    }
    //中クリック
    else if (sm == SketchMouse.middle)
    {
      //特になし
    }
    //右クリック
    else if (sm == SketchMouse.right)
    {
      //特になし
    }
  }

  public void MouseUp(Vector3 mousePos, SketchMouse sm)
  {
    Vector2 sketchPos = screenPointToLocalPointInSketch(mousePos);
    //Ctrl＋左アップ
    if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
      //特になし
    }
    //Shift＋左アップ
    else if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
    {
      //特になし
    }
    //左アップ
    else if (sm == SketchMouse.left)
    {
      DrawSketch_MouseUp(sketchPos);
    }
    //中アップ
    else if (sm == SketchMouse.middle)
    {
      //特になし
    }
    //右アップ
    else if (sm == SketchMouse.right)
    {
      //特になし
    }
  }

  public void MouseDrag(Vector3 preMousePos, Vector3 curMousePos, SketchMouse sm)
  {
    Vector2 preSketchPos = screenPointToLocalPointInSketch(preMousePos);
    Vector2 curSketchPos = screenPointToLocalPointInSketch(curMousePos);
    //Ctrl＋左ドラッグ
    if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
      ScaleSketch(curSketchPos.y - preSketchPos.y);
    }
    //Shift＋左ドラッグ
    else if (sm == SketchMouse.left && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
    {
      MoveSketch(preSketchPos, curSketchPos);
    }
    //左ドラッグ
    else if (sm == SketchMouse.left)
    {
      DrawSketch_MouseDrag(preSketchPos, curSketchPos);
    }
    //中ドラッグ
    else if (sm == SketchMouse.middle)
    {
      MoveSketch(preSketchPos, curSketchPos);
    }
    //右ドラッグ
    else if (sm == SketchMouse.right)
    {
      //特になし
    }
  }

  public void MouseWheel(Vector2 delta)
  {
    ScaleSketch(delta.y);
  }

  public void MoveSketch(Vector2 preMousePos, Vector2 curMousePos)
  {
    float dx = curMousePos.x - preMousePos.x;
    float dy = curMousePos.y - preMousePos.y;

    Vector3 move = new Vector3(dx, dy, 0);

    this.SketchZone.GetRectTransform().position += move;
  }

  public void ScaleSketch(float scale)
  {
    float size = this.SketchZone.GetSize();
    float nextSize = size * Mathf.Exp(ScalingSpeed * scale);
    this.SketchZone.SetSize(nextSize);
    //Debug.Log($"{size} -> {nextSize}({this.SketchZone.GetSize()})");
  }

  public void DrawSketch_MouseDown(Vector2 mousePos)
  {
    Vector2Int pos = correctPoint(mousePos, this.SketchManager.CurrentSketch.SCIP.Resolution);
    this.SketchPenManager.CurrentPen.OnPenDown(pos, this.SketchManager.CurrentSketch);
    this.SketchManager.CurrentSketch.UpdateView();
  }

  public void DrawSketch_MouseUp(Vector2 mousePos)
  {
    Vector2Int pos = correctPoint(mousePos, this.SketchManager.CurrentSketch.SCIP.Resolution);
    this.SketchPenManager.CurrentPen.OnPenUp(pos, this.SketchManager.CurrentSketch);
    this.SketchManager.CurrentSketch.UpdateView();
  }

  public void DrawSketch_MouseDrag(Vector2 preMousePos, Vector2 curMousePos)
  {
    Vector2Int prePos = correctPoint(preMousePos, this.SketchManager.CurrentSketch.SCIP.Resolution);
    Vector2Int curPos = correctPoint(curMousePos, this.SketchManager.CurrentSketch.SCIP.Resolution);
    this.SketchPenManager.CurrentPen.OnPenMove(prePos, curPos, this.SketchManager.CurrentSketch);
    this.SketchManager.CurrentSketch.UpdateView();
  }

  /* 汎用メソッド */
  private Vector2 screenPointToLocalPointInSketch(Vector3 screenPoint)
  {
    //var canvasRect = this.Canvas.GetComponent<RectTransform>();
    var canvasRect = this.SketchRoot.GetComponent<RectTransform>();

    Vector2 localpoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, this.Canvas.worldCamera, out localpoint);

    return localpoint;
  }

  private Vector2Int correctPoint(Vector2 sketchPoint, Vector2Int resolution)
  {
    Vector2Int p = new Vector2Int(
      (int)(sketchPoint.x + resolution.x / 2),
      (int)(sketchPoint.y + resolution.y / 2)
    );
    return p;
  }
}
