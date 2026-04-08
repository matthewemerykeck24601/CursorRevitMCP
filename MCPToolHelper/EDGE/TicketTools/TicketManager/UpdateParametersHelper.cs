// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.UpdateParametersHelper
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketManager.ViewModels;
using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class UpdateParametersHelper
{
  public Element elem;
  public Dictionary<string, string> paramDict;
  public AssemblyViewModel assemblyViewModel;

  public UpdateParametersHelper(
    Element elem,
    Dictionary<string, string> paramPairs,
    AssemblyViewModel assemblyViewModel)
  {
    this.assemblyViewModel = assemblyViewModel;
    this.elem = elem;
    this.paramDict = paramPairs;
  }
}
