// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.Tools.MKComparer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using Utils.AdminUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.Tools;

internal class MKComparer : IComparer<double>
{
  private double tolerance = double.Epsilon;

  public MKComparer(double tol)
  {
    this.tolerance = Math.Abs(tol);
    if (this.tolerance >= double.Epsilon)
      return;
    this.tolerance = double.Epsilon;
  }

  public int Compare(double x, double y)
  {
    if (Util.IsEqual(x + this.tolerance, y) || Util.IsEqual(x, y + this.tolerance) || Util.IsEqual(x - this.tolerance, y) || Util.IsEqual(x, y - this.tolerance))
      return 0;
    if (x < y - this.tolerance)
      return -1;
    return x > y + this.tolerance ? 1 : 0;
  }
}
