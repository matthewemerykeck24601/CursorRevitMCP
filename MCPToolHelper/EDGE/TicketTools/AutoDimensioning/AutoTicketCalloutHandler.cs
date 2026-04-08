// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketCalloutHandler
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.AssemblyUtils;
using Utils.Forms;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

internal class AutoTicketCalloutHandler
{
  private View _view;
  private Document _revitDoc;
  private AssemblyInstance _assInst;
  private LocationInFormAnalyzer _locationAnalyzer;
  private FamilyInstance _sfFamInst;
  private StructuralFramingBoundsObject SFBoundsManager;
  private Dictionary<CalloutQuadrant, List<AutoTicketTag>> NotesByQuadrant;
  private Dictionary<string, Dictionary<FormLocation, List<ElementId>>> ElemDict;
  private Dictionary<ElementId, BoundingBoxXYZ> tagBounds = new Dictionary<ElementId, BoundingBoxXYZ>();

  public AutoTicketCalloutHandler(
    View view,
    Document revitDoc,
    Dictionary<string, Dictionary<FormLocation, List<ElementId>>> elementDictionary,
    bool hardwareDetail = false,
    Element selectedElement = null)
  {
    this._revitDoc = revitDoc;
    this._view = view;
    this._assInst = revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance;
    this._locationAnalyzer = new LocationInFormAnalyzer(this._assInst, 1.0, hardwareDetail);
    this._sfFamInst = this._assInst.GetStructuralFramingElement() as FamilyInstance;
    this.ElemDict = elementDictionary;
    this.SFBoundsManager = !hardwareDetail ? new StructuralFramingBoundsObject(this._sfFamInst, this._view) : new StructuralFramingBoundsObject(this._assInst, this._view, selectedElement);
    this.NotesByQuadrant = new Dictionary<CalloutQuadrant, List<AutoTicketTag>>();
    this.NotesByQuadrant.Add(CalloutQuadrant.TopLeft, new List<AutoTicketTag>());
    this.NotesByQuadrant.Add(CalloutQuadrant.TopRight, new List<AutoTicketTag>());
    this.NotesByQuadrant.Add(CalloutQuadrant.BottomLeft, new List<AutoTicketTag>());
    this.NotesByQuadrant.Add(CalloutQuadrant.BottomRight, new List<AutoTicketTag>());
  }

