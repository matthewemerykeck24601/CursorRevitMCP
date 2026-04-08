// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.AutoTicketUserSetting_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.UserSettingTools.Views;
using System;
using System.Windows;

#nullable disable
namespace EDGE.UserSettingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class AutoTicketUserSetting_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    using (Transaction transaction = new Transaction(document, "Auto Ticket Setting"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        bool bCancelled;
        AutoTicketSettings autoTicketSettings = new AutoTicketSettings(document, out bCancelled);
        if (bCancelled)
        {
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
        autoTicketSettings.ShowDialog();
      }
      catch (Exception ex)
      {
        int num3 = (int) MessageBox.Show(ex.ToString());
      }
      int num4 = (int) transaction.Commit();
      return (Result) 0;
    }
  }
}
