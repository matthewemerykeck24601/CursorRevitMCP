// Decompiled with JetBrains decompiler
// Type: EDGE.SelectionAndPinningTools.SelectAllStructFraming
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SelectionAndPinningTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class SelectAllStructFraming : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    foreach (Element allElement in StructuralFraming.GetAllElements())
      elementIds.Add(allElement.Id);
    activeUiDocument.Selection.SetElementIds(elementIds);
    return (Result) 1;
  }
}
