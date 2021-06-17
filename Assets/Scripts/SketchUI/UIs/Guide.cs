using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
  public Vector3 Size;
  public Vector2 Width;
  public Material LineMaterial;

  private GameObject[] lineObjs;
  private LineRenderer[] lines;
  private Vector3 currentSize;
  private Vector2 currentWidth;
  private Material currentLineMaterial;

  // Start is called before the first frame update
  void Start()
  {
    lineObjs = new GameObject[12];
    lines = new LineRenderer[12];
    //6辺分の線を生成する．
    for(int i = 0; i < 12; i++)
    {
      lineObjs[i] = new GameObject("guide line");
      Utils.SetParent(this.transform.gameObject, lineObjs[i]);
      lines[i] = lineObjs[i].AddComponent<LineRenderer>();
      lines[i].positionCount = 2;
      lines[i].SetPositions(new Vector3[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 1f)
      });
    }
  }

  // Update is called once per frame
  void Update()
  {
    UpdateSize();
    UpdateWidth();
    UpdateLineMaterial();
  }

  private void UpdateSize()
  {
    if (Size == currentSize) return;

    float x2 = Size.x / 2;
    float y2 = Size.y / 2;
    float z2 = Size.z / 2;
    lines[0].SetPositions(new Vector3[] { new Vector3(x2, y2, -z2), new Vector3(x2, y2, z2) });
    lines[1].SetPositions(new Vector3[] { new Vector3(x2, y2, z2), new Vector3(-x2, y2, z2) });
    lines[2].SetPositions(new Vector3[] { new Vector3(-x2, y2, z2), new Vector3(-x2, y2, -z2) });
    lines[3].SetPositions(new Vector3[] { new Vector3(-x2, y2, -z2), new Vector3(x2, y2, -z2) });
    lines[4].SetPositions(new Vector3[] { new Vector3(x2, y2, -z2), new Vector3(x2, -y2, -z2) });
    lines[5].SetPositions(new Vector3[] { new Vector3(x2, y2, z2), new Vector3(x2, -y2, z2) });
    lines[6].SetPositions(new Vector3[] { new Vector3(-x2, y2, z2), new Vector3(-x2, -y2, z2) });
    lines[7].SetPositions(new Vector3[] { new Vector3(-x2, y2, -z2), new Vector3(-x2, -y2, -z2) });
    lines[8].SetPositions(new Vector3[] { new Vector3(x2, -y2, -z2), new Vector3(x2, -y2, z2) });
    lines[9].SetPositions(new Vector3[] { new Vector3(x2, -y2, z2), new Vector3(-x2, -y2, z2) });
    lines[10].SetPositions(new Vector3[] { new Vector3(-x2, -y2, z2), new Vector3(-x2, -y2, -z2) });
    lines[11].SetPositions(new Vector3[] { new Vector3(-x2, -y2, -z2), new Vector3(x2, -y2, -z2) });

    currentSize = Size;
  }

  private void UpdateWidth()
  {
    if (Width == currentWidth) return;

    foreach(var l in lines)
    {
      l.startWidth = Width.x;
      l.endWidth = Width.y;
    }

    currentWidth = Width;
  }

  private void UpdateLineMaterial()
  {
    if (LineMaterial == currentLineMaterial) return;

    foreach (var l in lines)
    {
      l.material = LineMaterial;
    }

    currentLineMaterial = LineMaterial;
  }
}
