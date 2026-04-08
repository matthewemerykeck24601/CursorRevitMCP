// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.HardwareDetailDimension
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using EDGE.TicketTools.AutoDimensioning;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail;

internal class HardwareDetailDimension
{
  private static string masterDimLineString = "HWDBoundLine";
  private static string dimLineString = "HWDDimLine";

  public bool createDimensionsOnViews(
    Document revitDoc,
    AssemblyInstance assInst,
    Element selectedElement,
    List<View> views,
    out string errorMessage)
  {
    errorMessage = string.Empty;
    List<AutoTicketAppendStringParameterData> appendStringData1;
    List<AutoTicketSettingsTools> ticketSettingsToolsList = AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "HARDWARE DETAIL", false, out appendStringData1);
    CalloutStyle styleDetails1 = (CalloutStyle) null;
    AutoTicketMinimumDimension minDimValue1 = (AutoTicketMinimumDimension) null;
    AutoTicketAppendString appendStringData2 = (AutoTicketAppendString) null;
    foreach (AutoTicketSettingsTools ticketSettingsTools in ticketSettingsToolsList)
    {
      if (ticketSettingsTools is AutoTicketCalloutAndDimensionTexts)
      {
        AutoTicketCalloutAndDimensionTexts andDimensionTexts = ticketSettingsTools as AutoTicketCalloutAndDimensionTexts;
        styleDetails1 = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoTicketGeneration, andDimensionTexts.CalloutFamily, andDimensionTexts.OverallDimension, andDimensionTexts.GeneralDimension, andDimensionTexts.TextStyle, andDimensionTexts.UseEQ);
      }
      if (ticketSettingsTools is AutoTicketMinimumDimension)
        minDimValue1 = ticketSettingsTools as AutoTicketMinimumDimension;
      if (ticketSettingsTools is AutoTicketAppendString)
        appendStringData2 = ticketSettingsTools as AutoTicketAppendString;
    }
    List<ElementId> list = assInst.GetMemberIds().ToList<ElementId>();
    Element element1 = (Element) null;
    foreach (ElementId id in list)
    {
      Element element2 = revitDoc.GetElement(id);
      if (element2 != null && element2.Name == selectedElement.Name)
      {
        element1 = element2;
        break;
      }
    }
    if (element1 == null)
      return false;
    if (list.Contains(element1.Id))
      list.Remove(element1.Id);
    string controlMark = selectedElement.GetControlMark();
    Dictionary<string, List<ElementId>> dictionary1 = new Dictionary<string, List<ElementId>>();
    dictionary1.Add(controlMark, new List<ElementId>()
    {
      element1.Id
    });
    foreach (ElementId id in list)
    {
      string key = revitDoc.GetElement(id).GetControlMark();
      if (key == "" && revitDoc.GetElement(id) is FamilyInstance element3)
        key = element3.Symbol.FamilyName;
      if (key != "")
      {
        if (dictionary1.Keys.Contains<string>(key))
          dictionary1[key].Add(id);
        else
          dictionary1.Add(key, new List<ElementId>() { id });
      }
    }
    foreach (View view1 in views)
    {
      Dictionary<DimensionEdge, bool> dictionary2 = new Dictionary<DimensionEdge, bool>();
      dictionary2.Add(DimensionEdge.Top, false);
      dictionary2.Add(DimensionEdge.Left, false);
      dictionary2.Add(DimensionEdge.Bottom, false);
      dictionary2.Add(DimensionEdge.Right, false);
      Dictionary<DimensionEdge, List<ReferencedPoint>> dictionary3 = new Dictionary<DimensionEdge, List<ReferencedPoint>>();
      dictionary3.Add(DimensionEdge.Top, new List<ReferencedPoint>());
      dictionary3.Add(DimensionEdge.Left, new List<ReferencedPoint>());
      dictionary3.Add(DimensionEdge.Bottom, new List<ReferencedPoint>());
      dictionary3.Add(DimensionEdge.Right, new List<ReferencedPoint>());
      dictionary3.Add(DimensionEdge.None, new List<ReferencedPoint>());
      List<ElementId> viewVisible = new FilteredElementCollector(revitDoc, view1.Id).ToElementIds().ToList<ElementId>();
      foreach (string key in dictionary1.Keys)
        dictionary1[key].RemoveAll((Predicate<ElementId>) (eid => !viewVisible.Contains(eid)));
      foreach (string key1 in dictionary1.Keys)
      {
        if (!(key1 == controlMark))
        {
          List<DimensionElement> dimElem = BatchAutoDimension.ConvertToDimElem(revitDoc, dictionary1[key1], view1);
          Dictionary<DimensionEdge, List<ElementId>> quadrantElementIds = HardwareDetailDimension.DetermineHWDQuadrantElementIds(revitDoc, view1, element1 as FamilyInstance, dimElem, out Dictionary<DimensionEdge, int> _);
          foreach (DimensionEdge key2 in quadrantElementIds.Keys)
          {
            Dictionary<ElementId, List<ReferencedPoint>> refsDictionary = new Dictionary<ElementId, List<ReferencedPoint>>();
            if (quadrantElementIds[key2].Count != 0)
            {
              foreach (ElementId elementId in quadrantElementIds[key2].ToList<ElementId>())
              {
                if (revitDoc.GetElement(elementId) is FamilyInstance element4)
                {
                  List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(element4, key2, view1);
                  if (hardwareDetailDimLines.Count == 0)
                    quadrantElementIds[key2].Remove(elementId);
                  else
                    refsDictionary.Add(elementId, hardwareDetailDimLines);
                }
              }
              if (dictionary3[key2].Count == 0)
                dictionary3[key2].AddRange((IEnumerable<ReferencedPoint>) HardwareDetailDimension.GetHardwareDetailDimLines(element1 as FamilyInstance, key2, view1, true));
              if (dictionary3[key2].Count > 1)
              {
                bool flag = false;
                if (quadrantElementIds[key2].Count != 0)
                  flag = this.Dimension(revitDoc, view1, quadrantElementIds[key2], key2, element1 as FamilyInstance, false, false, dictionary3[key2], refsDictionary, styleDetails1, appendStringData2, minDimValue1, appendStringData1, dictionary1[key1].Count<ElementId>());
                if (flag)
                  dictionary2[key2] = true;
              }
            }
          }
        }
      }
      dictionary2[DimensionEdge.Top] = true;
      dictionary2[DimensionEdge.Left] = true;
      foreach (DimensionEdge key in dictionary2.Keys)
      {
        if (dictionary2[key])
        {
          Dictionary<ElementId, List<ReferencedPoint>> refsDictionary = new Dictionary<ElementId, List<ReferencedPoint>>();
          if (dictionary3[key].Count == 0)
            dictionary3[key].AddRange((IEnumerable<ReferencedPoint>) HardwareDetailDimension.GetHardwareDetailDimLines(element1 as FamilyInstance, key, view1, true));
          if (dictionary3[key].Count != 0)
          {
            foreach (ElementId elementId in dictionary1[controlMark].ToList<ElementId>())
            {
              if (revitDoc.GetElement(elementId) is FamilyInstance element5)
              {
                List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(element5, key, view1);
                if (hardwareDetailDimLines.Count == 0)
                  dictionary1[controlMark].Remove(elementId);
                else
                  refsDictionary.Add(elementId, hardwareDetailDimLines);
              }
            }
            if ((key == DimensionEdge.Left || key == DimensionEdge.Top) && dictionary1[controlMark].Count<ElementId>() > 0)
              this.Dimension(revitDoc, view1, dictionary1[controlMark], key, element1 as FamilyInstance, false, false, dictionary3[key], refsDictionary, styleDetails1, appendStringData2, minDimValue1, appendStringData1, 1);
            Document revitDoc1 = revitDoc;
            View view2 = view1;
            List<ElementId> elementIds = new List<ElementId>();
            elementIds.Add(selectedElement.Id);
            int side = (int) key;
            FamilyInstance selectedElement1 = element1 as FamilyInstance;
            List<ReferencedPoint> masterRefs = dictionary3[key];
            CalloutStyle styleDetails2 = styleDetails1;
            AutoTicketAppendString appendStringData3 = appendStringData2;
            AutoTicketMinimumDimension minDimValue2 = minDimValue1;
            List<AutoTicketAppendStringParameterData> appendStringParameterDatas = appendStringData1;
            this.Dimension(revitDoc1, view2, elementIds, (DimensionEdge) side, selectedElement1, false, true, masterRefs, (Dictionary<ElementId, List<ReferencedPoint>>) null, styleDetails2, appendStringData3, minDimValue2, appendStringParameterDatas, 1);
          }
        }
      }
    }
    return true;
  }

  private bool Dimension(
    Document revitDoc,
    View view,
    List<ElementId> elementIds,
    DimensionEdge side,
    FamilyInstance selectedElement,
    bool eqForm,
    bool overall,
    List<ReferencedPoint> masterRefs,
    Dictionary<ElementId, List<ReferencedPoint>> refsDictionary,
    CalloutStyle styleDetails = null,
    AutoTicketAppendString appendStringData = null,
    AutoTicketMinimumDimension minDimValue = null,
    List<AutoTicketAppendStringParameterData> appendStringParameterDatas = null,
    int count = -1)
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
    SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
    schemaBuilder.SetSchemaName("PTAC_Dimensions");
    Schema schema = schemaBuilder.Finish();
    try
    {
      AssemblyInstance element1 = revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance;
      DimPlacementCalculator placementCalculator = new DimPlacementCalculator(selectedElement, view as ViewSection);
      List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
      Element element2 = (Element) null;
      if (!overall)
      {
        if (elementIds.Count == 0)
          return true;
        element2 = revitDoc.GetElement(elementIds[0]);
        foreach (ElementId elementId in elementIds)
        {
          if (refsDictionary.Keys.Contains<ElementId>(elementId))
            referencedPointList.AddRange((IEnumerable<ReferencedPoint>) refsDictionary[elementId]);
        }
        if (referencedPointList.Count == 0)
          return true;
      }
      DimensionType dimType = (DimensionType) null;
      if (styleDetails != null)
        dimType = styleDetails.dimensionStyle;
      DimPlacementInfo placementInfo = new DimPlacementInfo();
      XYZ xyz1 = new XYZ();
      XYZ dimLineDirectionVector = side == DimensionEdge.Top || side == DimensionEdge.Bottom ? view.RightDirection : view.UpDirection;
      placementInfo.DimensionFromEdge = side == DimensionEdge.Top || side == DimensionEdge.Bottom ? DimensionEdge.Left : DimensionEdge.Bottom;
      placementInfo.DimAlongDirection = dimLineDirectionVector;
      placementInfo.ExtremePoint = this.GetHWDExtremeDimLocationPoint(side, view, selectedElement, element1);
      placementInfo.PlacementLine = this.GetHWDNewDimLine(side, dimLineDirectionVector, placementInfo.ExtremePoint, dimType, view, revitDoc);
      XYZ source1 = placementInfo.PlacementLine.Direction.Normalize();
      if (side == DimensionEdge.Bottom || side == DimensionEdge.Right)
        source1 = source1.Negate();
      BoundingBoxXYZ boundingBoxXyz = selectedElement.get_BoundingBox((View) null);
      double num1 = Math.Max(boundingBoxXyz.Max.DotProduct(source1), boundingBoxXyz.Min.DotProduct(source1));
      XYZ endPoint = placementInfo.PlacementLine.GetEndPoint(0);
      placementInfo.PlacementLine.GetEndPoint(1);
      double num2 = endPoint.DotProduct(source1);
      double num3 = num1 - num2;
      double num4 = 0.5;
      placementInfo.TextPlacementPoint = endPoint + source1 * (num3 + num4);
      placementInfo.TextAngle = dimLineDirectionVector.AngleTo(view.RightDirection);
      DimReferencesManager refManager = new DimReferencesManager(revitDoc, side, placementInfo.DimAlongDirection, placementCalculator.GetDirectionVectorForDimPlacement(side));
      foreach (ReferencedPoint masterRef in masterRefs)
      {
        int num5 = (int) refManager.AddReference(masterRef, true);
      }
      if (!overall)
      {
        foreach (ReferencedPoint rP in referencedPointList)
        {
          int num6 = (int) refManager.AddReference(rP, true);
        }
      }
      refManager.DimReferences = TicketAutoDim_Command.AccountForMinRefs(revitDoc, refManager, placementInfo, minDim);
      DetailLine detailLine = revitDoc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(XYZ.Zero, placementInfo.PlacementLine.Direction.CrossProduct(view.ViewDirection).Normalize().Negate())) as DetailLine;
      refManager.DimReferences.Append(detailLine.GeometryCurve.Reference);
      Autodesk.Revit.DB.Dimension dimension = revitDoc.Create.NewDimension(view, placementInfo.PlacementLine, refManager.DimReferences);
      revitDoc.Delete(detailLine.Id);
      revitDoc.Regenerate();
      if (styleDetails != null)
      {
        if (overall)
        {
          if (styleDetails.dimensionStyle != null)
            dimension.DimensionType = styleDetails.dimensionStyle;
        }
        else if (styleDetails.otherDimensionStyle != null)
          dimension.DimensionType = styleDetails.otherDimensionStyle;
      }
      else
      {
        foreach (DimensionType dimensionType in new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType)))
        {
          if (overall)
          {
            if (dimensionType.Name == "PTAC - TICKET (GAP TO ELEMENT)")
            {
              dimension.DimensionType = dimensionType;
              break;
            }
          }
          else if (dimensionType.Name == "PTAC - TICKET (FIXED TO DIM. LINE)")
          {
            dimension.DimensionType = dimensionType;
            break;
          }
        }
      }
      if (overall)
        return true;
      Parameter parameter1 = Parameters.LookupParameter((Element) dimension, BuiltInParameter.LINEAR_DIM_TYPE);
      bool runningDimStyle = false;
      if (parameter1 != null && parameter1.AsValueString() == "Ordinate")
        runningDimStyle = true;
      List<AutoTicketDimSegment> dimSegs = new List<AutoTicketDimSegment>();
      if (dimension.Segments.Size > 0 && !overall)
      {
        double accuracy = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
        XYZ xyz2 = new XYZ();
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
        foreach (DimensionSegment segment in dimension.Segments)
        {
          dimSegs.Add(new AutoTicketDimSegment(dimension, segment, placementInfo.DimAlongDirection, runningDimStyle));
          double num7 = segment.Value.Value;
        }
        Parameter parameter2 = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE);
        Parameter parameter3 = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE);
        if (parameter2 != null && parameter3 != null)
        {
          parameter2.AsDouble();
          int scale1 = view.Scale;
          parameter2.AsDouble();
          parameter3.AsDouble();
          int scale2 = view.Scale;
        }
        XYZ xyz3 = new XYZ();
        if (dimension.Segments.Size > 0)
        {
          double width = 0.0;
          double val2 = double.MaxValue;
          foreach (DimensionSegment segment in dimension.Segments)
          {
            double num8 = segment.Value.Value;
            width += num8;
            XYZ xyz4 = segment.Origin + placementInfo.DimAlongDirection.CrossProduct(view.ViewDirection) * 0.5;
            XYZ source2 = segment.Origin - placementInfo.DimAlongDirection * num8 / 2.0;
            val2 = Math.Min(placementInfo.DimAlongDirection.DotProduct(source2), val2);
          }
          double midProj = val2 + width / 2.0;
          if (!runningDimStyle)
            AutoTicketDimSegment.HandleUpsets(width, midProj, dimSegs, side, accuracy);
        }
      }
      if (side == DimensionEdge.Bottom || side == DimensionEdge.Right)
      {
        XYZ xyz5 = placementInfo.DimAlongDirection.CrossProduct(view.ViewDirection).Normalize();
        List<double> source3 = new List<double>();
        double val2 = xyz5.DotProduct(placementInfo.ExtremePoint[ExtremePointType.SFBound]);
        if (placementInfo.ExtremePoint.ContainsKey(ExtremePointType.Bound))
          val2 = Math.Max(xyz5.DotProduct(placementInfo.ExtremePoint[ExtremePointType.Bound]), val2);
        double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(dimension, view);
        double flatTextBound = DimPlacementCalculator.GetFlatTextBound(dimension, view);
        XYZ source4;
        XYZ source5;
        if (dimension.Segments != null && dimension.NumberOfSegments > 0)
        {
          source4 = dimension.Segments.get_Item(0).Origin + xyz5.Negate() * textBoundForDim;
          source5 = dimension.Segments.get_Item(0).Origin + xyz5.Negate() * flatTextBound;
        }
        else
        {
          source4 = dimension.Origin + xyz5.Negate() * textBoundForDim;
          source5 = dimension.Origin + xyz5.Negate() * flatTextBound;
        }
        double num9 = xyz5.DotProduct(source4);
        source3.Add(num9);
        double num10 = xyz5.DotProduct(source5);
        source3.Add(num10);
        source3.Sort();
        if (source3.First<double>() != val2)
        {
          XYZ translation = xyz5 * Math.Abs(source3.Last<double>() - source3.First<double>());
          ElementTransformUtils.MoveElement(revitDoc, dimension.Id, translation);
          placementInfo.TextPlacementPoint += translation;
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
      if (!overall)
      {
        TextNoteOptions options = new TextNoteOptions();
        options.Rotation = placementInfo.TextAngle;
        options.TypeId = defaultElementTypeId;
        string text;
        if (appendStringData != null)
        {
          if (appendStringParameterDatas == null || appendStringParameterDatas.Count == 0)
            appendStringParameterDatas = AutoTicketSettingsReader.returnDefaultParameters();
          text = new AppendStringEvaluate(appendStringData.AppendStringValue, element2, element2 as FamilyInstance, count, appendStringParameterDatas).resultant;
          if (string.IsNullOrEmpty(text))
            return false;
        }
        else
        {
          string str1 = !string.IsNullOrWhiteSpace(element2.GetIdentityDescriptionShort()) ? element2.GetIdentityDescriptionShort() : (selectedElement != null ? selectedElement.Symbol.FamilyName : "");
          string str2 = Parameters.GetParameterAsString(element2, "LOCATION_IN_FORM");
          if (!string.IsNullOrWhiteSpace(str2))
            str2 = $" ({str2})";
          text = $"({count.ToString()}) {str1}{str2}";
        }
        TextNote note = TextNote.Create(revitDoc, view.Id, placementInfo.TextPlacementPoint, text, options);
        revitDoc.Regenerate();
        if (styleDetails != null && styleDetails.textNoteStyle != null)
          note.TextNoteType = styleDetails.textNoteStyle;
        note.Coord = placementCalculator.BumpNote(note, side);
        List<List<AutoTicketDimSegment>> source6 = new List<List<AutoTicketDimSegment>>();
        List<AutoTicketDimSegment> source7 = new List<AutoTicketDimSegment>();
        if (dimSegs.Count > 0)
        {
          string str = (string) null;
          foreach (AutoTicketDimSegment ticketDimSegment in dimSegs)
          {
            double accuracy = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
            if (str == null)
              source7.Add(ticketDimSegment);
            else if (ticketDimSegment.segment.ValueString != str)
            {
              source6.Add(source7.ToList<AutoTicketDimSegment>());
              source7.Clear();
              source7.Add(ticketDimSegment);
            }
            else
              source7.Add(ticketDimSegment);
            str = ticketDimSegment.segment.ValueString;
          }
        }
        List<AutoTicketDimSegment> source8 = new List<AutoTicketDimSegment>();
        foreach (List<AutoTicketDimSegment> source9 in source6.Where<List<AutoTicketDimSegment>>((Func<List<AutoTicketDimSegment>, bool>) (grp => grp.Count == 1)))
          source8.Add(source9.First<AutoTicketDimSegment>());
        source6.RemoveAll((Predicate<List<AutoTicketDimSegment>>) (grp => grp.Count == 1));
        if (!source8.Any<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => seg.hasLeader)) && source6.Count<List<AutoTicketDimSegment>>() > 0)
        {
          bool flag = styleDetails.UseEQ;
          if (flag)
          {
            foreach (List<AutoTicketDimSegment> source10 in source6)
            {
              string str3 = "";
              double num11 = source10.First<AutoTicketDimSegment>().value * (double) source10.Count;
              if (dimension.DimensionType.CanHaveEqualityFormula())
              {
                foreach (DimensionEqualityLabelFormatting equalityLabelFormatting in (IEnumerable<DimensionEqualityLabelFormatting>) dimension.DimensionType.GetEqualityFormula())
                {
                  string str4 = equalityLabelFormatting.Prefix.PadRight(equalityLabelFormatting.LeadingSpaces);
                  switch (equalityLabelFormatting.LabelType)
                  {
                    case LabelType.NumberOfSegments:
                      int count1 = source10.Count;
                      str4 += count1.ToString();
                      break;
                    case LabelType.LengthOfSegment:
                      str4 += source10.First<AutoTicketDimSegment>().segment.ValueString;
                      break;
                    case LabelType.TotalLength:
                      FormatOptions unitsFormatOptions = dimension.DimensionType.GetUnitsFormatOptions();
                      FormatValueOptions formatValueOptions = new FormatValueOptions();
                      formatValueOptions.SetFormatOptions(unitsFormatOptions);
                      string str5 = UnitFormatUtils.Format(revitDoc.GetUnits(), dimension.DimensionType.GetSpecTypeId(), num11, false, formatValueOptions);
                      str4 += str5;
                      break;
                  }
                  string str6 = str4 + equalityLabelFormatting.Suffix;
                  str3 += str6;
                }
                source10.First<AutoTicketDimSegment>().segment.ValueOverride = str3;
                double num12 = source10.First<AutoTicketDimSegment>().CalcTextWidth();
                double num13 = (double) source10.Count<AutoTicketDimSegment>();
                double? nullable1 = source10.First<AutoTicketDimSegment>().segment.Value;
                double? nullable2 = nullable1.HasValue ? new double?(num13 * nullable1.GetValueOrDefault()) : new double?();
                double valueOrDefault = nullable2.GetValueOrDefault();
                if (num12 > valueOrDefault & nullable2.HasValue)
                {
                  flag = false;
                  break;
                }
              }
            }
          }
          dimSegs.ForEach((Action<AutoTicketDimSegment>) (seg => seg.segment.ValueOverride = ""));
          if (flag)
          {
            dimSegs.ForEach((Action<AutoTicketDimSegment>) (seg => seg.segment.ResetTextPosition()));
            dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
          }
        }
        if (eqForm)
          dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
        double parameterAsDouble = Parameters.GetParameterAsDouble((Element) note.TextNoteType, BuiltInParameter.TEXT_SIZE);
        XYZ xyz6 = view.ViewDirection.CrossProduct(placementInfo.DimAlongDirection).Normalize();
        double num14 = parameterAsDouble * 1.5 / 2.0 * (double) view.Scale;
        XYZ translation = xyz6.Normalize() * num14;
        ElementTransformUtils.MoveElement(revitDoc, note.Id, translation);
        Entity entity = new Entity(schema);
        schema.GetField("FileName");
        entity.Set<string>("PTACDimensionGuid", note.UniqueId);
        note.SetEntity(entity);
      }
    }
    catch (Exception ex)
    {
      return false;
    }
    return true;
  }

  private static List<ReferencedPoint> GetHardwareDetailDimLines(
    FamilyInstance inst,
    DimensionEdge dimFromSide,
    View view,
    bool Master = false)
  {
    if (view == null)
      return (List<ReferencedPoint>) null;
    XYZ dimFromDirection = dimFromSide == DimensionEdge.Top || dimFromSide == DimensionEdge.Bottom ? view.UpDirection : view.RightDirection;
    string empty = string.Empty;
    string str = !Master ? HardwareDetailDimension.dimLineString : HardwareDetailDimension.masterDimLineString;
    if (inst != null)
    {
      List<ReferencedPoint> hardwareDetailDimLines = new List<ReferencedPoint>();
      List<ReferencedPoint> collection = new List<ReferencedPoint>();
      Document document = inst.Document;
      Options options = document.Application.Create.NewGeometryOptions();
      if (options != null)
      {
        options.ComputeReferences = true;
        options.DetailLevel = document.ActiveView.DetailLevel;
        options.IncludeNonVisibleObjects = true;
      }
      GeometryElement source = inst.get_Geometry(options);
      GeometryInstance geometryInstance = source.First<GeometryObject>() as GeometryInstance;
      if ((GeometryObject) geometryInstance == (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject in source)
        {
          if (geometryObject is GeometryInstance)
          {
            geometryInstance = geometryObject as GeometryInstance;
            break;
          }
        }
      }
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        GeometryElement symbolGeometry = geometryInstance.GetSymbolGeometry();
        if ((GeometryObject) symbolGeometry != (GeometryObject) null)
        {
          bool flag = false;
          foreach (GeometryObject geometryObject in symbolGeometry)
          {
            if (geometryObject is Line)
            {
              Line line = geometryObject as Line;
              ElementId graphicsStyleId = line.GraphicsStyleId;
              if (inst.Document.GetElement(graphicsStyleId).Name == str)
              {
                Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                if (HiddenGeomReferenceCalculator.AreParallel(transformed, dimFromDirection.Normalize()))
                {
                  hardwareDetailDimLines.Add(new ReferencedPoint()
                  {
                    Reference = line.Reference,
                    Point = transformed.GetEndPoint(0)
                  });
                  flag = true;
                }
                else if (HiddenGeomReferenceCalculator.LineIsParallelToDimPlane(transformed, dimFromDirection, view.ViewDirection))
                {
                  collection.Add(new ReferencedPoint()
                  {
                    Reference = line.GetEndPointReference(0),
                    Point = transformed.GetEndPoint(0)
                  });
                  flag = true;
                }
              }
            }
          }
          if (hardwareDetailDimLines.Count == 0)
          {
            foreach (GeometryObject geometryObject in symbolGeometry)
            {
              if (geometryObject is Line)
              {
                Line line = geometryObject as Line;
                ElementId graphicsStyleId = line.GraphicsStyleId;
                if (inst.Document.GetElement(graphicsStyleId).Name == str)
                {
                  Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                  if (HiddenGeomReferenceCalculator.AreParallel(transformed, view.ViewDirection.Normalize()) && !flag)
                    hardwareDetailDimLines.Add(new ReferencedPoint()
                    {
                      Reference = line.GetEndPointReference(1),
                      Point = transformed.GetEndPoint(1)
                    });
                }
              }
            }
          }
          if (hardwareDetailDimLines.Count == 0)
            hardwareDetailDimLines.AddRange((IEnumerable<ReferencedPoint>) collection);
          return hardwareDetailDimLines;
        }
      }
    }
    return (List<ReferencedPoint>) null;
  }

  private Dictionary<ExtremePointType, XYZ> GetHWDExtremeDimLocationPoint(
    DimensionEdge dimPlacementSide,
    View _view,
    FamilyInstance famInst,
    AssemblyInstance assInst)
  {
    Dictionary<ExtremePointType, XYZ> extremePoints = new Dictionary<ExtremePointType, XYZ>();
    XYZ orientationVector = new XYZ();
    orientationVector = dimPlacementSide == DimensionEdge.Top || dimPlacementSide == DimensionEdge.Bottom ? (dimPlacementSide == DimensionEdge.Top ? _view.UpDirection : _view.UpDirection.Negate()) : (dimPlacementSide == DimensionEdge.Right ? _view.RightDirection : _view.RightDirection.Negate());
    FilteredElementCollector source1 = new FilteredElementCollector(famInst.Document, _view.Id).OfClass(typeof (Autodesk.Revit.DB.Dimension));
    FilteredElementCollector source2 = new FilteredElementCollector(famInst.Document, _view.Id).OfClass(typeof (IndependentTag));
    List<XYZ> list1 = source1.Cast<Autodesk.Revit.DB.Dimension>().Select<Autodesk.Revit.DB.Dimension, XYZ>((Func<Autodesk.Revit.DB.Dimension, XYZ>) (s => s.NumberOfSegments <= 1 ? s.Origin : s.Segments.get_Item(0).Origin)).ToList<XYZ>();
    List<XYZ> source3 = new List<XYZ>();
    List<XYZ> source4 = new List<XYZ>();
    List<Autodesk.Revit.DB.Dimension> list2 = source1.Select<Element, Autodesk.Revit.DB.Dimension>((Func<Element, Autodesk.Revit.DB.Dimension>) (elem => elem as Autodesk.Revit.DB.Dimension)).ToList<Autodesk.Revit.DB.Dimension>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(_view);
    List<XYZ> source5 = new List<XYZ>();
    BoundingBoxXYZ bb1 = new BoundingBoxXYZ()
    {
      Min = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue),
      Max = new XYZ(double.MinValue, double.MinValue, double.MinValue)
    };
    foreach (ElementId id in assInst.GetMemberIds().ToList<ElementId>())
    {
      if (famInst.Document.GetElement(id) is FamilyInstance element)
      {
        bool bSymbol;
        foreach (Solid symbolSolid in Solids.GetSymbolSolids((Element) element, out bSymbol))
        {
          Solid solid = symbolSolid;
          if (bSymbol)
            solid = SolidUtils.CreateTransformed(symbolSolid, element.GetTransform());
          List<XYZ> outlinePoints;
          BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(solid, _view, out outlinePoints, bSymbol);
          bb1 = Utils.MiscUtils.MiscUtils.AdjustBBox(bb1, viewBbox);
          source5.AddRange((IEnumerable<XYZ>) outlinePoints);
        }
      }
    }
    source3.AddRange(source5.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => viewTransform.OfPoint(pt))));
    int index = 0;
    List<Autodesk.Revit.DB.Dimension> dimensionList = new List<Autodesk.Revit.DB.Dimension>();
    XYZ max = bb1.Max;
    XYZ min = bb1.Min;
    foreach (Autodesk.Revit.DB.Dimension dimension in list2)
    {
      bool flag = false;
      foreach (DimensionSegment segment in dimension.Segments)
      {
        XYZ xyz = viewTransform.Inverse.OfPoint(segment.Origin);
        switch (dimPlacementSide)
        {
          case DimensionEdge.Left:
            if (xyz.X >= min.X)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_29;
            }
            continue;
          case DimensionEdge.Right:
            if (xyz.X <= max.X)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_29;
            }
            continue;
          case DimensionEdge.Top:
            if (xyz.Y <= max.Y)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_29;
            }
            continue;
          case DimensionEdge.Bottom:
            if (xyz.Y >= min.Y)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_29;
            }
            continue;
          default:
            continue;
        }
      }
