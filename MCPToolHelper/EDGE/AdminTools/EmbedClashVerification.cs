// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.EmbedClashVerification
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class EmbedClashVerification : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (!document.ActiveView.ViewType.ToString().Equals("ThreeD"))
    {
      new TaskDialog("View Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "The current view is not a 3D view.  Please open a 3D view and run this tool again."
      }.Show();
      return (Result) 1;
    }
    if (StructuralFraming.GetAllElements().Count<Element>().Equals(0))
    {
      new TaskDialog("Notice")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "No Structural Members were found in this model."
      }.Show();
      return (Result) 1;
    }
    IEnumerable<Element> elements1 = (IEnumerable<Element>) StructuralFraming.RefineNestedFamilies((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document), document);
    ICollection<ElementId> list1 = (ICollection<ElementId>) Components.GetFilteredElements(new List<string>()
    {
      "PRODUCT BY OTHERS",
      "LIFTING",
      "CGRID",
      "SHEARGRID",
      "MESH",
      "WWF"
    }, (IEnumerable<Element>) null, document).Concat<ElementId>((IEnumerable<ElementId>) Components.GetEmbedIds(document)).ToList<ElementId>();
    ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
    using (Transaction transaction = new Transaction(document, "Embed Clash Verification"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (Element element in elements1)
        {
          BoundingBoxXYZ boundingBoxXyz = element.get_BoundingBox(document.ActiveView);
          Outline outline = new Outline(boundingBoxXyz.Min.Subtract(new XYZ(5.0, 5.0, 5.0)), boundingBoxXyz.Max.Add(new XYZ(5.0, 5.0, 5.0)));
          LogicalOrFilter filter1 = new LogicalOrFilter((ElementFilter) new BoundingBoxIntersectsFilter(outline), (ElementFilter) new BoundingBoxIsInsideFilter(outline));
          ElementIntersectsElementFilter filter2 = new ElementIntersectsElementFilter(element);
          FilteredElementCollector source = new FilteredElementCollector(document, list1).WherePasses((ElementFilter) filter1);
          source.WherePasses((ElementFilter) filter2);
          IEnumerable<ElementId> second = source.Where<Element>((Func<Element, bool>) (e => !this.hardwareDetailCheck(e))).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id));
          elementIds1 = (ICollection<ElementId>) elementIds1.Concat<ElementId>(second).ToList<ElementId>();
        }
        ICollection<ElementId> list2 = (ICollection<ElementId>) elementIds1.GroupBy<ElementId, ElementId>((Func<ElementId, ElementId>) (id => id)).SelectMany<IGrouping<ElementId, ElementId>, ElementId>((Func<IGrouping<ElementId, ElementId>, IEnumerable<ElementId>>) (group => group.Skip<ElementId>(1))).ToList<ElementId>().Distinct<ElementId>().ToList<ElementId>();
        if (list2.Count > 0)
        {
          if (new TaskDialog("Warning")
          {
            MainInstruction = $"Warning: There are {list2.Count} elements that intersect more than one Structural Framing element.\n\nIsolate these elements?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show() == 6)
          {
            activeUiDocument.ActiveView.IsolateElementsTemporary(list2);
            activeUiDocument.Selection.SetElementIds(list2);
          }
        }
        else
          new TaskDialog("Embed Clash Verification")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "All elements intersect only one structural framing element."
          }.Show();
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("The input collection of ids was empty, or its contents were not valid for iteration."))
          return (Result) 1;
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  private bool hardwareDetailCheck(Element elem)
  {
    if (Parameters.GetParameterAsBool(elem, "HARDWARE_DETAIL"))
      return true;
    if (elem.HasSuperComponent())
    {
      Element superComponent = elem.GetSuperComponent();
      if (superComponent != null)
        return this.hardwareDetailCheck(superComponent);
    }
    return false;
  }
}
