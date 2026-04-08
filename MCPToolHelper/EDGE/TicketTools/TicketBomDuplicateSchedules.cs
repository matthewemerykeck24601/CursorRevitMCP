// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketBomDuplicateSchedules
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TicketBomDuplicateSchedules : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (!document.ActiveView.IsAssemblyView || !(document.ActiveView is ViewSheet))
    {
      TaskDialog.Show("Error", "Error: Run this command in an Assembly Sheet View");
      return (Result) 1;
    }
    ElementId assemblyInstanceId = (document.ActiveView as ViewSheet).AssociatedAssemblyInstanceId;
    string name = document.GetElement(assemblyInstanceId).Name;
    ViewSheet currentSheetView = document.ActiveView as ViewSheet;
    IEnumerable<ElementId> source = new FilteredElementCollector(document).OfClass(typeof (ScheduleSheetInstance)).Cast<ScheduleSheetInstance>().Where<ScheduleSheetInstance>((Func<ScheduleSheetInstance, bool>) (ssi => ssi.OwnerViewId == currentSheetView.Id)).Select<ScheduleSheetInstance, ElementId>((Func<ScheduleSheetInstance, ElementId>) (ssi => ssi.ScheduleId));
    Transaction transaction = new Transaction(document, "Create BOM Schedules");
    if (transaction.Start() == TransactionStatus.Started)
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        string errMessage;
        Result functionResult;
        Dictionary<ElementId, string> instancesForAssembly = TicketBOMCore.GetScheduleInstancesForAssembly(document, commandData.Application.ActiveUIDocument, assemblyInstanceId, out errMessage, out functionResult);
        if (instancesForAssembly == null)
        {
          message = errMessage;
          return functionResult;
        }
        ElementId elementId = ElementId.InvalidElementId;
        if (document.ActiveView is ViewSheet)
        {
          Line bound = Line.CreateBound(XYZ.Zero, XYZ.BasisX);
          elementId = document.Create.NewDetailCurve(document.ActiveView, (Curve) bound).Id;
        }
        if (name.Equals("Sheet"))
        {
          SheetRenameForm sheetRenameForm = new SheetRenameForm();
          int num = (int) sheetRenameForm.ShowDialog();
          document.ActiveView.get_Parameter(BuiltInParameter.SHEET_NUMBER).Set(sheetRenameForm.numberBox.Text);
          document.ActiveView.Name = sheetRenameForm.nameBox.Text;
        }
        bool flag = false;
        foreach (ElementId key in instancesForAssembly.Keys)
        {
          if (!source.Contains<ElementId>(key))
          {
            flag = true;
            XYZ zero = XYZ.Zero;
            XYZ origin;
            try
            {
              origin = activeUiDocument.Selection.PickPoint(instancesForAssembly[key]);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
              break;
            }
            ScheduleSheetInstance.Create(document, document.ActiveView.Id, key, origin);
          }
        }
        if (!flag)
          TaskDialog.Show("Information", "No schedules found To place or all required schedules are already placed on this sheet.");
        if (elementId != ElementId.InvalidElementId)
          document.Delete(elementId);
        if (transaction.Commit() == TransactionStatus.Committed)
          return (Result) 0;
        message = "Failed to commit transaction.";
        return (Result) -1;
      }
      catch (Exception ex)
      {
        message = "Unhandled exception: " + ex.Message.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    else
    {
      message = "Failed to start transaction.";
      return (Result) -1;
    }
  }
}
