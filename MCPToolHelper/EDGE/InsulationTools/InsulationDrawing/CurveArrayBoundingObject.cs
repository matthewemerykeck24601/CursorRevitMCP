// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.CurveArrayBoundingObject
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class CurveArrayBoundingObject
{
  private double maxX = double.MinValue;
  private double maxY = double.MinValue;
  private double minX = double.MaxValue;
  private double minY = double.MaxValue;

  public CurveArrayBoundingObject(CurveArray curveArray)
  {
    foreach (Curve curve in curveArray)
    {
      if (curve is Line)
      {
        this.updateBounds(curve.GetEndPoint(0));
        this.updateBounds(curve.GetEndPoint(1));
      }
      else if (curve is Arc arc && arc.IsBound)
      {
        this.updateBounds(arc.GetEndPoint(0));
        this.updateBounds(arc.GetEndPoint(1));
        this.updateBounds(arc.Evaluate(0.5, true));
      }
    }
  }

  private void updateBounds(XYZ point)
  {
    if (point.X > this.maxX)
      this.maxX = point.X;
    if (point.X < this.minX)
      this.minX = point.X;
    if (point.Y > this.maxY)
      this.maxY = point.Y;
    if (point.Y >= this.minY)
      return;
    this.minY = point.Y;
  }

  public bool IsWithin(CurveArrayBoundingObject object2)
  {
    return this.maxX <= object2.maxX && this.maxY <= object2.maxY && this.minX >= object2.minX && this.minY >= object2.minY;
  }
}
