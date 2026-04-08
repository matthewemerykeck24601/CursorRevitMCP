// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.PointTest3D
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.DrawingUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class PointTest3D : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(document, "Move From Reference"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        View activeView = document.ActiveView;
        XYZ xyz = activeUiDocument.Selection.PickPoint();
        ViewOrientation3D orientation = (activeView as View3D).GetOrientation();
        XYZ eyePosition = orientation.EyePosition;
        XYZ forwardDirection = orientation.ForwardDirection;
        double num2 = xyz.DotProduct(forwardDirection);
        double num3 = eyePosition.DotProduct(forwardDirection);
        XYZ origin = xyz + forwardDirection.Negate() * (num2 - num3);
        Line unbound = Line.CreateUnbound(origin, forwardDirection);
        ReferenceWithContext nearest = new ReferenceIntersector(activeView as View3D).FindNearest(origin, unbound.Direction);
        Element element = (Element) null;
        XYZ end = xyz;
        if (nearest != null)
        {
          Reference reference = nearest.GetReference();
          element = document.GetElement(reference.ElementId);
          end = reference.GlobalPoint;
          ModelLines.DrawVector(document, end + forwardDirection.Negate(), end);
        }
        TaskDialog taskDialog1 = new TaskDialog("TEST");
        taskDialog1.MainInstruction = element == null ? "Current element: NULL" : "Current element: " + element.Id.ToString();
        taskDialog1.MainContent = $"Selected Point: {xyz?.ToString()}{Environment.NewLine}";
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.MainContent = $"{taskDialog2.MainContent}Selected Point (Max Height): {origin?.ToString()}{Environment.NewLine}";
        TaskDialog taskDialog3 = taskDialog1;
        taskDialog3.MainContent = $"{taskDialog3.MainContent}Projected Point: {end?.ToString()}";
        taskDialog1.Show();
        int num4 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
