/*
 * https://nn-hokuson.hatenablog.com/entry/2018/02/13/200114 から
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad : MonoBehaviour
{
  public Material Material;
  public float Size;

  void Start()
  {
    Mesh mesh = new Mesh();
    mesh.vertices = new Vector3[] {
        new Vector3 (-Size, -Size, 0),
        new Vector3 (-Size,  Size, 0),
        new Vector3 ( Size, -Size, 0),
        new Vector3 ( Size,  Size, 0),
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
    GetComponent<MeshFilter>().sharedMesh = mesh;
    GetComponent<MeshRenderer>().material = Material;
  }
}
