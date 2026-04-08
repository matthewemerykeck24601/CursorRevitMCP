// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.CopyInsulation.CopyInsulation_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.InsulationTools.InsulationPlacement;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.InsulationTools.CopyInsulation;

[Transaction(TransactionMode.Manual)]
public class CopyInsulation_Command : IExternalCommand
{
  private AssemblyInstance sourceAssembly;
  private bool copyPins;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    List<FamilyInstance> sourceInsulation = new List<FamilyInstance>();
    List<AssemblyInstance> assemblyInstanceList1 = new List<AssemblyInstance>();
    Dictionary<ElementId, List<FamilyInstance>> dictionary = new Dictionary<ElementId, List<FamilyInstance>>();
    int num1 = document.IsFamilyDocument ? 1 : 0;
    List<ElementId> list1 = activeUiDocument.Selection.GetElementIds().ToList<ElementId>();
    List<AssemblyInstance> assemblyInstanceList2 = new List<AssemblyInstance>();
    if (list1.Count != 0)
    {
      assemblyInstanceList2 = new FilteredElementCollector(document, (ICollection<ElementId>) list1).OfCategory(BuiltInCategory.OST_Assemblies).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
      if (assemblyInstanceList2.Count == 0)
      {
        Element element = new FilteredElementCollector(document, (ICollection<ElementId>) list1).OfClass(typeof (FamilyInstance)).FirstOrDefault<Element>((Func<Element, bool>) (e => e.AssemblyInstanceId != ElementId.InvalidElementId && e.Category.Id.IntegerValue == -2001320));
        if (element != null)
          assemblyInstanceList2.Add(element.AssemblyInstanceId.GetElement() as AssemblyInstance);
      }
    }
    if (assemblyInstanceList2.Count == 1)
    {
      this.sourceAssembly = assemblyInstanceList2[0];
      if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) this.sourceAssembly, "HARDWARE_DETAIL"))
        return (Result) 1;
      List<ElementId> list2 = this.sourceAssembly.GetMemberIds().ToList<ElementId>();
      if (!new FilteredElementCollector(document, (ICollection<ElementId>) list2).OfCategory(BuiltInCategory.OST_StructuralFraming).Any<Element>())
        return (Result) 1;
      sourceInsulation = this.GatherInsulation(document, list2);
      dictionary = this.GatherPins(this.sourceAssembly, sourceInsulation);
    }
    else
    {
      try
      {
        Reference reference = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new SourceAssemblyInstancesAndStructuralFraming(), "Pick Source Assembly to copy.");
        if (document.GetElement(reference) != null)
        {
          this.sourceAssembly = document.GetElement(reference) as AssemblyInstance;
          List<ElementId> elementIdList = new List<ElementId>();
          if (this.sourceAssembly == null)
          {
            ElementId id = document.GetElement(reference).Id;
            if (id.GetElement().AssemblyInstanceId != ElementId.InvalidElementId && id.GetElement().Category.Id.IntegerValue == -2001320)
              this.sourceAssembly = id.GetElement().AssemblyInstanceId.GetElement() as AssemblyInstance;
          }
          List<ElementId> list3 = this.sourceAssembly.GetMemberIds().ToList<ElementId>();
          sourceInsulation = this.GatherInsulation(document, list3);
          dictionary = this.GatherPins(this.sourceAssembly, sourceInsulation);
        }
      }
      catch (Exception ex)
      {
        return (Result) 1;
      }
    }
    try
    {
      foreach (Reference pickObject in (IEnumerable<Reference>) activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new TargetAssemblyInstancesAndStructuralFraming(this.sourceAssembly), "Pick Target Assemblies to copy to."))
      {
        if (document.GetElement(pickObject) != null)
        {
          AssemblyInstance assemblyInstance = !(document.GetElement(pickObject) is AssemblyInstance) ? document.GetElement(pickObject).AssemblyInstanceId.GetElement() as AssemblyInstance : document.GetElement(pickObject) as AssemblyInstance;
          if (assemblyInstance != null && assemblyInstance != this.sourceAssembly)
            assemblyInstanceList1.Add(assemblyInstance);
        }
      }
    }
    catch (Exception ex)
    {
      return (Result) 1;
    }
    Transform transform1 = this.sourceAssembly.GetTransform();
    if (transform1 == null)
      return (Result) 1;
    CopyFailure copyFailure1 = new CopyFailure();
    Dictionary<string, List<CopyFailure>> sourceList = new Dictionary<string, List<CopyFailure>>();
    bool flag1 = false;
    Reference reference1 = (Reference) null;
    if (this.sourceAssembly.GetStructuralFramingElement() is FamilyInstance structuralFramingElement1)
    {
      flag1 = this.CheckAutomaticInsulationValidity((Element) structuralFramingElement1);
      if (flag1)
      {
        PlanarFace frontFace = PlacementUtilities.GetFrontFace((Element) structuralFramingElement1);
        if ((GeometryObject) frontFace != (GeometryObject) null)
          reference1 = frontFace.Reference;
      }
    }
    using (Transaction transaction = new Transaction(document, "Copy Insulation"))
    {
      int num2 = (int) transaction.Start();
      Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(document);
      Insulation.Addons = Components.GetAddonsForInsulationPlacement(document);
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList1)
      {
        Reference frontFaceReference = (Reference) null;
        Transform transform2 = assemblyInstance.GetTransform();
        if (transform2 == null)
        {
          CopyFailure newFailure = new CopyFailure(this.sourceAssembly, (FamilyInstance) null, (FamilyInstance) null, assemblyInstance, CopyFailure.reasonEnum.InvalidTargetTransform);
          sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
        }
        else if (!(assemblyInstance.GetStructuralFramingElement() is FamilyInstance structuralFramingElement2))
        {
          CopyFailure newFailure = new CopyFailure(this.sourceAssembly, (FamilyInstance) null, (FamilyInstance) null, assemblyInstance, CopyFailure.reasonEnum.InvalidSFElement);
          sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
        }
        else
        {
          bool flag2 = false;
          if (flag1)
            flag2 = this.CheckAutomaticInsulationValidity((Element) structuralFramingElement2);
          if (flag2)
          {
            PlanarFace frontFace = PlacementUtilities.GetFrontFace((Element) structuralFramingElement2);
            if ((GeometryObject) frontFace != (GeometryObject) null)
              frontFaceReference = frontFace.Reference;
          }
          foreach (FamilyInstance familyInstance1 in sourceInsulation)
          {
            CopyInsulationFamily insulationFamily = new CopyInsulationFamily(familyInstance1, transform1, transform2);
            FamilyInstance familyInstance2 = (FamilyInstance) null;
            if (frontFaceReference != null && familyInstance1.Host != null && familyInstance1.Host.Id.IntegerValue == structuralFramingElement1.Id.IntegerValue && familyInstance1.HostFace != null && familyInstance1.HostFace.ElementId.IntegerValue == reference1.ElementId.IntegerValue)
              familyInstance2 = insulationFamily.Copy(frontFaceReference);
            if (familyInstance2 == null)
              familyInstance2 = insulationFamily.Copy();
            if (familyInstance2.Id.ToString().EndsWith("9"))
              familyInstance2 = (FamilyInstance) null;
            if (familyInstance2 == null)
            {
              CopyFailure newFailure = new CopyFailure(this.sourceAssembly, familyInstance1, (FamilyInstance) null, assemblyInstance, CopyFailure.reasonEnum.InvalidInsulation);
              sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
            }
            else
            {
              this.copyInsulationParamters(familyInstance1, familyInstance2);
              this.CutInsulation((Element) structuralFramingElement2, familyInstance2);
              if (this.copyPins && dictionary.ContainsKey(familyInstance1.Id))
              {
                Transform transform3 = familyInstance1.GetTransform();
                if (transform3 == null)
                {
                  CopyFailure newFailure = new CopyFailure(this.sourceAssembly, familyInstance1, (FamilyInstance) null, assemblyInstance, CopyFailure.reasonEnum.InvalidSourceInsulationTransform);
                  sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
                  continue;
                }
                Transform transform4 = familyInstance2.GetTransform();
                if (transform4 == null)
                {
                  CopyFailure newFailure = new CopyFailure(this.sourceAssembly, familyInstance1, (FamilyInstance) null, assemblyInstance, CopyFailure.reasonEnum.InvalidTargetInsulationTransform);
                  sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
                  continue;
                }
                FamilyInstance pin = (FamilyInstance) null;
                foreach (FamilyInstance famInst in dictionary[familyInstance1.Id])
                {
                  if (new CopyInsulationFamily(famInst, transform3, transform4).Copy() == null)
                  {
                    CopyFailure newFailure = new CopyFailure(this.sourceAssembly, familyInstance1, pin, assemblyInstance, CopyFailure.reasonEnum.CopyPinFailure);
                    sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
                  }
                }
              }
              Utils.ElementUtils.Parameters.LookupParameter((Element) structuralFramingElement2, "INSULATION_INCLUDED")?.Set(0);
            }
          }
          if (this.copyPins && dictionary.ContainsKey(ElementId.InvalidElementId))
          {
            foreach (FamilyInstance famInst in dictionary[ElementId.InvalidElementId])
            {
              FamilyInstance pin = new CopyInsulationFamily(famInst, transform1, transform2).Copy();
              if (pin == null)
              {
                CopyFailure newFailure = new CopyFailure(this.sourceAssembly, (FamilyInstance) null, pin, assemblyInstance, CopyFailure.reasonEnum.InvalidPin);
                sourceList = this.GetFailureList(assemblyInstance.Name, sourceList, newFailure);
              }
            }
          }
        }
      }
      int num3 = (int) transaction.Commit();
    }
    if (sourceList.Count == 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Copy Insulation Success");
      taskDialog1.MainInstruction = "Copy Insulation - Insulation copy completed.";
      taskDialog1.MainContent = "Insulation and pins were copied from source to the indicated target(s).";
      taskDialog1.ExpandedContent = "Copied Pieces:\n";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList1)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{assemblyInstance.Name} - \n";
        foreach (FamilyInstance familyInstance3 in sourceInsulation)
        {
          TaskDialog taskDialog3 = taskDialog1;
          taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent}     Insulation - {familyInstance3.Id.ToString()}\n";
          foreach (FamilyInstance familyInstance4 in dictionary[familyInstance3.Id])
          {
            TaskDialog taskDialog4 = taskDialog1;
            taskDialog4.ExpandedContent = $"{taskDialog4.ExpandedContent}         Pin - {familyInstance4.Id.ToString()}\n";
          }
        }
      }
      taskDialog1.Show();
    }
    else
    {
      TaskDialog taskDialog5 = new TaskDialog("Copy Insulation Failure");
      taskDialog5.MainInstruction = "Copy Insulation - Insulation copy completed.";
      taskDialog5.MainContent = "Insulation and pins were copied from source to the indicated target(s). One or more of the insulation pieces or pins failed to copy.";
      taskDialog5.ExpandedContent = "Failed Pieces:\n";
      List<CopyFailure> copyFailureList = new List<CopyFailure>();
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList1)
      {
        if (sourceList.ContainsKey(assemblyInstance.Name))
        {
          TaskDialog taskDialog6 = taskDialog5;
          taskDialog6.ExpandedContent = $"{taskDialog6.ExpandedContent}{assemblyInstance.Name} - \n";
          foreach (CopyFailure copyFailure2 in sourceList[assemblyInstance.Name])
          {
            switch (copyFailure2.Reason)
            {
              case CopyFailure.reasonEnum.CopyPinFailure:
              case CopyFailure.reasonEnum.InvalidPin:
                TaskDialog taskDialog7 = taskDialog5;
                taskDialog7.ExpandedContent = $"{taskDialog7.ExpandedContent}     Insulation - {copyFailure2.Insulation.Id?.ToString()} placed\n         Invalid Pin - {copyFailure2.Pin.Id?.ToString()}\n";
                continue;
              case CopyFailure.reasonEnum.InvalidInsulation:
                TaskDialog taskDialog8 = taskDialog5;
                taskDialog8.ExpandedContent = $"{taskDialog8.ExpandedContent}     Invalid insulation - {copyFailure2.Insulation.Id?.ToString()}\n";
                continue;
              case CopyFailure.reasonEnum.InvalidSourceTransform:
              case CopyFailure.reasonEnum.InvalidTargetTransform:
                taskDialog5.ExpandedContent += "     invalid assembly transform\n";
                continue;
              case CopyFailure.reasonEnum.InvalidSourceInsulationTransform:
              case CopyFailure.reasonEnum.InvalidTargetInsulationTransform:
                TaskDialog taskDialog9 = taskDialog5;
                taskDialog9.ExpandedContent = $"{taskDialog9.ExpandedContent}     Insulation - {copyFailure2.Insulation.Id?.ToString()} invalid family transform\n";
                continue;
              default:
                continue;
            }
          }
        }
      }
      taskDialog5.Show();
    }
    return (Result) 0;
  }

  private Dictionary<string, List<CopyFailure>> GetFailureList(
    string assemblyName,
    Dictionary<string, List<CopyFailure>> sourceList,
    CopyFailure newFailure)
  {
    Dictionary<string, List<CopyFailure>> failureList = sourceList;
    if (sourceList.ContainsKey(assemblyName))
      failureList[assemblyName].Add(newFailure);
    else
      failureList.Add(assemblyName, new List<CopyFailure>()
      {
        newFailure
      });
    return failureList;
  }

  private List<FamilyInstance> GatherInsulation(Document revitDoc, List<ElementId> memberIds)
  {
    List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
    List<FamilyInstance> familyInstanceList2 = new List<FamilyInstance>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    foreach (FamilyInstance insulation in new FilteredElementCollector(revitDoc, (ICollection<ElementId>) memberIds).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (x => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(x))).Cast<FamilyInstance>().ToList<FamilyInstance>())
    {
      if (!this.IsNested(insulation))
        familyInstanceList2.Add(insulation);
    }
    return familyInstanceList2;
  }

  private bool IsNested(FamilyInstance insulation)
  {
    if (insulation.SuperComponent != null)
    {
      Element superComponent = insulation.SuperComponent;
      if (superComponent.Category.Id.IntegerValue == -2001320)
        return true;
      if (!(superComponent is FamilyInstance))
        return false;
      this.IsNested(superComponent as FamilyInstance);
    }
    return false;
  }

  private bool CheckAutomaticInsulationValidity(Element element)
  {
    if (!Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(element))
      return false;
    Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter(element, "DIM_LENGTH");
    if (parameter1 == null || !parameter1.HasValue)
      return false;
    Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter(element, "DIM_WIDTH");
    if (parameter2 == null || !parameter2.HasValue)
      return false;
    Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter(element, "DIM_WYTHE_INNER");
    if (parameter3 == null || !parameter3.HasValue)
      return false;
    Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter(element, "DIM_WYTHE_INSULATION");
    return parameter4 != null && parameter4.HasValue;
  }

  private void copyInsulationParamters(
    FamilyInstance sourceInsulation,
    FamilyInstance targetInsulation)
  {
    Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter((Element) sourceInsulation, "DIM_LENGTH");
    if (parameter1 != null && parameter1.HasValue)
    {
      double num = parameter1.AsDouble();
      Utils.ElementUtils.Parameters.LookupParameter((Element) targetInsulation, "DIM_LENGTH")?.Set(num);
    }
    Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter((Element) sourceInsulation, "DIM_WIDTH");
    if (parameter2 == null || !parameter2.HasValue)
      return;
    double num1 = parameter2.AsDouble();
    Utils.ElementUtils.Parameters.LookupParameter((Element) targetInsulation, "DIM_WIDTH")?.Set(num1);
  }

  private Dictionary<ElementId, List<FamilyInstance>> GatherPins(
    AssemblyInstance sourceAssembly,
    List<FamilyInstance> sourceInsulation)
  {
    Dictionary<ElementId, List<FamilyInstance>> dictionary = new Dictionary<ElementId, List<FamilyInstance>>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<FamilyInstance> list = new FilteredElementCollector(sourceAssembly.Document, sourceAssembly.GetMemberIds()).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString((Element) (e as FamilyInstance).Symbol, "MANUFACTURE_COMPONENT").ToUpper().Contains("PIN"))).Cast<FamilyInstance>().ToList<FamilyInstance>();
    list.OrderBy<FamilyInstance, Element>((Func<FamilyInstance, Element>) (e => e.Host));
    if (list.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("Copy Pins");
      taskDialog.AllowCancellation = false;
      taskDialog.MainInstruction = "There are pins in the source assembly.";
      taskDialog.MainContent = "Would you like to copy the pin placement to the target assemblies? ";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Copy Pins");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Do Not Copy");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      this.copyPins = taskDialog.Show() == 1001;
      if (this.copyPins)
      {
        foreach (FamilyInstance familyInstance in list)
        {
          FamilyInstance hostInsulation = familyInstance.Host as FamilyInstance;
          if (hostInsulation != null)
          {
            if (sourceInsulation.Where<FamilyInstance>((Func<FamilyInstance, bool>) (i => i.Id == hostInsulation.Id)).Count<FamilyInstance>() > 0)
            {
              if (dictionary.ContainsKey(hostInsulation.Id))
                dictionary[hostInsulation.Id].Add(familyInstance);
              else
                dictionary.Add(hostInsulation.Id, new List<FamilyInstance>()
                {
                  familyInstance
                });
            }
            else if (dictionary.ContainsKey(ElementId.InvalidElementId))
              dictionary[ElementId.InvalidElementId].Add(familyInstance);
            else
              dictionary.Add(ElementId.InvalidElementId, new List<FamilyInstance>()
              {
                familyInstance
              });
          }
          else if (dictionary.ContainsKey(ElementId.InvalidElementId))
            dictionary[ElementId.InvalidElementId].Add(familyInstance);
          else
            dictionary.Add(ElementId.InvalidElementId, new List<FamilyInstance>()
            {
              familyInstance
            });
        }
      }
    }
    return dictionary;
  }

  private void CutInsulation(Element wall, FamilyInstance insulation)
  {
    string uniqueId = wall.GetTopLevelElement().UniqueId;
    bool flag1 = false;
    Document document = insulation.Document;
    BoundingBoxXYZ boundingBoxXyz = insulation.get_BoundingBox((View) null);
    Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
    ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    LogicalOrFilter filter2 = new LogicalOrFilter((ElementFilter) new BoundingBoxIsInsideFilter(outline), (ElementFilter) new BoundingBoxIntersectsFilter(outline));
    FilteredElementCollector elementCollector = new FilteredElementCollector(insulation.Document).WherePasses((ElementFilter) filter1).WherePasses((ElementFilter) filter2);
    List<ElementId> list = insulation.GetAllSubcomponents().ToList<ElementId>();
    foreach (Element element1 in elementCollector.ToElements().Where<Element>((Func<Element, bool>) (e => !e.GetTopLevelElement().Id.Equals((object) insulation.Id))).ToList<Element>())
    {
      if (!list.Contains(element1.Id))
      {
        Element element2 = (Element) null;
        Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter(element1, "HOST_GUID");
        bool voidCut = false;
        bool flag2 = false;
        if (Insulation.Addons.Contains(element1.Id))
        {
          if (uniqueId.Equals(parameter1.AsString()))
          {
            flag2 = true;
            element2 = element1;
          }
          if (!flag2 && JoinGeometryUtils.AreElementsJoined(wall.Document, wall, element1))
          {
            flag2 = true;
            element2 = element1;
          }
        }
        if (!flag2)
        {
          Element element3 = element1;
          if (element3 is FamilyInstance && (element3 as FamilyInstance).SuperComponent != null)
            element3 = insulation.Document.GetElement((element3 as FamilyInstance).SuperComponent.Id);
          Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter(element3, "MANUFACTURE_COMPONENT");
          if (parameter2 != null && parameter2.HasValue)
          {
            if (parameter2.AsString().Contains("WOOD") && parameter2.AsString().Contains("NAILER"))
            {
              if (uniqueId.Equals(parameter1.AsString()))
              {
                flag2 = true;
                element2 = element1;
              }
              if (!flag2 && JoinGeometryUtils.AreElementsJoined(wall.Document, wall, element1))
              {
                flag2 = true;
                element2 = element1;
              }
            }
            if (!flag2)
            {
              if (PlacementUtilities.checkVoidsForCutting(element3, wall, false, element3, out voidCut))
              {
                element2 = element3;
                flag2 = true;
              }
              if (!flag2 && (element3 as FamilyInstance).GetAllSubcomponents().Contains<ElementId>(element1.Id) && PlacementUtilities.checkVoidsForCutting(element1, wall, true, element3, out voidCut))
              {
                element2 = element3;
                flag2 = true;
                voidCut = true;
              }
            }
          }
        }
        if (flag2)
        {
          try
          {
            Transform insulationTransform = insulation.GetTransform();
            if ((voidCut ? (InstanceVoidCutUtils.CanBeCutWithVoid((Element) insulation) ? 1 : 0) : (SolidSolidCutUtils.CanElementCutElement(element2, (Element) insulation, out CutFailureReason _) ? 1 : 0)) != 0)
            {
              if (voidCut)
                InstanceVoidCutUtils.AddInstanceVoidCut(document, (Element) insulation, element2);
              else
                SolidSolidCutUtils.AddCutBetweenSolids(document, (Element) insulation, element2);
              document.Regenerate();
              bool bSymbol;
              List<Solid> source = Solids.GetSymbolSolids((Element) insulation, out bSymbol);
              if (bSymbol)
                source = source.Select<Solid, Solid>((Func<Solid, Solid>) (s => SolidUtils.CreateTransformed(s, insulationTransform))).ToList<Solid>();
              if (source.Count == 0)
              {
                wall.Document.Delete(insulation.Id);
                flag1 = true;
              }
              if (flag1)
                break;
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
    }
  }
}
