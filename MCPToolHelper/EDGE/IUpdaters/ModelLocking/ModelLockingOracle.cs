// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingOracle
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingOracle
{
  public HashSet<string> GeometryComponents;
  public HashSet<string> ConnectionsHardwareComponents;
  public HashSet<string> RebarHandlingComponents;
  public HashSet<string> AssemblyComponents;
  private List<string> GeometryKeyWords = new List<string>()
  {
    "ledge",
    "corbel",
    "cornice",
    "solid_zone",
    "solid zone",
    "void",
    "addon",
    "pilaster",
    "ldge",
    "spot_elevation",
    "centroid"
  }.Select<string, string>((Func<string, string>) (word => word.ToUpper())).ToList<string>();

  public ModelLockingOracle() => this.InitComponentLists();

  public ModelPermissionsCategory ResolveModelPermissionsCategory(Document revitDoc, string elemId)
  {
    Element element = revitDoc.GetElement(elemId);
    Element elem = !element.HasSuperComponent() || !Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW CONSUMABLE") ? element : element.GetSuperComponent();
    if (elem == null)
      return ModelPermissionsCategory.NonTracked | ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies;
    if (elem.Category == null)
      return ModelPermissionsCategory.None;
    if (elem.Category.Id.IntegerValue == -2003101)
      return ModelPermissionsCategory.Admin;
    if (elem.Category.Id.IntegerValue == -2009658)
      return ModelPermissionsCategory.Assemblies;
    ModelPermissionsCategory permissionsCategory1 = ModelPermissionsCategory.None;
    if (elem.Category.Id.IntegerValue == -2001320 || elem.Category.Id.IntegerValue == -2001300 || elem.Category.Id.IntegerValue == -2000240 || elem.Category.Id.IntegerValue == -2000220 || elem.Category.Id.IntegerValue == -2000011)
      return permissionsCategory1 | ModelPermissionsCategory.Geometry;
    if (elem.Category.Id.IntegerValue == -2000095)
    {
      ModelPermissionsCategory permissionsCategory2 = ModelPermissionsCategory.None;
      if (elem is Group)
      {
        foreach (ElementId memberId in (IEnumerable<ElementId>) (elem as Group).GetMemberIds())
          permissionsCategory2 |= this.ResolveModelPermissionsCategory(revitDoc, revitDoc.GetElement(memberId).UniqueId);
      }
      return permissionsCategory2;
    }
    string mfgComponent = elem.GetManufactureComponent();
    FamilyInstance familyInstance = elem as FamilyInstance;
    string nameToTest = "";
    if (familyInstance != null)
      nameToTest = familyInstance.Symbol.FamilyName;
    if (this.ContainsGeometryKeyWord(nameToTest))
      permissionsCategory1 |= ModelPermissionsCategory.Geometry;
    else if (this.GeometryComponents.Where<string>((Func<string, bool>) (s => mfgComponent.ToUpper().Contains(s))).Count<string>() > 0)
      permissionsCategory1 |= ModelPermissionsCategory.Geometry;
    if (this.ConnectionsHardwareComponents.Any<string>((Func<string, bool>) (comp => mfgComponent.Contains(comp))))
      permissionsCategory1 |= ModelPermissionsCategory.ConnectionsHardware;
    if (this.RebarHandlingComponents.Contains(mfgComponent))
      permissionsCategory1 |= ModelPermissionsCategory.RebarHandling;
    else if (mfgComponent.Contains("REBAR") || mfgComponent.Contains("LIFTING") || nameToTest.ToUpper().Contains("LIFT"))
      permissionsCategory1 |= ModelPermissionsCategory.RebarHandling;
    if (elem.Category.Id.IntegerValue == -2000267)
      permissionsCategory1 |= ModelPermissionsCategory.Assemblies;
    int num = (int) (permissionsCategory1 & (ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies));
    if ((permissionsCategory1 & (ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies)) == ModelPermissionsCategory.None)
      permissionsCategory1 = ModelPermissionsCategory.NonTracked;
    return permissionsCategory1;
  }

  private bool ContainsGeometryKeyWord(string nameToTest)
  {
    foreach (string geometryKeyWord in this.GeometryKeyWords)
    {
      if (nameToTest.Contains(geometryKeyWord))
        return true;
    }
    return false;
  }

  private void InitComponentLists()
  {
    this.GeometryComponents = new HashSet<string>();
    this.GeometryComponents.Add("INSULATION");
    this.GeometryComponents.Add("PRECAST FINISH");
    this.GeometryComponents.Add("SOLID ZONE");
    this.ConnectionsHardwareComponents = new HashSet<string>();
    this.ConnectionsHardwareComponents.Add("EMBED");
    this.ConnectionsHardwareComponents.Add("CONNECTION");
    this.ConnectionsHardwareComponents.Add("ERECTION");
    this.ConnectionsHardwareComponents.Add("CIP");
    this.ConnectionsHardwareComponents.Add("RAW CONSUMABLE CUT MATERIAL");
    this.ConnectionsHardwareComponents.Add("PRODUCT BY OTHERS");
    this.ConnectionsHardwareComponents.Add("WOOD NAILER");
    this.ConnectionsHardwareComponents.Add("WOOD_NAILER");
    this.RebarHandlingComponents = new HashSet<string>();
    this.RebarHandlingComponents.Add("CGRID STANDARD");
    this.RebarHandlingComponents.Add("SHEARGRID STANDARD");
    this.RebarHandlingComponents.Add("SHEARGRID CUSTOM");
    this.RebarHandlingComponents.Add("STEM MESH STANDARD");
    this.RebarHandlingComponents.Add("STEM MESH CUSTOM");
    this.RebarHandlingComponents.Add("WWF STANDARD");
    this.RebarHandlingComponents.Add("WWF CUSTOM");
    this.RebarHandlingComponents.Add("LIFTING STANDARD");
    this.RebarHandlingComponents.Add("LIFTING CUSTOM");
    this.RebarHandlingComponents.Add("LIFTING MANUFACTURED PRODUCT");
    this.RebarHandlingComponents.Add("ERECTION REBAR");
    this.RebarHandlingComponents.Add("CIP REBAR");
    this.RebarHandlingComponents.Add("REBAR");
    this.RebarHandlingComponents.Add("REBAR STANDARD");
    this.RebarHandlingComponents.Add("REBAR CUSTOM");
    this.RebarHandlingComponents.Add("REBAR SPIRAL");
    this.RebarHandlingComponents.Add("PRESTRESSING STRAND STANDARD");
    this.RebarHandlingComponents.Add("PRESTRESSING STRAND CUSTOM");
    this.RebarHandlingComponents.Add("STRAND STANDARD");
    this.RebarHandlingComponents.Add("STRAND CUSTOM");
    this.RebarHandlingComponents.Add("SHEARGRID");
    this.RebarHandlingComponents.Add("SHEAR GRID");
    this.RebarHandlingComponents.Add("RA EMBED CUSTOM");
    this.RebarHandlingComponents.Add("RA EMBED STANDARD");
  }
}
