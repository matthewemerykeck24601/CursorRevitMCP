// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.OldCopyTicketViews
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using Utils.Forms;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class OldCopyTicketViews : IExternalCommand
{
  private static ViewSheet[] SheetArray;
  private static string[] NameArray;
  private static string[] OrderedNameArray;
  private static string CheckedName = "";

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    UIApplication application = commandData.Application;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    ICollection<ElementId> elementsToCopy = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
    using (Transaction transaction = new Transaction(document, "Copy Ticket Views"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ISelectionFilter selFilter = (ISelectionFilter) new OnlyLegendsAndSymbols();
        ICollection<ElementId> elementIds2 = activeUiDocument.Selection.GetElementIds();
        if (elementIds2.Count == 0)
          elementIds2 = References.PickNewReferences(activeUiDocument, selFilter, "Select the Symbols and Legends to copy.");
        if (elementIds2.Count == 0)
          return (Result) 1;
        OldCopyTicketViews.GetAllViews();
        CheckBoxSelectionForm boxSelectionForm = new CheckBoxSelectionForm();
        boxSelectionForm.checkedListBox1.Items.AddRange((object[]) OldCopyTicketViews.OrderedNameArray);
        int num2 = (int) boxSelectionForm.ShowDialog();
        ICollection<int> ints = (ICollection<int>) new List<int>();
        foreach (int checkedIndex in boxSelectionForm.checkedListBox1.CheckedIndices)
        {
          OldCopyTicketViews.CheckedName = boxSelectionForm.checkedListBox1.Items[checkedIndex].ToString();
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          int index = Array.FindIndex<string>(OldCopyTicketViews.NameArray, OldCopyTicketViews.\u003C\u003EO.\u003C0\u003E__NameIsEqual ?? (OldCopyTicketViews.\u003C\u003EO.\u003C0\u003E__NameIsEqual = new Predicate<string>(OldCopyTicketViews.NameIsEqual)));
          ints.Add(index);
        }
        ICollection<ViewSheet> viewSheets = (ICollection<ViewSheet>) new List<ViewSheet>();
        foreach (int index in (IEnumerable<int>) ints)
          viewSheets.Add(OldCopyTicketViews.SheetArray[index]);
        foreach (ElementId id in (IEnumerable<ElementId>) elementIds2)
        {
          if (document.GetElement(id) is Viewport)
            elementIds1.Add(id);
          else if (document.GetElement(id) is AnnotationSymbol)
            elementsToCopy.Add(id);
        }
        foreach (ViewSheet destinationView in (IEnumerable<ViewSheet>) viewSheets)
        {
          if (elementsToCopy.Count != 0)
            ElementTransformUtils.CopyElements(document.ActiveView, elementsToCopy, (View) destinationView, Transform.Identity, new CopyPasteOptions());
          else
            break;
        }
        foreach (ElementId id in (IEnumerable<ElementId>) elementIds1)
        {
          Viewport element = document.GetElement(id) as Viewport;
          XYZ boxCenter = element.GetBoxCenter();
          ElementId viewId = element.ViewId;
          string name = (document.GetElement(viewId) as View).Name;
          foreach (ViewSheet viewSheet1 in (IEnumerable<ViewSheet>) viewSheets)
          {
            try
            {
              ViewSheet viewSheet2 = viewSheet1;
              string str = $"{viewSheet2.SheetNumber} - {viewSheet2.Name}";
              Viewport viewport = Viewport.Create(document, viewSheet2.Id, viewId, boxCenter);
              document.GetElement(viewport.Id).ChangeTypeId(element.GetTypeId());
            }
            catch (Exception ex)
            {
              if (!ex.ToString().Contains("viewId cannot be added to the ViewSheet"))
                break;
            }
          }
        }
        int num3 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        int num = (int) transaction.RollBack();
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static void GetAllViews()
  {
    FilteredElementCollector elementCollector = new FilteredElementCollector(ActiveModel.UIDoc.Document).OfClass(typeof (ViewSheet));
    ICollection<ViewSheet> viewSheets = (ICollection<ViewSheet>) new List<ViewSheet>();
    foreach (ViewSheet viewSheet in elementCollector)
    {
      if (viewSheet.AssociatedAssemblyInstanceId.IntegerValue != -1)
        viewSheets.Add(viewSheet);
    }
    OldCopyTicketViews.NameArray = new string[viewSheets.Count];
    OldCopyTicketViews.SheetArray = new ViewSheet[viewSheets.Count];
    OldCopyTicketViews.OrderedNameArray = new string[viewSheets.Count];
    int index = 0;
    foreach (ViewSheet viewSheet in (IEnumerable<ViewSheet>) viewSheets)
    {
      OldCopyTicketViews.NameArray[index] = $"{viewSheet.SheetNumber.ToString()} - {viewSheet.Name}";
      OldCopyTicketViews.OrderedNameArray[index] = $"{viewSheet.SheetNumber.ToString()} - {viewSheet.Name}";
      OldCopyTicketViews.SheetArray[index] = viewSheet;
      ++index;
    }
    Array.Sort<string>(OldCopyTicketViews.OrderedNameArray);
  }

  private static bool NameIsEqual(string arrayElement)
  {
    return arrayElement.Equals(OldCopyTicketViews.CheckedName);
  }
}
