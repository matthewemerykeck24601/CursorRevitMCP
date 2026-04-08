// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.UpdtTktFlagStatus
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.AssemblyUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class UpdtTktFlagStatus : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    if (!document.ActiveView.IsAssemblyView)
    {
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "This tool must be run within an active Assembly/Ticket view."
      }.Show();
      return (Result) 1;
    }
    document.GetElement(document.ActiveView.AssociatedAssemblyInstanceId).LookupParameter("TICKET_FLAGGED");
    using (Transaction transaction = new Transaction(document, "Update Ticket Flag Status"))
    {
      try
      {
        int num = (int) transaction.Start();
        return UpdtTktFlagStatus.Update(activeUiDocument, transaction);
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static Result Update(UIDocument document, Transaction transaction)
  {
    UIDocument uidoc = document;
    ActiveModel.GetInformation(uidoc);
    Document document1 = uidoc.Document;
    Application application = uidoc.Application.Application;
    if (!document1.ActiveView.IsAssemblyView)
    {
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "This tool must be run within an active Assembly/Ticket view."
      }.Show();
      return (Result) 1;
    }
    Element element = document1.GetElement(document1.ActiveView.AssociatedAssemblyInstanceId);
    if (element.LookupParameter("TICKET_FLAGGED").AsInteger().Equals(0))
    {
      new TaskDialog("Ticket Flag Status")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "There are no current Warning Flags for this ticket."
      }.Show();
      return (Result) 0;
    }
    TaskDialog taskDialog = new TaskDialog("Ticket Flagged Status Update");
    taskDialog.Id = "ID_Ticket_FLAG_Status";
    taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
    taskDialog.Title = "Ticket Flagged Status Update";
    taskDialog.TitleAutoPrefix = true;
    taskDialog.AllowCancellation = true;
    taskDialog.MainContent = "Update the Flagged Status of the current Ticket.";
    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Remove Ticket Flag and accept edits to the Ticket Assembly.");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Keep Assembly Flag as-is and Cancel.");
    TaskDialogResult taskDialogResult = taskDialog.Show();
    if (taskDialogResult == 1001)
    {
      if (new TaskDialog("Confirm Flag Removal")
      {
        MainContent = "Are you sure you wish to reset the Flagged Status for this Ticket?",
        CommonButtons = ((TaskDialogCommonButtons) 6),
        DefaultButton = ((TaskDialogResult) 7),
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show() != 6)
        return (Result) 1;
      element.LookupParameter("TICKET_FLAGGED").Set(0);
      Parameters.ShowEditCommentForm(application, element, "FLAG REVIEW");
      int num = (int) transaction.Commit();
      return (Result) 0;
    }
    if (taskDialogResult != 1002)
      return (Result) 1;
    Parameters.ShowEditCommentForm(application, element, "FLAG REVIEW");
    int num1 = (int) transaction.Commit();
    return (Result) 0;
  }
}
