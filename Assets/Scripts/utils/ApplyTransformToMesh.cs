using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyTransformToMesh : MonoBehaviour
{
  public Vector3 Position;
  public Vector3 Rotation;
  public Vector3 Scale;
  public float Size;

  public bool IsApplyTransform = false;

  private void Awake()
  {
    if(IsApplyTransform) ApplyTransform();
    else ScaleToSize();
  }

  public void ApplyTransform()
  {
    var mesh = GetComponent<MeshFilter>().sharedMesh;

    var newMesh = new Mesh();
    Debug.Log($"{mesh.bounds}");
    newMesh.vertices = verticesTransform(mesh.vertices, Position, Rotation, Scale);
    newMesh.triangles = mesh.triangles;
    newMesh.RecalculateNormals();
    newMesh.RecalculateBounds();
    GetComponent<MeshFilter>().sharedMesh = newMesh;
    Debug.Log($"{newMesh.bounds}");
  }

  /// <summary>
  /// サイズの大きさになるようにメッシュをスケール
  /// </summary>
  public void ScaleToSize()
  {
    var mesh = GetComponent<MeshFilter>().sharedMesh;
    mesh.RecalculateBounds();
    float maxEdge = Mathf.Max(new float[] { mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z });
    float mag = Size / maxEdge;

    var newMesh = new Mesh();
    Debug.Log($"{mesh.bounds}");
    newMesh.vertices = verticesTransform(mesh.vertices, -mesh.bounds.center, Vector3.zero, mag * Vector3.one);
    newMesh.triangles = mesh.triangles;
    newMesh.RecalculateNormals();
    newMesh.RecalculateBounds();
    GetComponent<MeshFilter>().sharedMesh = newMesh;
    Debug.Log($"{newMesh.bounds}");
  }

  private Vector3[] verticesTransform(Vector3[] vertices, Vector3 pos, Vector3 rot, Vector3 scale)
  {
    Vector3[] result = new Vector3[vertices.Length];
    for(int i = 0; i < vertices.Length; i++)
    {
      result[i] = vertices[i];
      result[i] += pos;
      result[i] = Matrix4x4.Scale(scale) * Matrix4x4.Rotate(Quaternion.Euler(rot)) * result[i];
    }
    return result;
  }
}
