// Decompiled with JetBrains decompiler
// Type: EDGE.SelectionAndPinningTools.SelectAllComponents
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
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SelectionAndPinningTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class SelectAllComponents : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document doc = activeUiDocument.Document;
    ICollection<ElementId> list = (ICollection<ElementId>) Components.GetAllEDGEAddonOrEmbedElements(doc).Except<Element>(Components.GetVoidIds(doc).Select<ElementId, Element>((Func<ElementId, Element>) (id => doc.GetElement(id)))).Select<Element, ElementId>((Func<Element, ElementId>) (component => component.Id)).ToList<ElementId>();
    activeUiDocument.Selection.SetElementIds(list);
    return (Result) 1;
  }
}
