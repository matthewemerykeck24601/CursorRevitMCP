// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingAdmin_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ModelLockingAdmin_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    Application application = commandData.Application.Application;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Document")
      {
        MainInstruction = "Permissions cannot be configured for a family document.",
        MainContent = "Family documents are currently not supported for model locking.",
        AllowCancellation = true
      }.Show();
      return (Result) 1;
    }
    if (!ModelLockingUtils.ModelLockingEnabled(document))
    {
      new TaskDialog("Model Locking")
      {
        MainInstruction = "Model locking is currently disabled.",
        MainContent = "Check Project Information to enable Model Locking before trying to manage permissions.",
        AllowCancellation = true
      }.Show();
      return (Result) 1;
    }
    bool flag = true;
    TaskDialog taskDialog = new TaskDialog("Model Locking Administration");
    taskDialog.MainInstruction = "The producer admin file you've designated was not found, or no producer admins were assigned in that file.";
    taskDialog.MainContent = "Would you like to assign producer admins now?";
    taskDialog.CommonButtons = (TaskDialogCommonButtons) 14;
    string path = App.MLFolderPath + "\\ModelLockingAdminRoster.txt";
    ManageAdmins manageAdmins = new ManageAdmins(document, path);
    if (File.Exists(path))
    {
      if (!manageAdmins.HasItems())
        flag = false;
    }
    else
      flag = false;
    if (!manageAdmins.bFileFailed && !flag)
    {
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (Result) 1;
      if (taskDialogResult != 6)
      {
        if (taskDialogResult == 7)
          ;
      }
      else
      {
        int num = (int) manageAdmins.ShowDialog();
      }
    }
    return App.LockingManger.ConfigureSettings(commandData.Application.ActiveUIDocument.Document);
  }
}
