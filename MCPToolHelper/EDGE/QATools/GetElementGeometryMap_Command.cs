// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.GetElementGeometryMap_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class GetElementGeometryMap_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    ICollection<ElementId> elementIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
    if (elementIds.Count != 1)
    {
      message = "Select only one element.";
      return (Result) -1;
    }
    int num = (int) new GeometryViewerForm(document.GetElement(elementIds.First<ElementId>())).ShowDialog();
    return (Result) 0;
  }
}
