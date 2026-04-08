// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.RowInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class RowInfo
{
  private int rowNum = 1;
  public List<InsulationDetail> details = new List<InsulationDetail>();
  private double horizontalPadding;
  private XYZ maxBounds;
  private XYZ minBounds;
  public double currentX;
  private bool alignToFirst;

  public RowInfo(
    int rowNum,
    double horizontalPadding,
    XYZ MaxBounds,
    XYZ MinBounds,
    bool alignToFirst = false)
  {
    this.rowNum = rowNum;
    this.horizontalPadding = horizontalPadding;
    this.maxBounds = MaxBounds;
    this.minBounds = MinBounds;
    this.currentX = MinBounds.X;
    this.alignToFirst = alignToFirst;
  }

  public bool AddInsulationDrawing(InsulationDetail detail, bool PinPointPlacenment = false)
  {
    if (PinPointPlacenment)
    {
      double num = this.currentX + detail.width;
      detail.masterXPosition = this.currentX;
      if (this.details.Count != 0)
        num += this.horizontalPadding;
      this.currentX = num;
      this.details.Add(detail);
      return true;
    }
    double num1 = this.currentX + detail.width;
    if (this.details.Count != 0)
      num1 += this.horizontalPadding;
    if (num1 > this.maxBounds.X && this.details.Count != 0)
      return false;
    double currentX = this.currentX;
    if (this.details.Count != 0)
      currentX += this.horizontalPadding;
    detail.masterXPosition = currentX;
    XYZ newLocation = new XYZ(currentX, this.maxBounds.Y - detail.height, 0.0);
    detail.MoveDrawing(newLocation);
    this.details.Add(detail);
    this.currentX = num1;
    return true;
  }

  public double AlignToY()
  {
    double num = 0.0;
    if (this.alignToFirst)
    {
      InsulationDetail insulationDetail = this.details.FirstOrDefault<InsulationDetail>();
      if (insulationDetail != null)
        num = insulationDetail.height;
    }
    else
    {
      foreach (InsulationDetail detail in this.details)
      {
        if (detail.height > num)
          num = detail.height;
      }
    }
    double y = this.maxBounds.Y - num;
    foreach (InsulationDetail detail in this.details)
      detail.MoveDrawing(new XYZ(detail.masterXPosition, y, 0.0));
    return y;
  }
}
