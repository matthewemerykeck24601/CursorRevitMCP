// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.MarkPrefixUserSetting_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.UserSettingTools.Views;
using System;
using System.Diagnostics;
using System.Windows;

#nullable disable
namespace EDGE.UserSettingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class MarkPrefixUserSetting_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    using (Transaction transaction = new Transaction(document, "User Setting"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        bool ifContinue;
        MarkVerificationSettingWindow verificationSettingWindow = new MarkVerificationSettingWindow(document, mainWindowHandle, out ifContinue);
        if (!ifContinue)
          return (Result) 1;
        verificationSettingWindow.ShowDialog();
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
