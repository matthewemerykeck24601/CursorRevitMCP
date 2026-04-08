// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketDimensions.CopyTicketDimensions_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.CopyTicketAnnotation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.CopyTicketDimensions;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CopyTicketDimensions_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
      Document document = activeUiDocument.Document;
      using (Transaction transaction = new Transaction(document, "Copy Dimensions"))
      {
        int num1 = (int) transaction.Start();
        bool flag = false;
        TaskDialog taskDialog = new TaskDialog("EDGE: Copy Ticket Dimensions");
        taskDialog.MainInstruction = "Please choose a copy method below:";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Single Copy");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Batch Copy");
        taskDialog.FooterText = $"*For single copy, please make sure you are in a sheet view of the specific ticket that you want to copy dimensions to.{Environment.NewLine}*Please note that the following detail views cannot be used as a source or destination for copying dimensions between two view.(3D Ortho, Part List)";
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
            int num2 = (int) MessageBox.Show("For single copy ticket annotation, you must be in a sheet view of the specific ticket that you want to copy annotation to.", "Warning");
            return (Result) 1;
          }
          View activeView = document.ActiveView;
          ElementId assemblyInstanceId = activeView.AssociatedAssemblyInstanceId;
          foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) list)
          {
            if (viewSheet.IsAssemblyView && viewSheet.AssociatedAssemblyInstanceId.Equals((object) assemblyInstanceId) && viewSheet.Id != activeView.Id)
              viewsheets1.Add(new ViewSheetItemEntry(viewSheet.Id, viewSheet.Title, false, activeUiDocument));
          }
          viewsheets1.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
          ViewSheetCopyFromDialog sheetCopyFromDialog = new ViewSheetCopyFromDialog((IEnumerable<ViewSheetItemEntry>) viewsheets1, activeUiDocument, "byView", "dimensions");
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
          ViewSheetCopyFromDialog sheetCopyFromDialog = new ViewSheetCopyFromDialog((IEnumerable<ViewSheetItemEntry>) viewsheets1, activeUiDocument, "byView", "dimensions");
          sheetCopyFromDialog.ShowDialog();
          viewSheetItemEntry1 = sheetCopyFromDialog.SelectedItem;
          if (!sheetCopyFromDialog.isContinue)
            return (Result) 1;
          View element = document.GetElement(viewSheetItemEntry1.elemid) as View;
          ElementId assemblyInstanceId = element.AssociatedAssemblyInstanceId;
          List<ViewSheetItemEntry> viewsheets2 = new List<ViewSheetItemEntry>();
          foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) list)
          {
            if (viewSheet.IsAssemblyView && viewSheet.AssociatedAssemblyInstanceId.Equals((object) assemblyInstanceId) && viewSheet.Id != element.Id)
              viewsheets2.Add(new ViewSheetItemEntry(viewSheet.Id, viewSheet.Title, false, activeUiDocument));
          }
          viewsheets2.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
          ViewSheetCopyToDialog sheetCopyToDialog = new ViewSheetCopyToDialog((IEnumerable<ViewSheetItemEntry>) viewsheets2, activeUiDocument, "byView", "dimensions");
          sheetCopyToDialog.ShowDialog();
          viewSheetItemEntryList = sheetCopyToDialog.SelectedItems;
          if (sheetCopyToDialog.isContinue)
            flag = true;
        }
        if (flag)
        {
          App.DialogSwitches.SuspendModelLockingforOperation = true;
          StringBuilder stringBuilder1 = new StringBuilder();
          List<ViewSheet> viewSheetList = new List<ViewSheet>();
          List<List<string>> copiedAll = new List<List<string>>();
          List<List<string>> Notcopied = new List<List<string>>();
          Dictionary<List<string>, List<string>> copiedPartial = new Dictionary<List<string>, List<string>>();
          ViewSheet element1 = document.GetElement(viewSheetItemEntry1.elemid) as ViewSheet;
          string parameterAsString1 = Parameters.GetParameterAsString((Element) (document.GetElement(element1.AssociatedAssemblyInstanceId) as AssemblyInstance), "ASSEMBLY_MARK_NUMBER");
          if (taskDialogResult == 1001)
          {
            ViewSheet activeView = document.ActiveView as ViewSheet;
            string parameterAsString2 = Parameters.GetParameterAsString((Element) (document.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance), "ASSEMBLY_MARK_NUMBER");
            int index1 = this.getIndex(parameterAsString1);
            int index2 = this.getIndex(parameterAsString2);
            if (parameterAsString1.Substring(0, index1).Equals(parameterAsString2.Substring(0, index2)))
            {
              this.checkAndCopyDetailView(activeView, element1, document, copiedAll, Notcopied, copiedPartial);
            }
            else
            {
              int num3 = (int) MessageBox.Show("The corresponding members of the views you chose were not like members. Transection will be cancelled.", "Warning");
            }
          }
          else
          {
            foreach (ViewSheetItemEntry viewSheetItemEntry2 in viewSheetItemEntryList)
            {
              ViewSheet element2 = document.GetElement(viewSheetItemEntry2.elemid) as ViewSheet;
              string parameterAsString3 = Parameters.GetParameterAsString((Element) (document.GetElement(element2.AssociatedAssemblyInstanceId) as AssemblyInstance), "ASSEMBLY_MARK_NUMBER");
              int index3 = this.getIndex(parameterAsString1);
              int index4 = this.getIndex(parameterAsString3);
              if (parameterAsString1.Substring(0, index3).Equals(parameterAsString3.Substring(0, index4)))
                this.checkAndCopyDetailView(element2, element1, document, copiedAll, Notcopied, copiedPartial);
              else
                viewSheetList.Add(element2);
            }
          }
          if (copiedAll.Count > 0)
          {
            stringBuilder1.AppendLine($"We've copied all the ticket sheet dimensions from {element1.Title} to:");
            foreach (List<string> stringList in copiedAll)
              stringBuilder1.AppendLine(stringList[1]);
            stringBuilder1.AppendLine();
          }
          if (copiedPartial.Count > 0)
          {
            stringBuilder1.AppendLine($"We've copied part of the ticket sheet dimensions from {element1.Title} to:");
            foreach (List<string> key in (IEnumerable<List<string>>) copiedPartial.Keys)
            {
              stringBuilder1.AppendLine(key[1]);
              List<string> stringList = copiedPartial[key];
              StringBuilder stringBuilder2 = new StringBuilder();
              if (stringList.Count > 1)
                stringBuilder2.AppendLine($"The following detail views from {key[0]}{Environment.NewLine}don't have corresponding detail views in the sheet you chose to copy to: ");
              else
                stringBuilder2.AppendLine($"The following detail view from {key[0]}{Environment.NewLine}doesn't have corresponding detail view in the sheet you chose to copy to: ");
              foreach (string str in stringList)
                stringBuilder2.AppendLine(str);
              stringBuilder1.AppendLine(stringBuilder2.ToString());
            }
            stringBuilder1.AppendLine();
          }
          if (Notcopied.Count > 0)
          {
            stringBuilder1.AppendLine($"None of the ticket sheet dimensions from {element1.Title}{Environment.NewLine}have been copied to the following sheets since none of the {Environment.NewLine}detail views have a corresponding detail view in the sheet you chose to copy to:");
            foreach (List<string> stringList in Notcopied)
              stringBuilder1.AppendLine(stringList[1]);
          }
          if (viewSheetList.Count > 0)
          {
            stringBuilder1.AppendLine($"None of the ticket sheet dimensions from {element1.Title}{Environment.NewLine}have been copied to the following sheets since the corresponding element{Environment.NewLine}members of the detail views are not like members: ");
            foreach (ViewSheet viewSheet in viewSheetList)
              stringBuilder1.AppendLine(viewSheet.Title);
          }
          IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
          new Message(stringBuilder1.ToString(), mainWindowHandle).Show();
        }
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          int num4 = (int) MessageBox.Show("Not transmitted!");
        }
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
    }
    return (Result) 0;
  }

  private void checkAndCopyDetailView(
    ViewSheet sheetCopyTo,
    ViewSheet sheetCopyFrom,
    Document doc,
    List<List<string>> copiedAll,
    List<List<string>> Notcopied,
    Dictionary<List<string>, List<string>> copiedPartial)
  {
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
            EDGE.TicketTools.CopyTicketDimensions.CopyTicketDimensions.copyTicketDimensions(dictionary2[key1], dictionary1[key2]);
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

  private int getIndex(string str)
  {
    char[] charArray = str.ToCharArray();
    int length = str.Length;
    int index = 0;
    while (index < length && !char.IsDigit(charArray[index]))
      ++index;
    return index;
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
}