  public bool processAllCallOuts(
    FamilySymbol leftTagFamily,
    FamilySymbol rightTagFamily,
    FamilySymbol leftTagFamilyTYP,
    FamilySymbol rightTagFamilyTYP,
    bool HardwareDetail = false)
  {
    Dictionary<ElementId, bool> TypDict = new Dictionary<ElementId, bool>();
    List<List<ElementId>> source1 = new List<List<ElementId>>();
    foreach (string key1 in this.ElemDict.Keys)
    {
      foreach (FormLocation key2 in this.ElemDict[key1].Keys)
      {
        if (this.ElemDict[key1][key2].Count > 0)
        {
          if (this.ElemDict[key1][key2].Count > 1)
            this.ElemDict[key1][key2].ForEach((Action<ElementId>) (eid => TypDict.Add(eid, true)));
          else
            TypDict.Add(this.ElemDict[key1][key2].First<ElementId>(), false);
          source1.Add(this.ElemDict[key1][key2].ToList<ElementId>());
        }
      }
    }
    if (!HardwareDetail)
    {
      foreach (List<ElementId> elementIdList in source1)
        elementIdList.RemoveAll((Predicate<ElementId>) (elem => Oracle.IsAddonFamily(this._revitDoc.GetElement(elem))));
      source1.RemoveAll((Predicate<List<ElementId>>) (list => list.Count == 0));
    }
    source1.OrderByDescending<List<ElementId>, int>((Func<List<ElementId>, int>) (x => x.Count));
    Dictionary<CalloutQuadrant, int> dictionary = new Dictionary<CalloutQuadrant, int>();
    dictionary.Add(CalloutQuadrant.TopLeft, 0);
    dictionary.Add(CalloutQuadrant.TopRight, 0);
    dictionary.Add(CalloutQuadrant.BottomLeft, 0);
    dictionary.Add(CalloutQuadrant.BottomRight, 0);
    dictionary.Add(CalloutQuadrant.Invalid, 0);
    List<ElementId> source2 = new List<ElementId>();
    using (List<List<ElementId>>.Enumerator enumerator = source1.GetEnumerator())
    {
label_51:
      if (enumerator.MoveNext())
      {
        List<ElementId> current = enumerator.Current;
        Dictionary<CalloutQuadrant, int> source3 = new Dictionary<CalloutQuadrant, int>();
        source3.Add(CalloutQuadrant.TopLeft, 0);
        source3.Add(CalloutQuadrant.TopRight, 0);
        source3.Add(CalloutQuadrant.BottomLeft, 0);
        source3.Add(CalloutQuadrant.BottomRight, 0);
        source3.Add(CalloutQuadrant.Invalid, 0);
        foreach (ElementId eid in current)
          source3[this.DetermineElementQuadrant(eid)]++;
        IEnumerable<KeyValuePair<CalloutQuadrant, int>> keyValuePairs = (IEnumerable<KeyValuePair<CalloutQuadrant, int>>) source3.ToList<KeyValuePair<CalloutQuadrant, int>>().OrderByDescending<KeyValuePair<CalloutQuadrant, int>, int>((Func<KeyValuePair<CalloutQuadrant, int>, int>) (x => x.Value));
        int num1 = 0;
        bool flag = false;
        do
        {
          foreach (KeyValuePair<CalloutQuadrant, int> keyValuePair in keyValuePairs)
          {
            if (keyValuePair.Key != CalloutQuadrant.Invalid && source3[keyValuePair.Key] != 0 && dictionary[keyValuePair.Key] == num1)
            {
              CalloutQuadrant key = keyValuePair.Key;
              flag = true;
              dictionary[keyValuePair.Key]++;
              List<ElementId> elementIdList = new List<ElementId>();
              foreach (ElementId eid in current)
              {
                if (this.DetermineElementQuadrant(eid) == key)
                  elementIdList.Add(eid);
              }
              double num2 = -1.0;
              ElementId elementId = (ElementId) null;
              foreach (ElementId eid in elementIdList)
              {
                double num3 = Math.Min(this.FindMinDistanceForId(eid)[0], this.FindMinDistanceForId(eid)[1]);
                if (num2 == -1.0 || num3 < num2)
                {
                  num2 = num3;
                  elementId = eid;
                }
              }
              if (elementId != (ElementId) null)
              {
                source2.Add(elementId);
                break;
              }
              TaskDialog.Show("Auto-Ticket", "No ID chosen for quadrant");
              break;
            }
          }
          ++num1;
        }
        while (!flag);
        goto label_51;
      }
    }
    List<ElementId> list1 = source2.OrderBy<ElementId, double>((Func<ElementId, double>) (x => this.distFromPlacementSource(x))).ToList<ElementId>();
    List<AutoTicketTag> tags = new List<AutoTicketTag>();
    List<ElementId> source4 = new List<ElementId>();
    foreach (ElementId elementId in list1)
    {
      CalloutQuadrant elementQuadrant = this.DetermineElementQuadrant(elementId);
      bool flag = TypDict[elementId];
      ElementId tagId;
      AutoTicketTag autoTicketTag = this.RenderTextNote(elementId, elementQuadrant, flag ? leftTagFamilyTYP : leftTagFamily, flag ? rightTagFamilyTYP : rightTagFamily, out tagId);
      if (autoTicketTag != null)
      {
        if (autoTicketTag.tag != null)
        {
          tags.Add(autoTicketTag);
          IndependentTag tag = autoTicketTag.tag;
          this.tagBounds.Add(tag.Id, DimReferencesManager.GetViewBBox(tag, this._view));
          this.NotesByQuadrant[elementQuadrant].Add(autoTicketTag);
        }
      }
      else
        source4.Add(tagId);
    }
    if (tags.Count > 0)
    {
      double num4 = double.MinValue;
      double num5 = double.MinValue;
      foreach (AutoTicketTag autoTicketTag in tags)
      {
        num4 = Math.Max(autoTicketTag.width, num4);
        num5 = Math.Max(autoTicketTag.height, num5);
      }
      Dictionary<SquarePlacement, List<AutoTicketPlacementSquare>> squareDict = new Dictionary<SquarePlacement, List<AutoTicketPlacementSquare>>()
      {
        {
          SquarePlacement.Left,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.Right,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.Top,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.Bottom,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.TopLeft,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.TopRight,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.BottomLeft,
          new List<AutoTicketPlacementSquare>()
        },
        {
          SquarePlacement.BottomRight,
          new List<AutoTicketPlacementSquare>()
        }
      };
      this.AddNewRow(squareDict, num4, num5);
      this.AssignCallouts(tags, squareDict, num4, num5);
    }
    foreach (ElementId elementId in new FilteredElementCollector(this._revitDoc, this._view.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>())
    {
      if (source4.Any<ElementId>().Equals((object) elementId))
        source4.Remove(elementId);
    }
    if (source4.Count > 0)
      this._revitDoc.Delete((ICollection<ElementId>) source4);
    return true;
  }

  private double distFromPlacementSource(ElementId eid)
  {
    XYZ point = (this._revitDoc.GetElement(eid).Location as LocationPoint).Point;
    double num1 = point.DotProduct(this._view.RightDirection.Normalize());
    double num2 = point.DotProduct(this._view.UpDirection.Normalize());
    double val1_1 = Math.Abs(num1 - this.SFBoundsManager.xMin);
    double val2_1 = Math.Abs(num1 - this.SFBoundsManager.xMax);
    double val1_2 = Math.Abs(num2 - this.SFBoundsManager.yMax);
    double val2_2 = Math.Abs(num2 - this.SFBoundsManager.yMin);
    double num3 = Math.Min(val1_1, val2_1);
    double num4 = Math.Min(val1_2, val2_2);
    return num3 < num4 ? num3 : num4;
  }

  private double[] FindMinDistanceForId(ElementId eid)
  {
    XYZ point = (this._revitDoc.GetElement(eid).Location as LocationPoint).Point;
    double num1 = point.DotProduct(this._view.RightDirection.Normalize());
    double num2 = point.DotProduct(this._view.UpDirection.Normalize());
    return new double[2]
    {
      Math.Min(Math.Abs(num1 - this.SFBoundsManager.xMin), Math.Abs(num1 - this.SFBoundsManager.xMax)),
      Math.Min(Math.Abs(num2 - this.SFBoundsManager.yMin), Math.Abs(num2 - this.SFBoundsManager.yMax))
    };
  }

  public CalloutQuadrant DetermineElementQuadrant(ElementId eid)
  {
    XYZ point = (this._revitDoc.GetElement(eid).Location as LocationPoint).Point;
    double num1 = point.DotProduct(this._view.RightDirection.Normalize());
    double num2 = point.DotProduct(this._view.UpDirection.Normalize());
    double xMid = this.SFBoundsManager.xMid;
    double yMid = this.SFBoundsManager.yMid;
    if (num1 < xMid && num2 > yMid)
      return CalloutQuadrant.TopLeft;
    if (num1 >= xMid && num2 > yMid)
      return CalloutQuadrant.TopRight;
    if (num1 < xMid && num2 <= yMid)
      return CalloutQuadrant.BottomLeft;
    return num1 >= xMid && num2 <= yMid ? CalloutQuadrant.BottomRight : CalloutQuadrant.Invalid;
  }

  public AutoTicketTag RenderTextNote(
    ElementId selectedElemId,
    CalloutQuadrant quadrant,
    FamilySymbol leftTagFamily,
    FamilySymbol rightTagFamily,
    out ElementId tagId)
  {
    tagId = ElementId.InvalidElementId;
    Element element = this._revitDoc.GetElement(selectedElemId);
    try
    {
      double num1 = 0.75;
      double num2 = 0.5;
      XYZ point = (element.Location as LocationPoint).Point;
      double num3 = point.DotProduct(this._view.RightDirection);
      double num4 = point.DotProduct(this._view.UpDirection);
      XYZ xyz;
      XYZ pnt;
      switch (quadrant)
      {
        case CalloutQuadrant.TopLeft:
          double num5 = Math.Abs(num3 - this.SFBoundsManager.xMin);
          double num6 = Math.Abs(num4 - this.SFBoundsManager.yMax);
          if (num5 < num6)
            num6 = 0.0;
          else
            num5 = 0.0;
          xyz = point + this._view.UpDirection.Normalize() * (num6 + num1) - this._view.RightDirection.Normalize() * (num5 + num1);
          pnt = xyz - this._view.RightDirection.Normalize() * num2;
          break;
        case CalloutQuadrant.TopRight:
          double num7 = Math.Abs(num3 - this.SFBoundsManager.xMax);
          double num8 = Math.Abs(num4 - this.SFBoundsManager.yMax);
          if (num7 < num8)
            num8 = 0.0;
          else
            num7 = 0.0;
          xyz = point + this._view.UpDirection.Normalize() * (num8 + num1) + this._view.RightDirection.Normalize() * (num7 + num1);
          pnt = xyz + this._view.RightDirection.Normalize() * num2;
          break;
        case CalloutQuadrant.BottomLeft:
          double num9 = Math.Abs(num3 - this.SFBoundsManager.xMin);
          double num10 = Math.Abs(num4 - this.SFBoundsManager.yMin);
          if (num9 < num10)
            num10 = 0.0;
          else
            num9 = 0.0;
          xyz = point - this._view.UpDirection.Normalize() * (num10 + num1) - this._view.RightDirection.Normalize() * (num9 + num1);
          pnt = xyz - this._view.RightDirection.Normalize() * num2;
          break;
        case CalloutQuadrant.BottomRight:
          double num11 = Math.Abs(num3 - this.SFBoundsManager.xMax);
          double num12 = Math.Abs(num4 - this.SFBoundsManager.yMin);
          if (num11 < num12)
            num12 = 0.0;
          else
            num11 = 0.0;
          xyz = point - this._view.UpDirection.Normalize() * (num12 + num1) + this._view.RightDirection.Normalize() * (num11 + num1);
          pnt = xyz + this._view.RightDirection.Normalize() * num2;
          break;
        default:
          double num13 = Math.Abs(num3 - this.SFBoundsManager.xMin);
          double num14 = Math.Abs(num4 - this.SFBoundsManager.yMax);
          if (num13 < num14)
            num14 = 0.0;
          else
            num13 = 0.0;
          xyz = point + this._view.UpDirection.Normalize() * (num14 + num1) - this._view.RightDirection.Normalize() * (num13 + num1);
          pnt = xyz - this._view.RightDirection.Normalize() * num2;
          break;
      }
      Reference referenceToTag = new Reference(this._revitDoc.GetElement(selectedElemId));
      IndependentTag tag = IndependentTag.Create(this._revitDoc, this._view.Id, referenceToTag, false, TagMode.TM_ADDBY_MULTICATEGORY, TagOrientation.Horizontal, pnt);
      if (rightTagFamily != null && leftTagFamily != null)
      {
        if (quadrant == CalloutQuadrant.TopLeft || quadrant == CalloutQuadrant.BottomLeft)
          tag.ChangeTypeId(rightTagFamily.Id);
        else
          tag.ChangeTypeId(leftTagFamily.Id);
      }
      tag.LeaderEndCondition = LeaderEndCondition.Free;
      Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(this._view);
      tagId = tag.Id;
      List<XYZ> outlinePoints;
      BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(tag, this._view, out outlinePoints);
      if (viewBbox == null)
        return (AutoTicketTag) null;
      List<XYZ> list = outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => viewTransform.Inverse.OfPoint(pt))).ToList<XYZ>();
      double num15 = list.Max<XYZ>((Func<XYZ, double>) (pt => pt.X)) - list.Min<XYZ>((Func<XYZ, double>) (pt => pt.X));
      double num16 = list.Max<XYZ>((Func<XYZ, double>) (pt => pt.Y)) - list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Y));
      return new AutoTicketTag()
      {
        tag = tag,
        elbowPoint = xyz,
        width = num15,
        height = num16,
        tagBBox = viewBbox,
        placementPoint = pnt,
        endPoint = point,
        quadrant = quadrant,
        leftFamily = leftTagFamily,
        rightFamily = rightTagFamily,
        view = this._view,
        referenceToTaggedElement = referenceToTag
      };
    }
    catch (Exception ex)
    {
      if (ex.ToString().Contains("The user aborted the pick operation."))
        return (AutoTicketTag) null;
      if (ex.ToString().Contains("Object reference not set to an instance of an object."))
      {
        new TaskDialog("Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "The selected Element is not valid for annotation. Only Structural Framing (excluding Assemblies), Specialty Equipment, and Generic Model Elements can be annotated."
        }.Show();
        return (AutoTicketTag) null;
      }
      ErrorForm errorForm = new ErrorForm();
      errorForm.textBox1.Text = ex.ToString();
      int num = (int) errorForm.ShowDialog();
      return (AutoTicketTag) null;
    }
  }

  public void AssignCallouts(
    List<AutoTicketTag> tags,
    Dictionary<SquarePlacement, List<AutoTicketPlacementSquare>> squareDict,
    double maxWidth,
    double maxHeight)
  {
    bool flag1 = this.SFBoundsManager.Height > this.SFBoundsManager.Width;
    try
    {
      List<AutoTicketPlacementSquare> ticketPlacementSquareList1 = new List<AutoTicketPlacementSquare>();
      List<AutoTicketPlacementSquare> ticketPlacementSquareList2 = squareDict[SquarePlacement.Top];
      List<AutoTicketPlacementSquare> ticketPlacementSquareList3 = squareDict[SquarePlacement.Bottom];
      List<AutoTicketPlacementSquare> ticketPlacementSquareList4 = squareDict[SquarePlacement.Left];
      List<AutoTicketPlacementSquare> ticketPlacementSquareList5 = squareDict[SquarePlacement.Right];
      List<AutoTicketPlacementSquare> allSquares = new List<AutoTicketPlacementSquare>();
      squareDict.Values.ToList<List<AutoTicketPlacementSquare>>().ForEach((Action<List<AutoTicketPlacementSquare>>) (sq => allSquares.AddRange((IEnumerable<AutoTicketPlacementSquare>) sq)));
      List<AutoTicketTag> list = tags.ToList<AutoTicketTag>();
      while (list.Count > 0)
      {
        double currMinX = double.MinValue;
        foreach (AutoTicketTag autoTicketTag1 in (IEnumerable<AutoTicketTag>) tags.OrderBy<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => Math.Round(t.GetProjected(), 2))).ThenByDescending<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => Math.Abs(t.GetProjected(true) - this.SFBoundsManager.yMid))))
        {
          AutoTicketTag tag = autoTicketTag1;
          XYZ endPoint = tag.endPoint;
          XYZ xyz = new XYZ();
          List<AutoTicketPlacementSquare> source1 = new List<AutoTicketPlacementSquare>();
          double projected = tag.GetProjected();
          switch (tag.quadrant)
          {
            case CalloutQuadrant.TopLeft:
              source1 = !flag1 ? squareDict[SquarePlacement.TopLeft].Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) ticketPlacementSquareList2).ToList<AutoTicketPlacementSquare>() : ticketPlacementSquareList2.ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.TopRight:
              source1 = !flag1 ? ticketPlacementSquareList2.Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) squareDict[SquarePlacement.TopRight]).ToList<AutoTicketPlacementSquare>() : ticketPlacementSquareList2.ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.BottomLeft:
              source1 = !flag1 ? squareDict[SquarePlacement.BottomLeft].Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) ticketPlacementSquareList3).ToList<AutoTicketPlacementSquare>() : ticketPlacementSquareList3.ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.BottomRight:
              source1 = !flag1 ? ticketPlacementSquareList3.Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) squareDict[SquarePlacement.BottomRight]).ToList<AutoTicketPlacementSquare>() : ticketPlacementSquareList3.ToList<AutoTicketPlacementSquare>();
              break;
          }
          foreach (AutoTicketPlacementSquare ticketPlacementSquare1 in (IEnumerable<AutoTicketPlacementSquare>) source1.Where<AutoTicketPlacementSquare>((Func<AutoTicketPlacementSquare, bool>) (sq => !sq.bOccupied && sq.projectedX >= currMinX)).OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.TagList.Count)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (sq => Math.Abs(tag.GetProjected() - sq.projectedX))))
          {
            if (Math.Abs(projected - ticketPlacementSquare1.projectedX) < maxWidth / 2.0)
              ticketPlacementSquare1.bOccupied = true;
            else if (list.Contains(tag) && ticketPlacementSquare1.TagList != null)
            {
              AutoTicketPlacementSquare ticketPlacementSquare2 = ticketPlacementSquare1;
              tag.square = ticketPlacementSquare2;
              ticketPlacementSquare2.bOccupied = true;
              ticketPlacementSquare2.TagList.Add(tag);
              list.Remove(tag);
              break;
            }
          }
          if (tag.square != null)
          {
            Line checkLine = Line.CreateBound(new XYZ(tag.GetProjected(), tag.GetProjected(true), 0.0), new XYZ(tag.square.projectedX, tag.square.projectedY, 0.0));
            foreach (AutoTicketPlacementSquare ticketPlacementSquare in source1)
            {
              XYZ square_PT = new XYZ(ticketPlacementSquare.projectedX, ticketPlacementSquare.projectedY, 0.0);
              IEnumerable<AutoTicketTag> source2 = ticketPlacementSquare.TagList.Where<AutoTicketTag>((Func<AutoTicketTag, bool>) (t => checkLine.Intersect((Curve) Line.CreateBound(square_PT, new XYZ(t.GetProjected(), t.GetProjected(true), 0.0))) != SetComparisonResult.Disjoint));
              if (source2.Count<AutoTicketTag>() > 0)
              {
                AutoTicketTag autoTicketTag2 = source2.First<AutoTicketTag>();
                tag.square.TagList.Remove(tag);
                tag.square.TagList.Add(autoTicketTag2);
                ticketPlacementSquare.TagList.Remove(autoTicketTag2);
                ticketPlacementSquare.TagList.Add(tag);
                autoTicketTag2.square = tag.square;
                tag.square = ticketPlacementSquare;
              }
            }
          }
        }
        double currMinY = double.MinValue;
        foreach (AutoTicketTag autoTicketTag3 in (IEnumerable<AutoTicketTag>) tags.OrderBy<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => Math.Round(t.GetProjected(true), 2))).ThenByDescending<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => Math.Abs(t.GetProjected() - this.SFBoundsManager.xMid))))
        {
          AutoTicketTag tag = autoTicketTag3;
          XYZ endPoint = tag.endPoint;
          XYZ xyz = new XYZ();
          List<AutoTicketPlacementSquare> source3 = new List<AutoTicketPlacementSquare>();
          double projected = tag.GetProjected();
          switch (tag.quadrant)
          {
            case CalloutQuadrant.TopLeft:
              source3 = !flag1 ? ticketPlacementSquareList4 : ticketPlacementSquareList4.Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) squareDict[SquarePlacement.TopLeft]).ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.TopRight:
              source3 = !flag1 ? ticketPlacementSquareList5.ToList<AutoTicketPlacementSquare>() : ticketPlacementSquareList5.Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) squareDict[SquarePlacement.TopRight]).ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.BottomLeft:
              source3 = !flag1 ? ticketPlacementSquareList4.ToList<AutoTicketPlacementSquare>() : squareDict[SquarePlacement.BottomLeft].Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) ticketPlacementSquareList4).ToList<AutoTicketPlacementSquare>();
              break;
            case CalloutQuadrant.BottomRight:
              source3 = !flag1 ? ticketPlacementSquareList5.ToList<AutoTicketPlacementSquare>() : squareDict[SquarePlacement.BottomRight].Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) ticketPlacementSquareList5).ToList<AutoTicketPlacementSquare>();
              break;
          }
          foreach (AutoTicketPlacementSquare ticketPlacementSquare3 in (IEnumerable<AutoTicketPlacementSquare>) source3.Where<AutoTicketPlacementSquare>((Func<AutoTicketPlacementSquare, bool>) (sq => !sq.bOccupied && sq.projectedY >= currMinY)).OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.TagList.Count)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (sq => Math.Abs(tag.GetProjected(true) - sq.projectedY))))
          {
            if (Math.Abs(projected - ticketPlacementSquare3.projectedX) < maxWidth / 2.0)
              ticketPlacementSquare3.bOccupied = true;
            else if (list.Contains(tag) && ticketPlacementSquare3.TagList != null)
            {
              AutoTicketPlacementSquare ticketPlacementSquare4 = ticketPlacementSquare3;
              tag.square = ticketPlacementSquare4;
              ticketPlacementSquare4.bOccupied = true;
              ticketPlacementSquare4.TagList.Add(tag);
              list.Remove(tag);
              break;
            }
          }
          if (tag.square != null)
          {
            Line checkLine = Line.CreateBound(new XYZ(tag.GetProjected(), tag.GetProjected(true), 0.0), new XYZ(tag.square.projectedX, tag.square.projectedY, 0.0));
            foreach (AutoTicketPlacementSquare ticketPlacementSquare in source3)
            {
              XYZ square_PT = new XYZ(ticketPlacementSquare.projectedX, ticketPlacementSquare.projectedY, 0.0);
              IEnumerable<AutoTicketTag> source4 = ticketPlacementSquare.TagList.Where<AutoTicketTag>((Func<AutoTicketTag, bool>) (t => checkLine.Intersect((Curve) Line.CreateBound(square_PT, new XYZ(t.GetProjected(), t.GetProjected(true), 0.0))) != SetComparisonResult.Disjoint));
              if (source4.Count<AutoTicketTag>() > 0)
              {
                AutoTicketTag autoTicketTag4 = source4.First<AutoTicketTag>();
                tag.square.TagList.Add(autoTicketTag4);
                tag.square.TagList.Remove(tag);
                ticketPlacementSquare.TagList.Remove(autoTicketTag4);
                ticketPlacementSquare.TagList.Add(tag);
                autoTicketTag4.square = tag.square;
                tag.square = ticketPlacementSquare;
              }
            }
          }
        }
        allSquares.ForEach((Action<AutoTicketPlacementSquare>) (sq => sq.bOccupied = false));
      }
      foreach (AutoTicketPlacementSquare ticketPlacementSquare in allSquares)
      {
        if (ticketPlacementSquare.TagList != null && ticketPlacementSquare.TagList.Count > 0)
        {
          bool flag2 = false;
          XYZ dir = new XYZ();
          XYZ otherDir = new XYZ();
          double num1 = -1.0;
          if (squareDict[SquarePlacement.TopLeft].Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection;
            otherDir = this._view.UpDirection;
            flag2 = true;
            num1 = maxHeight;
          }
          else if (squareDict[SquarePlacement.TopRight].Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection.Negate();
            otherDir = this._view.UpDirection;
            flag2 = true;
            num1 = maxHeight;
          }
          else if (squareDict[SquarePlacement.BottomLeft].Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection;
            otherDir = this._view.UpDirection.Negate();
            flag2 = true;
            num1 = maxHeight;
          }
          else if (squareDict[SquarePlacement.BottomRight].Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection.Negate();
            otherDir = this._view.UpDirection.Negate();
            flag2 = true;
            num1 = maxHeight;
          }
          if (ticketPlacementSquareList2.Contains(ticketPlacementSquare))
          {
            dir = this._view.UpDirection;
            otherDir = this._view.RightDirection;
            num1 = maxHeight;
          }
          else if (ticketPlacementSquareList3.Contains(ticketPlacementSquare))
          {
            dir = this._view.UpDirection.Negate();
            otherDir = this._view.RightDirection;
            num1 = maxHeight;
          }
          else if (ticketPlacementSquareList4.Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection.Negate();
            otherDir = this._view.UpDirection;
            num1 = maxWidth;
          }
          else if (ticketPlacementSquareList5.Contains(ticketPlacementSquare))
          {
            dir = this._view.RightDirection;
            otherDir = this._view.UpDirection;
            num1 = maxWidth;
          }
          if (!otherDir.IsAlmostEqualTo(this._view.RightDirection))
          {
            double yMid = this.SFBoundsManager.yMid;
          }
          else
          {
            double xMid = this.SFBoundsManager.xMid;
          }
          ticketPlacementSquare.TagList = ticketPlacementSquare.TagList.OrderBy<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => Math.Round(dir.DotProduct(t.endPoint), 2))).ThenBy<AutoTicketTag, double>((Func<AutoTicketTag, double>) (t => otherDir.DotProduct(t.endPoint))).ToList<AutoTicketTag>();
          for (int index = 0; index < ticketPlacementSquare.TagList.Count; ++index)
          {
            double num2 = 0.5;
            AutoTicketTag tag = ticketPlacementSquare.TagList[index];
            tag.side = ticketPlacementSquare.projectedX > tag.GetProjected() ? DimensionEdge.Left : DimensionEdge.Right;
            DimensionEdge side = tag.side;
            XYZ xyz1 = (side == DimensionEdge.Left ? ticketPlacementSquare.Left : ticketPlacementSquare.Right) + (double) index * (flag2 ? otherDir : dir) * num1 * 1.5 + num2 * (side == DimensionEdge.Left ? this._view.RightDirection : this._view.RightDirection.Negate());
            if (tag.rightFamily != null && tag.leftFamily != null)
            {
              if (side == DimensionEdge.Right)
                tag.tag.ChangeTypeId(tag.rightFamily.Id);
              else
                tag.tag.ChangeTypeId(tag.leftFamily.Id);
            }
            ElementTransformUtils.MoveElement(this._revitDoc, tag.tag.Id, xyz1 - tag.placementPoint);
            tag.placementPoint = xyz1;
            List<XYZ> outlinePoints;
            BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(tag.tag, this._view, out outlinePoints);
            XYZ min = viewBbox.Min;
            XYZ max = viewBbox.Max;
            double num3 = outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => this._view.UpDirection.DotProduct(pt)));
            double num4 = outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => this._view.UpDirection.DotProduct(pt)));
            double num5 = outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => this._view.RightDirection.DotProduct(pt)));
            double num6 = outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => this._view.RightDirection.DotProduct(pt)));
            double num7 = num4;
            XYZ xyz2 = new XYZ() + this._view.UpDirection * ((num3 + num7) / 2.0) + (side == DimensionEdge.Left ? num5 : num6) * this._view.RightDirection;
            tag.elbowPoint = xyz2 + (side == DimensionEdge.Right ? this._view.RightDirection : this._view.RightDirection.Negate()) * num2;
            tag.tag.HasLeader = true;
            tag.tag.SetLeaderElbow(tag.referenceToTaggedElement, tag.elbowPoint);
            tag.tag.SetLeaderEnd(tag.referenceToTaggedElement, tag.endPoint);
          }
        }
      }
    }
    catch (Exception ex)
    {
      if (ex.ToString().Contains("The user aborted the pick operation."))
        return;
      ErrorForm errorForm = new ErrorForm();
      errorForm.textBox1.Text = ex.ToString();
      int num = (int) errorForm.ShowDialog();
    }
  }

  private void AddNewRow(
    Dictionary<SquarePlacement, List<AutoTicketPlacementSquare>> squareDict,
    double maxWidth,
    double maxHeight)
  {
    double width = maxWidth + 0.5;
    int layer = 0;
    List<AutoTicketPlacementSquare> ticketPlacementSquareList = new List<AutoTicketPlacementSquare>();
    foreach (List<AutoTicketPlacementSquare> second in squareDict.Values)
      ticketPlacementSquareList = ticketPlacementSquareList.Union<AutoTicketPlacementSquare>((IEnumerable<AutoTicketPlacementSquare>) second).ToList<AutoTicketPlacementSquare>();
    if (ticketPlacementSquareList.Count > 0)
      layer = ticketPlacementSquareList.Max<AutoTicketPlacementSquare>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)) + 1;
    List<XYZ> list = this.SFBoundsManager.outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => this.SFBoundsManager.viewTransform.Inverse.OfPoint(pt))).ToList<XYZ>();
    XYZ point1 = new XYZ(list.Min<XYZ>((Func<XYZ, double>) (pt => pt.X)), list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Y)), list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    XYZ point2 = new XYZ(list.Max<XYZ>((Func<XYZ, double>) (pt => pt.X)), list.Max<XYZ>((Func<XYZ, double>) (pt => pt.Y)), list.Max<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    XYZ xyz1 = this.SFBoundsManager.viewTransform.OfPoint(point1);
    XYZ xyz2 = this.SFBoundsManager.viewTransform.OfPoint(point2);
    int num1 = (int) Math.Truncate(this.SFBoundsManager.Height / maxHeight);
    double num2 = this.SFBoundsManager.Height - maxHeight * (double) num1;
    int num3 = (int) Math.Truncate(this.SFBoundsManager.Width / width);
    double num4 = this.SFBoundsManager.Width - width * (double) num3;
    XYZ xyz3 = xyz1 + num2 / 2.0 * this._view.UpDirection + num4 / 2.0 * this._view.RightDirection;
    XYZ xyz4 = xyz2 - num2 / 2.0 * this._view.UpDirection - num4 / 2.0 * this._view.RightDirection;
    for (int index = 0; index < num1; ++index)
    {
      AutoTicketPlacementSquare ticketPlacementSquare1 = new AutoTicketPlacementSquare(this._view, xyz3 - (width / 3.0 + width / 2.0) * this._view.RightDirection + (maxHeight * (double) index + maxHeight / 2.0) * this._view.UpDirection - num4 / 2.0 * this._view.RightDirection - width * 1.1 * (double) layer * this._view.RightDirection, width, layer, SquarePlacement.Left);
      AutoTicketPlacementSquare ticketPlacementSquare2 = new AutoTicketPlacementSquare(this._view, xyz4 + (width / 3.0 + width / 2.0) * this._view.RightDirection - (maxHeight * (double) index + maxHeight / 2.0) * this._view.UpDirection + num4 / 2.0 * this._view.RightDirection + width * 1.1 * (double) layer * this._view.RightDirection, width, layer, SquarePlacement.Right);
      squareDict[SquarePlacement.Left].Add(ticketPlacementSquare1);
      squareDict[SquarePlacement.Right].Add(ticketPlacementSquare2);
    }
    for (int index = 0; index < num3; ++index)
    {
      AutoTicketPlacementSquare ticketPlacementSquare3 = new AutoTicketPlacementSquare(this._view, xyz4 - (width * (double) index + width / 2.0) * this._view.RightDirection + maxHeight * 1.5 * this._view.UpDirection + num2 / 2.0 * this._view.UpDirection + maxHeight * 1.1 * (double) layer * this._view.UpDirection, width, layer, SquarePlacement.Top);
      AutoTicketPlacementSquare ticketPlacementSquare4 = new AutoTicketPlacementSquare(this._view, xyz3 + (width * (double) index + width / 2.0) * this._view.RightDirection - maxHeight * 1.5 * this._view.UpDirection - num2 / 2.0 * this._view.UpDirection - maxHeight * 1.1 * (double) layer * this._view.UpDirection, width, layer, SquarePlacement.Bottom);
      squareDict[SquarePlacement.Top].Add(ticketPlacementSquare3);
      squareDict[SquarePlacement.Bottom].Add(ticketPlacementSquare4);
    }
    XYZ xyz5 = xyz3 - maxHeight * 1.1 * (double) layer * this._view.UpDirection;
    XYZ xyz6 = xyz4 + maxHeight * 1.1 * (double) layer * this._view.UpDirection;
    XYZ Center_PT1 = xyz6 - (this.SFBoundsManager.Width - num4 / 2.0 + width) * this._view.RightDirection + (maxHeight * 1.5 + num2 / 2.0) * this._view.UpDirection;
    XYZ Center_PT2 = xyz6 + (num4 / 2.0 + width) * this._view.RightDirection + (maxHeight * 1.5 + num2 / 2.0) * this._view.UpDirection;
    XYZ Center_PT3 = xyz5 - (num4 / 2.0 + width) * this._view.RightDirection - (maxHeight * 1.5 + num2 / 2.0) * this._view.UpDirection;
    XYZ Center_PT4 = xyz5 + (this.SFBoundsManager.Width - num4 / 2.0 + width) * this._view.RightDirection - (maxHeight * 1.5 + num2 / 2.0) * this._view.UpDirection;
    squareDict[SquarePlacement.Top] = squareDict[SquarePlacement.Top].OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (square => square.projectedX)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.Bottom] = squareDict[SquarePlacement.Bottom].OrderByDescending<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (square => square.projectedX)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.Left] = squareDict[SquarePlacement.Left].OrderByDescending<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (square => square.projectedY)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.Right] = squareDict[SquarePlacement.Right].OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ThenBy<AutoTicketPlacementSquare, double>((Func<AutoTicketPlacementSquare, double>) (square => square.projectedY)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.TopLeft].Add(new AutoTicketPlacementSquare(this._view, Center_PT1, width, layer, SquarePlacement.TopLeft));
    squareDict[SquarePlacement.TopRight].Add(new AutoTicketPlacementSquare(this._view, Center_PT2, width, layer, SquarePlacement.TopRight));
    squareDict[SquarePlacement.BottomLeft].Add(new AutoTicketPlacementSquare(this._view, Center_PT3, width, layer, SquarePlacement.BottomLeft));
    squareDict[SquarePlacement.BottomRight].Add(new AutoTicketPlacementSquare(this._view, Center_PT4, width, layer, SquarePlacement.BottomRight));
    squareDict[SquarePlacement.TopLeft] = squareDict[SquarePlacement.TopLeft].OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.TopRight] = squareDict[SquarePlacement.TopRight].OrderBy<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.BottomLeft] = squareDict[SquarePlacement.BottomLeft].OrderByDescending<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ToList<AutoTicketPlacementSquare>();
    squareDict[SquarePlacement.BottomRight] = squareDict[SquarePlacement.BottomRight].OrderByDescending<AutoTicketPlacementSquare, int>((Func<AutoTicketPlacementSquare, int>) (sq => sq.layer)).ToList<AutoTicketPlacementSquare>();
  }
}
