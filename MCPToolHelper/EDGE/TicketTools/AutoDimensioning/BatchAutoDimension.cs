// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.BatchAutoDimension
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using EDGE.TicketTools.CloneTicket;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

[Transaction(TransactionMode.Manual)]
public class BatchAutoDimension : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    return AutoTicketHandler.Execute(commandData.Application, "", false, 1, message);
  }

  public static Result DimensionView(
    Document revitDoc,
    View view,
    bool bFirst,
    Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dimensionDictionary,
    CalloutStyle autoTicketStyleSettings,
    AutoTicketAppendString appendStringData,
    AutoTicketCustomValues customValues,
    AutoTicketMinimumDimension minDimData,
    List<AutoTicketAppendStringParameterData> appendStringParameterDatas,
    bool cloneTicket = false,
    Dictionary<SortingInfoCloneTicketElement, Dictionary<DimensionEdge, bool>> cloneTicketSideDictionary = null)
  {
    Element structuralFramingElement = (revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance).GetStructuralFramingElement();
    FamilyInstance sfFamInst = structuralFramingElement as FamilyInstance;
    TicketAutoDim_Command ticketAutoDimCommand1 = new TicketAutoDim_Command();
    Dictionary<DimensionEdge, bool> dictionary1 = new Dictionary<DimensionEdge, bool>();
    dictionary1.Add(DimensionEdge.Top, false);
    dictionary1.Add(DimensionEdge.Left, false);
    dictionary1.Add(DimensionEdge.Bottom, false);
    dictionary1.Add(DimensionEdge.Right, false);
    bool eqForm = false;
    if (bFirst)
    {
      Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> dictionary2 = DimReferencesManager.InstrumentSFContours(revitDoc, structuralFramingElement.Id, view);
      if (dictionary2 != null)
      {
        foreach (int key1 in (IEnumerable<int>) dictionary2.Keys.OrderBy<int, int>((Func<int, int>) (i => i)))
        {
          string str = key1 >= 0 ? "BLOCKOUT" : "CONTOUR";
          if (customValues != null)
            str = !str.Equals("BLOCKOUT") ? customValues.Contour : customValues.Blockout;
          Dictionary<DimensionEdge, List<DimensionElement>> quadrant = BatchAutoDimension.DetermineQuadrant(revitDoc, view, sfFamInst, BatchAutoDimension.ConvertSFToDimElem(dictionary2[key1], view), true, out Dictionary<DimensionEdge, int> _);
          foreach (DimensionEdge key2 in quadrant.Keys)
            dictionary2[key1][key2] = quadrant[key2].Select<DimensionElement, ReferencedPoint>((Func<DimensionElement, ReferencedPoint>) (dE => dE.refP)).ToList<ReferencedPoint>();
          foreach (DimensionEdge key3 in quadrant.Keys)
          {
            if (dictionary2[key1][key3].Count > 0)
            {
              TicketAutoDim_Command ticketAutoDimCommand2 = ticketAutoDimCommand1;
              Document revitDoc1 = revitDoc;
              View view1 = view;
              List<ElementId> elementIds = new List<ElementId>();
              elementIds.Add(structuralFramingElement.Id);
              int side = (int) key3;
              List<ReferencedPoint> sfPoints = dictionary2[key1][key3];
              string sfDimName = str;
              CalloutStyle styleDetails = autoTicketStyleSettings;
              AutoTicketAppendString appendStringData1 = appendStringData;
              AutoTicketMinimumDimension minDimValue = minDimData;
              List<AutoTicketAppendStringParameterData> appendStringParameterData = appendStringParameterDatas;
              if (ticketAutoDimCommand2.Dimension(revitDoc1, view1, elementIds, (DimensionEdge) side, false, FormLocation.None, DimStyle.AutoTicketSFCONTOUR, sfPoints: sfPoints, sfDimName: sfDimName, styleDetails: styleDetails, appendStringData: appendStringData1, minDimValue: minDimValue, appendStringParameterData: appendStringParameterData) == 0)
                dictionary1[key3] = true;
            }
          }
        }
      }
    }
    foreach (string key4 in dimensionDictionary.Keys)
    {
      foreach (FormLocation key5 in dimensionDictionary[key4].Keys)
      {
        if (dimensionDictionary[key4][key5].Count > 0)
        {
          List<DimensionElement> dimElem = BatchAutoDimension.ConvertToDimElem(revitDoc, dimensionDictionary[key4][key5], view);
          Dictionary<DimensionEdge, int> quadCounts;
          Dictionary<DimensionEdge, List<ElementId>> quadrantElementIds = BatchAutoDimension.DetermineQuadrantElementIds(revitDoc, view, sfFamInst, dimElem, out quadCounts);
          foreach (DimensionEdge key6 in quadrantElementIds.Keys)
          {
            if (cloneTicket && cloneTicketSideDictionary != null)
            {
              SortingInfoCloneTicketElement key7 = new SortingInfoCloneTicketElement(key4, key5);
              if (cloneTicketSideDictionary.ContainsKey(key7) && !cloneTicketSideDictionary[key7][key6])
                continue;
            }
            bool flag = false;
            if (quadrantElementIds[key6].Count != 0)
              flag = ticketAutoDimCommand1.Dimension(revitDoc, view, quadrantElementIds[key6], key6, eqForm, key5, DimStyle.AutoTicketSTD, quadCounts[key6], sfDimName: (string) null, styleDetails: autoTicketStyleSettings, appendStringData: appendStringData, minDimValue: minDimData, appendStringParameterData: appendStringParameterDatas) == 0;
            if (flag)
              dictionary1[key6] = true;
          }
        }
      }
    }
    if (!cloneTicket)
    {
      if (customValues != null)
      {
        ticketAutoDimCommand1.Dimension(revitDoc, view, new List<ElementId>(), DimensionEdge.Top, false, FormLocation.None, DimStyle.AutoTicketOVERALL, sfDimName: customValues.Overall, styleDetails: autoTicketStyleSettings, appendStringData: appendStringData, minDimValue: minDimData);
        ticketAutoDimCommand1.Dimension(revitDoc, view, new List<ElementId>(), DimensionEdge.Left, false, FormLocation.None, DimStyle.AutoTicketOVERALL, sfDimName: customValues.Overall, styleDetails: autoTicketStyleSettings, appendStringData: appendStringData, minDimValue: minDimData);
      }
      else
      {
        ticketAutoDimCommand1.Dimension(revitDoc, view, new List<ElementId>(), DimensionEdge.Top, false, FormLocation.None, DimStyle.AutoTicketOVERALL, sfDimName: "OVERALL");
        ticketAutoDimCommand1.Dimension(revitDoc, view, new List<ElementId>(), DimensionEdge.Left, false, FormLocation.None, DimStyle.AutoTicketOVERALL, sfDimName: "OVERALL");
      }
    }
    return (Result) 0;
  }

  public static List<DimensionElement> ConvertSFToDimElem(
    Dictionary<DimensionEdge, List<ReferencedPoint>> refPoints,
    View view)
  {
    List<DimensionElement> source = new List<DimensionElement>();
    foreach (DimensionEdge key in refPoints.Keys)
    {
      foreach (ReferencedPoint refP in refPoints[key])
      {
        XYZ localPoint = refP.LocalPoint;
        double elemX = localPoint.X;
        double elemY = localPoint.Y;
        if (!source.Any<DimensionElement>((Func<DimensionElement, bool>) (dE => dE.horizPos == elemX && dE.verticalPos == elemY)))
          source.Add(new DimensionElement(elemX, elemY, refP));
      }
    }
    return source;
  }

  public static List<DimensionElement> ConvertToDimElem(
    Document revitDoc,
    List<ElementId> eidList,
    View view)
  {
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    List<DimensionElement> dimElem = new List<DimensionElement>();
    foreach (ElementId eid in eidList)
    {
      LocationPoint location = revitDoc.GetElement(eid).Location as LocationPoint;
      XYZ xyz = viewTransform.Inverse.OfPoint(location.Point);
      dimElem.Add(new DimensionElement(xyz.X, xyz.Y, eid));
    }
    return dimElem;
  }

  public static List<DimensionElement> CullDimensionElements(
    List<DimensionElement> dimElems,
    DimensionEdge side)
  {
    bool bVertical = false;
    bool flag = false;
    switch (side)
    {
      case DimensionEdge.Left:
        bVertical = false;
        flag = false;
        break;
      case DimensionEdge.Right:
        bVertical = false;
        flag = true;
        break;
      case DimensionEdge.Top:
        bVertical = true;
        flag = true;
        break;
      case DimensionEdge.Bottom:
        bVertical = true;
        flag = false;
        break;
    }
    List<DimensionElement> dimensionElementList = new List<DimensionElement>();
    if (dimElems.Count > 0)
    {
      foreach (IEnumerable<DimensionElement> source in dimElems.GroupBy<DimensionElement, double>((Func<DimensionElement, double>) (dE => Math.Round(bVertical ? dE.horizPos : dE.verticalPos, 4))))
      {
        List<DimensionElement> list = source.OrderBy<DimensionElement, double>((Func<DimensionElement, double>) (dE => !bVertical ? dE.horizPos : dE.verticalPos)).ToList<DimensionElement>();
        dimensionElementList.Add(flag ? list.Last<DimensionElement>() : list.First<DimensionElement>());
      }
    }
    return dimensionElementList;
  }

  public static Dictionary<DimensionEdge, List<DimensionElement>> DetermineQuadrant(
    Document revitDoc,
    View view,
    FamilyInstance sfFamInst,
    List<DimensionElement> DimList,
    bool bSF,
    out Dictionary<DimensionEdge, int> quadCounts)
  {
    Dictionary<DimensionEdge, List<DimensionElement>> quadrant = new Dictionary<DimensionEdge, List<DimensionElement>>();
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
          List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Left, view);
          if (dimLineReference == null || dimLineReference.Count == 0)
            dimensionElementList3.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList4.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Right, view);
          if (dimLineReference == null || dimLineReference.Count == 0)
            dimensionElementList4.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList1.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Top, view);
          if (dimLineReference == null || dimLineReference.Count == 0)
            dimensionElementList1.Remove(dimensionElement);
        }
      }
      foreach (DimensionElement dimensionElement in dimensionElementList2.ToList<DimensionElement>())
      {
        if (revitDoc.GetElement(dimensionElement.id) is FamilyInstance)
        {
          List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(revitDoc.GetElement(dimensionElement.id) as FamilyInstance, DimensionEdge.Bottom, view);
          if (dimLineReference == null || dimLineReference.Count == 0)
            dimensionElementList2.Remove(dimensionElement);
        }
      }
    }
    quadrant.Add(DimensionEdge.Left, BatchAutoDimension.CullDimensionElements(dimensionElementList3.ToList<DimensionElement>(), DimensionEdge.Left));
    quadCounts.Add(DimensionEdge.Left, dimensionElementList3.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    quadrant.Add(DimensionEdge.Right, BatchAutoDimension.CullDimensionElements(dimensionElementList4.ToList<DimensionElement>(), DimensionEdge.Right));
    quadCounts.Add(DimensionEdge.Right, dimensionElementList4.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    quadrant.Add(DimensionEdge.Top, BatchAutoDimension.CullDimensionElements(dimensionElementList1.ToList<DimensionElement>(), DimensionEdge.Top));
    quadCounts.Add(DimensionEdge.Top, dimensionElementList1.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    quadrant.Add(DimensionEdge.Bottom, BatchAutoDimension.CullDimensionElements(dimensionElementList2.ToList<DimensionElement>(), DimensionEdge.Bottom));
    quadCounts.Add(DimensionEdge.Bottom, dimensionElementList2.Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).Distinct<ElementId>().Count<ElementId>());
    return quadrant;
  }

  public static Dictionary<DimensionEdge, List<ElementId>> DetermineQuadrantElementIds(
    Document revitDoc,
    View view,
    FamilyInstance sfFamInst,
    List<DimensionElement> DimList,
    out Dictionary<DimensionEdge, int> quadCounts)
  {
    Dictionary<DimensionEdge, List<DimensionElement>> quadrant = BatchAutoDimension.DetermineQuadrant(revitDoc, view, sfFamInst, DimList, false, out quadCounts);
    Dictionary<DimensionEdge, List<ElementId>> quadrantElementIds = new Dictionary<DimensionEdge, List<ElementId>>();
    foreach (DimensionEdge key in quadrant.Keys)
      quadrantElementIds.Add(key, quadrant[key].Select<DimensionElement, ElementId>((Func<DimensionElement, ElementId>) (dE => dE.id)).ToList<ElementId>());
    return quadrantElementIds;
  }
}
