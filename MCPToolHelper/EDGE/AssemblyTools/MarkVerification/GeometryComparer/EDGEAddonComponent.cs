// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.EDGEAddonComponent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class EDGEAddonComponent : EDGEAssemblyComponent_Base
{
  public string HostGuid { get; private set; }

  public ElementId MaterialId { get; private set; }

  public double MaterialVolume_InInternalUnits { get; private set; }

  public bool IsValidForMarkVerification { get; }

  public EDGEAddonComponent(
    FamilyInstance addonComponentInstance,
    FamilyInstance hostInstance,
    ComparisonOption compareOption)
    : base(addonComponentInstance, hostInstance, compareOption)
  {
    this.IsValidForMarkVerification = true;
    this.HostGuid = Parameters.GetParameterAsString((Element) addonComponentInstance, "HOST_GUID");
    ICollection<ElementId> materialIds = this.inst.GetMaterialIds(false);
    if (materialIds.Count > 1)
    {
      this.StatusMessage += "Detected multiple materials in addon product.";
      QAUtils.DebugMessage($"AddonComponentData Constructor: Found Addon: {this.inst.Id?.ToString()}-{this.inst.Name} lineage: {(this.inst.Symbol != null ? $"{this.inst.Symbol.FamilyName}:{this.inst.Symbol.Name}" : " -Empty_Symbol- ")} Detected multiple materials in addon product");
    }
    else if (materialIds.Count == 1)
    {
      this.MaterialId = materialIds.First<ElementId>();
    }
    else
    {
      this.StatusMessage += "No materials assigned to this addon.";
      QAUtils.DebugMessage($"AddonComponentData Constructor: Found Addon: {this.inst.Id?.ToString()}-{this.inst.Name} lineage: {(this.inst.Symbol != null ? $"{this.inst.Symbol.FamilyName}:{this.inst.Symbol.Name}" : " -Empty_Symbol- ")} With no materials returned by GetMaterialIds()");
    }
    if (this.MaterialId != (ElementId) null && this.MaterialId != ElementId.InvalidElementId)
      this.MaterialVolume_InInternalUnits = this.inst.GetMaterialVolume(this.MaterialId);
    if (Math.Abs(this.MaterialVolume_InInternalUnits) >= 1E-07)
      return;
    this.MaterialVolume_InInternalUnits = addonComponentInstance.GetElementVolume();
    if (this.MaterialVolume_InInternalUnits >= 1E-07)
      return;
    this.IsValidForMarkVerification = false;
    QA.LogError("AddonComponentData Ctor", "unable to find addon material volume");
  }

  public override LocationCompareResult Matches(EDGEAssemblyComponent_Base other)
  {
    return !(other is EDGEAddonComponent edgeAddonComponent) || this.MaterialId != edgeAddonComponent.MaterialId || !EDGEAssemblyComponent_Base._toleranceComparer.NewValuesAreEqual(this.MaterialVolume_InInternalUnits, edgeAddonComponent.MaterialVolume_InInternalUnits, MKToleranceAspect.AddonVolume) ? LocationCompareResult.Failed : this.Matches(other, MKToleranceAspect.Geometry);
  }
}
