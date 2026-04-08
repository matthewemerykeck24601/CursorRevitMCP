// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.PlanGeometryTest_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.QATools;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
public class PlanGeometryTest_Command : IExternalCommand
{
  private bool bTraceOn = true;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    XYZ viewDirection = document.ActiveView.ViewDirection;
    XYZ upDirection = document.ActiveView.UpDirection;
    XYZ xyz = upDirection.CrossProduct(viewDirection);
    this.AutodimTrace("ViewDirection: " + viewDirection.ToString());
    this.AutodimTrace("UpDirection: " + upDirection.ToString());
    this.AutodimTrace("X Direction: " + xyz.ToString());
    Transform transform = document.ActiveView.CropBox.Transform;
    this.TraceTransform(transform);
    ICollection<ElementId> elementIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
    if (elementIds.Count != 1)
    {
      message = "Select only one element.";
      return (Result) -1;
    }
    int num = (int) new GeometryViewerForm(document.GetElement(elementIds.First<ElementId>()), transform).ShowDialog();
    return (Result) 0;
  }

  public void AutodimTrace(string message)
  {
    if (!QA.LogAutodimTrace || !this.bTraceOn)
      return;
    QA.Trace(message);
  }

  public void TraceTransform(Transform trans)
  {
    if (trans == null)
      return;
    this.AutodimTrace("Transform: ");
    this.AutodimTrace("     Origin: " + trans.Origin.ToString());
    this.AutodimTrace("     BasisX: " + trans.BasisX.ToString());
    this.AutodimTrace("     BasisY: " + trans.BasisY.ToString());
    this.AutodimTrace("     BasisZ: " + trans.BasisZ.ToString());
  }
}
