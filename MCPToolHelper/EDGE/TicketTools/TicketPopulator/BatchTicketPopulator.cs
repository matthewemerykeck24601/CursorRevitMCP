// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.BatchTicketPopulator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class BatchTicketPopulator : IExternalCommand
{
  public static string strLastUsedScale = "";
  private bool bMultiTicket;
  private int intMultiSheetCurrentSheetNumber = 1;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIApplication application = commandData.Application;
    ActiveModel.GetInformation(activeUiDocument);
    string error;
    List<ElementId> selectedIds = BatchPopulatorCore.CullSelectionForTicketPopulator(document, activeUiDocument.Selection.GetElementIds().ToList<ElementId>(), out error);
    if (selectedIds.Count == 0)
    {
      TaskDialog.Show("Batch Ticket Populator", error);
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(document, "Batch Ticket Populator"))
    {
      if (transaction.Start() != TransactionStatus.Started)
      {
        TaskDialog.Show("Error", "Unable to start transaction group, please contact support");
        return (Result) 1;
      }
      App.DialogSwitches.SuspendModelLockingforOperation = true;
      ViewSheet AssemblySheet;
      List<ElementId> relinquishList;
      Result result = BatchPopulatorCore.Execute(application, BatchTicketPopulator.strLastUsedScale, this.bMultiTicket, this.intMultiSheetCurrentSheetNumber, selectedIds, message, out AssemblySheet, out Dictionary<ElementId, Dictionary<ViewSheet, List<View>>> _, out relinquishList, out bool _);
      if (result != null)
      {
        int num = (int) transaction.RollBack();
        if (relinquishList.Count > 0)
        {
          RelinquishOptions generalCategories = new RelinquishOptions(false);
          generalCategories.CheckedOutElements = true;
          TransactWithCentralOptions options = new TransactWithCentralOptions();
          WorksharingUtils.RelinquishOwnership(document, generalCategories, options);
        }
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        return result;
      }
      if (transaction.Commit() != TransactionStatus.Committed)
      {
        TaskDialog.Show("Error", "Unable to commit transaction group.  Please contact support");
        int num = (int) transaction.RollBack();
        return (Result) 1;
      }
      application.ActiveUIDocument.RequestViewChange((View) AssemblySheet);
      App.DialogSwitches.SuspendModelLockingforOperation = false;
      return (Result) 0;
    }
  }
}
