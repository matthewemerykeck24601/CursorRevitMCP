// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketViews.ViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.CopyTicketViews;

internal class ViewModel
{
  private UIDocument uiDoc;

  public List<ViewSheetModel> ViewSheets { get; set; }

  public static List<ViewSheetModel> SelectedViewSheets { get; set; }

  public ICollection<ElementId> SelectedIdList { get; set; }

  public ICommand Paste { get; set; }

  public ICommand Close { get; set; }

  public ViewModel(UIDocument uiDoc, ICollection<ElementId> selectedIdList, bool hardwareDetail)
  {
    this.uiDoc = uiDoc;
    this.ViewSheets = new List<ViewSheetModel>();
    this.ViewSheets = this.GetAllViewSheets(hardwareDetail);
    ViewModel.SelectedViewSheets = new List<ViewSheetModel>();
    this.SelectedIdList = selectedIdList;
    this.Close = (ICommand) new Command(new Command.ICommandOnExecute(this.ExecuteClose), new Command.ICommandOnCatExecute(this.CanExecuteClose));
    this.Paste = (ICommand) new Command(new Command.ICommandOnExecute(this.ExecutePaste), new Command.ICommandOnCatExecute(this.CanExecutePaste));
  }

  private List<ViewSheetModel> GetAllViewSheets(bool hardwareDetail)
  {
    Document doc = this.uiDoc.Document;
    List<ViewSheetModel> list = new FilteredElementCollector(doc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId != (ElementId) null && s.AssociatedAssemblyInstanceId != ElementId.InvalidElementId && Parameters.GetParameterAsBool((Element) (doc.GetElement(s.AssociatedAssemblyInstanceId) as AssemblyInstance), "HARDWARE_DETAIL") == hardwareDetail)).Select<ViewSheet, ViewSheetModel>((Func<ViewSheet, ViewSheetModel>) (viewSheet => new ViewSheetModel(viewSheet))).ToList<ViewSheetModel>();
    list.Sort((Comparison<ViewSheetModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    return list;
  }

  public static void SortSelection(
    Document revitDoc,
    ICollection<ElementId> selectedIdList,
    out ICollection<ElementId> symbolIdList,
    out ICollection<ElementId> legendIdList)
  {
    symbolIdList = (ICollection<ElementId>) new List<ElementId>();
    legendIdList = (ICollection<ElementId>) new List<ElementId>();
    foreach (ElementId selectedId in (IEnumerable<ElementId>) selectedIdList)
    {
      if (revitDoc.GetElement(selectedId) is Viewport)
        legendIdList.Add(selectedId);
      else if (revitDoc.GetElement(selectedId) is AnnotationSymbol)
        symbolIdList.Add(selectedId);
    }
  }

  private void SortSelection(
    ICollection<ElementId> selectedIdList,
    out ICollection<ElementId> symbolIdList,
    out ICollection<ElementId> legendIdList)
  {
    Document document = this.uiDoc.Document;
    symbolIdList = (ICollection<ElementId>) new List<ElementId>();
    legendIdList = (ICollection<ElementId>) new List<ElementId>();
    foreach (ElementId selectedId in (IEnumerable<ElementId>) selectedIdList)
    {
      if (document.GetElement(selectedId) is Viewport)
        legendIdList.Add(selectedId);
      else if (document.GetElement(selectedId) is AnnotationSymbol)
        symbolIdList.Add(selectedId);
    }
  }

  public void PasteToViews(ICollection<ElementId> selectedLegendsAndSymbols)
  {
    Document doc = this.uiDoc.Document;
    ICollection<ElementId> symbolIdList = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> legendIdList = (ICollection<ElementId>) new List<ElementId>();
    using (Transaction transaction = new Transaction(doc, "Copy Ticket views"))
    {
      int num1 = (int) transaction.Start();
      this.SortSelection(selectedLegendsAndSymbols, out symbolIdList, out legendIdList);
      foreach (ViewSheetModel selectedViewSheet in ViewModel.SelectedViewSheets)
      {
        ICollection<ElementId> elementsToCopy = (ICollection<ElementId>) new List<ElementId>();
        if (symbolIdList.Count != 0)
        {
          FilteredElementCollector source = new FilteredElementCollector(doc, selectedViewSheet.ViewSheet.Id).OfClass(typeof (FamilyInstance));
          if (source.Count<Element>() > 0)
          {
            foreach (ElementId elementId in (IEnumerable<ElementId>) symbolIdList)
            {
              ElementId symbolId = elementId;
              if (source.Where<Element>((Func<Element, bool>) (symbol => symbol is AnnotationSymbol && (symbol as AnnotationSymbol).Name.Equals((doc.GetElement(symbolId) as AnnotationSymbol).Name) && ((symbol as AnnotationSymbol).Location as LocationPoint).Point.IsAlmostEqualTo(((doc.GetElement(symbolId) as AnnotationSymbol).Location as LocationPoint).Point))).Select<Element, ElementId>((Func<Element, ElementId>) (symbol => (symbol as AnnotationSymbol).Id)).ToList<ElementId>().Count == 0)
                elementsToCopy.Add(symbolId);
            }
          }
          if (elementsToCopy.Count > 0)
            ElementTransformUtils.CopyElements(doc.ActiveView, elementsToCopy, (View) selectedViewSheet.ViewSheet, Transform.Identity, new CopyPasteOptions());
        }
        else
          break;
      }
      foreach (ElementId id in (IEnumerable<ElementId>) legendIdList)
      {
        Viewport element = doc.GetElement(id) as Viewport;
        XYZ boxCenter = element.GetBoxCenter();
        ElementId viewId = element.ViewId;
        string name = (doc.GetElement(viewId) as View).Name;
        foreach (ViewSheetModel selectedViewSheet in ViewModel.SelectedViewSheets)
        {
          try
          {
            ViewSheet viewSheet = selectedViewSheet.ViewSheet;
            string str = $"{viewSheet.SheetNumber} - {viewSheet.Name}";
            Viewport viewport = Viewport.Create(doc, viewSheet.Id, viewId, boxCenter);
            doc.GetElement(viewport.Id).ChangeTypeId(element.GetTypeId());
          }
          catch (Exception ex)
          {
            if (!ex.ToString().Contains("viewId cannot be added to the ViewSheet"))
              break;
          }
        }
      }
      int num2 = (int) transaction.Commit();
    }
  }

  public bool CanExecuteClose(object parameter) => true;

  public void ExecuteClose(object parameter)
  {
    if (!(parameter is MainWindow))
      return;
    (parameter as MainWindow).Close();
  }

  public bool CanExecutePaste(object parameter) => ViewModel.SelectedViewSheets.Count > 0;

  public void ExecutePaste(object parameter)
  {
    this.PasteToViews(this.SelectedIdList);
    if (!(parameter is MainWindow))
      return;
    (parameter as MainWindow).Close();
  }
}
