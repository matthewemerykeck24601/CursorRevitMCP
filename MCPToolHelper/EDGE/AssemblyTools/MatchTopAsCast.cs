// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MatchTopAsCast
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
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class MatchTopAsCast : IExternalCommand
{
  private int schemaIndex = -1;
  private int originSchemaIndex = -1;
  private bool abort;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.MatchTopAsCast))
      return (Result) 1;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Match Top as Cast Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Match Top As Cast must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (document.ActiveView.ViewType == ViewType.Legend || document.ActiveView.ViewType == ViewType.DrawingSheet || document.ActiveView.ViewType == ViewType.ColumnSchedule || document.ActiveView.ViewType == ViewType.AreaPlan || document.ActiveView.ViewType == ViewType.CostReport || document.ActiveView.ViewType == ViewType.DraftingView || document.ActiveView.ViewType == ViewType.LoadsReport || document.ActiveView.ViewType == ViewType.PanelSchedule || document.ActiveView.ViewType == ViewType.PresureLossReport || document.ActiveView.ViewType == ViewType.ProjectBrowser || document.ActiveView.ViewType == ViewType.Rendering || document.ActiveView.ViewType == ViewType.Report || document.ActiveView.ViewType == ViewType.Schedule || document.ActiveView.ViewType == ViewType.SystemBrowser || document.ActiveView.ViewType == ViewType.Walkthrough)
    {
      message = "Current View Type is not suitable for Match Top As Cast.";
      return (Result) 1;
    }
    SpatialFieldManager.GetSpatialFieldManager(document.ActiveView)?.Clear();
    Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
    Dictionary<Transform, Face> dictionary = new Dictionary<Transform, Face>();
    List<Element> elementList1 = new List<Element>();
    List<Element> elementList2 = new List<Element>();
    List<string> stringList = new List<string>();
    ICollection<ElementId> elementIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
    if (elementIds.Count<ElementId>() > 0)
      elementIds.Clear();
    try
    {
      App.DialogSwitches.SuspendModelLockingforOperation = true;
      if (elementIds.Count<ElementId>() == 0)
      {
        Reference reference;
        try
        {
          reference = selection.PickObject((ObjectType) 1, (ISelectionFilter) new AssemblyInstancesAndStructuralFraming(), "Please select an assembly to copy the assembly origin position and orientation from.");
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          App.DialogSwitches.SuspendModelLockingforOperation = false;
          return (Result) 1;
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
        {
          App.DialogSwitches.SuspendModelLockingforOperation = false;
          ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
          return (Result) 1;
        }
        ElementId id = document.GetElement(reference).Id;
        elementIds.Add(id);
      }
      Element element1 = document.GetElement(elementIds.First<ElementId>());
      Element sourceflatElement = (Element) null;
      if (element1 is AssemblyInstance)
        sourceflatElement = this.checkElement(element1 as AssemblyInstance, document);
      string name = (element1 as AssemblyInstance).Name;
      FamilyInstance topLevelElement1 = (element1 as AssemblyInstance).GetStructuralFramingElement().GetTopLevelElement() as FamilyInstance;
      string familyName = topLevelElement1.Symbol.FamilyName;
      IList<Reference> source1;
      try
      {
        source1 = selection.PickObjects((ObjectType) 1, (ISelectionFilter) new AssemblyandFilter(element1, familyName), "Please select the assemblies to copy the assembly origin position and orientation to.");
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        return (Result) 1;
      }
      if (document.ActiveView.ViewType == ViewType.Legend || document.ActiveView.ViewType == ViewType.DrawingSheet || document.ActiveView.ViewType == ViewType.ColumnSchedule || document.ActiveView.ViewType == ViewType.AreaPlan || document.ActiveView.ViewType == ViewType.CostReport || document.ActiveView.ViewType == ViewType.DraftingView || document.ActiveView.ViewType == ViewType.LoadsReport || document.ActiveView.ViewType == ViewType.PanelSchedule || document.ActiveView.ViewType == ViewType.PresureLossReport || document.ActiveView.ViewType == ViewType.ProjectBrowser || document.ActiveView.ViewType == ViewType.Rendering || document.ActiveView.ViewType == ViewType.Report || document.ActiveView.ViewType == ViewType.Schedule || document.ActiveView.ViewType == ViewType.SystemBrowser || document.ActiveView.ViewType == ViewType.Walkthrough)
      {
        message = "Current View Type is not suitable for Match Top As Cast.";
        return (Result) 1;
      }
      if (source1.Count == 0)
      {
        new TaskDialog("Warning")
        {
          MainContent = "No target assemblies were selected."
        }.Show();
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        return (Result) 1;
      }
      Solids.GetInstanceSolids((Element) topLevelElement1);
      using (TransactionGroup transactionGroup = new TransactionGroup(document, "Match Top As Cast"))
      {
        int num1 = (int) transactionGroup.Start("Match Top As Cast");
        ElementId assemblyInstanceId1 = sourceflatElement.AssemblyInstanceId;
        AssemblyInstance topLevelElement2 = document.GetElement(assemblyInstanceId1).GetTopLevelElement() as AssemblyInstance;
        List<ElementId> elementIdList = new List<ElementId>();
        List<Element> elementList3 = new List<Element>();
        List<ElementId> list1 = source1.Select<Reference, ElementId>((Func<Reference, ElementId>) (r => r.ElementId)).ToList<ElementId>();
        using (Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "subTransaction"))
        {
          bool faceValue = false;
          bool flag1 = true;
          this.schemaIndex = -1;
          this.originSchemaIndex = -1;
          Transform transform1 = topLevelElement1.GetTransform();
          XYZ offset = this.calculateOffset(topLevelElement2, topLevelElement1, transform1);
          Transform rotation = this.calculateRotation(topLevelElement2, topLevelElement1, transform1);
          XYZ sourceLength = this.calculateSourceLength(topLevelElement1, sourceflatElement, offset, transform1);
          Element targetelement = (Element) null;
          foreach (ElementId id in list1)
          {
            targetelement = this.checkElement(document.GetElement(id) as AssemblyInstance, document);
            elementList3.Add(targetelement);
          }
          Face face1 = this.skewedelements(topLevelElement2, transform1, out faceValue);
          bool flag2 = (GeometryObject) face1 != (GeometryObject) null && this.vertexCheck(face1, topLevelElement2);
          bool sourceedge = true;
          XYZ zero = XYZ.Zero;
          Transform transform2 = topLevelElement2.GetTransform();
          if (flag2)
          {
            List<Edge> edgeList = new List<Edge>();
            XYZ source2 = this.vertexCheckSource(face1, topLevelElement2);
            foreach (EdgeArray edgeLoop in face1.EdgeLoops)
            {
              foreach (Edge edge in edgeLoop)
              {
                if (edge.AsCurve().GetEndPoint(0).IsAlmostEqualTo(source2) || edge.AsCurve().GetEndPoint(1).IsAlmostEqualTo(source2))
                  edgeList.Add(edge);
              }
            }
            foreach (Edge edge in edgeList)
            {
              if (edge.AsCurve() is Line && Util.IsParallel((edge.AsCurve() as Line).Direction, transform2.BasisY))
                sourceedge = false;
            }
          }
          if (!flag2)
            flag1 = false;
          foreach (Element targetflatElement in elementList3)
          {
            int num2 = (int) transaction.Start();
            bool flag3 = false;
            Face targetFace1 = (Face) null;
            FamilyInstance target = targetflatElement as FamilyInstance;
            ElementId assemblyInstanceId2 = targetflatElement.AssemblyInstanceId;
            AssemblyInstance topLevelElement3 = document.GetElement(assemblyInstanceId2).GetTopLevelElement() as AssemblyInstance;
            Transform transform3 = target.GetTransform();
            XYZ proportion = this.calculateProportion(sourceLength, target, targetflatElement, transform3);
            XYZ point = new XYZ(proportion.X, proportion.Y, proportion.Z);
            XYZ xyz1 = transform3.OfPoint(point);
            Transform identity = Transform.Identity;
            identity.Origin = xyz1;
            identity.BasisX = transform3.OfVector(rotation.BasisX);
            identity.BasisY = transform3.OfVector(rotation.BasisY);
            identity.BasisZ = transform3.OfVector(rotation.BasisZ);
            topLevelElement3.SetTransform(identity);
            LocationInFormAnalyzer locationInFormAnalyzer = new LocationInFormAnalyzer(topLevelElement3, 1.0);
            List<Element> list2 = topLevelElement3.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).ToList<Element>();
            AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(document);
            foreach (Element elem in (IEnumerable<Element>) list2)
            {
              if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("EMBED"))
              {
                Parameter parameter1 = elem.LookupParameter("LOCATION_IN_FORM");
                Parameter parameter2 = elem.LookupParameter("LOCATION_IN_FORM");
                bool flag4 = true;
                if (parameter2 != null)
                  flag4 = parameter2.IsReadOnly;
                if (parameter1 != null)
                {
                  if (locationInFormAnalyzer.ElementsInTopFaces.Contains(elem.Id))
                  {
                    if (!flag4)
                      parameter2.Set(locationInFormValues.TIF);
                  }
                  else if (locationInFormAnalyzer.ElementsInSideFaces.Contains(elem.Id))
                  {
                    if (!flag4)
                      parameter2.Set(locationInFormValues.SIF);
                  }
                  else if (locationInFormAnalyzer.ElementsInDownFaces.Contains(elem.Id) && !flag4)
                    parameter2.Set(locationInFormValues.BIF);
                }
                else
                {
                  Element element2 = elem.Document.GetElement(elem.GetTypeId());
                  if ((element2 == null ? (Parameter) null : element2.LookupParameter("LOCATION_IN_FORM")) == null)
                  {
                    elementList1.Add((Element) topLevelElement3);
                    break;
                  }
                  elementList2.Add((Element) topLevelElement3);
                  break;
                }
              }
            }
            Face targetFace2 = (Face) null;
            if (faceValue)
            {
              if (flag1)
              {
                bool noEquivFace = false;
                List<Face> matchedFaces = this.GetMatchedFaces(face1, topLevelElement3, targetelement, out noEquivFace);
                if (matchedFaces.Count == 0)
                  flag3 = true;
                else if (noEquivFace)
                {
                  Face face2 = matchedFaces.First<Face>();
                  UV uv = new UV();
                  face2.GetSurface().Project(identity.Origin, out uv, out double _);
                  XYZ xyz2 = face2.Evaluate(uv);
                  identity.Origin = xyz2;
                  topLevelElement3.SetTransform(identity);
                  if (this.vertexCheck(face2, topLevelElement3))
                  {
                    targetFace1 = face2;
                    flag3 = true;
                  }
                }
                try
                {
                  if (!flag3)
                  {
                    XYZ findPoint = this.targetfaceToFindPoint(topLevelElement3, matchedFaces, out targetFace2);
                    targetFace1 = targetFace2;
                    identity.Origin = findPoint;
                    if ((GeometryObject) targetFace1 != (GeometryObject) null)
                      topLevelElement3.SetTransform(this.aligningedges(sourceedge, identity, targetFace1));
                    else
                      targetFace1 = (Face) null;
                  }
                }
                catch (Exception ex)
                {
                  new TaskDialog("Match Top as Cast Tool Exception")
                  {
                    MainContent = "Tool failed to run to completion and has been canceled. The geometry of the target structural framing piece is unsupported for processing. Please check the geometry of this piece and try again.",
                    ExpandedContent = ex.Message
                  }.Show();
                  int num3 = (int) transactionGroup.RollBack();
                  return (Result) -1;
                }
              }
            }
            stringList.Add(topLevelElement3.Name);
            dictionary.Add(identity, targetFace1);
            document.Regenerate();
            if (transaction.Commit().ToString() != TransactionStatus.Committed.ToString())
            {
              this.abort = true;
              break;
            }
          }
        }
        if (!this.abort)
        {
          using (Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "subTransaction2"))
          {
            int num4 = (int) transaction.Start();
            foreach (KeyValuePair<Transform, Face> keyValuePair in dictionary)
            {
              Transform key = keyValuePair.Key;
              Face faceRef = keyValuePair.Value;
              this.DrawVisualization(activeUiDocument, faceRef, key.BasisZ, key.BasisX, key.Origin);
            }
            if (elementList2.Count > 0)
              TaskDialog.Show("Match Top As Cast", "The LOCATION_IN_FORM parameter is set as a Type parameter on some or all of the relevant elements within the selected assembly. The LOCATION_IN_FORM parameter will not be updated on these elements. Please update LOCATION_IN_FORM to be an instance parameter for the value to be automatically assigned by this tool.");
            if (elementList1.Count > 0)
              TaskDialog.Show("Match Top As Cast", "LOCATION_IN_FORM parameter not present, retry after running Shared Parameter Updater to auto-generate Location in Form information.");
            stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
            string str = string.Join(", ", stringList.ToArray());
            new TaskDialog("Success")
            {
              MainContent = $"The assembly origin is successfully copied from {name} to the following assemblies: \n{str}",
              CommonButtons = ((TaskDialogCommonButtons) 1)
            }.Show();
            int num5 = (int) transaction.Commit();
          }
          int num6 = (int) transactionGroup.Assimilate();
        }
        else
        {
          int num7 = (int) transactionGroup.RollBack();
          new TaskDialog("Match Top As Cast")
          {
            MainContent = "Match Top as Cast Tool was unsuccessful in copying assembly origin to the target assemblies.",
            CommonButtons = ((TaskDialogCommonButtons) 8)
          }.Show();
        }
        return (Result) 0;
      }
    }
    catch (Exception ex)
    {
      message = ex.ToString();
      App.DialogSwitches.SuspendModelLockingforOperation = false;
      return (Result) -1;
    }
    finally
    {
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
  }

  public XYZ calculateOffset(
    AssemblyInstance assembyinstance,
    FamilyInstance familyinstance,
    Transform familyTransform)
  {
    Transform transform = assembyinstance.GetTransform();
    return familyTransform.Inverse.OfPoint(transform.Origin);
  }

  public Transform calculateRotation(
    AssemblyInstance assembyinstance,
    FamilyInstance familyinstance,
    Transform f)
  {
    Transform transform = assembyinstance.GetTransform();
    Transform translation = Transform.CreateTranslation(new XYZ());
    XYZ xyz1 = f.Inverse.OfVector(transform.BasisX);
    XYZ xyz2 = f.Inverse.OfVector(transform.BasisY);
    XYZ xyz3 = f.Inverse.OfVector(transform.BasisZ);
    translation.BasisX = xyz1;
    translation.BasisY = xyz2;
    translation.BasisZ = xyz3;
    return translation;
  }

  public XYZ calculateSourceLength(
    FamilyInstance family,
    Element sourceflatElement,
    XYZ offset,
    Transform sourceFamilyTransform)
  {
    BoundingBoxXYZ transformedBoundingBox = Util.getTransformedBoundingBox(sourceFamilyTransform, sourceflatElement);
    XYZ xyz = transformedBoundingBox.Max.Subtract(transformedBoundingBox.Min);
    double num1 = offset.X / xyz.X;
    if (double.IsNaN(num1) || double.IsInfinity(num1))
      num1 = 0.0;
    double num2 = offset.Y / xyz.Y;
    if (double.IsNaN(num2) || double.IsInfinity(num2))
      num2 = 0.0;
    double num3 = offset.Z / xyz.Z;
    if (double.IsNaN(num3) || double.IsInfinity(num3))
      num3 = 0.0;
    return new XYZ(num1, num2, num3);
  }

  public XYZ calculateProportion(
    XYZ sourcedetails,
    FamilyInstance target,
    Element targetflatElement,
    Transform targetFamilyTransform)
  {
    BoundingBoxXYZ transformedBoundingBox = Util.getTransformedBoundingBox(targetFamilyTransform, targetflatElement);
    XYZ xyz = transformedBoundingBox.Max.Subtract(transformedBoundingBox.Min);
    double x1 = xyz.X;
    double x2 = sourcedetails.X * x1;
    double y1 = xyz.Y;
    double y2 = sourcedetails.Y * y1;
    double z1 = xyz.Z;
    double z2 = sourcedetails.Z * z1;
    return new XYZ(x2, y2, z2);
  }

  public Element checkElement(AssemblyInstance assemblyelement, Document revitDoc)
  {
    FamilyInstance structuralFramingElement = assemblyelement.GetStructuralFramingElement() as FamilyInstance;
    return !structuralFramingElement.IsWarpableProduct() ? assemblyelement.GetStructuralFramingElement() : StructuralFraming.RefineNestedFamily((Element) structuralFramingElement);
  }

  public Face skewedelements(AssemblyInstance assembly, Transform famTrans, out bool faceValue)
  {
    Element structuralFramingElement = assembly.GetStructuralFramingElement();
    Transform transform = assembly.GetTransform();
    XYZ origin = transform.Origin;
    XYZ basisZ = transform.BasisZ;
    XYZ source = famTrans.Inverse.OfVector(basisZ);
    List<Solid> instanceSolids = Solids.GetInstanceSolids(structuralFramingElement);
    List<Face> faceList = new List<Face>();
    foreach (Solid solid in instanceSolids)
    {
      foreach (Face face in solid.Faces)
      {
        if (face is PlanarFace)
        {
          XYZ faceNormal = (face as PlanarFace).FaceNormal;
          if (famTrans.Inverse.OfVector(faceNormal).IsAlmostEqualTo(source) && Math.Abs(transform.Inverse.OfPoint(face.Origin()).Z).ApproximatelyEquals(0.0))
            faceList.Add(face);
        }
      }
    }
    faceValue = false;
    foreach (Face face in faceList)
    {
      if (face.Project(origin) != null)
      {
        faceValue = true;
        return face;
      }
    }
    return (Face) null;
  }

  private List<Face> GetMatchedFaces(
    Face sourceFace,
    AssemblyInstance targetAssembly,
    Element targetelement,
    out bool noEquivFace)
  {
    noEquivFace = false;
    List<Face> matchedFaces = new List<Face>();
    Element structuralFramingElement = targetAssembly.GetStructuralFramingElement();
    Transform transform1 = targetAssembly.GetTransform();
    List<Solid> instanceSolids = Solids.GetInstanceSolids(structuralFramingElement);
    foreach (Solid solid in instanceSolids)
    {
      foreach (Face face in solid.Faces)
      {
        if (face.OriginNormal().IsAlmostEqualTo(transform1.BasisZ) && transform1.Inverse.OfPoint(face.Origin()).Z.ApproximatelyEquals(0.0))
          matchedFaces.Add(face);
      }
    }
    if (matchedFaces.Count == 0)
    {
      noEquivFace = true;
      Dictionary<double, List<Face>> source1 = new Dictionary<double, List<Face>>();
      foreach (Solid solid in instanceSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            Transform transform2 = (targetelement as FamilyInstance).GetTransform();
            XYZ source2 = transform2.Inverse.OfVector(transform1.BasisZ);
            if (transform2.Inverse.OfVector((face as PlanarFace).FaceNormal).IsAlmostEqualTo(source2))
            {
              double key = Math.Abs(transform1.Inverse.OfPoint(face.Origin()).Z);
              if (source1.ContainsKey(key))
                source1[key].Add(face);
              else
                source1.Add(key, new List<Face>() { face });
            }
          }
        }
      }
      if (source1.Count > 0)
      {
        source1.Keys.OrderBy<double, double>((Func<double, double>) (e => e)).Min();
        foreach (Face face in source1.First<KeyValuePair<double, List<Face>>>().Value)
          matchedFaces.Add(face);
      }
    }
    return matchedFaces;
  }

  public XYZ targetfaceToFindPoint(
    AssemblyInstance targetAssembly,
    List<Face> list,
    out Face targetFace)
  {
    Transform transform = targetAssembly.GetTransform();
    Dictionary<Face, XYZ> dictionary1 = new Dictionary<Face, XYZ>();
    List<Line> lineList = new List<Line>();
    foreach (Face key1 in list)
    {
      Dictionary<double, List<XYZ>> dictionary2 = new Dictionary<double, List<XYZ>>();
      XYZ xyz1 = (XYZ) null;
      double other1 = double.MaxValue;
      double other2 = double.MaxValue;
      double other3 = double.MinValue;
      double other4 = double.MinValue;
      foreach (EdgeArray edgeLoop in key1.EdgeLoops)
      {
        foreach (Edge edge in edgeLoop)
        {
          XYZ xyz2 = transform.Inverse.OfPoint(edge.AsCurve().GetEndPoint(0));
          transform.Inverse.OfPoint(edge.AsCurve().GetEndPoint(1));
          other1 = xyz2.X < other1 ? xyz2.X : other1;
          other2 = xyz2.Y < other2 ? xyz2.Y : other2;
          other3 = xyz2.X > other3 ? xyz2.X : other3;
          other4 = xyz2.Y > other4 ? xyz2.Y : other4;
        }
        foreach (Edge edge in edgeLoop)
        {
          XYZ endpoint1 = transform.Inverse.OfPoint(edge.AsCurve().GetEndPoint(0));
          XYZ endpoint2 = transform.Inverse.OfPoint(edge.AsCurve().GetEndPoint(1));
          if (endpoint1.X.ApproximatelyEquals(other1) || endpoint2.Y.ApproximatelyEquals(other2) || endpoint1.X.ApproximatelyEquals(other3) || endpoint2.Y.ApproximatelyEquals(other4) || endpoint1.Y.ApproximatelyEquals(other2) || endpoint2.X.ApproximatelyEquals(other1) || endpoint1.Y.ApproximatelyEquals(other4) || endpoint2.X.ApproximatelyEquals(other3))
            lineList.Add(Line.CreateBound(endpoint1, endpoint2));
        }
      }
      foreach (Line line in lineList)
      {
        if (Util.IsParallel((XYZ.Zero - line.GetEndPoint(0)).Normalize(), line.Direction))
        {
          for (int index = 0; index < 2; ++index)
          {
            XYZ endPoint = line.GetEndPoint(index);
            double key2 = endPoint.DistanceTo(XYZ.Zero);
            if (!dictionary2.ContainsKey(key2))
              dictionary2.Add(key2, new List<XYZ>()
              {
                transform.OfPoint(endPoint)
              });
            else
              dictionary2[key2].Add(transform.OfPoint(endPoint));
          }
        }
      }
      if (dictionary2.Keys.Count > 0)
      {
        double key3 = dictionary2.Keys.Min();
        xyz1 = dictionary2[key3].ElementAt<XYZ>(0);
      }
      if (xyz1 != null)
        dictionary1.Add(key1, xyz1);
    }
    XYZ origin = transform.Origin;
    double num1 = double.MaxValue;
    Face face = (Face) null;
    foreach (Face key in dictionary1.Keys)
    {
      double num2 = transform.Origin.DistanceTo(dictionary1[key]);
      if (num2 < num1)
      {
        face = key;
        origin = dictionary1[key];
        num1 = num2;
      }
    }
    targetFace = face;
    return origin;
  }

  private void DrawVisualization(
    UIDocument UIDoc,
    Face faceRef,
    XYZ normal,
    XYZ bottomDirection,
    XYZ originPoint)
  {
    Document document = UIDoc.Document;
    SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(document.ActiveView) ?? SpatialFieldManager.CreateSpatialFieldManager(document.ActiveView, 1);
    if ((GeometryObject) faceRef != (GeometryObject) null)
    {
      int num1 = sfm.AddSpatialFieldPrimitive(faceRef, Transform.Identity);
      int num2 = sfm.AddSpatialFieldPrimitive();
      List<UV> points = new List<UV>();
      BoundingBoxUV boundingBox = faceRef.GetBoundingBox();
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
      AnalysisResultSchema analysisResultSchema1 = (AnalysisResultSchema) null;
      if (this.schemaIndex != -1)
      {
        analysisResultSchema1 = sfm.GetResultSchema(this.schemaIndex);
      }
      else
      {
        AnalysisResultSchema surfaceResultSchema = this.GetSurfaceResultSchema(document);
        this.schemaIndex = sfm.RegisterResult(surfaceResultSchema);
      }
      sfm.UpdateSpatialFieldPrimitive(num1, (FieldDomainPoints) domainPointsByUv, fieldValues1, this.schemaIndex);
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
      AnalysisResultSchema analysisResultSchema2 = (AnalysisResultSchema) null;
      if (this.originSchemaIndex != -1)
      {
        analysisResultSchema2 = sfm.GetResultSchema(this.originSchemaIndex);
      }
      else
      {
        AnalysisResultSchema arrowsResultSchema = MatchTopAsCast.GetArrowsResultSchema(document);
        this.originSchemaIndex = sfm.RegisterResult(arrowsResultSchema);
      }
      sfm.UpdateSpatialFieldPrimitive(num2, (FieldDomainPoints) domainPointsByXyz, fieldValues2, this.originSchemaIndex);
      App.AVFViewTimers.Enqueue(new AVFViewTimer(document.ActiveView, num1, num2, sfm));
    }
    else
    {
      int num = sfm.AddSpatialFieldPrimitive();
      List<double> values = new List<double>();
      List<ValueAtPoint> valueAtPoint = new List<ValueAtPoint>();
      values.Add(0.0);
      valueAtPoint.Add(new ValueAtPoint((IList<double>) values));
      valueAtPoint.Add(new ValueAtPoint((IList<double>) values));
      FieldValues fieldValues3 = new FieldValues((IList<ValueAtPoint>) valueAtPoint);
      AnalysisResultSchema analysisResultSchema3 = (AnalysisResultSchema) null;
      if (this.schemaIndex != -1)
      {
        analysisResultSchema3 = sfm.GetResultSchema(this.schemaIndex);
      }
      else
      {
        AnalysisResultSchema surfaceResultSchema = this.GetSurfaceResultSchema(document);
        this.schemaIndex = sfm.RegisterResult(surfaceResultSchema);
      }
      FieldDomainPointsByXYZ domainPointsByXyz = new FieldDomainPointsByXYZ((IList<XYZ>) new List<XYZ>()
      {
        originPoint,
        originPoint,
        originPoint
      });
      VectorAtPoint vectorAtPoint4 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        bottomDirection.Normalize().Multiply(0.5)
      });
      VectorAtPoint vectorAtPoint5 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        normal.Normalize().Multiply(1.15)
      });
      VectorAtPoint vectorAtPoint6 = new VectorAtPoint((IList<XYZ>) new List<XYZ>()
      {
        normal.CrossProduct(bottomDirection).Normalize().Multiply(1.05)
      });
      FieldValues fieldValues4 = new FieldValues((IList<VectorAtPoint>) new List<VectorAtPoint>()
      {
        vectorAtPoint4,
        vectorAtPoint6,
        vectorAtPoint5
      });
      AnalysisResultSchema analysisResultSchema4 = (AnalysisResultSchema) null;
      if (this.originSchemaIndex != -1)
      {
        analysisResultSchema4 = sfm.GetResultSchema(this.originSchemaIndex);
      }
      else
      {
        AnalysisResultSchema arrowsResultSchema = MatchTopAsCast.GetArrowsResultSchema(document);
        this.originSchemaIndex = sfm.RegisterResult(arrowsResultSchema);
      }
      sfm.UpdateSpatialFieldPrimitive(num, (FieldDomainPoints) domainPointsByXyz, fieldValues4, this.originSchemaIndex);
      App.AVFViewTimers.Enqueue(new AVFViewTimer(document.ActiveView, 0, num, sfm));
    }
  }

  public static AnalysisResultSchema GetArrowsResultSchema(Document revitDoc)
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
    AnalysisDisplayStyle analysisDisplayStyle = Utils.MiscUtils.MiscUtils.DeleteExtraSchema(revitDoc, nameof (MatchTopAsCast));
    if (analysisDisplayStyle == null)
    {
      AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
      AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
      colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.SolidColorRanges;
      System.Drawing.Color goldenrod = System.Drawing.Color.Goldenrod;
      colorSettings.MaxColor = new Autodesk.Revit.DB.Color(goldenrod.R, goldenrod.G, goldenrod.B);
      colorSettings.MinColor = new Autodesk.Revit.DB.Color(goldenrod.R, goldenrod.G, goldenrod.B);
      analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(revitDoc, nameof (MatchTopAsCast), coloredSurfaceSettings, colorSettings, new AnalysisDisplayLegendSettings()
      {
        ShowLegend = false
      });
    }
    return new AnalysisResultSchema(nameof (MatchTopAsCast), "Match Top As Cast Visualization")
    {
      AnalysisDisplayStyleId = analysisDisplayStyle.Id
    };
  }

  private bool vertexCheck(Face face, AssemblyInstance assemblyInstance)
  {
    XYZ origin = assemblyInstance.GetTransform().Origin;
    foreach (EdgeArray edgeLoop in face.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        if (edge.Evaluate(0.0).IsWithinTolerance(origin, 0.0) || edge.Evaluate(1.0).IsWithinTolerance(origin, 0.0))
          return true;
      }
    }
    return false;
  }

  private XYZ vertexCheckSource(Face face, AssemblyInstance assemblyInstance)
  {
    XYZ origin = assemblyInstance.GetTransform().Origin;
    foreach (EdgeArray edgeLoop in face.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        if (edge.Evaluate(0.0).IsWithinTolerance(origin, 0.0) || edge.Evaluate(1.0).IsWithinTolerance(origin, 0.0))
          return edge.Evaluate(0.0);
      }
    }
    return XYZ.Zero;
  }

  private Transform aligningedges(bool sourceedge, Transform newTargetTransform, Face targetFace)
  {
    Dictionary<double, XYZ> dictionary = new Dictionary<double, XYZ>();
    bool flag = false;
    XYZ source = sourceedge ? newTargetTransform.BasisX : newTargetTransform.BasisY;
    XYZ xyz1 = XYZ.Zero;
    foreach (EdgeArray edgeLoop in targetFace.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        XYZ xyz2 = (edge.AsCurve().GetEndPoint(1) - edge.AsCurve().GetEndPoint(0)).Normalize();
        for (int index = 0; index < 2; ++index)
        {
          if (edge.AsCurve().GetEndPoint(index).IsAlmostEqualTo(newTargetTransform.Origin))
          {
            double me1 = xyz2.AngleTo(source);
            double me2 = xyz2.Negate().AngleTo(source);
            if (me1.ApproximatelyEquals(0.0))
            {
              xyz1 = xyz2;
              flag = true;
            }
            if (me2.ApproximatelyEquals(0.0))
            {
              xyz1 = xyz2.Negate();
              flag = true;
            }
            if (!flag)
            {
              double key = me1 <= me2 ? me1 : me2 * -1.0;
              if (!dictionary.ContainsKey(key))
              {
                dictionary.Add(key, xyz2);
                break;
              }
              dictionary[key] = xyz2;
              break;
            }
            break;
          }
        }
        if (flag)
          break;
      }
      if (flag)
        break;
    }
    if (!flag)
    {
      double num = dictionary.Keys.OrderBy<double, double>((Func<double, double>) (edgepoint => Math.Abs(edgepoint))).First<double>();
      dictionary.TryGetValue(dictionary.Keys.OrderBy<double, double>((Func<double, double>) (edgepoint => Math.Abs(edgepoint))).First<double>(), out xyz1);
      xyz1 = num < 0.0 ? xyz1.Negate() : xyz1;
    }
    if (!sourceedge)
    {
      newTargetTransform.BasisY = xyz1;
      newTargetTransform.BasisX = newTargetTransform.BasisY.CrossProduct(newTargetTransform.BasisZ);
    }
    else
    {
      newTargetTransform.BasisX = xyz1;
      newTargetTransform.BasisY = newTargetTransform.BasisZ.CrossProduct(newTargetTransform.BasisX);
    }
    return newTargetTransform;
  }
}
