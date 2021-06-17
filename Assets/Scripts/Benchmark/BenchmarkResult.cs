using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkResultManager
{
  public List<BenchmarkResult> Results;

  public BenchmarkResultManager()
  {
    Results = new List<BenchmarkResult>();
  }

  public override string ToString()
  {
    string str = "";
    foreach(var r in Results)
    {
      str += r + "\n";
    }
    return str + base.ToString();
  }

  public string GetCSVHeader()
  {
    return Results[0]?.GetCSVHeader();
  }

  public string ToCSV()
  {
    string csv = "";
    foreach(var r in Results)
    {
      csv += r.ToCSV() + "\n";
    }
    return csv;
  }
}

public class BenchmarkResult
{
  public GTData GTData;
  public BenchmarkCameraSet BenchmarkCameraSet;
  public ModelGenerater ModelGenerater;
  public MGP MGP;
  public List<BenchmarkData> BenchmarkDatas;

  public BenchmarkResult(GTData gtData, BenchmarkCameraSet bcs, MGP mgp, List<Benchmark> bms)
  {
    this.GTData = gtData;
    this.BenchmarkCameraSet = bcs;
    this.ModelGenerater = mgp.TargetModelGenerater;
    this.MGP = mgp;
    this.BenchmarkDatas = new List<BenchmarkData>();
    foreach(var bm in bms)
    {
      this.BenchmarkDatas.Add(bm.CreateData());
    }
  }

  public override string ToString()
  {
    string str =
      $"GT : {GTData}\n" +
      $"BCS : {BenchmarkCameraSet}\n" +
      $"MG : {ModelGenerater}\n" +
      $"MGP : {MGP}\n" +
      $"Time : {ModelGenerater.GeneratedTime.TotalSeconds}\n";

    foreach(var bm in this.BenchmarkDatas)
    {
      str +=
        $"BM : {bm}\n";
    }

    return str + base.ToString();
  }

  public string GetCSVHeader()
  {
    string header = 
      $"{GTData.GetCSVHeader()}, " +
      $"{BenchmarkCameraSet.GetCSVHeader()}, " +
      $"{ModelGenerater.GetCSVHeader()}, " +
      $"{MGP.GetCSVHeader()}, " +
      $"Time, ";

    foreach (var bm in this.BenchmarkDatas)
    {
      header += bm.GetCSVHeader() + ", ";
    }

    return header; 
  }

  public string ToCSV()
  {
    string csv =
      $"{GTData.ToCSV()}, " +
      $"{BenchmarkCameraSet.ToCSV()}, " +
      $"{ModelGenerater.ToCSV()}, " +
      $"{MGP.ToCSV()}, " +
      $"{ModelGenerater.GeneratedTime.TotalSeconds}, ";

    foreach (var bm in this.BenchmarkDatas)
    {
      csv += bm.ToCSV() + ", ";
    }

    return csv;
  }
}
