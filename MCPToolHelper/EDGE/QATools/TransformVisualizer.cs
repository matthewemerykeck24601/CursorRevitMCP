// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.TransformVisualizer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.QATools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class TransformVisualizer : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    using (Transaction transaction = new Transaction(revitDoc, "AddTransformVisualization"))
    {
      if (transaction.Start() != TransactionStatus.Started)
        return (Result) -1;
      foreach (Element element in commandData.Application.ActiveUIDocument.Selection.GetElementIds().ToList<ElementId>().Select<ElementId, Element>((Func<ElementId, Element>) (s => revitDoc.GetElement(s))))
      {
        if (element is FamilyInstance)
        {
          TaskDialog taskDialog = new TaskDialog("Choose");
          taskDialog.MainInstruction = "Normal or Inverse?";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Current");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Inverse");
          if (taskDialog.Show() == 1001)
          {
            Transform transform = (element as FamilyInstance).GetTransform();
            this.DrawVisualization(revitDoc, transform.BasisY, transform.BasisX, transform.Origin);
          }
          else
          {
            Transform inverse = (element as FamilyInstance).GetTransform().Inverse;
            this.DrawVisualization(revitDoc, inverse.BasisY, inverse.BasisX, inverse.Origin);
          }
          Options options = new Options();
          GeometryElement geometryElement = (element as FamilyInstance).get_Geometry(options);
          FamilyInstance familyInstance = element as FamilyInstance;
          familyInstance.HasModifiedGeometry();
          foreach (GeometryObject geometryObject in geometryElement)
          {
            Solid solid = geometryObject as Solid;
            if ((GeometryObject) null != (GeometryObject) solid && solid.Volume > 0.01)
            {
              this.DrawSolidVisualization(revitDoc, solid, familyInstance.GetTransform().Inverse);
            }
            else
            {
              int num = (GeometryObject) null != (GeometryObject) (geometryObject as GeometryInstance) ? 1 : 0;
            }
          }
        }
      }
      int num1 = (int) transaction.Commit();
    }
    return (Result) 0;
  }

  private void DrawSolidVisualization(Document revitDoc, Solid solid, Transform transform)
  {
    foreach (Face face in solid.Faces)
      this.DrawFaceVisualization(revitDoc, face, transform);
  }

  private void DrawFaceVisualization(Document revitDoc, Face face, Transform transformToApply)
  {
    XYZ basisX = transformToApply == null ? face.ComputeDerivatives(new UV(0.0, 0.0)).BasisX : transformToApply.OfVector(face.ComputeDerivatives(new UV(0.0, 0.0)).BasisX);
    XYZ basisY = transformToApply == null ? face.ComputeDerivatives(new UV(0.0, 0.0)).BasisY : transformToApply.OfVector(face.ComputeDerivatives(new UV(0.0, 0.0)).BasisY);
    Plane byOriginAndBasis = Plane.CreateByOriginAndBasis(transformToApply == null ? face.Evaluate(new UV(0.0, 0.0)) : transformToApply.OfPoint(face.Evaluate(new UV(0.0, 0.0))), basisX, basisY);
    SketchPlane sketchPlane = SketchPlane.Create(revitDoc, byOriginAndBasis);
    CurveArray visualizationCurves = this.GetFaceVisualizationCurves(face, transformToApply);
    revitDoc.Create.NewModelCurveArray(visualizationCurves, sketchPlane);
  }

  private CurveArray GetFaceVisualizationCurves(Face face, Transform transformToApply)
  {
    CurveArray visualizationCurves = new CurveArray();
    foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) face.GetEdgesAsCurveLoops())
    {
      foreach (Curve curve1 in edgesAsCurveLoop)
      {
        Curve curve2 = transformToApply == null ? curve1 : curve1.CreateTransformed(transformToApply);
        visualizationCurves.Append(curve2);
      }
    }
    return visualizationCurves;
  }

  private void DrawVisualization(
    Document Doc,
    XYZ leftDirection,
    XYZ bottomDirection,
    XYZ originPoint)
  {
    Document revitDoc = Doc;
    SpatialFieldManager spatialFieldManager = SpatialFieldManager.GetSpatialFieldManager(revitDoc.ActiveView) ?? SpatialFieldManager.CreateSpatialFieldManager(revitDoc.ActiveView, 1);
    spatialFieldManager.Clear();
    int idx = spatialFieldManager.AddSpatialFieldPrimitive();
    FieldDomainPointsByXYZ domainPointsByXyz = new FieldDomainPointsByXYZ((IList<XYZ>) new List<XYZ>()
    {
      originPoint,
      originPoint,
      originPoint
    });
    FieldValues fieldValues = new FieldValues((IList<VectorAtPoint>) new List<VectorAtPoint>()
    {
      new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        bottomDirection.Normalize().Multiply(0.5)
      }),
      new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        leftDirection.Normalize().Multiply(1.05)
      }),
      new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        bottomDirection.CrossProduct(leftDirection).Normalize().Multiply(1.15)
      })
    });
    AnalysisResultSchema arrowsResultSchema = this.GetArrowsResultSchema(revitDoc);
    int resultIndex = spatialFieldManager.RegisterResult(arrowsResultSchema);
    spatialFieldManager.UpdateSpatialFieldPrimitive(idx, (FieldDomainPoints) domainPointsByXyz, fieldValues, resultIndex);
  }

  private AnalysisResultSchema GetArrowsResultSchema(Document revitDoc)
  {
    IEnumerable<AnalysisDisplayStyle> source = new FilteredElementCollector(revitDoc).OfClass(typeof (AnalysisDisplayStyle)).ToElements().Cast<AnalysisDisplayStyle>().Where<AnalysisDisplayStyle>((Func<AnalysisDisplayStyle, bool>) (s => s.Name == "Transform_Axes"));
    AnalysisDisplayStyle analysisDisplayStyle;
    if (source.Any<AnalysisDisplayStyle>())
    {
      analysisDisplayStyle = source.First<AnalysisDisplayStyle>();
    }
    else
    {
      AnalysisDisplayVectorSettings vectorSettings = new AnalysisDisplayVectorSettings();
      vectorSettings.ArrowheadScale = AnalysisDisplayStyleVectorArrowheadScale.Length20Percent;
      vectorSettings.ArrowLineWeight = 5;
      vectorSettings.VectorOrientation = AnalysisDisplayStyleVectorOrientation.Linear;
      vectorSettings.VectorPosition = AnalysisDisplayStyleVectorPosition.FromDataPoint;
      vectorSettings.VectorTextType = AnalysisDisplayStyleVectorTextType.ShowNone;
      AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
      colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.SolidColorRanges;
      IList<AnalysisDisplayColorEntry> map = (IList<AnalysisDisplayColorEntry>) new List<AnalysisDisplayColorEntry>()
      {
        new AnalysisDisplayColorEntry(new Color(byte.MaxValue, (byte) 0, (byte) 0), 1.0),
        new AnalysisDisplayColorEntry(new Color((byte) 0, byte.MaxValue, (byte) 0), 1.1),
        new AnalysisDisplayColorEntry(new Color((byte) 0, (byte) 0, byte.MaxValue), 1.2)
      };
      colorSettings.SetIntermediateColors(map);
      colorSettings.MaxColor = new Color((byte) 0, (byte) 0, byte.MaxValue);
      colorSettings.MinColor = new Color(byte.MaxValue, (byte) 0, (byte) 0);
      analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(revitDoc, "Transform_Axes", vectorSettings, colorSettings, new AnalysisDisplayLegendSettings()
      {
        ShowLegend = false
      });
    }
    return new AnalysisResultSchema("Origin", "Origin visualization")
    {
      AnalysisDisplayStyleId = analysisDisplayStyle.Id,
      Scale = 1.0
    };
  }
}
