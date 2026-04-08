// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.AutoWarping
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using EDGE.QATools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

[Transaction(TransactionMode.Manual)]
internal class AutoWarping : IExternalCommand
{
  private XYZ[] GetEndpointsForSF(FamilyInstance instance, Transform transform)
  {
    XYZ directionForSfElement = WarpingUtils.GetPlanDirectionForSFElement(instance, transform);
    if (directionForSfElement == null)
      return (XYZ[]) null;
    XYZ xyz = transform.Inverse.OfVector(directionForSfElement);
    List<XYZ> xyzList = new List<XYZ>();
    XYZ[] endpointsForSf = new XYZ[4];
    if (instance.Location is LocationPoint)
    {
      LocationPoint location = instance.Location as LocationPoint;
      XYZ endpoint1 = transform.Inverse.OfPoint(location.Point);
      XYZ endpoint2 = endpoint1 + xyz.Normalize() * Utils.ElementUtils.Parameters.GetParameterAsZeroBasedDouble((Element) instance, "DIM_LENGTH");
      Line transformed = Line.CreateBound(endpoint1, endpoint2).CreateTransformed(transform) as Line;
      endpointsForSf[2] = endpoint1;
      endpointsForSf[3] = endpoint2;
      FamilyInstance instance1 = instance;
      Transform transform1 = transform;
      XYZ[] lineWithVoid = WarpingUtils.GetLineWithVoid(transformed, instance1, transform1);
      XYZ[] xyzArray = new XYZ[2];
      if (lineWithVoid == null)
        return (XYZ[]) null;
      endpointsForSf[0] = lineWithVoid[0];
      endpointsForSf[1] = lineWithVoid[1];
    }
    return endpointsForSf;
  }

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Autodesk.Revit.DB.Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Auto Warping Must be run in the Project Environment",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.AutoWarping))
      return (Result) 1;
    Autodesk.Revit.Creation.Document create = revitDoc.Create;
    List<ElementId> idsToCheck = new List<ElementId>();
    List<ElementId> list1 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).ToElementIds().ToList<ElementId>();
    List<ElementId> list2 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_GenericModel).ToElementIds().ToList<ElementId>();
    List<ElementId> list3 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).ToElementIds().ToList<ElementId>();
    idsToCheck.AddRange((IEnumerable<ElementId>) list1);
    idsToCheck.AddRange((IEnumerable<ElementId>) list2);
    idsToCheck.AddRange((IEnumerable<ElementId>) list3);
    if (revitDoc.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Auto Warping", idsToCheck, revitDoc, activeUiDocument, out List<ElementId> _))
      return (Result) 1;
    CurveArray curveArray = new CurveArray();
    FilteredElementCollector source1 = (FilteredElementCollector) null;
    List<ElementId> list4 = commandData.Application.ActiveUIDocument.Selection.GetElementIds().ToList<ElementId>();
    List<Element> elementList = new List<Element>();
    if (list4.Count > 0)
    {
      source1 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list4);
      source1.OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (s => s.Name == "SPOT_ELEVATION"));
      elementList = source1.ToList<Element>();
    }
    TaskDialog taskDialog1 = new TaskDialog("Auto Warping");
    taskDialog1.Id = "ID_Auto_Warping";
    taskDialog1.TitleAutoPrefix = true;
    taskDialog1.AllowCancellation = true;
    taskDialog1.MainInstruction = "Auto Warping";
    taskDialog1.MainContent = "Select the scope of Auto Warping.";
    taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Run Auto Warping for the Whole Model.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Run Auto Warping for the Active View.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Run Auto Warping for Selected Spot Elevations.");
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
    taskDialog1.DefaultButton = (TaskDialogResult) 2;
    TaskDialogResult taskDialogResult = taskDialog1.Show();
    List<ElementId> source2 = new List<ElementId>();
    if (taskDialogResult == 1001)
    {
      source1 = new FilteredElementCollector(revitDoc);
      source1.OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance));
    }
    else if (taskDialogResult == 1002)
    {
      source1 = new FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id);
      source1.OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance));
    }
    else
    {
      if (taskDialogResult != 1003)
        return (Result) 1;
      if (elementList.Count == 0)
        source2 = References.PickNewReferences(commandData.Application.ActiveUIDocument, (ISelectionFilter) new SpotFilter(), "Select Spot Elevations to be updated.").ToList<ElementId>();
    }
    if (source2 == null || source1 == null && source2.Count == 0)
      return (Result) 1;
    IEnumerable<Element> source3 = source2.Count <= 0 ? source1.Where<Element>((Func<Element, bool>) (s => s.Name == "SPOT_ELEVATION")) : source2.Select<ElementId, Element>((Func<ElementId, Element>) (eid => revitDoc.GetElement(eid)));
    List<ElementId> elementIdList1 = new List<ElementId>();
    foreach (FamilyInstance familyInstance in source3.Cast<FamilyInstance>())
    {
      switch (familyInstance.Host)
      {
        case ReferencePlane _:
        case Level _:
          continue;
        default:
          string str = "Invalid Spot Elevations:" + Environment.NewLine;
          elementIdList1.Add(familyInstance.Id);
          continue;
      }
    }
    if (elementIdList1.Count > 0)
    {
      string str1 = "{";
      foreach (ElementId elementId in elementIdList1)
        str1 = $"{str1}{elementId.ToString()}, ";
      string str2;
      string str3 = str2 = str1.Remove(str1.Length - 2) + "}";
      new TaskDialog("Auto Warping")
      {
        MainContent = "Some spot elevations had missing or invalid work planes. All spot elevations must be hosted to a reference plane or level to run Auto Warping. The operation will be cancelled. Expand for details.",
        ExpandedContent = str3
      }.Show();
      return (Result) 1;
    }
    if (source3.Count<Element>() == 0)
    {
      TaskDialog.Show("Auto Warping", "No Spot Elevations found in scope. Ensure that Spot Elevation elements surround elements to be warped. The operation will be cancelled.");
      return (Result) 1;
    }
    List<string> list5 = source3.Select<Element, string>((Func<Element, string>) (elem => Utils.ElementUtils.Parameters.GetParameterAsString(elem, "Work Plane"))).Distinct<string>().ToList<string>();
    new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Entourage).OfClass(typeof (DirectShape)).ToElementIds();
    try
    {
      using (Transaction transaction = new Transaction(revitDoc, "Set Member Warping"))
      {
        int num1 = (int) transaction.Start();
        Dictionary<string, List<string>> entourages = this.CollectEntourages(revitDoc);
        List<string> source4 = new List<string>();
        Dictionary<string, List<ElementId>> dictionary1 = new Dictionary<string, List<ElementId>>();
        List<WarpingEdgeLoop> nonLinearLoops = new List<WarpingEdgeLoop>();
        List<List<WarpingEdge>> collisions = new List<List<WarpingEdge>>();
        Dictionary<string, List<SpotElevationPoint>> dictionary2 = new Dictionary<string, List<SpotElevationPoint>>();
        Dictionary<string, List<ElementId>> dictionary3 = new Dictionary<string, List<ElementId>>();
        foreach (string str in list5)
        {
          string levelName = str;
          List<SpotElevationPoint> list6 = source3.Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Work Plane").Equals(levelName))).Select<FamilyInstance, SpotElevationPoint>((Func<FamilyInstance, SpotElevationPoint>) (s => new SpotElevationPoint(s))).ToList<SpotElevationPoint>();
          dictionary2.Add(levelName, list6);
          Dictionary<XYZ, ElementId> dictionary4 = new Dictionary<XYZ, ElementId>();
          List<ElementId> elementIdList2 = new List<ElementId>();
          foreach (SpotElevationPoint spotElevationPoint in list6)
          {
            bool flag = false;
            foreach (XYZ key in dictionary4.Keys)
            {
              if (key.IsAlmostEqualTo(spotElevationPoint.planePoint))
              {
                flag = true;
                if (!elementIdList2.Contains(dictionary4[key]))
                  elementIdList2.Add(dictionary4[key]);
              }
            }
            if (!flag)
              dictionary4.Add(spotElevationPoint.planePoint, spotElevationPoint.elemId);
            else
              elementIdList2.Add(spotElevationPoint.elemId);
          }
          if (elementIdList2.Count > 0)
            dictionary3.Add(levelName, elementIdList2);
        }
        if (dictionary3.Keys.Count > 0)
        {
          TaskDialog taskDialog2 = new TaskDialog("Auto warping");
          taskDialog2.MainContent = "One or more spot elevations were overlapping in their respective level(s) and prevented auto warping from running properly. The operation will be cancelled. Expand for details.";
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string key in dictionary3.Keys)
          {
            if (!key.ToUpper().Contains("LEVEL"))
              stringBuilder.AppendLine("Level: " + key);
            else
              stringBuilder.AppendLine(key);
            string str4 = "Spot Elevations: {";
            foreach (ElementId elementId in dictionary3[key])
              str4 = $"{str4}{elementId.ToString()}, ";
            string str5 = str4.Substring(0, str4.Length - 2) + "}";
            stringBuilder.AppendLine(str5);
          }
          taskDialog2.ExpandedContent = stringBuilder.ToString();
          taskDialog2.Show();
          return (Result) 1;
        }
        List<ElementId> warpedDS = new List<ElementId>();
        Dictionary<string, List<WarpingEdgeLoop>> dictionary5 = new Dictionary<string, List<WarpingEdgeLoop>>();
        Dictionary<string, List<List<WarpingEdge>>> dictionary6 = new Dictionary<string, List<List<WarpingEdge>>>();
        ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_SpecialityEquipment,
          BuiltInCategory.OST_GenericModel
        });
        List<FamilyInstance> list7 = new FilteredElementCollector(revitDoc).WherePasses((ElementFilter) filter).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => !warpedDS.Contains(famInst.Id))).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => famInst.Host == null)).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => WarpingEdgeLoop.PassesWarpableCriteria(famInst, false))).ToList<FamilyInstance>();
        List<ElementId> elementIdList3 = new List<ElementId>();
        foreach (string str in list5)
        {
          string levelName = str;
          List<SpotElevationPoint> spotElevationPointList = dictionary2[levelName];
          Transform transform = spotElevationPointList.First<SpotElevationPoint>().transform;
          List<FamilyInstance> list8 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Select<FamilyInstance, FamilyInstance>((Func<FamilyInstance, FamilyInstance>) (famInst => famInst.SuperComponent != null ? famInst.SuperComponent as FamilyInstance : famInst)).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Work Plane").Equals(levelName) || Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, BuiltInParameter.SKETCH_PLANE_PARAM).Equals(levelName))).Where<FamilyInstance>((Func<FamilyInstance, bool>) (faminst => Oracle.ContainsWarpableConstructionProduct(faminst))).ToList<FamilyInstance>();
          Dictionary<FamilyInstance, XYZ[]> sfDict = new Dictionary<FamilyInstance, XYZ[]>();
          foreach (FamilyInstance structFramingElem in list8)
          {
            if (!sfDict.Keys.Contains<FamilyInstance>(structFramingElem))
            {
              FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, structFramingElem);
              XYZ[] endpointsForSf = this.GetEndpointsForSF(flatElement, transform);
              if (endpointsForSf != null)
                sfDict.Add(flatElement, endpointsForSf);
              else if (!elementIdList3.Contains(structFramingElem.Id))
                elementIdList3.Add(structFramingElem.Id);
            }
          }
          PathFinder pathFinder = new PathFinder(sfDict, spotElevationPointList, revitDoc, transform, out nonLinearLoops, out collisions);
          if (collisions.Count > 0)
          {
            dictionary6.Add(levelName, collisions);
          }
          else
          {
            if (nonLinearLoops.Count > 0)
              dictionary5.Add(levelName, nonLinearLoops);
            QATimer qaTimer1 = new QATimer("OverallOperation");
            App.DialogSwitches.SuspendEntourageWarning = true;
            if (pathFinder.EdgeLoops.Count == 0)
            {
              source4.Add(levelName);
            }
            else
            {
              pathFinder.CheckOverlapLoops();
              List<FamilyInstance> list9 = list7.Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString(famInst.GetTopLevelElement(), "Work Plane").Equals(levelName) || Utils.ElementUtils.Parameters.GetParameterAsString(famInst.GetTopLevelElement(), "Host").Equals(levelName))).ToList<FamilyInstance>();
              foreach (WarpingEdgeLoop edgeLoop in pathFinder.EdgeLoops)
              {
                QATimer qaTimer2 = new QATimer("WarpMembers");
                List<FamilyInstance> familyInstanceList = new List<FamilyInstance>();
                List<FamilyInstance> errorMembers = new List<FamilyInstance>();
                XYZ loopPlanDirection;
                if (!edgeLoop.SetMemberElevations(revitDoc, activeUiDocument, levelName, transform, out loopPlanDirection, out errorMembers))
                {
                  TaskDialog taskDialog3 = new TaskDialog("Auto Warping Error");
                  taskDialog3.MainContent = $"Couldn't set elevation for members on {levelName}.";
                  taskDialog3.ExpandedContent = "Problem members: ";
                  foreach (Element element in errorMembers)
                  {
                    taskDialog3.ExpandedContent += element.Id.ToString();
                    taskDialog3.ExpandedContent += ", ";
                  }
                  taskDialog3.Show();
                  return (Result) 1;
                }
                edgeLoop.CreateDirectShapesOfEmbeds(levelName, entourages, loopPlanDirection, warpedDS, list9);
                qaTimer2.Stop();
                new QATimer("DirectShapeCreation").Stop();
              }
              if (pathFinder.UnusedSpotElevations.Count != 0)
              {
                foreach (SpotElevationPoint unusedSpotElevation in pathFinder.UnusedSpotElevations)
                {
                  if (!dictionary1.Keys.Contains<string>(levelName) || !dictionary1[levelName].Contains(unusedSpotElevation.elemId))
                  {
                    if (dictionary1.Keys.Contains<string>(levelName))
                      dictionary1[levelName].Add(unusedSpotElevation.elemId);
                    else
                      dictionary1.Add(levelName, new List<ElementId>()
                      {
                        unusedSpotElevation.elemId
                      });
                  }
                }
              }
              qaTimer1.Flush();
            }
          }
        }
        int num2 = (int) transaction.Commit();
        if (transaction.GetStatus() != TransactionStatus.Committed)
          return (Result) 1;
        if (elementIdList3.Count > 0)
        {
          TaskDialog taskDialog4 = new TaskDialog("EDGE Error");
          taskDialog4.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog4.AllowCancellation = false;
          taskDialog4.MainInstruction = "One or more structural framing members had geometry issues that prevented them from being warped.";
          string str = "Problem elements: ";
          foreach (ElementId elementId in elementIdList3)
          {
            str += elementId.ToString();
            str += ", ";
          }
          taskDialog4.ExpandedContent = str.Substring(0, str.Length - 2);
          taskDialog4.Show();
        }
        if (dictionary6.Keys.Count > 0)
        {
          TaskDialog taskDialog5 = new TaskDialog("EDGE Error");
          taskDialog5.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog5.AllowCancellation = false;
          taskDialog5.MainInstruction = "Colliding Edges";
          taskDialog5.MainContent = "Warping Edges within the model intersect and could not be resolved. Expand for details. Cancelling.";
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string key in dictionary6.Keys)
          {
            stringBuilder.AppendLine(key + ":");
            stringBuilder.AppendLine();
            foreach (List<WarpingEdge> warpingEdgeList in dictionary6[key])
            {
              stringBuilder.AppendLine("Edge collision: ");
              string str6 = "";
              foreach (WarpingEdge warpingEdge in warpingEdgeList)
                str6 = $"{str6}{{{warpingEdge.StartPoint.elemId.ToString()},{warpingEdge.EndPoint.elemId.ToString()}}}, ";
              string str7 = str6.Remove(str6.Length - 2);
              stringBuilder.AppendLine(str7);
              stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine();
          }
          taskDialog5.ExpandedContent = stringBuilder.ToString();
          taskDialog5.Show();
          return (Result) 1;
        }
        if (dictionary5.Keys.Count > 0)
        {
          TaskDialog taskDialog6 = new TaskDialog("Auto Warping");
          taskDialog6.MainContent = "Unable to calculate warp for some warping loops";
          taskDialog6.MainInstruction = "The following warping loops were not warped due to ambiguous or non-linear warping elevations.";
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string key in dictionary5.Keys)
          {
            stringBuilder.AppendLine(key + ":");
            stringBuilder.AppendLine();
            foreach (WarpingEdgeLoop warpingEdgeLoop in dictionary5[key])
            {
              List<ElementId> elementIdList4 = new List<ElementId>();
              foreach (WarpingEdge edge in warpingEdgeLoop.Edges)
              {
                if (!elementIdList4.Contains(edge.StartPoint.elemId))
                  elementIdList4.Add(edge.StartPoint.elemId);
                if (!elementIdList4.Contains(edge.EndPoint.elemId))
                  elementIdList4.Add(edge.EndPoint.elemId);
              }
              string str8 = "Loop: {";
              foreach (ElementId elementId in elementIdList4)
                str8 = $"{str8} {elementId.ToString()},";
              string str9 = str8.Remove(str8.Length - 1, 1) + " }";
              stringBuilder.AppendLine(str9);
            }
          }
          taskDialog6.ExpandedContent = stringBuilder.ToString();
          taskDialog6.Show();
        }
        if (source4.Count<string>() != 0)
        {
          string str10 = "";
          foreach (string str11 in source4)
            str10 = $"{str10}{str11}, ";
          TaskDialog.Show("Auto Warping", $"No valid Warping Edge Loops were found on level(s): {Environment.NewLine}{str10.Substring(0, str10.Length - 2)}");
        }
        int num3 = 0;
        if (dictionary1.Keys.Count > 0)
        {
          StringBuilder stringBuilder = new StringBuilder();
          TaskDialog taskDialog7 = new TaskDialog("Auto Warping");
          taskDialog7.MainContent = "Some levels had unusable Spot Elevations. Expand for details.";
          foreach (string key in dictionary1.Keys)
          {
            if (!key.ToUpper().Contains("LEVEL"))
              stringBuilder.AppendLine("Level: " + key);
            else
              stringBuilder.AppendLine(key);
            string str12 = "Spot Elevations: {";
            foreach (ElementId elementId in dictionary1[key])
            {
              ++num3;
              str12 = $"{str12}{elementId.ToString()}, ";
            }
            string str13 = str12.Substring(0, str12.Length - 2) + "}";
            stringBuilder.AppendLine(str13);
          }
          taskDialog7.ExpandedContent = stringBuilder.ToString();
          taskDialog7.Show();
        }
        if (num3 < source3.Count<Element>())
        {
          if (source4.Count != list5.Count)
            goto label_205;
        }
        TaskDialog.Show("Auto Warping", "Auto Warping operation completed. No elements were warped.");
        return (Result) 0;
      }
    }
    catch (Exception ex)
    {
      if (ex.ToString().ToUpper().Contains("WS"))
        return (Result) 1;
      if (ex.Message.ToString().Contains("The referenced object is not valid, possibly because it has been deleted from the database, or its creation was undone."))
      {
        new TaskDialog("EDGE Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "Family Error",
          MainContent = "Certain families could not be modified by Auto Warping. Please check the families and try again."
        }.Show();
        return (Result) 1;
      }
      if (ex.Message.ToString().ToUpper().Contains("OPEN EDITING TRANSACTION"))
      {
        new TaskDialog("EDGE Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "Ownership Error",
          MainContent = "Current user does not have ownership. Please coordinate with your team to resolve the issue."
        }.Show();
        return (Result) 1;
      }
      if (ex.Message.ToString().ToUpper().Contains("HOST_GUID"))
      {
        new TaskDialog("EDGE Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "No Host Guid",
          MainContent = "Entourage does not have a host guid. Please run Project Shared Parameters before running Auto Warping to set the Host Guids for Entourages."
        }.Show();
        return (Result) 1;
      }
      if (ex is OverlappingEdgeException || ex.Message.ToString().ToUpper().Contains("READ ONLY"))
        return (Result) 1;
      if (ex.Message.ToString().ToUpper().Contains("LOOP"))
      {
        new TaskDialog("EDGE Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "Invalid Edge",
          MainContent = "A Warping Edge Loop is contained within another Warping Edge Loop. Cancelling."
        }.Show();
        return (Result) 1;
      }
      new TaskDialog("EDGE Error")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        AllowCancellation = false,
        MainInstruction = "Unexpected Exception Running Auto-Warp Routine",
        MainContent = $"EDGE encountered an unexpected exception: {ex.GetType().Name} please contact EDGE support if necessary.  See expanded content for exception detail",
        ExpandedContent = ex.Message
      }.Show();
      return (Result) 1;
    }
    finally
    {
      App.DialogSwitches.SuspendEntourageWarning = false;
    }
label_205:
    TaskDialog.Show("Auto Warping", "Auto Warping operation completed.");
    return (Result) 0;
  }

  private XYZ ProjectToSlabTop(XYZ point, Floor floor)
  {
    return floor.GetVerticalProjectionPoint(point, FloorFace.Top);
  }

  private Dictionary<string, List<string>> CollectEntourages(Autodesk.Revit.DB.Document revitDoc)
  {
    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Entourage);
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (Element elem in elementCollector)
    {
      if (elem != null)
      {
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID");
        if (parameterAsString != null)
        {
          if (revitDoc.GetElement(parameterAsString) == null)
            elementIdList.Add(elem.Id);
          if (dictionary.Keys.Contains<string>(parameterAsString))
            dictionary[parameterAsString].Add(elem.UniqueId);
          else
            dictionary.Add(parameterAsString, new List<string>()
            {
              elem.UniqueId
            });
        }
      }
    }
    revitDoc.Delete((ICollection<ElementId>) elementIdList);
    return dictionary;
  }
}
