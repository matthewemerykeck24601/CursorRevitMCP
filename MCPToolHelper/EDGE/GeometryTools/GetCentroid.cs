// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.GetCentroid
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class GetCentroid : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    View activeView = document.ActiveView;
    Transaction transaction = new Transaction(revitDoc, "Get Centroid");
    XYZ xyz = new XYZ();
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Get Centroid must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Get Centroid must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    FamilySymbol[] centroidFamilies = this.getCentroidFamilies(document);
    if (centroidFamilies[1] == null || centroidFamilies[0] == null)
    {
      new TaskDialog("Centroid Project Family")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "One or more EDGE centroid families were not found.",
        MainContent = "The EDGE centroid families have not been loaded into the project. Please load the family into the project before running this command.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (!Utils.ElementUtils.Parameters.ParamExistsForDoc(document, "HOST_GUID", BuiltInCategory.OST_SpecialityEquipment, ParamType.INSTANCE))
    {
      if (Utils.ElementUtils.Parameters.FamParamIsInstance(centroidFamilies[0].Family, "HOST_GUID"))
      {
        TaskDialog taskDialog = new TaskDialog("Centroid Family")
        {
          AllowCancellation = false,
          CommonButtons = (TaskDialogCommonButtons) 1,
          MainInstruction = "The EDGE centroid family had HOST_GUID built into its family definition. Continue?",
          MainContent = "You may proceed with the Get Centroid operation, but note that this is not the recommended workflow for other EDGE tools. Consider redefining HOST_GUID as a project parameter.",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        };
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
        if (taskDialog.Show() == 2)
          return (Result) 1;
      }
      else
      {
        new TaskDialog("No HOST_GUID Parameter")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "The HOST_GUID parameter was not defined as an instance parameter for this document",
          MainContent = "The Get Centroid tool requires HOST_GUID to be defined as an instance parameter bound to the Specialty Equipment category in order to run correctly. Please add this parameter and try again.",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        }.Show();
        return (Result) 1;
      }
    }
    if (activeView.ViewType.Equals((object) ViewType.Schedule))
    {
      TaskDialog.Show("Invalid View", "Get Centroid cannot be used in Schedule View.");
      return (Result) 1;
    }
    if (activeView.ViewType.Equals((object) ViewType.ProjectBrowser))
    {
      TaskDialog.Show("Invalid View", "Get Centroid cannot be used in the Project Browser.");
      return (Result) 1;
    }
    try
    {
      bool cancelled = false;
      ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
      TaskDialog taskDialog1 = new TaskDialog("Get Centroid");
      taskDialog1.Id = "ID_GetCentroid";
      taskDialog1.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
      taskDialog1.Title = "Get the Center of Gravity";
      taskDialog1.TitleAutoPrefix = true;
      taskDialog1.AllowCancellation = true;
      taskDialog1.MainInstruction = "Get the Centroid";
      taskDialog1.MainContent = "Select the scope for getting the Center of Gravity.\nEDGE Centroid families must be loaded into the project to use this tool.";
      taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Get the Center of Gravity for the Whole Project.");
      taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Get the Center of Gravity for a Selection Group.");
      taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Get the Center of Gravity for Selected Element.");
      taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog1.DefaultButton = (TaskDialogResult) 2;
      int num1 = (int) transaction.Start();
      int count1 = elementIds.Count;
      ICollection<ElementId> list1 = (ICollection<ElementId>) elementIds.Where<ElementId>((Func<ElementId, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsBool(revitDoc.GetElement(e), "HARDWARE_DETAIL"))).ToList<ElementId>();
      int count2 = list1.Count;
      if (count1 != count2)
      {
        TaskDialog taskDialog2 = new TaskDialog("Hardware Detail Elements Selected");
        taskDialog2.MainContent = "One or more elements in the selection group were flagged as hardware detail elements. These will not be processed by Get Centroid.";
        taskDialog2.CommonButtons = (TaskDialogCommonButtons) 9;
        if (list1.Count == 0)
          taskDialog2.CommonButtons = (TaskDialogCommonButtons) 8;
        if (taskDialog2.Show() != 1)
        {
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
      }
      if (list1.Count<ElementId>() == 0)
      {
        TaskDialogResult taskDialogResult = taskDialog1.Show();
        if (taskDialogResult == 1001)
        {
          XYZ centroid = this.CalculateCentroid(document, (ICollection<ElementId>) null, centroidFamilies, out cancelled);
          if (cancelled)
          {
            int num3 = (int) transaction.RollBack();
            return (Result) 1;
          }
          document.Regenerate();
          int num4 = (int) transaction.Commit();
          if (centroid != null)
            TaskDialog.Show("Get Centroid", "Project centroid placed successfully.");
          return (Result) 0;
        }
        if (taskDialogResult == 1002)
        {
          ICollection<ElementId> list2 = (ICollection<ElementId>) activeUiDocument.Selection.PickObjects((ObjectType) 1).Select<Reference, ElementId>((Func<Reference, ElementId>) (reference => reference.ElementId)).ToList<ElementId>();
          if (list2 == null || list2.Count == 0)
          {
            int num5 = (int) transaction.RollBack();
            return (Result) 1;
          }
          XYZ centroid = this.CalculateCentroid(document, list2, centroidFamilies, out cancelled);
          if (cancelled)
          {
            int num6 = (int) transaction.RollBack();
            return (Result) 1;
          }
          document.Regenerate();
          int num7 = (int) transaction.Commit();
          if (centroid != null)
            TaskDialog.Show("Get Centroid", "Successfully completed Get Centroid operation for selection group");
          return (Result) 0;
        }
        if (taskDialogResult == 1003)
        {
          ICollection<ElementId> selectedElements = (ICollection<ElementId>) new List<ElementId>()
          {
            activeUiDocument.Selection.PickObject((ObjectType) 1).ElementId
          };
          if (selectedElements == null || selectedElements.Count == 0)
          {
            int num8 = (int) transaction.RollBack();
            return (Result) 1;
          }
          XYZ centroid = this.CalculateCentroid(document, selectedElements, centroidFamilies, out cancelled);
          if (cancelled)
          {
            int num9 = (int) transaction.RollBack();
            return (Result) 1;
          }
          document.Regenerate();
          int num10 = (int) transaction.Commit();
          if (centroid != null)
            TaskDialog.Show("Get Centroid", "Successfully placed centroid for selected element.");
          return (Result) 0;
        }
        if (transaction.HasStarted())
        {
          int num11 = (int) transaction.RollBack();
        }
        return (Result) 1;
      }
      XYZ centroid1 = this.CalculateCentroid(document, list1, centroidFamilies, out cancelled);
      if (cancelled)
      {
        int num12 = (int) transaction.RollBack();
        return (Result) 1;
      }
      document.Regenerate();
      int num13 = (int) transaction.Commit();
      if (centroid1 != null)
        TaskDialog.Show("Get Centroid", "Successfully completed Get Centroid operation.");
      return (Result) 0;
    }
    catch (Exception ex)
    {
      if (transaction.HasStarted())
      {
        int num14 = (int) transaction.RollBack();
      }
      if (ex is Autodesk.Revit.Exceptions.OperationCanceledException)
        return (Result) 1;
      if (ex.ToString().Contains("Empty Project"))
      {
        new TaskDialog("Get Centroid")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "No applicable elements were found in the model. Cancelling."
        }.Show();
        return (Result) 1;
      }
      if (ex.ToString().Contains("No Solids"))
      {
        new TaskDialog("Get Centroid")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "No processed elements had solid geometry. Cancelling."
        }.Show();
        return (Result) 1;
      }
      if (ex.ToString().Contains("No Concrete"))
      {
        new TaskDialog("Get Centroid")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "Processed elements did not contain precast concrete among their materials. Cancelling."
        }.Show();
        return (Result) 1;
      }
      int num15 = (int) MessageBox.Show(ex.ToString());
      return (Result) -1;
    }
  }

  private bool testMaterial(Element elem)
  {
    Document document = elem.Document;
    foreach (ElementId materialId in (IEnumerable<ElementId>) elem.GetMaterialIds(false))
    {
      string name = document.GetElement(materialId).Name;
      if (name.ToUpper().Contains("PRECAST") || name.ToUpper().Contains("CONCRETE"))
        return true;
    }
    return false;
  }

  private XYZ CalculateCentroid(
    Document revitDoc,
    ICollection<ElementId> selectedElements,
    FamilySymbol[] centroidFamilies,
    out bool cancelled)
  {
    if (!centroidFamilies[0].IsActive)
      centroidFamilies[0].Activate();
    if (!centroidFamilies[1].IsActive)
      centroidFamilies[1].Activate();
    cancelled = false;
    XYZ centroid1 = (XYZ) null;
    List<Solid> solidList1 = new List<Solid>();
    List<Solid> solidList2 = new List<Solid>();
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> source1 = new List<ElementId>();
    List<ElementId> elementIdList2 = new List<ElementId>();
    FamilySymbol centroidFamily1 = centroidFamilies[0];
    FamilySymbol centroidFamily2 = centroidFamilies[1];
    if (selectedElements == null)
    {
      if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.PlaceCentroid))
      {
        cancelled = true;
        return (XYZ) null;
      }
      List<ElementId> elementIdList3 = new List<ElementId>();
      List<ElementId> pinnedList = new List<ElementId>();
      List<ElementId> centroidInstances = this.getRelevantCentroidInstances(revitDoc, new List<string>(), out pinnedList, true);
      if (pinnedList.Count > 0)
      {
        TaskDialog taskDialog = new TaskDialog("Get Centroid");
        taskDialog.MainInstruction = "One or more existing project centroids were pinned.";
        taskDialog.MainContent = "Unpin or delete these elements and try again.  Expand for details.";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (ElementId elementId in pinnedList)
        {
          stringBuilder.Append(elementId.IntegerValue);
          stringBuilder.AppendLine();
        }
        taskDialog.ExpandedContent = stringBuilder.ToString();
        taskDialog.Show();
        return (XYZ) null;
      }
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_SpecialityEquipment
      });
      List<Element> list1 = new FilteredElementCollector(revitDoc).WherePasses((ElementFilter) filter).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsBool(e, "HARDWARE_DETAIL"))).ToList<Element>();
      list1.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Select<Element, Element>((Func<Element, Element>) (elem => StructuralFraming.RefineNestedFamily(elem))).ToList<Element>());
      if (list1.Count == 0)
        throw new Exception("Empty Project");
      foreach (Element elem in list1)
      {
        if (this.testMaterial(elem))
          elementIdList2.Add(elem.Id);
      }
      if (elementIdList2.Count == 0)
        throw new Exception("No Concrete");
      foreach (ElementId id in elementIdList2)
      {
        List<Solid> list2 = Solids.GetInstanceSolids(revitDoc.GetElement(id)).Where<Solid>((Func<Solid, bool>) (solid => solid.Volume > 0.0)).ToList<Solid>();
        if (list2 != null && list2.Count != 0)
          solidList1.AddRange((IEnumerable<Solid>) list2);
      }
      if (solidList1.Count<Solid>() == 0)
        throw new Exception("No Solids");
      XYZ centroid2 = solidList1.Count != 1 ? GetCentroid.CalcCentroid(revitDoc, solidList1) : solidList1.First<Solid>().ComputeCentroid();
      GetCentroid.PlaceCentroidFamilyWholeProject(revitDoc, centroid2, centroidInstances, centroidFamily2);
      return centroid2;
    }
    List<ElementId> list3 = selectedElements.Select<ElementId, Element>((Func<ElementId, Element>) (eid => revitDoc.GetElement(eid))).Where<Element>((Func<Element, bool>) (elem => elem.Category.Id.IntegerValue == -2001320)).Select<Element, ElementId>((Func<Element, ElementId>) (elem => StructuralFraming.RefineNestedFamily(elem).Id)).Where<ElementId>((Func<ElementId, bool>) (elem => this.testMaterial(revitDoc.GetElement(elem)))).ToList<ElementId>();
    List<ElementId> list4 = selectedElements.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (eid => revitDoc.GetElement(eid) as AssemblyInstance)).Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (elem => elem != null && !Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem, "HARDWARE_DETAIL") && elem.GetStructuralFramingElement() != null && this.testMaterial(elem.GetStructuralFramingElement()))).Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (assem => assem.Id)).ToList<ElementId>();
    TaskDialogResult taskDialogResult1 = (TaskDialogResult) 0;
    List<ElementId> list5 = selectedElements.Where<ElementId>((Func<ElementId, bool>) (eid => this.testMaterial(revitDoc.GetElement(eid)))).ToList<ElementId>();
    if (list3.Count > 0 || list4.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("Get Centroid");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Would you like to place individual centroids?";
      taskDialog.MainContent = "Your selection group includes Structural Framing and/or Structural Framing Assemblies. Would you like to place individual centroids for these elements or report an overall selection group centroid relative to the project base point?";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Place individual centroids");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Report selection group centroid");
      if (list3.Count + list4.Count == 1 && (selectedElements.Count == 1 || list4.Count == 1 && selectedElements.Count - list5.Count == list4.Count || list3.Count == 1 && list5.Count == 1))
        taskDialogResult1 = (TaskDialogResult) 1001;
      else if (list4.Count == 1 && list3.Count == 1)
      {
        taskDialogResult1 = !(revitDoc.GetElement(list4.First<ElementId>()) as AssemblyInstance).GetStructuralFramingElement().Id.Equals((object) list3.First<ElementId>()) ? taskDialog.Show() : (TaskDialogResult) 1001;
      }
      else
      {
        if (selectedElements.Count > list3.Count + list4.Count)
        {
          if (list5.Count > 0)
            TaskDialog.Show("Get Centroid", "Your selection contains both Structural Framing/Assembly pieces and other concrete. Get Centroid for individual will place centroid(s) for structural framing/assemblies in your selection group and their hosted (or assembly member) addons. Report selection group centroid will display the centroid for all concrete in the selection group.");
          else
            TaskDialog.Show("Get Centroid", "Your selection contains both Structural Framing/Assembly pieces and non-concrete elements. Get Centroid for individual will place centroid(s) for structural framing/assemblies in your selection group and their hosted (or assembly member) addons. Report selection group centroid will display the centroid for all concrete in the selection group.");
        }
        taskDialogResult1 = taskDialog.Show();
      }
    }
    else if (selectedElements.Count > 0 && list3.Count == 0 && list4.Count == 0)
    {
      selectedElements = (ICollection<ElementId>) selectedElements.Where<ElementId>((Func<ElementId, bool>) (e => !(revitDoc.GetElement(e) is AssemblyInstance))).ToList<ElementId>();
      if (selectedElements.Count == 0)
      {
        TaskDialog.Show("Get Centroid", "Assemblies must contain a concrete structural framing element to be considered by Get Centroid.");
        return (XYZ) null;
      }
      TaskDialog taskDialog = new TaskDialog("Get Centroid");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Would you like to place individual centroids?";
      taskDialog.MainContent = "Your selection group does not include Structural Framing or Structural Framing Assemblies. Would you like to place individual centroids for these elements or report an overall selection group centroid relative to the project base point?";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Place individual centroids");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1004, "Report selection group centroid");
      source1 = selectedElements.ToList<ElementId>();
      if (selectedElements.Count == 1)
        taskDialogResult1 = (TaskDialogResult) 1003;
      else if (list5.Count > 0 && list5.Count < selectedElements.Count)
      {
        source1 = list5.ToList<ElementId>();
        TaskDialog.Show("Get Centroid", "Your selection contains both concrete and non-concrete elements. Get Centroid will process centroid(s) for only concrete elements in your selection group.");
        taskDialogResult1 = list5.Count != 1 ? taskDialog.Show() : (TaskDialogResult) 1003;
      }
      else if (list5.Count == 0)
      {
        TaskDialog.Show("Get Centroid", "Your selection contains only non-concrete elements. Get Centroid will place individual centroid(s) for all elements in your selection group.");
        taskDialogResult1 = (TaskDialogResult) 1003;
      }
      else
        taskDialogResult1 = taskDialog.Show();
    }
    List<ElementId> source2 = new List<ElementId>();
    List<ElementId> pinnedList1 = new List<ElementId>();
    TaskDialog taskDialog1 = new TaskDialog("Get Centroid");
    taskDialog1.MainInstruction = "One or more elements that would have received centroids had previous centroids that were pinned.";
    taskDialog1.MainContent = "Unpin or delete these elements and try again.  Expand for details.";
    switch (taskDialogResult1 - 1001)
    {
      case 0:
        if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.PlaceCentroid))
        {
          cancelled = true;
          return (XYZ) null;
        }
        List<Element> list6 = Components.GetAllProjectAddonsAHU(revitDoc).Select<FamilyInstance, Element>((Func<FamilyInstance, Element>) (e => (Element) e)).ToList<Element>();
        if (Components.CheckAddonHosting(revitDoc, list6))
        {
          TaskDialogResult taskDialogResult2 = new TaskDialog("Warning")
          {
            AllowCancellation = false,
            CommonButtons = ((TaskDialogCommonButtons) 6),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Document contains non hosted Addons.  Continue?",
            MainContent = "Addons in this document have not had Addon Hosting Updater run on them. Get Centroid requires the user to have run Addon Hosting Updater on any elements which would be processed by the Get Centroid Tool.  Please ensure that Addon Hosting Updater has been run on the addons for which you wish to run the Get Centroid Tool.  Get Centroid may not produce accurate results if Addon Hosting Updater has not been run successfully on the entire model (or at least the pieces considered)."
          }.Show();
          if (taskDialogResult2 == 7 || taskDialogResult2 == 2)
            return (XYZ) null;
        }
        Dictionary<string, List<ElementId>> dictionary = new Dictionary<string, List<ElementId>>();
        List<ElementId> elementIdList4 = new List<ElementId>();
        source2.AddRange((IEnumerable<ElementId>) list3);
        List<string> list7 = source2.Select<ElementId, string>((Func<ElementId, string>) (eid => revitDoc.GetElement(eid).UniqueId)).ToList<string>();
        foreach (string key in list7)
          dictionary.Add(key, new List<ElementId>());
        foreach (ElementId id in list4)
        {
          AssemblyInstance element = revitDoc.GetElement(id) as AssemblyInstance;
          Element elem1 = element.GetStructuralFramingElement();
          List<ElementId> list8 = element.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)).Where<Element>((Func<Element, bool>) (elem => (elem.Category.Id.IntegerValue == -2000011 || elem.Category.Id.IntegerValue == -2000151 || elem.Category.Id.IntegerValue == -2001350) && elem is FamilyInstance)).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
          if (elem1 != null)
            elem1 = StructuralFraming.RefineNestedFamily(elem1);
          if (!source2.Contains(elem1.Id))
            source2.Add(elem1.Id);
          string uniqueId = elem1.UniqueId;
          if (!list7.Contains(uniqueId))
          {
            list7.Add(uniqueId);
            dictionary.Add(uniqueId, new List<ElementId>());
          }
          if (list8.Count > 0)
          {
            dictionary[uniqueId].AddRange((IEnumerable<ElementId>) list8);
            elementIdList4.AddRange((IEnumerable<ElementId>) list8);
          }
        }
        foreach (Element elem in list6)
        {
          string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID");
          if (list7.Contains(parameterAsString) && !elementIdList4.Contains(elem.Id))
          {
            elementIdList4.Add(elem.Id);
            dictionary[parameterAsString].Add(elem.Id);
          }
        }
        List<ElementId> centroidInstances1 = this.getRelevantCentroidInstances(revitDoc, dictionary.Keys.ToList<string>(), out pinnedList1);
        if (pinnedList1.Count > 0)
        {
          StringBuilder stringBuilder = new StringBuilder();
          foreach (ElementId elementId in pinnedList1)
          {
            stringBuilder.Append(elementId.IntegerValue);
            stringBuilder.AppendLine();
          }
          taskDialog1.ExpandedContent = stringBuilder.ToString();
          taskDialog1.Show();
          return (XYZ) null;
        }
        foreach (ElementId elementId in source2)
        {
          Element element = revitDoc.GetElement(elementId);
          string uniqueId = element.UniqueId;
          List<Element> elementList = new List<Element>()
          {
            element
          };
          List<Solid> solidList3 = new List<Solid>();
          foreach (ElementId id in dictionary[uniqueId])
            elementList.Add(revitDoc.GetElement(id));
          if (this.testMaterial(element))
          {
            foreach (Element elem in elementList)
            {
              if (this.testMaterial(elem))
                solidList3.AddRange(Solids.GetInstanceSolids(elem).Where<Solid>((Func<Solid, bool>) (solid => solid.Volume > 0.0)));
            }
            if (solidList3.Count != 0)
            {
              XYZ centroid3 = GetCentroid.CalcCentroid(revitDoc, solidList3);
              GetCentroid.PlaceCentroidFamily(revitDoc, centroid3, elementId, centroidInstances1, centroidFamily1);
            }
          }
        }
        return new XYZ();
      case 1:
        ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_GenericModel,
          BuiltInCategory.OST_Walls,
          BuiltInCategory.OST_SpecialityEquipment
        });
        IEnumerable<Element> source3 = new FilteredElementCollector(revitDoc, selectedElements).WherePasses((ElementFilter) filter1).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsBool(e, "HARDWARE_DETAIL")));
        List<ElementId> list9 = list3.ToList<ElementId>().Union<ElementId>(source3.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id))).ToList<ElementId>();
        foreach (ElementId id in list4)
        {
          AssemblyInstance element1 = revitDoc.GetElement(id) as AssemblyInstance;
          Element elem = element1.GetStructuralFramingElement();
          if (elem != null)
            elem = StructuralFraming.RefineNestedFamily(elem);
          if (!list9.Contains(elem.Id))
            list9.Add(elem.Id);
          foreach (Element element2 in element1.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)))
          {
            if (filter1.PassesFilter(revitDoc, element2.Id) && !list9.Contains(element2.Id))
              list9.Add(element2.Id);
          }
        }
        List<Solid> solidList4 = new List<Solid>();
        foreach (ElementId id in list9)
        {
          Element element = revitDoc.GetElement(id);
          if (this.testMaterial(element))
            solidList4.AddRange(Solids.GetInstanceSolids(element).Where<Solid>((Func<Solid, bool>) (solid => solid.Volume > 0.0)));
        }
        XYZ centroid4 = GetCentroid.CalcCentroid(revitDoc, solidList4);
        string str = GetCentroid.DisplayCentroid(revitDoc, centroid4);
        new TaskDialog("Centroid")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = str
        }.Show();
        return centroid4;
      case 2:
        if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.PlaceCentroid))
        {
          cancelled = true;
          return (XYZ) null;
        }
        List<ElementId> elementIdList5 = new List<ElementId>();
        List<string> list10 = source1.Select<ElementId, string>((Func<ElementId, string>) (e => revitDoc.GetElement(e).UniqueId)).ToList<string>();
        List<ElementId> centroidInstances2 = this.getRelevantCentroidInstances(revitDoc, list10, out pinnedList1);
        if (pinnedList1.Count > 0)
        {
          StringBuilder stringBuilder = new StringBuilder();
          foreach (ElementId elementId in pinnedList1)
          {
            stringBuilder.Append(elementId.IntegerValue);
            stringBuilder.AppendLine();
          }
          taskDialog1.ExpandedContent = stringBuilder.ToString();
          taskDialog1.Show();
          return (XYZ) null;
        }
        foreach (ElementId elementId in source1)
        {
          List<Solid> list11 = Solids.GetInstanceSolids(revitDoc.GetElement(elementId)).Where<Solid>((Func<Solid, bool>) (solid => solid.Volume > 0.0)).ToList<Solid>();
          if (list11.Count > 0)
          {
            elementIdList5.Add(elementId);
            GetCentroid.PlaceCentroidFamily(revitDoc, GetCentroid.CalcCentroid(revitDoc, list11), elementId, centroidInstances2, centroidFamily1);
          }
        }
        if (elementIdList5.Count == 0)
          throw new Exception("No Solids");
        return centroid1;
      case 3:
        List<Solid> solidList5 = new List<Solid>();
        List<ElementId> list12 = selectedElements.Where<ElementId>((Func<ElementId, bool>) (e => this.testMaterial(revitDoc.GetElement(e)))).ToList<ElementId>();
        foreach (ElementId id in list12.Count > 0 ? list12.ToList<ElementId>() : selectedElements.ToList<ElementId>())
        {
          List<Solid> list13 = Solids.GetInstanceSolids(revitDoc.GetElement(id)).Where<Solid>((Func<Solid, bool>) (solid => solid.Volume > 0.0)).ToList<Solid>();
          if (list13.Count > 0)
            solidList5.AddRange((IEnumerable<Solid>) list13);
        }
        XYZ centroid5 = solidList5.Count != 0 ? GetCentroid.CalcCentroid(revitDoc, solidList5) : throw new Exception("No Solids");
        new TaskDialog("Centroid")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = GetCentroid.DisplayCentroid(revitDoc, centroid5)
        }.Show();
        return centroid5;
      default:
        return (XYZ) null;
    }
  }

  private List<ElementId> getRelevantCentroidInstances(
    Document revitDoc,
    List<string> keys,
    out List<ElementId> pinnedList,
    bool project = false)
  {
    pinnedList = new List<ElementId>();
    List<ElementId> centroidInstances = new List<ElementId>();
    string name = project ? "CENTROID_PROJECT" : "CENTROID";
    List<Element> list = new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (e => e.Name == name)).ToList<Element>();
    if (!project)
    {
      foreach (Element elem in list)
      {
        if (keys.Contains(Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID")))
        {
          if (elem.Pinned)
            pinnedList.Add(elem.Id);
          else
            centroidInstances.Add(elem.Id);
        }
      }
    }
    else
    {
      foreach (ElementId id in list.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>())
      {
        Element element = revitDoc.GetElement(id);
        if (element.Pinned)
          pinnedList.Add(element.Id);
        else
          centroidInstances.Add(element.Id);
      }
    }
    return centroidInstances;
  }

  private static List<double> GetVolume(List<Solid> solidList)
  {
    List<double> volume = new List<double>();
    foreach (Solid solid in solidList)
      volume.Add(solid.Volume);
    return volume;
  }

  private static List<double> GetSurfaceArea(List<Solid> solidList)
  {
    List<double> surfaceArea = new List<double>();
    foreach (Solid solid in solidList)
      surfaceArea.Add(solid.SurfaceArea);
    return surfaceArea;
  }

  private static XYZ CalcCentroid(Document revitDoc, List<Solid> solidList)
  {
    XYZ xyz = new XYZ();
    double num1 = 0.0;
    double num2 = 0.0;
    double num3 = 0.0;
    double num4 = 0.0;
    foreach (Solid solid in solidList)
    {
      XYZ centroid = solid.ComputeCentroid();
      double volume = solid.Volume;
      num2 += volume * centroid.X;
      num3 += volume * centroid.Y;
      num4 += volume * centroid.Z;
      num1 += volume;
    }
    return new XYZ(num2 / num1, num3 / num1, num4 / num1);
  }

  private static bool checkPins(
    Document revitDoc,
    ElementId selectedId,
    List<ElementId> centroidInstances,
    out List<ElementId> pinnedElems)
  {
    pinnedElems = new List<ElementId>();
    string uniqueId = revitDoc.GetElement(selectedId).UniqueId;
    foreach (ElementId centroidInstance in centroidInstances)
    {
      FamilyInstance element = revitDoc.GetElement(centroidInstance) as FamilyInstance;
      if (Utils.ElementUtils.Parameters.GetParameterAsString((Element) element, "HOST_GUID").Equals(uniqueId) && element.Pinned)
        pinnedElems.Add(centroidInstance);
    }
    return pinnedElems.Count <= 0;
  }

  private FamilySymbol[] getCentroidFamilies(Document revitDoc)
  {
    FamilySymbol[] centroidFamilies = new FamilySymbol[2];
    foreach (Element element in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilySymbol)))
    {
      if (element.Name.Equals("CENTROID"))
        centroidFamilies[0] = element as FamilySymbol;
      if (element.Name.Equals("CENTROID_PROJECT"))
        centroidFamilies[1] = element as FamilySymbol;
    }
    return centroidFamilies;
  }

  private static bool PlaceCentroidFamily(
    Document revitDoc,
    XYZ centroid,
    ElementId selectedId,
    List<ElementId> centroidInstances,
    FamilySymbol centroidFamily)
  {
    string uniqueId = revitDoc.GetElement(selectedId).UniqueId;
    List<ElementId> source = new List<ElementId>();
    foreach (ElementId centroidInstance in centroidInstances)
    {
      Element element = revitDoc.GetElement(centroidInstance);
      if (Utils.ElementUtils.Parameters.GetParameterAsString(element, "HOST_GUID").Equals(uniqueId))
        source.Add(element.Id);
    }
    List<ElementId> list = source.Distinct<ElementId>().ToList<ElementId>();
    ElementId id = ElementId.InvalidElementId;
    if (list.Count >= 1)
    {
      id = list.First<ElementId>();
      list.Remove(list.First<ElementId>());
    }
    foreach (ElementId elementId in list)
      revitDoc.Delete(elementId);
    if (id != ElementId.InvalidElementId)
    {
      Element element = revitDoc.GetElement(id);
      if (element.Location.Move(centroid - (element.Location as LocationPoint).Point))
        return true;
    }
    revitDoc.Create.NewFamilyInstance(centroid, centroidFamily, StructuralType.NonStructural).LookupParameter("HOST_GUID").Set(uniqueId);
    return true;
  }

  private static bool PlaceCentroidFamilyWholeProject(
    Document revitDoc,
    XYZ centroid,
    List<ElementId> centroidInstances,
    FamilySymbol centroidFamily)
  {
    List<ElementId> source = new List<ElementId>();
    foreach (ElementId centroidInstance in centroidInstances)
      source.Add(centroidInstance);
    List<ElementId> list = source.Distinct<ElementId>().ToList<ElementId>();
    ElementId elementId1 = ElementId.InvalidElementId;
    if (list.Count >= 1)
    {
      elementId1 = list.First<ElementId>();
      list.Remove(list.First<ElementId>());
    }
    foreach (ElementId elementId2 in list)
      revitDoc.Delete(elementId2);
    if (elementId1 != ElementId.InvalidElementId)
    {
      Element element = revitDoc.GetElement(elementId1);
      if (element.Location.Move(centroid - (element.Location as LocationPoint).Point))
        return true;
      revitDoc.Delete(elementId1);
    }
    revitDoc.Create.NewFamilyInstance(centroid, centroidFamily, StructuralType.NonStructural);
    return true;
  }

  private bool IsCentroidLoaded(Document revitDoc)
  {
    bool flag = false;
    ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel
    });
    foreach (Element element in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilySymbol)))
    {
      if (element.Name.Equals("CENTROID"))
      {
        flag = true;
        break;
      }
    }
    return flag;
  }

  private bool IsCentroidProjectLoaded(Document revitDoc)
  {
    bool flag = false;
    ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel
    });
    foreach (Element element in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilySymbol)))
    {
      if (element.Name.Equals("CENTROID_PROJECT"))
      {
        flag = true;
        break;
      }
    }
    return flag;
  }

  private static string DisplayCentroid(Document revitDoc, XYZ centroid)
  {
    string str1 = "";
    string str2 = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId().ToString();
    if (str2 != null)
    {
      switch (str2.Length)
      {
        case 10:
          if (str2 == "DUT_METERS")
          {
            str1 = "m";
            break;
          }
          break;
        case 14:
          if (str2 == "DUT_DECIMETERS")
          {
            str1 = "dm";
            break;
          }
          break;
        case 15:
          switch (str2[4])
          {
            case 'C':
              if (str2 == "DUT_CENTIMETERS")
              {
                str1 = "cm";
                break;
              }
              break;
            case 'M':
              if (str2 == "DUT_MILLIMETERS")
              {
                str1 = "mm";
                break;
              }
              break;
          }
          break;
        case 16 /*0x10*/:
          if (str2 == "DUT_DECIMAL_FEET")
          {
            str1 = "'";
            break;
          }
          break;
        case 18:
          if (str2 == "DUT_DECIMAL_INCHES")
          {
            str1 = "\"";
            break;
          }
          break;
        case 21:
          if (str2 == "DUT_FRACTIONAL_INCHES")
          {
            str1 = "";
            break;
          }
          break;
        case 22:
          if (str2 == "DUT_METERS_CENTIMETERS")
          {
            str1 = "m";
            break;
          }
          break;
        case 26:
          if (str2 == "DUT_FEET_FRACTIONAL_INCHES")
          {
            str1 = "";
            break;
          }
          break;
      }
    }
    string str3 = UnitFormatUtils.Format(revitDoc.GetUnits(), SpecTypeId.Length, centroid.X, false);
    string str4 = UnitFormatUtils.Format(revitDoc.GetUnits(), SpecTypeId.Length, centroid.Y, false);
    string str5 = UnitFormatUtils.Format(revitDoc.GetUnits(), SpecTypeId.Length, centroid.Z, false);
    return $"Centroid is at the coordinates: \nX: {str3} {str1}\nY: {str4} {str1}\nZ: {str5} {str1}";
  }
}
