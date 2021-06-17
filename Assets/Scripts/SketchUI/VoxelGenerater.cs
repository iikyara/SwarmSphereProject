using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;

public class VoxelGenerater : MonoBehaviour
{
  public GameObject Target;
  public GameObject Parent;
  public Material VoxelMaterial;

  public GameObject VoxelObject;

  public bool Create;

  public static Bounds VoxelRange = new Bounds(Vector3.zero, 20f * Vector3.one);
  //public static Vector3Int Denseity = new Vector3Int(32, 32, 32);
  public static int Resolution = 64;

  private void Update()
  {
    if (Create && Target)
    {
      var mf = Target.GetComponent<MeshFilter>();
      if (mf && mf.sharedMesh)
      {
        VoxelObject = Utils.CreateVoxel(mf.sharedMesh, VoxelRange, Resolution, out _, out _, out _);
        Utils.SetParent(Parent, VoxelObject);
      }

      var mr = VoxelObject.GetComponent<MeshRenderer>();
      mr.material = VoxelMaterial;

      Create = false;

      /*List<Voxel_t> voxels;
      float unit;
      CPUVoxelizer.Voxelize(mf.sharedMesh, VoxelRange, Resolution, out voxels, out unit, out _);

      var parent = new GameObject("Voxel Object");
      foreach (var v in voxels)
      {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = v.position;
        go.transform.rotation = new Quaternion();
        go.transform.localScale = 0.1f * Vector3.one;
        Utils.SetParent(parent, go);
      }*/
    }
  }
}
