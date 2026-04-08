// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.ParentClass
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools;

public class ParentClass : DependencyObject
{
  public List<HierarchyCheckBoxItems> Members { get; set; }

  public bool ExpandedBool { get; set; }

  public string Name { get; set; }

  public bool checkedItemBoolParent { get; set; }

  public ParentClass(List<HierarchyCheckBoxItems> members, string name = "All Assemblies")
  {
    members.Sort((Comparison<HierarchyCheckBoxItems>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.AssemblyName, q.AssemblyName)));
    this.Members = members;
    this.Name = name;
    this.checkedItemBoolParent = false;
    this.ExpandedBool = true;
  }
}
