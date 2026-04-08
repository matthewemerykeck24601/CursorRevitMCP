// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.CopyAnnotation
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
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class CopyAnnotation : IExternalCommand
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
        View element1 = document.GetElement(new ElementId(2430450)) as View;
        BoundingBoxXYZ boundingBoxXyz1 = (document.GetElement(element1.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement().get_BoundingBox((View) null);
        XYZ xyz1 = new XYZ((boundingBoxXyz1.Max.X + boundingBoxXyz1.Min.X) / 2.0, (boundingBoxXyz1.Max.Y + boundingBoxXyz1.Min.Y) / 2.0, (boundingBoxXyz1.Max.Z + boundingBoxXyz1.Min.Z) / 2.0);
        FilteredElementCollector elementCollector1 = new FilteredElementCollector(document, element1.Id).OwnedByView(element1.Id);
        List<TextNote> textNoteList = new List<TextNote>();
        List<FilledRegion> filledRegionList = new List<FilledRegion>();
        List<DetailLine> detailLineList = new List<DetailLine>();
        View element2 = document.GetElement(new ElementId(2430244)) as View;
        BoundingBoxXYZ boundingBoxXyz2 = (document.GetElement(element2.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement().get_BoundingBox((View) null);
        XYZ xyz2 = new XYZ((boundingBoxXyz2.Max.X + boundingBoxXyz2.Min.X) / 2.0, (boundingBoxXyz2.Max.Y + boundingBoxXyz2.Min.Y) / 2.0, (boundingBoxXyz2.Max.Z + boundingBoxXyz2.Min.Z) / 2.0);
        foreach (Element element3 in elementCollector1)
        {
          if (element3 is TextNote)
            textNoteList.Add(element3 as TextNote);
          else if (element3 is DetailLine)
            detailLineList.Add(element3 as DetailLine);
        }
        XYZ xyz3 = xyz1 - element1.CropBox.Transform.Origin;
        XYZ xyz4 = xyz2 - element2.CropBox.Transform.Origin;
        XYZ vec1 = xyz3 - xyz3.DotProduct(element1.ViewDirection.Normalize()) * element1.ViewDirection.Normalize();
        XYZ vec2 = element1.CropBox.Transform.Inverse.OfVector(vec1);
        XYZ xyz5 = element2.CropBox.Transform.OfVector(vec2);
        XYZ vector = xyz4 - xyz4.DotProduct(element2.ViewDirection.Normalize()) * element2.ViewDirection.Normalize() - xyz5;
        foreach (TextNote textNote in textNoteList)
        {
          CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
          View sourceView = element1;
          List<ElementId> elementsToCopy = new List<ElementId>();
          elementsToCopy.Add(textNote.Id);
          View destinationView = element2;
          Transform translation = Transform.CreateTranslation(vector);
          CopyPasteOptions options = copyPasteOptions;
          ElementTransformUtils.CopyElements(sourceView, (ICollection<ElementId>) elementsToCopy, destinationView, translation, options);
        }
        FilteredElementCollector elementCollector2 = new FilteredElementCollector(document, element1.Id);
        elementCollector2.OfClass(typeof (FilledRegion));
        foreach (FilledRegion filledRegion in elementCollector2)
        {
          CurveLoop curveLoop = new CurveLoop();
          foreach (CurveLoop boundary in (IEnumerable<CurveLoop>) filledRegion.GetBoundaries())
          {
            foreach (Curve curve in boundary)
            {
              Curve transformed = curve.CreateTransformed(element1.CropBox.Transform.Inverse).CreateTransformed((document.GetElement(element2.Id) as ViewSection).CropBox.Transform);
              curveLoop.Append(transformed);
            }
          }
          if (!filledRegion.IsMasking)
          {
            FilledRegion.Create(document, filledRegion.GetTypeId(), element2.Id, (IList<CurveLoop>) new List<CurveLoop>()
            {
              curveLoop
            });
          }
          else
          {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
            View sourceView = element1;
            List<ElementId> elementsToCopy = new List<ElementId>();
            elementsToCopy.Add(filledRegion.Id);
            View destinationView = element2;
            Transform identity = Transform.Identity;
            CopyPasteOptions options = copyPasteOptions;
            ElementTransformUtils.CopyElements(sourceView, (ICollection<ElementId>) elementsToCopy, destinationView, identity, options);
          }
        }
        foreach (DetailLine detailLine in detailLineList)
        {
          CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
          View sourceView = element1;
          List<ElementId> elementsToCopy = new List<ElementId>();
          elementsToCopy.Add(detailLine.Id);
          View destinationView = element2;
          Transform translation = Transform.CreateTranslation(vector);
          CopyPasteOptions options = copyPasteOptions;
          ElementTransformUtils.CopyElements(sourceView, (ICollection<ElementId>) elementsToCopy, destinationView, translation, options);
        }
        FilteredElementCollector source = new FilteredElementCollector(document, document.ActiveView.Id);
        source.OfCategory(BuiltInCategory.OST_DetailComponents);
        source.Count<Element>();
        foreach (Element element4 in source)
        {
          Location location = element4.Location;
          XYZ xyz6 = new XYZ();
          if (location is LocationPoint)
          {
            XYZ point = (location as LocationPoint).Point;
            XYZ xyz7 = new XYZ(point.X - xyz1.X, point.Y - xyz1.Y, point.Z - xyz1.Z);
            XYZ origin = new XYZ(xyz2.X + xyz7.X, xyz2.Y + xyz7.Y, xyz2.Z + xyz7.Z);
            ViewSection element5 = document.GetElement(element2.Id) as ViewSection;
            FamilyInstance familyInstance = element4 as FamilyInstance;
            document.Create.NewFamilyInstance(origin, familyInstance.Symbol, (View) element5);
          }
          else if (location is LocationCurve)
          {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
            View sourceView = element1;
            List<ElementId> elementsToCopy = new List<ElementId>();
            elementsToCopy.Add(element4.Id);
            View destinationView = element2;
            Transform translation = Transform.CreateTranslation(vector);
            CopyPasteOptions options = copyPasteOptions;
            ElementTransformUtils.CopyElements(sourceView, (ICollection<ElementId>) elementsToCopy, destinationView, translation, options);
          }
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
}
