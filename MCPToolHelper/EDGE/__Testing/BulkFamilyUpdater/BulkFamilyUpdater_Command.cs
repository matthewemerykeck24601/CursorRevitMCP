// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.BulkFamilyUpdater_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.__Testing.BulkFamilyUpdater.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater;

[Transaction(TransactionMode.Manual)]
public class BulkFamilyUpdater_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document1 = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Autodesk.Revit.ApplicationServices.Application application = commandData.Application.Application;
    string str1 = BulkFamilyUpdater_Command.SaveAndClose(document1);
    if (str1.Equals(""))
      return (Result) 1;
    DirectoryInfo directoryInfo = new DirectoryInfo(str1);
    FileInfo fileInfo1 = new FileInfo(str1);
    FileInfo[] array1 = ((IEnumerable<FileInfo>) directoryInfo.GetFiles("*.*")).Where<FileInfo>((Func<FileInfo, bool>) (s => s.FullName.EndsWith(".rfa"))).ToArray<FileInfo>();
    List<string> stringList1 = new List<string>();
    if (directoryInfo.Exists)
    {
      if (((IEnumerable<FileInfo>) array1).Count<FileInfo>() == 0)
      {
        new TaskDialog("EDGE^R")
        {
          MainInstruction = "File Info Error",
          MainContent = "There were no .rfa files found in the selected folder to update. Please try again selecting a different folder."
        }.Show();
        return (Result) 1;
      }
      foreach (FileInfo fileInfo2 in array1)
        stringList1.Add(fileInfo2.FullName.ToUpper());
    }
    bool flag = true;
    List<string> stringList2 = new List<string>();
    List<string> openFamilies = new List<string>();
    foreach (Document document2 in commandData.Application.Application.Documents)
    {
      if (document2.IsFamilyDocument && document2.IsModified && stringList1.Contains(document2.PathName.ToUpper()))
      {
        if (document2.PathName.ToUpper().Equals(document1.PathName.ToUpper()))
          flag = false;
        stringList2.Add(document2.Title);
      }
      openFamilies.Add(document2.Title);
    }
    if (!flag && stringList2.Count == 1)
    {
      TaskDialog taskDialog = new TaskDialog("Bulk Family Updater");
      taskDialog.MainInstruction = $"The open family {document1.Title} is set to be updated but has unsaved changes. All pending changes must be resolved before running Bulk Family Updater.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Save family", document1.PathName);
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel");
      if (taskDialog.Show() != 1001)
        return (Result) 1;
      document1.Save();
    }
    else if (stringList2.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Bulk Family Updater");
      taskDialog1.MainInstruction = "One or more families to be updated are currently open and have pending changes. Please close or save these families before attempting to run the Bulk Family Updater. Expand for details.";
      foreach (string str2 in stringList2)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str2}\n";
      }
      taskDialog1.CommonButtons = (TaskDialogCommonButtons) 1;
      taskDialog1.Show();
      return (Result) 1;
    }
    BulkFamilyUpdatersUtils.populateDictionaries(document1);
    IList<FamilyParameter> familyParameters = (IList<FamilyParameter>) null;
    if (document1.IsFamilyDocument)
      familyParameters = document1.FamilyManager.GetParameters();
    DefinitionFile definitionFile = (DefinitionFile) null;
    try
    {
      definitionFile = application.OpenSharedParameterFile();
    }
    catch (Exception ex)
    {
      new TaskDialog("Bulk Family Updater")
      {
        MainContent = ("Unable to access current shared parameter file: " + application.SharedParametersFilename)
      }.Show();
    }
    if (definitionFile == null)
      new TaskDialog("Bulk Family Updater")
      {
        MainContent = "Current shared parameter file not accessible. Please check that a valid shared parameter file is selected."
      }.Show();
    DefinitionGroups sharedParameters = (DefinitionGroups) null;
    if (definitionFile != null)
      sharedParameters = definitionFile.Groups;
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    BFUMainWindow bfuMainWindow = new BFUMainWindow(familyParameters, sharedParameters, mainWindowHandle, document1);
    bfuMainWindow.ShowDialog();
    if (bfuMainWindow.cancelBool)
      return (Result) 1;
    TaskManager taskManager = bfuMainWindow.getTaskManager();
    FileInfo[] array2 = ((IEnumerable<FileInfo>) directoryInfo.GetFiles("*.*")).Where<FileInfo>((Func<FileInfo, bool>) (s => s.FullName.EndsWith(".rfa"))).ToArray<FileInfo>();
    if (directoryInfo.Exists && ((IEnumerable<FileInfo>) array2).Count<FileInfo>() == 0)
    {
      new TaskDialog("EDGE^R")
      {
        MainInstruction = "File Info Error",
        MainContent = "There were no .rfa files found in the selected folder to update. Please run the tool again targeting a valid folder that includes .rfa files."
      }.Show();
      return (Result) 1;
    }
    taskManager.setSharedParameterDefinition(sharedParameters);
    taskManager.runTasks(array2, document1, application, openFamilies);
    taskManager.createExcelSpreadSheet(str1);
    return (Result) 0;
  }

  internal static string SaveAndClose(Document revitDoc)
  {
    string fileSavePath = BulkFamilyUpdater_Command.GetFileSavePath(revitDoc);
    return string.IsNullOrWhiteSpace(fileSavePath) ? "" : fileSavePath;
  }

  public static string GetFileSavePath(Document revitDoc)
  {
    bool flag1 = false;
    try
    {
      string fileSavePath = "C:\\EDGEforRevit\\BulkUpdater\\";
      Parameter parameter = (Parameter) null;
      if (revitDoc.ProjectInformation != null)
        parameter = revitDoc.ProjectInformation.LookupParameter("BULK_UPDATER_SAVE_FOLDER_PATH");
      if (parameter != null && parameter.HasValue)
        fileSavePath = parameter.AsString();
      if (fileSavePath.Equals("") || fileSavePath.Equals(" "))
        fileSavePath = "C:\\EDGEforRevit\\BulkUpdater\\";
      while (!flag1)
      {
        TaskDialog taskDialog = new TaskDialog("Bulk Family Updater");
        taskDialog.MainInstruction = "Family files in the following folder path will be updated: \n" + fileSavePath;
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Browse for new path");
        TaskDialogResult taskDialogResult1 = taskDialog.Show();
        bool flag2;
        if (taskDialogResult1 != 1)
        {
          if (taskDialogResult1 == 2)
            return "";
          if (taskDialogResult1 == 1001)
          {
            fileSavePath = BulkFamilyUpdater_Command.BrowseNewPath(fileSavePath);
            if (fileSavePath.Equals(""))
            {
              fileSavePath = "C:\\EDGEforRevit\\BulkUpdater\\";
            }
            else
            {
              if (parameter != null && !parameter.IsReadOnly && (!parameter.HasValue || !parameter.AsString().Equals(fileSavePath)))
              {
                TaskDialogResult taskDialogResult2 = new TaskDialog("Family Updater Path")
                {
                  MainInstruction = $"Set {fileSavePath} to new default Bulk Family Updater path?",
                  CommonButtons = ((TaskDialogCommonButtons) 6)
                }.Show();
                if (taskDialogResult2 == 6)
                {
                  using (Transaction transaction = new Transaction(revitDoc, "Set new Bulk Family Updater path"))
                  {
                    int num1 = (int) transaction.Start();
                    parameter.Set(fileSavePath);
                    int num2 = (int) transaction.Commit();
                    flag2 = true;
                  }
                }
                else if (taskDialogResult2 == 2)
                  return "";
              }
              flag1 = true;
            }
          }
        }
        else
        {
          flag2 = true;
          if (parameter != null && !parameter.IsReadOnly && (!parameter.HasValue || !parameter.AsString().Equals(fileSavePath)))
          {
            TaskDialogResult taskDialogResult3 = new TaskDialog("Family Updater Path")
            {
              MainInstruction = $"Set {fileSavePath} to new default Bulk Family Updater path?",
              CommonButtons = ((TaskDialogCommonButtons) 6)
            }.Show();
            if (taskDialogResult3 == 6)
            {
              using (Transaction transaction = new Transaction(revitDoc, "Set new Bulk Family Updater path"))
              {
                int num3 = (int) transaction.Start();
                parameter.Set(fileSavePath);
                int num4 = (int) transaction.Commit();
                flag2 = true;
              }
            }
            else if (taskDialogResult3 == 2)
              return "";
          }
          flag1 = true;
        }
      }
      FileInfo fileInfo = new FileInfo(fileSavePath);
      if (new DirectoryInfo(fileSavePath).Exists)
        return fileSavePath;
      new TaskDialog("EDGE^R")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Unable to save to location specified",
        MainContent = "Unable to save to directory in the specified location.  Please change save location to a writable location. "
      }.Show();
      return "";
    }
    catch (Exception ex)
    {
      new TaskDialog("EDGE^R - Bulk Family Updater")
      {
        MainInstruction = "Error: Unable to save file",
        MainContent = "One or more files to be updated is currently open. Please close these files and try again."
      }.Show();
      return "";
    }
  }

  private static string BrowseNewPath(string exportPath)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    folderBrowserDialog.Description = "Select a path for export";
    folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
    folderBrowserDialog.SelectedPath = exportPath;
    switch (folderBrowserDialog.ShowDialog())
    {
      case DialogResult.OK:
        return folderBrowserDialog.SelectedPath + "\\";
      case DialogResult.Cancel:
        return exportPath = "";
      default:
        return exportPath;
    }
  }
}
