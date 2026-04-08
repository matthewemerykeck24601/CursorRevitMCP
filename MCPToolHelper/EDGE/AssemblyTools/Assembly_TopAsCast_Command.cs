// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.Assembly_TopAsCast_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.Exceptions;
using Utils.IEnumerable_Extensions;
using Utils.MiscUtils;
using Utils.SelectionUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class Assembly_TopAsCast_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    IEnumerable<ElementId> elementIds = (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds();
    ElementId invalidElementId = ElementId.InvalidElementId;
    if (document.ActiveView.ViewType == ViewType.Legend || document.ActiveView.ViewType == ViewType.DrawingSheet || document.ActiveView.ViewType == ViewType.ColumnSchedule || document.ActiveView.ViewType == ViewType.AreaPlan || document.ActiveView.ViewType == ViewType.CostReport || document.ActiveView.ViewType == ViewType.DraftingView || document.ActiveView.ViewType == ViewType.LoadsReport || document.ActiveView.ViewType == ViewType.PanelSchedule || document.ActiveView.ViewType == ViewType.PresureLossReport || document.ActiveView.ViewType == ViewType.ProjectBrowser || document.ActiveView.ViewType == ViewType.Rendering || document.ActiveView.ViewType == ViewType.Report || document.ActiveView.ViewType == ViewType.Schedule || document.ActiveView.ViewType == ViewType.SystemBrowser || document.ActiveView.ViewType == ViewType.Walkthrough)
    {
      message = "Current View Type is not suitable for setting Top As Cast.";
      return (Result) -1;
    }
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.TopAsCast))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "TopAsCast"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        if (elementIds.Count<ElementId>() == 0)
        {
          Reference reference;
          try
          {
            reference = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new OnlyAssemblyInstances(), "Please select the assembly for which to set top as cast.");
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            return (Result) 1;
          }
          catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
          {
            ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
            return (Result) 1;
          }
          Element element = document.GetElement(reference);
          activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>()
          {
            element.Id
          });
        }
        IEnumerable<AssemblyInstance> source1 = new FilteredElementCollector(document, activeUiDocument.Selection.GetElementIds()).OfClass(typeof (AssemblyInstance)).ToElements().Cast<AssemblyInstance>();
        if (activeUiDocument.Selection.GetElementIds().Count > ((IEnumerable<Element>) source1).Count<Element>())
        {
          message = "Please select an Assembly Before Running the Top as Cast Tool";
          return (Result) -1;
        }
        if (((IEnumerable<Element>) source1).Count<Element>() == 0)
        {
          message = "No Assemblies Found in Selection";
          return (Result) -1;
        }
        if (((IEnumerable<Element>) source1).Count<Element>() > 1)
        {
          new TaskDialog("EDGE Error")
          {
            MainInstruction = "More than one assembly selected",
            MainContent = "Top as Cast should be run on one assembly at a time.  Please select one assembly and re-run the tool.",
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show();
          return (Result) 1;
        }
        AssemblyInstance assemblyInstance = source1.First<AssemblyInstance>();
        Element element1 = assemblyInstance.GetStructuralFramingElement();
        if (element1 == null || Utils.ElementUtils.Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
        {
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "The selected assembly is invalid. Please make sure the assembly contains a structural framing element and is not a hardware detail assembly."
          }.Show();
          return (Result) 1;
        }
        if (element1.HasSubComponents())
        {
          foreach (ElementId id in element1.GetSubComponentIds().ToList<ElementId>())
          {
            Element element2 = document.GetElement(id);
            if (element2.Name.Contains("FLAT"))
              element1 = element2;
          }
        }
        if (!new FilteredElementCollector(document, document.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).Select<Element, ElementId>((Func<Element, ElementId>) (s => s.Id)).ToHashSet<ElementId>().Contains(element1.Id))
        {
          new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            AllowCancellation = false,
            MainInstruction = "Flat element for this assembly is not visible in this view",
            MainContent = "To properly orient the assembly axes, faces and edges of the flat element must be visible for selection. Enable flat element visibility from the EDGE^R Visibility Tab and re-start this tool"
          }.Show();
          return (Result) 1;
        }
        if (commandData.Application.ActiveUIDocument.ActiveView.CanUseTemporaryVisibilityModes())
        {
          TaskDialogResult taskDialogResult = new TaskDialog("Isolate Element")
          {
            MainInstruction = "Isolate Assembly?",
            CommonButtons = ((TaskDialogCommonButtons) 6)
          }.Show();
          if (taskDialogResult == 6)
          {
            List<ElementId> list = assemblyInstance.GetMemberIds().ToList<ElementId>();
            List<ElementId> source2 = new List<ElementId>();
            List<ElementId> elementIdList = new List<ElementId>();
            foreach (ElementId id in list)
            {
              Element element3 = document.GetElement(id);
              if (!element3.Equals((object) element1))
              {
                elementIdList.Add(element3.Id);
                if (element3 is FamilyInstance)
                  elementIdList.AddRange((IEnumerable<ElementId>) (element3 as FamilyInstance).GetSubComponentIds().ToList<ElementId>());
              }
            }
            if ((element1 as FamilyInstance).SuperComponent != null && (element1 as FamilyInstance).SuperComponent.IsValidObject)
              elementIdList.Add((element1 as FamilyInstance).SuperComponent.Id);
            else
              elementIdList.Add((element1 as FamilyInstance).Id);
            list.AddRange((IEnumerable<ElementId>) source2.ToList<ElementId>());
            commandData.Application.ActiveUIDocument.ActiveView.IsolateElementsTemporary((ICollection<ElementId>) elementIdList);
          }
          else if (taskDialogResult == 2)
          {
            int num2 = (int) transaction.RollBack();
            return (Result) 1;
          }
        }
        Transform transform = Transform.Identity;
        bool flag1 = false;
        Face pickedFace = (Face) null;
        Reference reference1 = (Reference) null;
        while ((GeometryObject) pickedFace == (GeometryObject) null)
        {
          try
          {
            reference1 = activeUiDocument.Selection.PickObject((ObjectType) 4, (ISelectionFilter) new OnlyStructuralFramingFaces(element1.Id), "Select Top As Cast Face.");
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            return (Result) 1;
          }
          Element element4 = document.GetElement(reference1.ElementId);
          try
          {
            pickedFace = element4.GetGeometryObjectFromReference(reference1) as Face;
            if ((GeometryObject) pickedFace != (GeometryObject) null)
            {
              pickedFace.Evaluate(new UV(0.0, 0.0));
              if (element4 is FamilyInstance familyInstance)
              {
                flag1 = familyInstance.HasModifiedGeometry();
                Transform inverse = familyInstance.GetTransform().Inverse;
                transform = familyInstance.GetTransform();
                break;
              }
              TaskDialog.Show("ERROR", "Couldn't get transform of family instance");
              break;
            }
          }
          catch
          {
            TaskDialog.Show("Edge Error", "Unknown Error in Top As Cast while finding face");
          }
          if ((GeometryObject) pickedFace == (GeometryObject) null)
          {
            if (new TaskDialog("Error")
            {
              MainInstruction = "Selected Assembly Does Not Contain This Face: Select Again?",
              MainContent = "The face selected for Top As Cast must belong to an element of the selected assembly",
              CommonButtons = ((TaskDialogCommonButtons) 10)
            }.Show() == 2)
              return (Result) 1;
          }
        }
        bool flag2 = false;
        Edge pickedEdge1 = (Edge) null;
        Edge pickedEdge2 = (Edge) null;
        for (; !flag2; flag2 = this.checkEdges(pickedEdge1, pickedEdge2, pickedFace) == 0)
        {
          if (this.PickEdge(document, activeUiDocument, reference1, element1, true, ref pickedEdge1) == 1 || this.PickEdge(document, activeUiDocument, reference1, element1, false, ref pickedEdge2) == 1)
            return (Result) 1;
        }
        if (this.GetMemberOrientation(pickedFace, pickedEdge1, pickedEdge2, flag1 ? (Transform) null : transform) == Assembly_TopAsCast_Command.MemberOrientationCondition.Undefined)
          TaskDialog.Show("Edge Error", "Unable to determine piece orientation, please contact support");
        bool flag3 = pickedEdge1.AsCurve().GetEndPoint(0).DistanceTo(pickedEdge2.AsCurve().GetEndPoint(0)) > pickedEdge1.AsCurve().GetEndPoint(1).DistanceTo(pickedEdge2.AsCurve().GetEndPoint(0));
        int num3 = pickedEdge2.AsCurve().GetEndPoint(0).DistanceTo(pickedEdge1.AsCurve().GetEndPoint(0)) > pickedEdge2.AsCurve().GetEndPoint(1).DistanceTo(pickedEdge1.AsCurve().GetEndPoint(0)) ? 1 : 0;
        XYZ vec = pickedEdge1.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize();
        XYZ xyz = pickedEdge2.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize();
        if (flag3)
          vec = vec.Negate();
        if (num3 != 0)
          xyz = xyz.Negate();
        if (!flag1)
          transform.OfVector(xyz).Normalize();
        else
          xyz.Normalize();
        bool flag4 = true;
        if (xyz != null)
        {
          double num4 = Math.Abs(vec.CrossProduct(xyz).GetLength());
          if (num4 > 1.0001 || num4 < 0.9999)
          {
            TaskDialog taskDialog = new TaskDialog("Non-Orthogonal");
            taskDialog.AllowCancellation = false;
            taskDialog.MainInstruction = "Picked Edges are not perpendicular";
            taskDialog.MainContent = "In order to set the assembly origin, it will be aligned with one of the picked edges.  Either Green will be aligned with the left edge or red will be aligned with the bottom edge";
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Align Green Arrow with Left Edge");
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Align Red Arrow with the Bottom Edge");
            if (taskDialog.Show() == 1002)
              flag4 = false;
          }
        }
        Transform identity = Transform.Identity;
        identity.Origin = flag1 ? pickedEdge1.AsCurve().GetEndPoint(flag3 ? 1 : 0) : transform.OfPoint(pickedEdge1.AsCurve().GetEndPoint(flag3 ? 1 : 0));
        identity.BasisZ = flag1 ? pickedFace.ComputeNormal(new UV(0.0, 0.0)).Normalize() : transform.OfVector(pickedFace.ComputeNormal(new UV(0.0, 0.0))).Normalize();
        if (flag4)
        {
          identity.BasisY = flag1 ? vec : transform.OfVector(vec).Normalize();
          identity.BasisX = identity.BasisY.CrossProduct(identity.BasisZ).Normalize();
        }
        else
        {
          identity.BasisX = flag1 ? xyz : transform.OfVector(xyz).Normalize();
          identity.BasisY = identity.BasisZ.CrossProduct(identity.BasisX).Normalize();
        }
        try
        {
          assemblyInstance.SetTransform(identity);
        }
        catch (Exception ex)
        {
          if (!identity.IsConformal)
          {
            new TaskDialog("Transform Error")
            {
              AllowCancellation = false,
              CommonButtons = ((TaskDialogCommonButtons) 1),
              MainInstruction = "Transform is non-conformal.  Please verify that correct face and edges have been selected.",
              MainContent = "Orientation edges picked for top as cast must be parallel to the picked top as cast face.  Please ensure that proper edges have been selected.",
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show();
            return (Result) 1;
          }
          int num5 = (int) MessageBox.Show(ex.ToString(), "Warning");
          return (Result) 1;
        }
        List<Element> list1 = assemblyInstance.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).ToList<Element>();
        ElementId sfCatId = new ElementId(BuiltInCategory.OST_StructuralFraming);
        list1.Remove(list1.FirstOrDefault<Element>((Func<Element, bool>) (elem => elem.Category.Id.Equals((object) sfCatId))));
        LocationInFormAnalyzer locationInFormAnalyzer = new LocationInFormAnalyzer(assemblyInstance, 1.0);
        AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(document);
        foreach (Element elem in (IEnumerable<Element>) list1)
        {
          if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("EMBED"))
          {
            Parameter parameter = elem.LookupParameter("LOCATION_IN_FORM");
            if (parameter != null)
            {
              bool isReadOnly = parameter.IsReadOnly;
              if (locationInFormAnalyzer.ElementsInTopFaces.Contains(elem.Id))
              {
                if (!isReadOnly)
                  parameter.Set(locationInFormValues.TIF);
              }
              else if (locationInFormAnalyzer.ElementsInSideFaces.Contains(elem.Id))
              {
                if (!isReadOnly)
                  parameter.Set(locationInFormValues.SIF);
              }
              else if (locationInFormAnalyzer.ElementsInDownFaces.Contains(elem.Id) && !isReadOnly)
                parameter.Set(locationInFormValues.BIF);
            }
            else
            {
              TaskDialog.Show("Top As Cast", "LOCATION_IN_FORM parameter not present, retry after running Shared Parameter Updater to auto-generate Location in Form information.");
              break;
            }
          }
        }
        if (commandData.Application.ActiveUIDocument.ActiveView.CanUseTemporaryVisibilityModes())
        {
          try
          {
            commandData.Application.ActiveUIDocument.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
          }
          catch
          {
            this.ShowTd("GraphicsError", "Revit API was unable to reset the temporary hide/isolate");
          }
        }
        this.DrawVisualization(activeUiDocument, reference1, identity.BasisZ, identity.BasisX, identity.Origin, identity);
        int num6 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        int num7 = (int) transaction.RollBack();
        return (Result) -1;
      }
    }
  }

  private void ShowInvalidPickedEdgeWarning()
  {
    new TaskDialog("Error")
    {
      MainInstruction = "Selected Edge is not part of the main structural framing member of this assembly.",
      MainContent = "The line selected for Top As Cast Edge must belong to the structural framing element of the assembly.  Please choose again."
    }.Show();
  }

  private Assembly_TopAsCast_Command.MemberOrientationCondition GetMemberOrientation(
    Face pickedFace,
    Edge pickedLeftEdge,
    Edge pickedBottomEdge,
    Transform assemblyTransform = null)
  {
    if ((GeometryObject) pickedFace == (GeometryObject) null || (GeometryObject) pickedLeftEdge == (GeometryObject) null || (GeometryObject) pickedBottomEdge == (GeometryObject) null)
      throw new Exception("face, or edge null in GetMemberOrientation for Top As Cast");
    XYZ vec1 = pickedFace.ComputeNormal(new UV(0.0, 0.0));
    XYZ vec2 = pickedBottomEdge.ComputeDerivatives(0.0).BasisX.Normalize();
    if (assemblyTransform != null)
    {
      vec1 = assemblyTransform.OfVector(vec1);
      vec2 = assemblyTransform.OfVector(vec2);
    }
    int num = vec1.CrossProduct(new XYZ(0.0, 0.0, 1.0)).IsAlmostEqualTo(new XYZ(0.0, 0.0, 0.0)) ? 1 : 0;
    bool flag1 = vec1.DotProduct(new XYZ(0.0, 0.0, 1.0)) < 0.001;
    bool flag2 = this.IsApproximatelyEqual(vec2.DotProduct(new XYZ(0.0, 0.0, 1.0)), 0.0);
    if (num != 0)
      return Assembly_TopAsCast_Command.MemberOrientationCondition.FlatBeam;
    if (!flag1)
      return Assembly_TopAsCast_Command.MemberOrientationCondition.SlopedBeam;
    if (flag1 & flag2)
      return Assembly_TopAsCast_Command.MemberOrientationCondition.VerticalElement;
    return flag1 && !flag2 ? Assembly_TopAsCast_Command.MemberOrientationCondition.SlopedVerticalElement : Assembly_TopAsCast_Command.MemberOrientationCondition.Undefined;
  }

  private bool IsApproximatelyEqual(double a, double b) => b <= a + 0.0001 && b >= a - 0.0001;

  private void ShowTd(string title, string message)
  {
    new TaskDialog(title)
    {
      MainInstruction = message,
      AllowCancellation = true
    }.Show();
  }

  private void DrawVisualization(
    UIDocument UIDoc,
    Reference faceRef,
    XYZ normal,
    XYZ bottomDirection,
    XYZ originPoint,
    Transform newTransform)
  {
    Document document = UIDoc.Document;
    SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(document.ActiveView) ?? SpatialFieldManager.CreateSpatialFieldManager(document.ActiveView, 1);
    sfm.Clear();
    Face objectFromReference = document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
    int num1;
    try
    {
      num1 = sfm.AddSpatialFieldPrimitive(faceRef);
    }
    catch (Exception ex)
    {
      num1 = sfm.AddSpatialFieldPrimitive(objectFromReference, Transform.Identity);
    }
    int num2 = sfm.AddSpatialFieldPrimitive();
    List<UV> points = new List<UV>();
    BoundingBoxUV boundingBox = objectFromReference.GetBoundingBox();
    UV min = boundingBox.Min;
    UV max = boundingBox.Max;
    points.Add(new UV(min.U, min.V));
    points.Add(new UV(max.U, max.V));
    FieldDomainPointsByUV domainPointsByUv = new FieldDomainPointsByUV((IList<UV>) points);
    List<double> values = new List<double>();
    List<ValueAtPoint> valueAtPoint = new List<ValueAtPoint>();
    values.Add(0.0);
    valueAtPoint.Add(new ValueAtPoint((IList<double>) values));
    valueAtPoint.Add(new ValueAtPoint((IList<double>) values));
    FieldValues fieldValues1 = new FieldValues((IList<ValueAtPoint>) valueAtPoint);
    AnalysisResultSchema surfaceResultSchema = this.GetSurfaceResultSchema(document);
    int resultIndex1 = sfm.RegisterResult(surfaceResultSchema);
    sfm.UpdateSpatialFieldPrimitive(num1, (FieldDomainPoints) domainPointsByUv, fieldValues1, resultIndex1);
    FieldDomainPointsByXYZ domainPointsByXyz = new FieldDomainPointsByXYZ((IList<XYZ>) new List<XYZ>()
    {
      originPoint,
      originPoint,
      originPoint
    });
    VectorAtPoint vectorAtPoint1 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
    {
      bottomDirection.Normalize().Multiply(0.5)
    });
    VectorAtPoint vectorAtPoint2 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
    {
      normal.Normalize().Multiply(1.15)
    });
    VectorAtPoint vectorAtPoint3 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
    {
      normal.CrossProduct(bottomDirection).Normalize().Multiply(1.05)
    });
    FieldValues fieldValues2 = new FieldValues((IList<VectorAtPoint>) new List<VectorAtPoint>()
    {
      vectorAtPoint1,
      vectorAtPoint3,
      vectorAtPoint2
    });
    AnalysisResultSchema arrowsResultSchema = this.GetArrowsResultSchema(document);
    int resultIndex2 = sfm.RegisterResult(arrowsResultSchema);
    sfm.UpdateSpatialFieldPrimitive(num2, (FieldDomainPoints) domainPointsByXyz, fieldValues2, resultIndex2);
    App.AVFViewTimers.Enqueue(new AVFViewTimer(document.ActiveView, num1, num2, sfm));
  }

  private AnalysisResultSchema GetArrowsResultSchema(Document revitDoc)
  {
    AnalysisDisplayStyle analysisDisplayStyle = Utils.MiscUtils.MiscUtils.DeleteExtraSchema(revitDoc, "Assembly_Origin");
    if (analysisDisplayStyle == null)
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
        new AnalysisDisplayColorEntry(new Autodesk.Revit.DB.Color(byte.MaxValue, (byte) 0, (byte) 0), 1.0),
        new AnalysisDisplayColorEntry(new Autodesk.Revit.DB.Color((byte) 0, byte.MaxValue, (byte) 0), 1.1),
        new AnalysisDisplayColorEntry(new Autodesk.Revit.DB.Color((byte) 0, (byte) 0, byte.MaxValue), 1.2)
      };
      colorSettings.SetIntermediateColors(map);
      colorSettings.MaxColor = new Autodesk.Revit.DB.Color((byte) 0, (byte) 0, byte.MaxValue);
      colorSettings.MinColor = new Autodesk.Revit.DB.Color(byte.MaxValue, (byte) 0, (byte) 0);
      analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(revitDoc, "Assembly_Origin", vectorSettings, colorSettings, new AnalysisDisplayLegendSettings()
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

  private AnalysisResultSchema GetSurfaceResultSchema(Document revitDoc)
  {
    AnalysisDisplayStyle analysisDisplayStyle = Utils.MiscUtils.MiscUtils.DeleteExtraSchema(revitDoc, "Assembly_TopAsCast");
    if (analysisDisplayStyle == null)
    {
      AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
      AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
      colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.SolidColorRanges;
      System.Drawing.Color goldenrod = System.Drawing.Color.Goldenrod;
      colorSettings.MaxColor = new Autodesk.Revit.DB.Color(goldenrod.R, goldenrod.G, goldenrod.B);
      colorSettings.MinColor = new Autodesk.Revit.DB.Color(goldenrod.R, goldenrod.G, goldenrod.B);
      analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(revitDoc, "Assembly_TopAsCast", coloredSurfaceSettings, colorSettings, new AnalysisDisplayLegendSettings()
      {
        ShowLegend = false
      });
    }
    return new AnalysisResultSchema("TopAsCast", "Top As Cast Visualization")
    {
      AnalysisDisplayStyleId = analysisDisplayStyle.Id
    };
  }

  private Result PickEdge(
    Document revitDoc,
    UIDocument UIDoc,
    Reference pickedFaceRef,
    Element structuralFramingElement,
    bool bLeftorBottom,
    ref Edge pickedEdge)
  {
    pickedEdge = (Edge) null;
    string str = !bLeftorBottom ? "Select Right Edge of Horizontal Element or Bottom Edge of Vertical Element" : "Select Mark End of Horizontal Element or Left Edge of Vertical Element";
    while ((GeometryObject) pickedEdge == (GeometryObject) null)
    {
      Reference reference;
      try
      {
        reference = UIDoc.Selection.PickObject((ObjectType) 3, (ISelectionFilter) new OnlyStructuralFramingFaces(structuralFramingElement.Id), str);
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 1;
      }
      if (reference.ElementId != pickedFaceRef.ElementId)
      {
        this.ShowInvalidPickedEdgeWarning();
      }
      else
      {
        Element element = revitDoc.GetElement(reference.ElementId);
        try
        {
          pickedEdge = element.GetGeometryObjectFromReference(reference) as Edge;
          if ((GeometryObject) pickedEdge != (GeometryObject) null)
            return (Result) 0;
        }
        catch
        {
        }
        if ((GeometryObject) pickedEdge == (GeometryObject) null)
        {
          if (new TaskDialog("Error")
          {
            MainInstruction = "Selected Assembly Does Not Contain This Line: Select Again?",
            MainContent = (!bLeftorBottom ? "The line selected for Top As Cast Bottom Edge must belong to an element of the selected assembly" : "The line selected for Top As Cast Left Edge must belong to an element of the selected assembly"),
            CommonButtons = ((TaskDialogCommonButtons) 10)
          }.Show() == 2)
            return (Result) 1;
        }
      }
    }
    return (Result) -1;
  }

  private Result checkEdges(Edge pickedLeftEdge, Edge pickedBottomEdge, Face pickedFace)
  {
    double num1 = Math.Abs(pickedLeftEdge.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize().CrossProduct(pickedBottomEdge.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize()).GetLength());
    double num2 = Math.Abs(pickedLeftEdge.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize().CrossProduct(pickedFace.ComputeNormal(new UV(0.0, 0.0).Normalize())).GetLength());
    double num3 = Math.Abs(pickedBottomEdge.AsCurve().ComputeDerivatives(0.0, true).BasisX.Normalize().CrossProduct(pickedFace.ComputeNormal(new UV(0.0, 0.0).Normalize())).GetLength());
    if (num1 < 0.001)
    {
      if (new TaskDialog("Edge Error")
      {
        AllowCancellation = true,
        MainInstruction = "Picked Edges are parallel",
        MainContent = "Picked edges must not be parallel, please choose again."
      }.Show() == 2)
        return (Result) 1;
      pickedBottomEdge = (Edge) null;
      return (Result) -1;
    }
    if (num2 >= 0.999 && num3 >= 0.999)
      return (Result) 0;
    if (new TaskDialog("Edge Error")
    {
      AllowCancellation = true,
      MainInstruction = "One or More Picked Edges are not in the Plane of the Top As Cast Face",
      MainContent = "Picked edges must be perpendicular to the normal of the picked top as cast face (they must be parallel to the top as cast face), please choose again."
    }.Show() == 2)
      return (Result) 1;
    pickedBottomEdge = (Edge) null;
    return (Result) -1;
  }

  private enum MemberOrientationCondition
  {
    Undefined,
    SlopedBeam,
    FlatBeam,
    VerticalElement,
    SlopedVerticalElement,
  }
}
