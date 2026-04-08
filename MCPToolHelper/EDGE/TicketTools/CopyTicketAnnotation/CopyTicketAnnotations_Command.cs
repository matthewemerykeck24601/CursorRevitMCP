// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.CopyTicketAnnotations_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CopyTicketAnnotations_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
      Document document = activeUiDocument.Document;
      using (Transaction transaction = new Transaction(document, "Copy Annotations"))
      {
        int num1 = (int) transaction.Start();
        if (document.IsFamilyDocument)
        {
          new TaskDialog("Copy Ticket Annotations")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Error:  Run inside the Project Environment and not within the Family Editor."
          }.Show();
          return (Result) 1;
        }
        ICollection<AssemblyInstance> list1 = (ICollection<AssemblyInstance>) new FilteredElementCollector(document).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
        List<ViewSheetItemEntry> viewsheets1 = new List<ViewSheetItemEntry>();
        ICollection<ViewSheet> list2 = (ICollection<ViewSheet>) new FilteredElementCollector(document).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().ToList<ViewSheet>();
        foreach (AssemblyInstance assemblyInstance in (IEnumerable<AssemblyInstance>) list1)
        {
          AssemblyInstance assIns = assemblyInstance;
          if (list2.Where<ViewSheet>((Func<ViewSheet, bool>) (vs => vs.AssociatedAssemblyInstanceId == assIns.Id)).ToList<ViewSheet>().Count<ViewSheet>() > 0)
          {
            if (assIns.Name != "")
            {
              viewsheets1.Add(new ViewSheetItemEntry(assIns.Id, assIns.Name, false, activeUiDocument));
            }
            else
            {
              new TaskDialog("Copy Ticket Annotations")
              {
                FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                MainInstruction = "Error:  One or more assemblies in the document do not have names. Please give them names and try again."
              }.Show();
              return (Result) 1;
            }
          }
        }
        viewsheets1.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
        ViewSheetCopyFromDialog sheetCopyFromDialog = new ViewSheetCopyFromDialog((IEnumerable<ViewSheetItemEntry>) viewsheets1, activeUiDocument, "byAssembly", "annotations");
        sheetCopyFromDialog.ShowDialog();
        ViewSheetItemEntry assItemCopyFrom = sheetCopyFromDialog.SelectedItem;
        if (!sheetCopyFromDialog.isContinue)
          return (Result) 1;
        List<ViewSheetItemEntry> viewsheets2 = new List<ViewSheetItemEntry>();
        foreach (AssemblyInstance assemblyInstance in (IEnumerable<AssemblyInstance>) list1)
        {
          AssemblyInstance assIns = assemblyInstance;
          int num2 = list2.Where<ViewSheet>((Func<ViewSheet, bool>) (vs => vs.AssociatedAssemblyInstanceId == assIns.Id)).ToList<ViewSheet>().Count<ViewSheet>();
          if (!assIns.Name.Equals(assItemCopyFrom.elemName) && num2 > 0)
          {
            if (assIns.Name != "")
            {
              viewsheets2.Add(new ViewSheetItemEntry(assIns.Id, assIns.Name, false, activeUiDocument));
            }
            else
            {
              new TaskDialog("Copy Ticket Annotations")
              {
                FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                MainInstruction = "Error:  One or more assemblies in the document do not have names. Please give them names and try again."
              }.Show();
              return (Result) 1;
            }
          }
        }
        viewsheets2.Sort((Comparison<ViewSheetItemEntry>) ((item1, item2) => item1.elemName.CompareTo(item2.elemName)));
        ViewSheetCopyToDialog sheetCopyToDialog = new ViewSheetCopyToDialog((IEnumerable<ViewSheetItemEntry>) viewsheets2, activeUiDocument, "byAssembly", "annotations");
        sheetCopyToDialog.ShowDialog();
        List<ViewSheetItemEntry> selectedItems = sheetCopyToDialog.SelectedItems;
        if (!sheetCopyToDialog.isContinue)
          return (Result) 1;
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        document.GetElement(assItemCopyFrom.elemid);
        ICollection<View> list3 = (ICollection<View>) new FilteredElementCollector(document).OfClass(typeof (View)).Cast<View>().ToList<View>();
        List<View> viewList1 = new List<View>();
        List<View> list4 = list3.Where<View>((Func<View, bool>) (v => v.AssociatedAssemblyInstanceId == assItemCopyFrom.elemid)).ToList<View>();
        Dictionary<string, Element> dictionary1 = new Dictionary<string, Element>();
        foreach (View view in list4)
        {
          string name = view.Name;
          string title = view.Title;
          if (!dictionary1.ContainsKey(name))
            dictionary1.Add(name, (Element) view);
        }
        List<View> viewList2 = new List<View>();
        foreach (ViewSheetItemEntry viewSheetItemEntry in selectedItems)
        {
          ViewSheetItemEntry itemTo = viewSheetItemEntry;
          viewList2.AddRange((IEnumerable<View>) list3.Where<View>((Func<View, bool>) (vs => vs.AssociatedAssemblyInstanceId == itemTo.elemid)).ToList<View>());
        }
        Dictionary<string, List<Element>> dictionary2 = new Dictionary<string, List<Element>>();
        foreach (View view in viewList2)
        {
          string name = view.Name;
          if (dictionary2.ContainsKey(name))
            dictionary2[name].Add((Element) view);
          else
            dictionary2.Add(name, new List<Element>()
            {
              (Element) view
            });
        }
        Dictionary<string, Element>.KeyCollection keys1 = dictionary1.Keys;
        ICollection<string> keys2 = (ICollection<string>) dictionary2.Keys;
        foreach (string key in (IEnumerable<string>) keys1)
        {
          if (keys2.Contains(key))
          {
            List<Element> elementList = dictionary2[key];
            if (document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories.Contains("EdgeRotationLine"))
            {
              foreach (Element viewSheetCopyTo in elementList)
                CopyTicketAnnotations.copyTicketAnnotations(dictionary1[key], viewSheetCopyTo);
            }
            else
            {
              TaskDialog.Show("Copy Ticket Annotations", "Line Style \"EdgeRotationLine\" not found, please run the shared parameter updater and try again.");
              return (Result) 1;
            }
          }
        }
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          int num3 = (int) MessageBox.Show("Not transmitted!");
        }
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
    }
    return (Result) 0;
  }
}
