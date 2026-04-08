// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingLegendInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class InsulationDrawingLegendInfo
{
  private View legend;
  private double height;
  private double width;
  private double heightNoScale;
  private double widthNoScale;
  private double verticalPadding;
  private double horizontalPadding;
  private XYZ TopLeft = XYZ.Zero;
  private List<RowInfo> rows = new List<RowInfo>();
  private RowInfo currentRow;
  private double currentY;
  private double maxX;
  private int InsulationCount = 1;
  public bool CreateNewLegend;
  public bool DrawingToLarge;
  public string minMark = string.Empty;
  public string maxMark = string.Empty;
  public bool existingViewPort;

  private BoundingBoxUV outline => this.legend == null ? (BoundingBoxUV) null : this.legend.Outline;

  public double trueHeight => this.outline == null ? 0.0 : this.outline.Max.V - this.outline.Min.V;

  public double trueWidth => this.outline == null ? 0.0 : this.outline.Max.U - this.outline.Min.U;

  public InsulationDrawingLegendInfo(
    View legend,
    double height,
    double width,
    XYZ TopLeft,
    double horizontalPadding = 0.2,
    double verticalPadding = 0.1)
  {
    this.legend = legend;
    this.heightNoScale = height;
    this.widthNoScale = width;
    this.height = height * (double) legend.Scale;
    this.width = width * (double) legend.Scale;
    this.verticalPadding = verticalPadding * (double) legend.Scale;
    this.horizontalPadding = horizontalPadding * (double) legend.Scale;
    this.TopLeft = TopLeft;
    this.currentY = TopLeft.Y;
  }

  public bool AddInsulationDrawing(InsulationDetail detail)
  {
    XYZ xyz = new XYZ((double) this.InsulationCount, (double) this.InsulationCount, 0.0);
    if (!detail.DrawDetail(this.legend, xyz, false, false))
    {
      detail.DeleteDrawing();
      if (!detail.DrawDetail(this.legend, xyz, false, false))
        return false;
    }
    if (this.rows.Count == 0 && (detail.height > this.height || detail.width > this.width))
    {
      detail.DeleteDrawing();
      this.DrawingToLarge = true;
      return false;
    }
    if (this.currentRow == null)
      this.CreateNewRow();
    if (this.rows.Count > 0 && this.currentY - this.verticalPadding - detail.height < this.TopLeft.Y - this.height)
    {
      detail.DeleteDrawing();
      this.CreateNewLegend = true;
      return false;
    }
    if (!this.currentRow.AddInsulationDrawing(detail))
    {
      if (this.currentY - this.verticalPadding - detail.height < this.TopLeft.Y - this.height)
      {
        detail.DeleteDrawing();
        this.CreateNewLegend = true;
        this.currentY = this.currentRow.AlignToY();
        return false;
      }
      if (this.currentRow.currentX > this.maxX)
        this.maxX = this.currentRow.currentX;
      this.currentY = this.currentRow.AlignToY();
      this.CreateNewRow();
      if (!this.currentRow.AddInsulationDrawing(detail))
      {
        detail.DeleteDrawing();
        this.CreateNewLegend = true;
        return false;
      }
      if (this.trueHeight > this.heightNoScale)
      {
        detail.DeleteDrawing();
        this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
        this.CreateNewLegend = true;
        this.currentY = this.currentRow.AlignToY();
        return false;
      }
      if (this.trueWidth > this.widthNoScale)
      {
        this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
        detail.MoveDrawing(xyz);
        this.currentY = this.currentRow.AlignToY();
        this.CreateNewRow();
        if (!this.currentRow.AddInsulationDrawing(detail))
        {
          detail.DeleteDrawing();
          this.CreateNewLegend = true;
          return false;
        }
        if (this.trueHeight > this.heightNoScale)
        {
          this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
          detail.DeleteDrawing();
          this.CreateNewLegend = true;
          this.currentY = this.currentRow.AlignToY();
          return false;
        }
      }
      if (this.currentRow.currentX > this.maxX)
        this.maxX = this.currentRow.currentX;
    }
    else
    {
      if (this.trueHeight > this.heightNoScale)
      {
        this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
        detail.DeleteDrawing();
        this.CreateNewLegend = true;
        this.currentY = this.currentRow.AlignToY();
        return false;
      }
      if (this.trueWidth > this.widthNoScale)
      {
        this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
        detail.MoveDrawing(xyz);
        this.currentY = this.currentRow.AlignToY();
        this.CreateNewRow();
        if (!this.currentRow.AddInsulationDrawing(detail))
        {
          detail.DeleteDrawing();
          this.CreateNewLegend = true;
          return false;
        }
        if (this.trueHeight > this.heightNoScale)
        {
          this.currentRow.details.RemoveAt(this.currentRow.details.Count - 1);
          detail.DeleteDrawing();
          this.CreateNewLegend = true;
          this.currentY = this.currentRow.AlignToY();
          return false;
        }
      }
      if (this.currentRow.currentX > this.maxX)
        this.maxX = this.currentRow.currentX;
    }
    if (string.IsNullOrWhiteSpace(this.minMark))
      this.minMark = detail.InsulationMark;
    this.maxMark = detail.InsulationMark;
    this.InsulationCount += 5;
    return true;
  }

  public bool AddInsulationDrawing(InsulationDetail detail, XYZ PlacementLocation)
  {
    if (!detail.DrawDetail(this.legend, PlacementLocation, false))
    {
      detail.DeleteDrawing();
      if (!detail.DrawDetail(this.legend, PlacementLocation, false))
        return false;
    }
    if (this.currentRow == null)
    {
      this.currentRow = new RowInfo(this.rows.Count + 1, this.horizontalPadding, new XYZ(this.TopLeft.X + this.width, PlacementLocation.Y + detail.height, 0.0), new XYZ(PlacementLocation.X, this.TopLeft.Y - this.height, 0.0), true);
      this.rows.Add(this.currentRow);
    }
    this.currentRow.AddInsulationDrawing(detail, true);
    if (string.IsNullOrWhiteSpace(this.minMark))
      this.minMark = detail.InsulationMark;
    this.maxMark = detail.InsulationMark;
    this.currentY = PlacementLocation.Y;
    return true;
  }

  public void FinalizePosition() => this.currentY = this.currentRow.AlignToY();

  public bool ContainsDrawing()
  {
    foreach (RowInfo row in this.rows)
    {
      if (row.details.Count > 0)
        return true;
    }
    return this.existingViewPort;
  }

  private void CreateNewRow()
  {
    this.currentRow = new RowInfo(this.rows.Count + 1, this.horizontalPadding, new XYZ(this.TopLeft.X + this.width, this.currentY - this.verticalPadding, 0.0), new XYZ(this.TopLeft.X, this.TopLeft.Y - this.height, 0.0));
    this.rows.Add(this.currentRow);
  }
}
