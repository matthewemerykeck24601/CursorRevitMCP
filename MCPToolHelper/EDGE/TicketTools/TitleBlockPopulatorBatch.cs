// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TitleBlockPopulatorBatch
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TitleBlockPopulatorBatch : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (commandData.Application.ActiveUIDocument.Document == null)
    {
      int num = (int) MessageBox.Show("Failed to run tool. Unable to retrieve current view sheets in the project.");
      return (Result) 1;
    }
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Batch Title Block Populator must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Batch Title Block Populator must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    IList<Element> elements1 = new FilteredElementCollector(document).WherePasses((ElementFilter) new ElementClassFilter(typeof (ViewSheet))).ToElements();
    List<ViewSheet> source = new List<ViewSheet>();
    foreach (ViewSheet viewSheet in (IEnumerable<Element>) elements1)
    {
      if (viewSheet.IsAssemblyView)
        source.Add(viewSheet);
    }
    Dictionary<string, List<ViewSheet>> dictionary = new Dictionary<string, List<ViewSheet>>();
    foreach (ViewSheet viewSheet in source.ToList<ViewSheet>())
    {
      ElementId assemblyInstanceId = viewSheet.AssociatedAssemblyInstanceId;
      Element element = document.GetElement(assemblyInstanceId);
      if (element == null || Utils.ElementUtils.Parameters.GetParameterAsBool(element, "HARDWARE_DETAIL") || (element as AssemblyInstance).GetStructuralFramingElement() == null)
      {
        source.Remove(viewSheet);
      }
      else
      {
        string name = element.Name;
        if (dictionary.ContainsKey(name))
        {
          dictionary[name].Add(viewSheet);
        }
        else
        {
          List<ViewSheet> viewSheetList = new List<ViewSheet>()
          {
            viewSheet
          };
          dictionary.Add(name, viewSheetList);
        }
      }
    }
    List<HierarchyCheckBoxItems> hierarchyCheckBoxItemsList1 = new List<HierarchyCheckBoxItems>();
    foreach (KeyValuePair<string, List<ViewSheet>> keyValuePair in dictionary)
    {
      List<string> sheetNames = new List<string>();
      foreach (View view in keyValuePair.Value)
      {
        string str = view.Title.Replace(keyValuePair.Key + ":", "").Trim();
        sheetNames.Add(str);
      }
      hierarchyCheckBoxItemsList1.Add(new HierarchyCheckBoxItems(keyValuePair.Key, sheetNames));
    }
    List<HierarchyCheckBoxItems> hierarchyCheckBoxItemsList2 = new List<HierarchyCheckBoxItems>();
    List<ParentClass> items = new List<ParentClass>()
    {
      new ParentClass(hierarchyCheckBoxItemsList1)
    };
    if (hierarchyCheckBoxItemsList1.Count<HierarchyCheckBoxItems>() == 0)
    {
      new TaskDialog("Batch Ticket Title Block Populator")
      {
        MainContent = "There are no assembly sheets in the project. Please create an assembly sheet manually or with an EDGE^R ticket generation tool, and try to run again."
      }.Show();
      return (Result) 1;
    }
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    BatchTitleBlockPopulator titleBlockPopulator = new BatchTitleBlockPopulator(items, mainWindowHandle);
    if (!titleBlockPopulator.ShowDialog().GetValueOrDefault())
      return (Result) 1;
    List<ParentClass> returnedList = titleBlockPopulator.getReturnedList();
    if (returnedList.Count == 1)
      hierarchyCheckBoxItemsList2 = returnedList[0].Members;
    if (hierarchyCheckBoxItemsList2 == null)
      return (Result) 1;
    List<ViewSheet> viewSheetList1 = new List<ViewSheet>();
    foreach (HierarchyCheckBoxItems hierarchyCheckBoxItems in hierarchyCheckBoxItemsList2)
    {
      foreach (Sheets member in hierarchyCheckBoxItems.Members)
      {
        if (member.CheckedSheetBool)
        {
          foreach (ViewSheet viewSheet in source)
          {
            if (viewSheet.Title.ToUpper().Contains(hierarchyCheckBoxItems.AssemblyName.ToUpper()) && viewSheet.Title.ToUpper().Contains(member.SheetName.ToUpper()))
            {
              viewSheetList1.Add(viewSheet);
              break;
            }
          }
        }
      }
    }
    using (Transaction transaction = new Transaction(activeUiDocument.Document, "Batch Title Block Populator"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        List<Element> elementList1 = new List<Element>();
        List<Element> elementList2 = new List<Element>();
        bool flag = true;
        foreach (ViewSheet sheetView in viewSheetList1)
        {
          App.DialogSwitches.SuspendModelLockingforOperation = true;
          if (sheetView == null || sheetView.AssociatedAssemblyInstanceId == ElementId.InvalidElementId || sheetView.AssociatedAssemblyInstanceId == (ElementId) null)
          {
            new TaskDialog("EDGE Error")
            {
              MainInstruction = "View Selected is not an Assembly Sheet View",
              MainContent = "A View that is not an Assembly Sheet View Should not be displayed in the Batch Ticket Populator Window.",
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show();
            int num2 = (int) transaction.RollBack();
            return (Result) 1;
          }
          List<Element> invalidWeightInputsOut = new List<Element>();
          List<Element> weightParametesDoNotExistOut = new List<Element>();
          Result result;
          if (flag)
          {
            result = TitleblockPopCore.PopulateTicketTitleBlock(activeUiDocument, ref message, sheetView, out invalidWeightInputsOut, out weightParametesDoNotExistOut);
            flag = false;
          }
          else
            result = TitleblockPopCore.PopulateTicketTitleBlock(activeUiDocument, ref message, sheetView, out invalidWeightInputsOut, out weightParametesDoNotExistOut, false);
          if (invalidWeightInputsOut.Count > 0)
          {
            foreach (Element element in invalidWeightInputsOut)
            {
              if (!elementList1.Contains(element))
                elementList1.Add(element);
            }
          }
          if (weightParametesDoNotExistOut.Count > 0)
          {
            foreach (Element element in weightParametesDoNotExistOut)
            {
              if (!elementList2.Contains(element))
                elementList2.Add(element);
            }
          }
          if (result != null)
          {
            int num3 = (int) transaction.RollBack();
            return result;
          }
        }
        if (elementList2.Count > 0)
        {
          TaskDialog taskDialog = new TaskDialog("Batch Ticket Title Block Populator Warning");
          taskDialog.MainContent = "Neither the UNIT_WEIGHT or WEIGHT_PER_UNIT parameters exist for one or more of the elements (shown below). This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
          taskDialog.ExpandedContent += "Elements:\n";
          List<string> stringList = new List<string>();
          foreach (Element elem in elementList2)
          {
            string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
            if (!stringList.Contains(str))
              stringList.Add(str);
          }
          stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          foreach (string str in stringList)
            taskDialog.ExpandedContent += str;
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
          if (taskDialog.Show() != 1)
          {
            int num4 = (int) transaction.RollBack();
            return (Result) 1;
          }
        }
        if (elementList1.Count > 0)
        {
          TaskDialog taskDialog = new TaskDialog("Batch Ticket Title Block Populator Warning");
          taskDialog.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
          taskDialog.ExpandedContent += "Elements:\n";
          List<string> stringList = new List<string>();
          foreach (Element elem in elementList1)
          {
            string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
            if (!stringList.Contains(str))
              stringList.Add(str);
          }
          stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          foreach (string str in stringList)
            taskDialog.ExpandedContent += str;
          if (taskDialog.Show() != 1)
          {
            int num5 = (int) transaction.RollBack();
            return (Result) 1;
          }
        }
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Error", "Exception in TitleBlock Populator.  Please Contact Support: " + ex.Message);
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        if (transaction.HasStarted() && transaction.Commit() != TransactionStatus.Committed)
          TaskDialog.Show("Error", "Transaction didn't committed!");
      }
    }
    return (Result) 0;
  }
}
