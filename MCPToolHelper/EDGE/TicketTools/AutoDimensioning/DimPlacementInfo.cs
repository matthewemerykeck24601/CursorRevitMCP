// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimPlacementInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimPlacementInfo
{
  public Dictionary<ExtremePointType, XYZ> ExtremePoint { get; set; }

  public Line PlacementLine { get; set; }

  public XYZ TextPlacementPoint { get; set; }

  public double TextAngle { get; set; }

  public DimensionEdge DimensionFromEdge { get; set; }

  public XYZ DimAlongDirection { get; set; }
}
