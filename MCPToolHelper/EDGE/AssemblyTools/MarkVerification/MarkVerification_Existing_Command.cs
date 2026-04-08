// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.MarkVerification_Existing_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.QA;
using EDGE.AssemblyTools.MarkVerification.ResultsPresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class MarkVerification_Existing_Command : IExternalCommand
{
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
        MainInstruction = "Mark Verification Existing Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Mark Verification Existing must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    try
    {
      ICollection<ElementId> elementIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
      IEnumerable<Element> source1 = (IEnumerable<Element>) null;
      IEnumerable<AssemblyInstance> source2 = (IEnumerable<AssemblyInstance>) null;
      bool flag = false;
      ElementFilter hwdFilter = Components.GetHWDFilter(revitDoc);
      if (elementIds.Any<ElementId>())
      {
        if (hwdFilter != null)
        {
          source1 = new FilteredElementCollector(revitDoc, elementIds).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (s => !(s.AssemblyInstanceId != (ElementId) null) || !(s.AssemblyInstanceId != ElementId.InvalidElementId) || !(revitDoc.GetElement(s.AssemblyInstanceId) is AssemblyInstance element) || !hwdFilter.PassesFilter((Element) element)));
          source2 = new FilteredElementCollector(revitDoc, elementIds).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.GetStructuralFramingElement() != null && !hwdFilter.PassesFilter((Element) e)));
        }
        else
        {
          source1 = (IEnumerable<Element>) new FilteredElementCollector(revitDoc, elementIds).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance));
          source2 = new FilteredElementCollector(revitDoc, elementIds).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.GetStructuralFramingElement() != null));
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
      IEnumerable<FamilyInstance> familyInstances1 = (IEnumerable<FamilyInstance>) new List<FamilyInstance>();
      Guid controlMarkGuid = Guid.Empty;
      IEnumerable<FamilyInstance> source3;
      if (flag)
      {
        HashSet<string> selectedMarkStrings = ((IEnumerable<Element>) new List<FamilyInstance>().Concat<FamilyInstance>(source1.Cast<FamilyInstance>().Select<FamilyInstance, FamilyInstance>((Func<FamilyInstance, FamilyInstance>) (s => AssemblyInstances.GetFlatElement(revitDoc, s)))).Concat<FamilyInstance>(source2.Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (s => s.GetStructuralFramingElement())).Cast<FamilyInstance>())).Select<Element, string>((Func<Element, string>) (s =>
        {
          Parameter parameter = (Parameter) null;
          string str = "";
          if (controlMarkGuid != Guid.Empty)
            parameter = s.get_Parameter(controlMarkGuid);
          if (parameter == null)
            parameter = Utils.ElementUtils.Parameters.LookupParameter(s, "CONTROL_MARK");
          if (parameter != null)
          {
            if (parameter.HasValue)
              str = parameter.AsString();
            if (controlMarkGuid == Guid.Empty)
              controlMarkGuid = parameter.GUID;
          }
          return str;
        })).Distinct<string>().ToHashSet<string>();
        IEnumerable<FamilyInstance> familyInstances2 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (s => selectedMarkStrings.Contains(Utils.ElementUtils.Parameters.GetParameterAsString(s, "CONTROL_MARK")))).Cast<FamilyInstance>();
        List<ElementId> elementIdList = new List<ElementId>();
        List<FamilyInstance> familyInstanceList = new List<FamilyInstance>();
        foreach (Element elem in familyInstances2)
        {
          if (elem.GetTopLevelElement() is FamilyInstance topLevelElement)
          {
            FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, topLevelElement);
            if (!elementIdList.Contains(flatElement.Id))
            {
              familyInstanceList.Add(flatElement);
              elementIdList.Add(flatElement.Id);
            }
          }
        }
        source3 = (IEnumerable<FamilyInstance>) familyInstanceList;
      }
      else
      {
        FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance));
        List<ElementId> elementIdList = new List<ElementId>();
        List<FamilyInstance> familyInstanceList = new List<FamilyInstance>();
        foreach (Element element in elementCollector)
        {
          if (element is FamilyInstance elem && elem.GetTopLevelElement() is FamilyInstance topLevelElement)
          {
            FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, topLevelElement);
            if (!elementIdList.Contains(flatElement.Id))
            {
              familyInstanceList.Add(flatElement);
              elementIdList.Add(flatElement.Id);
            }
          }
        }
        source3 = (IEnumerable<FamilyInstance>) familyInstanceList;
      }
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      CompareEngine_Selection_Window engineSelectionWindow = new CompareEngine_Selection_Window(mainWindowHandle, "existing");
      engineSelectionWindow.ShowDialog();
      if (!engineSelectionWindow.isContinue)
        return (Result) 1;
      MarkQA markQa1 = new MarkQA();
      markQa1.bFamilyTypeTest = true;
      markQa1.bCompareAllParameters = engineSelectionWindow.familyParametercheckBox.IsChecked.Value;
      markQa1.bCompareMaterialVolumes = engineSelectionWindow.mainMaterialVolumncheckBox.IsChecked.Value;
      markQa1.bCompareAddons_VolMatCountFamily = engineSelectionWindow.addonFamilycheckBox.IsChecked.Value;
      markQa1.bCompareAddons_LocationAndOrientation = engineSelectionWindow.memberGeometry_AddonLocation_checkBox.IsChecked.Value;
      MarkQA markQa2 = markQa1;
      bool? isChecked = engineSelectionWindow.plateFamilycheckBox.IsChecked;
      int num1 = isChecked.Value ? 1 : 0;
      markQa2.bComparePlates_NamesAndCounts = num1 != 0;
      MarkQA markQa3 = markQa1;
      isChecked = engineSelectionWindow.memberGeometry_EmbededLocation_checkBox.IsChecked;
      int num2 = isChecked.Value ? 1 : 0;
      markQa3.bComparePlate_LocationAndOrientation = num2 != 0;
      MarkQA markQa4 = markQa1;
      isChecked = engineSelectionWindow.memberGeometry_Solid_checkBox.IsChecked;
      int num3 = isChecked.Value ? 1 : 0;
      markQa4.bCompareSolidsFaces = num3 != 0;
      MarkQA markQa5 = markQa1;
      isChecked = engineSelectionWindow.traditionalBox.IsChecked;
      int num4 = isChecked.Value ? 1 : 0;
      markQa5.traditionalApproach = num4 != 0;
      mkExistingWarning mkExistingWarning = new mkExistingWarning(mainWindowHandle);
      mkExistingWarning.ShowDialog();
      if (!mkExistingWarning.ifContinue)
        return (Result) 1;
      IEnumerable<IGrouping<string, FamilyInstance>> groupedSFElementsToVerify = source3.GroupBy<FamilyInstance, string>((Func<FamilyInstance, string>) (s =>
      {
        Parameter parameter = (Parameter) null;
        string str = "";
        if (controlMarkGuid != Guid.Empty)
          parameter = s.get_Parameter(controlMarkGuid);
        if (parameter == null)
          parameter = Utils.ElementUtils.Parameters.LookupParameter((Element) s, "CONTROL_MARK");
        if (parameter != null)
        {
          if (parameter.HasValue)
            str = parameter.AsString();
          if (controlMarkGuid == Guid.Empty)
            controlMarkGuid = parameter.GUID;
        }
        return str;
      }));
      List<MarkResult> results = MKComparisonEngine.RunMarkVerifictionOnExisting(activeUiDocument, groupedSFElementsToVerify, markQa1);
      if (results == null)
        return (Result) 1;
      results.Sort((Comparison<MarkResult>) ((p, q) => p.CompareTo((object) q)));
      if (App.MarkVerificationExistingWindow != null)
        App.MarkVerificationExistingWindow.Close();
      TimeSpan timeSpan = DateTime.Now - now2;
      App.MarkVerificationExistingWindow = new MKVerificationResults_Existing(results, application, revitDoc, mainWindowHandle, markQa1, new ExternalEvent[1]
      {
        ExternalEvent.Create((IExternalEventHandler) new HighLightElements())
      });
      App.MarkVerificationExistingWindow.Show();
      return (Result) 0;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("MVCNULL"))
        TaskDialog.Show("Mark Verification Error", "One or more families did not have a MEMBER_VOLUME_CAST or MEMBER_WEIGHT_CAST parameter. Please run Project Shared Parameters and try again.");
      else
        TaskDialog.Show("Mark Verification Error", "EDGE Encountered an unexpected problem in Mark Verification Initial: " + ex.Message);
      return (Result) -1;
    }
  }
}
