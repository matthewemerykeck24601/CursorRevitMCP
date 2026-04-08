// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.CopyTicketAnnotation_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CopyTicketAnnotation_Command : IExternalCommand
{
  private UIApplication uiApp;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      this.uiApp = commandData.Application;
      UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
      Document document = activeUiDocument.Document;
      bool flag = false;
      TaskDialog taskDialog = new TaskDialog("EDGE: Copy Ticket Annotation");
      taskDialog.MainInstruction = "Please choose a copy method below:";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Single Copy");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Batch Copy");
      taskDialog.FooterText = $"*For single copy, please make sure you are in a sheet view of the specific ticket that you want to copy annotation to.{Environment.NewLine}*Please note that the following detail views cannot be used as a source or destination for copying annotations between two view.(3D Ortho, Part List)";
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (Result) 1;
      ICollection<ViewSheet> list = (ICollection<ViewSheet>) new FilteredElementCollector(document).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().ToList<ViewSheet>();
      List<ViewSheetItemEntry> viewsheets1 = new List<ViewSheetItemEntry>();
      ViewSheetItemEntry viewSheetItemEntry1 = new ViewSheetItemEntry();
      List<ViewSheetItemEntry> viewSheetItemEntryList = new List<ViewSheetItemEntry>();
      if (taskDialogResult == 1001)
      {
        if (!document.ActiveView.IsAssemblyView || !document.ActiveView.ViewType.ToString().Equals("DrawingSheet"))
        {
          int num = (int) MessageBox.Show("For single copy ticket annotation, you must be in a sheet view of the specific ticket that you want to copy annotation to.", "Warning");
          return (Result) 1;
        }
        ViewSheet activeView = document.ActiveView as ViewSheet;
        foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) list)
        {
          if (viewSheet.IsAssemblyView && viewSheet.Id != activeView.Id)
            viewsheets1.Add(new ViewSheetItemEntry(viewSheet.Id, viewSheet.Title, false, activeUiDocument));
        }
        viewsheets1.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
        ViewSheetCopyFromDialog sheetCopyFromDialog = new ViewSheetCopyFromDialog((IEnumerable<ViewSheetItemEntry>) viewsheets1, activeUiDocument, "byView", "annotations");
        sheetCopyFromDialog.ShowDialog();
        viewSheetItemEntry1 = sheetCopyFromDialog.SelectedItem;
        if (!sheetCopyFromDialog.isContinue)
          return (Result) 1;
        flag = true;
      }
      else if (taskDialogResult == 1002)
      {
        foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) list)
        {
          if (viewSheet.IsAssemblyView)
            viewsheets1.Add(new ViewSheetItemEntry(viewSheet.Id, viewSheet.Title, false, activeUiDocument));
        }
        viewsheets1.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
        ViewSheetCopyFromDialog sheetCopyFromDialog = new ViewSheetCopyFromDialog((IEnumerable<ViewSheetItemEntry>) viewsheets1, activeUiDocument, "byView", "annotations");
        sheetCopyFromDialog.ShowDialog();
        viewSheetItemEntry1 = sheetCopyFromDialog.SelectedItem;
        if (!sheetCopyFromDialog.isContinue)
          return (Result) 1;
        List<ViewSheetItemEntry> viewsheets2 = new List<ViewSheetItemEntry>();
        foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) list)
        {
          if (viewSheet.IsAssemblyView && viewSheetItemEntry1.elemid != viewSheet.Id)
            viewsheets2.Add(new ViewSheetItemEntry(viewSheet.Id, viewSheet.Title, false, activeUiDocument));
        }
        viewsheets2.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
        ViewSheetCopyToDialog sheetCopyToDialog = new ViewSheetCopyToDialog((IEnumerable<ViewSheetItemEntry>) viewsheets2, activeUiDocument, "byView", "annotations");
        sheetCopyToDialog.ShowDialog();
        viewSheetItemEntryList = sheetCopyToDialog.SelectedItems;
        if (sheetCopyToDialog.isContinue)
          flag = true;
      }
      if (flag)
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        StringBuilder stringBuilder = new StringBuilder();
        List<ViewSheet> viewSheetList = new List<ViewSheet>();
        List<List<string>> copiedAll = new List<List<string>>();
        List<List<string>> Notcopied = new List<List<string>>();
        Dictionary<List<string>, List<string>> copiedPartial = new Dictionary<List<string>, List<string>>();
        ViewSheet element1 = document.GetElement(viewSheetItemEntry1.elemid) as ViewSheet;
        string constructionProduct1 = (document.GetElement(element1.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement().GetConstructionProduct();
        if (taskDialogResult == 1001)
        {
          ViewSheet activeView = document.ActiveView as ViewSheet;
          if ((document.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement().GetConstructionProduct().Equals(constructionProduct1))
          {
            this.checkAndCopyDetailView(activeView, element1, document, copiedAll, Notcopied, copiedPartial);
          }
          else
          {
            int num = (int) MessageBox.Show("The corresponding structural framing element in the views you are copying from is not the same as the element in the views you are attempting to copy to. The transaction will be canceled.", "Warning");
            return (Result) 1;
          }
        }
        else
        {
          foreach (ViewSheetItemEntry viewSheetItemEntry2 in viewSheetItemEntryList)
          {
            ViewSheet element2 = document.GetElement(viewSheetItemEntry2.elemid) as ViewSheet;
            string constructionProduct2 = (document.GetElement(element2.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement().GetConstructionProduct();
            if (constructionProduct1.Equals(constructionProduct2))
              this.checkAndCopyDetailView(element2, element1, document, copiedAll, Notcopied, copiedPartial);
            else
              viewSheetList.Add(element2);
          }
        }
      }
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
    }
    return (Result) 0;
  }

  private string getViewLevelName(string name)
  {
    if (name.Contains("Elevation Top"))
      return "Top";
    if (name.Contains("Elevation Left"))
      return "Left";
    if (name.Contains("Elevation Right"))
      return "Right";
    if (name.Contains("Elevation Bottom"))
      return "Bottom";
    if (name.Contains("Elevation Front"))
      return "Front";
    if (name.Contains("Elevation Black"))
      return "Black";
    if (name.Contains("Section A"))
      return "Section A";
    if (name.Contains("Section B"))
      return "Section B";
    if (name.Contains("Plan"))
      return "Plan";
    if (name.Contains("3D Ortho"))
      return "3D Ortho";
    return name.Contains("Part List") ? "Part List" : "";
  }

  private void checkAndCopyDetailView(
    ViewSheet sheetCopyTo,
    ViewSheet sheetCopyFrom,
    Document doc,
    List<List<string>> copiedAll,
    List<List<string>> Notcopied,
    Dictionary<List<string>, List<string>> copiedPartial)
  {
    StringBuilder stringBuilder = new StringBuilder();
    ICollection<ElementId> allViewports1 = sheetCopyTo.GetAllViewports();
    ICollection<ElementId> allViewports2 = sheetCopyFrom.GetAllViewports();
    Dictionary<string, Element> dictionary1 = new Dictionary<string, Element>();
    foreach (ElementId id in (IEnumerable<ElementId>) allViewports1)
    {
      ElementId viewId = (doc.GetElement(id) as Viewport).ViewId;
      Element element = doc.GetElement(viewId);
      string name = element.Name;
      dictionary1.Add(name, element);
    }
    Dictionary<string, Element> dictionary2 = new Dictionary<string, Element>();
    foreach (ElementId id in (IEnumerable<ElementId>) allViewports2)
    {
      ElementId viewId = (doc.GetElement(id) as Viewport).ViewId;
      Element element = doc.GetElement(viewId);
      string name = element.Name;
      dictionary2.Add(name, element);
    }
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    foreach (string key1 in dictionary2.Keys)
    {
      string viewLevelName = this.getViewLevelName(key1);
      if (viewLevelName != "3D Ortho" && viewLevelName != "Part List" && viewLevelName != "")
      {
        bool flag = false;
        foreach (string key2 in dictionary1.Keys)
        {
          if (key2.Contains(viewLevelName))
          {
            flag = true;
            stringList2.Add(key1);
            StringBuilder sb;
            CopyTicketAnnotations.copyTicketAnnotationNew(dictionary2[key1], dictionary1[key2], this.uiApp, out sb);
            stringBuilder.AppendLine($"ElementIds in {key1} :");
            stringBuilder.Append((object) sb).AppendLine();
            break;
          }
        }
        if (!flag)
          stringList1.Add(key1);
      }
    }
    if (stringList1.Count == 0)
      copiedAll.Add(new List<string>()
      {
        sheetCopyFrom.Title,
        sheetCopyTo.Title
      });
    else if (stringList2.Count == 0)
    {
      Notcopied.Add(new List<string>()
      {
        sheetCopyFrom.Title,
        sheetCopyTo.Title
      });
    }
    else
    {
      Dictionary<List<string>, List<string>> dictionary3 = copiedPartial;
      List<string> key = new List<string>();
      key.Add(sheetCopyFrom.Title);
      key.Add(sheetCopyTo.Title);
      List<string> stringList3 = stringList1;
      dictionary3.Add(key, stringList3);
    }
  }
}
