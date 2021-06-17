using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Benchmark : MonoBehaviour
{
  public string BenchmarkName = "Something";
  public GTData GroundTruth;
  public GData Generated;
  public float Score;

  public abstract void Exec();

  public void SetGTData(GTData gt, GData g)
  {
    this.GroundTruth = gt;
    this.Generated = g;
  }

  public abstract BenchmarkData CreateData();
}

public abstract class BenchmarkData
{
  public string BenchmarkName = "Something";
  public float Score;

  public BenchmarkData(string name, float score)
  {
    this.BenchmarkName = name;
    this.Score = score;
  }
  
  public abstract string GetCSVHeader();

  public abstract string ToCSV();
}