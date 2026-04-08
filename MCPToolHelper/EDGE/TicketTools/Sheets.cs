// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.Sheets
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Windows;

#nullable disable
namespace EDGE.TicketTools;

public class Sheets : DependencyObject
{
  public string SheetName { get; set; }

  public bool CheckedSheetBool { get; set; }

  public bool ExpandedBool { get; set; }

  public Sheets(string name)
  {
    this.SheetName = name;
    this.CheckedSheetBool = false;
    this.ExpandedBool = true;
  }
}
