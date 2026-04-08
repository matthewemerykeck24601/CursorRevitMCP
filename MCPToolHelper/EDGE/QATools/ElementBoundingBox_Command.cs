// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.ElementBoundingBox_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Utils.DrawingUtils;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class ElementBoundingBox_Command : IExternalCommand
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
        IEnumerable<ElementId> elementIds = (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds();
        if (elementIds.Any<ElementId>())
        {
          if (elementIds.Count<ElementId>() > 1)
          {
            TaskDialog.Show("EDGE: Selection Error", "More than one element selected");
            return (Result) 1;
          }
          BoundingBoxXYZ boundingBoxXyz = document.GetElement(elementIds.First<ElementId>()).get_BoundingBox((View) null);
          ModelLines.DrawModelLine(document, boundingBoxXyz.Min, new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Min.Z));
          ModelLines.DrawModelLine(document, boundingBoxXyz.Min, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, boundingBoxXyz.Min, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z));
          ModelLines.DrawModelLine(document, boundingBoxXyz.Max, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, boundingBoxXyz.Max, new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z), boundingBoxXyz.Max);
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z), new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z), new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Min.Z), new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Max.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Min.Z), new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z), new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z));
          ModelLines.DrawModelLine(document, new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Min.Z), new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z));
        }
        if (transaction.Commit() == TransactionStatus.Committed)
          return (Result) 0;
        message = "EDGE: Unable to commit transaction.";
        return (Result) -1;
      }
    }
    catch
    {
      return (Result) -1;
    }
  }
}
