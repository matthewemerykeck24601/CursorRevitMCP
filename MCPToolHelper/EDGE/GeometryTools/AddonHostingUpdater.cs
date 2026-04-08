// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AddonHostingUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.HostingUtils;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class AddonHostingUpdater : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str1 = "Addon Hosting Updater";
    string str2 = "Success:  Add-ons have been updated with the appropriate parameter values";
    string str3 = "Error:  There was an error updating parameters for the elements.";
    HashSet<string> stringSet = new HashSet<string>();
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.AddonHosting))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Update Addon Parameters"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        TaskDialog taskDialog = new TaskDialog("Addon Hosting Updater");
        taskDialog.Id = "ID_AddonHosting_Updater";
        taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
        taskDialog.Title = "Addon Hosting Updater";
        taskDialog.TitleAutoPrefix = true;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "Addon Hosting Updater";
        taskDialog.MainContent = "Select the scope of the Addon hosting update.";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Update Hosting for Addons in the Whole Model.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Update Hosting for Addons in the Active View.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Update Hosting for Selected Addon Elements.");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          int num1 = (int) transaction.Start();
          HashSet<string> processedHostGuids;
          try
          {
            processedHostGuids = AddonHosting.ProcessAndUpdateHostedElements(document, false, (ICollection<ElementId>) null, out bool _);
            if (processedHostGuids == null)
              return (Result) 1;
          }
          catch (Exception ex)
          {
            if (ex.Message.Trim() != "NO MESSAGE")
              new TaskDialog("Addon Hosting")
              {
                MainContent = "Addon Hosting updater has encountered an error.",
                ExpandedContent = ex.StackTrace
              }.Show();
            return (Result) -1;
          }
          if (!this.checkForWeightParameters(document, processedHostGuids))
          {
            int num2 = (int) transaction.RollBack();
            return (Result) 1;
          }
          AddonHosting.TagFinishWallsWithHostGuid(document, processedHostGuids);
          document.Regenerate();
          AddonHosting.SumAndPublishVolumesAndWeights(document, processedHostGuids);
          AddonHosting.SumAndPublishArchSF(document, processedHostGuids);
          if (transaction.Commit() == TransactionStatus.Committed)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + " in the Whole Model")
            }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1002)
        {
          int num3 = (int) transaction.Start();
          HashSet<string> processedHostGuids;
          try
          {
            processedHostGuids = AddonHosting.ProcessAndUpdateHostedElements(document, true, (ICollection<ElementId>) null, out bool _);
            if (processedHostGuids == null)
              return (Result) 1;
          }
          catch (Exception ex)
          {
            if (ex.Message.Trim() != "NO MESSAGE")
              new TaskDialog("Addon Hosting")
              {
                MainContent = "Addon Hosting updater has encountered an error.",
                ExpandedContent = ex.StackTrace
              }.Show();
            return (Result) -1;
          }
          if (!this.checkForWeightParameters(document, processedHostGuids))
          {
            int num4 = (int) transaction.RollBack();
            return (Result) 1;
          }
          AddonHosting.TagFinishWallsWithHostGuid(document, processedHostGuids);
          document.Regenerate();
          AddonHosting.SumAndPublishVolumesAndWeights(document, processedHostGuids);
          AddonHosting.SumAndPublishArchSF(document, processedHostGuids);
          if (transaction.Commit() == TransactionStatus.Committed)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + " in the Active View")
            }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1003)
        {
          ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
          if (elementIds.Count == 0)
            elementIds = References.PickNewReferences(activeUiDocument, (ISelectionFilter) new OnlyAddons(), "Select Add-ons to be updated.");
          if (elementIds == null || elementIds.Count == 0)
            return (Result) 1;
          List<ElementId> selectedElementIds = new List<ElementId>();
          selectedElementIds.AddRange((IEnumerable<ElementId>) elementIds);
          if (!this.WarnUserIfSelectionDoesntContainAddons(document, selectedElementIds))
            return (Result) 1;
          int num5 = (int) transaction.Start();
          bool hardwareDetailBool = false;
          HashSet<string> processedHostGuids;
          try
          {
            processedHostGuids = AddonHosting.ProcessAndUpdateHostedElements(document, false, elementIds, out hardwareDetailBool);
            if (processedHostGuids == null)
              return (Result) 1;
          }
          catch (Exception ex)
          {
            return (Result) -1;
          }
          if (hardwareDetailBool)
            new TaskDialog("Add-On Hosting Updater")
            {
              MainContent = "One or more elements selected were not updated because they are hardware detail elements."
            }.Show();
          if (!this.checkForWeightParameters(document, processedHostGuids))
          {
            int num6 = (int) transaction.RollBack();
            return (Result) 1;
          }
          AddonHosting.TagFinishWallsWithHostGuid(document, processedHostGuids);
          document.Regenerate();
          AddonHosting.SumAndPublishVolumesAndWeights(document, processedHostGuids);
          AddonHosting.SumAndPublishArchSF(document, processedHostGuids);
          if (transaction.Commit() == TransactionStatus.Committed)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + ".")
            }.Show();
          return (Result) 0;
        }
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        if (ex.ToString().Contains("The parameter is read-only."))
        {
          new TaskDialog("Read-Only Parameter Issue")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "One or more families had read-only parameters and could not be updated. Please check relevant parameters and try again."
          }.Show();
          return (Result) -1;
        }
        if (ex is EdgeException)
        {
          message = ex.Message;
          return (Result) -1;
        }
        new TaskDialog("Addon Hosting")
        {
          MainContent = "Addon Hosting updater has encountered an error.",
          ExpandedContent = ex.StackTrace
        }.Show();
        message = str3 + ex?.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    return (Result) 1;
  }

  private bool WarnUserIfSelectionDoesntContainAddons(
    Document revitDoc,
    List<ElementId> selectedElementIds)
  {
    bool flag1 = false;
    int num = 0;
    foreach (ElementId selectedElementId in selectedElementIds)
    {
      Element element1 = revitDoc.GetElement(selectedElementId);
      switch (element1)
      {
        case Wall _:
          ++num;
          continue;
        case FamilyInstance _:
          FamilyInstance familyInstance1 = element1 as FamilyInstance;
          bool flag2 = false;
          if (!Oracle.IsRepeatedComponent(element1))
          {
            flag1 = true;
            continue;
          }
          if (Oracle.ElementIsInsulation((Element) familyInstance1))
          {
            flag1 = true;
            continue;
          }
          if (Oracle.IsAddonFamilyForAddonHostingUpdater(familyInstance1))
            flag2 = true;
          foreach (Element element2 in familyInstance1.GetAllSubcomponents().Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)).Where<Element>((Func<Element, bool>) (elem => elem.Name.Contains("CONNECTOR_COMPONENT"))))
          {
            Element element3 = revitDoc.GetElement(element2.GetTypeId());
            if (element3 != null)
            {
              Parameter parameter = element3.LookupParameter("CONNECTOR_COMPONENT");
              if (parameter != null && parameter.AsValueString().ToUpper().Equals("YES") && element2 is FamilyInstance familyInstance2 && familyInstance2.SuperComponent is FamilyInstance superComponent && Oracle.IsAddonFamily(superComponent))
              {
                flag2 = true;
                ++num;
                break;
              }
            }
          }
          if (!flag2)
          {
            flag1 = true;
            continue;
          }
          continue;
        default:
          flag1 = true;
          continue;
      }
    }
    if (flag1 && num == 0)
    {
      new TaskDialog("Error")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        CommonButtons = ((TaskDialogCommonButtons) 1),
        AllowCancellation = false,
        MainInstruction = "No AddOn Elements Encountered in Selection",
        MainContent = "EDGE^R detected only non-addon elements in the selection.  Please select add on components before running selected components again"
      }.Show();
      return false;
    }
    if (flag1)
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        CommonButtons = ((TaskDialogCommonButtons) 1),
        AllowCancellation = false,
        MainInstruction = "Non AddOn Elements Encountered in Selection",
        MainContent = "EDGE^R detected some non-addon elements in the selection.  Addon elements will be processed and non-addon elements will be ignored."
      }.Show();
    return true;
  }

  private bool checkForWeightParameters(Document revitDoc, HashSet<string> processedHostGuids)
  {
    List<Element> elementList1 = new List<Element>();
    List<Element> elementList2 = new List<Element>();
    foreach (string processedHostGuid in processedHostGuids)
    {
      Element element = revitDoc.GetElement(processedHostGuid);
      if (element != null)
      {
        Parameter parameter1 = Parameters.LookupParameter(element, EDGEParams.unitWeightParam.Str());
        Parameter parameter2 = Parameters.LookupParameter(element, EDGEParams.weightPerUnitParam.Str());
        if (parameter1 == null && parameter2 == null)
          elementList1.Add(element);
        if (parameter1 != null)
        {
          if (parameter1.AsDouble() <= 0.0)
            elementList2.Add(element);
        }
        else if (parameter2 != null && parameter2.AsDouble() <= 0.0)
          elementList2.Add(element);
      }
    }
    if (elementList1.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("Addon Hosting Updater Warning");
      taskDialog.MainContent = "Neither the UNIT_WEIGHT or WEIGHT_PER_UNIT parameters exist for one or more of the elements (shown below). This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
      taskDialog.ExpandedContent += "Elements:\n";
      List<string> stringList = new List<string>();
      foreach (Element elem in elementList1)
      {
        string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
        if (!stringList.Contains(str))
          stringList.Add(str);
      }
      stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
      foreach (string str in stringList)
        taskDialog.ExpandedContent += str;
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
      if (taskDialog.Show() != 1)
        return false;
    }
    if (elementList2.Count <= 0)
      return true;
    TaskDialog taskDialog1 = new TaskDialog("Addon Hosting Updater Warning");
    taskDialog1.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 9;
    taskDialog1.ExpandedContent += "Elements:\n";
    List<string> stringList1 = new List<string>();
    foreach (Element elem in elementList2)
    {
      string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
      if (!stringList1.Contains(str))
        stringList1.Add(str);
    }
    stringList1.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    foreach (string str in stringList1)
      taskDialog1.ExpandedContent += str;
    return taskDialog1.Show() == 1;
  }
}
