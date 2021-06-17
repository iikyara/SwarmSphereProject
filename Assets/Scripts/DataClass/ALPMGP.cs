using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AgentLocatedProblemのパラメータ
/// </summary>
public class ALPMGP : MGP
{
  public ModelGenerateParam MGP;

  public override object GetParam()
  {
    return MGP;
  }

  public override string ToString()
  {
    var fields = typeof(ModelGenerateParam).GetFields();
    string str = "";
    for (int i = 1; i < fields.Length; i++)
    {
      str += $"{fields[i].Name} : {fields[i].GetValue(this.MGP)}\n";
    }
    return str + base.ToString();
  }

  public override string GetCSVHeader()
  {
    var fields = typeof(ModelGenerateParam).GetFields();
    string header = fields[0].Name;
    for(int i = 1; i < fields.Length; i++)
    {
      header += $", {fields[i].Name}";
    }

    return header;
  }

  public override string ToCSV()
  {
    var fields = typeof(ModelGenerateParam).GetFields();
    string csv = fields[0].GetValue(this.MGP).ToString();
    for (int i = 1; i < fields.Length; i++)
    {
      csv += $", {fields[i].GetValue(this.MGP)}";
    }

    return csv;
  }
}
