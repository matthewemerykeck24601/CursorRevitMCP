// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.RecessDetailInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class RecessDetailInfo
{
  public double depth = -1.0;
  public CurveArray curves;
  public List<XYZ> endPoints = new List<XYZ>();

  public RecessDetailInfo(double depth, CurveArray curves)
  {
    this.depth = depth;
    this.curves = curves;
  }

  public string GetRecessText()
  {
    return this.depth == -1.0 ? (string) null : Math.Round(UnitUtils.ConvertFromInternalUnits(this.depth, UnitTypeId.Inches)).ToString() + "\" RECESS";
  }

  public CurveArray GetScaledCurveArray(Transform transform, XYZ scaledVector)
  {
    return this.TransformCurveArray(this.TransformCurveArray(this.curves, transform), scaledVector);
  }

  private CurveArray TransformCurveArray(CurveArray curvesList, Transform transform)
  {
    CurveArray curveArray = new CurveArray();
    foreach (Curve curves in curvesList)
    {
      Curve transformed = curves.CreateTransformed(transform);
      curveArray.Append(transformed);
    }
    return curveArray;
  }

  private CurveArray TransformCurveArray(CurveArray original, XYZ offset)
  {
    Transform translation = Transform.CreateTranslation(offset);
    CurveArray curveArray = new CurveArray();
    foreach (Curve curve in original)
      curveArray.Append(curve.CreateTransformed(translation));
    return curveArray;
  }
}
