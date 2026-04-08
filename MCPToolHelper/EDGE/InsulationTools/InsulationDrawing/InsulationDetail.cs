// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDetail
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using EDGE.AdminTools;
using EDGE.AdminTools.CAM;
using EDGE.TicketTools.AutoDimensioning;
using EDGE.UserSettingTools.Insulation_Drawing_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class InsulationDetail
{
  private Document revitDoc;
  public string InsulationMark = string.Empty;
  private Schema extensibleStorageScheme;
  private FamilyInstance InsulationInstance;
  private List<RecessDetailInfo> recessList = new List<RecessDetailInfo>();
  private InsulationDetailLineInfo insulationLineInfo;
  private InsulationDrawingSettingsObject insulationDrawingSettingsObject;
  private string extensibleStorageGuid = "a4cbb208-6cf4-450f-8cbe-d12a85b7d5b9";
  private List<TextNote> textNotes = new List<TextNote>();
  private List<DetailCurve> detailCurves = new List<DetailCurve>();
  private List<Dimension> dimensions = new List<Dimension>();
  private double extremeMaxX = double.MinValue;
  private double extremeMaxY = double.MinValue;
  private double extremeMinX = double.MaxValue;
  private double extremeMinY = double.MaxValue;
  private double insulationMaxX = double.MinValue;
  private double insulationMaxY = double.MinValue;
  private double insulationMinX = double.MaxValue;
  private double insulationMinY = double.MaxValue;
  private int scaleFactor = 32 /*0x20*/;
  private XYZ center = XYZ.Zero;
  public bool detailDrawn;
  public double masterXPosition;

  public double height => this.extremeMaxY - this.extremeMinY;

  public double width => this.extremeMaxX - this.extremeMinX;

  public XYZ BottomLeft
  {
    get => new XYZ(this.center.X - this.width / 2.0, this.center.Y - this.height / 2.0, 0.0);
  }

  public InsulationDetail(
    FamilyInstance insulation,
    InsulationDrawingSettingsObject settingsObject,
    string insulationMark = "")
  {
    this.revitDoc = insulation.Document;
    this.InsulationMark = !string.IsNullOrWhiteSpace(insulationMark) ? insulationMark : Parameters.GetParameterAsString((Element) insulation, "INSULATION_MARK");
    this.insulationDrawingSettingsObject = settingsObject;
    this.InsulationInstance = insulation;
    Guid guid = new Guid(this.extensibleStorageGuid);
    if (this.extensibleStorageScheme == null)
    {
      this.extensibleStorageScheme = Schema.Lookup(guid);
      if (this.extensibleStorageScheme == null)
      {
        SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
        schemaBuilder.AddSimpleField(nameof (InsulationMark), typeof (string)).SetDocumentation("Identifier for PTAC's Insulation Drawings.");
        schemaBuilder.SetSchemaName("PTAC_Insulation_Drawings");
        this.extensibleStorageScheme = schemaBuilder.Finish();
      }
    }
    Transform transform1 = insulation.GetTransform();
    Transform transform2 = (Transform) null;
    if (insulation.AssemblyInstanceId != (ElementId) null && insulation.AssemblyInstanceId.IntegerValue != -1 && this.revitDoc.GetElement(insulation.AssemblyInstanceId) is AssemblyInstance element)
    {
      Transform transform3 = element.GetTransform();
      if (transform3.BasisZ.IsAlmostEqualTo(transform1.BasisZ) || transform3.BasisZ.IsAlmostEqualTo(transform1.BasisZ.Negate()))
      {
        transform2 = transform3;
      }
      else
      {
        XYZ xyz = transform1.BasisZ.CrossProduct(transform3.BasisZ);
        if (xyz.IsAlmostEqualTo(transform3.BasisY) || xyz.IsAlmostEqualTo(transform3.BasisY.Negate()))
          transform2 = new Transform(transform1)
          {
            BasisY = transform3.BasisY,
            BasisX = transform3.BasisY.CrossProduct(transform1.BasisZ)
          };
      }
    }
    if (transform2 == null)
      transform2 = transform1;
    List<Solid> instanceSolids = Solids.GetInstanceSolids((Element) insulation);
    Solid solid1 = (Solid) null;
    foreach (Solid solid2 in instanceSolids)
    {
      if (solid2.Volume > 0.0)
        solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid2, solid1, BooleanOperationsType.Union) : SolidUtils.Clone(solid2);
    }
    List<Tuple<CurveLoop, XYZ, bool>> outerCurves = new List<Tuple<CurveLoop, XYZ, bool>>();
    List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
    List<Tuple<CurveLoop, XYZ>> downwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
    double maxZ;
    double minZ;
    CAM_Utils.GetCurveLoops(solid1, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ, out minZ, transform2);
    Dictionary<double, List<CurveLoop>> recesses = this.GetRecesses(solid1, transform2, maxZ, minZ);
    Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
    if (!CAM_Utils.GetLayerDictionary(LayerDictionary, outerCurves, upwardFacingCurves, downwardFacingCurves, this.revitDoc, "", 0.0, minZ, maxZ, Math.Abs(maxZ - minZ), out bool _, transform2))
      return;
    Solid solid3 = (Solid) null;
    foreach (Solid key in LayerDictionary.Keys)
      solid3 = !((GeometryObject) solid3 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(key, solid3, BooleanOperationsType.Union) : SolidUtils.Clone(key);
    outerCurves.Clear();
    upwardFacingCurves.Clear();
    downwardFacingCurves.Clear();
    CAM_Utils.GetCurveLoops(solid3, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ, out minZ, transform2);
    List<CurveLoop> originalCurveLoopList = new List<CurveLoop>();
    foreach (Tuple<CurveLoop, XYZ, bool> tuple in outerCurves)
    {
      CurveLoop curveLoop = tuple.Item1;
      if (transform2.Inverse.OfPoint(tuple.Item2).Z.ApproximatelyEquals(maxZ))
        originalCurveLoopList.Add(curveLoop);
    }
    bool OverallSplines;
    Dictionary<CurveLoop, CurveLoop> flattenedToZ1 = CAM_Utils.GetFlattenedToZ(originalCurveLoopList, transform2, 0.0, 1.0 / 384.0, out OverallSplines);
    List<CurveArray> curves = new List<CurveArray>();
    foreach (CurveLoop key in flattenedToZ1.Keys)
    {
      CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform2);
      curves.Add(curveArrayFromLoop);
    }
    List<CurveLoop> second = new List<CurveLoop>();
    foreach (Tuple<CurveLoop, XYZ> tuple in upwardFacingCurves)
      second.Add(tuple.Item1);
    List<CurveLoop> first = new List<CurveLoop>();
    foreach (double key in recesses.Keys)
      first.AddRange((IEnumerable<CurveLoop>) recesses[key]);
    Dictionary<CurveLoop, CurveLoop> flattenedToZ2 = CAM_Utils.GetFlattenedToZ(first.Union<CurveLoop>((IEnumerable<CurveLoop>) second).ToList<CurveLoop>(), transform2, 0.0, 1.0 / 384.0, out OverallSplines);
    Dictionary<double, List<CurveArray>> dictionary = new Dictionary<double, List<CurveArray>>();
    foreach (CurveLoop key1 in flattenedToZ2.Keys)
    {
      CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key1, transform2);
      bool flag = false;
      foreach (double key2 in recesses.Keys)
      {
        foreach (object obj in recesses[key2])
        {
          if (obj.Equals((object) flattenedToZ2[key1]))
          {
            if (dictionary.ContainsKey(key2))
              dictionary[key2].Add(curveArrayFromLoop);
            else
              dictionary.Add(key2, new List<CurveArray>()
              {
                curveArrayFromLoop
              });
            flag = true;
            break;
          }
        }
      }
      if (!flag)
        curves.Add(curveArrayFromLoop);
    }
    foreach (double key in dictionary.Keys)
    {
      foreach (CurveArray outerCurveArray in this.GetOuterCurveArrays(dictionary[key]))
        this.recessList.Add(new RecessDetailInfo(key, outerCurveArray));
    }
    this.insulationLineInfo = new InsulationDetailLineInfo(curves);
  }

  private Dictionary<double, List<CurveLoop>> GetRecesses(
    Solid solid,
    Transform transform,
    double maxZ,
    double minZ)
  {
    Dictionary<double, List<CurveLoop>> recesses = new Dictionary<double, List<CurveLoop>>();
    foreach (Face face in SolidUtils.CreateTransformed(solid, transform.Inverse).Faces)
    {
      if (face is PlanarFace planarFace && (planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) || planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate())) && !planarFace.Origin.Z.ApproximatelyEquals(maxZ) && !planarFace.Origin.Z.ApproximatelyEquals(minZ))
      {
        double key = maxZ - planarFace.Origin.Z;
        List<CurveLoop> curveLoopList = new List<CurveLoop>();
        foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops())
          curveLoopList.Add(CurveLoop.CreateViaTransform(edgesAsCurveLoop, transform));
        if (curveLoopList.Count<CurveLoop>() != 0)
        {
          if (recesses.ContainsKey(key))
            recesses[key].AddRange((IEnumerable<CurveLoop>) curveLoopList);
          else
            recesses.Add(key, curveLoopList);
        }
      }
    }
    return recesses;
  }

  private void PlaceInsulationDetailLines(
    View view,
    Transform transform,
    List<CurveArray> curves,
    Dictionary<XYZ, Reference> HorizontalDimensionReferences,
    Dictionary<XYZ, Reference> VerticalDimensionReferences)
  {
    foreach (CurveArray curve in curves)
    {
      foreach (Curve geometryCurve1 in curve)
      {
        DetailCurve elem = this.revitDoc.Create.NewDetailCurve(view, geometryCurve1);
        if (elem != null)
        {
          elem.LineStyle = (Element) this.insulationDrawingSettingsObject.InsulationDetailLineStyle.GetGraphicsStyle(GraphicsStyleType.Projection);
          this.AddExtensibleStorage((Element) elem);
          this.detailCurves.Add(elem);
          XYZ key1 = transform.Inverse.OfPoint(elem.GeometryCurve.GetEndPoint(0));
          XYZ key2 = transform.Inverse.OfPoint(elem.GeometryCurve.GetEndPoint(1));
          if (elem.GeometryCurve is Line geometryCurve2)
          {
            XYZ xyz = transform.Inverse.OfVector(geometryCurve2.Direction);
            if (xyz.CrossProduct(XYZ.BasisX).IsAlmostEqualTo(XYZ.Zero))
            {
              if (!VerticalDimensionReferences.ContainsKey(key1))
                VerticalDimensionReferences.Add(key1, geometryCurve2.Reference);
            }
            else if (xyz.CrossProduct(XYZ.BasisY).IsAlmostEqualTo(XYZ.Zero))
            {
              if (!HorizontalDimensionReferences.ContainsKey(key1))
                HorizontalDimensionReferences.Add(key1, geometryCurve2.Reference);
            }
            else
            {
              Reference endPointReference1 = geometryCurve2.GetEndPointReference(0);
              if (!HorizontalDimensionReferences.ContainsKey(key1))
                HorizontalDimensionReferences.Add(key1, endPointReference1);
              if (!VerticalDimensionReferences.ContainsKey(key1))
                VerticalDimensionReferences.Add(key1, endPointReference1);
              Reference endPointReference2 = geometryCurve2.GetEndPointReference(1);
              if (!HorizontalDimensionReferences.ContainsKey(key2))
                HorizontalDimensionReferences.Add(key2, endPointReference2);
              if (!VerticalDimensionReferences.ContainsKey(key2))
                VerticalDimensionReferences.Add(key2, endPointReference2);
            }
          }
        }
      }
    }
  }

  private void PlaceRecess(
    View view,
    Transform transform,
    XYZ scaledVector,
    Dictionary<XYZ, Reference> HorizontalRecessDimensionReferences,
    Dictionary<XYZ, Reference> VerticalRecessDimensionReferences)
  {
    foreach (RecessDetailInfo recess in this.recessList)
    {
      recess.endPoints = new List<XYZ>();
      foreach (Curve scaledCurve in recess.GetScaledCurveArray(transform, scaledVector))
      {
        DetailCurve elem = this.revitDoc.Create.NewDetailCurve(view, scaledCurve);
        if (elem != null)
        {
          elem.LineStyle = (Element) this.insulationDrawingSettingsObject.InsulationDetailLineStyle.GetGraphicsStyle(GraphicsStyleType.Projection);
          this.AddExtensibleStorage((Element) elem);
          this.detailCurves.Add(elem);
          XYZ endPoint0 = transform.Inverse.OfPoint(elem.GeometryCurve.GetEndPoint(0));
          XYZ endPoint1 = transform.Inverse.OfPoint(elem.GeometryCurve.GetEndPoint(1));
          if (!recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endPoint0))).Any<XYZ>())
            recess.endPoints.Add(endPoint0);
          if (!recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endPoint1))).Any<XYZ>())
            recess.endPoints.Add(endPoint1);
          if (elem.GeometryCurve is Line geometryCurve)
          {
            XYZ xyz = transform.Inverse.OfVector(geometryCurve.Direction);
            if (xyz.CrossProduct(XYZ.BasisX).IsAlmostEqualTo(XYZ.Zero))
            {
              if (!VerticalRecessDimensionReferences.ContainsKey(endPoint0))
                VerticalRecessDimensionReferences.Add(endPoint0, geometryCurve.Reference);
            }
            else if (xyz.CrossProduct(XYZ.BasisY).IsAlmostEqualTo(XYZ.Zero))
            {
              if (!HorizontalRecessDimensionReferences.ContainsKey(endPoint0))
                HorizontalRecessDimensionReferences.Add(endPoint0, geometryCurve.Reference);
            }
            else
            {
              Reference endPointReference1 = geometryCurve.GetEndPointReference(0);
              if (!HorizontalRecessDimensionReferences.ContainsKey(endPoint0))
                HorizontalRecessDimensionReferences.Add(endPoint0, endPointReference1);
              if (!VerticalRecessDimensionReferences.ContainsKey(endPoint0))
                VerticalRecessDimensionReferences.Add(endPoint0, endPointReference1);
              Reference endPointReference2 = geometryCurve.GetEndPointReference(1);
              if (!HorizontalRecessDimensionReferences.ContainsKey(endPoint1))
                HorizontalRecessDimensionReferences.Add(endPoint1, endPointReference2);
              if (!VerticalRecessDimensionReferences.ContainsKey(endPoint1))
                VerticalRecessDimensionReferences.Add(endPoint1, endPointReference2);
            }
          }
        }
      }
    }
  }

  private void PlaceRecessTextNotes(
    View view,
    Transform transform,
    XYZ scaledVector,
    XYZ InsulationMax,
    XYZ InsulationMin)
  {
    foreach (RecessDetailInfo recess in this.recessList)
    {
      string recessText = recess.GetRecessText();
      if (recessText != null)
      {
        double recessMaxX = double.MinValue;
        double recessMinX = double.MaxValue;
        double recessMaxY = double.MinValue;
        double recessMinY = double.MaxValue;
        foreach (XYZ endPoint in recess.endPoints)
        {
          if (endPoint.X < recessMinX)
            recessMinX = endPoint.X;
          if (endPoint.X > recessMaxX)
            recessMaxX = endPoint.X;
          if (endPoint.Y < recessMinY)
            recessMinY = endPoint.Y;
          if (endPoint.Y > recessMaxY)
            recessMaxY = endPoint.Y;
        }
        double num1 = Math.Abs(InsulationMin.X - recessMinX);
        double num2 = num1;
        double num3 = Math.Abs(InsulationMax.X - recessMaxX);
        if (num3 < num2)
          num2 = num3;
        double num4 = Math.Abs(InsulationMax.Y - recessMaxY);
        if (num4 < num2)
          num2 = num4;
        double num5 = Math.Abs(InsulationMin.Y - recessMinY);
        if (num5 < num2)
          num2 = num5;
        List<DimensionEdge> source = new List<DimensionEdge>();
        if (num2 == num1)
          source.Add(DimensionEdge.Left);
        if (num2 == num5)
          source.Add(DimensionEdge.Bottom);
        if (num2 == num3)
          source.Add(DimensionEdge.Right);
        if (num2 == num4)
          source.Add(DimensionEdge.Top);
        DimensionEdge edge;
        if (source.Count == 1)
        {
          edge = source.First<DimensionEdge>();
        }
        else
        {
          edge = source.First<DimensionEdge>();
          double num6 = double.MinValue;
          foreach (int num7 in source)
          {
            if (num7 == 0)
            {
              List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.X == recessMaxX)).ToList<XYZ>();
              if (list.Count > 1)
              {
                double num8 = list.First<XYZ>().DistanceTo(list.Last<XYZ>());
                if (num8 > num6)
                {
                  num6 = num8;
                  edge = DimensionEdge.Left;
                }
              }
            }
            if (num7 == 1)
            {
              List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.X == recessMinX)).ToList<XYZ>();
              if (list.Count > 1)
              {
                double num9 = list.First<XYZ>().DistanceTo(list.Last<XYZ>());
                if (num9 > num6)
                {
                  num6 = num9;
                  edge = DimensionEdge.Right;
                }
              }
            }
            if (num7 == 2)
            {
              List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.Y == recessMaxY)).ToList<XYZ>();
              if (list.Count > 1)
              {
                double num10 = list.First<XYZ>().DistanceTo(list.Last<XYZ>());
                if (num10 > num6)
                {
                  num6 = num10;
                  edge = DimensionEdge.Top;
                }
              }
            }
            if (num7 == 3)
            {
              List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.Y == recessMinY)).ToList<XYZ>();
              if (list.Count > 1)
              {
                double num11 = list.First<XYZ>().DistanceTo(list.Last<XYZ>());
                if (num11 > num6)
                {
                  num6 = num11;
                  edge = DimensionEdge.Bottom;
                }
              }
            }
          }
        }
        XYZ xyz1 = new XYZ();
        XYZ point1 = new XYZ();
        XYZ point2 = new XYZ();
        bool flag = false;
        if (edge == DimensionEdge.Top)
        {
          List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.Y == recessMaxY)).ToList<XYZ>();
          if (list.Count > 1)
          {
            XYZ xyz2 = list.First<XYZ>();
            XYZ xyz3 = list.Last<XYZ>();
            point1 = new XYZ((xyz2.X + xyz3.X) / 2.0, xyz2.Y + 1.0 / 320.0 * (double) this.scaleFactor, 0.0);
          }
          else
          {
            XYZ xyz4 = list.First<XYZ>();
            point1 = new XYZ(xyz4.X, xyz4.Y + 1.0 / 320.0 * (double) this.scaleFactor, 0.0);
          }
          double num12 = InsulationMax.Y + 5.0 / 64.0 * (double) this.scaleFactor - point1.Y;
          double num13 = num12 / Math.Tan(30.0);
          double x1 = point1.X + num13;
          double y = point1.Y + num12;
          double num14 = InsulationMax.X - 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.X - InsulationMin.X);
          if (x1 > num14)
            x1 = num14;
          double num15 = InsulationMin.X + 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.X - InsulationMin.X);
          if (x1 < num15)
            x1 = num15;
          point2 = new XYZ(x1, y, 0.0);
        }
        if (edge == DimensionEdge.Bottom)
        {
          List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.Y == recessMinY)).ToList<XYZ>();
          if (list.Count > 1)
          {
            XYZ xyz5 = list.First<XYZ>();
            XYZ xyz6 = list.Last<XYZ>();
            point1 = new XYZ((xyz5.X + xyz6.X) / 2.0, xyz5.Y + 1.0 / 320.0 * (double) this.scaleFactor, 0.0);
          }
          else
          {
            XYZ xyz7 = list.First<XYZ>();
            point1 = new XYZ(xyz7.X, xyz7.Y + 1.0 / 320.0 * (double) this.scaleFactor, 0.0);
          }
          double num16 = point1.Y - (InsulationMin.Y - 5.0 / 64.0 * (double) this.scaleFactor);
          double num17 = num16 / Math.Tan(30.0);
          double x2 = point1.X + num17;
          double y = point1.Y - num16;
          double num18 = InsulationMax.X - 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.X - InsulationMin.X);
          if (x2 > num18)
            x2 = num18;
          double num19 = InsulationMin.X + 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.X - InsulationMin.X);
          if (x2 < num19)
            x2 = num19;
          point2 = new XYZ(x2, y, 0.0);
        }
        if (edge == DimensionEdge.Left)
        {
          List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.X == recessMinX)).ToList<XYZ>();
          if (list.Count > 1)
          {
            XYZ xyz8 = list.First<XYZ>();
            XYZ xyz9 = list.Last<XYZ>();
            point1 = new XYZ(xyz8.X + 1.0 / 320.0 * (double) this.scaleFactor, (xyz8.Y + xyz9.Y) / 2.0, 0.0);
          }
          else
          {
            XYZ xyz10 = list.First<XYZ>();
            point1 = new XYZ(xyz10.X + 1.0 / 320.0 * (double) this.scaleFactor, xyz10.Y, 0.0);
          }
          double num20 = point1.X - (InsulationMin.X - 3.0 / 32.0 * (double) this.scaleFactor);
          double num21 = num20 / Math.Tan(30.0);
          double x3 = point1.X - num20;
          double num22 = point1.Y + num21;
          double num23 = InsulationMax.Y - 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.Y - InsulationMin.Y);
          if (num22 > num23)
            num22 = num23;
          double num24 = InsulationMin.Y + 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.Y - InsulationMin.Y);
          if (num22 < num24)
            num22 = num24;
          double y = num22;
          point2 = new XYZ(x3, y, 0.0);
          flag = true;
        }
        if (edge == DimensionEdge.Right)
        {
          List<XYZ> list = recess.endPoints.Where<XYZ>((Func<XYZ, bool>) (x => x.X == recessMaxX)).ToList<XYZ>();
          if (list.Count > 1)
          {
            XYZ xyz11 = list.First<XYZ>();
            XYZ xyz12 = list.Last<XYZ>();
            point1 = new XYZ(xyz11.X - 1.0 / 320.0 * (double) this.scaleFactor, (xyz11.Y + xyz12.Y) / 2.0, 0.0);
          }
          else
          {
            XYZ xyz13 = list.First<XYZ>();
            point1 = new XYZ(xyz13.X - 1.0 / 320.0 * (double) this.scaleFactor, xyz13.Y, 0.0);
          }
          double num25 = InsulationMax.X + 5.0 / 64.0 * (double) this.scaleFactor - point1.X;
          double num26 = num25 / Math.Tan(30.0);
          double x4 = point1.X + num25;
          double num27 = point1.Y + num26;
          double num28 = InsulationMax.Y - 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.Y - InsulationMin.Y);
          if (num27 > num28)
            num27 = num28;
          double num29 = InsulationMin.Y + 1.0 / 640.0 * (double) this.scaleFactor * (InsulationMax.Y - InsulationMin.Y);
          if (num27 < num29)
            num27 = num29;
          double y = num27;
          point2 = new XYZ(x4, y, 0.0);
          flag = true;
        }
        TextNoteOptions options = new TextNoteOptions();
        if (flag)
          options.Rotation = Math.PI / 2.0;
        options.TypeId = this.insulationDrawingSettingsObject.RecessCalloutsTextStyle.Id;
        XYZ position = transform.OfPoint(point2);
        XYZ xyz14 = transform.OfPoint(point1);
        TextNote elem = TextNote.Create(this.revitDoc, view.Id, position, recessText, options);
        Leader leader = elem.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
        this.revitDoc.Regenerate();
        XYZ translation = position - leader.Anchor;
        ElementTransformUtils.MoveElement(this.revitDoc, elem.Id, translation);
        leader.End = xyz14;
        this.AddExtensibleStorage((Element) elem);
        this.textNotes.Add(elem);
        BoundingBoxXYZ boundingBoxXyz = elem.get_BoundingBox(view);
        if (boundingBoxXyz != null)
        {
          XYZ xyz15 = transform.Inverse.OfPoint(boundingBoxXyz.Max);
          XYZ xyz16 = transform.Inverse.OfPoint(boundingBoxXyz.Min);
          this.UpdateExtremes(new XYZ(xyz15.X, xyz16.Y, 0.0), edge);
          this.UpdateExtremes(new XYZ(xyz16.X, xyz15.Y, 0.0), edge);
        }
      }
    }
  }

  private void AddDimension(
    View view,
    Transform transform,
    ReferenceArray referenceArray,
    DimensionType dimType,
    DimensionEdge edge,
    double seperation = 1.0)
  {
    XYZ point1 = XYZ.Zero;
    XYZ direction = XYZ.Zero;
    Line line = (Line) null;
    XYZ xyz1 = XYZ.Zero;
    seperation = seperation / 32.0 * (double) this.scaleFactor;
    switch (edge)
    {
      case DimensionEdge.Left:
        point1 = transform.OfPoint(new XYZ(this.extremeMinX - seperation, this.extremeMinY, 0.0));
        direction = view.UpDirection;
        line = Line.CreateBound(point1 + direction * 2.0, point1 - direction * 2.0);
        xyz1 = view.RightDirection;
        break;
      case DimensionEdge.Right:
        point1 = transform.OfPoint(new XYZ(this.extremeMaxX + seperation, this.extremeMaxY, 0.0));
        direction = view.UpDirection;
        line = Line.CreateBound(point1 + direction * 2.0, point1 - direction * 2.0);
        xyz1 = view.RightDirection;
        break;
      case DimensionEdge.Top:
        point1 = transform.OfPoint(new XYZ(this.extremeMaxX, this.extremeMaxY + seperation, 0.0));
        direction = view.RightDirection;
        line = Line.CreateBound(point1 + direction * 2.0, point1 - direction * 2.0);
        xyz1 = view.UpDirection;
        break;
      case DimensionEdge.Bottom:
        point1 = transform.OfPoint(new XYZ(this.extremeMinX, this.extremeMinY - seperation, 0.0));
        direction = view.RightDirection;
        line = Line.CreateBound(point1 + direction * 2.0, point1 - direction * 2.0);
        xyz1 = view.UpDirection;
        break;
    }
    DetailLine detailLine = this.revitDoc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(XYZ.Zero, line.Direction.CrossProduct(view.ViewDirection).Normalize().Negate())) as DetailLine;
    referenceArray.Append(detailLine.GeometryCurve.Reference);
    Dimension dimension = this.revitDoc.Create.NewDimension(view, line, referenceArray);
    if (dimType != null)
      dimension.DimensionType = dimType;
    this.revitDoc.Delete(detailLine.Id);
    this.AddExtensibleStorage((Element) dimension);
    this.dimensions.Add(dimension);
    this.revitDoc.Regenerate();
    Parameter parameter = Parameters.LookupParameter((Element) dimension, BuiltInParameter.LINEAR_DIM_TYPE);
    bool runningDimStyle = false;
    if (parameter != null && parameter.AsValueString() == "Ordinate")
      runningDimStyle = true;
    List<AutoTicketDimSegment> dimSegs = new List<AutoTicketDimSegment>();
    foreach (DimensionSegment segment in dimension.Segments)
      dimSegs.Add(new AutoTicketDimSegment(dimension, segment, direction, runningDimStyle));
    XYZ xyz2 = new XYZ();
    double width = 0.0;
    double val2 = double.MaxValue;
    foreach (DimensionSegment segment in dimension.Segments)
    {
      double num = segment.Value.Value;
      width += num;
      XYZ xyz3 = segment.Origin + direction.CrossProduct(view.ViewDirection) * (1.0 / 64.0 * (double) this.scaleFactor);
      XYZ source = segment.Origin - direction * num / 2.0;
      val2 = Math.Min(direction.DotProduct(source), val2);
    }
    double midProj = (val2 + width) / 2.0;
    double accuracy = this.revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
    if (!runningDimStyle)
    {
      AutoTicketDimSegment.HandleUpsets(width, midProj, dimSegs, edge, accuracy);
      this.revitDoc.Regenerate();
    }
    double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(dimension, view);
    XYZ xyz4 = transform.Inverse.OfPoint(point1);
    double num1 = edge == DimensionEdge.Top || edge == DimensionEdge.Bottom ? xyz4.Y : xyz4.X;
    foreach (DimensionSegment segment in dimension.Segments)
    {
      XYZ zero1 = XYZ.Zero;
      XYZ zero2 = XYZ.Zero;
      XYZ xyz5;
      XYZ xyz6;
      if (edge == DimensionEdge.Top || edge == DimensionEdge.Bottom)
      {
        xyz5 = transform.Inverse.OfPoint(segment.Origin + xyz1 * textBoundForDim);
        xyz6 = transform.Inverse.OfPoint(segment.Origin);
      }
      else
      {
        xyz5 = transform.Inverse.OfPoint(segment.Origin - xyz1 * textBoundForDim);
        xyz6 = transform.Inverse.OfPoint(segment.Origin);
      }
      switch (edge)
      {
        case DimensionEdge.Left:
          if (xyz5.X > num1)
            num1 = xyz5.X;
          if (xyz6.X > num1)
          {
            num1 = xyz6.X;
            continue;
          }
          continue;
        case DimensionEdge.Right:
          if (xyz5.X < num1)
            num1 = xyz5.X;
          if (xyz6.X < num1)
          {
            num1 = xyz6.X;
            continue;
          }
          continue;
        case DimensionEdge.Top:
          if (xyz5.Y < num1)
            num1 = xyz5.Y;
          if (xyz6.Y < num1)
          {
            num1 = xyz6.Y;
            continue;
          }
          continue;
        case DimensionEdge.Bottom:
          if (xyz5.Y > num1)
            num1 = xyz5.Y;
          if (xyz6.Y > num1)
          {
            num1 = xyz6.Y;
            continue;
          }
          continue;
        default:
          continue;
      }
    }
    XYZ zero3 = XYZ.Zero;
    XYZ vec = edge == DimensionEdge.Top || edge == DimensionEdge.Bottom ? xyz4 - new XYZ(xyz4.X, num1, xyz4.Z) : xyz4 - new XYZ(num1, xyz4.Y, xyz4.Z);
    if (!vec.IsAlmostEqualTo(XYZ.Zero))
    {
      XYZ translation = transform.OfVector(vec);
      ElementTransformUtils.MoveElement(this.revitDoc, dimension.Id, translation);
    }
    this.revitDoc.Regenerate();
    BoundingBoxXYZ boundingBoxXyz = dimension.get_BoundingBox(view);
    if (boundingBoxXyz != null)
    {
      XYZ xyz7 = transform.Inverse.OfPoint(boundingBoxXyz.Max);
      XYZ xyz8 = transform.Inverse.OfPoint(boundingBoxXyz.Min);
      this.UpdateExtremes(new XYZ(xyz7.X, xyz8.Y, 0.0), edge);
      this.UpdateExtremes(new XYZ(xyz8.X, xyz7.Y, 0.0), edge);
    }
    else if (dimension.Segments.Size > 0)
    {
      foreach (DimensionSegment segment in dimension.Segments)
      {
        XYZ zero4 = XYZ.Zero;
        XYZ zero5 = XYZ.Zero;
        XYZ point2;
        XYZ origin;
        if (edge == DimensionEdge.Top || edge == DimensionEdge.Bottom)
        {
          point2 = segment.Origin + xyz1 * textBoundForDim;
          origin = segment.Origin;
        }
        else
        {
          point2 = segment.Origin - xyz1 * textBoundForDim;
          origin = segment.Origin;
        }
        this.UpdateExtremes(transform.Inverse.OfPoint(point2), edge);
        this.UpdateExtremes(transform.Inverse.OfPoint(origin), edge);
      }
    }
    else
    {
      XYZ zero6 = XYZ.Zero;
      XYZ zero7 = XYZ.Zero;
      XYZ point3;
      XYZ origin;
      if (edge == DimensionEdge.Top || edge == DimensionEdge.Bottom)
      {
        point3 = dimension.Origin + xyz1 * textBoundForDim;
        origin = dimension.Origin;
      }
      else
      {
        point3 = dimension.Origin - xyz1 * textBoundForDim;
        origin = dimension.Origin;
      }
      this.UpdateExtremes(transform.Inverse.OfPoint(point3), edge);
      this.UpdateExtremes(transform.Inverse.OfPoint(origin), edge);
    }
  }

  private void AddInsulationMarkAnnotations(
    View view,
    Transform transform,
    XYZ insulationMax,
    XYZ insulationMin)
  {
    TextNoteType insulationMarkTextStyle = this.insulationDrawingSettingsObject.InsulationMarkTextStyle;
    insulationMarkTextStyle.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
    int scale = view.Scale;
    TextNoteOptions options = new TextNoteOptions();
    options.TypeId = insulationMarkTextStyle.Id;
    XYZ xyz1 = transform.OfPoint(new XYZ((insulationMax.X + insulationMin.X) / 2.0, this.extremeMinY - 3.0 / 32.0 * (double) this.scaleFactor, 0.0));
    TextNote elem1 = TextNote.Create(this.revitDoc, view.Id, xyz1, this.InsulationMark, options);
    this.revitDoc.Regenerate();
    this.AddExtensibleStorage((Element) elem1);
    this.textNotes.Add(elem1);
    XYZ xyz2 = transform.Inverse.OfPoint(xyz1);
    BoundingBoxXYZ boundingBoxXyz1 = elem1.get_BoundingBox(view);
    XYZ xyz3 = transform.Inverse.OfPoint(elem1.Coord);
    XYZ xyz4 = transform.OfPoint(new XYZ(xyz3.X, xyz3.Y + 3.0 / 64.0 * (double) this.scaleFactor, 0.0));
    XYZ xyz5 = transform.OfPoint(new XYZ(xyz3.X, xyz3.Y - 3.0 / 64.0 * (double) this.scaleFactor, 0.0));
    XYZ xyz6 = transform.OfPoint(new XYZ(xyz3.X + 3.0 / 64.0 * (double) this.scaleFactor, xyz3.Y, 0.0));
    XYZ xyz7 = transform.OfPoint(new XYZ(xyz3.X - 3.0 / 64.0 * (double) this.scaleFactor, xyz3.Y, 0.0));
    if (boundingBoxXyz1 != null)
    {
      XYZ xyz8 = transform.Inverse.OfPoint(boundingBoxXyz1.Max);
      XYZ xyz9 = transform.Inverse.OfPoint(boundingBoxXyz1.Min);
      XYZ translation = transform.OfVector(xyz2 - new XYZ((xyz8.X + xyz9.X) / 2.0, (xyz8.Y + xyz9.Y) / 2.0, 0.0));
      ElementTransformUtils.MoveElement(this.revitDoc, elem1.Id, translation);
      BoundingBoxXYZ boundingBoxXyz2 = elem1.get_BoundingBox(view);
      if (boundingBoxXyz2 != null)
      {
        XYZ xyz10 = transform.Inverse.OfPoint(boundingBoxXyz2.Max);
        XYZ xyz11 = transform.Inverse.OfPoint(boundingBoxXyz2.Min);
        double num = Math.Abs(xyz10.X - xyz11.X) / 2.0;
        if (Math.Abs(xyz10.Y - xyz11.Y) / 2.0 > num)
          num = Math.Abs(xyz10.Y - xyz11.Y) / 2.0;
        double x = (xyz10.X + xyz11.X) / 2.0;
        double y = (xyz10.Y + xyz11.Y) / 2.0;
        xyz4 = transform.OfPoint(new XYZ(x, y + num, 0.0));
        xyz5 = transform.OfPoint(new XYZ(x, y - num, 0.0));
        xyz6 = transform.OfPoint(new XYZ(x + num, y, 0.0));
        xyz7 = transform.OfPoint(new XYZ(x - num, y, 0.0));
      }
    }
    CurveArray curveArray = new CurveArray();
    curveArray.Append((Curve) Arc.Create(xyz4, xyz5, xyz7));
    curveArray.Append((Curve) Arc.Create(xyz4, xyz5, xyz6));
    foreach (Curve geometryCurve in curveArray)
    {
      DetailCurve elem2 = this.revitDoc.Create.NewDetailCurve(view, geometryCurve);
      if (elem2 != null)
      {
        elem2.LineStyle = (Element) this.insulationDrawingSettingsObject.MarkCircleDetailLineStyle.GetGraphicsStyle(GraphicsStyleType.Projection);
        this.AddExtensibleStorage((Element) elem2);
        this.detailCurves.Add(elem2);
      }
    }
    this.UpdateExtremes(transform.Inverse.OfPoint(xyz4), DimensionEdge.Bottom);
    this.UpdateExtremes(transform.Inverse.OfPoint(xyz5), DimensionEdge.Bottom);
    this.UpdateExtremes(transform.Inverse.OfPoint(xyz6), DimensionEdge.Bottom);
    this.UpdateExtremes(transform.Inverse.OfPoint(xyz7), DimensionEdge.Bottom);
  }

  private void AddExtensibleStorage(Element elem)
  {
    Entity entity = new Entity(this.extensibleStorageScheme);
    entity.Set<string>("InsulationMark", this.InsulationMark);
    elem.SetEntity(entity);
  }

  private string GetInsulationMarkFromExtensibleStorage(Element element)
  {
    string empty = string.Empty;
    if (element.GetEntitySchemaGuids().Contains(new Guid(this.extensibleStorageGuid)))
    {
      Entity entity = element.GetEntity(this.extensibleStorageScheme);
      if (entity != null)
        empty = entity.Get<string>(this.extensibleStorageScheme.GetField("InsulationMark"));
    }
    return empty;
  }

  private void UpdateExtremes(List<XYZ> points, DimensionEdge edge)
  {
    foreach (XYZ point in points)
      this.UpdateExtremes(point, edge);
  }

  private void UpdateExtremes(XYZ point, DimensionEdge edge)
  {
    double x = point.X;
    double y = point.Y;
    if (edge == DimensionEdge.Top || edge == DimensionEdge.Bottom)
    {
      if (x > this.insulationMaxX)
        x = this.insulationMaxX;
      if (x < this.insulationMinX)
        x = this.insulationMinX;
    }
    if (edge == DimensionEdge.Right || edge == DimensionEdge.Left)
    {
      if (y > this.insulationMaxY)
        y = this.insulationMaxY;
      if (y < this.insulationMinY)
        y = this.insulationMinY;
    }
    point = new XYZ(x, y, 0.0);
    this.UpdateExtremeX(point);
    this.UpdateExtremeY(point);
  }

  private void UpdateExtremeX(XYZ point)
  {
    if (point.Y < this.insulationMinY || point.Y > this.insulationMaxY)
      return;
    if (point.X > this.extremeMaxX)
      this.extremeMaxX = point.X;
    if (point.X >= this.extremeMinX)
      return;
    this.extremeMinX = point.X;
  }

  private void UpdateExtremeY(XYZ point)
  {
    if (point.X < this.insulationMinX || point.X > this.insulationMaxX)
      return;
    if (point.Y > this.extremeMaxY)
      this.extremeMaxY = point.Y;
    if (point.Y >= this.extremeMinY)
      return;
    this.extremeMinY = point.Y;
  }

  private void GetRefernceDictionaries(
    Dictionary<XYZ, Reference> OriginalDictionary,
    Dictionary<double, Reference> TopRightDictionary,
    Dictionary<double, Reference> BottomLeftDictionary,
    bool horizontal)
  {
    Dictionary<double, List<XYZ>> dictionary = new Dictionary<double, List<XYZ>>();
    foreach (XYZ key1 in OriginalDictionary.Keys)
    {
      double num = !horizontal ? key1.Y : key1.X;
      bool flag = false;
      foreach (double key2 in dictionary.Keys)
      {
        if (key2.ApproximatelyEquals(num))
        {
          flag = true;
          dictionary[key2].Add(key1);
        }
      }
      if (!flag)
        dictionary.Add(num, new List<XYZ>() { key1 });
    }
    foreach (double key3 in dictionary.Keys)
    {
      List<XYZ> source = dictionary[key3];
      XYZ key4 = source.First<XYZ>();
      XYZ key5 = source.First<XYZ>();
      if (source.Count<XYZ>() > 1)
      {
        foreach (XYZ xyz in source)
        {
          if (horizontal)
          {
            if (xyz.Y > key4.Y)
              key4 = xyz;
            if (xyz.Y < key5.Y)
              key5 = xyz;
          }
          else
          {
            if (xyz.X > key4.X)
              key4 = xyz;
            if (xyz.X < key5.X)
              key5 = xyz;
          }
        }
      }
      if (horizontal)
      {
        TopRightDictionary.Add(key4.X, OriginalDictionary[key4]);
        BottomLeftDictionary.Add(key5.X, OriginalDictionary[key5]);
      }
      else
      {
        TopRightDictionary.Add(key4.Y, OriginalDictionary[key4]);
        BottomLeftDictionary.Add(key5.Y, OriginalDictionary[key5]);
      }
    }
  }

  public bool DrawDetail(View view, XYZ location, bool Transaction = true, bool placeBottomLeft = true)
  {
    if (this.detailDrawn)
      return false;
    this.scaleFactor = view.Scale;
    using (Transaction transaction = new Transaction(this.revitDoc, "Insulation Drawing : " + this.InsulationMark))
    {
      if (Transaction)
      {
        int num1 = (int) transaction.Start();
      }
      Transform transform = new Transform(view.CropBox.Transform);
      transform.Origin = new XYZ(0.0, 0.0, 0.0);
      XYZ newMax;
      XYZ newMin;
      XYZ scaledVector;
      List<CurveArray> curveArrays = this.insulationLineInfo.GetCurveArrays(location, transform, out newMax, out newMin, out scaledVector);
      if (newMax.X > newMin.X)
      {
        this.insulationMaxX = newMax.X;
        this.insulationMinX = newMin.X;
      }
      else
      {
        this.insulationMaxX = newMin.X;
        this.insulationMinX = newMax.X;
      }
      if (newMax.Y > newMin.Y)
      {
        this.insulationMaxY = newMax.Y;
        this.insulationMinY = newMin.Y;
      }
      else
      {
        this.insulationMaxY = newMin.Y;
        this.insulationMinY = newMax.Y;
      }
      XYZ xyz = new XYZ(this.insulationMaxX, this.insulationMaxY, 0.0);
      newMin = new XYZ(this.insulationMinX, this.insulationMinY, 0.0);
      this.UpdateExtremes(xyz, DimensionEdge.Top);
      this.UpdateExtremes(newMin, DimensionEdge.Top);
      Dictionary<XYZ, Reference> dictionary1 = new Dictionary<XYZ, Reference>();
      Dictionary<XYZ, Reference> dictionary2 = new Dictionary<XYZ, Reference>();
      Dictionary<XYZ, Reference> dictionary3 = new Dictionary<XYZ, Reference>();
      Dictionary<XYZ, Reference> dictionary4 = new Dictionary<XYZ, Reference>();
      this.PlaceInsulationDetailLines(view, transform, curveArrays, dictionary1, dictionary2);
      this.PlaceRecess(view, transform, scaledVector, dictionary3, dictionary4);
      this.PlaceRecessTextNotes(view, transform, scaledVector, xyz, newMin);
      Dictionary<double, Reference> TopRightDictionary1 = new Dictionary<double, Reference>();
      Dictionary<double, Reference> BottomLeftDictionary1 = new Dictionary<double, Reference>();
      this.GetRefernceDictionaries(dictionary1, TopRightDictionary1, BottomLeftDictionary1, true);
      Dictionary<double, Reference> TopRightDictionary2 = new Dictionary<double, Reference>();
      Dictionary<double, Reference> BottomLeftDictionary2 = new Dictionary<double, Reference>();
      this.GetRefernceDictionaries(dictionary2, TopRightDictionary2, BottomLeftDictionary2, false);
      Dictionary<double, Reference> TopRightDictionary3 = new Dictionary<double, Reference>();
      Dictionary<double, Reference> BottomLeftDictionary3 = new Dictionary<double, Reference>();
      this.GetRefernceDictionaries(dictionary3, TopRightDictionary3, BottomLeftDictionary3, true);
      Dictionary<double, Reference> TopRightDictionary4 = new Dictionary<double, Reference>();
      Dictionary<double, Reference> BottomLeftDictionary4 = new Dictionary<double, Reference>();
      this.GetRefernceDictionaries(dictionary4, TopRightDictionary4, BottomLeftDictionary4, false);
      if (TopRightDictionary1.Count > 2)
      {
        ReferenceArray referenceArray = new ReferenceArray();
        foreach (Reference reference in TopRightDictionary1.Values)
          referenceArray.Append(reference);
        this.AddDimension(view, transform, referenceArray, this.insulationDrawingSettingsObject.GeneralDimensionStyle, DimensionEdge.Top);
      }
      if (BottomLeftDictionary2.Count > 2)
      {
        ReferenceArray referenceArray = new ReferenceArray();
        foreach (Reference reference in BottomLeftDictionary2.Values)
          referenceArray.Append(reference);
        this.AddDimension(view, transform, referenceArray, this.insulationDrawingSettingsObject.GeneralDimensionStyle, DimensionEdge.Left, 2.0);
      }
      Reference reference1 = (Reference) null;
      Reference reference2 = (Reference) null;
      Reference reference3 = (Reference) null;
      Reference reference4 = (Reference) null;
      Reference reference5 = (Reference) null;
      Reference reference6 = (Reference) null;
      Reference reference7 = (Reference) null;
      Reference reference8 = (Reference) null;
      if (TopRightDictionary1.ContainsKey(this.insulationMaxX))
        reference1 = TopRightDictionary1[this.insulationMaxX];
      if (TopRightDictionary1.ContainsKey(this.insulationMinX))
        reference2 = TopRightDictionary1[this.insulationMinX];
      if (BottomLeftDictionary1.ContainsKey(this.insulationMaxX))
        reference3 = BottomLeftDictionary1[this.insulationMaxX];
      if (BottomLeftDictionary1.ContainsKey(this.insulationMinX))
        reference4 = BottomLeftDictionary1[this.insulationMinX];
      if (TopRightDictionary2.ContainsKey(this.insulationMaxY))
        reference7 = TopRightDictionary2[this.insulationMaxY];
      if (TopRightDictionary2.ContainsKey(this.insulationMinY))
        reference8 = TopRightDictionary2[this.insulationMinY];
      if (BottomLeftDictionary2.ContainsKey(this.insulationMaxY))
        reference5 = BottomLeftDictionary2[this.insulationMaxY];
      if (BottomLeftDictionary2.ContainsKey(this.insulationMinY))
        reference6 = BottomLeftDictionary2[this.insulationMinY];
      if (reference1 == null || reference2 == null)
      {
        List<double> list = TopRightDictionary1.Keys.ToList<double>();
        list.Sort();
        if (reference1 == null)
          reference1 = TopRightDictionary1[list.Last<double>()];
        if (reference2 == null)
          reference2 = TopRightDictionary1[list.First<double>()];
      }
      if (reference3 == null || reference4 == null)
      {
        List<double> list = BottomLeftDictionary1.Keys.ToList<double>();
        list.Sort();
        if (reference3 == null)
          reference3 = BottomLeftDictionary1[list.Last<double>()];
        if (reference4 == null)
          reference4 = BottomLeftDictionary1[list.First<double>()];
      }
      if (reference5 == null || reference6 == null)
      {
        List<double> list = BottomLeftDictionary2.Keys.ToList<double>();
        list.Sort();
        if (reference5 == null)
          reference5 = BottomLeftDictionary2[list.Last<double>()];
        if (reference6 == null)
          reference6 = BottomLeftDictionary2[list.First<double>()];
      }
      if (reference7 == null || reference8 == null)
      {
        List<double> list = TopRightDictionary2.Keys.ToList<double>();
        list.Sort();
        if (reference7 == null)
          reference7 = TopRightDictionary2[list.Last<double>()];
        if (reference8 == null)
          reference8 = TopRightDictionary2[list.First<double>()];
      }
      ReferenceArray referenceArray1 = new ReferenceArray();
      referenceArray1.Append(reference1);
      referenceArray1.Append(reference2);
      ReferenceArray referenceArray2 = new ReferenceArray();
      referenceArray2.Append(reference5);
      referenceArray2.Append(reference6);
      ReferenceArray referenceArray3 = new ReferenceArray();
      referenceArray3.Append(reference7);
      referenceArray3.Append(reference8);
      List<double> list1 = TopRightDictionary2.Keys.ToList<double>();
      list1.Sort();
      double other1 = list1.First<double>();
      double other2 = list1.Last<double>();
      foreach (double key in TopRightDictionary4.Keys)
      {
        if (!key.ApproximatelyEquals(other1, 0.001) && !key.ApproximatelyEquals(other2, 0.001))
          referenceArray3.Append(TopRightDictionary4[key]);
      }
      ReferenceArray referenceArray4 = new ReferenceArray();
      referenceArray4.Append(reference3);
      referenceArray4.Append(reference4);
      List<double> list2 = BottomLeftDictionary1.Keys.ToList<double>();
      list2.Sort();
      double other3 = list2.First<double>();
      double other4 = list2.Last<double>();
      foreach (double key in BottomLeftDictionary3.Keys)
      {
        if (!key.ApproximatelyEquals(other3, 0.001) && !key.ApproximatelyEquals(other4, 0.001))
          referenceArray4.Append(BottomLeftDictionary3[key]);
      }
      this.AddDimension(view, transform, referenceArray1, this.insulationDrawingSettingsObject.OverallDimensionStyle, DimensionEdge.Top);
      this.AddDimension(view, transform, referenceArray2, this.insulationDrawingSettingsObject.OverallDimensionStyle, DimensionEdge.Left);
      if (referenceArray3.Size > 2)
        this.AddDimension(view, transform, referenceArray3, this.insulationDrawingSettingsObject.GeneralDimensionStyle, DimensionEdge.Right);
      if (referenceArray4.Size > 2)
        this.AddDimension(view, transform, referenceArray4, this.insulationDrawingSettingsObject.GeneralDimensionStyle, DimensionEdge.Bottom);
      this.AddInsulationMarkAnnotations(view, transform, xyz, newMin);
      XYZ translation = location - transform.OfPoint(new XYZ(this.extremeMinX, this.extremeMinY, 0.0));
      List<ElementId> elementsToMove = new List<ElementId>();
      foreach (TextNote textNote in this.textNotes)
        elementsToMove.Add(textNote.Id);
      foreach (DetailCurve detailCurve in this.detailCurves)
        elementsToMove.Add(detailCurve.Id);
      foreach (Dimension dimension in this.dimensions)
        elementsToMove.Add(dimension.Id);
      this.center = new XYZ((this.extremeMaxX + this.extremeMinX) / 2.0, (this.extremeMaxY + this.extremeMinY) / 2.0, 0.0);
      if (placeBottomLeft)
      {
        ElementTransformUtils.MoveElements(this.revitDoc, (ICollection<ElementId>) elementsToMove, translation);
        this.center += translation;
      }
      if (Transaction)
      {
        int num2 = (int) transaction.Commit();
      }
    }
    this.detailDrawn = true;
    return true;
  }

  public XYZ GetDrawingCenter(out double height, out double width)
  {
    height = this.extremeMaxY - this.extremeMinY;
    width = this.extremeMaxX - this.extremeMinX;
    return this.center;
  }

  public XYZ GetDrawingCenter() => this.center;

  public void MoveDrawing(XYZ newLocation)
  {
    double x = this.center.X - (this.extremeMaxX - this.extremeMinX) / 2.0;
    double y = this.center.Y - (this.extremeMaxY - this.extremeMinY) / 2.0;
    XYZ translation = newLocation - new XYZ(x, y, 0.0);
    List<ElementId> elementsToMove = new List<ElementId>();
    foreach (TextNote textNote in this.textNotes)
      elementsToMove.Add(textNote.Id);
    foreach (DetailCurve detailCurve in this.detailCurves)
      elementsToMove.Add(detailCurve.Id);
    foreach (Dimension dimension in this.dimensions)
      elementsToMove.Add(dimension.Id);
    ElementTransformUtils.MoveElements(this.revitDoc, (ICollection<ElementId>) elementsToMove, translation);
    this.center += translation;
  }

  public void DeleteDrawing()
  {
    if (!this.detailDrawn)
      return;
    this.extremeMaxX = double.MinValue;
    this.extremeMaxY = double.MinValue;
    this.extremeMinX = double.MaxValue;
    this.extremeMinY = double.MaxValue;
    this.insulationMaxX = double.MinValue;
    this.insulationMaxY = double.MinValue;
    this.insulationMinX = double.MaxValue;
    this.insulationMinY = double.MaxValue;
    this.center = XYZ.Zero;
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (TextNote textNote in this.textNotes)
      elementIdList.Add(textNote.Id);
    foreach (DetailCurve detailCurve in this.detailCurves)
      elementIdList.Add(detailCurve.Id);
    foreach (Dimension dimension in this.dimensions)
      elementIdList.Add(dimension.Id);
    this.revitDoc.Delete((ICollection<ElementId>) elementIdList);
    this.textNotes.Clear();
    this.detailCurves.Clear();
    this.dimensions.Clear();
    this.detailDrawn = false;
  }

  private List<CurveArray> GetOuterCurveArrays(List<CurveArray> arrayList)
  {
    if (arrayList.Count < 2)
      return arrayList;
    List<CurveArray> outerCurveArrays = new List<CurveArray>();
    Dictionary<CurveArray, CurveArrayBoundingObject> dictionary = new Dictionary<CurveArray, CurveArrayBoundingObject>();
    foreach (CurveArray array1 in arrayList)
    {
      bool flag1 = true;
      foreach (CurveArray array2 in arrayList)
      {
        bool flag2 = false;
        foreach (Curve curve1 in array1)
        {
          foreach (Curve curve2 in array2)
          {
            IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
            if (curve1.Intersect(curve2) == SetComparisonResult.Overlap)
            {
              flag2 = true;
              break;
            }
          }
          if (flag2)
            break;
        }
        if (!flag2)
        {
          CurveArrayBoundingObject arrayBoundingObject;
          if (dictionary.ContainsKey(array1))
          {
            arrayBoundingObject = dictionary[array1];
          }
          else
          {
            arrayBoundingObject = new CurveArrayBoundingObject(array1);
            dictionary.Add(array1, arrayBoundingObject);
          }
          CurveArrayBoundingObject object2;
          if (dictionary.ContainsKey(array2))
          {
            object2 = dictionary[array2];
          }
          else
          {
            object2 = new CurveArrayBoundingObject(array2);
            dictionary.Add(array2, object2);
          }
          if (arrayBoundingObject.IsWithin(object2))
          {
            flag1 = false;
            break;
          }
        }
      }
      if (flag1)
        outerCurveArrays.Add(array1);
    }
    return outerCurveArrays;
  }
}
