// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.BphUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.HostingUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SchedulingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class BphUpdater : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    DateTime now1 = DateTime.Now;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    BomHostingV2.bphCanceled = false;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    BomHostingV2.bphCheckWorkSharing = document.IsWorkshared;
    bool flag1 = document.IsWorkshared;
    if (document.IsWorkshared)
    {
      IList<Workset> worksets = new FilteredWorksetCollector(document).WherePasses((WorksetFilter) new WorksetKindFilter(WorksetKind.UserWorkset)).ToWorksets();
      bool flag2 = true;
      foreach (Workset workset in (IEnumerable<Workset>) worksets)
      {
        if (!workset.IsEditable)
        {
          flag2 = false;
          break;
        }
      }
      if (flag2)
      {
        flag1 = false;
        BomHostingV2.bphCheckWorkSharing = false;
      }
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Update BPH Parameters"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        TaskDialog taskDialog1 = new TaskDialog("BPH Parameter Value Updater");
        taskDialog1.Id = "ID_BPH_Updater";
        taskDialog1.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
        taskDialog1.Title = "BPH Updater";
        taskDialog1.TitleAutoPrefix = true;
        taskDialog1.AllowCancellation = true;
        taskDialog1.MainInstruction = "BOM_PRODUCT_HOST Parameter Value Updater";
        taskDialog1.MainContent = "Select the scope of the BPH parameter update.";
        taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Update BPH values for elements in the Whole Model.");
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Update BPH values for elements in the Active View.");
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Update BPH values for Selected Elements.");
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1004, "RESET all BPH values to empty strings in the whole model.");
        taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog1.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog1.Show();
        if (taskDialogResult == 1001)
        {
          int num = (int) transactionGroup.Start();
          ICollection<ElementId> list = (ICollection<ElementId>) StructuralFraming.RefineNestedFamilies(document).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
          if (list.Count == 0)
          {
            new TaskDialog("Error")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = "There are no elements in the Whole Model that can be updated"
            }.Show();
            return (Result) 1;
          }
          DateTime now2 = DateTime.Now;
          if (document.IsWorkshared & flag1)
          {
            TaskDialog taskDialog2 = new TaskDialog("BOM Product Hosting - Work Sharing");
            taskDialog2.MainInstruction = "Work Sharing Ownership Check Disabled";
            taskDialog2.MainContent = "To provide optimal performance, BOM Product Hosting will not check the ownership of elements it edits when updating the whole model. Please ensure no other users are currently working in the model and have relinquished all elements. It is suggested to make worksets editable to ensure that there are no issues. If you still wish for the tool to check ownership status, check the box below. However this may result in degraded performance.";
            taskDialog2.ExtraCheckBoxText = "Check Edited Elements' Ownership Status";
            taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue");
            taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel");
            if (taskDialog2.Show() != 1001)
              return (Result) 1;
            BomHostingV2.bphCheckWorkSharing = taskDialog2.WasExtraCheckBoxChecked();
          }
          QA.LogLine($"There are {list.Count.ToString()} structural framing elements to update in the whole model.");
          DateTime now3 = DateTime.Now;
          QA.LogLine("Staring the improved BOM hosting test for Whole Model at: " + now3.ToString());
          BomHostingV2.UpdateHosting(list);
          if (BomHostingV2.bphCanceled)
            return (Result) 1;
          QA.LogLine("The improved BOM hosting test for Whole Model takes: " + (DateTime.Now - now3).ToString());
          QA.LogLine("**********************************************************");
          if (transactionGroup.Assimilate() != TransactionStatus.Committed)
            return (Result) 1;
          TimeSpan timeSpan = DateTime.Now - now1;
          new TaskDialog("BPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The BPH Parameter values have been updated in the Whole Model"
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1002)
        {
          int num = (int) transactionGroup.Start();
          FilteredElementCollector elementsInView = StructuralFraming.GetElementsInView();
          List<ElementId> hostElementIdsToUpdate = new List<ElementId>();
          foreach (Element element in (IEnumerable<Element>) elementsInView.ToElements())
            hostElementIdsToUpdate.Add(AssemblyInstances.GetFlatElement(document, element as FamilyInstance).Id);
          if (hostElementIdsToUpdate.Count.Equals(0))
          {
            new TaskDialog("Information")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = "There are no elements in the Active View that can be updated"
            }.Show();
            return (Result) 1;
          }
          DateTime now4 = DateTime.Now;
          QA.LogLine("Staring the improved BOM hosting test for Active View at: " + now4.ToString());
          BomHostingV2.UpdateHosting((ICollection<ElementId>) hostElementIdsToUpdate);
          if (BomHostingV2.bphCanceled)
            return (Result) 1;
          QA.LogLine("The improved BOM hosting test for Active View takes: " + (DateTime.Now - now4).ToString());
          QA.LogLine("**********************************************************");
          if (transactionGroup.Assimilate() != TransactionStatus.Committed)
            return (Result) 1;
          TimeSpan timeSpan = DateTime.Now - now1;
          new TaskDialog("BPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The BPH Parameter values have been updated in the Active View"
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1003)
        {
          ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
          ISelectionFilter selFilter = (ISelectionFilter) new OnlyStructuralFraming();
          if (elementIds.Count == 0)
            elementIds = References.PickNewReferences(activeUiDocument, selFilter, "Select elements to be updated with the appropriate BOM_PRODUCT_HOST parameter value.", elementIds);
          if (elementIds.Count == 0)
            return (Result) 1;
          int num = (int) transactionGroup.Start();
          FilteredElementCollector selectedElemList = new FilteredElementCollector(document, elementIds).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance));
          List<ElementId> hostElementIdsToUpdate = new List<ElementId>();
          foreach (Element element in (IEnumerable<Element>) selectedElemList.ToElements())
            hostElementIdsToUpdate.Add(AssemblyInstances.GetFlatElement(document, element as FamilyInstance).Id);
          StructuralFraming.RefineNestedFamilies((IEnumerable<Element>) selectedElemList, document);
          if (hostElementIdsToUpdate.Count == 0)
          {
            new TaskDialog("Information")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = "There are no elements in the selection group that can be updated. Please select one or more structural framing elements and try again."
            }.Show();
            return (Result) 1;
          }
          DateTime now5 = DateTime.Now;
          QA.LogLine("Starting the improved BOM hosting test for Selected Elements at: " + now5.ToString());
          BomHostingV2.UpdateHosting((ICollection<ElementId>) hostElementIdsToUpdate);
          if (BomHostingV2.bphCanceled)
            return (Result) 1;
          QA.LogLine("The improved BOM hosting test for Selected Elements takes: " + (DateTime.Now - now5).ToString());
          QA.LogLine("*********************************************************************************");
          if (transactionGroup.Assimilate() != TransactionStatus.Committed)
            return (Result) 1;
          new TaskDialog("BPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The BPH Parameter values have been updated in the Selected Element"
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1004)
        {
          if (new TaskDialog("Reset BOM_PRODUCT_HOST Parameters.")
          {
            MainInstruction = "Are you sure you want to reset BOM_PRODUCT_HOST Parameter values for all Elements in the model?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            DefaultButton = ((TaskDialogResult) 7)
          }.Show() == 6)
          {
            if (document.IsWorkshared & flag1)
            {
              TaskDialog taskDialog3 = new TaskDialog("BOM Product Hosting - Work Sharing");
              taskDialog3.MainInstruction = "Work Sharing Ownership Check Disabled";
              taskDialog3.MainContent = "To provide optimal performance, BOM Product Hosting will not check the ownership of elements it is reseting paraemeter values in the model. Please ensure no other users are currently working in the model and have relinquished all elements. It is suggested to make worksets editable to ensure that there are no issues. If you still wish for the tool to check ownership status, check the box below. However this may result in degraded performance.";
              taskDialog3.ExtraCheckBoxText = "Check Edited Elements' Ownership Status";
              taskDialog3.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue");
              taskDialog3.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel");
              if (taskDialog3.Show() != 1001)
                return (Result) 1;
              taskDialog3.WasExtraCheckBoxChecked();
            }
            int num1 = (int) transactionGroup.Start();
            using (Transaction transaction = new Transaction(document, "Reset BPH Parameters"))
            {
              int num2 = (int) transaction.Start();
              Reset.BOMProductHost(document);
              int num3 = (int) transaction.Commit();
            }
            if (transactionGroup.Assimilate() != TransactionStatus.Committed)
              return (Result) 1;
            new TaskDialog("BPH Parameter Updater")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "Success.  BOM_PRODUCT_HOST parameter values have been reset"
            }.Show();
            TimeSpan timeSpan = DateTime.Now - now1;
            return (Result) 0;
          }
        }
      }
      catch (Exception ex)
      {
        if (transactionGroup.HasStarted())
        {
          int num = (int) transactionGroup.RollBack();
        }
        message = "BPH Parameter Updater error. \n" + ex?.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    return (Result) 1;
  }

  private bool checkElementOwnershipCondition(ICollection<ElementId> listToTest, Document doc)
  {
    ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds3 = WorksharingUtils.CheckoutElements(doc, listToTest);
    foreach (ElementId elementId in (IEnumerable<ElementId>) listToTest)
    {
      if (!elementIds3.Contains(elementId))
      {
        elementIds1.Add(elementId);
      }
      else
      {
        switch (WorksharingUtils.GetModelUpdatesStatus(doc, elementId))
        {
          case ModelUpdatesStatus.DeletedInCentral:
          case ModelUpdatesStatus.UpdatedInCentral:
            elementIds2.Add(elementId);
            continue;
          default:
            if (WorksharingUtils.GetCheckoutStatus(doc, elementId) != CheckoutStatus.OwnedByCurrentUser)
            {
              elementIds1.Add(elementId);
              continue;
            }
            continue;
        }
      }
    }
    if (elementIds1.Count != 0)
    {
      TaskDialog taskDialog = new TaskDialog("BPH Parameter Updater");
      taskDialog.MainInstruction = "The elements necessary for BOM Product Hosting are not currently editable. Please coordinate with project members to allow for ownership of the elements.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Cancel the process.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Continue Anyway.");
      if (taskDialog.Show() == 1001)
        return false;
      new TaskDialog("BPH Parameter Updater")
      {
        MainInstruction = "Please be aware that we will skip the elements that are not editable."
      }.Show();
      return true;
    }
    if (elementIds2.Count == 0)
      return true;
    TaskDialog taskDialog1 = new TaskDialog("BPH Parameter Updater");
    taskDialog1.MainInstruction = $"There are {elementIds2.Count.ToString()} Elements that are being updated in the central model! Process will be canceled. Please try reloading the latest version.";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Cancel the process and reload the latest version.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Continue Anyway.");
    if (taskDialog1.Show() == 1001)
      return false;
    new TaskDialog("BPH Parameter Updater")
    {
      MainInstruction = "Please be aware that we will skip the elements that are currently in the central model."
    }.Show();
    return true;
  }
}
