// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.CADExportSettings_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class CADExportSettings_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (document.IsFamilyDocument)
    {
      TaskDialog.Show("Error", "CAD Export Settings cannot be run from the family editor.");
      return (Result) 1;
    }
    if (string.IsNullOrWhiteSpace(App.CADExFolderPath))
    {
      TaskDialog.Show("CAD Export Settings", "Please set the CAD Export Settings folder path in Edge Preferences to use CAD Export Settings");
      return (Result) 1;
    }
    string manufacturer = "";
    Parameter parameter = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      manufacturer = parameter.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer))
    {
      TaskDialog.Show("CAD EXPORT SETTINGS", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to open CAD Export Settings again.");
      return (Result) 1;
    }
    new CADExportSettingsWindow(Process.GetCurrentProcess().MainWindowHandle, document.GetUnits(), manufacturer, !Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(document)).ShowDialog();
    return (Result) 0;
  }
}
