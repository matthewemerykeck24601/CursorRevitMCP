// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimSegmentDummy
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimSegmentDummy : AutoTicketDimSegment
{
  public double text_start;
  public double text_end;

  public DimSegmentDummy(double start, double end)
  {
    this.text_start = start;
    this.text_end = end;
  }
}
