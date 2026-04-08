// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.InsulationRectangle
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.AdminTools.CAM;

internal class InsulationRectangle
{
  private const double buffer = 0.0;
  public Outline insOutline;
  public List<CurveArray> insulationCurveArray = new List<CurveArray>();
  public List<CurveArray> reductionCurveArray = new List<CurveArray>();
  public List<CurveArray> compositePinLines = new List<CurveArray>();
  public List<CurveArray> normalPinLines = new List<CurveArray>();
  public List<CurveArray> pinDiamonds = new List<CurveArray>();
  public TextDetails TextNote = new TextDetails();

  public XYZ Min => this.insOutline.MinimumPoint;

  public XYZ Max => this.insOutline.MaximumPoint;

  public XYZ Center
  {
    get => new XYZ((this.Max.X + this.Min.X) / 2.0, (this.Max.Y + this.Min.Y) / 2.0, 0.0);
  }

  public double Width => this.Max.X - this.Min.X;

  public double Length => this.Max.Y - this.Min.Y;

  public InsulationRectangle(XYZ min, XYZ max) => this.insOutline = new Outline(min, max);

  public bool Overlap(InsulationRectangle other)
  {
    return this.insOutline.Intersects(other.insOutline, 0.0);
  }

  public bool VerticalOverlap(InsulationRectangle other)
  {
    return this.Min.Y < other.Min.Y && this.Max.Y > other.Max.Y || this.Min.Y > other.Min.Y && this.Min.Y < other.Max.Y || this.Max.Y > other.Min.Y && this.Max.Y < other.Max.Y;
  }

  public bool HorizontalOverlap(InsulationRectangle other)
  {
    return this.Min.X < other.Min.X && this.Max.X > other.Max.X || this.Min.X > other.Min.X && this.Min.X < other.Max.X || this.Max.X > other.Min.X && this.Max.X < other.Max.X;
  }

  public bool VerticalContains(InsulationRectangle other)
  {
    return other.Max.Y <= this.Max.Y && other.Min.Y >= this.Min.Y;
  }

  public bool HorizContains(InsulationRectangle other)
  {
    return other.Max.X <= this.Max.X && other.Min.X >= this.Min.X;
  }

  private static void GetInfo(
    List<InsulationRectangle> rectangles,
    out double left,
    out double right,
    out double bottom,
    out double top,
    out XYZ center)
  {
    double num1 = double.MaxValue;
    double num2 = double.MinValue;
    double num3 = double.MaxValue;
    double num4 = double.MinValue;
    foreach (InsulationRectangle rectangle in rectangles)
    {
      if (rectangle.Min.X < num1)
        num1 = rectangle.Min.X;
      if (rectangle.Max.X > num2)
        num2 = rectangle.Max.X;
      if (rectangle.Min.Y < num3)
        num3 = rectangle.Min.Y;
      if (rectangle.Max.Y > num4)
        num4 = rectangle.Max.Y;
    }
    center = new XYZ((num2 + num1) / 2.0, (num4 + num3) / 2.0, 0.0);
    left = num1;
    bottom = num3;
    right = num2;
    top = num4;
  }

  public static Dictionary<InsulationRectangle, XYZ> Expand(List<InsulationRectangle> rectangles)
  {
    XYZ center;
    InsulationRectangle.GetInfo(rectangles, out double _, out double _, out double _, out double _, out center);
    Dictionary<InsulationRectangle, XYZ> dictionary = new Dictionary<InsulationRectangle, XYZ>();
    foreach (InsulationRectangle rectangle in rectangles)
    {
      XYZ xyz = rectangle.Center - center;
      dictionary.Add(rectangle, xyz * 0.2);
    }
    return dictionary;
  }

  public void Offset(XYZ scaledVector)
  {
    this.InsulationCurveArrayOffset(scaledVector);
    this.ReductionCurveArrayOffset(scaledVector);
    this.CompositePinLinesOffset(scaledVector);
    this.NormalPinLinesOffset(scaledVector);
    this.PinDiamondsOffset(scaledVector);
    this.TextNoteLocationOffset(scaledVector);
  }

  public void InsulationCurveArrayOffset(XYZ scaledVector)
  {
    this.insulationCurveArray = this.insulationCurveArray.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (i => this.TransformCurveArray(i, scaledVector))).ToList<CurveArray>();
  }

  public void ReductionCurveArrayOffset(XYZ scaledVector)
  {
    this.reductionCurveArray = this.reductionCurveArray.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (i => this.TransformCurveArray(i, scaledVector))).ToList<CurveArray>();
  }

  public void CompositePinLinesOffset(XYZ scaledVector)
  {
    this.compositePinLines = this.compositePinLines.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (i => this.TransformCurveArray(i, scaledVector))).ToList<CurveArray>();
  }

  public void NormalPinLinesOffset(XYZ scaledVector)
  {
    this.normalPinLines = this.normalPinLines.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (i => this.TransformCurveArray(i, scaledVector))).ToList<CurveArray>();
  }

  public void PinDiamondsOffset(XYZ scaledVector)
  {
    this.pinDiamonds = this.pinDiamonds.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (i => this.TransformCurveArray(i, scaledVector))).ToList<CurveArray>();
  }

  public void TextNoteLocationOffset(XYZ scaledVector)
  {
    this.TextNote.Location = Transform.CreateTranslation(scaledVector).OfPoint(this.TextNote.Location);
  }

  private CurveArray TransformCurveArray(CurveArray original, XYZ offset)
  {
    Transform translation = Transform.CreateTranslation(offset);
    CurveArray curveArray = new CurveArray();
    foreach (Curve curve in original)
      curveArray.Append(curve.CreateTransformed(translation));
    return curveArray;
  }

  internal void GenerateNote(string v)
  {
    this.TextNote = new TextDetails()
    {
      Location = this.Center,
      Text = v
    };
  }
}
