// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.UpdtTktCreatedInformation
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

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
internal class UpdtTktCreatedInformation : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(document, "Update Ticket Creation Information"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        Element element = document.GetElement(document.ActiveView.AssociatedAssemblyInstanceId);
        Parameters.UpdateStatusParameters(element, "CREATED");
        Parameters.AddEditComment(element, "CREATED", "N/A");
        int num2 = (int) transaction.Commit();
        return (Result) 0;
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
}
