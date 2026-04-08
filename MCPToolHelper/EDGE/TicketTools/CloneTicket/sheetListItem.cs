// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.sheetListItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

internal class sheetListItem
{
  public string SheetID { get; set; }

  public string AlignmentSelection { get; set; }

  public string StackingSelection { get; set; }

  public List<string> AlignmentList { get; set; }

  public List<string> StackingList { get; set; }

  public sheetListItem(string name)
  {
    this.SheetID = name;
    this.AlignmentSelection = "Top";
    this.StackingSelection = "Stack";
    this.AlignmentList = new List<string>()
    {
      "Top",
      "Bottom"
    };
    this.StackingList = new List<string>()
    {
      "Stack",
      "Independent"
    };
  }
}
