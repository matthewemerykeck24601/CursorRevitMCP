// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.DirectionalMarker.DirectionalMarkerMain
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using EDGE.InsulationTools.PinPlacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.DirectionalMarker;

[Transaction(TransactionMode.Manual)]
public class DirectionalMarkerMain : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Directional Marker Placement must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Directional Marker Placement must be run in the project environment. Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Directional Marker Placement"))
    {
      int num1 = (int) transactionGroup.Start();
      List<FamilySymbol> familySymbol;
      int num2 = DirectionalMarkerMain.checkForVoidFamily(document, out familySymbol) ? 1 : 0;
      FamilySymbol xpsFamily = familySymbol.Where<FamilySymbol>((Func<FamilySymbol, bool>) (e => e.FamilyName.Contains("XPS"))).ToList<FamilySymbol>().FirstOrDefault<FamilySymbol>();
      FamilySymbol isoFamily = familySymbol.Where<FamilySymbol>((Func<FamilySymbol, bool>) (e => e.FamilyName.Contains("ISO"))).ToList<FamilySymbol>().FirstOrDefault<FamilySymbol>();
      if (num2 == 0)
      {
        string str = xpsFamily != null || isoFamily != null ? (xpsFamily != null ? "VOID_DIRECTIONAL_MARKER_ISO family" : "VOID_DIRECTIONAL_MARKER_XPS family") : "VOID_DIRECTIONAL_MARKER_ISO & VOID_DIRECTIONAL_MARKER_XPS families";
        new TaskDialog("Warning")
        {
          MainInstruction = $"Please load {str} in order to successfully place directional markers.",
          TitleAutoPrefix = false
        }.Show();
        return (Result) 1;
      }
      TaskDialog taskDialog = new TaskDialog("Directional Marker Placement");
      taskDialog.Title = "Directional Marker Placement";
      taskDialog.TitleAutoPrefix = true;
      taskDialog.AllowCancellation = true;
      taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
      taskDialog.MainInstruction = "Directional Marker Placement";
      taskDialog.MainContent = "Select the scope for Directional Marker Placement.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Directional Marker Placement for the Whole Project.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Directional Marker Placement for the Active View");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Directional Marker Placement for a Selection Group");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      Result result = (Result) 0;
      ICollection<ElementId> elementIds = application.ActiveUIDocument.Selection.GetElementIds();
      List<Element> elementList1 = new List<Element>();
      List<Element> elementList2;
      string str1;
      if (taskDialogResult == 1001)
      {
        elementList2 = PinPlacementChoice.WholeModel(document);
        str1 = " model.";
      }
      else if (taskDialogResult == 1002)
      {
        elementList2 = PinPlacementChoice.ActiveView(document, activeUiDocument);
        str1 = " view.";
      }
      else if (taskDialogResult == 1003)
      {
        Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
        elementList2 = PinPlacementChoice.SelectedElements(document, elementIds, selection, true);
        str1 = " selection.";
        if (elementList2 == null)
          return (Result) 1;
      }
      else
      {
        int num3 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (result == 1 || result == -1)
      {
        int num4 = (int) transactionGroup.RollBack();
        return result;
      }
      if (elementList2.Count == 0)
      {
        int num5 = (int) MessageBox.Show("There was no insulation with MANUFACTURE_COMPONENT containing INSULATION found in the" + str1);
        return (Result) 1;
      }
      Dictionary<Element, List<Element>> wallPanelAndInsulations = new Dictionary<Element, List<Element>>();
      List<string> values1 = new List<string>();
      List<string> values2 = new List<string>();
      List<Element> removeElements = new List<Element>();
      List<Element> collection = new List<Element>();
      bool flag = false;
      elementList2.Select<Element, ElementId>((Func<Element, ElementId>) (i => i.Id)).ToList<ElementId>();
      foreach (Element elem in elementList2)
      {
        Parameter parameter = Parameters.LookupParameter(elem, "HOST_GUID");
        if (parameter != null || !string.IsNullOrEmpty(parameter.AsString()))
        {
          if (document.GetElement(parameter.AsString()) is FamilyInstance)
          {
            List<Element> removeElements1;
            List<Element> addElements;
            DirectionalMarkerMain.filterCorrectInsulationPieces(document, elem, elementList2, out removeElements1, out addElements);
            removeElements.AddRange((IEnumerable<Element>) removeElements1);
            collection.AddRange((IEnumerable<Element>) addElements);
          }
          else if (Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(elem, true))
          {
            List<Element> removeElements2;
            List<Element> addElements;
            DirectionalMarkerMain.filterCorrectInsulationPieces(document, elem, elementList2, out removeElements2, out addElements);
            removeElements.AddRange((IEnumerable<Element>) removeElements2);
            collection.AddRange((IEnumerable<Element>) addElements);
          }
        }
      }
      elementList2.AddRange((IEnumerable<Element>) collection);
      if (removeElements.Count > 0)
        elementList2.RemoveAll((Predicate<Element>) (e => removeElements.Contains(e)));
      foreach (Element elem in elementList2)
      {
        Parameter parameter = Parameters.LookupParameter(elem, "HOST_GUID");
        if (parameter == null)
        {
          string str2 = $"{(elem as FamilySymbol).FamilyName} - {elem.Id?.ToString()}";
          values1.Add(str2);
          flag = true;
        }
        else if (string.IsNullOrEmpty(parameter.AsString()))
        {
          string str3 = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Id?.ToString()}";
          values1.Add(str3);
          flag = true;
        }
        else
        {
          Element element = document.GetElement(parameter.AsString());
          if (element != null)
          {
            if (wallPanelAndInsulations.ContainsKey(element))
              wallPanelAndInsulations[element].Add(elem);
            else
              wallPanelAndInsulations.Add(element, new List<Element>()
              {
                elem
              });
          }
          else
          {
            string str4 = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Id?.ToString()}";
            values2.Add(str4);
            flag = true;
          }
        }
      }
      if (flag)
      {
        if (values1.Count > 0)
          new TaskDialog("Warning")
          {
            MainInstruction = "The following element(s) are either missing the HOST_GUID parameter or do not have a value for it. Please ensure this parameter exists and that you run BOM Product Hosting on the whole model before runnning Directional Marker Placement.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values1),
            TitleAutoPrefix = false
          }.Show();
        if (values2.Count > 0)
          new TaskDialog("Warning")
          {
            MainInstruction = "The following element(s) are not Hosted to a wall panel. Please ensure that insulation exists in a wall panel.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values2),
            TitleAutoPrefix = false
          }.Show();
        return (Result) 1;
      }
      if (wallPanelAndInsulations.Keys.Count > 0)
      {
        using (Transaction transaction = new Transaction(document, "Directional Marker Placement"))
        {
          int num6 = (int) transaction.Start();
          List<string> offendingVolume = new List<string>();
          List<string> notool = new List<string>();
          List<Element> allVoids = new List<Element>();
          DirectionalMarkerMain.directionalMarkerMain(document, wallPanelAndInsulations, xpsFamily, isoFamily, out offendingVolume, out notool, out allVoids);
          document.Regenerate();
          DirectionalMarkerMain.directionalMarkerVoids(document, allVoids, offendingVolume, notool);
          int num7 = (int) transaction.Commit();
        }
      }
      int num8 = (int) transactionGroup.Assimilate();
    }
    return (Result) 0;
  }

  private static void filterCorrectInsulationPieces(
    Document revitDoc,
    Element elem,
    List<Element> listofInsulation,
    out List<Element> removeElements,
    out List<Element> addElements)
  {
    FamilyInstance familyInstance = elem as FamilyInstance;
    List<ElementId> elementIdList = new List<ElementId>();
    removeElements = new List<Element>();
    addElements = new List<Element>();
    if (familyInstance.SuperComponent == null)
    {
      List<ElementId> list = familyInstance.GetSubComponentIds().Where<ElementId>((Func<ElementId, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(revitDoc.GetElement(e)))).ToList<ElementId>();
      foreach (ElementId elementId in list)
      {
        ElementId subId = elementId;
        if (!listofInsulation.Any<Element>((Func<Element, bool>) (e => e.Id.IntegerValue.Equals(subId.IntegerValue))))
          addElements.Add(revitDoc.GetElement(subId));
      }
      if (list.Count <= 0)
        return;
      removeElements.Add(elem);
    }
    else
    {
      if ((familyInstance.SuperComponent as FamilyInstance).GetSubComponentIds().Count <= 0)
        return;
      foreach (ElementId elementId in (familyInstance.SuperComponent as FamilyInstance).GetSubComponentIds().Where<ElementId>((Func<ElementId, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(revitDoc.GetElement(e)))).ToList<ElementId>())
      {
        ElementId subElementId = elementId;
        if (!listofInsulation.Any<Element>((Func<Element, bool>) (e => e.Id.IntegerValue.Equals(subElementId.IntegerValue))))
          addElements.Add(revitDoc.GetElement(subElementId));
      }
    }
  }

  public static void directionalMarkerMain(
    Document revitDoc,
    Dictionary<Element, List<Element>> wallPanelAndInsulations,
    FamilySymbol xpsFamily,
    FamilySymbol isoFamily,
    out List<string> offendingVolume,
    out List<string> notool,
    out List<Element> allVoids)
  {
    offendingVolume = new List<string>();
    notool = new List<string>();
    allVoids = new List<Element>();
    foreach (Element key in wallPanelAndInsulations.Keys)
    {
      foreach (Element element in wallPanelAndInsulations[key])
      {
        bool orientation = DirectionalMarkerMain.checkWallandInsulationOrienatation(element, key);
        string str1 = $"{(element as FamilyInstance).Symbol.FamilyName} - {element.Id?.ToString()}";
        if (element.GetElementVolume() <= 0.0)
        {
          offendingVolume.Add(str1);
        }
        else
        {
          Parameter parameter = Parameters.LookupParameter(element, "TOOL");
          if (parameter != null && parameter.HasValue)
          {
            FamilySymbol voidFamilySymbol = (FamilySymbol) null;
            string str2 = parameter.AsString();
            if (str2.Contains("103"))
              voidFamilySymbol = xpsFamily;
            else if (str2.Contains("106"))
              voidFamilySymbol = isoFamily;
            if (voidFamilySymbol == null)
            {
              notool.Add(str1);
            }
            else
            {
              Element directMark;
              DirectionalMarkerMain.MainPlacement(element, key, revitDoc, voidFamilySymbol, out directMark, orientation);
              if (directMark != null)
                allVoids.Add(directMark);
            }
          }
          else
            notool.Add(str1);
        }
      }
    }
  }

  public static void directionalMarkerVoids(
    Document revitDoc,
    List<Element> allVoids,
    List<string> offendingVolume,
    List<string> notool,
    bool bReplacementTool = false)
  {
    foreach (FamilyInstance allVoid in allVoids)
    {
      Element host = allVoid.Host;
      List<ElementId> list = InstanceVoidCutUtils.GetCuttingVoidInstances(host).Where<ElementId>((Func<ElementId, bool>) (e => (revitDoc.GetElement(e) as FamilyInstance).Symbol.FamilyName.Contains("DIRECTIONAL_MARKER"))).ToList<ElementId>();
      if (list.Count > 0)
      {
        foreach (ElementId elementId in list)
          revitDoc.Delete(elementId);
      }
      if (!InstanceVoidCutUtils.GetElementsBeingCut((Element) allVoid).Contains(host.Id))
        InstanceVoidCutUtils.AddInstanceVoidCut(revitDoc, host, (Element) allVoid);
    }
    if (!bReplacementTool && allVoids.Count > 0)
    {
      int num = (int) MessageBox.Show("All Directional Markers have been placed successfully.");
    }
    if (offendingVolume.Count > 0)
      new TaskDialog("Warning")
      {
        MainContent = "One or more insulation had zero volume and will not get a directional marker placed for it",
        ExpandedContent = string.Join("\n", (IEnumerable<string>) offendingVolume),
        TitleAutoPrefix = false
      }.Show();
    if (notool.Count <= 0)
      return;
    new TaskDialog("Warning")
    {
      MainContent = "Please check the TOOL parameter for the following pieces of insulation. The parameter either does not exist or does not contain a valid value.",
      ExpandedContent = string.Join("\n", (IEnumerable<string>) notool),
      TitleAutoPrefix = false
    }.Show();
  }

  public static bool checkForVoidFamily(Document revitDoc, out List<FamilySymbol> familySymbol)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList = new List<Element>();
    List<Element> list = new FilteredElementCollector(revitDoc).OfClass(typeof (FamilySymbol)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => (e as FamilySymbol).FamilyName.Contains("DIRECTIONAL") && (e as FamilySymbol).FamilyName.Contains("MARKER"))).ToList<Element>();
    familySymbol = new List<FamilySymbol>();
    if (!list.Any<Element>((Func<Element, bool>) (e => (e as FamilySymbol).FamilyName.Contains("ISO"))) || !list.Any<Element>((Func<Element, bool>) (e => (e as FamilySymbol).FamilyName.Contains("XPS"))))
      return false;
    familySymbol = list.Select<Element, FamilySymbol>((Func<Element, FamilySymbol>) (e => e as FamilySymbol)).ToList<FamilySymbol>();
    return true;
  }

  public static void MainPlacement(
    Element insulationList,
    Element wallPanel,
    Document revitDoc,
    FamilySymbol voidFamilySymbol,
    out Element directMark,
    bool orientation)
  {
    voidFamilySymbol.Activate();
    Transform transform1 = (insulationList as FamilyInstance).GetTransform();
    Transform transform2 = (wallPanel as FamilyInstance).GetTransform();
    FamilyInstance wall = wallPanel as FamilyInstance;
    if (!orientation)
    {
      bool bSymbol1;
      PlanarFace frontFace = PlacementUtilities.GetFrontFace((Element) wall, out bSymbol1);
      Transform identity = Transform.Identity;
      identity.BasisX = transform2.BasisZ.CrossProduct(transform1.BasisZ);
      identity.BasisY = transform1.BasisZ;
      identity.BasisZ = transform2.BasisZ;
      bool bSymbol2;
      PlanarFace topFaceReturn;
      XYZ xyz = DirectionalMarkerMain.locationForOrienatation(insulationList, wallPanel, revitDoc, identity, out bSymbol2, out topFaceReturn, out List<PlanarFace> _);
      double x1 = xyz.X;
      double y = xyz.Y;
      double z1 = xyz.Z;
      double x2 = x1 + 1.0 / 6.0;
      double z2 = z1 + 1.0 / 6.0;
      XYZ point = new XYZ(x2, y, z2);
      XYZ location = identity.OfPoint(point);
      if ((!bSymbol1 ? frontFace.FaceNormal : transform2.OfPoint(frontFace.FaceNormal)).CrossProduct(!bSymbol2 ? topFaceReturn.FaceNormal : transform1.OfPoint(topFaceReturn.FaceNormal)).IsAlmostEqualTo(XYZ.Zero))
      {
        directMark = (Element) null;
      }
      else
      {
        FamilyInstance familyInstance = revitDoc.Create.NewFamilyInstance(topFaceReturn.Reference, location, identity.BasisX, voidFamilySymbol);
        directMark = (Element) familyInstance;
      }
    }
    else
    {
      PlanarFace topFaceReturn;
      XYZ location = DirectionalMarkerMain.GetLocation(insulationList, wallPanel, transform2, transform1, out topFaceReturn);
      FamilyInstance familyInstance = !PlacementUtilities.IsMirrored(wallPanel) ? revitDoc.Create.NewFamilyInstance(topFaceReturn.Reference, location, -transform2.BasisX, voidFamilySymbol) : revitDoc.Create.NewFamilyInstance(topFaceReturn.Reference, location, transform2.BasisX, voidFamilySymbol);
      directMark = (Element) familyInstance;
    }
  }

  public static bool checkWallandInsulationOrienatation(Element insulation, Element wallPanel)
  {
    FamilyInstance familyInstance1 = insulation as FamilyInstance;
    FamilyInstance familyInstance2 = wallPanel as FamilyInstance;
    Transform transform = familyInstance1.GetTransform();
    return familyInstance2.GetTransform().BasisY.CrossProduct(transform.BasisZ).IsAlmostEqualTo(XYZ.Zero);
  }

  public static XYZ locationForOrienatation(
    Element insulation,
    Element wallPanel,
    Document revitDoc,
    Transform newCoordinateSystem,
    out bool bSymbol,
    out PlanarFace topFaceReturn,
    out List<PlanarFace> upFaces)
  {
    bool bSymbol1;
    List<PlanarFace> upFaces1;
    PlanarFace topFace;
    DirectionalMarkerMain.GetFrontFaceOrientation(revitDoc, insulation, out topFace, out bSymbol1, out upFaces1, out PlanarFace _);
    upFaces = upFaces1;
    bSymbol = bSymbol1;
    PlacementUtilities.IsMirrored(insulation);
    (wallPanel as FamilyInstance).GetTransform();
    Transform transform = (insulation as FamilyInstance).GetTransform();
    EdgeArrayArray edgeLoops = topFace.EdgeLoops;
    List<CurveLoop> loops = new List<CurveLoop>();
    foreach (IEnumerable source in edgeLoops)
    {
      CurveLoop curveLoop = CurveLoop.Create((IList<Curve>) source.Cast<Edge>().ToList<Edge>().Select<Edge, Curve>((Func<Edge, Curve>) (e => e.AsCurveFollowingFace((Face) topFace))).ToList<Curve>());
      loops.Add(curveLoop);
    }
    List<XYZ> xyzList = new List<XYZ>();
    CurveLoop outerLoop = PlacementUtilities.GetOuterLoop(loops);
    XYZ xyz1 = new XYZ();
    foreach (Curve curve in outerLoop)
    {
      XYZ point1 = curve.GetEndPoint(0);
      XYZ point2 = curve.GetEndPoint(1);
      if (bSymbol)
      {
        point1 = transform.OfPoint(curve.GetEndPoint(0));
        point2 = transform.OfPoint(curve.GetEndPoint(1));
      }
      XYZ source1 = newCoordinateSystem.Inverse.OfPoint(point1);
      XYZ source2 = newCoordinateSystem.Inverse.OfPoint(point2);
      if (xyzList.Count == 0)
      {
        xyzList.Add(source1);
        xyzList.Add(source2);
      }
      else
      {
        bool flag1 = false;
        bool flag2 = false;
        foreach (XYZ xyz2 in xyzList)
        {
          if (!flag1 && xyz2.IsAlmostEqualTo(source1))
            flag1 = true;
          if (!flag2 && xyz2.IsAlmostEqualTo(source2))
            flag2 = true;
          if (flag1 & flag2)
            break;
        }
        if (!flag1)
          xyzList.Add(source1);
        if (!flag2)
          xyzList.Add(source2);
      }
    }
    double x = double.MaxValue;
    double y = double.MaxValue;
    double z = double.MaxValue;
    foreach (XYZ xyz3 in xyzList)
    {
      if (xyz3.X < x)
        x = xyz3.X;
      if (xyz3.Y < y)
        y = xyz3.Y;
      if (xyz3.Z < z)
        z = xyz3.Z;
    }
    XYZ xyz4 = new XYZ(x, y, z);
    XYZ xyz5 = new XYZ(x, y, z);
    topFaceReturn = topFace;
    return xyz5;
  }

  public static void GetFrontFaceOrientation(
    Document revitDoc,
    Element insulation,
    out PlanarFace topFace,
    out bool bSymbol,
    out List<PlanarFace> upFaces,
    out PlanarFace backFace)
  {
    topFace = (PlanarFace) null;
    backFace = (PlanarFace) null;
    List<PlanarFace> planarFaceList = new List<PlanarFace>();
    upFaces = planarFaceList;
    List<Solid> symbolSolids = Solids.GetSymbolSolids(insulation, out bSymbol, options: new Options()
    {
      ComputeReferences = true
    });
    Transform transform = (insulation as FamilyInstance).GetTransform();
    PlacementUtilities.IsMirrored(insulation);
    XYZ basisZ = XYZ.BasisZ;
    EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace transformedFace1 = (EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace) null;
    EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace transformedFace2 = (EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace) null;
    if (symbolSolids.Count<Solid>() == 0)
      return;
    if (bSymbol)
    {
      foreach (Solid solid in symbolSolids)
      {
        foreach (Face face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace planarFace = face as PlanarFace;
            XYZ normal = face.ComputeNormal(new UV(0.0, 0.0));
            if (normal.IsAlmostEqualTo(basisZ))
            {
              if ((GeometryObject) topFace == (GeometryObject) null)
                topFace = planarFace;
              if (planarFace.Origin.Z > topFace.Origin.Z)
                topFace = planarFace;
              planarFaceList.Add(planarFace);
            }
            else if (normal.IsAlmostEqualTo(basisZ.Negate()))
            {
              if ((GeometryObject) backFace == (GeometryObject) null)
                backFace = planarFace;
              if (planarFace.Origin.Z < backFace.Origin.Z)
                backFace = planarFace;
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
            EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace transformedFace3 = new EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace(transform, f);
            if (transformedFace3.Normal.IsAlmostEqualTo(basisZ))
            {
              if (transformedFace1 == null)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
              if (transformedFace1.Origin.Z < transformedFace3.Origin.Z)
              {
                transformedFace1 = transformedFace3;
                topFace = f;
              }
              planarFaceList.Add(f);
            }
            else if (transformedFace3.Normal.IsAlmostEqualTo(basisZ.Negate()))
            {
              if (transformedFace2 == null)
              {
                transformedFace2 = transformedFace3;
                backFace = f;
              }
              if (transformedFace2.Origin.Z > backFace.Origin.Z)
              {
                transformedFace2 = transformedFace3;
                backFace = f;
              }
            }
          }
        }
      }
    }
  }

  public static XYZ GetLocation(
    Element insulation,
    Element wallPanel,
    Transform wallPanelTransfrom,
    Transform insulationTransform,
    out PlanarFace topFaceReturn)
  {
    Document document = insulation.Document;
    PlacementUtilities.GetFrontFace(wallPanel);
    Element insulation1 = insulation;
    PlanarFace topFace;
    ref PlanarFace local1 = ref topFace;
    PlanarFace planarFace;
    ref PlanarFace local2 = ref planarFace;
    bool flag1;
    ref bool local3 = ref flag1;
    PlacementUtilities.GetInsulationFace(document, insulation1, out local1, out local2, out local3);
    bool flag2 = PlacementUtilities.IsMirrored(wallPanel);
    EdgeArrayArray edgeLoops = topFace.EdgeLoops;
    List<CurveLoop> loops = new List<CurveLoop>();
    foreach (IEnumerable source in edgeLoops)
    {
      CurveLoop curveLoop = CurveLoop.Create((IList<Curve>) source.Cast<Edge>().ToList<Edge>().Select<Edge, Curve>((Func<Edge, Curve>) (e => e.AsCurveFollowingFace((Face) topFace))).ToList<Curve>());
      loops.Add(curveLoop);
    }
    List<XYZ> xyzList = new List<XYZ>();
    CurveLoop outerLoop = PlacementUtilities.GetOuterLoop(loops);
    XYZ xyz1 = new XYZ();
    bool flag3 = true;
    foreach (Curve curve in outerLoop)
    {
      XYZ point1 = curve.GetEndPoint(0);
      XYZ point2 = curve.GetEndPoint(1);
      if (flag1)
      {
        point1 = insulationTransform.OfPoint(curve.GetEndPoint(0));
        point2 = insulationTransform.OfPoint(curve.GetEndPoint(1));
      }
      XYZ xyz2 = wallPanelTransfrom.Inverse.OfPoint(point1);
      XYZ xyz3 = wallPanelTransfrom.Inverse.OfPoint(point2);
      xyzList.Add(xyz2);
      if (flag2)
      {
        if (flag3)
        {
          if (xyz2.Z.ApproximatelyEquals(xyz3.Z))
            xyz1 = xyz2.X >= xyz3.X ? xyz3 : xyz2;
          else if (xyz2.Z < xyz3.Z)
            xyz1 = xyz3;
          else if (xyz3.Z < xyz2.Z)
            xyz1 = xyz3;
          flag3 = false;
        }
        else
        {
          if (xyz2.Z <= xyz1.Z)
          {
            if (xyz2.Z.ApproximatelyEquals(xyz1.Z))
            {
              if (xyz2.X <= xyz1.X)
                xyz1 = xyz2;
            }
            else
              xyz1 = xyz2;
          }
          if (xyz3.Z <= xyz1.Z)
          {
            if (xyz3.Z.ApproximatelyEquals(xyz1.Z))
            {
              if (xyz3.X <= xyz1.X)
                xyz1 = xyz3;
            }
            else
              xyz1 = xyz3;
          }
        }
      }
      else if (flag3)
      {
        if (xyz2.Z.ApproximatelyEquals(xyz3.Z))
          xyz1 = xyz2.X <= xyz3.X ? xyz3 : xyz2;
        else if (xyz2.Z < xyz3.Z)
          xyz1 = xyz3;
        else if (xyz3.Z < xyz2.Z)
          xyz1 = xyz3;
        flag3 = false;
      }
      else
      {
        if (xyz2.Z <= xyz1.Z)
        {
          if (xyz2.Z.ApproximatelyEquals(xyz1.Z))
          {
            if (xyz2.X >= xyz1.X)
              xyz1 = xyz2;
          }
          else
            xyz1 = xyz2;
        }
        if (xyz3.Z <= xyz1.Z)
        {
          if (xyz3.Z.ApproximatelyEquals(xyz1.Z))
          {
            if (xyz3.X >= xyz1.X)
              xyz1 = xyz3;
          }
          else
            xyz1 = xyz3;
        }
      }
    }
    XYZ point = !flag2 ? xyz1 - XYZ.BasisX * (1.0 / 6.0) + XYZ.BasisZ * (1.0 / 6.0) : xyz1 + XYZ.BasisX * (1.0 / 6.0) + XYZ.BasisZ * (1.0 / 6.0);
    XYZ location = wallPanelTransfrom.OfPoint(point);
    topFaceReturn = topFace;
    return location;
  }
}
