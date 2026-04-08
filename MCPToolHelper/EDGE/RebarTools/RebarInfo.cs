// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.RebarTools;

public class RebarInfo
{
  public string BarMark;
  public int iBarDiam;
  public string strBarDiameter;
  public string totalLength;
  public string barShape;
  public string strKeyHashDiam;
  public double dblBarDiameter;
  public string kerkstraGradeWeldFinishInfo;
  public Rebar Bar;

  public RebarInfo(string mark, int idiam, string strBarDiam, Rebar bar)
  {
    this.Bar = bar;
    this.BarMark = mark;
    this.iBarDiam = idiam;
    this.strBarDiameter = strBarDiam;
    this.totalLength = bar.HashedBarDims[BarSide.BarTotalLength];
    this.barShape = bar.BarShape;
    this.dblBarDiameter = bar.dblBarDiameter;
    string str = "n";
    if (bar.bFinishMark_Epoxy && !bar.bFinishMark_Galvanized)
      str = "e";
    else if (!bar.bFinishMark_Epoxy && bar.bFinishMark_Galvanized)
      str = "g";
    this.kerkstraGradeWeldFinishInfo = str;
  }

  public override string ToString()
  {
    return $"Info: Mk:{this.BarMark} iDiam:{this.iBarDiam.ToString()} strDiam:{this.strBarDiameter} L:{this.totalLength} kspecial:{this.kerkstraGradeWeldFinishInfo}";
  }
}
