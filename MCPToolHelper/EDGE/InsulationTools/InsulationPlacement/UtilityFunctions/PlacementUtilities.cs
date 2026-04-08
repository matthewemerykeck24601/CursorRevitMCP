// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.PlacementUtilities
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AdminTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Utils.ElementUtils;
using Utils.FamilyUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;

public class PlacementUtilities
{
  public static Element SelectWallPanel(Document revitDoc, UIDocument uiDoc)
  {
    try
    {
      Reference reference = uiDoc.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new InsulationPlacementFilter(), "Pick Structural Framing Element For Which To Manually Place Insulation");
      Element element = (Element) null;
      if (revitDoc.GetElement(reference) != null)
        element = revitDoc.GetElement(reference);
      return element;
    }
    catch
    {
      return (Element) null;
    }
  }

  public static Element FilterWallPanel(Element selection, out ManualPlacementErrorType mpe)
  {
    Element element = (Element) null;
    ManualPlacementErrorType placementErrorType = ManualPlacementErrorType.NoMessage;
    if (Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(selection))
      element = selection;
    else
      placementErrorType = ManualPlacementErrorType.NoValidElement;
    mpe = placementErrorType;
    return element;
  }

  public static void GetAllTopFacesFromInsulation(
    Document revitDoc,
    Element insulation,
    out PlanarFace topFace,
    out PlanarFace botFace,
    out bool bSymbol,
    out List<PlanarFace> allTopFaces)
  {
    List<PlanarFace> planarFaceList1 = new List<PlanarFace>();
    allTopFaces = planarFaceList1;
    topFace = (PlanarFace) null;
    botFace = (PlanarFace) null;
    List<Solid> symbolSolids = Solids.GetSymbolSolids(insulation, out bSymbol, options: new Options()
    {
      ComputeReferences = true
    });
    string parameterAsString = Parameters.GetParameterAsString(insulation, "HOST_GUID");
    Element element = revitDoc.GetElement(parameterAsString);
    Transform transform1 = (element as FamilyInstance).GetTransform();
    Transform transform2 = (insulation as FamilyInstance).GetTransform();
    bool flag = PlacementUtilities.IsMirrored(element);
    XYZ source = flag ? -XYZ.BasisY : XYZ.BasisY;
    List<PlanarFace> planarFaceList2 = new List<PlanarFace>();
    if (symbolSolids.Count<Solid>() == 0)
      return;
    TransformedFace transformedFace1 = (TransformedFace) null;
    TransformedFace transformedFace2 = (TransformedFace) null;
    if (bSymbol)
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace f = face as PlanarFace;
            TransformedFace transformedFace3 = new TransformedFace(transform2, transform1, f);
            XYZ origin = transformedFace3.Origin;
            if (transformedFace3.Normal.IsAlmostEqualTo(source))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
              double y1 = transformedFace3.Origin.Y;
              double y2 = transformedFace1.Origin.Y;
              allTopFaces.Add(f);
              if (flag)
              {
                if (y1 < y2)
                {
                  transformedFace1 = transformedFace3;
                  topFace = f;
                }
              }
              else if (y1 > y2)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
            }
            else if (transformedFace3.Normal.IsAlmostEqualTo(-source))
            {
              if (transformedFace2 == null)
              {
                transformedFace2 = transformedFace3;
                botFace = f;
              }
              double y3 = transformedFace3.Origin.Y;
              double y4 = transformedFace2.Origin.Y;
              if (flag)
              {
                if (y3 > y4)
                {
                  transformedFace2 = transformedFace3;
                  botFace = f;
                }
              }
              else if (y3 < y4)
              {
                transformedFace2 = transformedFace3;
                botFace = f;
              }
            }
          }
        }
      }
    }
    else
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace f = face as PlanarFace;
            TransformedFace transformedFace4 = new TransformedFace((element as FamilyInstance).GetTransform(), f);
            if (transformedFace4.Normal.IsAlmostEqualTo(source))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace4;
                topFace = f;
              }
              double y5 = transformedFace4.Origin.Y;
              double y6 = transformedFace1.Origin.Y;
              allTopFaces.Add(f);
              if (flag)
              {
                if (y5 < y6)
                {
                  transformedFace1 = transformedFace4;
                  topFace = f;
                }
              }
              else if (y5 > y6)
              {
                transformedFace1 = transformedFace4;
                topFace = f;
              }
            }
            else if (transformedFace4.Normal.IsAlmostEqualTo(-source))
            {
              if (transformedFace2 == null)
              {
                transformedFace2 = transformedFace4;
                botFace = f;
              }
              double y7 = transformedFace4.Origin.Y;
              double y8 = transformedFace2.Origin.Y;
              if (flag)
              {
                if (y7 > y8)
                {
                  transformedFace2 = transformedFace4;
                  botFace = f;
                }
              }
              else if (y7 < y8)
              {
                transformedFace2 = transformedFace4;
                botFace = f;
              }
            }
          }
        }
      }
    }
  }

  public static void GetInsulationFace(
    Document revitDoc,
    Element insulation,
    out PlanarFace topFace,
    out PlanarFace botFace)
  {
    PlacementUtilities.GetInsulationFace(revitDoc, insulation, out topFace, out botFace, out bool _);
  }

  public static void GetInsulationFace(
    Document revitDoc,
    Element insulation,
    out PlanarFace topFace,
    out PlanarFace botFace,
    out bool bSymbol)
  {
    topFace = (PlanarFace) null;
    botFace = (PlanarFace) null;
    List<Solid> symbolSolids = Solids.GetSymbolSolids(insulation, out bSymbol, options: new Options()
    {
      ComputeReferences = true
    });
    string parameterAsString = Parameters.GetParameterAsString(insulation, "HOST_GUID");
    Element element = revitDoc.GetElement(parameterAsString);
    Transform transform1 = (element as FamilyInstance).GetTransform();
    Transform transform2 = (insulation as FamilyInstance).GetTransform();
    bool flag = PlacementUtilities.IsMirrored(element);
    XYZ source = flag ? -XYZ.BasisY : XYZ.BasisY;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    if (symbolSolids.Count<Solid>() == 0)
      return;
    TransformedFace transformedFace1 = (TransformedFace) null;
    TransformedFace transformedFace2 = (TransformedFace) null;
    if (bSymbol)
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace f = face as PlanarFace;
            TransformedFace transformedFace3 = new TransformedFace(transform2, transform1, f);
            XYZ origin = transformedFace3.Origin;
            if (transformedFace3.Normal.IsAlmostEqualTo(source))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
              double y1 = transformedFace3.Origin.Y;
              double y2 = transformedFace1.Origin.Y;
              if (flag)
              {
                if (y1 < y2)
                {
                  transformedFace1 = transformedFace3;
                  topFace = f;
                }
              }
              else if (y1 > y2)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
            }
            else if (transformedFace3.Normal.IsAlmostEqualTo(-source))
            {
              if (transformedFace2 == null)
              {
                transformedFace2 = transformedFace3;
                botFace = f;
              }
              double y3 = transformedFace3.Origin.Y;
              double y4 = transformedFace2.Origin.Y;
              if (flag)
              {
                if (y3 > y4)
                {
                  transformedFace2 = transformedFace3;
                  botFace = f;
                }
              }
              else if (y3 < y4)
              {
                transformedFace2 = transformedFace3;
                botFace = f;
              }
            }
          }
        }
      }
    }
    else
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace f = face as PlanarFace;
            TransformedFace transformedFace4 = new TransformedFace((element as FamilyInstance).GetTransform(), f);
            if (transformedFace4.Normal.IsAlmostEqualTo(source))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace4;
                topFace = f;
              }
              double y5 = transformedFace4.Origin.Y;
              double y6 = transformedFace1.Origin.Y;
              if (flag)
              {
                if (y5 < y6)
                {
                  transformedFace1 = transformedFace4;
                  topFace = f;
                }
              }
              else if (y5 > y6)
              {
                transformedFace1 = transformedFace4;
                topFace = f;
              }
            }
            else if (transformedFace4.Normal.IsAlmostEqualTo(-source))
            {
              if (transformedFace2 == null)
              {
                transformedFace2 = transformedFace4;
                botFace = f;
              }
              double y7 = transformedFace4.Origin.Y;
              double y8 = transformedFace2.Origin.Y;
              if (flag)
              {
                if (y7 > y8)
                {
                  transformedFace2 = transformedFace4;
                  botFace = f;
                }
              }
              else if (y7 < y8)
              {
                transformedFace2 = transformedFace4;
                botFace = f;
              }
            }
          }
        }
      }
    }
  }

  public static PlanarFace GetFrontFace(Element wall)
  {
    return PlacementUtilities.GetFrontFace(wall, out bool _);
  }

  public static PlanarFace GetFrontFace(Element wall, out bool bSymbol)
  {
    PlanarFace frontFace = (PlanarFace) null;
    List<Solid> symbolSolids = Solids.GetSymbolSolids(wall, out bSymbol, options: new Options()
    {
      ComputeReferences = true
    });
    XYZ source = PlacementUtilities.IsMirrored(wall) ? -XYZ.BasisY : XYZ.BasisY;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    if (bSymbol)
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace planarFace = face as PlanarFace;
            if (face.ComputeNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(source))
            {
              if ((GeometryObject) frontFace == (GeometryObject) null)
                frontFace = planarFace;
              double y1 = planarFace.Origin.Y;
              double y2 = frontFace.Origin.Y;
              if (y1.ApproximatelyEquals(0.0))
                frontFace = planarFace;
            }
          }
        }
      }
    }
    else
    {
      TransformedFace transformedFace1 = (TransformedFace) null;
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace f = face as PlanarFace;
            TransformedFace transformedFace2 = new TransformedFace((wall as FamilyInstance).GetTransform(), f);
            if (transformedFace2.Normal.IsAlmostEqualTo(source))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace2;
                frontFace = f;
              }
              double y3 = transformedFace2.Origin.Y;
              double y4 = transformedFace1.Origin.Y;
              if (y3.ApproximatelyEquals(0.0))
              {
                transformedFace1 = transformedFace2;
                frontFace = f;
              }
            }
          }
        }
      }
    }
    return frontFace;
  }

  public static PlanarFace GetValidLoopFace(Element wall, Insulation insul)
  {
    PlanarFace planarFace1 = (PlanarFace) null;
    double wytheInner = insul.WytheInner;
    bool flag = PlacementUtilities.IsMirrored(wall);
    XYZ source = flag ? -XYZ.BasisY : XYZ.BasisY;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    TransformedFace transformedFace1 = (TransformedFace) null;
    foreach (Face face in insul.UnionedSolid.Faces)
    {
      if (face is PlanarFace)
      {
        PlanarFace f = face as PlanarFace;
        TransformedFace transformedFace2 = new TransformedFace((wall as FamilyInstance).GetTransform(), f);
        if ((-transformedFace2.Normal).IsAlmostEqualTo(source) && Math.Abs(wytheInner).ApproximatelyEquals(Math.Abs(transformedFace2.Origin.Y), 1E-07))
        {
          if (transformedFace1 == null)
          {
            transformedFace1 = transformedFace2;
            planarFace1 = f;
          }
          double y1 = transformedFace2.Origin.Y;
          double y2 = transformedFace1.Origin.Y;
          if (flag)
          {
            if (y1 <= y2)
            {
              transformedFace1 = transformedFace2;
              PlanarFace planarFace2 = f;
              planarFaceList.Add(planarFace2);
            }
          }
          else if (y1 >= y2)
          {
            transformedFace1 = transformedFace2;
            PlanarFace planarFace3 = f;
            planarFaceList.Add(planarFace3);
          }
        }
      }
    }
    PlanarFace planarFace4 = (PlanarFace) null;
    PlanarFace validLoopFace = (PlanarFace) null;
    (wall as FamilyInstance).GetTransform();
    foreach (PlanarFace planarFace5 in planarFaceList)
    {
      if ((GeometryObject) planarFace4 == (GeometryObject) null)
      {
        planarFace4 = planarFace5;
      }
      else
      {
        if (PlacementUtilities.GetOuterLoop(planarFace5.GetEdgesAsCurveLoops().ToList<CurveLoop>()).Equals((object) PlacementUtilities.GetOuterLoop(planarFace4.GetEdgesAsCurveLoops().ToList<CurveLoop>())))
        {
          validLoopFace = planarFace4;
          break;
        }
        planarFace4 = planarFace5;
      }
    }
    if ((GeometryObject) validLoopFace == (GeometryObject) null)
      validLoopFace = planarFace4;
    return validLoopFace;
  }

  public static PlanarFace GetInnerFace(Element wall, Insulation insul)
  {
    PlanarFace planarFace1 = (PlanarFace) null;
    double wytheInner = insul.WytheInner;
    bool flag = PlacementUtilities.IsMirrored(wall);
    XYZ source = flag ? -XYZ.BasisY : XYZ.BasisY;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    TransformedFace transformedFace1 = (TransformedFace) null;
    foreach (Face face in insul.UnionedSolid.Faces)
    {
      if (face is PlanarFace)
      {
        PlanarFace f = face as PlanarFace;
        TransformedFace transformedFace2 = new TransformedFace((wall as FamilyInstance).GetTransform(), f);
        if (transformedFace2.Normal.IsAlmostEqualTo(source) && Math.Abs(transformedFace2.Origin.Y).ApproximatelyEquals(Math.Abs(Math.Abs(wytheInner) + insul.InsulThickness), 1E-07))
        {
          if (transformedFace1 == null)
          {
            transformedFace1 = transformedFace2;
            planarFace1 = f;
          }
          double y1 = transformedFace2.Origin.Y;
          double y2 = transformedFace1.Origin.Y;
          if (flag)
          {
            if (y1 >= y2)
            {
              transformedFace1 = transformedFace2;
              PlanarFace planarFace2 = f;
              planarFaceList.Add(planarFace2);
            }
          }
          else if (y1 <= y2)
          {
            transformedFace1 = transformedFace2;
            PlanarFace planarFace3 = f;
            planarFaceList.Add(planarFace3);
          }
        }
      }
    }
    PlanarFace planarFace4 = (PlanarFace) null;
    PlanarFace innerFace = (PlanarFace) null;
    (wall as FamilyInstance).GetTransform();
    foreach (PlanarFace planarFace5 in planarFaceList)
    {
      if ((GeometryObject) planarFace4 == (GeometryObject) null)
      {
        planarFace4 = planarFace5;
      }
      else
      {
        if (PlacementUtilities.GetOuterLoop(planarFace5.GetEdgesAsCurveLoops().ToList<CurveLoop>()).Equals((object) PlacementUtilities.GetOuterLoop(planarFace4.GetEdgesAsCurveLoops().ToList<CurveLoop>())))
        {
          innerFace = planarFace4;
          break;
        }
        planarFace4 = planarFace5;
      }
    }
    if ((GeometryObject) innerFace == (GeometryObject) null)
      innerFace = planarFace4;
    return innerFace;
  }

  public static void RevertIsolation(Document revitDoc)
  {
    using (Transaction transaction = new Transaction(revitDoc, "Revert Isolation"))
    {
      int num1 = (int) transaction.Start();
      revitDoc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
      int num2 = (int) transaction.Commit();
    }
  }

  public static ManualPlacementErrorType CheckView(
    Document revitDoc,
    UIDocument uiDoc,
    Element wall,
    PlanarFace face,
    bool faceSymbol)
  {
    ManualPlacementErrorType placementErrorType = ManualPlacementErrorType.NoMessage;
    if (revitDoc.ActiveView.ViewType == ViewType.ThreeD)
      return PlacementUtilities.ZoomToWall(revitDoc, uiDoc, wall, face, faceSymbol);
    if (!revitDoc.ActiveView.ViewDirection.IsAlmostEqualTo((wall as FamilyInstance).GetTransform().BasisY) && !revitDoc.ActiveView.ViewDirection.IsAlmostEqualTo((wall as FamilyInstance).GetTransform().BasisY.Negate()) || !((GeometryObject) face != (GeometryObject) null))
      return ManualPlacementErrorType.InvalidView;
    uiDoc.ShowElements((ICollection<ElementId>) new List<ElementId>()
    {
      wall.Id
    });
    using (Transaction transaction = new Transaction(revitDoc, "Set Plane"))
    {
      int num1 = (int) transaction.Start();
      SketchPlane sketchPlane = SketchPlane.Create(revitDoc, face.Reference);
      uiDoc.ActiveView.SketchPlane = sketchPlane;
      uiDoc.ActiveView.SketchPlane.GetPlane();
      int num2 = (int) transaction.Commit();
    }
    return placementErrorType;
  }

  public static ManualPlacementErrorType ZoomToWall(
    Document revitDoc,
    UIDocument uiDoc,
    Element wall,
    PlanarFace face,
    bool faceSymbol)
  {
    Transform transform = (wall as FamilyInstance).GetTransform();
    using (Transaction transaction = new Transaction(revitDoc, "Set Plane"))
    {
      int num1 = (int) transaction.Start();
      SketchPlane sketchPlane = SketchPlane.Create(revitDoc, face.Reference);
      uiDoc.ActiveView.SketchPlane = sketchPlane;
      uiDoc.ActiveView.SketchPlane.GetPlane();
      View3D activeView = revitDoc.ActiveView as View3D;
      XYZ elemIdCenter = Utils.MiscUtils.MiscUtils.GetElemIdCenter((ICollection<ElementId>) new List<ElementId>()
      {
        wall.Id
      }, revitDoc);
      List<UIView> list = uiDoc.GetOpenUIViews().ToList<UIView>();
      UIView uiView1 = (UIView) null;
      foreach (UIView uiView2 in list)
      {
        if (uiView2.ViewId.Equals((object) activeView.Id))
          uiView1 = uiView2;
      }
      XYZ xyz = faceSymbol ? transform.OfVector(face.FaceNormal) : face.FaceNormal;
      ViewOrientation3D newViewOrientation3D = new ViewOrientation3D(elemIdCenter + xyz * 2.0, transform.BasisZ, -xyz);
      if (activeView.IsLocked)
        return ManualPlacementErrorType.UnlockView;
      if (!activeView.IsLocked)
      {
        activeView.SetOrientation(newViewOrientation3D);
        BoundingBoxXYZ boundingBox = Utils.MiscUtils.MiscUtils.GetBoundingBox((ICollection<ElementId>) new List<ElementId>()
        {
          wall.Id
        }, revitDoc);
        uiView1.ZoomAndCenterRectangle(boundingBox.Max, boundingBox.Min);
      }
      int num2 = (int) transaction.Commit();
    }
    return ManualPlacementErrorType.NoMessage;
  }

  public static bool IsolateWall(Element wall, UIDocument uiDoc, Document revitDoc)
  {
    TaskDialog taskDialog = new TaskDialog("Isolate Elements");
    taskDialog.MainInstruction = "Would you like to temporarily isolate the view?";
    taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Isolate Assembly", "Include all assembly elements in the isolation group.");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Isolate Insulation", "Include only the structural framing element, solid zones, and existing insulation in the isolation group.");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Do Not Isolate");
    TaskDialogResult taskDialogResult = taskDialog.Show();
    if (taskDialogResult == 1003)
      return true;
    if (taskDialogResult != 1001 && taskDialogResult != 1002)
      return false;
    List<ElementId> elementIdList = new List<ElementId>();
    elementIdList.Add(wall.Id);
    string wallGuid = wall.UniqueId;
    if (!string.IsNullOrWhiteSpace(wallGuid))
    {
      if (taskDialogResult == 1001 && revitDoc.GetElement(wall.AssemblyInstanceId) is AssemblyInstance element1)
      {
        ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_SpecialityEquipment,
          BuiltInCategory.OST_GenericModel
        });
        ICollection<ElementId> memberIds = element1.GetMemberIds();
        if (memberIds.Count > 0)
        {
          ICollection<ElementId> elementIds = new FilteredElementCollector(revitDoc, memberIds).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).ToElementIds();
          elementIdList.AddRange((IEnumerable<ElementId>) elementIds);
        }
      }
      List<Element> list = PlacementUtilities.RetrieveInsulation(revitDoc).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsString(e, "HOST_GUID").Contains(wallGuid))).ToList<Element>();
      List<ElementId> collection = new List<ElementId>();
      foreach (Element element2 in list)
        collection.Add(element2.Id);
      elementIdList.AddRange((IEnumerable<ElementId>) collection);
    }
    using (Transaction transaction = new Transaction(revitDoc, "Isolate Elems"))
    {
      int num1 = (int) transaction.Start();
      uiDoc.ActiveView.IsolateElementsTemporary((ICollection<ElementId>) elementIdList);
      int num2 = (int) transaction.Commit();
    }
    return true;
  }

  public static List<Element> RetrieveInsulation(Document revitDoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_GenericModel
    });
    return new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (s =>
    {
      string parameterAsString = Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT");
      string familyName = (s as FamilyInstance).Symbol.FamilyName;
      return parameterAsString.ToUpper().Contains("INSULATION") || familyName.ToUpper().Contains("SOLID") && familyName.ToUpper().Contains("ZONE") || parameterAsString.ToUpper().Contains("WOOD") && parameterAsString.ToUpper().Contains("NAILER") || familyName.ToUpper().Contains("WOOD") && familyName.ToUpper().Contains("NAILER");
    })).ToList<Element>();
  }

  public static Tuple<XYZ, XYZ> RetrieveClickPoints(UIDocument uiDoc)
  {
    try
    {
      return new Tuple<XYZ, XYZ>(uiDoc.Selection.PickPoint("Please select the first point for Insulation Placement"), uiDoc.Selection.PickPoint("Please select the second point for Insulation Placement"));
    }
    catch (Exception ex)
    {
      return (Tuple<XYZ, XYZ>) null;
    }
  }

  public static XYZ RetrieveClickPoint(UIDocument uiDoc)
  {
    try
    {
      return uiDoc.Selection.PickPoint("Please select the second point for Insulation Placement");
    }
    catch (Exception ex)
    {
      return (XYZ) null;
    }
  }

  public static Tuple<XYZ, XYZ> ApplyFamilyTransform(
    Tuple<XYZ, XYZ> clickPoints,
    Transform wallTransformInverse)
  {
    return new Tuple<XYZ, XYZ>(wallTransformInverse.OfPoint(clickPoints.Item1), wallTransformInverse.OfPoint(clickPoints.Item2));
  }

  public static TaskDialogResult UseDefaultDialog(string defaultValue, out bool bDontShow)
  {
    TaskDialog taskDialog = new TaskDialog("Insulation Placement");
    taskDialog.Title = "Insulation Placement - Use Default Value";
    taskDialog.MainInstruction = $"Would you like to use the default insulation family ({defaultValue}) for this project? Select No to choose a different insulation family.";
    taskDialog.CommonButtons = (TaskDialogCommonButtons) 6;
    taskDialog.ExtraCheckBoxText = "Do not show again";
    TaskDialogResult taskDialogResult = taskDialog.Show();
    bDontShow = taskDialog.WasExtraCheckBoxChecked();
    return taskDialogResult;
  }

  public static TaskDialogResult SetDefaultDialog(string insName)
  {
    return new TaskDialog("Insulation Placement")
    {
      Title = "Insulation Placement - Set Default Value",
      MainInstruction = $"Would you like to set {insName} as the default insulation family for this project",
      CommonButtons = ((TaskDialogCommonButtons) 6)
    }.Show();
  }

  public static Family GetInsulFamily(Document revitDoc)
  {
    List<Element> insulationFamilies = PlacementUtilities.GetUniqueInsulationFamilies(revitDoc);
    if (insulationFamilies.Count == 0)
    {
      new TaskDialog("Insulation Placement")
      {
        MainContent = "No insulation families were found in the model. Please ensure their MANUFACTURE_COMPONENT parameter contains EDGE and INSULATION and that they also have all other relevant parameters for processing"
      }.Show();
      return (Family) null;
    }
    Family insulFamily = (Family) null;
    using (Transaction transaction = new Transaction(revitDoc, "Family Selection"))
    {
      int num1 = (int) transaction.Start();
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("DEFAULT_INSULATION");
      if (parameter != null)
      {
        string str = parameter != null ? parameter.AsString() : "";
        if (!string.IsNullOrEmpty(str) && PlacementUtilities.defaultExists(revitDoc, str) == null)
        {
          if (new TaskDialog("Insulation Placement")
          {
            MainContent = "The family specified for DEFAULT_INSULATION in Project Information does not exist in the model. Continue?",
            CommonButtons = ((TaskDialogCommonButtons) 9)
          }.Show() != 1)
            return (Family) null;
          str = "";
        }
        if (string.IsNullOrEmpty(str))
        {
          if (insulationFamilies.Count<Element>() == 1)
          {
            insulFamily = PlacementUtilities.FamilySelection(revitDoc);
            if (insulFamily == null)
              return (Family) null;
            if (!insulFamily.Name.Equals(str))
            {
              TaskDialogResult taskDialogResult = PlacementUtilities.SetDefaultDialog(insulFamily.Name);
              if (taskDialogResult == 6)
                parameter.Set(insulFamily.Name);
              if (taskDialogResult == 2)
                return (Family) null;
            }
          }
          else
          {
            insulFamily = PlacementUtilities.FamilySelection(revitDoc);
            if (insulFamily == null)
              return (Family) null;
            if (!insulFamily.Name.Equals(str))
            {
              TaskDialogResult taskDialogResult = PlacementUtilities.SetDefaultDialog(insulFamily.Name);
              if (taskDialogResult == 6)
                parameter.Set(insulFamily.Name);
              if (taskDialogResult == 2)
                return (Family) null;
            }
          }
        }
        else if (insulationFamilies.Count == 1)
        {
          bool flag = false;
          foreach (Family family in insulationFamilies)
          {
            if (family.Name.Equals(str))
            {
              insulFamily = family;
              flag = true;
            }
          }
          if (!flag)
            str = insulationFamilies.First<Element>().Name;
          insulFamily = PlacementUtilities.defaultExists(revitDoc, str);
        }
        else
        {
          insulFamily = PlacementUtilities.defaultExists(revitDoc, str);
          if (insulFamily != null && !App.defaultInsulationUseTrue && !App.defaultInsulationUseFalse)
          {
            bool bDontShow;
            TaskDialogResult taskDialogResult1 = PlacementUtilities.UseDefaultDialog(str, out bDontShow);
            if (taskDialogResult1 == 2)
              return (Family) null;
            if (taskDialogResult1 == 7)
            {
              if (bDontShow)
                App.defaultInsulationUseFalse = true;
              insulFamily = PlacementUtilities.FamilySelection(revitDoc);
              if (insulFamily == null)
                return (Family) null;
              if (!insulFamily.Name.Equals(str))
              {
                TaskDialogResult taskDialogResult2 = PlacementUtilities.SetDefaultDialog(insulFamily.Name);
                if (taskDialogResult2 == 6)
                  parameter.Set(insulFamily.Name);
                if (taskDialogResult2 == 2)
                  return (Family) null;
              }
            }
            else if (bDontShow)
              App.defaultInsulationUseTrue = true;
          }
          else if (App.defaultInsulationUseFalse || insulFamily == null)
          {
            insulFamily = PlacementUtilities.FamilySelection(revitDoc);
            if (insulFamily == null)
              return (Family) null;
            if (!insulFamily.Name.Equals(str))
            {
              TaskDialogResult taskDialogResult = PlacementUtilities.SetDefaultDialog(insulFamily.Name);
              if (taskDialogResult == 6)
                parameter.Set(insulFamily.Name);
              if (taskDialogResult == 2)
                return (Family) null;
            }
          }
        }
      }
      else
      {
        insulFamily = PlacementUtilities.FamilySelection(revitDoc);
        if (insulFamily == null)
          return (Family) null;
      }
      int num2 = (int) transaction.Commit();
    }
    return insulFamily;
  }

  public static int SetInsulDimensionsandPlace(
    Insulation insul,
    Element wall,
    XYZ originPoint,
    FamilySymbol insulSymbol,
    PlanarFace face)
  {
    Document document = wall.Document;
    using (Transaction transaction = new Transaction(document, "Place Insulation"))
    {
      int num1 = (int) transaction.Start();
      insulSymbol.Activate();
      FamilyInstance familyInstance = document.Create.NewFamilyInstance(insul.Face.Reference, originPoint, insul.DirectionVector, insulSymbol);
      familyInstance.LookupParameter("HOST_GUID")?.Set(wall.UniqueId);
      Parameter parameter1 = familyInstance.LookupParameter("BOM_PRODUCT_HOST");
      if (parameter1 != null)
      {
        string parameterAsString = Parameters.GetParameterAsString(wall, "CONTROL_MARK");
        parameter1.Set(parameterAsString);
      }
      Parameter parameter2 = familyInstance.LookupParameter("DIM_LENGTH");
      if (parameter2 != null)
      {
        if (parameter2.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter2.IsReadOnly)
          {
            int num2 = (int) transaction.RollBack();
            return -1;
          }
          parameter2.Set(insul.InsulLength);
        }
        else
        {
          int num3 = (int) transaction.RollBack();
          return -1;
        }
      }
      Parameter parameter3 = familyInstance.LookupParameter("DIM_WIDTH");
      if (parameter3 != null)
      {
        if (parameter3.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter3.IsReadOnly)
          {
            int num4 = (int) transaction.RollBack();
            return -2;
          }
          parameter3.Set(insul.InsulWidth);
        }
        else
        {
          int num5 = (int) transaction.RollBack();
          return -2;
        }
      }
      Parameter parameter4 = familyInstance.LookupParameter("DIM_THICKNESS");
      if (parameter4 != null)
      {
        if (parameter4.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter4.IsReadOnly)
          {
            int num6 = (int) transaction.RollBack();
            return -3;
          }
          parameter4.Set(insul.InsulThickness);
        }
        else
        {
          int num7 = (int) transaction.RollBack();
          return -3;
        }
      }
      familyInstance.LookupParameter("MANUFACTURE_COMPONENT")?.Set(insul.manufComponent);
      Parameter parameter5 = familyInstance.LookupParameter("Offset from Host");
      if (parameter5 != null)
      {
        double num8 = parameter5.AsDouble();
        parameter5.Set(num8 - insul.WytheInner);
      }
      insul.InsulSolids.AddRange((IEnumerable<Solid>) Solids.GetSymbolSolids((Element) familyInstance));
      document.Regenerate();
      ManualPlacementErrorType placeError;
      PlacementUtilities.CutInsulation(wall, familyInstance, insul, out placeError);
      if (placeError != ManualPlacementErrorType.Success)
        PlacementUtilities.CallManualPlacementErrorMessage(placeError);
      FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
      failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) new WarningSwallower());
      transaction.SetFailureHandlingOptions(failureHandlingOptions);
      int num9 = (int) transaction.Commit();
    }
    return 1;
  }

  public static void CutInsulation(
    Element wall,
    FamilyInstance insulation,
    Insulation insul,
    out ManualPlacementErrorType placeError)
  {
    placeError = ManualPlacementErrorType.Success;
    string uniqueId = wall.GetTopLevelElement().UniqueId;
    Document revitDoc = insul.RevitDoc;
    bool flag1 = false;
    BoundingBoxXYZ boundingBoxXyz = insulation.get_BoundingBox((View) null);
    Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
    ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    LogicalOrFilter filter2 = new LogicalOrFilter((ElementFilter) new BoundingBoxIsInsideFilter(outline), (ElementFilter) new BoundingBoxIntersectsFilter(outline));
    FilteredElementCollector elementCollector = new FilteredElementCollector(insul.RevitDoc).WherePasses((ElementFilter) filter1).WherePasses((ElementFilter) filter2);
    List<ElementId> list = insulation.GetAllSubcomponents().ToList<ElementId>();
    foreach (Element element1 in elementCollector.ToElements().Where<Element>((Func<Element, bool>) (e => !e.GetTopLevelElement().Id.Equals((object) insulation.Id))).ToList<Element>())
    {
      if (!list.Contains(element1.Id))
      {
        Element element2 = (Element) null;
        Parameter parameter1 = Parameters.LookupParameter(element1, "HOST_GUID");
        bool voidCut = false;
        bool flag2 = false;
        if (Insulation.Addons.Contains(element1.Id))
        {
          if (uniqueId.Equals(parameter1.AsString()))
          {
            flag2 = true;
            element2 = element1;
          }
          if (!flag2 && JoinGeometryUtils.AreElementsJoined(insul.RevitDoc, wall, element1))
          {
            flag2 = true;
            element2 = element1;
          }
        }
        if (!flag2)
        {
          Element element3 = insul.RevitDoc.GetElement(element1.Id);
          if (element3 is FamilyInstance && (element3 as FamilyInstance).SuperComponent != null)
            element3 = insul.RevitDoc.GetElement((element3 as FamilyInstance).SuperComponent.Id);
          Parameter parameter2 = Parameters.LookupParameter(element3, "MANUFACTURE_COMPONENT");
          if (parameter2 != null && parameter2.HasValue)
          {
            if (parameter2.AsString().Contains("WOOD") && parameter2.AsString().Contains("NAILER"))
            {
              if (uniqueId.Equals(parameter1.AsString()))
              {
                flag2 = true;
                element2 = element1;
              }
              if (!flag2 && JoinGeometryUtils.AreElementsJoined(insul.RevitDoc, wall, element1))
              {
                flag2 = true;
                element2 = element1;
              }
            }
            if (!flag2)
            {
              if (PlacementUtilities.checkVoidsForCutting(element3, wall, false, element3, out voidCut))
              {
                element2 = element3;
                flag2 = true;
              }
              if (!flag2 && (element3 as FamilyInstance).GetAllSubcomponents().Contains<ElementId>(element1.Id) && PlacementUtilities.checkVoidsForCutting(element1, wall, true, element3, out voidCut))
              {
                element2 = element3;
                flag2 = true;
                voidCut = true;
              }
            }
          }
        }
        if (flag2)
        {
          try
          {
            Transform insulationTransform = insulation.GetTransform();
            if ((voidCut ? (InstanceVoidCutUtils.CanBeCutWithVoid((Element) insulation) ? 1 : 0) : (SolidSolidCutUtils.CanElementCutElement(element2, (Element) insulation, out CutFailureReason _) ? 1 : 0)) != 0)
            {
              if (voidCut)
                InstanceVoidCutUtils.AddInstanceVoidCut(revitDoc, (Element) insulation, element2);
              else
                SolidSolidCutUtils.AddCutBetweenSolids(revitDoc, (Element) insulation, element2);
              revitDoc.Regenerate();
              bool bSymbol;
              List<Solid> source = Solids.GetSymbolSolids((Element) insulation, out bSymbol);
              if (bSymbol)
                source = source.Select<Solid, Solid>((Func<Solid, Solid>) (s => SolidUtils.CreateTransformed(s, insulationTransform))).ToList<Solid>();
              if (source.Count == 0)
              {
                placeError = ManualPlacementErrorType.PointsOutsideLoop;
                wall.Document.Delete(insulation.Id);
                flag1 = true;
              }
              else
              {
                foreach (Solid solid0 in source)
                {
                  if (solid0.Volume == 0.0)
                  {
                    wall.Document.Delete(insulation.Id);
                    flag1 = true;
                  }
                  else
                  {
                    if (!insul.IsMirrored)
                    {
                      XYZ basisY = XYZ.BasisY;
                    }
                    else
                    {
                      XYZ xyz1 = -XYZ.BasisY;
                    }
                    XYZ xyz2 = insul.IsMirrored ? -insul.WallTransform.BasisY : insul.WallTransform.BasisY;
                    Solid extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) insul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>(), xyz2, insul.InsulThickness);
                    try
                    {
                      if (!PlacementUtilities.CheckIntersectedSolids(BooleanOperationsUtils.ExecuteBooleanOperation(solid0, extrusionGeometry, BooleanOperationsType.Intersect), xyz2))
                      {
                        placeError = ManualPlacementErrorType.InsulationCutIntoMultiple;
                        wall.Document.Delete(insulation.Id);
                        flag1 = true;
                      }
                    }
                    catch
                    {
                    }
                  }
                }
              }
              if (flag1)
                break;
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
    }
  }

  public static bool checkVoidsForCutting(
    Element elem,
    Element wall,
    bool subComponent,
    Element topLevelForVoid,
    out bool voidCut)
  {
    bool flag = false;
    voidCut = false;
    Parameter parameter1 = Parameters.LookupParameter(elem, "MANUFACTURE_COMPONENT");
    string uniqueId = wall.GetTopLevelElement().UniqueId;
    Parameter parameter2 = Parameters.LookupParameter(elem, "HOST_GUID");
    if (!string.IsNullOrEmpty(parameter1.AsString()) && parameter1.AsString().Contains("VOID"))
    {
      voidCut = true;
      if (uniqueId.Equals(parameter2.AsString()))
        flag = true;
      if (!flag)
      {
        List<ElementId> list = InstanceVoidCutUtils.GetCuttingVoidInstances(wall).ToList<ElementId>();
        if (subComponent)
        {
          if (list.Contains(topLevelForVoid.Id))
            flag = true;
        }
        else if (list.Contains(elem.Id))
          flag = true;
      }
    }
    return flag;
  }

  public static List<Element> RetrieveVoids(Document revitDoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel
    });
    return new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("VOID"))).ToList<Element>();
  }

  public static int SetInsulDimensionsandPlaceCopyForAutomated(
    Insulation insul,
    Element wall,
    XYZ originPoint,
    FamilySymbol insulSymbol,
    PlanarFace face,
    out FamilyInstance insulationFam,
    out bool duplicateErr,
    out ManualPlacementErrorType errors)
  {
    errors = ManualPlacementErrorType.Success;
    duplicateErr = false;
    Document document = wall.Document;
    using (Transaction transaction = new Transaction(document, "Place Insulation"))
    {
      int num1 = (int) transaction.Start();
      insulSymbol.Activate();
      FamilyInstance familyInstance = document.Create.NewFamilyInstance(insul.Face.Reference, originPoint, insul.DirectionVector, insulSymbol);
      insulationFam = familyInstance;
      Parameter parameter1 = familyInstance.LookupParameter("BOM_PRODUCT_HOST");
      if (parameter1 != null)
      {
        string parameterAsString = Parameters.GetParameterAsString(wall, "CONTROL_MARK");
        parameter1.Set(parameterAsString);
      }
      familyInstance.LookupParameter("HOST_GUID")?.Set(wall.UniqueId);
      Parameter parameter2 = familyInstance.LookupParameter("DIM_LENGTH");
      if (parameter2 != null)
      {
        if (parameter2.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter2.IsReadOnly)
          {
            int num2 = (int) transaction.RollBack();
            return -1;
          }
          parameter2.Set(insul.InsulLength);
        }
        else
        {
          new TaskDialog("Warning")
          {
            MainContent = "The DIM_LENGTH parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
          }.Show();
          int num3 = (int) transaction.RollBack();
          return -4;
        }
      }
      Parameter parameter3 = familyInstance.LookupParameter("DIM_WIDTH");
      if (parameter3 != null)
      {
        if (parameter3.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter3.IsReadOnly)
          {
            int num4 = (int) transaction.RollBack();
            return -2;
          }
          parameter3.Set(insul.InsulWidth);
        }
        else
        {
          new TaskDialog("Warning")
          {
            MainContent = "The DIM_WIDTH parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
          }.Show();
          int num5 = (int) transaction.RollBack();
          return -4;
        }
      }
      Parameter parameter4 = familyInstance.LookupParameter("DIM_THICKNESS");
      if (parameter4 != null)
      {
        if (parameter4.Definition.GetDataType().Equals((object) SpecTypeId.Length))
        {
          if (parameter4.IsReadOnly)
          {
            int num6 = (int) transaction.RollBack();
            return -3;
          }
          parameter4.Set(insul.InsulThickness);
        }
        else
        {
          new TaskDialog("Warning")
          {
            MainContent = "The DIM_THICKNESS parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
          }.Show();
          int num7 = (int) transaction.RollBack();
          return -4;
        }
      }
      familyInstance.LookupParameter("MANUFACTURE_COMPONENT")?.Set(insul.manufComponent);
      Parameter parameter5 = familyInstance.LookupParameter("Offset from Host");
      if (parameter5 != null)
      {
        parameter5.AsDouble();
        parameter5.Set(-insul.WytheInner);
      }
      insul.InsulSolids.AddRange((IEnumerable<Solid>) Solids.GetSymbolSolids((Element) familyInstance));
      document.Regenerate();
      PlacementUtilities.CutInsulation(wall, familyInstance, insul, out errors);
      FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
      WarningSwallower preprocessor = new WarningSwallower();
      failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
      transaction.SetFailureHandlingOptions(failureHandlingOptions);
      int num8 = (int) transaction.Commit();
      if ((preprocessor.FailType & WarningSwallower.FailureType.DuplicateInstances) == WarningSwallower.FailureType.DuplicateInstances)
        duplicateErr = true;
    }
    return 1;
  }

  public static bool IsMirrored(Element wall)
  {
    return (wall as FamilyInstance).FacingFlipped || (wall as FamilyInstance).Mirrored;
  }

  public static XYZ GetLengthWidthAndOrigin(
    XYZ locationPoint,
    Tuple<XYZ, XYZ> transformedClickPoints,
    Insulation insul,
    Transform wallTransform,
    bool bMetric,
    out double len,
    out double wid,
    out MaxMinStatus mms,
    out ManualPlacementErrorType mpe,
    out PointOrientation pointOrientation)
  {
    XYZ lengthWidthAndOrigin = locationPoint;
    XYZ xyz1 = wallTransform.Inverse.OfPoint(transformedClickPoints.Item1);
    XYZ xyz2 = wallTransform.Inverse.OfPoint(transformedClickPoints.Item2);
    double num1 = xyz2.X - xyz1.X;
    double num2 = xyz2.Z - xyz1.Z;
    mms = MaxMinStatus.Normal;
    mpe = ManualPlacementErrorType.NoMessage;
    PointOrientation pointOrientation1 = PointOrientation.LowerLeft;
    if (num1 < 0.0 && num2 < 0.0)
      pointOrientation1 = PointOrientation.LowerRight;
    else if (num1 < 0.0 && num2 >= 0.0)
      pointOrientation1 = PointOrientation.UpperRight;
    else if (num1 >= 0.0 && num2 < 0.0)
      pointOrientation1 = PointOrientation.LowerLeft;
    else if (num1 >= 0.0 && num2 >= 0.0)
      pointOrientation1 = PointOrientation.UpperLeft;
    pointOrientation = pointOrientation1;
    double val1 = Math.Abs(num1);
    double val2 = Math.Abs(num2);
    double num3 = PlacementUtilities.RoundValue(val1, bMetric);
    double num4 = PlacementUtilities.RoundValue(val2, bMetric);
    double result1;
    double.TryParse($"{num3:0.######0}", out result1);
    double result2;
    double.TryParse($"{num4:0.######0}", out result2);
    double currDim1 = double.IsNaN(result1) || result1 == 0.0 ? num3 : result1;
    double currDim2 = double.IsNaN(result2) || result2 == 0.0 ? num4 : result2;
    double num5;
    double num6;
    if (currDim1 > currDim2)
    {
      if (currDim1 < insul.MinLength)
      {
        mms = PlacementUtilities.PromptDimensionError(ExpandError.MinLength, currDim1, insul.MinLength);
        num5 = insul.MinLength;
        if (mms == MaxMinStatus.Expand)
          insul.ExpandedToMin = true;
      }
      else if (currDim1 > insul.MaxLength)
      {
        if (currDim1 > insul.MaxLength && currDim1 - insul.MaxLength < insul.MinLength)
        {
          num5 = currDim1 - insul.MinLength;
        }
        else
        {
          mms = PlacementUtilities.PromptDimensionError(ExpandError.MaxLength, currDim1, insul.MaxLength);
          num5 = insul.MaxLength;
          if (mms == MaxMinStatus.Expand)
            insul.ExpandedToMax = true;
        }
      }
      else
        num5 = currDim1;
      if (mms == MaxMinStatus.Reselect)
      {
        len = 0.0;
        wid = 0.0;
        return (XYZ) null;
      }
      if (currDim2 < insul.MinWidth)
      {
        mms = PlacementUtilities.PromptDimensionError(ExpandError.MinWidth, currDim2, insul.MinWidth);
        num6 = insul.MinWidth;
        if (mms == MaxMinStatus.Expand)
          insul.ExpandedToMin = true;
      }
      else if (currDim2 > insul.MaxWidth)
      {
        if (currDim2 > insul.MaxWidth && currDim2 - insul.MaxWidth < insul.MinWidth)
        {
          num6 = currDim2 - insul.MinWidth;
        }
        else
        {
          mms = PlacementUtilities.PromptDimensionError(ExpandError.MaxWidth, currDim2, insul.MaxWidth);
          num6 = insul.MaxWidth;
          if (mms == MaxMinStatus.Expand)
            insul.ExpandedToMax = true;
        }
      }
      else
        num6 = currDim2;
      if (mms == MaxMinStatus.Reselect)
      {
        len = 0.0;
        wid = 0.0;
        return (XYZ) null;
      }
      double num7 = num5;
      double num8 = num6;
      insul.DirectionVector = -wallTransform.BasisZ;
      switch (pointOrientation1)
      {
        case PointOrientation.LowerLeft:
          lengthWidthAndOrigin += wallTransform.BasisX * num7;
          break;
        case PointOrientation.UpperLeft:
          lengthWidthAndOrigin = lengthWidthAndOrigin + wallTransform.BasisZ * num8 + wallTransform.BasisX * num7;
          break;
        case PointOrientation.UpperRight:
          lengthWidthAndOrigin += wallTransform.BasisZ * num8;
          break;
      }
    }
    else
    {
      if (currDim2 < insul.MinLength)
      {
        mms = PlacementUtilities.PromptDimensionError(ExpandError.MinLength, currDim2, insul.MinLength);
        num5 = insul.MinLength;
        if (mms == MaxMinStatus.Expand)
          insul.ExpandedToMin = true;
      }
      else if (currDim2 > insul.MaxLength)
      {
        if (currDim2 > insul.MaxLength && currDim2 - insul.MaxLength < insul.MinLength)
        {
          num5 = currDim2 - insul.MinLength;
        }
        else
        {
          mms = PlacementUtilities.PromptDimensionError(ExpandError.MaxLength, currDim2, insul.MaxLength);
          num5 = insul.MaxLength;
          if (mms == MaxMinStatus.Expand)
            insul.ExpandedToMax = true;
        }
      }
      else
        num5 = currDim2;
      if (mms == MaxMinStatus.Reselect)
      {
        len = 0.0;
        wid = 0.0;
        return (XYZ) null;
      }
      if (currDim1 < insul.MinWidth)
      {
        mms = PlacementUtilities.PromptDimensionError(ExpandError.MinWidth, currDim1, insul.MinWidth);
        num6 = insul.MinWidth;
        if (mms == MaxMinStatus.Expand)
          insul.ExpandedToMin = true;
      }
      else if (currDim1 > insul.MaxWidth)
      {
        if (currDim1 > insul.MaxWidth && currDim1 - insul.MaxWidth < insul.MinWidth)
        {
          num6 = currDim1 - insul.MinWidth;
        }
        else
        {
          mms = PlacementUtilities.PromptDimensionError(ExpandError.MaxWidth, currDim1, insul.MaxWidth);
          num6 = insul.MaxWidth;
          if (mms == MaxMinStatus.Expand)
            insul.ExpandedToMax = true;
        }
      }
      else
        num6 = currDim1;
      if (mms == MaxMinStatus.Reselect)
      {
        len = 0.0;
        wid = 0.0;
        return (XYZ) null;
      }
      double num9 = num5;
      double num10 = num6;
      insul.DirectionVector = -wallTransform.BasisX;
      switch (pointOrientation1)
      {
        case PointOrientation.LowerLeft:
          lengthWidthAndOrigin = lengthWidthAndOrigin - wallTransform.BasisZ * num9 + wallTransform.BasisX * num10;
          break;
        case PointOrientation.UpperLeft:
          lengthWidthAndOrigin += wallTransform.BasisX * num10;
          break;
        case PointOrientation.LowerRight:
          lengthWidthAndOrigin -= wallTransform.BasisZ * num9;
          break;
      }
    }
    len = num5;
    wid = num6;
    return lengthWidthAndOrigin;
  }

  public static MaxMinStatus PromptDimensionError(ExpandError ee, double currDim, double extentDim)
  {
    return MaxMinStatus.Expand;
  }

  public static void GetParamsForInsul(Insulation insul, FamilySymbol insulFamily, Element wall)
  {
    bool bMetric = Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(wall.Document);
    Parameter parameter1 = insulFamily.LookupParameter("DIM_THICKNESS");
    if (parameter1 != null)
    {
      insul.InsulThickness = PlacementUtilities.RoundValue(parameter1.AsDouble(), bMetric);
    }
    else
    {
      Parameter parameter2 = Parameters.LookupParameter(wall, "DIM_WYTHE_INSULATION");
      if (parameter2 != null)
        insul.InsulThickness = PlacementUtilities.RoundValue(parameter2.AsDouble(), bMetric);
    }
    Parameter parameter3 = insulFamily.LookupParameter("DIM_LENGTH_MAX");
    if (parameter3 != null)
      insul.MaxLength = parameter3.AsDouble();
    Parameter parameter4 = insulFamily.LookupParameter("DIM_LENGTH_MIN");
    if (parameter4 != null)
      insul.MinLength = parameter4.AsDouble();
    Parameter parameter5 = insulFamily.LookupParameter("DIM_WIDTH_MAX");
    if (parameter5 != null)
      insul.MaxWidth = parameter5.AsDouble();
    Parameter parameter6 = insulFamily.LookupParameter("DIM_WIDTH_MIN");
    if (parameter6 != null)
      insul.MinWidth = parameter6.AsDouble();
    Parameter parameter7 = Parameters.LookupParameter(wall, "DIM_WYTHE_OUTER");
    if (parameter7 != null)
      insul.WytheOuter = parameter7.AsDouble();
    Parameter parameter8 = Parameters.LookupParameter(wall, "DIM_WYTHE_INNER");
    if (parameter8 == null)
      return;
    insul.WytheInner = parameter8.AsDouble();
  }

  public static PlanarFace GetOuterCurveLoop(Element wall, Insulation insul)
  {
    return PlacementUtilities.GetOuterCurveLoop(wall, insul, out List<CurveLoop> _, true);
  }

  public static PlanarFace GetOuterCurveLoop(
    Element wall,
    Insulation insul,
    out List<CurveLoop> faceCurves,
    bool bManual)
  {
    try
    {
      bool bSymbol;
      List<Solid> symbolSolids = Solids.GetSymbolSolids(wall, out bSymbol, options: new Options()
      {
        ComputeReferences = true
      });
      insul.BSymbol = bSymbol;
      insul.WallSolids.AddRange((IEnumerable<Solid>) symbolSolids);
      PlacementUtilities.GetInnerLoops(wall, insul.WallTransform, insul, insul.RevitDoc);
      PlacementUtilities.UnionSolids(insul);
      PlanarFace validLoopFace = PlacementUtilities.GetValidLoopFace(wall, insul);
      PlanarFace innerFace = PlacementUtilities.GetInnerFace(wall, insul);
      List<CurveLoop> list1 = validLoopFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
      List<CurveLoop> list2 = innerFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
      Transform transform = (wall as FamilyInstance).GetTransform();
      Solid extrusionGeometry1;
      Solid extrusionGeometry2;
      if (PlacementUtilities.IsMirrored(wall))
      {
        extrusionGeometry1 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) list2, -transform.BasisY, insul.InsulThickness);
        extrusionGeometry2 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) list1, transform.BasisY, insul.InsulThickness);
      }
      else
      {
        extrusionGeometry1 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) list2, transform.BasisY, insul.InsulThickness);
        extrusionGeometry2 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) list1, -transform.BasisY, insul.InsulThickness);
      }
      Solid combinedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(extrusionGeometry2, extrusionGeometry1, BooleanOperationsType.Union);
      PlacementUtilities.applyingTolerance(wall, insul, combinedSolid, insul.IsMirrored);
      faceCurves = insul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
      return validLoopFace;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("failed when creating the extruded solid") && bManual)
        new TaskDialog("Therommass")
        {
          MainContent = "The geometry of the selected wall could not be processed for insulation placement tool."
        }.Show();
      faceCurves = new List<CurveLoop>();
      return (PlanarFace) null;
    }
  }

  public static PlanarFace applyingTolerance(
    Element wall,
    Insulation tempInsul,
    Solid combinedSolid,
    bool isMirrored)
  {
    bool flag = false;
    PlanarFace planarFace1 = (PlanarFace) null;
    tempInsul.BSymbol = flag;
    double wytheInner = tempInsul.WytheInner;
    XYZ source = isMirrored ? XYZ.BasisY : -XYZ.BasisY;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    TransformedFace transformedFace1 = (TransformedFace) null;
    foreach (Face face in combinedSolid.Faces)
    {
      if (face is PlanarFace)
      {
        PlanarFace f = face as PlanarFace;
        TransformedFace transformedFace2 = new TransformedFace((wall as FamilyInstance).GetTransform(), f);
        if (transformedFace2.Normal.IsAlmostEqualTo(source))
        {
          if (transformedFace1 == null)
          {
            transformedFace1 = transformedFace2;
            planarFace1 = f;
          }
          double y1 = transformedFace2.Origin.Y;
          double y2 = transformedFace1.Origin.Y;
          if (isMirrored)
          {
            if (y1 >= y2)
            {
              transformedFace1 = transformedFace2;
              PlanarFace planarFace2 = f;
              planarFaceList.Add(planarFace2);
            }
          }
          else if (y1 <= y2)
          {
            transformedFace1 = transformedFace2;
            PlanarFace planarFace3 = f;
            planarFaceList.Add(planarFace3);
          }
        }
      }
    }
    PlanarFace planarFace4 = (PlanarFace) null;
    PlanarFace planarFace5 = (PlanarFace) null;
    foreach (PlanarFace planarFace6 in planarFaceList)
    {
      if ((GeometryObject) planarFace4 == (GeometryObject) null)
      {
        planarFace4 = planarFace6;
      }
      else
      {
        Transform transform = (wall as FamilyInstance).GetTransform();
        if (Math.Abs(transform.Inverse.OfPoint(planarFace6.Origin).Y - transform.Inverse.OfPoint(planarFace4.Origin).Y) == Math.Abs(wytheInner))
        {
          planarFace5 = planarFace4;
          break;
        }
        planarFace4 = planarFace6;
      }
    }
    if ((GeometryObject) planarFace5 == (GeometryObject) null)
      planarFace5 = planarFace4;
    tempInsul.UnionedFace = planarFace5;
    Parameter parameter = Parameters.LookupParameter(wall, "TOLERANCE");
    if (parameter != null && parameter.HasValue)
    {
      tempInsul.toleranceLine = parameter.AsDouble();
      try
      {
        List<CurveLoop> list = tempInsul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
        List<CurveLoop> curveLoopList = new List<CurveLoop>();
        List<CurveLoop> profileLoops = new List<CurveLoop>();
        CurveLoop outerLoop = PlacementUtilities.GetOuterLoop(list);
        foreach (CurveLoop curveLoop2 in list)
        {
          if (curveLoop2.Equals((object) outerLoop))
            curveLoopList.Add(curveLoop2);
          else if (!CAM_Utils.CurveLoopContainsCurveLoop(outerLoop, curveLoop2))
            curveLoopList.Add(curveLoop2);
          else
            profileLoops.Add(curveLoop2);
        }
        if (tempInsul.toleranceLine != 0.0)
        {
          foreach (CurveLoop original in curveLoopList)
          {
            CurveLoop viaOffset = CurveLoop.CreateViaOffset(original, -tempInsul.toleranceLine, tempInsul.UnionedFace.FaceNormal);
            profileLoops.Add(viaOffset);
          }
          foreach (Face face in GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) profileLoops, tempInsul.UnionedFace.FaceNormal.Negate(), tempInsul.InsulThickness).Faces)
          {
            PlanarFace planarFace7 = face as PlanarFace;
            if (!((GeometryObject) planarFace7 == (GeometryObject) null) && planarFace7.FaceNormal.IsAlmostEqualTo(tempInsul.UnionedFace.FaceNormal))
            {
              tempInsul.UnionedFace = planarFace7;
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        new TaskDialog("Warning").MainInstruction = $"Please check the TOLERANCE parameter on {(wall as FamilyInstance).Symbol.FamilyName} - {wall.Name}. This is causing exception with the way Revit geometry is built.";
      }
    }
    return tempInsul.UnionedFace;
  }

  public static void GetInnerLoops(
    Element wall,
    Transform wallTransform,
    Insulation insul,
    Document revitDoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_StructConnections
    });
    foreach (Element element in Insulation.Addons.Count > 0 ? new FilteredElementCollector(revitDoc, Insulation.Addons).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsString(e, "HOST_GUID").Equals(wall.UniqueId))).ToList<Element>() : new List<Element>())
    {
      if (element is FamilyInstance)
      {
        FamilyInstance elem = element as FamilyInstance;
        bool bSymbol;
        foreach (Solid symbolSolid in Solids.GetSymbolSolids((Element) elem, out bSymbol, useViewDetail: true, options: new Options()))
        {
          if (symbolSolid.Volume >= 0.0)
          {
            insul.WallSolids.Add(symbolSolid);
            if (bSymbol)
            {
              Solid transformed = SolidUtils.CreateTransformed(symbolSolid, elem.GetTransform());
              insul.InvalidInsulSolids.Add(transformed);
            }
            else
              insul.InvalidInsulSolids.Add(SolidUtils.Clone(symbolSolid));
          }
        }
      }
    }
    PlacementUtilities.DrawInteriorSolidLines(insul, revitDoc);
  }

  public static void UnionSolids(Insulation insul)
  {
    Solid solid0 = (Solid) null;
    foreach (Solid wallSolid in insul.WallSolids)
    {
      Solid solid1 = wallSolid;
      if (insul.BSymbol)
        solid1 = SolidUtils.CreateTransformed(wallSolid, insul.WallTransform);
      solid0 = !((GeometryObject) solid0 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid0, solid1, BooleanOperationsType.Union) : solid1;
    }
    insul.UnionedSolid = solid0;
  }

  public static void DrawUnionedFaceLines(Insulation insul, Document revitDoc)
  {
    using (Transaction transaction = new Transaction(revitDoc, "Draw Model Lines - Unioned Face"))
    {
      int num1 = (int) transaction.Start();
      foreach (EdgeArray edgeLoop in insul.UnionedFace.EdgeLoops)
      {
        foreach (Edge edge in edgeLoop)
        {
          int num2 = edge.AsCurve().IsBound ? 1 : 0;
        }
      }
      int num3 = (int) transaction.Commit();
    }
  }

  public static void DrawInteriorSolidLines(Insulation insul, Document revitDoc)
  {
  }

  public static bool ValidateFinalDimensions(
    XYZ originPoint,
    double len,
    double wid,
    Insulation insul,
    PointOrientation pto,
    out ManualPlacementErrorType m)
  {
    m = ManualPlacementErrorType.NoMessage;
    XYZ xyz1 = insul.WallTransform.Inverse.OfPoint(originPoint);
    double num1;
    double num2;
    if (len > wid)
    {
      num1 = len;
      num2 = wid;
    }
    else
    {
      num2 = len;
      num1 = wid;
    }
    if (insul.IsMirrored)
      num2 *= -1.0;
    switch (pto)
    {
      case PointOrientation.LowerLeft:
        num2 = -num2;
        num1 = -num1;
        break;
      case PointOrientation.UpperLeft:
        num2 = -num2;
        break;
      case PointOrientation.LowerRight:
        num1 = -num1;
        break;
    }
    XYZ point1 = new XYZ(xyz1.X - num2, xyz1.Y, xyz1.Z + num1);
    double insulThickness = insul.InsulThickness;
    double y1 = insul.WallTransform.Inverse.OfPoint(insul.UnionedFace.Origin).Y;
    XYZ xyz2 = new XYZ(xyz1.X, y1, xyz1.Z);
    XYZ xyz3 = new XYZ(point1.X, y1, point1.Z);
    XYZ xyz4 = new XYZ(xyz1.X, y1, point1.Z);
    XYZ xyz5 = new XYZ(point1.X, y1, xyz1.Z);
    Curve bound1 = (Curve) Line.CreateBound(xyz2, xyz4);
    Curve bound2 = (Curve) Line.CreateBound(xyz4, xyz3);
    Curve bound3 = (Curve) Line.CreateBound(xyz3, xyz5);
    Curve bound4 = (Curve) Line.CreateBound(xyz5, xyz2);
    XYZ point2 = insul.WallTransform.OfPoint(point1);
    CurveLoop curveLoop = new CurveLoop();
    curveLoop.Append(bound1);
    curveLoop.Append(bound2);
    curveLoop.Append(bound3);
    curveLoop.Append(bound4);
    XYZ xyz6 = insul.IsMirrored ? -XYZ.BasisY : XYZ.BasisY;
    XYZ xyz7 = insul.IsMirrored ? -insul.WallTransform.BasisY : insul.WallTransform.BasisY;
    Solid intersectedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(SolidUtils.CreateTransformed(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
    {
      curveLoop
    }, xyz6, insulThickness), insul.WallTransform), GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) insul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>(), xyz7, insulThickness), BooleanOperationsType.Intersect);
    if (!PlacementUtilities.CheckIntersectedSolids(intersectedSolid, xyz7))
      m = ManualPlacementErrorType.InsulationCutIntoMultiple;
    PlanarFace planarFace1 = (PlanarFace) null;
    foreach (Face face in intersectedSolid.Faces)
    {
      if (face is PlanarFace)
      {
        PlanarFace planarFace2 = face as PlanarFace;
        if (face.ComputeNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(xyz6))
        {
          if ((GeometryObject) planarFace1 == (GeometryObject) null)
            planarFace1 = planarFace2;
          double y2 = planarFace2.Origin.Y;
          double y3 = planarFace1.Origin.Y;
          if (insul.IsMirrored)
          {
            if (y2 < y3)
              planarFace1 = planarFace2;
          }
          else if (y2 > y3)
            planarFace1 = planarFace2;
        }
      }
    }
    BoundingBoxUV boundingBox = planarFace1.GetBoundingBox();
    XYZ xyz8 = planarFace1.Evaluate(boundingBox.Min);
    XYZ xyz9 = planarFace1.Evaluate(boundingBox.Max);
    XYZ xyz10 = planarFace1.Evaluate(new UV(boundingBox.Min.U, boundingBox.Max.V));
    XYZ xyz11 = planarFace1.Evaluate(new UV(boundingBox.Max.U, boundingBox.Min.V));
    List<XYZ> source = new List<XYZ>();
    source.Add(xyz8);
    source.Add(xyz9);
    source.Add(xyz10);
    source.Add(xyz11);
    double y4 = xyz8.Y;
    double num3 = source.Min<XYZ>((Func<XYZ, double>) (p => p.X));
    double num4 = source.Max<XYZ>((Func<XYZ, double>) (p => p.X));
    double num5 = source.Min<XYZ>((Func<XYZ, double>) (p => p.Z));
    double num6 = source.Max<XYZ>((Func<XYZ, double>) (p => p.Z));
    if (len > wid)
    {
      if (Math.Abs(num6 - num5) < insul.MinLength)
        m = ManualPlacementErrorType.LessThanLenMin;
      if (Math.Abs(num4 - num3) < insul.MinWidth)
        m = ManualPlacementErrorType.LessThanLenMin;
    }
    else
    {
      if (Math.Abs(num4 - num3) < insul.MinLength)
        m = ManualPlacementErrorType.LessThanLenMin;
      if (Math.Abs(num6 - num5) < insul.MinWidth)
        m = ManualPlacementErrorType.LessThanLenMin;
    }
    if (m == ManualPlacementErrorType.LessThanLenMin)
      return false;
    bool flag1 = PlacementUtilities.CheckPointWithinLoop(originPoint, insul.UnionedFace);
    if (flag1)
      flag1 = PlacementUtilities.CheckPointAgainstInnerLoop(originPoint, insul);
    if (!flag1)
      return false;
    bool flag2 = PlacementUtilities.CheckPointWithinLoop(point2, insul.UnionedFace);
    if (flag2)
      flag2 = PlacementUtilities.CheckPointAgainstInnerLoop(point2, insul);
    return flag2;
  }

  public static bool CheckSplitInsulation(
    XYZ originPoint,
    double len,
    double wid,
    Insulation insul,
    PointOrientation pto,
    out ManualPlacementErrorType m)
  {
    m = ManualPlacementErrorType.NoMessage;
    XYZ xyz1 = insul.WallTransform.Inverse.OfPoint(originPoint);
    double num1;
    double num2;
    if (len > wid)
    {
      num1 = len;
      num2 = wid;
    }
    else
    {
      num2 = len;
      num1 = wid;
    }
    if (insul.IsMirrored)
      num2 *= -1.0;
    switch (pto)
    {
      case PointOrientation.LowerLeft:
        num2 = -num2;
        num1 = -num1;
        break;
      case PointOrientation.UpperLeft:
        num2 = -num2;
        break;
      case PointOrientation.LowerRight:
        num1 = -num1;
        break;
    }
    XYZ point1 = new XYZ(xyz1.X - num2, xyz1.Y, xyz1.Z + num1);
    double insulThickness = insul.InsulThickness;
    double y = insul.WallTransform.Inverse.OfPoint(insul.UnionedFace.Origin).Y;
    XYZ xyz2 = new XYZ(xyz1.X, y, xyz1.Z);
    XYZ xyz3 = new XYZ(point1.X, y, point1.Z);
    XYZ xyz4 = new XYZ(xyz1.X, y, point1.Z);
    XYZ xyz5 = new XYZ(point1.X, y, xyz1.Z);
    Curve bound1 = (Curve) Line.CreateBound(xyz2, xyz4);
    Curve bound2 = (Curve) Line.CreateBound(xyz4, xyz3);
    Curve bound3 = (Curve) Line.CreateBound(xyz3, xyz5);
    Curve bound4 = (Curve) Line.CreateBound(xyz5, xyz2);
    CurveLoop curveLoop = new CurveLoop();
    curveLoop.Append(bound1);
    curveLoop.Append(bound2);
    curveLoop.Append(bound3);
    curveLoop.Append(bound4);
    XYZ point2 = insul.WallTransform.OfPoint(point1);
    XYZ extrusionDir = insul.IsMirrored ? -XYZ.BasisY : XYZ.BasisY;
    XYZ xyz6 = insul.IsMirrored ? -insul.WallTransform.BasisY : insul.WallTransform.BasisY;
    Solid transformed = SolidUtils.CreateTransformed(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
    {
      curveLoop
    }, extrusionDir, insulThickness), insul.WallTransform);
    Solid extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) insul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>(), xyz6, insulThickness);
    try
    {
      if (!PlacementUtilities.CheckIntersectedSolids(BooleanOperationsUtils.ExecuteBooleanOperation(transformed, extrusionGeometry, BooleanOperationsType.Intersect), xyz6))
        m = ManualPlacementErrorType.InsulationCutIntoMultiple;
    }
    catch
    {
      return false;
    }
    bool flag1 = PlacementUtilities.CheckPointWithinLoop(originPoint, insul.UnionedFace);
    if (flag1)
      flag1 = PlacementUtilities.CheckPointAgainstInnerLoop(originPoint, insul);
    if (!flag1)
      return false;
    bool flag2 = PlacementUtilities.CheckPointWithinLoop(point2, insul.UnionedFace);
    if (flag2)
      flag2 = PlacementUtilities.CheckPointAgainstInnerLoop(point2, insul);
    return flag2;
  }

  public static Tuple<XYZ, XYZ> VerifyClickPoints(
    Tuple<XYZ, XYZ> clickPoints,
    PlanarFace loopFace,
    Insulation insul,
    out ManualPlacementErrorType mpe)
  {
    Tuple<XYZ, XYZ> tuple = (Tuple<XYZ, XYZ>) null;
    clickPoints = PlacementUtilities.FixToleranceLineIssues(clickPoints, insul.UnionedFace, insul.toleranceLine, insul);
    bool flag1 = false;
    bool flag2 = PlacementUtilities.CheckPointWithinLoop(clickPoints.Item1, loopFace, insul);
    if (flag2)
    {
      flag2 = PlacementUtilities.CheckPointAgainstInnerLoop(clickPoints.Item1, insul);
      if (flag2)
      {
        ManualPlacementErrorType mpe1 = ManualPlacementErrorType.NoMessage;
        Tuple<XYZ, XYZ> newPoints = PlacementUtilities.FindNewPoints(clickPoints.Item1, clickPoints.Item2, insul, out mpe1);
        mpe = mpe1;
        return newPoints;
      }
      flag1 = true;
    }
    bool flag3 = PlacementUtilities.CheckPointWithinLoop(clickPoints.Item2, loopFace, insul);
    if (flag3)
    {
      flag3 = PlacementUtilities.CheckPointAgainstInnerLoop(clickPoints.Item2, insul);
      if (flag1 & flag3)
        tuple = new Tuple<XYZ, XYZ>(clickPoints.Item1, clickPoints.Item2);
      else if (!flag1 & flag3)
        tuple = new Tuple<XYZ, XYZ>(clickPoints.Item2, clickPoints.Item1);
    }
    else if (!flag3 & flag1)
      tuple = new Tuple<XYZ, XYZ>(clickPoints.Item1, clickPoints.Item2);
    ManualPlacementErrorType mpe2 = ManualPlacementErrorType.NoMessage;
    if (!flag2 && !flag3)
      mpe2 = ManualPlacementErrorType.PointsOutsideLoop;
    if (flag2 | flag3)
      tuple = PlacementUtilities.FindNewPoints(tuple.Item1, tuple.Item2, insul, out mpe2);
    mpe = mpe2;
    return tuple;
  }

  private static Tuple<XYZ, XYZ> FixToleranceLineIssues(
    Tuple<XYZ, XYZ> clickPoints,
    PlanarFace loopFace,
    double toleranceLine,
    Insulation insul)
  {
    if (toleranceLine == 0.0)
      return clickPoints;
    double num = 1.0 / 192.0;
    XYZ xyz1 = clickPoints.Item1;
    bool flag1 = false;
    XYZ xyz2 = clickPoints.Item2;
    bool flag2 = false;
    Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(loopFace.FaceNormal, clickPoints.Item1);
    using (IEnumerator<CurveLoop> enumerator = loopFace.GetEdgesAsCurveLoops().GetEnumerator())
    {
label_27:
      while (enumerator.MoveNext())
      {
        CurveLoop current = enumerator.Current;
        CurveLoop viaOffset = CurveLoop.CreateViaOffset(current, toleranceLine, loopFace.FaceNormal);
        Plane plane = viaOffset.GetPlane();
        UV uv;
        double distance1;
        plane.Project(xyz1, out uv, out distance1);
        xyz1 = plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
        double distance2;
        plane.Project(xyz2, out uv, out distance2);
        xyz2 = plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
        int index = 0;
        while (true)
        {
          if (index < current.Count<Curve>() && !(flag1 & flag2))
          {
            Curve curve = viaOffset.ElementAt<Curve>(index);
            Curve cv = current.ElementAt<Curve>(index);
            if (curve.IsBound)
            {
              XYZ xyz3 = curve.Evaluate(0.0, true);
              XYZ xyz4 = curve.Evaluate(1.0, true);
              XYZ xyz5 = curve.Evaluate(0.5, true);
              if ((xyz3.DistanceTo(xyz1) <= num || xyz4.DistanceTo(xyz1) <= num || xyz5.DistanceTo(xyz1) <= num) && !flag1)
              {
                if (xyz3.DistanceTo(xyz1) <= num)
                  xyz1 = cv.Evaluate(0.0, true);
                else if (xyz4.DistanceTo(xyz1) <= num)
                {
                  xyz1 = cv.Evaluate(1.0, true);
                }
                else
                {
                  double parameter = cv.Project(xyz1).Parameter;
                  xyz1 = PlacementUtilities.PointFromParam(cv, parameter);
                }
                flag1 = true;
              }
              if ((xyz3.DistanceTo(xyz2) <= num || xyz4.DistanceTo(xyz2) <= num || xyz5.DistanceTo(xyz2) <= num) && !flag2)
              {
                if (xyz3.DistanceTo(xyz2) <= num)
                  xyz2 = cv.Evaluate(0.0, true);
                else if (xyz4.DistanceTo(xyz2) <= num)
                {
                  xyz2 = cv.Evaluate(1.0, true);
                }
                else
                {
                  double parameter = cv.Project(xyz2).Parameter;
                  xyz2 = PlacementUtilities.PointFromParam(cv, parameter) + distance2 * plane.Normal;
                }
                flag2 = true;
              }
              if (curve.IsBound)
              {
                if (curve.Distance(xyz1) < num && !flag1)
                {
                  double parameter = cv.Project(xyz1).Parameter;
                  xyz1 = PlacementUtilities.PointFromParam(cv, parameter) + distance1 * plane.Normal;
                }
                if (curve.Distance(xyz2) < num && !flag2)
                {
                  double parameter = cv.Project(xyz2).Parameter;
                  xyz2 = PlacementUtilities.PointFromParam(cv, parameter) + distance2 * plane.Normal;
                }
              }
            }
            ++index;
          }
          else
            goto label_27;
        }
      }
    }
    UV uv1;
    double distance;
    byNormalAndOrigin.Project(xyz1, out uv1, out distance);
    XYZ xyz6 = byNormalAndOrigin.Origin + uv1.U * byNormalAndOrigin.XVec + uv1.V * byNormalAndOrigin.YVec;
    byNormalAndOrigin.Project(xyz2, out uv1, out distance);
    XYZ xyz7 = byNormalAndOrigin.Origin + uv1.U * byNormalAndOrigin.XVec + uv1.V * byNormalAndOrigin.YVec;
    return new Tuple<XYZ, XYZ>(xyz6, xyz7);
  }

  private static XYZ PointFromParam(Curve cv, double param)
  {
    double normalizedParameter = cv.ComputeNormalizedParameter(param);
    if (normalizedParameter < 0.0)
      return cv.Evaluate(0.0, true);
    return normalizedParameter > 1.0 ? cv.Evaluate(1.0, true) : cv.Evaluate(normalizedParameter, true);
  }

  public static Tuple<XYZ, XYZ> AdjustPoint(
    XYZ originPoint,
    XYZ clickPoint,
    Insulation insul,
    PlanarFace loopFace,
    bool ptInsideLoop)
  {
    double maxLength1 = insul.MaxLength;
    double maxWidth1 = insul.MaxWidth;
    double maxLength2 = insul.MaxLength;
    double maxWidth2 = insul.MaxWidth;
    double y1 = insul.UnionedFace.Origin.Y;
    double y2 = clickPoint.Y;
    double x1 = originPoint.X;
    double z1 = originPoint.Z;
    double x2 = clickPoint.X;
    double z2 = clickPoint.Z;
    double dirVector1 = x2 - x1;
    double dirVector2 = z2 - z1;
    XYZ xyz1 = dirVector1 < 0.0 ? -XYZ.BasisX : XYZ.BasisX;
    XYZ xyz2 = dirVector2 < 0.0 ? -XYZ.BasisZ : XYZ.BasisZ;
    double val2_1 = Math.Abs(dirVector1);
    double val2_2 = Math.Abs(dirVector2);
    XYZ point1 = new XYZ(x1, y2, z2);
    XYZ point2 = new XYZ(x2, y2, z1);
    double x3 = point1.X;
    double z3 = point1.Z;
    double x4 = point2.X;
    double z4 = point2.Z;
    double dirVector3 = x4 - x3;
    double num1 = z3;
    double dirVector4 = z4 - num1;
    double dirVector5 = x3 - x4;
    double dirVector6 = x3 - x4;
    Curve directionalCurve1 = PlacementUtilities.CreateDirectionalCurve(originPoint, dirVector1, y1, true);
    Curve directionalCurve2 = PlacementUtilities.CreateDirectionalCurve(originPoint, dirVector2, y1);
    Curve directionalCurve3 = PlacementUtilities.CreateDirectionalCurve(point1, dirVector3, y1, true);
    Curve directionalCurve4 = PlacementUtilities.CreateDirectionalCurve(point1, dirVector4, y1);
    Curve directionalCurve5 = PlacementUtilities.CreateDirectionalCurve(point2, dirVector5, y1, true);
    Curve directionalCurve6 = PlacementUtilities.CreateDirectionalCurve(point2, dirVector6, y1);
    bool flag1 = PlacementUtilities.CheckPointWithinLoop(point1, loopFace);
    if (flag1)
      flag1 = PlacementUtilities.CheckPointAgainstInnerLoop(point1, insul);
    bool flag2 = PlacementUtilities.CheckPointWithinLoop(point2, loopFace);
    if (flag2)
      flag2 = PlacementUtilities.CheckPointAgainstInnerLoop(point2, insul);
    double d1_1;
    double d2_1;
    PlacementUtilities.FindIntersectionPoints(originPoint, directionalCurve1, directionalCurve2, insul, out d1_1, out d2_1);
    double val1_1 = d1_1;
    double val1_2 = d2_1;
    if (flag1)
    {
      double d1_2;
      PlacementUtilities.FindIntersectionPoints(point1, directionalCurve3, directionalCurve4, insul, out d1_2, out double _);
      val1_1 = Math.Max(d1_1, d1_2);
    }
    if (flag2)
    {
      double d2_2;
      PlacementUtilities.FindIntersectionPoints(point2, directionalCurve5, directionalCurve6, insul, out double _, out d2_2);
      val1_2 = Math.Max(d2_1, d2_2);
    }
    double num2 = Math.Min(val1_1, val2_1);
    double num3 = Math.Min(val1_2, val2_2);
    XYZ xyz3 = originPoint + xyz1 * num2 + xyz2 * num3;
    return new Tuple<XYZ, XYZ>(originPoint, xyz3);
  }

  public static Tuple<XYZ, XYZ> FindNewPoints(
    XYZ originPoint,
    XYZ clickPoint,
    Insulation insul,
    out ManualPlacementErrorType mpe)
  {
    try
    {
      double maxLength1 = insul.MaxLength;
      double maxWidth1 = insul.MaxWidth;
      double maxLength2 = insul.MaxLength;
      double maxWidth2 = insul.MaxWidth;
      double insulThickness = insul.InsulThickness;
      mpe = ManualPlacementErrorType.NoMessage;
      double y1 = insul.WallTransform.Inverse.OfPoint(insul.UnionedFace.Origin).Y;
      XYZ xyz1 = insul.WallTransform.Inverse.OfPoint(originPoint);
      XYZ xyz2 = insul.WallTransform.Inverse.OfPoint(clickPoint);
      double num1 = xyz2.X - xyz1.X;
      double num2 = xyz2.Z - xyz1.Z;
      PointOrientation pointOrientation = PointOrientation.LowerLeft;
      if (num1 < 0.0 && num2 < 0.0)
        pointOrientation = PointOrientation.LowerRight;
      else if (num1 < 0.0 && num2 >= 0.0)
        pointOrientation = PointOrientation.UpperRight;
      else if (num1 >= 0.0 && num2 < 0.0)
        pointOrientation = PointOrientation.LowerLeft;
      else if (num1 >= 0.0 && num2 >= 0.0)
        pointOrientation = PointOrientation.UpperLeft;
      XYZ xyz3 = new XYZ(xyz1.X, y1, xyz1.Z);
      XYZ xyz4 = new XYZ(xyz2.X, y1, xyz2.Z);
      XYZ xyz5 = new XYZ(xyz1.X, y1, xyz2.Z);
      XYZ xyz6 = new XYZ(xyz2.X, y1, xyz1.Z);
      Curve bound1 = (Curve) Line.CreateBound(xyz3, xyz5);
      Curve bound2 = (Curve) Line.CreateBound(xyz5, xyz4);
      Curve bound3 = (Curve) Line.CreateBound(xyz4, xyz6);
      Curve bound4 = (Curve) Line.CreateBound(xyz6, xyz3);
      CurveLoop curveLoop = new CurveLoop();
      curveLoop.Append(bound1);
      curveLoop.Append(bound2);
      curveLoop.Append(bound3);
      curveLoop.Append(bound4);
      XYZ xyz7 = insul.IsMirrored ? -XYZ.BasisY : XYZ.BasisY;
      XYZ extrusionDir = insul.IsMirrored ? -insul.WallTransform.BasisY : insul.WallTransform.BasisY;
      Solid transformed = SolidUtils.CreateTransformed(BooleanOperationsUtils.ExecuteBooleanOperation(SolidUtils.CreateTransformed(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
      {
        curveLoop
      }, xyz7, insulThickness), insul.WallTransform), GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) insul.UnionedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>(), extrusionDir, insulThickness), BooleanOperationsType.Intersect), insul.WallTransform.Inverse);
      PlanarFace planarFace1 = (PlanarFace) null;
      foreach (Face face in transformed.Faces)
      {
        if (face is PlanarFace)
        {
          PlanarFace planarFace2 = face as PlanarFace;
          if (face.ComputeNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(xyz7))
          {
            if ((GeometryObject) planarFace1 == (GeometryObject) null)
              planarFace1 = planarFace2;
            double y2 = planarFace2.Origin.Y;
            double y3 = planarFace1.Origin.Y;
            if (insul.IsMirrored)
            {
              if (y2 < y3)
                planarFace1 = planarFace2;
            }
            else if (y2 > y3)
              planarFace1 = planarFace2;
          }
        }
      }
      if (!PlacementUtilities.CheckIntersectedSolids(transformed, xyz7))
        mpe = ManualPlacementErrorType.InsulationCutIntoMultiple;
      if (transformed.Volume == 0.0)
      {
        mpe = ManualPlacementErrorType.PointsOutsideLoop;
        return (Tuple<XYZ, XYZ>) null;
      }
      BoundingBoxUV boundingBox = planarFace1.GetBoundingBox();
      XYZ xyz8 = planarFace1.Evaluate(boundingBox.Min);
      XYZ xyz9 = planarFace1.Evaluate(boundingBox.Max);
      XYZ xyz10 = planarFace1.Evaluate(new UV(boundingBox.Min.U, boundingBox.Max.V));
      XYZ xyz11 = planarFace1.Evaluate(new UV(boundingBox.Max.U, boundingBox.Min.V));
      List<XYZ> source = new List<XYZ>();
      source.Add(xyz8);
      source.Add(xyz9);
      source.Add(xyz10);
      source.Add(xyz11);
      double y4 = xyz8.Y;
      double x1 = source.Min<XYZ>((Func<XYZ, double>) (p => p.X));
      double x2 = source.Max<XYZ>((Func<XYZ, double>) (p => p.X));
      double z1 = source.Min<XYZ>((Func<XYZ, double>) (p => p.Z));
      double z2 = source.Max<XYZ>((Func<XYZ, double>) (p => p.Z));
      XYZ xyz12 = new XYZ(x1, y4, z1);
      XYZ xyz13 = new XYZ(x2, y4, z2);
      XYZ xyz14 = new XYZ(x2, y4, z1);
      XYZ xyz15 = new XYZ(x1, y4, z2);
      Line.CreateBound(xyz12, xyz15);
      Line.CreateBound(xyz15, xyz13);
      Line.CreateBound(xyz13, xyz14);
      Line.CreateBound(xyz14, xyz12);
      double x3 = 0.0;
      double z3 = 0.0;
      double x4 = 0.0;
      double z4 = 0.0;
      switch (pointOrientation)
      {
        case PointOrientation.LowerLeft:
          x3 = xyz12.X;
          z3 = xyz13.Z;
          x4 = xyz13.X;
          z4 = xyz12.Z;
          break;
        case PointOrientation.UpperLeft:
          x3 = xyz12.X;
          z3 = xyz12.Z;
          x4 = xyz13.X;
          z4 = xyz13.Z;
          break;
        case PointOrientation.LowerRight:
          x3 = xyz13.X;
          z3 = xyz13.Z;
          x4 = xyz12.X;
          z4 = xyz12.Z;
          break;
        case PointOrientation.UpperRight:
          x3 = xyz13.X;
          z3 = xyz12.Z;
          x4 = xyz12.X;
          z4 = xyz13.Z;
          break;
      }
      XYZ xyz16 = new XYZ(xyz12.X, xyz2.Y, xyz12.Z);
      XYZ xyz17 = new XYZ(xyz13.X, xyz2.Y, xyz13.Z);
      XYZ point1 = new XYZ(x3, xyz2.Y, z3);
      XYZ point2 = new XYZ(x4, xyz2.Y, z4);
      return new Tuple<XYZ, XYZ>(insul.WallTransform.OfPoint(point1), insul.WallTransform.OfPoint(point2));
    }
    catch (Exception ex)
    {
      mpe = ManualPlacementErrorType.InvalidDimensions;
      return (Tuple<XYZ, XYZ>) null;
    }
  }

  public static bool CheckIntersectedSolids(Solid intersectedSolid, XYZ basisY)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (Face face in intersectedSolid.Faces)
    {
      PlanarFace planarFace = face as PlanarFace;
      if ((GeometryObject) planarFace != (GeometryObject) null && (planarFace.FaceNormal.IsAlmostEqualTo(basisY) || planarFace.FaceNormal.IsAlmostEqualTo(-basisY)))
      {
        ++num2;
        if (num2 <= 1)
        {
          if (planarFace.EdgeLoops.Size > 1)
          {
            foreach (CurveLoop edgesAsCurveLoop1 in (IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops())
            {
              bool flag = false;
              foreach (CurveLoop edgesAsCurveLoop2 in (IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops())
              {
                if (CAM_Utils.CurveLoopContainsCurveLoop(edgesAsCurveLoop2, edgesAsCurveLoop1))
                {
                  flag = true;
                  break;
                }
              }
              if (!flag)
                ++num1;
              if (num1 > 1)
                break;
            }
          }
        }
        else
          break;
      }
    }
    return num1 <= 1;
  }

  public static CurveLoop GetOuterLoop(List<CurveLoop> loops)
  {
    foreach (CurveLoop loop1 in loops)
    {
      bool flag = true;
      foreach (CurveLoop loop2 in loops)
      {
        if (CAM_Utils.CurveLoopContainsCurveLoop(loop2, loop1))
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return loop1;
    }
    return (CurveLoop) null;
  }

  public static Tuple<XYZ, XYZ> FindIntersectionPoints(
    XYZ point,
    Curve c1,
    Curve c2,
    Insulation insul,
    out double d1,
    out double d2)
  {
    double num1 = double.MaxValue;
    double num2 = double.MaxValue;
    XYZ xyz1 = (XYZ) null;
    XYZ xyz2 = (XYZ) null;
    foreach (EdgeArray edgeLoop in insul.UnionedFace.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        Curve curve = edge.AsCurve();
        if (!((GeometryObject) curve == (GeometryObject) null) && curve.IsBound)
        {
          IntersectionResultArray resultArray1;
          if (c1.Intersect(curve, out resultArray1) == SetComparisonResult.Overlap)
          {
            XYZ xyzPoint = resultArray1.get_Item(0).XYZPoint;
            if (resultArray1.get_Item(0).UVPoint.U > 0.0)
            {
              double num3 = xyzPoint.DistanceTo(point);
              if (num3 < num1)
              {
                num1 = num3;
                xyz1 = xyzPoint;
              }
            }
          }
          IntersectionResultArray resultArray2;
          if (c2.Intersect(curve, out resultArray2) == SetComparisonResult.Overlap)
          {
            XYZ xyzPoint = resultArray2.get_Item(0).XYZPoint;
            if (resultArray2.get_Item(0).UVPoint.U > 0.0)
            {
              double num4 = xyzPoint.DistanceTo(point);
              if (num4 < num2)
              {
                num2 = num4;
                xyz2 = xyzPoint;
              }
            }
          }
        }
      }
    }
    d1 = num1;
    d2 = num2;
    return new Tuple<XYZ, XYZ>(xyz1, xyz2);
  }

  public static double FindCurveIntersection(
    Curve c,
    XYZ point,
    Insulation insul,
    out XYZ intersectionPoint)
  {
    double curveIntersection = double.MaxValue;
    XYZ xyz = (XYZ) null;
    foreach (EdgeArray edgeLoop in insul.UnionedFace.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        Curve curve = edge.AsCurve();
        IntersectionResultArray resultArray;
        if (!((GeometryObject) curve == (GeometryObject) null) && curve.IsBound && c.Intersect(curve, out resultArray) == SetComparisonResult.Overlap)
        {
          XYZ xyzPoint = resultArray.get_Item(0).XYZPoint;
          double num = xyzPoint.DistanceTo(point);
          if (num < curveIntersection)
          {
            curveIntersection = num;
            xyz = xyzPoint;
          }
        }
      }
    }
    intersectionPoint = xyz;
    return curveIntersection;
  }

  public static Curve CreateDirectionalCurve(
    XYZ point,
    double dirVector,
    double unionedY,
    bool isX = false)
  {
    XYZ direction = isX ? XYZ.BasisX : XYZ.BasisZ;
    return dirVector < 0.0 ? (Curve) Line.CreateUnbound(new XYZ(point.X, unionedY, point.Z), -direction) : (Curve) Line.CreateUnbound(new XYZ(point.X, unionedY, point.Z), direction);
  }

  public static bool CheckPointWithinLoop(XYZ point, PlanarFace loopFace)
  {
    XYZ point1 = new XYZ(point.X, loopFace.Origin.Y, point.Z);
    return loopFace.Project(point1) != null;
  }

  public static bool CheckPointWithinLoop(XYZ point, PlanarFace loopFace, Insulation insul)
  {
    if (loopFace.Project(point) != null)
      return true;
    foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) loopFace.GetEdgesAsCurveLoops())
    {
      Solid extrusionGeometry;
      if (insul.toleranceLine > 0.0)
        extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          edgesAsCurveLoop,
          CurveLoop.CreateViaOffset(edgesAsCurveLoop, insul.toleranceLine, loopFace.FaceNormal)
        }, loopFace.FaceNormal, 1.0);
      else
        extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          edgesAsCurveLoop
        }, loopFace.FaceNormal, 1.0);
      if (!((GeometryObject) extrusionGeometry.Faces.Cast<Face>().Where<Face>((Func<Face, bool>) (f => f.ComputeNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(loopFace.FaceNormal))).FirstOrDefault<Face>() == (GeometryObject) null))
        return true;
    }
    return false;
  }

  public static bool CheckPointAgainstInnerLoop(XYZ point, Insulation insul)
  {
    XYZ faceNormal = insul.Face.FaceNormal;
    foreach (Solid invalidInsulSolid in insul.InvalidInsulSolids)
    {
      foreach (Face face in invalidInsulSolid.Faces)
      {
        XYZ normal = face.ComputeNormal(new UV(0.0, 0.0));
        if (Math.Abs(faceNormal.DotProduct(normal)) > 1E-05)
        {
          IntersectionResult intersectionResult = face.Project(point);
          try
          {
            if (intersectionResult != null)
            {
              if (!((GeometryObject) intersectionResult.EdgeObject != (GeometryObject) null))
                return false;
            }
          }
          catch
          {
            return false;
          }
        }
      }
    }
    return true;
  }

  public static Tuple<XYZ, XYZ> CheckPointsOnEdge(
    Tuple<XYZ, XYZ> clickPoints,
    PlanarFace loopFace,
    List<Solid> insulSolids,
    double toleranceLine)
  {
    XYZ xyz1 = new XYZ(clickPoints.Item1.X, loopFace.Origin.Y, clickPoints.Item1.Z);
    XYZ xyz2 = new XYZ(clickPoints.Item2.X, loopFace.Origin.Y, clickPoints.Item2.Z);
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    double num1 = 1.0 / 192.0;
    foreach (EdgeArray edgeLoop in loopFace.EdgeLoops)
    {
      foreach (Edge edge in edgeLoop)
      {
        if (!(flag1 & flag2))
        {
          Curve curve = edge.AsCurve();
          if (curve.IsBound)
          {
            XYZ endPoint1 = curve.GetEndPoint(0);
            XYZ endPoint2 = curve.GetEndPoint(1);
            XYZ xyz3 = curve.Evaluate(0.5, true);
            if (endPoint1.DistanceTo(xyz1) <= num1 || endPoint2.DistanceTo(xyz1) <= num1 || xyz3.DistanceTo(xyz1) <= num1)
              flag1 = true;
            if (endPoint1.DistanceTo(xyz2) <= num1 || endPoint2.DistanceTo(xyz2) <= num1 || xyz3.DistanceTo(xyz2) <= num1)
              flag2 = true;
          }
        }
        else
          break;
      }
    }
    if (toleranceLine > 0.0)
    {
      foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) loopFace.GetEdgesAsCurveLoops())
      {
        foreach (Curve curve in CurveLoop.CreateViaOffset(edgesAsCurveLoop, toleranceLine, loopFace.FaceNormal))
        {
          if (!(flag1 & flag2))
          {
            if (curve.IsBound)
            {
              XYZ endPoint3 = curve.GetEndPoint(0);
              XYZ endPoint4 = curve.GetEndPoint(1);
              XYZ xyz4 = curve.Evaluate(0.5, true);
              if (endPoint3.DistanceTo(xyz1) <= num1 || endPoint4.DistanceTo(xyz1) <= num1 || xyz4.DistanceTo(xyz1) <= num1)
              {
                if (!flag1)
                  flag1 = true;
                else
                  continue;
              }
              if ((endPoint3.DistanceTo(xyz2) <= num1 || endPoint4.DistanceTo(xyz2) <= num1 || xyz4.DistanceTo(xyz2) <= num1) && !flag2)
                flag2 = true;
            }
          }
          else
            break;
        }
      }
    }
    foreach (Solid insulSolid in insulSolids)
    {
      if (!(flag1 & flag2))
      {
        foreach (Edge edge in insulSolid.Edges)
        {
          if (!(flag1 & flag2))
          {
            Curve curve = edge.AsCurve();
            if (curve.IsBound)
            {
              XYZ endPoint5 = curve.GetEndPoint(0);
              XYZ endPoint6 = curve.GetEndPoint(1);
              XYZ xyz5 = curve.Evaluate(0.5, true);
              if (endPoint5.DistanceTo(xyz1) <= num1 || endPoint6.DistanceTo(xyz1) <= num1 || xyz5.DistanceTo(xyz1) <= num1)
                flag1 = true;
              if (endPoint5.DistanceTo(xyz2) <= num1 || endPoint6.DistanceTo(xyz2) <= num1 || xyz5.DistanceTo(xyz2) <= num1)
                flag2 = true;
            }
          }
          else
            break;
        }
      }
      else
        break;
    }
    if (flag1 & flag2 || flag1 && !flag2)
      return clickPoints;
    if (!flag1 & flag2)
      return new Tuple<XYZ, XYZ>(clickPoints.Item2, clickPoints.Item1);
    if (!flag1 && !flag2)
    {
      foreach (EdgeArray edgeLoop in loopFace.EdgeLoops)
      {
        foreach (Edge edge in edgeLoop)
        {
          Curve curve = edge.AsCurve();
          if (curve.IsBound)
          {
            if (curve is Arc)
            {
              if (clickPoints.Item1.X.ApproximatelyEquals((curve as Arc).Center.X) && clickPoints.Item1.Z.ApproximatelyEquals((curve as Arc).Center.Z) && (curve as Arc).Radius < num1)
              {
                flag3 = true;
                flag4 = true;
              }
            }
            else
            {
              if (curve.Distance(xyz1) < num1)
                flag3 = true;
              if (curve.Distance(xyz2) < num1)
                flag4 = true;
            }
          }
        }
      }
      if (toleranceLine > 0.0)
      {
        foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) loopFace.GetEdgesAsCurveLoops())
        {
          foreach (Curve curve in CurveLoop.CreateViaOffset(edgesAsCurveLoop, toleranceLine, loopFace.FaceNormal))
          {
            if (curve.IsBound)
            {
              if (curve.Distance(xyz1) < num1)
                flag3 = true;
              if (curve.Distance(xyz2) < num1)
                flag4 = true;
            }
          }
        }
      }
      foreach (Solid insulSolid in insulSolids)
      {
        foreach (Edge edge in insulSolid.Edges)
        {
          Curve curve = edge.AsCurve();
          if (curve.IsBound)
          {
            if (curve.Distance(xyz1) < num1)
              flag3 = true;
            if (curve.Distance(xyz1) < num1)
              flag4 = true;
          }
        }
      }
    }
    if (flag3 & flag4 || flag3 && !flag4)
      return clickPoints;
    if (!flag3 & flag4)
      return new Tuple<XYZ, XYZ>(clickPoints.Item2, clickPoints.Item1);
    if (flag3)
      return clickPoints;
    int num2 = flag4 ? 1 : 0;
    return clickPoints;
  }

  public static void CallManualPlacementErrorMessage(ManualPlacementErrorType errType)
  {
    switch (errType)
    {
      case ManualPlacementErrorType.NoValidElement:
        TaskDialog.Show("Warning", "No valid structural framing element(s) found for insulation placement. Please ensure that a structural framing member contains both \"WALL\" and \"INSULATED\" in the CONSTRUCTION PRODUCT and is either selected or exists in your active view.");
        break;
      case ManualPlacementErrorType.TooManySelected:
        TaskDialog.Show("Warning", "Too many elements have been selected. Please ensure that only one structural framing member with a CONSTRUCTION PRODUCT containing both \"WALL\" and \"INSULATED\" has been selected.");
        break;
      case ManualPlacementErrorType.InvalidView:
        TaskDialog.Show("Error", "Current view is not an acceptable view for placement. Please ensure that the interior or exterior face of the selected wall panel is normal to the active view or run Manual Placement in a 3D view.");
        break;
      case ManualPlacementErrorType.UnlockView:
        TaskDialog.Show("Warning", "Manual Insulation Placement cannot be run while the 3d View is locked");
        break;
      case ManualPlacementErrorType.Success:
        TaskDialog.Show("Insulation Placed", "Insulation placed.");
        break;
      case ManualPlacementErrorType.FamilySymbolNotFound:
        TaskDialog.Show("Warning", "The insulation family was not loaded into the current project. Please ensure the family is loaded and active in the current project in order to run the Manual Insulation Placement tool.");
        break;
      case ManualPlacementErrorType.PointsOutsideLoop:
        TaskDialog.Show("Warning", "Both clicked points were outside of the bounds of the available click space. Please ensure at least one point is within available click space for insulation placement");
        break;
      case ManualPlacementErrorType.LessThanLenMin:
        TaskDialog.Show("Warning", "The selected length or width is less than the DIM_LENGTH_MIN/DIM_WIDTH_MIN for insulation and upon expansion to the minimum will intersect with invalid space. Please ensure that your point placement allows for valid expansion of the minimum in valid space.");
        break;
      case ManualPlacementErrorType.InsulationCutIntoMultiple:
        TaskDialog.Show("Warning", "The insulation you are trying to place will be split into multiple pieces of insulation by invalid space. Please ensure that your point placement only allows for one possible piece of insulation to be created.");
        break;
      case ManualPlacementErrorType.InvalidDimensions:
        TaskDialog.Show("Warning", "The insulation you are trying to place has a length and/or width of 0. Please ensure that the length and/or width of the insulation is greater than 0.");
        break;
    }
  }

  public static void RunInsulationRemoval(Element elem, Document revitDoc)
  {
    List<Element> elements = EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.RetrieveValidSFPieces(new List<Element>()
    {
      elem
    });
    List<Element> roElements = new List<Element>();
    List<Element> sfElements = Utils.SelectionUtils.SelectionUtils.CheckForReadOnly(elements, "INSULATION_INCLUDED", out roElements);
    try
    {
      int num = (int) EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.RemoveInsulFromSF(revitDoc, sfElements);
    }
    catch
    {
    }
  }

  public static List<Element> GetInsulationTypesForFamily(Family family)
  {
    Document document = family.Document;
    List<Element> source = new List<Element>();
    foreach (ElementId familySymbolId in (IEnumerable<ElementId>) family.GetFamilySymbolIds())
      source.Add(document.GetElement(familySymbolId));
    return source.Where<Element>((Func<Element, bool>) (s => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(s))).ToList<Element>();
  }

  public static List<Element> GetInsulationFamilies(Document revitDoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList = new List<Element>();
    return new FilteredElementCollector(revitDoc).OfClass(typeof (FamilySymbol)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(e))).ToList<Element>();
  }

  public static FamilySymbol getTypesForManual(
    UIDocument uiDoc,
    Family family,
    List<Element> allSymbols,
    bool ifInstance,
    Element wall)
  {
    FamilySymbol typesForManual = (FamilySymbol) null;
    string name = family.Name;
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    double parameterAsDouble1 = Parameters.GetParameterAsDouble(wall, "DIM_WYTHE_INSULATION");
    if (!ifInstance)
    {
      List<Element> values = new List<Element>();
      bool flag = true;
      foreach (FamilySymbol allSymbol in allSymbols)
      {
        double parameterAsDouble2 = Parameters.GetParameterAsDouble((Element) allSymbol, "DIM_THICKNESS");
        if (parameterAsDouble1.ApproximatelyEquals(parameterAsDouble2))
        {
          values.Add((Element) allSymbol);
          flag = false;
        }
      }
      if (!flag)
      {
        InsulationSelection insulationSelection = new InsulationSelection(values, mainWindowHandle, false);
        string str;
        if (insulationSelection.insulations.Items.Count == 1)
        {
          str = (insulationSelection.insulations.Items[0] as ListViewItem).Content.ToString();
        }
        else
        {
          insulationSelection.ShowDialog();
          str = (insulationSelection.insulations.SelectedItem as ListViewItem).Content.ToString();
        }
        foreach (Element allSymbol in allSymbols)
        {
          if ((allSymbol as FamilySymbol).Name.Equals(str))
          {
            typesForManual = allSymbol as FamilySymbol;
            break;
          }
        }
      }
      else
      {
        string str = $"   {(wall as FamilyInstance).Symbol.FamilyName} {wall.Name} : {wall.Id.ToString()}\n";
        new TaskDialog("Manual Insulation Placement")
        {
          Title = "Manual Insulation Placement",
          MainContent = "A type for the selected insulation family could not be found with a DIM_THICKNESS value that corresponds to the insulation thickness defined by the structural framing element. The structural framing elements listed below were not processed since an appropriate insulation family type could not be found to place.",
          ExpandedContent = str
        }.Show();
        return (FamilySymbol) null;
      }
    }
    else
    {
      InsulationSelection insulationSelection = new InsulationSelection(allSymbols, mainWindowHandle, false);
      string str;
      if (insulationSelection.insulations.Items.Count == 1)
      {
        str = (insulationSelection.insulations.Items[0] as ListViewItem).Content.ToString();
      }
      else
      {
        insulationSelection.ShowDialog();
        str = (insulationSelection.insulations.SelectedItem as ListViewItem).Content.ToString();
      }
      foreach (Element allSymbol in allSymbols)
      {
        if ((allSymbol as FamilySymbol).Name.Equals(str))
        {
          typesForManual = allSymbol as FamilySymbol;
          break;
        }
      }
    }
    return typesForManual;
  }

  public static Dictionary<FamilySymbol, List<Element>> getTypes(
    UIDocument uiDoc,
    Family family,
    List<Element> allSymbols,
    bool ifInstance,
    Dictionary<double, List<Element>> thicknessSeparatedElements = null,
    List<Element> alltheElements = null)
  {
    Dictionary<FamilySymbol, List<Element>> types = new Dictionary<FamilySymbol, List<Element>>();
    List<Element> source = new List<Element>();
    string name = family.Name;
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    if (!ifInstance)
    {
      foreach (double key in thicknessSeparatedElements.Keys)
      {
        List<Element> values = new List<Element>();
        bool flag = false;
        foreach (FamilySymbol allSymbol in allSymbols)
        {
          double parameterAsDouble = Parameters.GetParameterAsDouble((Element) allSymbol, "DIM_THICKNESS");
          if (key.ApproximatelyEquals(parameterAsDouble))
          {
            values.Add((Element) allSymbol);
            flag = true;
          }
        }
        if (!flag)
        {
          foreach (Element element in thicknessSeparatedElements[key])
            source.Add(element);
        }
        else
        {
          InsulationSelection insulationSelection = new InsulationSelection(values, mainWindowHandle, false);
          string str;
          if (insulationSelection.insulations.Items.Count == 1)
          {
            str = (insulationSelection.insulations.Items[0] as ListViewItem).Content.ToString();
          }
          else
          {
            insulationSelection.ShowDialog();
            str = (insulationSelection.insulations.SelectedItem as ListViewItem).Content.ToString();
          }
          foreach (Element allSymbol in allSymbols)
          {
            if ((allSymbol as FamilySymbol).Name.Equals(str))
            {
              types.Add(allSymbol as FamilySymbol, thicknessSeparatedElements[key]);
              break;
            }
          }
        }
      }
      if (source.Count<Element>() > 0)
      {
        string str = "Elements : \n";
        foreach (Element element in source)
          str = $"{str}   {(element as FamilyInstance).Symbol.FamilyName} {element.Name} : {element.Id.ToString()}\n";
        new TaskDialog("Automatic Insulation Placement")
        {
          Title = "Automatic Insulation Placement",
          MainContent = "A type for the selected insulation family could not be found with a DIM_THICKNESS value that corresponds to the insulation thickness defined by the structural framing element. The structural framing elements listed below were not processed since an appropriate insulation family type could not be found to place.",
          ExpandedContent = str
        }.Show();
      }
    }
    else
    {
      InsulationSelection insulationSelection = new InsulationSelection(allSymbols, mainWindowHandle, false);
      string str;
      if (insulationSelection.insulations.Items.Count == 1)
      {
        str = (insulationSelection.insulations.Items[0] as ListViewItem).Content.ToString();
      }
      else
      {
        insulationSelection.ShowDialog();
        str = (insulationSelection.insulations.SelectedItem as ListViewItem).Content.ToString();
      }
      foreach (Element allSymbol in allSymbols)
      {
        if ((allSymbol as FamilySymbol).Name.Equals(str))
        {
          types.Add(allSymbol as FamilySymbol, alltheElements);
          break;
        }
      }
    }
    return types;
  }

  public static Family defaultExists(Document revitDoc, string familyname)
  {
    List<Element> insulationFamilies = PlacementUtilities.GetInsulationFamilies(revitDoc);
    List<Element> source = new List<Element>();
    foreach (Element element in insulationFamilies)
    {
      string familyName = (element as FamilySymbol).FamilyName;
      if (familyName.Equals(familyname) && familyName.Equals(familyname))
        source.Add(element);
    }
    return source.Count == 0 ? (Family) null : (source.First<Element>() as FamilySymbol).Family;
  }

  public static List<Element> GetUniqueInsulationFamilies(Document revitdoc)
  {
    List<Element> insulationFamilies = PlacementUtilities.GetInsulationFamilies(revitdoc);
    List<Element> source = new List<Element>();
    foreach (Element element1 in insulationFamilies)
    {
      FamilySymbol familySymbol = element1 as FamilySymbol;
      Family family = familySymbol.Family;
      string familyName = familySymbol.FamilyName;
      bool flag = false;
      if (source.Count<Element>() > 0)
      {
        foreach (Element element2 in source)
        {
          if (element2.Name.Equals(family.Name))
            flag = true;
        }
        if (!flag)
          source.Add((Element) family);
      }
      else
        source.Add((Element) family);
    }
    return source;
  }

  public static Family FamilySelection(Document revitDoc)
  {
    List<Element> insulationFamilies = PlacementUtilities.GetInsulationFamilies(revitDoc);
    string str = "";
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    InsulationSelection insulationSelection = new InsulationSelection(insulationFamilies, mainWindowHandle, true);
    if (insulationSelection.insulations.Items.Count == 1)
    {
      str = (insulationSelection.insulations.Items[0] as ListViewItem).Content.ToString();
    }
    else
    {
      insulationSelection.ShowDialog();
      if (insulationSelection.insulations.SelectedItem is ListViewItem)
        str = (insulationSelection.insulations.SelectedItem as ListViewItem).Content.ToString();
    }
    List<Element> source = new List<Element>();
    Family family = (Family) null;
    if (!string.IsNullOrEmpty(str))
    {
      foreach (Element element in insulationFamilies)
      {
        if ((element as FamilySymbol).FamilyName.Equals(str))
          source.Add(element);
      }
      if (source.Count<Element>() > 0)
        family = (source.First<Element>() as FamilySymbol).Family;
    }
    return family;
  }

  public static bool checkInstanceParam(
    string paramName,
    Family insulationFamily,
    ParameterValues parameter)
  {
    if (parameter != null)
    {
      if (parameter.parameterType == parameterExistence.INSTANCE)
      {
        if (!parameter.readOnly)
          return true;
        new TaskDialog("Insulation Placement")
        {
          Title = "Insulation Placement",
          MainInstruction = $"Please check the {paramName} parameter. It is set to read only for the {insulationFamily.Name} family."
        }.Show();
        return false;
      }
      new TaskDialog("Insulation Placement")
      {
        Title = "Insulation Placement",
        MainInstruction = $"Please check {paramName} parameter. It should be an instance parameter in the {insulationFamily.Name} family."
      }.Show();
      return false;
    }
    new TaskDialog("Insulation Placement")
    {
      Title = "Insulation Placement",
      MainInstruction = $"Please check the {paramName} parameter. It does not exist for the {insulationFamily.Name} family."
    }.Show();
    return false;
  }

  public static double RoundValue(double val, bool bMetric = false)
  {
    return bMetric ? Math.Round(val * 304.8) / 304.8 : Math.Floor(Math.Round(val * 256.0 * 12.0) / 32.0) / 8.0 / 12.0;
  }
}