label_29:
      ++index;
      int num = flag ? 1 : 0;
    }
    foreach (Autodesk.Revit.DB.Dimension dimension in dimensionList)
      list2.Remove(dimension);
    XYZ xyz1 = orientationVector;
    if (dimPlacementSide == DimensionEdge.Bottom || dimPlacementSide == DimensionEdge.Right)
      xyz1 = orientationVector.Negate();
    foreach (Autodesk.Revit.DB.Dimension dimension in list2)
    {
      double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(dimension, _view);
      foreach (DimensionSegment segment in dimension.Segments)
      {
        source4.Add(segment.Origin + xyz1 * textBoundForDim);
        source4.Add(segment.Origin);
      }
    }
    List<XYZ> collection = new List<XYZ>();
    foreach (IndependentTag independentTag in source2.Cast<IndependentTag>())
    {
      collection.Add(independentTag.get_BoundingBox(_view).Max);
      collection.Add(independentTag.get_BoundingBox(_view).Min);
    }
    source4.AddRange((IEnumerable<XYZ>) collection);
    extremePoints.Add(ExtremePointType.SFBound, source3.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>());
    list1.RemoveAll((Predicate<XYZ>) (pt => pt.DotProduct(orientationVector) < extremePoints[ExtremePointType.SFBound].DotProduct(orientationVector)));
    source4.RemoveAll((Predicate<XYZ>) (pt => pt.DotProduct(orientationVector) < extremePoints[ExtremePointType.SFBound].DotProduct(orientationVector)));
    if (source4.Count > 0)
    {
      XYZ xyz2 = source4.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>();
      extremePoints.Add(ExtremePointType.Bound, xyz2);
    }
    if (list1.Count > 0)
    {
      XYZ xyz3 = list1.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>();
      extremePoints.Add(ExtremePointType.Line, xyz3);
    }
    return extremePoints;
  }

  private Line GetHWDNewDimLine(
    DimensionEdge dimPlacementSide,
    XYZ dimLineDirectionVector,
    Dictionary<ExtremePointType, XYZ> extremeDimensionLocationPoint,
    DimensionType dimType,
    View _view,
    Document revitDoc)
  {
    XYZ xyz1 = new XYZ();
    XYZ xyz2;
    XYZ xyz3;
    switch (dimPlacementSide)
    {
      case DimensionEdge.Right:
        xyz3 = _view.RightDirection;
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        xyz2 = dimPlacementSide == DimensionEdge.Top ? _view.UpDirection : _view.UpDirection.Negate();
        goto label_5;
      default:
        xyz3 = _view.RightDirection.Negate();
        break;
    }
    xyz2 = xyz3;
label_5:
    if (dimType == null)
    {
      ElementId defaultElementTypeId = revitDoc.GetDefaultElementTypeId(ElementTypeGroup.LinearDimensionType);
      dimType = revitDoc.GetElement(defaultElementTypeId) as DimensionType;
    }
    double num1 = Parameters.GetParameterAsDouble((Element) dimType, BuiltInParameter.DIM_STYLE_DIM_LINE_SNAP_DIST) * (double) _view.Scale;
    XYZ source = !extremeDimensionLocationPoint.ContainsKey(ExtremePointType.Line) ? extremeDimensionLocationPoint[ExtremePointType.SFBound] + xyz2.Normalize() * (num1 * 1.5) : extremeDimensionLocationPoint[ExtremePointType.Line] + xyz2.Normalize() * num1;
    if (extremeDimensionLocationPoint.ContainsKey(ExtremePointType.Bound) && xyz2.DotProduct(source) < xyz2.DotProduct(extremeDimensionLocationPoint[ExtremePointType.Bound]))
    {
      double num2 = xyz2.DotProduct(source);
      double num3 = Math.Abs(xyz2.DotProduct(extremeDimensionLocationPoint[ExtremePointType.Bound]) - num2);
      source += xyz2.Normalize() * (num3 + num1 * 0.5);
    }
    return Line.CreateBound(source - dimLineDirectionVector * 2.0, source + dimLineDirectionVector * 2.0);
  }

  public static Dictionary<DimensionEdge, List<ElementId>> DetermineHWDQuadrantElementIds(
    Document revitDoc,
    View view,
    FamilyInstance sfFamInst,
    List<DimensionElement> DimList,
    out Dictionary<DimensionEdge, int> quadCounts)
  {
    Dictionary<DimensionEdge, List<DimensionElement>> hwdQuadrant = HardwareDetailDimension.DetermineHWDQuadrant(revitDoc, view, sfFamInst, DimList, false, out quadCounts);
    Dictionary<DimensionEdge, List<ElementId>> quadrantElementIds = new Dictionary<DimensionEdge, List<ElementId>>();
    foreach (DimensionEdge key in hwdQuadrant.Keys)
      quadrantElementIds.Add(key, hwdQuadrant[key].Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).ToList<ElementId>());
    return quadrantElementIds;
  }

  public static Dictionary<DimensionEdge, List<DimensionElement>> DetermineHWDQuadrant(
    Document revitDoc,
    View view,
    FamilyInstance sfFamInst,
    List<DimensionElement> DimList,
    bool bSF,
    out Dictionary<DimensionEdge, int> quadCounts)
  {
    Dictionary<DimensionEdge, List<DimensionElement>> hwdQuadrant = new Dictionary<DimensionEdge, List<DimensionElement>>();
    List<DimensionElement> dimensionElementList1 = new List<DimensionElement>();
    List<DimensionElement> dimensionElementList2 = new List<DimensionElement>();
    List<DimensionElement> dimensionElementList3 = new List<DimensionElement>();
    List<DimensionElement> dimensionElementList4 = new List<DimensionElement>();
    BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(sfFamInst, view, out List<XYZ> _);
    double other1 = viewBbox.Min.X;
    double other2 = viewBbox.Max.X;
    double other3 = viewBbox.Min.Y;
    double other4 = viewBbox.Max.Y;
    if (other1 > other2)
    {
      double num = other2;
      other2 = other1;
      other1 = num;
    }
    if (other3 > other4)
    {
      double num = other4;
      other4 = other3;
      other3 = num;
    }
    double other5 = (other1 + other2) / 2.0;
    double other6 = (other3 + other4) / 2.0;
    DimensionEdge dimensionEdge1 = DimensionEdge.None;
    DimensionEdge dimensionEdge2 = DimensionEdge.None;
    foreach (DimensionElement dim in DimList)
    {
      double horizPos = dim.horizPos;
      double verticalPos = dim.verticalPos;
      if (horizPos.ApproximatelyEquals(other1) || dimensionEdge1 == DimensionEdge.Left)
      {
        dimensionElementList3.Add(dim);
        dimensionElementList3.AddRange((IEnumerable<DimensionElement>) dimensionElementList4);
        dimensionElementList4.Clear();
        dimensionEdge1 = DimensionEdge.Left;
      }
      else if (horizPos.ApproximatelyEquals(other2) || dimensionEdge1 == DimensionEdge.Right)
      {
        dimensionElementList4.Add(dim);
        dimensionElementList4.AddRange((IEnumerable<DimensionElement>) dimensionElementList3);
        dimensionElementList3.Clear();
        dimensionEdge1 = DimensionEdge.Right;
      }
      else if (verticalPos.ApproximatelyEquals(other3) || dimensionEdge2 == DimensionEdge.Bottom)
      {
        dimensionElementList2.Add(dim);
        dimensionElementList2.AddRange((IEnumerable<DimensionElement>) dimensionElementList1);
        dimensionElementList1.Clear();
        dimensionEdge2 = DimensionEdge.Bottom;
      }
      else if (verticalPos.ApproximatelyEquals(other4) || dimensionEdge2 == DimensionEdge.Top)
      {
        dimensionElementList1.Add(dim);
        dimensionElementList1.AddRange((IEnumerable<DimensionElement>) dimensionElementList2);
        dimensionElementList2.Clear();
        dimensionEdge2 = DimensionEdge.Top;
      }
      if (dimensionEdge2 == DimensionEdge.None)
      {
        if (verticalPos.ApproximatelyEquals(other6))
        {
          dimensionElementList1.Add(dim);
          dimensionElementList2.Add(dim);
        }
        else
          (verticalPos > other6 ? dimensionElementList1 : dimensionElementList2).Add(dim);
      }
      if (dimensionEdge1 == DimensionEdge.None)
      {
        if (horizPos.ApproximatelyEquals(other5))
        {
          dimensionElementList3.Add(dim);
          dimensionElementList4.Add(dim);
        }
        else
          (horizPos > other5 ? dimensionElementList4 : dimensionElementList3).Add(dim);
      }
    }
    DimensionEdge dimensionEdge3 = dimensionElementList1.Count<DimensionElement>() > dimensionElementList2.Count<DimensionElement>() ? DimensionEdge.Top : DimensionEdge.Bottom;
    int num1 = dimensionElementList4.Count<DimensionElement>() > dimensionElementList3.Count<DimensionElement>() ? 1 : 0;
    int num2 = 0;
    if (num1 == 0)
    {
      foreach (DimensionElement dimensionElement in dimensionElementList3)
      {
        DimensionElement dimElem = dimensionElement;
        if (dimensionElementList4.Any<DimensionElement>((Func<DimensionElement, bool>) (dE => dE.Matches(dimElem, DimensionEdge.Right))))
          ++num2;
      }
      if (num2 == dimensionElementList3.Count)
      {
        dimensionElementList3.AddRange((IEnumerable<DimensionElement>) dimensionElementList4);
        dimensionElementList4.Clear();
      }
    }
    else
    {
      foreach (DimensionElement dimensionElement in dimensionElementList4)
      {
        DimensionElement dimElem = dimensionElement;
        if (dimensionElementList3.Any<DimensionElement>((Func<DimensionElement, bool>) (dE => dE.Matches(dimElem, DimensionEdge.Right))))
          ++num2;
      }
      if (num2 == dimensionElementList4.Count)
      {
        dimensionElementList4.AddRange((IEnumerable<DimensionElement>) dimensionElementList3);
        dimensionElementList3.Clear();
      }
    }
    int num3 = 0;
    if (dimensionEdge3 == DimensionEdge.Top)
    {
      foreach (DimensionElement dimensionElement in dimensionElementList1)
      {
        DimensionElement dimElem = dimensionElement;
        if (dimensionElementList2.Any<DimensionElement>((Func<DimensionElement, bool>) (dE => dE.Matches(dimElem, DimensionEdge.Top))))
          ++num3;
      }
      if (num3 == dimensionElementList1.Count)
      {
        dimensionElementList1.AddRange((IEnumerable<DimensionElement>) dimensionElementList2);
        dimensionElementList2.Clear();
      }
    }
    else
    {
      foreach (DimensionElement dimensionElement in dimensionElementList2)
      {
        DimensionElement dimElem = dimensionElement;
        if (dimensionElementList1.Any<DimensionElement>((Func<DimensionElement, bool>) (dE => dE.Matches(dimElem, DimensionEdge.Top))))
          ++num3;
      }
      if (num3 == dimensionElementList2.Count)
      {
        dimensionElementList2.AddRange((IEnumerable<DimensionElement>) dimensionElementList1);
        dimensionElementList1.Clear();
      }
    }
    quadCounts = new Dictionary<DimensionEdge, int>();
    if (!bSF)
    {
      foreach (DimensionElement dimensionElement in dimensionElementList3.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Left, view);
          if (hardwareDetailDimLines == null || hardwareDetailDimLines.Count == 0)
            dimensionElementList3.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList4.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Right, view);
          if (hardwareDetailDimLines == null || hardwareDetailDimLines.Count == 0)
            dimensionElementList4.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList1.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Top, view);
          if (hardwareDetailDimLines == null || hardwareDetailDimLines.Count == 0)
            dimensionElementList1.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList2.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> hardwareDetailDimLines = HardwareDetailDimension.GetHardwareDetailDimLines(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Bottom, view);
          if (hardwareDetailDimLines == null || hardwareDetailDimLines.Count == 0)
            dimensionElementList2.Remove(dimensionElement);
        }
      }
    }
    hwdQuadrant.Add(DimensionEdge.Left, BatchAutoDimension.CullDimensionElements(dimensionElementList3.ToList<DimensionElement>(), DimensionEdge.Left));
    quadCounts.Add(DimensionEdge.Left, dimensionElementList3.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    hwdQuadrant.Add(DimensionEdge.Right, BatchAutoDimension.CullDimensionElements(dimensionElementList4.ToList<DimensionElement>(), DimensionEdge.Right));
    quadCounts.Add(DimensionEdge.Right, dimensionElementList4.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    hwdQuadrant.Add(DimensionEdge.Top, BatchAutoDimension.CullDimensionElements(dimensionElementList1.ToList<DimensionElement>(), DimensionEdge.Top));
    quadCounts.Add(DimensionEdge.Top, dimensionElementList1.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    hwdQuadrant.Add(DimensionEdge.Bottom, BatchAutoDimension.CullDimensionElements(dimensionElementList2.ToList<DimensionElement>(), DimensionEdge.Bottom));
    quadCounts.Add(DimensionEdge.Bottom, dimensionElementList2.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    return hwdQuadrant;
  }
}
