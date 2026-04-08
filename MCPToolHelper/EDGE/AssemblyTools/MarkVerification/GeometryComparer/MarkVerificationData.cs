// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.MarkVerificationData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.HostingUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class MarkVerificationData
{
  public List<ElementId> repeatedComponents;
  public List<ElementId> repeatedComponentsForWarpable;
  public List<ElementId> projectFinishFamilies;
  public List<FamilyInstance> ProjectAddons;
  public List<Element> ProjectFinishes;
  private List<EDGEAddonComponent> _ProjectEdgeAddonComponents;
  private List<EDGEFinishComponent> _ProjectEdgeFinishComponents;
  public Dictionary<string, List<ElementId>> PlateIdsPerStructuralFramingElement;
  public Dictionary<string, List<ElementId>> FinishFamiliesPerStructuralFramingElement;
  public bool cancel;

  public ComparisonOption CompareOption { get; private set; }

  public bool IsValidForMarkVerification { get; private set; }

  public MarkVerificationData(Document revitDoc, ComparisonOption compareOption, bool bInsulation)
  {
    this.CompareOption = compareOption;
  }

  public MarkVerificationData(Document revitDoc, ComparisonOption compareOption)
  {
    DateTime now = DateTime.Now;
    this.CompareOption = compareOption;
    this.ProjectAddons = Components.GetAllProjectAddons(revitDoc).ToList<FamilyInstance>();
    this.ProjectFinishes = new FilteredElementCollector(revitDoc).OfClass(typeof (Wall)).ToList<Element>();
    this._ProjectEdgeAddonComponents = new List<EDGEAddonComponent>();
    this._ProjectEdgeFinishComponents = new List<EDGEFinishComponent>();
    ElementFilter addonFilter = Components.GetAddonFilter(revitDoc);
    if (addonFilter == null)
    {
      this.cancel = true;
    }
    else
    {
      ElementFilter addonInverted = Components.GetAddonInverted(revitDoc);
      if (addonInverted == null)
      {
        this.cancel = true;
      }
      else
      {
        ElementFilter repeatedComponentFilter = Oracle.GetIsRepeatedComponentFilter(revitDoc);
        if (repeatedComponentFilter == null)
        {
          this.cancel = true;
        }
        else
        {
          ElementFilter warpableMvFilter = Oracle.GetIsRepeatedForWarpableMVFilter(revitDoc, repeatedComponentFilter);
          if (warpableMvFilter == null)
          {
            this.cancel = true;
          }
          else
          {
            ElementFilter connectorComponentFilter = Components.GetConnectorComponentFilter(revitDoc);
            if (connectorComponentFilter == null)
            {
              this.cancel = true;
            }
            else
            {
              FilteredElementCollector elementsCollector = Components.GetAllEDGEAddonOrEmbedElementsCollector(revitDoc);
              this.projectFinishFamilies = Components.GetAllProjectFinishFamilies(revitDoc, addonFilter).ToList<ElementId>();
              ICollection<ElementId> elementIds = elementsCollector.ToElementIds();
              FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc, elementIds);
              if (warpableMvFilter != null)
                elementCollector.WherePasses(warpableMvFilter);
              if (addonInverted != null)
                elementCollector.WherePasses(addonInverted);
              this.repeatedComponentsForWarpable = elementCollector.ToElementIds().ToList<ElementId>();
              List<ElementId> elementIdList = new List<ElementId>();
              elementIdList.AddRange((IEnumerable<ElementId>) this.repeatedComponentsForWarpable);
              foreach (ElementId id in elementIdList)
              {
                Element element = revitDoc.GetElement(id);
                if (connectorComponentFilter != null && connectorComponentFilter.PassesFilter(element))
                {
                  FamilyInstance familyInstance = element as FamilyInstance;
                  if (familyInstance.SuperComponent != null)
                  {
                    Element superComponent = familyInstance.SuperComponent;
                    if ((warpableMvFilter == null || warpableMvFilter.PassesFilter(superComponent)) && (addonFilter == null || !addonFilter.PassesFilter(superComponent)))
                    {
                      if (!this.repeatedComponentsForWarpable.Contains(superComponent.Id))
                        this.repeatedComponentsForWarpable.Add(superComponent.Id);
                    }
                    else
                      this.repeatedComponentsForWarpable.Remove(id);
                  }
                }
              }
              this.repeatedComponents = new FilteredElementCollector(revitDoc, elementIds).WherePasses(repeatedComponentFilter).WherePasses(addonInverted).ToElementIds().ToList<ElementId>();
              foreach (ElementId id in new List<ElementId>((IEnumerable<ElementId>) this.repeatedComponents))
              {
                Element element = revitDoc.GetElement(id);
                if (connectorComponentFilter.PassesFilter(element))
                {
                  FamilyInstance familyInstance = element as FamilyInstance;
                  if (familyInstance.SuperComponent != null)
                  {
                    Element superComponent = familyInstance.SuperComponent;
                    if ((repeatedComponentFilter == null || repeatedComponentFilter.PassesFilter(superComponent)) && (addonFilter == null || !addonFilter.PassesFilter(superComponent)))
                    {
                      if (!this.repeatedComponents.Contains(superComponent.Id))
                        this.repeatedComponents.Add(superComponent.Id);
                    }
                    else
                      this.repeatedComponents.Remove(id);
                  }
                }
              }
              bool flag = Components.CheckAddonHosting(revitDoc);
              foreach (FamilyInstance projectAddon in this.ProjectAddons)
              {
                if (revitDoc.GetElement(projectAddon.GetHostGuid()) is FamilyInstance element)
                  this._ProjectEdgeAddonComponents.Add(new EDGEAddonComponent(projectAddon, element, this.CompareOption));
              }
              foreach (Element projectFinish in this.ProjectFinishes)
              {
                Parameter parameter = projectFinish.LookupParameter("HOST_GUID");
                if (parameter != null && parameter.HasValue)
                {
                  if (!(revitDoc.GetElement(parameter.AsString()) is FamilyInstance element))
                    flag = true;
                  else
                    this._ProjectEdgeFinishComponents.Add(new EDGEFinishComponent(projectFinish as Wall, element, ComparisonOption.DoNotRound));
                }
              }
              TimeSpan timeSpan = DateTime.Now - now;
              TaskDialogResult taskDialogResult1 = (TaskDialogResult) 7;
              if (this._ProjectEdgeAddonComponents.Where<EDGEAddonComponent>((Func<EDGEAddonComponent, bool>) (s => !s.IsValidForMarkVerification)).Any<EDGEAddonComponent>() || this._ProjectEdgeFinishComponents.Where<EDGEFinishComponent>((Func<EDGEFinishComponent, bool>) (t => !t.IsValidForMarkVerification)).Any<EDGEFinishComponent>())
                taskDialogResult1 = new TaskDialog("Warning")
                {
                  AllowCancellation = false,
                  CommonButtons = ((TaskDialogCommonButtons) 6),
                  FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                  MainInstruction = "Addon components found with no material defined.  Continue?",
                  MainContent = "Mark Verification requires the user to have successfully run addon hosting updater on any elements which would be processed by the Mark Verification Tool.   Mark Verification has detected addon components with no volume and therefore cannot be accurately compared."
                }.Show();
              TaskDialogResult taskDialogResult2 = (TaskDialogResult) 7;
              if (flag)
                taskDialogResult2 = new TaskDialog("Warning")
                {
                  AllowCancellation = false,
                  CommonButtons = ((TaskDialogCommonButtons) 6),
                  FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                  MainInstruction = "Addon Hosting Issue.  Continue?",
                  MainContent = "Mark Verification requires the user to have run addon hosting updater on any elements which would be processed by the Mark Verification Tool.  Please ensure that Addon Hosting Updater has been run on the elements for which you wish to run Mark Verification Tool.  Mark Verification may not produce accurate results if Addon Hosting Updater has not been run successfully on the entire model (or at least the pieces considered."
                }.Show();
              this.IsValidForMarkVerification = !flag || taskDialogResult1 == 6 || taskDialogResult2 == 6;
              this.PlateIdsPerStructuralFramingElement = new Dictionary<string, List<ElementId>>();
              this.FinishFamiliesPerStructuralFramingElement = new Dictionary<string, List<ElementId>>();
            }
          }
        }
      }
    }
  }

  public List<EDGEFinishComponent> GetFinishesForHost(FamilyInstance hostInstance)
  {
    return this._ProjectEdgeFinishComponents.Where<EDGEFinishComponent>((Func<EDGEFinishComponent, bool>) (s => s.HostGuid == hostInstance.UniqueId)).ToList<EDGEFinishComponent>();
  }

  public List<EDGEAddonComponent> GetAddonsForHost(FamilyInstance hostInst)
  {
    Document document = hostInst.Document;
    List<EDGEAddonComponent> list1 = this._ProjectEdgeAddonComponents.Where<EDGEAddonComponent>((Func<EDGEAddonComponent, bool>) (s => s.HostGuid == hostInst.UniqueId)).ToList<EDGEAddonComponent>();
    List<ElementId> list2;
    if (this.FinishFamiliesPerStructuralFramingElement.ContainsKey(hostInst.UniqueId))
    {
      list2 = this.FinishFamiliesPerStructuralFramingElement[hostInst.UniqueId].Distinct<ElementId>().ToList<ElementId>();
    }
    else
    {
      List<ElementId> elementIdList = new List<ElementId>();
      list2 = BomHostingV2.GetIntersectingElements(document, (Element) hostInst, (ICollection<ElementId>) new List<ElementId>()).Where<ElementId>((Func<ElementId, bool>) (s => this.projectFinishFamilies.Contains(s))).Distinct<ElementId>().ToList<ElementId>();
      this.FinishFamiliesPerStructuralFramingElement.Add(hostInst.UniqueId, list2.ToList<ElementId>());
    }
    if (list2.Count == 0)
      return list1;
    foreach (Element addonComponentInstance in new FilteredElementCollector(document, (ICollection<ElementId>) list2).OfClass(typeof (FamilyInstance)))
    {
      EDGEAddonComponent edgeAddonComponent = new EDGEAddonComponent(addonComponentInstance as FamilyInstance, hostInst, this.CompareOption);
      list1.Add(edgeAddonComponent);
    }
    return list1;
  }

  public List<ElementId> GetPlateIdsForFamilyInstance(FamilyInstance hostInst)
  {
    List<ElementId> list;
    if (this.PlateIdsPerStructuralFramingElement.ContainsKey(hostInst.UniqueId))
    {
      list = this.PlateIdsPerStructuralFramingElement[hostInst.UniqueId].Distinct<ElementId>().ToList<ElementId>();
    }
    else
    {
      List<ElementId> elementIdList = new List<ElementId>();
      List<ElementId> source = !Oracle.IsWarpableConstructionProduct(hostInst) ? BomHostingV2.GetOnlyRepeatedIntersectingElementIds(hostInst.Document, (Element) hostInst, this.repeatedComponents).ToList<ElementId>() : BomHostingV2.GetOnlyRepeatedIntersectingElementIds(hostInst.Document, (Element) hostInst, this.repeatedComponentsForWarpable).ToList<ElementId>();
      if (source == null)
        return (List<ElementId>) null;
      this.PlateIdsPerStructuralFramingElement.Add(hostInst.UniqueId, source);
      list = source.Distinct<ElementId>().ToList<ElementId>();
    }
    return list;
  }

  public List<EDGEEmbedComponent> GetEmbedComponentDatas(
    FamilyInstance hostInstance,
    out Dictionary<string, List<ElementId>> embedsExcluded)
  {
    embedsExcluded = new Dictionary<string, List<ElementId>>();
    Document document = hostInstance.Document;
    List<EDGEEmbedComponent> embedComponentDatas = new List<EDGEEmbedComponent>();
    List<ElementId> forFamilyInstance = this.GetPlateIdsForFamilyInstance(hostInstance);
    if (forFamilyInstance == null)
      return (List<EDGEEmbedComponent>) null;
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> elementIdList2 = new List<ElementId>();
    List<ElementId> elementIdList3 = new List<ElementId>();
    List<ElementId> elementIdList4 = new List<ElementId>();
    foreach (ElementId id in forFamilyInstance)
    {
      bool flag = false;
      if (elementIdList2.Contains(id))
        flag = true;
      Element element = hostInstance.Document.GetElement(id);
      if (!elementIdList1.Contains(element.Id) && element != null)
      {
        if (elementIdList2.Contains(id))
          flag = true;
        else if (!elementIdList3.Contains(id))
        {
          if (Parameters.GetParameterAsBool(element, "MULTIPLY_FOR_MARKS"))
          {
            flag = true;
            elementIdList2.Add(id);
            elementIdList4.Add(element.Id);
          }
          else
            elementIdList3.Add(id);
        }
        Element elem = element;
        if (elem != null)
        {
          while (elem.HasSuperComponent() && !flag)
          {
            elem = elem.GetSuperComponent();
            if (elem != null)
            {
              if (elementIdList2.Contains(elem.Id))
              {
                flag = true;
                break;
              }
              if (!elementIdList3.Contains(elem.Id))
              {
                if (Parameters.GetParameterAsBool(elem, "MULTIPLY_FOR_MARKS"))
                {
                  flag = true;
                  elementIdList2.Add(elem.Id);
                  elementIdList4.Add(elem.Id);
                }
                else
                  elementIdList3.Add(elem.Id);
              }
            }
            else
              break;
          }
        }
      }
      if (!flag)
      {
        elementIdList3.Add(id);
        elementIdList1.Add(element.Id);
      }
    }
    foreach (ElementId id in elementIdList4)
    {
      Element element = document.GetElement(id);
      if (element != null)
      {
        string familyName = (element as FamilyInstance).Symbol.FamilyName;
        string name = (element as FamilyInstance).Symbol.Name;
        string key = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
        if (embedsExcluded.ContainsKey(key))
          embedsExcluded[key].Add(id);
        else
          embedsExcluded.Add(key, new List<ElementId>()
          {
            id
          });
      }
    }
    ElementFilter hwdFilter = Components.GetHWDFilter(document);
    foreach (ElementId id in elementIdList1)
    {
      if (!(document.GetElement(id) is FamilyInstance element))
        return (List<EDGEEmbedComponent>) null;
      if (hwdFilter != null)
      {
        if (hwdFilter.PassesFilter((Element) element))
        {
          if (element.HasSuperComponent() && element.SuperComponent is FamilyInstance superComponent1)
          {
            if (hwdFilter.PassesFilter((Element) superComponent1))
            {
              bool flag = false;
              while (superComponent1.HasSuperComponent())
              {
                if (superComponent1.SuperComponent is FamilyInstance superComponent1 && !hwdFilter.PassesFilter((Element) superComponent1))
                {
                  flag = true;
                  break;
                }
              }
              if (flag)
                continue;
            }
            else
              continue;
          }
        }
        else
          continue;
      }
      embedComponentDatas.Add(new EDGEEmbedComponent(element, hostInstance, this.CompareOption));
    }
    return embedComponentDatas;
  }
}
