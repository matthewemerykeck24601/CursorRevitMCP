// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.SourceAssembly
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class SourceAssembly
{
  public string Name { get; set; }

  public List<View> Views { get; set; }

  public List<ViewSheet> Sheets { get; set; }

  public ElementId AssemID { get; set; }

  public SourceAssembly(AssemblyInstance assemInst)
  {
    this.Name = assemInst.Name;
    this.AssemID = assemInst.Id;
    this.Views = new List<View>();
    this.Sheets = new List<ViewSheet>();
  }
}
