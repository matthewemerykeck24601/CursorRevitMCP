// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.IntersectsElementFilter_Test
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
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class IntersectsElementFilter_Test : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      Document document = commandData.Application.ActiveUIDocument.Document;
      using (Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "Journaling"))
      {
        if (transaction.Start() != TransactionStatus.Started)
        {
          message = "EDGE: Unable to start transaction.";
          return (Result) -1;
        }
        Element element = document.GetElement(new ElementId(750296));
        ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>()
        {
          new ElementId(750445)
        };
        if (new FilteredElementCollector(document, elementIds).WherePasses((ElementFilter) new ElementIntersectsElementFilter(element, false)).ToList<Element>().Count<Element>() == 0)
        {
          int num = (int) MessageBox.Show("Not intersected!");
        }
        if (transaction.Commit() == TransactionStatus.Committed)
          return (Result) 0;
        message = "EDGE: Unable to commit transaction.";
        return (Result) -1;
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
      return (Result) -1;
    }
  }

  private Element GetFlat(Document doc, Element structFramingElem)
  {
    try
    {
      Element element1 = (Element) null;
      List<ElementId> list = structFramingElem.GetSubComponentIds().ToList<ElementId>();
      if (list.Any<ElementId>())
      {
        Element element2 = list.Select(subComponent => new
        {
          subComponent = subComponent,
          elem = doc.GetElement(subComponent)
        }).Select(_param1 => new
        {
          \u003C\u003Eh__TransparentIdentifier0 = _param1,
          name = _param1.elem.Name.ToUpper()
        }).Where(_param1 => _param1.name.Contains("FLAT (DO NOT USE)")).Select(_param1 => _param1.\u003C\u003Eh__TransparentIdentifier0.elem).SingleOrDefault<Element>();
        if (element2 != null)
          element1 = element2;
      }
      return element1 ?? structFramingElem;
    }
    catch (Exception ex)
    {
      return (Element) null;
    }
  }
}
