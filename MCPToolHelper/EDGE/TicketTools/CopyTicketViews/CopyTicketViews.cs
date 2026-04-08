// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketViews.CopyTicketViews
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.CopyTicketViews;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CopyTicketViews : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    if (!(activeUiDocument.Document.ActiveView is ViewSheet))
    {
      new TaskDialog("EDGE Error")
      {
        MainInstruction = "Current View is not a Sheet View",
        MainContent = "Copy Ticket Views should be run in a sheet view.  Please switch to a sheet view and re-run the tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    bool hardwareDetail = false;
    ViewSheet activeView = activeUiDocument.Document.ActiveView as ViewSheet;
    if (activeView.AssociatedAssemblyInstanceId != (ElementId) null && activeView.AssociatedAssemblyInstanceId != ElementId.InvalidElementId && document.GetElement(activeView.AssociatedAssemblyInstanceId) is AssemblyInstance element && Utils.ElementUtils.Parameters.GetParameterAsBool((Element) element, "HARDWARE_DETAIL"))
      hardwareDetail = true;
    using (Transaction transaction = new Transaction(document, "Copy Ticket Views"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> selection = this.GetSelection(activeUiDocument);
        if (selection == null)
          return (Result) 1;
        int num2 = (int) transaction.Commit();
        if (selection.Count > 0)
        {
          IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
          new MainWindow(activeUiDocument, selection, mainWindowHandle, hardwareDetail).ShowDialog();
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static bool copyTicketViews(ViewSheet source, ViewSheet target, bool cloneTicketFlag = false)
  {
    Document revitDoc = target.Document;
    List<ElementId> elementIdList = new List<ElementId>();
    List<ElementId> selectedIdList = EDGE.TicketTools.CopyTicketViews.CopyTicketViews.CollectAllLegendsAndSymbols(source);
    ICollection<ElementId> symbolIdList = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> legendIdList = (ICollection<ElementId>) new List<ElementId>();
    ViewModel.SortSelection(revitDoc, (ICollection<ElementId>) selectedIdList, out symbolIdList, out legendIdList);
    ICollection<ElementId> elementsToCopy = (ICollection<ElementId>) new List<ElementId>();
    if (symbolIdList.Count > 0)
    {
      FilteredElementCollector source1 = new FilteredElementCollector(revitDoc).OwnedByView(target.Id).OfClass(typeof (FamilyInstance));
      if (source1.Count<Element>() > 0)
      {
        foreach (ElementId elementId in (IEnumerable<ElementId>) symbolIdList)
        {
          ElementId symbolId = elementId;
          if (source1.Where<Element>((Func<Element, bool>) (symbol => symbol is AnnotationSymbol && (symbol as AnnotationSymbol).Name.Equals((revitDoc.GetElement(symbolId) as AnnotationSymbol).Name) && ((symbol as AnnotationSymbol).Location as LocationPoint).Point.IsAlmostEqualTo(((revitDoc.GetElement(symbolId) as AnnotationSymbol).Location as LocationPoint).Point))).Select<Element, ElementId>((Func<Element, ElementId>) (symbol => (symbol as AnnotationSymbol).Id)).ToList<ElementId>().Count == 0)
            elementsToCopy.Add(symbolId);
        }
      }
      if (elementsToCopy.Count > 0)
        ElementTransformUtils.CopyElements((View) source, elementsToCopy, (View) target, Transform.Identity, new CopyPasteOptions());
    }
    string str1 = "";
    foreach (ElementId id in (IEnumerable<ElementId>) legendIdList)
    {
      Viewport element1 = revitDoc.GetElement(id) as Viewport;
      XYZ boxCenter = element1.GetBoxCenter();
      try
      {
        ViewSheet viewSheet = target;
        Viewport viewport = (Viewport) null;
        if (cloneTicketFlag)
        {
          View element2 = revitDoc.GetElement(element1.ViewId) as View;
          if ((element2.Name.ToUpper().StartsWith("END PATTERN") || element2.Name.ToUpper().StartsWith("STRAND PATTERN") ? 1 : (element2.Name.ToUpper().StartsWith("REINFORCING PATTERN") ? 1 : 0)) != 0)
          {
            List<string> namesToSearch = new List<string>();
            List<string> list1 = new List<ElementId>()
            {
              target.AssociatedAssemblyInstanceId
            }.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (assemblyId => revitDoc.GetElement(assemblyId) as AssemblyInstance)).Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (assembly => assembly.GetStructuralFramingElement())).Select<Element, string>((Func<Element, string>) (sfElem => Utils.ElementUtils.Parameters.GetParameterAsString(sfElem, "DESIGN_NUMBER"))).ToList<string>();
            if (element2.Name.Contains("END PATTERN"))
            {
              foreach (string str2 in list1)
                namesToSearch.Add("END PATTERN " + str2);
            }
            else if (element2.Name.Contains("STRAND PATTERN"))
            {
              foreach (string str3 in list1)
                namesToSearch.Add("STRAND PATTERN " + str3);
            }
            else if (element2.Name.Contains("REINFORCING PATTERN"))
            {
              foreach (string str4 in list1)
                namesToSearch.Add("REINFORCING PATTERN " + str4);
            }
            List<ElementId> list2 = new FilteredElementCollector(revitDoc).OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (v => v.ViewType == ViewType.Legend && namesToSearch.Contains(v.Name))).Select<View, ElementId>((Func<View, ElementId>) (v => v.Id)).ToList<ElementId>();
            if (list2.Count<ElementId>() > 0)
            {
              View element3 = revitDoc.GetElement(list2.FirstOrDefault<ElementId>()) as View;
              viewport = Viewport.Create(revitDoc, viewSheet.Id, element3.Id, boxCenter);
            }
          }
          else
          {
            if (string.IsNullOrWhiteSpace(str1) && source.AssemblyInstanceId != (ElementId) null && revitDoc.GetElement(source.AssociatedAssemblyInstanceId) is AssemblyInstance element4)
              str1 = element4.Name;
            ElementId viewId = element1.ViewId;
            if (!string.IsNullOrWhiteSpace(str1) && element2.Name == "INSULATION DETAIL - " + str1)
            {
              string newLegendName = "";
              if (target.AssemblyInstanceId != (ElementId) null && revitDoc.GetElement(target.AssociatedAssemblyInstanceId) is AssemblyInstance element5)
                newLegendName = "INSULATION DETAIL - " + element5.Name;
              if (!string.IsNullOrWhiteSpace(newLegendName))
              {
                View view = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Views).Cast<View>().Where<View>((Func<View, bool>) (x => x.ViewType == ViewType.Legend && x.Name == newLegendName)).FirstOrDefault<View>();
                if (view != null)
                  viewId = view.Id;
                else
                  continue;
              }
              else
                continue;
            }
            viewport = Viewport.Create(revitDoc, viewSheet.Id, viewId, boxCenter);
          }
        }
        else
          viewport = Viewport.Create(revitDoc, viewSheet.Id, element1.ViewId, boxCenter);
        if (viewport != null)
          revitDoc.GetElement(viewport.Id).ChangeTypeId(element1.GetTypeId());
      }
      catch (Exception ex)
      {
        if (!ex.ToString().Contains("viewId cannot be added to the ViewSheet"))
          return false;
      }
    }
    return true;
  }

  private static List<ElementId> CollectAllLegendsAndSymbols(ViewSheet sheet)
  {
    Document revitDoc = sheet.Document;
    return new FilteredElementCollector(revitDoc, sheet.Id).OfClass(typeof (Viewport)).Where<Element>((Func<Element, bool>) (vp => !(revitDoc.GetElement((vp as Viewport).ViewId) is ViewSection))).ToList<Element>().Union<Element>((IEnumerable<Element>) new FilteredElementCollector(revitDoc, sheet.Id).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (e => e is AnnotationSymbol)).ToList<Element>()).Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>();
  }

  private ICollection<ElementId> GetSelection(UIDocument uidoc)
  {
    ISelectionFilter selFilter = (ISelectionFilter) new OnlyLegendsAndSymbols();
    ICollection<ElementId> source = uidoc.Selection.GetElementIds();
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (source.Count > 0)
    {
      foreach (Element element in source.Select<ElementId, Element>((Func<ElementId, Element>) (s => uidoc.Document.GetElement(s))))
      {
        if (selFilter.AllowElement(element))
          elementIds.Add(element.Id);
      }
      source.Clear();
      foreach (ElementId elementId in (IEnumerable<ElementId>) elementIds)
        source.Add(elementId);
      if (source.Count < 1)
        TaskDialog.Show("Error", "Selected elements are not able to be copied.  Please verify that you have selected copyable views.");
    }
    else
      source = References.PickNewReferences(uidoc, selFilter, "Select the Symbols and Legends to copy.");
    return source;
  }
}
