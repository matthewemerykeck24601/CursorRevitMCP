// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.TicketManagerCustomizationSetting_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.Views;
using System;
using System.Diagnostics;
using System.Windows;

#nullable disable
namespace EDGE.UserSettingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TicketManagerCustomizationSetting_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Ticket Manager Customization Settings must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Ticket Manager Customization Settings must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(document, "Edge Ticket Manager Customization Setting"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        CustomizeWindow customizeWindow = new CustomizeWindow(commandData.Application);
        if (customizeWindow.closed)
          return (Result) 1;
        customizeWindow.ShowDialog();
        if (customizeWindow.closed)
          return (Result) 1;
      }
      catch (Exception ex)
      {
        int num2 = (int) MessageBox.Show(ex.ToString());
      }
      int num3 = (int) transaction.Commit();
      return (Result) 0;
    }
  }
}
