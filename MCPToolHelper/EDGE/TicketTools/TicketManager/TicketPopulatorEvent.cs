// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.TicketPopulatorEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using EDGE.TicketTools.TicketPopulator;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class TicketPopulatorEvent : IExternalEventHandler
{
  public static string strLastUsedScale = "";
  private bool bMultiTicket;
  private int intMultiSheetCurrentSheetNumber = 1;
  private string message = "";

  public void Execute(UIApplication app)
  {
    TicketPopulatorCore.Execute(app, TicketPopulatorEvent.strLastUsedScale, this.bMultiTicket, this.intMultiSheetCurrentSheetNumber, this.message);
    ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
  }

  public string GetName() => "Ticket Populator Event";
}
