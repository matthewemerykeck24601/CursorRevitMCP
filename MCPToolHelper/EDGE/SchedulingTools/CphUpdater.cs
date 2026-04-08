// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.CphUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using Utils.AdminUtils;
using Utils.HostingUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SchedulingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CphUpdater : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Update CPH Parameters"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        TaskDialog taskDialog = new TaskDialog("CPH Parameter Value Updater");
        taskDialog.Id = "ID_CPH_Updater";
        taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
        taskDialog.Title = "CPH Updater";
        taskDialog.TitleAutoPrefix = true;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "CONSTRUCTION_PRODUCT_HOST Parameter Value Updater";
        taskDialog.MainContent = "Select the scope of the CONSTRUCTION parameter update.";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Update CPH values for elements in the Whole Model.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Update CPH values for elements in the Active View.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Update CPH values for Selected Elements.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1004, "RESET all CPH values to empty strings in the whole model.");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          int num = (int) transaction.Start();
          ConstructionProductHosting.WholeModel();
          if (transaction.Commit() != TransactionStatus.Committed)
            return (Result) 1;
          new TaskDialog("CPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The CPH Parameter values have been updated in the Whole Model"
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1002)
        {
          int num = (int) transaction.Start();
          ConstructionProductHosting.ActiveView();
          if (transaction.Commit() != TransactionStatus.Committed)
            return (Result) 1;
          new TaskDialog("CPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The CPH Parameter values have been updated in the Active View."
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1003)
        {
          Autodesk.Revit.UI.Selection.Selection selection = activeUiDocument.Selection;
          ICollection<ElementId> selectedIdList = activeUiDocument.Selection.GetElementIds();
          if (selectedIdList.Count == 0)
            selectedIdList = References.PickNewReferences("Select the elements to be updated.");
          if (selectedIdList.Count == 0)
            return (Result) 1;
          int num = (int) transaction.Start();
          ConstructionProductHosting.SelectedElements(selectedIdList);
          if (transaction.Commit() != TransactionStatus.Committed)
            return (Result) 1;
          new TaskDialog("CPH Parameter Updater")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Success.  The CPH Parameter values have been updated in the Selected Element."
          }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1004)
        {
          if (new TaskDialog("Reset CONSTRUCTION_PRODUCT_HOST Parameters.")
          {
            MainInstruction = "Are you sure you want to reset CONSTRUCTION_PRODUCT_HOST Parameter values for all Elements in the model?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            DefaultButton = ((TaskDialogResult) 7)
          }.Show() == 6)
          {
            int num = (int) transaction.Start();
            Reset.ConstructionProductHost(document);
            if (transaction.Commit() != TransactionStatus.Committed)
              return (Result) 1;
            new TaskDialog("CPH Parameter Updater")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "Success.  CONSTRUCTION_PRODUCT_HOST parameter values have been reset"
            }.Show();
            return (Result) 0;
          }
        }
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = "CPH Parameter Updater error. \n" + ex?.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    return (Result) 1;
  }
}
