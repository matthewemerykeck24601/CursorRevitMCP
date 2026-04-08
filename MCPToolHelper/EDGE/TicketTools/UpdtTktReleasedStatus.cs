// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.UpdtTktReleasedStatus
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
internal class UpdtTktReleasedStatus : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    using (Transaction transaction = new Transaction(activeUiDocument.Document, "Update Ticket Release Status"))
    {
      try
      {
        int num = (int) transaction.Start();
        return UpdtTktReleasedStatus.Update(activeUiDocument, transaction);
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
    Element element = document1.GetElement(document1.ActiveView.AssociatedAssemblyInstanceId);
    Parameter parameter = element.LookupParameter("TICKET_FLAGGED");
    if (!document1.ActiveView.IsAssemblyView)
    {
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "This tool must be run within an active Assembly/Ticket view."
      }.Show();
      return (Result) 1;
    }
    if (parameter.AsInteger().Equals(1))
    {
      new TaskDialog("Ticket Flagged Status Update")
      {
        Id = "ID_Ticket_FLAG_Status",
        MainIcon = ((TaskDialogIcon) (int) ushort.MaxValue),
        Title = "Ticket Flagged Warning",
        TitleAutoPrefix = true,
        AllowCancellation = true,
        MainContent = "This Ticket has Warning Flags which must be reviewed before releasing.  Please review the Edit History in the Ticket Manager and Accept Edits before attempting to release this Ticket.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    Parameters.UpdateStatusParameters(element, "RELEASED");
    Parameters.AddEditComment(element, "RELEASED", "TICKET RELEASED");
    int num = (int) transaction.Commit();
    return (Result) 0;
  }
}
