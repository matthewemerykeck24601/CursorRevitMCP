// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.AcceptChangesEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class AcceptChangesEvent : IExternalEventHandler
{
  public void Execute(UIApplication app)
  {
    IEnumerable<AssemblyViewModel> list = (IEnumerable<AssemblyViewModel>) App.TicketManagerWindow.DataGrid.SelectedItems.Cast<AssemblyViewModel>().ToList<AssemblyViewModel>();
    List<Element> elementList = new List<Element>();
    using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "Accept Changes - "))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        int num1 = (int) transaction.Start();
        bool flag = false;
        foreach (AssemblyViewModel assemblyViewModel in list)
        {
          if (assemblyViewModel.Assemblied == "No")
          {
            new TaskDialog("Ticket Release Error")
            {
              Id = "ID_Ticket_Status",
              Title = "Ticket Assembly Warning",
              MainIcon = ((TaskDialogIcon) (int) ushort.MaxValue),
              TitleAutoPrefix = true,
              AllowCancellation = true,
              MainContent = $"The Element with MarkNumber: {assemblyViewModel.MarkNumber} is not an Assembly yet",
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show();
          }
          else
          {
            if (assemblyViewModel.IsFlagged || assemblyViewModel.isInvalidQuantity || assemblyViewModel.SheetCountFlagged)
            {
              assemblyViewModel.IsFlagged = false;
              assemblyViewModel.isInvalidQuantity = false;
              assemblyViewModel.SheetCountFlagged = false;
              assemblyViewModel.SetWarnings();
              assemblyViewModel.SetMarkNumberCellColors();
              assemblyViewModel.SetQuantityCellColors();
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "TKT_TOTAL_RELEASED");
              int parameterAsInt1 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) assemblyViewModel.Assembly, "TKT_TOTAL_CREATED");
              int parameterAsInt2 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) assemblyViewModel.Assembly, "TKT_TOTAL_DETAILED");
              Parameter parameter1 = assemblyViewModel.Assembly.LookupParameter("TKT_TOTAL_RELEASED");
              if (parameter1 != null && parameterAsString != "")
                parameter1.Set(assemblyViewModel.Quantity.ToString());
              Parameter parameter2 = assemblyViewModel.Assembly.LookupParameter("TKT_TOTAL_CREATED");
              if (parameter2 != null && parameterAsInt1 != -1)
                parameter2.Set(assemblyViewModel.Quantity);
              Parameter parameter3 = assemblyViewModel.Assembly.LookupParameter("TKT_TOTAL_DETAILED");
              if (parameter3 != null && parameterAsInt2 != -1)
                parameter3.Set(assemblyViewModel.Quantity);
              assemblyViewModel.Assembly.LookupParameter("TKT_VIEWSHEET_COUNT")?.Set(assemblyViewModel.AssociatedViewSheets.Count);
              assemblyViewModel.Assembly.LookupParameter("TICKET_FLAGGED")?.Set(0);
              Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) assemblyViewModel.Assembly, "EDITS ACCEPTED");
              flag = true;
            }
            if (assemblyViewModel.SheetFlagged)
            {
              int num2 = (int) MessageBox.Show("Edit cannot be accepted because all sheets were deleted after the ticket was detailed or released. In order to accept edits, please create a ticket view sheet for this assembly.", "Warning");
            }
          }
        }
        if (transaction.Commit() == TransactionStatus.Committed && flag && app.ActiveUIDocument.Document.IsWorkshared)
        {
          ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
          foreach (AssemblyViewModel assemblyViewModel in list)
            elementsToCheckout.Add(assemblyViewModel.Assembly.Id);
          WorksharingUtils.CheckoutElements(app.ActiveUIDocument.Document, elementsToCheckout);
        }
        ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
        ActiveModel.UIDoc = app.ActiveUIDocument;
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Exception", ex.ToString());
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public string GetName() => "Accept Changes Event";
}
