// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarEditingUpdateUtility
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.RebarTools;

public class RebarEditingUpdateUtility
{
  private static Document s_revitDoc;
  public static bool ShowDebugDialogs;
  public static bool ShowDebugTimes;

  public static void RebarMarkUpdaterCore(
    IEnumerable<Element> rebarList,
    Document doc,
    bool bAddedBar = false)
  {
    RebarEditingUpdateUtility.s_revitDoc = doc;
    Stack<DateTime> dateTimeStack = new Stack<DateTime>();
    if (doc.IsFamilyDocument)
      return;
    if (doc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER") == null && !App.ProjectParameterWarningDialogList.Contains(ActiveModel.Document.PathName))
    {
      TaskDialog taskDialog = new TaskDialog("Edge Rebar Warning");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainContent = $"This project does not have the PROJECT_CLIENT_PRECAST_MANUFACTURER shared parameter available at the project level which is required (along with its text value) to read standard bars into memory from a corresponding text file.{Environment.NewLine}{Environment.NewLine}Please add the PROJECT_CLIENT_PRECAST_MANUFACTURER shared parameter to the project and assign it to Project Information in order for EDGE^R automatic bar marking to operate with manufacturer specific standard bars.{Environment.NewLine}{Environment.NewLine}This parameter should be included even if there are no standard bars to reference.{Environment.NewLine}{Environment.NewLine}Note:  This dialog is being presented now in this first run of the EDGE^R rebar automatic mark updating code for this project in this session of Revit and will not be displayed any further during this session of Revit.  Automatic bar marking will continue with project specific marks only.";
      App.ProjectParameterWarningDialogList.Add(ActiveModel.Document.PathName);
      taskDialog.Show();
    }
    List<Element> list = rebarList.ToList<Element>();
    if (App.RebarManager.Manager(doc.PathName) == null)
      QA.InHouseMessage($"Major problem here!  We are trying to get a manager for doc: {doc.PathName} but one does not exist.");
    foreach (Element element in list)
    {
      if (element is FamilySymbol familySymbol && familySymbol.LookupParameter("CONTROL_MARK") != null)
        list.Remove(element);
    }
    if (list.Count <= 0)
      return;
    using (SubTransaction subTransaction = new SubTransaction(doc))
    {
      bool flag = false;
      try
      {
        if (!string.IsNullOrEmpty(doc.PathName))
        {
          if (!list.Any<Element>())
            return;
          int num1 = (int) subTransaction.Start();
          dateTimeStack.Push(DateTime.Now);
          RebarEditingUpdateUtility.AltRebarMarkUpdater(list, doc, bAddedBar);
          dateTimeStack.Push(DateTime.Now);
          int num2 = (int) subTransaction.Commit();
          TimeSpan timeSpan = dateTimeStack.Pop() - dateTimeStack.Pop();
          if (!(EdgeBuildInformation.IsDebugCheck & flag))
            return;
          new TaskDialog("Time")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainContent = (" RebarUpdate Time: " + timeSpan.ToString())
          }.Show();
        }
        else
          new TaskDialog("Error")
          {
            MainContent = "EDGE Automated Rebar Marking cannot function when the file has not been saved at least once.  Therefore the automated marking process has been omitted.",
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show();
      }
      catch (Exception ex)
      {
        ErrorDialog errorDialog = new ErrorDialog();
        errorDialog.Text = "Rebar Update Error";
        errorDialog.label1.Text = ex.ToString();
        int num = (int) errorDialog.ShowDialog();
      }
    }
  }

  public static void AltRebarMarkUpdater(
    List<Element> editedRebarElems,
    Document revitDoc,
    bool bAddedBar)
  {
    foreach (Element editedRebarElem in editedRebarElems)
    {
      if (!editedRebarElem.HasSuperComponent())
      {
        if (Parameters.GetParameterAsString(editedRebarElem, "BAR_SHAPE").Contains("STRAIGHT") || Parameters.GetParameterAsString(editedRebarElem, "IDENTITY_DESCRIPTION").Contains("STRAIGHT BAR (X LENGTH SCHEDULED)"))
        {
          FamilyInstance rebarInstance = editedRebarElem as FamilyInstance;
          Parameter parameter1 = editedRebarElem.LookupParameter("CONTROL_MARK");
          Parameter parameter2 = editedRebarElem.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
          Parameter parameter3 = editedRebarElem.LookupParameter("IDENTITY_DESCRIPTION");
          if (parameter1 != null && !parameter1.IsReadOnly)
          {
            RebarControlMarkManager3 controlMarkManager3 = App.RebarManager.Manager(revitDoc.PathName);
            Rebar rebarBar = new Rebar(rebarInstance, controlMarkManager3.mfgSettingsData._bucketKeying);
            string straightBarMark = App.RebarManager.Manager(revitDoc.PathName).GetStraightBarMark(rebarBar);
            string str = parameter3 != null ? parameter3.AsString() : "";
            if (parameter1.AsString() != straightBarMark || str != rebarBar.IdentityDescriptionLong)
            {
              parameter1.Set(straightBarMark);
              if (parameter2 != null && !parameter2.IsReadOnly)
                parameter2.Set(straightBarMark);
              if (parameter3 != null && !parameter3.IsReadOnly)
                parameter3.Set(rebarBar.IdentityDescriptionLong);
            }
          }
        }
        else if (editedRebarElem is FamilyInstance rebarInstance1)
        {
          RebarControlMarkManager3 controlMarkManager3 = App.RebarManager.Manager(revitDoc.PathName);
          Rebar bar = new Rebar(rebarInstance1, controlMarkManager3.mfgSettingsData._bucketKeying);
          string barMark = controlMarkManager3.GetBarMark(bar, bAddedBar: bAddedBar);
          Parameter parameter4 = editedRebarElem.LookupParameter("CONTROL_MARK");
          Parameter parameter5 = editedRebarElem.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
          Parameter parameter6 = editedRebarElem.LookupParameter("IDENTITY_DESCRIPTION");
          if (parameter4 != null && !parameter4.IsReadOnly)
          {
            string str = parameter6 != null ? parameter6.AsString() : "";
            if (parameter4.AsString() != barMark || str != bar.IdentityDescriptionLong)
            {
              parameter4.Set(barMark);
              if (parameter5 != null && !parameter5.IsReadOnly)
                parameter5.Set(barMark);
              if (parameter6 != null && !parameter6.IsReadOnly)
                parameter6.Set(bar.IdentityDescriptionLong);
            }
          }
        }
      }
    }
  }

  private static void ShowDebugDialog(
    string dialogHeader,
    string dialogMainContent,
    bool showInner)
  {
    TaskDialog taskDialog = new TaskDialog(dialogHeader);
    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    taskDialog.MainContent = dialogMainContent;
    if (!showInner)
      return;
    taskDialog.Show();
  }
}
