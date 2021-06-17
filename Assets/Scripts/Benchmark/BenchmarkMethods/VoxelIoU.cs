using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;

public class VoxelIoU : Benchmark
{
  bool[,,] GroundTruthVoxel;
  bool[,,] GeneratedVoxel;

  public Bounds VoxelRange = new Bounds(Vector3.zero, 20f * Vector3.one);
  public int Resolution = 32;

  public bool CreateView;

  public override void Exec()
  {
    //生成と真値の両方をボクセル化
    this.GroundTruthVoxel = convertMeshToVoxel(this.GroundTruth.Mesh);
    this.GeneratedVoxel = convertMeshToVoxel(this.Generated.Mesh);

    this.Score = calcIoU(this.GroundTruthVoxel, this.GeneratedVoxel);
  }

  private bool[,,] convertMeshToVoxel(Mesh mesh)
  {
    if (CreateView)
    {
      List<Voxel_t> voxels;
      float unit;
      CPUVoxelizer.Voxelize(mesh, VoxelRange, Resolution, out voxels, out unit, out _);

      var parent = new GameObject("Voxel Object");
      foreach (var v in voxels)
      {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = v.position;
        go.transform.rotation = new Quaternion();
        go.transform.localScale = unit / Resolution / 0.5f * Vector3.one;
        Utils.SetParent(parent, go);
      }
    }

    return Utils.CalcVoxel(mesh, this.VoxelRange, this.Resolution);
  }

  private float calcIoU(bool[,,] gt, bool[,,] g)
  {
    int inter = 0;
    int union = 0;

    for(int x = 0; x < gt.GetLength(0); x++)
    {
      for(int y = 0; y < gt.GetLength(1); y++)
      {
        for(int z = 0; z < gt.GetLength(2); z++)
        {
          inter += (gt[x, y, z] & g[x, y, z]) ? 1 : 0;
          union += (gt[x, y, z] | g[x, y, z]) ? 1 : 0;
        }
      }
    }

    return (float)inter / union;
  }

  public override BenchmarkData CreateData()
  {
    return new VoxelIoUData(this.BenchmarkName, this.VoxelRange, this.Resolution, this.Score);
  }
}

public class VoxelIoUData : BenchmarkData
{
  public Bounds VoxelRange;
  public int Resolution;

  public VoxelIoUData(string name, Bounds voxelRange, int resolution, float score) : base(name, score)
  {
    this.VoxelRange = voxelRange;
    this.Resolution = resolution;
  }

  public override string ToString()
  {
    string str =
      $"VoxelIoU.Range : {VoxelRange}\n" +
      $"VoxelIoU.Resolution : {Resolution}\n" +
      $"VoxelIoU.Score : {Score}\n";
    return str + base.ToString();
  }

  public override string GetCSVHeader()
  {
    return
      $"VoxelIoU.Range, " +
      $"VoxelIoU.Resolution, " +
      $"VoxelIoU.Score";
  }

  public override string ToCSV()
  {
    return
      $"{VoxelRange.extents.x}, " +
      $"{Resolution}, " +
      $"{Score}";
  }
}