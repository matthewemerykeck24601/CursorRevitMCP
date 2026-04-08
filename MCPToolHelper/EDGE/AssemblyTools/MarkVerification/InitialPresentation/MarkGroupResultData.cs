// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.MarkGroupResultData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class MarkGroupResultData
{
  public List<FamilyInstance> GroupMembers { get; set; }

  public string ControlMark { get; set; }

  public bool FailedPlateRotationComparison { get; set; }

  public bool CountMultiplierExcluded { get; set; }

  public string ConstructionProduct { get; set; }

  public string LowestControlNumber { get; internal set; }

  public MarkGroupResultData()
  {
    this.GroupMembers = new List<FamilyInstance>();
    this.FailedPlateRotationComparison = false;
    this.CountMultiplierExcluded = false;
    this.ControlMark = "";
  }

  public string GetLowestControlNumber()
  {
    List<string> list = this.GroupMembers.Select<FamilyInstance, string>((Func<FamilyInstance, string>) (s => s.GetControlNumber())).NaturalSort().ToList<string>();
    if (!list.Any<string>())
      return "";
    this.LowestControlNumber = list.First<string>();
    return list.First<string>();
  }
}
