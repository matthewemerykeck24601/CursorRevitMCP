// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.MultiVoidTools.MultiVoidCutUncutTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing.MultiVoidTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class MultiVoidCutUncutTest : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (new Transaction(document, "Multi Void Cut/Uncut Test"))
    {
      try
      {
        TaskDialog taskDialog = new TaskDialog("Select Type");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Cut");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Uncut");
        List<ElementId> voidIds = Components.GetVoidIds(document).ToList<ElementId>();
        if (taskDialog.Show() == 1001)
        {
          ICollection<ElementId> source = References.PickNewReferences(activeUiDocument, (ISelectionFilter) new OnlyVoids(voidIds), "Select Voids");
          if (source == null)
            return (Result) 1;
          EDGE.__Testing.MultiVoidTools.MultiVoidTools.CutElements(source.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elem => new List<ElementId>()
          {
            elem.Id
          }.Concat<ElementId>(elem.GetSubComponentIds()))).Select<ElementId, Element>(new Func<ElementId, Element>(ActiveModel.Document.GetElement)).Where<Element>((Func<Element, bool>) (voidElem => voidIds.Contains(voidElem.Id))).ToList<Element>(), voidIds);
        }
        else
        {
          ICollection<ElementId> source = References.PickNewReferences(activeUiDocument, (ISelectionFilter) new OnlyVoids(voidIds), "Select Voids");
          if (source == null)
            return (Result) 1;
          EDGE.__Testing.MultiVoidTools.MultiVoidTools.UncutElements(source.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elem => new List<ElementId>()
          {
            elem.Id
          }.Concat<ElementId>(elem.GetSubComponentIds()))).Select<ElementId, Element>(new Func<ElementId, Element>(ActiveModel.Document.GetElement)).Where<Element>((Func<Element, bool>) (voidElem => voidIds.Contains(voidElem.Id))).ToList<Element>(), voidIds);
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex is Autodesk.Revit.Exceptions.OperationCanceledException)
          return (Result) 0;
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
