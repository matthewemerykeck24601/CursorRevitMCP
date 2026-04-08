// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TicketWindowOutline
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class TicketWindowOutline
{
  public Outline viewPort { get; set; }

  public double Width => Math.Abs(this.viewPort.MaximumPoint.X - this.viewPort.MinimumPoint.X);

  public double Height => Math.Abs(this.viewPort.MaximumPoint.Y - this.viewPort.MinimumPoint.Y);

  public TicketWindowOutline(Outline viewportOutline) => this.viewPort = viewportOutline;

  private void showMaxMin()
  {
  }
}
