// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Insulation_Drawing_Settings.InsulationDrawingSetting_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;

#nullable disable
namespace EDGE.UserSettingTools.Insulation_Drawing_Settings;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class InsulationDrawingSetting_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    new InsulationDrawingSettingsWindow(commandData.Application.ActiveUIDocument.Document, Process.GetCurrentProcess().MainWindowHandle).ShowDialog();
    return (Result) 0;
  }
}
