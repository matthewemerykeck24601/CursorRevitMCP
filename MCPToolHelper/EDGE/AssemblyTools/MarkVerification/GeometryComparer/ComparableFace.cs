// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.ComparableFace
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.DrawingUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

internal class ComparableFace : IEquatable<ComparableFace>
{
  private Face _face;
  private List<CurveLoop> _CurveLoops;
  private MKTolerances _toleranceComparer;

  public double Area { get; set; }

  public XYZ FaceNormal { get; private set; }

  public bool Matched { get; set; }

  public Type FaceType { get; private set; }

  public ComparableFace(Face face, MKTolerances comparer, ComparisonOption option = ComparisonOption.DoNotRound)
  {
    this._CurveLoops = new List<CurveLoop>();
    this._face = face;
    this.Area = face.Area;
    this.FaceNormal = face.ComputeNormal(new UV(0.0, 0.0));
    foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) face.GetEdgesAsCurveLoops())
      this._CurveLoops.Add(edgesAsCurveLoop);
    this._toleranceComparer = comparer;
    this.FaceType = face.GetType();
  }

  public List<CurveLoop> GetCurveLoops() => this._CurveLoops.ToList<CurveLoop>();

  public bool Equals(ComparableFace other) => this.CompareCurveLoops(other.GetCurveLoops());

  private bool CompareCurveLoops(List<CurveLoop> otherLoops)
  {
    foreach (CurveLoop curveLoop in this._CurveLoops)
    {
      bool flag = false;
      curveLoop.GetExactLength();
      foreach (CurveLoop otherLoop in otherLoops)
      {
        if (this.CompareCurveLoop(curveLoop, otherLoop))
          flag = true;
      }
      if (!flag)
        return false;
    }
    return true;
  }

  private bool CompareCurveLoop(CurveLoop loop1, CurveLoop loop2)
  {
    if (loop1.Count<Curve>() != loop2.Count<Curve>())
      return false;
    List<Curve> curveList = new List<Curve>();
    foreach (Curve curve1 in loop1)
    {
      foreach (Curve curve2 in loop2)
      {
        if (this.CompareCurves(curve1, curve2))
        {
          curveList.Add(curve1);
          break;
        }
      }
    }
    return curveList.Count == loop1.Count<Curve>();
  }

  private bool CompareCurves(Curve curve1, Curve curve2)
  {
    bool flag = false;
    if (curve1.GetType() != curve2.GetType())
      return false;
    if (curve1 is Line)
      return this._toleranceComparer.NewPointsAreEqual(curve1.GetEndPoint(0), curve2.GetEndPoint(0), MKToleranceAspect.Geometry) && this._toleranceComparer.NewPointsAreEqual(curve1.GetEndPoint(1), curve2.GetEndPoint(1), MKToleranceAspect.Geometry) || this._toleranceComparer.NewPointsAreEqual(curve1.GetEndPoint(1), curve2.GetEndPoint(0), MKToleranceAspect.Geometry) && this._toleranceComparer.NewPointsAreEqual(curve1.GetEndPoint(0), curve2.GetEndPoint(1), MKToleranceAspect.Geometry);
    int num = 0;
    do
    {
      for (int index = 0; index < 6; ++index)
      {
        double parameter = (double) index / 6.0;
        if (parameter > 1.0)
          parameter = 1.0;
        else if (parameter < 0.0)
          parameter = 0.0;
        if (this._toleranceComparer.NewPointsAreEqual(curve1.Evaluate(flag ? 1.0 - parameter : parameter, true), curve2.Evaluate(parameter, true), MKToleranceAspect.Geometry))
          return true;
        flag = !flag;
        ++num;
      }
    }
    while (num < 2);
    return false;
  }

  public bool TransformFace(Transform transform)
  {
    this.FaceNormal = transform.OfVector(this.FaceNormal);
    List<CurveLoop> collection = new List<CurveLoop>();
    foreach (CurveLoop curveLoop in this._CurveLoops)
    {
      CurveLoop viaTransform = CurveLoop.CreateViaTransform(curveLoop, transform);
      collection.Add(viaTransform);
    }
    this._CurveLoops.Clear();
    this._CurveLoops.AddRange((IEnumerable<CurveLoop>) collection);
    return true;
  }

  public void DrawFaceVisualization(Document revitDoc)
  {
    if (this.FaceType == typeof (PlanarFace))
    {
      foreach (CurveLoop curveLoop in this._CurveLoops)
      {
        foreach (Curve curve in curveLoop)
          ModelLines.DrawModelLine(revitDoc, curve.GetEndPoint(0), curve.GetEndPoint(1));
      }
    }
    else
    {
      foreach (CurveLoop curveLoop in this._CurveLoops)
      {
        foreach (Curve curve in curveLoop)
        {
          IEnumerator<XYZ> enumerator = curve.Tessellate().GetEnumerator();
          curve.Tessellate();
          enumerator.MoveNext();
          XYZ current = enumerator.Current;
          while (enumerator.MoveNext())
          {
            ModelLines.DrawModelLine(revitDoc, current, enumerator.Current);
            current = enumerator.Current;
          }
        }
      }
    }
  }
}
