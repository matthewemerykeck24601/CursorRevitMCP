// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.EDGERTicketPopulator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class EDGERTicketPopulator : IExternalCommand
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
    string strLastUsedScale = EDGERTicketPopulator.strLastUsedScale;
    int num = this.bMultiTicket ? 1 : 0;
    int currentSheetNumber = this.intMultiSheetCurrentSheetNumber;
    string message1 = message;
    return TicketPopulatorCore.Execute(application, strLastUsedScale, num != 0, currentSheetNumber, message1);
  }
}
