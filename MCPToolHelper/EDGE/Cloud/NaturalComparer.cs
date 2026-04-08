// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.NaturalComparer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace EDGE.Cloud;

internal class NaturalComparer : IComparer<string>
{
  private Regex _regex;

  public NaturalComparer() => this._regex = new Regex("\\d+$", RegexOptions.IgnoreCase);

  private string MatchEvaluator(Match m) => Convert.ToInt32(m.Value).ToString("D10");

  public int Compare(string x, string y)
  {
    x = this._regex.Replace(x.ToString(), new System.Text.RegularExpressions.MatchEvaluator(this.MatchEvaluator));
    y = this._regex.Replace(y.ToString(), new System.Text.RegularExpressions.MatchEvaluator(this.MatchEvaluator));
    return x.CompareTo(y);
  }
}
