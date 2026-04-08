// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.ViewModels.MainViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.FindReferringViews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.ViewModels;

internal class MainViewModel
{
  public List<LegendModel> AllLegends;
  public static List<LegendModel> CheckedLegends = new List<LegendModel>();
  public List<SymbolFamilyModel> AllSymbols;
  public static List<SymbolFamilyModel> CheckedSymbols = new List<SymbolFamilyModel>();
  private static FilteredElementCollector ViewportCollector;

  public MainViewModel()
  {
    MainViewModel.ViewportCollector = new FilteredElementCollector(ActiveModel.UIDoc.Document).OfClass(typeof (Viewport));
    MainViewModel.CheckedLegends = new List<LegendModel>();
    this.AllLegends = new List<LegendModel>();
    LegendModel.LegendList = new List<LegendModel>();
    this.AllLegends = this.GetAllLegends();
    MainViewModel.CheckedSymbols = new List<SymbolFamilyModel>();
    this.AllSymbols = new List<SymbolFamilyModel>();
    this.AllSymbols = this.GetAllSymbols();
  }

  private List<LegendModel> GetAllLegends()
  {
    List<LegendModel> legendModelList = new List<LegendModel>();
    foreach (View legend in new FilteredElementCollector(ActiveModel.Document).OfClass(typeof (View)))
    {
      if (legend.Title.Contains("Legend: "))
        LegendModel.LegendList.Add(new LegendModel(legend));
    }
    LegendModel.LegendList.Sort((Comparison<LegendModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    return LegendModel.LegendList;
  }

  public static List<LegendModel> GetAssociatedViewSheets(List<LegendModel> checkedLegends)
  {
    Document doc = ActiveModel.UIDoc.Document;
    if (checkedLegends.Count == 0)
      return checkedLegends;
    List<LegendModel> legendModelList = checkedLegends;
    List<Viewport> list = MainViewModel.ViewportCollector.Where<Element>((Func<Element, bool>) (viewport => (doc.GetElement((viewport as Viewport).SheetId) as ViewSheet).AssociatedAssemblyInstanceId.IntegerValue != -1)).Select<Element, Viewport>((Func<Element, Viewport>) (viewport => viewport as Viewport)).ToList<Viewport>();
    foreach (LegendModel legend in legendModelList)
    {
      legend.AssociatedViewSheets.Clear();
      foreach (Viewport viewport in list)
      {
        if (viewport.SheetId.IntegerValue != -1 && doc.GetElement(viewport.ViewId).Name.Equals(legend.Name))
        {
          ViewSheetModel viewSheetModel = new ViewSheetModel(doc.GetElement(viewport.SheetId) as ViewSheet, legend);
          legend.AssociatedViewSheets.Add(viewSheetModel);
        }
      }
      legend.AssociatedViewSheets.Sort((Comparison<ViewSheetModel>) ((x, y) => string.Compare(x.Name, y.Name)));
      legend.AssociatedViewSheets = legend.AssociatedViewSheets.Distinct<ViewSheetModel>().ToList<ViewSheetModel>();
    }
    return checkedLegends;
  }

  private List<SymbolFamilyModel> GetAllSymbols()
  {
    Dictionary<string, List<FamilySymbol>> dictionary = new FilteredElementCollector(ActiveModel.UIDoc.Document).OfClass(typeof (FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericAnnotation).Cast<FamilySymbol>().GroupBy<FamilySymbol, string>((Func<FamilySymbol, string>) (e => e.Family.Name)).ToDictionary<IGrouping<string, FamilySymbol>, string, List<FamilySymbol>>((Func<IGrouping<string, FamilySymbol>, string>) (e => e.Key), (Func<IGrouping<string, FamilySymbol>, List<FamilySymbol>>) (e => e.ToList<FamilySymbol>()));
    List<SymbolFamilyModel> allSymbols = new List<SymbolFamilyModel>();
    foreach (KeyValuePair<string, List<FamilySymbol>> keyValuePair in dictionary)
    {
      SymbolFamilyModel parent = new SymbolFamilyModel(keyValuePair.Key);
      foreach (FamilySymbol symbol in keyValuePair.Value)
      {
        string name = symbol.Name;
        SymbolTypeModel symbolTypeModel = new SymbolTypeModel(symbol, parent);
        parent.SymbolTypeList.Add(symbolTypeModel);
      }
      allSymbols.Add(parent);
    }
    allSymbols.Sort((Comparison<SymbolFamilyModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    return allSymbols;
  }

  public static List<SymbolFamilyModel> GetAssociatedViewSheets(
    List<SymbolFamilyModel> checkedSymbolFamilies)
  {
    Document document = ActiveModel.UIDoc.Document;
    if (checkedSymbolFamilies.Count == 0)
      return checkedSymbolFamilies;
    FilteredElementCollector source = new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericAnnotation);
    List<ViewSheet> list = new FilteredElementCollector(document).OfClass(typeof (ViewSheet)).Where<Element>((Func<Element, bool>) (viewSheet => (viewSheet as ViewSheet).AssociatedAssemblyInstanceId.IntegerValue != -1)).Select<Element, ViewSheet>((Func<Element, ViewSheet>) (viewSheet => viewSheet as ViewSheet)).ToList<ViewSheet>();
    document.GetWorksetTable();
    List<SymbolFamilyModel> associatedViewSheets = checkedSymbolFamilies;
    foreach (SymbolFamilyModel symbolFamilyModel in associatedViewSheets)
    {
      foreach (SymbolTypeModel checkedSymbol in symbolFamilyModel.CheckedSymbols)
      {
        SymbolTypeModel symbolTypeInfo = checkedSymbol;
        symbolTypeInfo.AssociatedViewSheets.Clear();
        foreach (AnnotationSymbol symbol in source.Where<Element>((Func<Element, bool>) (annotationSymbol => annotationSymbol.Name.Equals(symbolTypeInfo.Name))).Select<Element, AnnotationSymbol>((Func<Element, AnnotationSymbol>) (annotationSymbol => annotationSymbol as AnnotationSymbol)).ToList<AnnotationSymbol>())
        {
          if (symbol != null)
          {
            string name = document.GetElement(symbol.OwnerViewId).Name;
            foreach (ViewSheet viewSheet in list)
            {
              if (viewSheet.Name.Equals(name))
              {
                ViewSheetModel newViewSheetInfo = new ViewSheetModel(viewSheet, symbol);
                if (symbolTypeInfo.AssociatedViewSheets.FindIndex((Predicate<ViewSheetModel>) (s => s.Name == newViewSheetInfo.Name)) == -1)
                  symbolTypeInfo.AssociatedViewSheets.Add(newViewSheetInfo);
              }
            }
          }
        }
      }
    }
    return associatedViewSheets;
  }
}
