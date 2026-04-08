// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDetailLineInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class InsulationDetailLineInfo
{
  private List<CurveArray> curves;
  private XYZ Max;
  private XYZ Min;

  public InsulationDetailLineInfo(List<CurveArray> curves)
  {
    this.curves = curves;
    this.calculateMaxAndMin(curves, out this.Max, out this.Min);
  }

  public List<CurveArray> GetCurveArrays(
    XYZ Location,
    Transform transform,
    out XYZ newMax,
    out XYZ newMin,
    out XYZ scaledVector)
  {
    List<CurveArray> source = this.TransformCurveArray(this.curves, transform);
    XYZ xyz = transform.OfPoint(new XYZ((this.Max.X + this.Min.X) / 2.0, (this.Max.Y + this.Min.Y) / 2.0, 0.0));
    scaledVector = Location - xyz;
    XYZ temp = scaledVector;
    Func<CurveArray, CurveArray> selector = (Func<CurveArray, CurveArray>) (i => this.OffsetCurveArray(i, temp));
    List<CurveArray> list = source.Select<CurveArray, CurveArray>(selector).ToList<CurveArray>();
    this.calculateMaxAndMin(list, out newMax, out newMin, transform);
    return list;
  }

  private CurveArray OffsetCurveArray(CurveArray original, XYZ offset)
  {
    Transform translation = Transform.CreateTranslation(offset);
    CurveArray curveArray = new CurveArray();
    foreach (Curve curve in original)
      curveArray.Append(curve.CreateTransformed(translation));
    return curveArray;
  }

  private List<CurveArray> TransformCurveArray(List<CurveArray> curvesList, Transform transform)
  {
    List<CurveArray> curveArrayList = new List<CurveArray>();
    foreach (CurveArray curves in curvesList)
    {
      CurveArray curveArray = new CurveArray();
      foreach (Curve curve in curves)
      {
        Curve transformed = curve.CreateTransformed(transform);
        curveArray.Append(transformed);
      }
      curveArrayList.Add(curveArray);
    }
    return curveArrayList;
  }

  private void calculateMaxAndMin(
    List<CurveArray> tempList,
    out XYZ max,
    out XYZ min,
    Transform transform = null)
  {
    if (tempList == null)
      tempList = this.curves;
    double num1 = double.MaxValue;
    double num2 = double.MaxValue;
    double num3 = double.MinValue;
    double num4 = double.MinValue;
    foreach (CurveArray temp in tempList)
    {
      foreach (Curve curve in temp)
      {
        XYZ xyz1;
        XYZ xyz2;
        if (transform == null)
        {
          xyz1 = curve.GetEndPoint(0);
          xyz2 = curve.GetEndPoint(1);
        }
        else
        {
          xyz1 = transform.Inverse.OfPoint(curve.GetEndPoint(0));
          xyz2 = transform.Inverse.OfPoint(curve.GetEndPoint(1));
        }
        num1 = Math.Min(xyz1.X, num1);
        num2 = Math.Min(xyz1.Y, num2);
        num3 = Math.Max(xyz1.X, num3);
        num4 = Math.Max(xyz1.Y, num4);
        num1 = Math.Min(xyz2.X, num1);
        num2 = Math.Min(xyz2.Y, num2);
        num3 = Math.Max(xyz2.X, num3);
        num4 = Math.Max(xyz2.Y, num4);
      }
    }
    max = new XYZ(num3, num4, 0.0);
    min = new XYZ(num1, num2, 0.0);
  }
}
