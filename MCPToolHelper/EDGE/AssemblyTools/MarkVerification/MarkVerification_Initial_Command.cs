// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.MarkVerification_Initial_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using EDGE.AssemblyTools.MarkVerification.QA;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Utils;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class MarkVerification_Initial_Command : IExternalCommand
{
  private bool bUserAllowedToModifyGeometry = true;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    DateTime now1 = DateTime.Now;
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    UIApplication application = commandData.Application;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Mark Verification Initial Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Mark Verification Initial must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    try
    {
      if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.MVInitial))
        return (Result) 1;
      ICollection<ElementId> elementIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
      List<Element> source1 = (List<Element>) null;
      List<AssemblyInstance> source2 = (List<AssemblyInstance>) null;
      bool flag = false;
      ElementFilter hwdFilter = Components.GetHWDFilter(revitDoc);
      if (elementIds.Any<ElementId>())
      {
        if (hwdFilter != null)
        {
          source1 = new FilteredElementCollector(revitDoc, elementIds).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (s => !(s.AssemblyInstanceId != (ElementId) null) || !(s.AssemblyInstanceId != ElementId.InvalidElementId) || !(revitDoc.GetElement(s.AssemblyInstanceId) is AssemblyInstance element) || hwdFilter.PassesFilter((Element) element))).ToList<Element>();
          source2 = new FilteredElementCollector(revitDoc, elementIds).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.GetStructuralFramingElement() != null && hwdFilter.PassesFilter((Element) e))).ToList<AssemblyInstance>();
        }
        else
        {
          source1 = new FilteredElementCollector(revitDoc, elementIds).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).ToList<Element>();
          source2 = new FilteredElementCollector(revitDoc, elementIds).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.GetStructuralFramingElement() != null)).ToList<AssemblyInstance>();
        }
        if (source1.Any<Element>() || ((IEnumerable<Element>) source2).Any<Element>())
        {
          TaskDialog taskDialog = new TaskDialog("EDGE: Mark Verification");
          taskDialog.MainInstruction = "Choose verification method";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "All elements in model by mark number");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Only for selected structural framing element");
          TaskDialogResult taskDialogResult = taskDialog.Show();
          if (taskDialogResult == 2)
            return (Result) 1;
          if (taskDialogResult == 1002)
            flag = true;
        }
      }
      DateTime now2 = DateTime.Now;
      List<FamilyInstance> source3 = new List<FamilyInstance>();
      List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
      List<FamilyInstance> familyInstanceList2;
      if (flag)
      {
        List<ElementId> elementIdList = new List<ElementId>();
        List<FamilyInstance> collection1 = new List<FamilyInstance>();
        foreach (Element element in source1)
        {
          if (element is FamilyInstance elem)
          {
            FamilyInstance topLevelElement = elem.GetTopLevelElement() as FamilyInstance;
            FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, topLevelElement);
            if (!elementIdList.Contains(flatElement.Id))
            {
              elementIdList.Add(flatElement.Id);
              collection1.Add(flatElement);
            }
          }
        }
        source3.AddRange((IEnumerable<FamilyInstance>) collection1);
        IEnumerable<FamilyInstance> collection2 = source2.Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (s => s.GetStructuralFramingElement())).Cast<FamilyInstance>();
        source3.AddRange(collection2);
        familyInstanceList2 = new List<FamilyInstance>(source3.Distinct<FamilyInstance>());
      }
      else
      {
        FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance));
        elementCollector.ToElementIds();
        List<ElementId> elementIdList = new List<ElementId>();
        familyInstanceList2 = new List<FamilyInstance>();
        foreach (Element elem in elementCollector)
        {
          FamilyInstance topLevelElement = (elem as FamilyInstance).GetTopLevelElement() as FamilyInstance;
          FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, topLevelElement);
          if (!elementIdList.Contains(flatElement.Id))
          {
            elementIdList.Add(flatElement.Id);
            familyInstanceList2.Add(flatElement);
          }
        }
      }
      HashSet<string> existingControlMarks = new HashSet<string>();
      if (flag)
      {
        foreach (string str in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (s => !s.IsWarpableProduct() || s.IsFlatFamily())).Select<FamilyInstance, string>((Func<FamilyInstance, string>) (s => s.GetControlMark())).Distinct<string>())
          existingControlMarks.Add(str);
      }
      List<ElementId> idsToCheck = new List<ElementId>();
      foreach (FamilyInstance familyInstance in familyInstanceList2)
        idsToCheck.Add(familyInstance.Id);
      Dictionary<string, MarkGroupResultData> dictionary1 = new Dictionary<string, MarkGroupResultData>();
      MarkQA MarkQAForInitial = new MarkQA();
      Dictionary<string, MarkGroupResultData> dictionary2;
      if (revitDoc.IsWorkshared)
      {
        if (!CheckElementsOwnership.CheckOwnership("Mark Verification Initial", idsToCheck, revitDoc, activeUiDocument, out List<ElementId> _))
          return (Result) 1;
        CompareEngine_Selection_Window engineSelectionWindow = new CompareEngine_Selection_Window(Process.GetCurrentProcess().MainWindowHandle, "initial");
        engineSelectionWindow.ShowDialog();
        if (!engineSelectionWindow.isContinue)
          return (Result) 1;
        MarkQAForInitial.bFamilyTypeTest = true;
        MarkQAForInitial.bCompareAllParameters = engineSelectionWindow.familyParametercheckBox.IsChecked.Value;
        MarkQA markQa1 = MarkQAForInitial;
        bool? isChecked = engineSelectionWindow.mainMaterialVolumncheckBox.IsChecked;
        int num1 = isChecked.Value ? 1 : 0;
        markQa1.bCompareMaterialVolumes = num1 != 0;
        MarkQA markQa2 = MarkQAForInitial;
        isChecked = engineSelectionWindow.addonFamilycheckBox.IsChecked;
        int num2 = isChecked.Value ? 1 : 0;
        markQa2.bCompareAddons_VolMatCountFamily = num2 != 0;
        MarkQA markQa3 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_AddonLocation_checkBox.IsChecked;
        int num3 = isChecked.Value ? 1 : 0;
        markQa3.bCompareAddons_LocationAndOrientation = num3 != 0;
        MarkQA markQa4 = MarkQAForInitial;
        isChecked = engineSelectionWindow.plateFamilycheckBox.IsChecked;
        int num4 = isChecked.Value ? 1 : 0;
        markQa4.bComparePlates_NamesAndCounts = num4 != 0;
        MarkQA markQa5 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_EmbededLocation_checkBox.IsChecked;
        int num5 = isChecked.Value ? 1 : 0;
        markQa5.bComparePlate_LocationAndOrientation = num5 != 0;
        MarkQA markQa6 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_Solid_checkBox.IsChecked;
        int num6 = isChecked.Value ? 1 : 0;
        markQa6.bCompareSolidsFaces = num6 != 0;
        dictionary2 = MKComparisonEngine.RunInitialComparison(activeUiDocument, (IEnumerable<FamilyInstance>) familyInstanceList2, existingControlMarks, MarkQAForInitial);
      }
      else
      {
        CompareEngine_Selection_Window engineSelectionWindow = new CompareEngine_Selection_Window(Process.GetCurrentProcess().MainWindowHandle, "initial");
        engineSelectionWindow.ShowDialog();
        if (!engineSelectionWindow.isContinue)
          return (Result) 1;
        MarkQAForInitial.bFamilyTypeTest = true;
        MarkQAForInitial.bCompareAllParameters = engineSelectionWindow.familyParametercheckBox.IsChecked.Value;
        MarkQA markQa7 = MarkQAForInitial;
        bool? isChecked = engineSelectionWindow.mainMaterialVolumncheckBox.IsChecked;
        int num7 = isChecked.Value ? 1 : 0;
        markQa7.bCompareMaterialVolumes = num7 != 0;
        MarkQA markQa8 = MarkQAForInitial;
        isChecked = engineSelectionWindow.addonFamilycheckBox.IsChecked;
        int num8 = isChecked.Value ? 1 : 0;
        markQa8.bCompareAddons_VolMatCountFamily = num8 != 0;
        MarkQA markQa9 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_AddonLocation_checkBox.IsChecked;
        int num9 = isChecked.Value ? 1 : 0;
        markQa9.bCompareAddons_LocationAndOrientation = num9 != 0;
        MarkQA markQa10 = MarkQAForInitial;
        isChecked = engineSelectionWindow.plateFamilycheckBox.IsChecked;
        int num10 = isChecked.Value ? 1 : 0;
        markQa10.bComparePlates_NamesAndCounts = num10 != 0;
        MarkQA markQa11 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_EmbededLocation_checkBox.IsChecked;
        int num11 = isChecked.Value ? 1 : 0;
        markQa11.bComparePlate_LocationAndOrientation = num11 != 0;
        MarkQA markQa12 = MarkQAForInitial;
        isChecked = engineSelectionWindow.memberGeometry_Solid_checkBox.IsChecked;
        int num12 = isChecked.Value ? 1 : 0;
        markQa12.bCompareSolidsFaces = num12 != 0;
        DateTime now3 = DateTime.Now;
        dictionary2 = MKComparisonEngine.RunInitialComparison(activeUiDocument, (IEnumerable<FamilyInstance>) familyInstanceList2, existingControlMarks, MarkQAForInitial);
        TimeSpan timeSpan = DateTime.Now - now3;
      }
      if (dictionary2 == null)
        return (Result) 1;
      DateTime now4 = DateTime.Now;
      if (this.bUserAllowedToModifyGeometry)
      {
        mkInitialWarning mkInitialWarning = new mkInitialWarning(Process.GetCurrentProcess().MainWindowHandle);
        mkInitialWarning.ShowDialog();
        Guid guid = Guid.Empty;
        if (!mkInitialWarning.isContinue)
          return (Result) 1;
        using (Transaction transaction = new Transaction(revitDoc, "set mark number"))
        {
          int num = (int) transaction.Start();
          try
          {
            foreach (string key in dictionary2.Keys)
            {
              foreach (FamilyInstance groupMember in dictionary2[key].GroupMembers)
              {
                if (groupMember.SuperComponent != null)
                {
                  Parameter parameter = (Parameter) null;
                  if (guid != Guid.Empty)
                    parameter = groupMember.SuperComponent.get_Parameter(guid);
                  if (parameter == null)
                    parameter = groupMember.SuperComponent.LookupParameter("CONTROL_MARK");
                  if (parameter != null && !parameter.IsReadOnly)
                  {
                    parameter.Set(key);
                    if (guid == Guid.Empty)
                      guid = parameter.GUID;
                  }
                }
                else
                {
                  Parameter parameter = (Parameter) null;
                  if (guid != Guid.Empty)
                    parameter = groupMember.get_Parameter(guid);
                  if (parameter == null)
                    parameter = groupMember.LookupParameter("CONTROL_MARK");
                  if (parameter != null && !parameter.IsReadOnly)
                  {
                    parameter.Set(key);
                    if (guid == Guid.Empty)
                      guid = parameter.GUID;
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            TaskDialog.Show("Warning", ex.ToString());
          }
          if (transaction.Commit() != TransactionStatus.Committed)
          {
            QAUtils.LogError("Mark Verification Initial", "Unable to commit transaction for writing new control marks to elements");
            message = "Failed to commit transaction to update control marks";
            return (Result) -1;
          }
        }
      }
      TimeSpan timeSpan1 = DateTime.Now - now4;
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      SortedDictionary<string, MarkGroupResultData> dictionary3 = new SortedDictionary<string, MarkGroupResultData>((IDictionary<string, MarkGroupResultData>) dictionary2);
      if (App.MarkVerificationInitialWindow != null)
        App.MarkVerificationInitialWindow.Close();
      App.MarkVerificationInitialWindow = new MKVerificationResult_Initial(new ExternalEvent[1]
      {
        ExternalEvent.Create((IExternalEventHandler) new SetControlmarkEvent())
      }, dictionary3, application, mainWindowHandle, this.bUserAllowedToModifyGeometry, revitDoc);
      TimeSpan timeSpan2 = DateTime.Now - now2;
      App.MarkVerificationInitialWindow.Show();
      this.AlertUserIfSFNamesNoLongerMatchTheirAssemblyNames(revitDoc, familyInstanceList2.ToList<FamilyInstance>());
      return (Result) 0;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("MVCNULL"))
        TaskDialog.Show("Mark Verification Error", "One or more families did not have a MEMBER_VOLUME_CAST or MEMBER_WEIGHT_CAST parameter. Please run Project Shared Parameters and try again.");
      else
        TaskDialog.Show("Mark Verification Error", "EDGE Encountered an unexpected problem in Mark Verification Initial: " + ex.Message);
      return (Result) 1;
    }
  }

  private bool StructuralFramingToUpdateHasAssociatedAssembly(List<FamilyInstance> list)
  {
    return list.Where<FamilyInstance>((Func<FamilyInstance, bool>) (s => s.AssemblyInstanceId != ElementId.InvalidElementId)).Any<FamilyInstance>();
  }

  private void AlertUserIfSFNamesNoLongerMatchTheirAssemblyNames(
    Document revitDoc,
    List<FamilyInstance> processedStructuralFraming)
  {
    bool flag = false;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (FamilyInstance elem in processedStructuralFraming)
    {
      if (!(elem.AssemblyInstanceId == ElementId.InvalidElementId))
      {
        AssemblyInstance element = revitDoc.GetElement(elem.AssemblyInstanceId) as AssemblyInstance;
        string controlMark = elem.GetControlMark();
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) element, "ASSEMBLY_MARK_NUMBER");
        string str = parameterAsString;
        if (!controlMark.Equals(str))
        {
          flag = true;
          stringBuilder.AppendLine("Assembly: " + parameterAsString);
        }
      }
    }
    if (!flag)
      return;
    new TaskDialog("Warning")
    {
      MainInstruction = "Found One or More Assemblies with Mismatched Structural Framing Control Marks.",
      MainContent = "Running Mark Verification Initial will overwrite control marks of certain structural framing elements.  If those elements are already part of an assembly the mark of the assembly may now be out of date with the structural framing element.  To fix this, update the ASSEMBLY_MARK_NUMBER parameter for affected assemblies.  See Expanded Content for detailed information.",
      ExpandedContent = stringBuilder.ToString(),
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      AllowCancellation = false
    }.Show();
  }
}
