// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.AssemblyListObject
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class AssemblyListObject
{
  public ElementId assemblyId;

  public string Name { get; set; }

  public bool IsChecked { get; set; }

  public AssemblyListObject(string assemblyName, ElementId id)
  {
    this.Name = assemblyName;
    this.assemblyId = id;
    this.IsChecked = false;
  }
}
