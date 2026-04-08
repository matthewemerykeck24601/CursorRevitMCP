// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.ExtensionMethods
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.RebarTools;

public static class ExtensionMethods
{
  public static bool ApproximatelyEqual(this double dbl1, double dbl2)
  {
    double num = 0.01;
    return dbl1 > dbl2 - num && dbl1 < dbl2 + num;
  }
}
