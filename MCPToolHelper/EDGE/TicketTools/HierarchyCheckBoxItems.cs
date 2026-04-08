// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HierarchyCheckBoxItems
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools;

public class HierarchyCheckBoxItems : DependencyObject
{
  public string AssemblyName { get; set; }

  public bool ExpandedBool { get; set; }

  public List<Sheets> Members { get; set; }

  public bool CheckedItemBool { get; set; }

  public HierarchyCheckBoxItems(string name, List<string> sheetNames)
  {
    this.AssemblyName = name;
    List<Sheets> sheetsList = new List<Sheets>();
    foreach (string sheetName in sheetNames)
      sheetsList.Add(new Sheets(sheetName));
    this.Members = sheetsList;
    this.CheckedItemBool = false;
    this.ExpandedBool = true;
  }
}
