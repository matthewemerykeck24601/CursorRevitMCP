// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.LoadFamily
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.EDGEBrowser.Forms;
using System;
using System.IO;

#nullable disable
namespace EDGE.EDGEBrowser;

public class LoadFamily : IExternalEventHandler
{
  private void LoadFamilies(
    string filepath,
    string fileName,
    string txtPath,
    string txtName,
    Document revitDoc)
  {
    if (File.Exists("C:\\EDGEforRevit\\FamiliesTemp\\" + fileName))
    {
      Uri uri1 = new Uri(filepath);
      if (string.IsNullOrEmpty(txtName))
      {
        IFamilyLoadOptions familyLoadOptions = UIDocument.GetRevitUIFamilyLoadOptions();
        Family family = (Family) null;
        revitDoc.LoadFamily("C:\\EDGEforREvit\\FamiliesTemp\\" + fileName, familyLoadOptions, out family);
        string str = fileName.Split('.')[0];
        if (family != null)
          TaskDialog.Show("EDGE Browser", str + " has been successfully added to the project");
        else
          TaskDialog.Show("EDGE Browser", "Unable to Load Family. Please ensure that the family you are trying to load is valid for the currently active file or ensure that the family is not already loaded in the currently active project.");
      }
      else
      {
        Uri uri2 = new Uri(txtPath);
        FamilyTypeSelectionPaneWindow selectionPaneWindow = new FamilyTypeSelectionPaneWindow(File.ReadAllLines("C:\\EDGEforREvit\\FamiliesTemp\\" + txtName), fileName, "C:\\EDGEforREvit\\FamiliesTemp\\" + fileName, revitDoc);
        selectionPaneWindow.ShowDialog();
        string str = fileName.Split('.')[0];
        if (selectionPaneWindow.closed)
          return;
        TaskDialog.Show("EDGE Browser", str + " has been successfully added to the project");
      }
    }
    else
    {
      Directory.CreateDirectory("C:\\EDGEforRevit\\FamiliesTemp");
      if (!Directory.Exists("C:\\EDGEforRevit\\FamiliesTemp"))
        return;
      Uri uri3 = new Uri(filepath);
      if (string.IsNullOrEmpty(txtName))
      {
        IFamilyLoadOptions familyLoadOptions = UIDocument.GetRevitUIFamilyLoadOptions();
        Family family = (Family) null;
        revitDoc.LoadFamily("C:\\EDGEforREvit\\FamiliesTemp\\" + fileName, familyLoadOptions, out family);
        string str = fileName.Split('.')[0];
        if (family != null)
          TaskDialog.Show("EDGE Browser", str + " has been successfully added to the project");
        else
          TaskDialog.Show("EDGE Browser", "Unable to Load Family. Please ensure that the family you are trying to load is valid for the currently active file or ensure that the family is not already loaded in the currently active project.");
      }
      else
      {
        Uri uri4 = new Uri(txtPath);
        FamilyTypeSelectionPaneWindow selectionPaneWindow = new FamilyTypeSelectionPaneWindow(File.ReadAllLines("C:\\EDGEforREvit\\FamiliesTemp\\" + txtName), fileName, "C:\\EDGEforREvit\\FamiliesTemp\\" + fileName, revitDoc);
        selectionPaneWindow.ShowDialog();
        string str = fileName.Split('.')[0];
        if (selectionPaneWindow.closed)
          return;
        TaskDialog.Show("EDGE Browser", str + " has been successfully added to the project");
      }
    }
  }

  public void Execute(UIApplication app)
  {
    try
    {
      using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "Load Family"))
      {
        string filePath = ((BrowserViewModel) App.ParentPaneWindowRef.DataContext).FilePath;
        string fileName = ((BrowserViewModel) App.ParentPaneWindowRef.DataContext).FileName;
        string txtFilePath = ((BrowserViewModel) App.ParentPaneWindowRef.DataContext).txtFilePath;
        string txtFileName = ((BrowserViewModel) App.ParentPaneWindowRef.DataContext).txtFileName;
        int num1 = (int) transaction.Start();
        this.LoadFamilies(filePath, fileName, txtFilePath, txtFileName, app.ActiveUIDocument.Document);
        int num2 = (int) transaction.Commit();
      }
    }
    catch (Exception ex)
    {
      TaskDialog.Show("Unable to Load Family", "Unable to Load Family. Please check your internet connection and try again.");
    }
  }

  public string GetName() => "Load Family Event";
}
