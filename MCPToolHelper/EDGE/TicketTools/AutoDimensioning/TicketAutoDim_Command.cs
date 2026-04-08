// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.TicketAutoDim_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SelectionUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

[Transaction(TransactionMode.Manual)]
public class TicketAutoDim_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Application application = commandData.Application.Application;
    if (document.ActiveView.AssociatedAssemblyInstanceId == ElementId.InvalidElementId)
    {
      TaskDialog.Show("Error", "Current View is not an Assembly View");
      return (Result) 1;
    }
    if (document.ActiveView.ViewType != ViewType.Detail && document.ActiveView.ViewType != ViewType.Section)
    {
      TaskDialog.Show("Error", "Current View is not an Acceptable Assembly View for Dimensioning");
      return (Result) 1;
    }
    AssemblyInstance element1 = document.GetElement(document.ActiveView.AssociatedAssemblyInstanceId) as AssemblyInstance;
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    IEnumerable<Element> source = element1.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(element1.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320));
    if (source.Count<Element>() == 1)
    {
      if (Solids.GetInstanceSolids((Element) (source.First<Element>() as FamilyInstance)).Count<Solid>() == 0)
        flag3 = true;
    }
    else if (source.Count<Element>() == 0)
      flag2 = true;
    else
      flag1 = true;
    if (flag1)
    {
      string name = element1.Name;
      new TaskDialog("Ticket Auto Dimension Error")
      {
        MainContent = $"The selected assembly, {name} has more than one structural framing element. Auto-Dimension operation will be cancelled."
      }.Show();
      return (Result) 1;
    }
    if (flag2)
    {
      string name = element1.Name;
      new TaskDialog("Ticket Auto Dimension Error")
      {
        MainContent = $"The selected assembly, {name} does not contain a structural framing element. Auto-Dimension operation will be cancelled."
      }.Show();
      return (Result) 1;
    }
    if (flag3)
    {
      string name = element1.Name;
      new TaskDialog("Ticket Auto Dimension Error")
      {
        MainContent = $"The selected assembly, {name} does not contain any solids in its structural framing element. Auto-Dimension operation will be cancelled."
      }.Show();
      return (Result) 1;
    }
    DimPlacementCalculator placementCalculator = new DimPlacementCalculator(element1.GetStructuralFramingElement() as FamilyInstance, document.ActiveView as ViewSection);
    bool flag4 = true;
    while (flag4)
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        Reference reference1;
        Element element2;
        bool flag5;
        do
        {
          reference1 = activeUiDocument.Selection.PickObject((ObjectType) 1, "Pick element to dimension");
          element2 = document.GetElement(reference1);
          if (!(element2 is FamilyInstance) || !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(element2 as FamilyInstance))
          {
            new TaskDialog("EDGE^R")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "Selected element for dimensioning does not include EdgeDimLines",
              MainContent = "Please update family to include EdgeDimLines or select a family instance which already contains these dimensioning lines"
            }.Show();
            flag5 = true;
          }
          else
            flag5 = false;
        }
        while (flag5);
        new List<Reference>() { reference1 };
        IList<Reference> referenceList = (IList<Reference>) new List<Reference>();
        if (element2 is FamilyInstance)
        {
          FamilyInstance elem = element2 as FamilyInstance;
          string controlMark = elem.GetControlMark();
          if (!string.IsNullOrWhiteSpace(controlMark))
          {
            foreach (ElementId elementId in (IEnumerable<ElementId>) new FilteredElementCollector(document, document.ActiveView.Id).OfClass(typeof (FamilyInstance)).ToElementIds())
            {
              if (element1.GetMemberIds().Contains(elementId) && document.GetElement(elementId).GetControlMark().Equals(controlMark))
                referenceList.Add(new Reference(document.GetElement(elementId)));
            }
          }
          else
          {
            string str = elem.Symbol != null ? elem.Symbol.Name + elem.Symbol.FamilyName : "";
            foreach (ElementId memberId in (IEnumerable<ElementId>) element1.GetMemberIds())
            {
              if (document.GetElement(memberId) is FamilyInstance element3 && element3.Symbol != null && (element3.Symbol.Name + element3.Symbol.FamilyName).Equals(str))
                referenceList.Add(new Reference(document.GetElement(memberId)));
            }
          }
        }
        bool flag6 = false;
        if (referenceList.Count > 1)
          flag6 = new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Preselect matching control marks?",
            MainContent = "It looks like there are more than a few of these, do you want EDGE^R to preselect others for you?",
            CommonButtons = ((TaskDialogCommonButtons) 6)
          }.Show() == 6;
        if (!flag6)
        {
          referenceList.Clear();
          referenceList.Add(reference1);
        }
        List<Reference> list = activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new SameControlMarkFilter(document.GetElement(reference1.ElementId)), "Pick additional Elements To Dimension", referenceList).ToList<Reference>();
        DimensionEdge dimensionEdge = DimensionEdge.None;
        if (element2 != null)
        {
          XYZ zero = XYZ.Zero;
          bool flag7 = false;
          do
          {
            try
            {
              XYZ pickedPoint = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 0, "Pick placement side");
              dimensionEdge = placementCalculator.GetDimPlacementEdgeBasedOnPickedPoint(pickedPoint);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
              flag7 = true;
            }
          }
          while (dimensionEdge == DimensionEdge.None && !flag7);
          if (flag7)
            return (Result) 0;
          bool flag8 = false;
          List<string> stringList = new List<string>();
          foreach (Reference reference2 in list)
          {
            ElementId elementId = reference2.ElementId;
            Element element4 = document.GetElement(elementId);
            if (element4 is FamilyInstance && HiddenGeomReferenceCalculator.GetDimLineReference(element4 as FamilyInstance, dimensionEdge, (View) (document.ActiveView as ViewSection)).Count < 1)
            {
              flag8 = true;
              break;
            }
          }
          if (flag8)
          {
            new TaskDialog("EDGE^R")
            {
              MainInstruction = "One or more selected elements did not contain EDGE dim lines that could be used in the chosen view/side.",
              MainContent = "Try another angle or view or add usable EDGE Dim lines."
            }.Show();
          }
          else
          {
            using (Transaction transaction = new Transaction(document, "Ticket Auto-Dimension"))
            {
              int num1 = (int) transaction.Start();
              LocationInFormAnalyzer locationAnalyzer = new LocationInFormAnalyzer(element1, 1.0);
              List<AutoTicketAppendStringParameterData> appendStringData1;
              List<AutoTicketSettingsTools> ticketSettingsToolsList = AutoTicketSettingsReader.ReaderforAutoTicketSettings(document, "AUTO-DIMENSION", false, out appendStringData1);
              CalloutStyle styleDetails = (CalloutStyle) null;
              AutoTicketAppendString appendStringData2 = (AutoTicketAppendString) null;
              AutoTicketMinimumDimension minDimValue = (AutoTicketMinimumDimension) null;
              AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(document);
              if (ticketSettingsToolsList != null)
              {
                foreach (AutoTicketSettingsTools ticketSettingsTools in ticketSettingsToolsList)
                {
                  if (ticketSettingsTools is AutoTicketCalloutAndDimensionTexts)
                  {
                    AutoTicketCalloutAndDimensionTexts andDimensionTexts = ticketSettingsTools as AutoTicketCalloutAndDimensionTexts;
                    styleDetails = new CalloutStyle(document, AutoTicketEnhancementWorkflow.AutoDimension, andDimensionTexts.CalloutFamily, andDimensionTexts.OverallDimension, andDimensionTexts.GeneralDimension, andDimensionTexts.TextStyle);
                  }
                  if (ticketSettingsTools is AutoTicketAppendString)
                    appendStringData2 = ticketSettingsTools as AutoTicketAppendString;
                  if (ticketSettingsTools is AutoTicketMinimumDimension)
                    minDimValue = ticketSettingsTools as AutoTicketMinimumDimension;
                }
              }
              List<ElementId> elementIds1 = new List<ElementId>();
              List<ElementId> elementIds2 = new List<ElementId>();
              List<ElementId> elementIds3 = new List<ElementId>();
              List<ElementId> elementIds4 = new List<ElementId>();
              foreach (Reference reference3 in list)
              {
                ElementId elementId = reference3.ElementId;
                if (Utils.ElementUtils.Parameters.GetParameterAsString(document.GetElement(elementId), "MANUFACTURE_COMPONENT").Contains("EMBED"))
                {
                  string str = LocationInFormAnalyzer.ProcessAndAssignLIF(document, locationInFormValues, elementId, locationAnalyzer);
                  if (str.Equals(locationInFormValues.TIF))
                    elementIds1.Add(elementId);
                  else if (str.Equals(locationInFormValues.BIF))
                    elementIds2.Add(elementId);
                  else if (str.Equals(locationInFormValues.SIF))
                    elementIds3.Add(elementId);
                  else
                    elementIds4.Add(elementId);
                }
                else
                  elementIds4.Add(elementId);
              }
              bool eqForm = false;
              if (new TaskDialog("EDGE^R")
              {
                FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                AllowCancellation = false,
                MainInstruction = "Use Equality Formula?",
                ExpandedContent = "If you answer yes to this question, the Equality Display will be set to \"Equality Formula\"",
                CommonButtons = ((TaskDialogCommonButtons) 6)
              }.Show() == 6)
                eqForm = true;
              if (elementIds1.Count > 0)
                this.Dimension(document, document.ActiveView, elementIds1, dimensionEdge, eqForm, FormLocation.TIF, DimStyle.AutoDim, elementIds1.Count, styleDetails: styleDetails, appendStringData: appendStringData2, minDimValue: minDimValue, appendStringParameterData: appendStringData1);
              if (elementIds2.Count > 0)
                this.Dimension(document, document.ActiveView, elementIds2, dimensionEdge, eqForm, FormLocation.BIF, DimStyle.AutoDim, elementIds2.Count, styleDetails: styleDetails, appendStringData: appendStringData2, minDimValue: minDimValue, appendStringParameterData: appendStringData1);
              if (elementIds3.Count > 0)
                this.Dimension(document, document.ActiveView, elementIds3, dimensionEdge, eqForm, FormLocation.SIF, DimStyle.AutoDim, elementIds3.Count, styleDetails: styleDetails, appendStringData: appendStringData2, minDimValue: minDimValue, appendStringParameterData: appendStringData1);
              if (elementIds4.Count > 0)
                this.Dimension(document, document.ActiveView, elementIds4, dimensionEdge, eqForm, FormLocation.None, DimStyle.AutoDim, elementIds4.Count, styleDetails: styleDetails, appendStringData: appendStringData2, minDimValue: minDimValue, appendStringParameterData: appendStringData1);
              int num2 = (int) transaction.Commit();
            }
          }
        }
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        flag4 = false;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    return (Result) 0;
  }

  public Result Dimension(
    Document revitDoc,
    View view,
    List<ElementId> elementIds,
    DimensionEdge side,
    bool eqForm,
    FormLocation fLocation,
    DimStyle dimStyle,
    int count = -1,
    List<ReferencedPoint> sfPoints = null,
    string sfDimName = "",
    CalloutStyle styleDetails = null,
    AutoTicketAppendString appendStringData = null,
    AutoTicketMinimumDimension minDimValue = null,
    List<AutoTicketAppendStringParameterData> appendStringParameterData = null)
  {
    return this.Dimension(revitDoc, view, elementIds, side, eqForm, fLocation, dimStyle, out Autodesk.Revit.DB.Dimension _, count, sfPoints, sfDimName, styleDetails, appendStringData, minDimValue, appendStringParameterData);
  }

  public Result Dimension(
    Document revitDoc,
    View view,
    List<ElementId> elementIds,
    DimensionEdge side,
    bool eqForm,
    FormLocation fLocation,
    DimStyle dimStyle,
    out Autodesk.Revit.DB.Dimension newDim,
    int count = -1,
    List<ReferencedPoint> sfPoints = null,
    string sfDimName = "",
    CalloutStyle styleDetails = null,
    AutoTicketAppendString appendStringData = null,
    AutoTicketMinimumDimension minDimValue = null,
    List<AutoTicketAppendStringParameterData> appendStringParameterDatas = null)
  {
    double minDim = 0.0;
    if (minDimValue != null)
      minDim = FeetAndInchesRounding.covertFeetInchToDouble(new string[2]
      {
        string.IsNullOrEmpty(minDimValue.FeetValue) ? "" : minDimValue.FeetValue,
        string.IsNullOrEmpty(minDimValue.inchesValue) ? "" : minDimValue.inchesValue
      });
    if (minDim <= 0.0)
      minDim = 0.0;
    newDim = (Autodesk.Revit.DB.Dimension) null;
    SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
    schemaBuilder.SetSchemaName("PTAC_Dimensions");
    Schema schema = schemaBuilder.Finish();
    FamilyInstance structuralFramingElement = (revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement() as FamilyInstance;
    StructuralFramingBoundsObject framingBoundsObject = new StructuralFramingBoundsObject(structuralFramingElement, view);
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences = DimensioningGeometryUtils.GetDimensioningPointsReferences(structuralFramingElement, (View) (view as ViewSection));
    DimPlacementCalculator placementCalculator = new DimPlacementCalculator(structuralFramingElement, view as ViewSection);
    try
    {
      List<AutoTicketDimSegment> dimSegs = new List<AutoTicketDimSegment>();
      Element elem = (Element) null;
      FamilyInstance famInst = (FamilyInstance) null;
      List<Reference> referenceList = new List<Reference>();
      if (dimStyle == DimStyle.AutoDim || dimStyle == DimStyle.AutoTicketSTD)
      {
        if (elementIds.Count == 0)
          return (Result) 0;
        elem = revitDoc.GetElement(elementIds[0]);
        famInst = elem as FamilyInstance;
        foreach (ElementId elementId in elementIds)
          referenceList.Add(new Reference(revitDoc.GetElement(elementId)));
      }
      DimensionType dimType = (DimensionType) null;
      if (styleDetails != null)
        dimType = styleDetails.dimensionStyle;
      DimPlacementInfo dimPlacementInfo = placementCalculator.GetDimPlacementInfo(side, dimType);
      DimensionEdge dimensionFromEdge = dimPlacementInfo.DimensionFromEdge;
      ReferencedPoint extentReference1 = pointsReferences[side][dimensionFromEdge];
      ReferencedPoint extentReference2 = pointsReferences[side][this.GetOppDimEdge(dimensionFromEdge)];
      if (extentReference1 != null)
      {
        DimReferencesManager refManager = new DimReferencesManager(revitDoc, dimensionFromEdge, dimPlacementInfo.DimAlongDirection, placementCalculator.GetDirectionVectorForDimPlacement(side));
        refManager.AddExtentReference(extentReference1);
        refManager.AddExtentReference(extentReference2);
        if (famInst != null || dimStyle == DimStyle.AutoTicketOVERALL || dimStyle == DimStyle.AutoTicketSFCONTOUR)
        {
          switch (dimStyle)
          {
            case DimStyle.AutoTicketSFCONTOUR:
              int num1 = 0;
              foreach (ReferencedPoint sfPoint in sfPoints)
              {
                if (refManager.PointIsOverlappedSF(sfPoint).Count == 0)
                  refManager.DimReferences.Append(sfPoint.Reference);
                else
                  ++num1;
              }
              if (num1 == sfPoints.Count)
                return (Result) 1;
              goto case DimStyle.AutoTicketOVERALL;
            case DimStyle.AutoTicketOVERALL:
              if (minDim > 0.0 && (dimStyle == DimStyle.AutoTicketSTD || dimStyle == DimStyle.AutoDim))
                refManager.DimReferences = TicketAutoDim_Command.AccountForMinRefs(revitDoc, refManager, dimPlacementInfo, minDim);
              DetailLine detailLine = revitDoc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(XYZ.Zero, dimPlacementInfo.PlacementLine.Direction.CrossProduct(view.ViewDirection).Normalize().Negate())) as DetailLine;
              refManager.DimReferences.Append(detailLine.GeometryCurve.Reference);
              newDim = revitDoc.Create.NewDimension(view, dimPlacementInfo.PlacementLine, refManager.DimReferences);
              revitDoc.Delete(detailLine.Id);
              revitDoc.Regenerate();
              if (dimStyle != DimStyle.AutoDim)
              {
                if (styleDetails != null)
                {
                  if (dimStyle == DimStyle.AutoTicketOVERALL)
                  {
                    if (styleDetails.dimensionStyle != null)
                      newDim.DimensionType = styleDetails.dimensionStyle;
                  }
                  else if (styleDetails.otherDimensionStyle != null)
                    newDim.DimensionType = styleDetails.otherDimensionStyle;
                }
                else
                {
                  foreach (DimensionType dimensionType in new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType)))
                  {
                    if (dimStyle == DimStyle.AutoTicketOVERALL)
                    {
                      if (dimensionType.Name == "PTAC - TICKET (GAP TO ELEMENT)")
                      {
                        newDim.DimensionType = dimensionType;
                        break;
                      }
                    }
                    else if (dimensionType.Name == "PTAC - TICKET (FIXED TO DIM. LINE)")
                    {
                      newDim.DimensionType = dimensionType;
                      break;
                    }
                  }
                }
              }
              else if (dimStyle == DimStyle.AutoDim && styleDetails != null && styleDetails.dimensionStyle != null)
                newDim.DimensionType = styleDetails.dimensionStyle;
              Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter((Element) newDim, BuiltInParameter.LINEAR_DIM_TYPE);
              bool runningDimStyle = false;
              if (parameter1 != null && parameter1.AsValueString() == "Ordinate")
                runningDimStyle = true;
              if (dimStyle != DimStyle.AutoDim && newDim.Segments.Size > 0)
              {
                double accuracy = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
                XYZ xyz1 = new XYZ();
                switch (side)
                {
                  case DimensionEdge.Left:
                    view.RightDirection.Negate().Normalize();
                    break;
                  case DimensionEdge.Right:
                    view.RightDirection.Negate().Normalize();
                    break;
                  case DimensionEdge.Top:
                    view.UpDirection.Normalize();
                    break;
                  case DimensionEdge.Bottom:
                    view.UpDirection.Normalize();
                    break;
                }
                foreach (DimensionSegment segment in newDim.Segments)
                {
                  dimSegs.Add(new AutoTicketDimSegment(newDim, segment, dimPlacementInfo.DimAlongDirection, runningDimStyle));
                  double num2 = segment.Value.Value;
                }
                Parameter parameter2 = newDim.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE);
                Parameter parameter3 = newDim.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE);
                if (parameter2 != null && parameter3 != null)
                {
                  parameter2.AsDouble();
                  int scale1 = view.Scale;
                  parameter2.AsDouble();
                  parameter3.AsDouble();
                  int scale2 = view.Scale;
                }
                XYZ xyz2 = new XYZ();
                if (newDim.Segments.Size > 0)
                {
                  double width = 0.0;
                  double val2 = double.MaxValue;
                  foreach (DimensionSegment segment in newDim.Segments)
                  {
                    double num3 = segment.Value.Value;
                    width += num3;
                    XYZ xyz3 = segment.Origin + dimPlacementInfo.DimAlongDirection.CrossProduct(view.ViewDirection) * 0.5;
                    XYZ source = segment.Origin - dimPlacementInfo.DimAlongDirection * num3 / 2.0;
                    val2 = Math.Min(dimPlacementInfo.DimAlongDirection.DotProduct(source), val2);
                  }
                  double midProj = val2 + width / 2.0;
                  if (!runningDimStyle)
                    AutoTicketDimSegment.HandleUpsets(width, midProj, dimSegs, side, accuracy);
                }
              }
              if (side == DimensionEdge.Bottom || side == DimensionEdge.Right)
              {
                XYZ xyz = dimPlacementInfo.DimAlongDirection.CrossProduct(view.ViewDirection).Normalize();
                List<double> source1 = new List<double>();
                double val2 = xyz.DotProduct(dimPlacementInfo.ExtremePoint[ExtremePointType.SFBound]);
                if (dimPlacementInfo.ExtremePoint.ContainsKey(ExtremePointType.Bound))
                  val2 = Math.Max(xyz.DotProduct(dimPlacementInfo.ExtremePoint[ExtremePointType.Bound]), val2);
                double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(newDim, view);
                double flatTextBound = DimPlacementCalculator.GetFlatTextBound(newDim, view);
                XYZ source2;
                XYZ source3;
                if (newDim.Segments != null && newDim.NumberOfSegments > 0)
                {
                  source2 = newDim.Segments.get_Item(0).Origin + xyz.Negate() * textBoundForDim;
                  source3 = newDim.Segments.get_Item(0).Origin + xyz.Negate() * flatTextBound;
                }
                else
                {
                  source2 = newDim.Origin + xyz.Negate() * textBoundForDim;
                  source3 = newDim.Origin + xyz.Negate() * flatTextBound;
                }
                double num4 = xyz.DotProduct(source2);
                source1.Add(num4);
                double num5 = xyz.DotProduct(source3);
                source1.Add(num5);
                source1.Sort();
                if (source1.First<double>() != val2)
                {
                  XYZ translation = xyz * Math.Abs(source1.Last<double>() - source1.First<double>());
                  ElementTransformUtils.MoveElement(revitDoc, newDim.Id, translation);
                  dimPlacementInfo.TextPlacementPoint += translation;
                  if (!runningDimStyle)
                  {
                    foreach (AutoTicketDimSegment ticketDimSegment in dimSegs)
                    {
                      if (ticketDimSegment.hasLeader)
                        ticketDimSegment.setTextPosition(ticketDimSegment.position + translation);
                      else
                        ticketDimSegment.setTextPosition(ticketDimSegment.segment.TextPosition);
                    }
                  }
                }
              }
              ElementId defaultElementTypeId = revitDoc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
              TextNoteOptions options = new TextNoteOptions();
              options.Rotation = dimPlacementInfo.TextAngle;
              options.TypeId = defaultElementTypeId;
              TextNote note;
              switch (dimStyle)
              {
                case DimStyle.AutoTicketSFCONTOUR:
                  if (string.IsNullOrEmpty(sfDimName))
                    return (Result) 1;
                  note = TextNote.Create(revitDoc, view.Id, dimPlacementInfo.TextPlacementPoint, sfDimName, options);
                  break;
                case DimStyle.AutoTicketOVERALL:
                  if (string.IsNullOrEmpty(sfDimName))
                    return (Result) 1;
                  note = TextNote.Create(revitDoc, view.Id, dimPlacementInfo.TextPlacementPoint, sfDimName, options);
                  break;
                default:
                  string text;
                  if (appendStringData != null)
                  {
                    if (appendStringParameterDatas == null || appendStringParameterDatas.Count == 0)
                      appendStringParameterDatas = AutoTicketSettingsReader.returnDefaultParameters();
                    text = new AppendStringEvaluate(appendStringData.AppendStringValue, elem, famInst, count, appendStringParameterDatas).resultant;
                    if (string.IsNullOrEmpty(text))
                      return (Result) 1;
                  }
                  else
                  {
                    string str1 = !string.IsNullOrWhiteSpace(elem.GetIdentityDescriptionShort()) ? elem.GetIdentityDescriptionShort() : (famInst != null ? famInst.Symbol.FamilyName : "");
                    string str2 = Utils.ElementUtils.Parameters.GetParameterAsString(elem, "LOCATION_IN_FORM");
                    if (!string.IsNullOrWhiteSpace(str2))
                      str2 = $" ({str2})";
                    text = $"({count.ToString()}) {str1}{str2}";
                  }
                  note = TextNote.Create(revitDoc, view.Id, dimPlacementInfo.TextPlacementPoint, text, options);
                  break;
              }
              revitDoc.Regenerate();
              if (dimStyle != DimStyle.AutoDim)
              {
                if (styleDetails != null)
                {
                  if (styleDetails.textNoteStyle != null)
                    note.TextNoteType = styleDetails.textNoteStyle;
                }
                else
                {
                  foreach (TextNoteType textNoteType in new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType)))
                  {
                    if (textNoteType.Name.Equals("PTAC - TICKET TEXT"))
                      note.TextNoteType = textNoteType;
                  }
                }
              }
              else if (styleDetails != null && styleDetails.textNoteStyle != null)
                note.TextNoteType = styleDetails.textNoteStyle;
              note.Coord = placementCalculator.BumpNote(note, side);
              List<List<AutoTicketDimSegment>> source4 = new List<List<AutoTicketDimSegment>>();
              List<AutoTicketDimSegment> source5 = new List<AutoTicketDimSegment>();
              if (dimSegs.Count > 0)
              {
                string str = (string) null;
                foreach (AutoTicketDimSegment ticketDimSegment in dimSegs)
                {
                  double accuracy = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
                  if (str == null)
                    source5.Add(ticketDimSegment);
                  else if (ticketDimSegment.segment.ValueString != str)
                  {
                    source4.Add(source5.ToList<AutoTicketDimSegment>());
                    source5.Clear();
                    source5.Add(ticketDimSegment);
                  }
                  else
                    source5.Add(ticketDimSegment);
                  str = ticketDimSegment.segment.ValueString;
                }
                source4.Add(source5);
              }
              List<AutoTicketDimSegment> source6 = new List<AutoTicketDimSegment>();
              foreach (List<AutoTicketDimSegment> source7 in source4.Where<List<AutoTicketDimSegment>>((Func<List<AutoTicketDimSegment>, bool>) (grp => grp.Count == 1)))
                source6.Add(source7.First<AutoTicketDimSegment>());
              source4.RemoveAll((Predicate<List<AutoTicketDimSegment>>) (grp => grp.Count == 1));
              if (!source6.Any<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => seg.hasLeader)) && source4.Count<List<AutoTicketDimSegment>>() > 0)
              {
                bool flag = styleDetails.UseEQ;
                if (flag)
                {
                  foreach (List<AutoTicketDimSegment> source8 in source4)
                  {
                    string str3 = "";
                    double num6 = source8.First<AutoTicketDimSegment>().value * (double) source8.Count;
                    if (newDim.DimensionType.CanHaveEqualityFormula())
                    {
                      foreach (DimensionEqualityLabelFormatting equalityLabelFormatting in (IEnumerable<DimensionEqualityLabelFormatting>) newDim.DimensionType.GetEqualityFormula())
                      {
                        string str4 = equalityLabelFormatting.Prefix.PadRight(equalityLabelFormatting.LeadingSpaces);
                        switch (equalityLabelFormatting.LabelType)
                        {
                          case LabelType.NumberOfSegments:
                            int count1 = source8.Count;
                            str4 += count1.ToString();
                            break;
                          case LabelType.LengthOfSegment:
                            str4 += source8.First<AutoTicketDimSegment>().segment.ValueString;
                            break;
                          case LabelType.TotalLength:
                            FormatOptions unitsFormatOptions = newDim.DimensionType.GetUnitsFormatOptions();
                            FormatValueOptions formatValueOptions = new FormatValueOptions();
                            formatValueOptions.SetFormatOptions(unitsFormatOptions);
                            string str5 = UnitFormatUtils.Format(revitDoc.GetUnits(), newDim.DimensionType.GetSpecTypeId(), num6, false, formatValueOptions);
                            str4 += str5;
                            break;
                        }
                        string str6 = str4 + equalityLabelFormatting.Suffix;
                        str3 += str6;
                      }
                      source8.First<AutoTicketDimSegment>().segment.ValueOverride = str3;
                      double num7 = source8.First<AutoTicketDimSegment>().CalcTextWidth();
                      double num8 = (double) source8.Count<AutoTicketDimSegment>();
                      double? nullable1 = source8.First<AutoTicketDimSegment>().segment.Value;
                      double? nullable2 = nullable1.HasValue ? new double?(num8 * nullable1.GetValueOrDefault()) : new double?();
                      double valueOrDefault = nullable2.GetValueOrDefault();
                      if (num7 > valueOrDefault & nullable2.HasValue)
                      {
                        flag = false;
                        break;
                      }
                    }
                  }
                }
                dimSegs.ForEach((Action<AutoTicketDimSegment>) (seg => seg.segment.ValueOverride = ""));
                if (dimStyle != 0 & flag)
                {
                  dimSegs.ForEach((Action<AutoTicketDimSegment>) (seg => seg.segment.ResetTextPosition()));
                  newDim.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
                }
              }
              if (dimStyle == DimStyle.AutoDim & eqForm)
                newDim.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
              double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) note.TextNoteType, BuiltInParameter.TEXT_SIZE);
              XYZ xyz4 = view.ViewDirection.CrossProduct(dimPlacementInfo.DimAlongDirection).Normalize();
              double num9 = parameterAsDouble * 1.5 / 2.0 * (double) view.Scale;
              XYZ translation1 = xyz4.Normalize() * num9;
              ElementTransformUtils.MoveElement(revitDoc, note.Id, translation1);
              Entity entity = new Entity(schema);
              schema.GetField("FileName");
              entity.Set<string>("PTACDimensionGuid", note.UniqueId);
              note.SetEntity(entity);
              break;
            default:
              foreach (Reference reference in referenceList)
                refManager.AddElementReference(reference, view);
              if (refManager.DimReferences.Size < 3)
                return (Result) 1;
              goto case DimStyle.AutoTicketOVERALL;
          }
        }
      }
      return (Result) 0;
    }
    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
    {
      return (Result) -1;
    }
  }

  private static XYZ PosFromRef(Reference Ref, Document revitDoc)
  {
    XYZ xyz1;
    if (Ref.GlobalPoint != null)
    {
      xyz1 = new XYZ(Ref.GlobalPoint.X, Ref.GlobalPoint.Y, Ref.GlobalPoint.Z);
    }
    else
    {
      Element element = revitDoc.GetElement(Ref);
      XYZ xyz2 = new XYZ();
      if (element.Location is LocationPoint)
        xyz2 = (element.Location as LocationPoint).Point;
      else if (element.Location is LocationCurve)
        xyz2 = (element.Location as LocationCurve).Curve.GetEndPoint(0);
      xyz1 = new XYZ(xyz2.X, xyz2.Y, xyz2.Z);
    }
    return xyz1;
  }

  public static ReferenceArray AccountForMinRefs(
    Document revitDoc,
    DimReferencesManager refManager,
    DimPlacementInfo placementInfo,
    double minDim)
  {
    double num1 = Math.Round(minDim, 5);
    ReferenceArray referenceArray = new ReferenceArray();
    List<TicketAutoDim_Command.RefWithPosition> source = new List<TicketAutoDim_Command.RefWithPosition>();
    for (int index = 0; index < refManager.DimReferences.Size; ++index)
    {
      Reference reference = refManager.DimReferences.get_Item(index);
      XYZ xyz = (XYZ) null;
      foreach (ReferencedPoint key in refManager.referencePairs.Keys)
      {
        if (refManager.referencePairs[key] == reference)
          xyz = key.Point;
      }
      if (xyz == null)
        xyz = TicketAutoDim_Command.PosFromRef(reference, revitDoc);
      double p = xyz.DotProduct(placementInfo.PlacementLine.Direction);
      source.Add(new TicketAutoDim_Command.RefWithPosition(reference, p));
    }
    List<TicketAutoDim_Command.RefWithPosition> list1 = source.OrderBy<TicketAutoDim_Command.RefWithPosition, double>((Func<TicketAutoDim_Command.RefWithPosition, double>) (r => r.Position)).ToList<TicketAutoDim_Command.RefWithPosition>();
    double num2 = (list1.Last<TicketAutoDim_Command.RefWithPosition>().Position + list1.First<TicketAutoDim_Command.RefWithPosition>().Position) / 2.0;
    foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in list1)
      refWithPosition.idx = list1.IndexOf(refWithPosition);
    foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in list1)
    {
      TicketAutoDim_Command.RefWithPosition RWP = refWithPosition;
      RWP.equivRefs = list1.Where<TicketAutoDim_Command.RefWithPosition>((Func<TicketAutoDim_Command.RefWithPosition, bool>) (r => Math.Round(r.Position, 5) == Math.Round(RWP.Position, 5) && !r.Equals((object) RWP))).ToList<TicketAutoDim_Command.RefWithPosition>();
    }
    List<double> doubleList = new List<double>();
    bool flag = false;
    int num3 = 0;
    List<IGrouping<double, TicketAutoDim_Command.RefWithPosition>> list2 = list1.GroupBy<TicketAutoDim_Command.RefWithPosition, double>((Func<TicketAutoDim_Command.RefWithPosition, double>) (r => r.Position)).ToList<IGrouping<double, TicketAutoDim_Command.RefWithPosition>>();
    if (list2.Count <= 3)
      return refManager.DimReferences;
    for (int index = 2; index < list2.Count; ++index)
    {
      IGrouping<double, TicketAutoDim_Command.RefWithPosition> grouping1 = list2[index];
      IGrouping<double, TicketAutoDim_Command.RefWithPosition> grouping2 = list2[index - 1];
      if (Math.Round(Math.Abs(grouping2.Key - grouping1.Key), 5) < num1 && index < list2.Count - 1)
      {
        if (Math.Round(Math.Abs(list2[index + 1].Key - grouping1.Key), 5) < minDim && index + 1 < list2.Count - 1)
        {
          foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping1)
            refWithPosition.bRemove = true;
          flag = true;
          ++num3;
        }
        else if (flag)
        {
          flag = false;
          num3 = 0;
        }
        else
        {
          double num4 = num2 - grouping2.Key;
          double num5 = num2 - grouping1.Key;
          if (grouping2.Key < num2 && grouping1.Key < num2)
          {
            foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping1)
              refWithPosition.bRemove = true;
          }
          else if (grouping2.Key > num2 && grouping1.Key > num2)
          {
            foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping2)
              refWithPosition.bRemove = true;
          }
          else if (Math.Abs(num4) > Math.Abs(num5))
          {
            if (grouping2.Key < 0.0)
            {
              foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping1)
                refWithPosition.bRemove = true;
            }
            else
            {
              foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping2)
                refWithPosition.bRemove = true;
            }
          }
          else if (grouping1.Key < 0.0)
          {
            foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping1)
              refWithPosition.bRemove = true;
          }
          else
          {
            foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in (IEnumerable<TicketAutoDim_Command.RefWithPosition>) grouping2)
              refWithPosition.bRemove = true;
          }
        }
      }
      else if (flag)
      {
        flag = false;
        num3 = 0;
      }
    }
    foreach (TicketAutoDim_Command.RefWithPosition refWithPosition in list1)
    {
      if (!refWithPosition.bRemove)
        referenceArray.Append(refWithPosition.Ref);
    }
    return referenceArray;
  }

  private static bool IsEqualWithinTolerance(double factor, double val1, double val2)
  {
    return Math.Abs(val1 - val2) < factor;
  }

  private DimensionEdge GetOppDimEdge(DimensionEdge dEdge)
  {
    switch (dEdge)
    {
      case DimensionEdge.Left:
        return DimensionEdge.Right;
      case DimensionEdge.Right:
        return DimensionEdge.Left;
      case DimensionEdge.Top:
        return DimensionEdge.Bottom;
      case DimensionEdge.Bottom:
        return DimensionEdge.Top;
      default:
        return DimensionEdge.None;
    }
  }

  private class RefWithPosition
  {
    public bool bRemove;

    public double Position { get; set; }

    public Reference Ref { get; set; }

    public List<TicketAutoDim_Command.RefWithPosition> equivRefs { get; set; }

    public int idx { get; set; }

    public RefWithPosition(Reference r, double p)
    {
      this.Ref = r;
      this.Position = p;
      this.idx = -1;
      this.equivRefs = new List<TicketAutoDim_Command.RefWithPosition>();
    }
  }
}
