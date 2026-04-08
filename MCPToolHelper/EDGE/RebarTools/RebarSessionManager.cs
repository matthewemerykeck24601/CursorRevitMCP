// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarSessionManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Utils;

#nullable disable
namespace EDGE.RebarTools;

public class RebarSessionManager
{
  public Dictionary<string, RebarControlMarkManager3> BarMarkManagers;
  private Dictionary<string, bool> RebarAutomationToggles;

  public RebarSessionManager()
  {
    this.BarMarkManagers = new Dictionary<string, RebarControlMarkManager3>();
    this.RebarAutomationToggles = new Dictionary<string, bool>();
  }

  public AddDocTrackerResult AddNewDocumentRebarTracker(Document revitDoc, string manufactuerName)
  {
    string pathName = revitDoc.PathName;
    if (this.BarMarkManagers.ContainsKey(pathName))
      QA.InHouseMessage("in AddNewDocumentRebarTracker() but document name already exists.  Should we be adding one here? About to crash because we already have one...");
    ManufacturerRebarSettings mfgSettingsData = new ManufacturerRebarSettings(revitDoc, manufactuerName);
    if (mfgSettingsData.IsValidSettings)
    {
      if (this.BarMarkManagers.ContainsKey(pathName))
        this.BarMarkManagers.Remove(pathName);
      this.BarMarkManagers.Add(pathName, new RebarControlMarkManager3(pathName, mfgSettingsData));
      return AddDocTrackerResult.OK;
    }
    if (mfgSettingsData.UserCancelledLoad)
      return AddDocTrackerResult.UserCancelledLoad;
    this.SetBarMarkAutomationForDocument(pathName, false);
    return AddDocTrackerResult.DisableBarMarkAutomation;
  }

  public RebarControlMarkManager3 Manager(string documentName)
  {
    if (this.BarMarkManagers.ContainsKey(documentName))
      return this.BarMarkManagers[documentName];
    QA.InHouseMessage($"document: {documentName} requested a mark manager that doesn't exist.  Need to create one first!");
    return (RebarControlMarkManager3) null;
  }

  public void DisposeOfMarkManager(string documentName)
  {
    if (!this.BarMarkManagers.ContainsKey(documentName))
    {
      if (this.IsAutomatedDocument(documentName))
        QA.InHouseMessage($"Could not find mark manager for this document: {documentName} Should there be one?  This document is supposed to be automated.");
    }
    else
      this.BarMarkManagers.Remove(documentName);
    if (!this.RebarAutomationToggles.ContainsKey(documentName))
      return;
    this.RebarAutomationToggles.Remove(documentName);
  }

  public bool IsAutomatedDocument(string documentPath)
  {
    return App.RebarMarkAutomation && this.RebarAutomationToggles.ContainsKey(documentPath) && this.RebarAutomationToggles[documentPath];
  }

  public bool IsAutomatedDocument(Document revitDoc) => this.IsAutomatedDocument(revitDoc.PathName);

  public void SetBarMarkAutomationForDocument(string documentPath, bool bValue)
  {
    if (this.RebarAutomationToggles.ContainsKey(documentPath))
      this.RebarAutomationToggles[documentPath] = bValue;
    else
      this.RebarAutomationToggles.Add(documentPath, bValue);
  }

  public void HandleModelSavedAs(string OriginalPath, string NewPath)
  {
    if (OriginalPath.Equals(NewPath))
      return;
    if (this.BarMarkManagers.ContainsKey(OriginalPath))
    {
      if (this.BarMarkManagers.ContainsKey(NewPath))
        QA.InHouseMessage($"RebarSessionManager: Handle model saved as already contains a manager for the new path: {NewPath} WHat should happen here?  How did this happen in the first place?");
      RebarControlMarkManager3 barMarkManager = this.BarMarkManagers[OriginalPath];
      this.BarMarkManagers.Add(NewPath, barMarkManager);
      this.BarMarkManagers.Remove(OriginalPath);
    }
    else if (QAUtils.IsInHouse())
      TaskDialog.Show("ERROR", "RebarSessionManager: Handle model saved as is attempting to update for a document that is not actually tracked but IsTracked thinks it is.  What is happening here?");
    if (this.RebarAutomationToggles.ContainsKey(OriginalPath))
    {
      bool automationToggle = this.RebarAutomationToggles[OriginalPath];
      this.RebarAutomationToggles.Add(NewPath, automationToggle);
      this.RebarAutomationToggles.Remove(OriginalPath);
    }
    else
    {
      if (!QAUtils.IsInHouse())
        return;
      TaskDialog.Show("ERROR", "RebarSessionManager: Handle model saved as is attempting to update toggles for a document that is not actually tracked but IsTracked thinks it is.  What is happening here?");
    }
  }
}
