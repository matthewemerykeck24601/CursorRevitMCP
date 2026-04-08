// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingElementData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingElementData
{
  public bool Exclusive { get; set; }

  public ElementId Id { get; set; }

  public ModelPermissionsCategory PermissionsCategory { get; set; }

  public ElementId AssemblyId { get; set; }

  public Transform transform { get; set; }

  public ElementId PlanViewID { get; set; }

  public Dictionary<string, string> ParameterDict { get; set; }

  public int AssemblyMemberCount { get; set; }

  public string SuperComponentID { get; set; }

  public ModelLockingElementData()
  {
    this.Id = ElementId.InvalidElementId;
    this.PermissionsCategory = ModelPermissionsCategory.None;
  }

  public ModelLockingElementData(Document revitDoc, ElementId id, ModelPermissionsCategory cat)
  {
    this.Id = id;
    Element element = revitDoc.GetElement(id);
    this.AssemblyId = element.AssemblyInstanceId;
    this.PermissionsCategory = cat;
    if (element is Group)
      this.Exclusive = true;
    if (element is Level)
      this.PlanViewID = (element as Level).FindAssociatedPlanViewId();
    if (element is FamilyInstance)
    {
      this.transform = (element as FamilyInstance).GetTransform();
      this.SuperComponentID = (element as FamilyInstance).SuperComponent == null ? "" : (element as FamilyInstance).SuperComponent.UniqueId;
    }
    if (!(element is AssemblyInstance))
      return;
    this.transform = (element as AssemblyInstance).GetTransform();
    this.AssemblyMemberCount = (element as AssemblyInstance).GetMemberIds().Count;
    this.ParameterDict = ModelLockingElementData.ConvertParamsToDictionary(element);
  }

  public static Dictionary<string, string> ConvertParamsToDictionary(Element elem)
  {
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (Parameter orderedParameter in (IEnumerable<Parameter>) elem.GetOrderedParameters())
      dictionary.Add(orderedParameter.Definition.Name, Parameters.GetParameterAsSmartString(elem, orderedParameter.Definition.Name));
    return dictionary;
  }

  public override string ToString()
  {
    return $"{this.Id.ToString():14}, {this.PermissionsCategory.ToString():30}";
  }
}
