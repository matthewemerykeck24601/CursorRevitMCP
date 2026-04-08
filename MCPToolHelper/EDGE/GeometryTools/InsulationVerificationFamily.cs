// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.InsulationVerificationFamily
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.GeometryTools;

public class InsulationVerificationFamily
{
  public List<InsulationSolid> InsulationSolids = new List<InsulationSolid>();
  private MarkVerificationData _MVData;
  private MKTolerances _toleranceComparer;
  private ComparisonOption _compareOption;
  private Document _revitDoc;
  public string UniqueId;

  public InsulationVerificationFamily(FamilyInstance familyInstance, MarkVerificationData data)
  {
    this.UniqueId = familyInstance.UniqueId;
    this._revitDoc = familyInstance.Document;
    this._compareOption = data.CompareOption;
    this._MVData = data;
    this._toleranceComparer = new MKTolerances(this._compareOption, this._revitDoc);
    this.FillInsulationSolids(familyInstance);
  }

  protected void FillInsulationSolids(FamilyInstance familyInstance)
  {
    this.InsulationSolids = new List<InsulationSolid>();
    foreach (Solid instanceSolid in Solids.GetInstanceSolids((Element) familyInstance))
      this.InsulationSolids.Add(new InsulationSolid(instanceSolid, familyInstance, this._toleranceComparer));
  }

  public bool InsulationMatches(InsulationVerificationFamily other)
  {
    if (this.InsulationSolids.Count != other.InsulationSolids.Count)
      return false;
    List<InsulationSolid> list1 = this.InsulationSolids.OrderByDescending<InsulationSolid, double>((Func<InsulationSolid, double>) (iS => iS.Volume)).ToList<InsulationSolid>();
    List<InsulationSolid> list2 = other.InsulationSolids.OrderByDescending<InsulationSolid, double>((Func<InsulationSolid, double>) (iS => iS.Volume)).ToList<InsulationSolid>();
    for (int index = 0; index < list1.Count; ++index)
    {
      if (!list1[index].Equals(list2[index]))
        return false;
    }
    return true;
  }
}
