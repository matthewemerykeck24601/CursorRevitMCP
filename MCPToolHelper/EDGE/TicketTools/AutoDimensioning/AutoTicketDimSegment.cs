// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketDimSegment
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class AutoTicketDimSegment
{
  public XYZ direction;
  public XYZ orthoDirection;
  public XYZ position;
  public DimensionSegment segment;
  public bool hasLeader;
  public double value;
  public int numOffsets;
  public List<AutoTicketDimSegment> EQ_Group;
  public double fontSize;
  public double textOffset;
  public double textWidth;
  public double verticalOffset;
  public double firstOffset;
  private double lateralOffset;
  public XYZ text_startPoint;
  public XYZ text_endPoint;
  public DetailCurve detailLine;
  public bool bHalfUpset;
  public bool bHandled;
  public AutoTicketDimSegment.UpsetDir presumedDir;
  private View view;

  public AutoTicketDimSegment()
  {
  }

  public AutoTicketDimSegment(
    Dimension dim,
    DimensionSegment dimSeg,
    XYZ direction,
    bool runningDimStyle,
    bool equalToPrev = false)
  {
    this.view = dim.View;
    this.fontSize = dim.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
    this.textOffset = dim.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE).AsDouble();
    this.segment = dimSeg;
    this.direction = direction;
    this.orthoDirection = direction.CrossProduct(this.view.ViewDirection.Negate()).Normalize();
    this.position = this.segment.TextPosition;
    this.value = this.segment.Value.Value;
    this.verticalOffset = (this.fontSize + this.textOffset) * (double) this.view.Scale * 1.2;
    this.firstOffset = (this.fontSize * 1.5 + this.textOffset) * (double) this.view.Scale * 1.2;
    if (!runningDimStyle)
    {
      this.textWidth = Math.Abs(this.segment.TextPosition.DotProduct(direction) - this.segment.LeaderEndPosition.DotProduct(direction)) * 2.0;
      this.lateralOffset = (this.fontSize + this.textOffset) * 0.75 * (double) this.view.Scale + this.value / 2.0 + this.textWidth / 2.0;
      this.text_startPoint = this.segment.TextPosition - direction * this.textWidth / 2.0;
      this.text_endPoint = this.segment.TextPosition + direction * this.textWidth / 2.0;
      this.hasLeader = this.textWidth > this.value * 0.9;
    }
    else
    {
      this.textWidth = 0.0;
      this.lateralOffset = 0.0;
      this.text_startPoint = dimSeg.Origin;
      this.text_endPoint = dimSeg.Origin;
      this.hasLeader = false;
    }
    this.numOffsets = 0;
  }

  public double CalcTextWidth()
  {
    return Math.Abs(this.segment.TextPosition.DotProduct(this.direction) - this.segment.LeaderEndPosition.DotProduct(this.direction)) * 2.0;
  }

  private static bool IsEqualWithinTolerance(double factor, double val1, double val2)
  {
    return Math.Abs(val1 - val2) < factor;
  }

  public void setTextPosition(XYZ newPos)
  {
    this.position = newPos;
    this.segment.TextPosition = newPos;
    this.value = this.segment.Value.Value;
    this.text_startPoint = this.position - this.direction * this.textWidth / 2.0;
    this.text_endPoint = this.position + this.direction * this.textWidth / 2.0;
  }

  public static void HandleUpsets(
    double width,
    double midProj,
    List<AutoTicketDimSegment> dimSegs,
    DimensionEdge side,
    double roundingFactor)
  {
    Dictionary<AutoTicketDimSegment, AutoTicketDimSegment.UpsetDir> dictionary = new Dictionary<AutoTicketDimSegment, AutoTicketDimSegment.UpsetDir>();
    List<List<AutoTicketDimSegment>> ticketDimSegmentListList1 = new List<List<AutoTicketDimSegment>>();
    List<AutoTicketDimSegment> source1 = new List<AutoTicketDimSegment>();
    int index = 0;
    while (index < dimSegs.Count)
    {
      if (dimSegs[index].hasLeader)
      {
        List<AutoTicketDimSegment> ticketDimSegmentList1 = new List<AutoTicketDimSegment>();
        do
        {
          ticketDimSegmentList1.Add(dimSegs[index]);
          ++index;
        }
        while (index < dimSegs.Count && dimSegs[index].hasLeader);
        if (ticketDimSegmentList1.Count > 1)
        {
          List<AutoTicketDimSegment> first = new List<AutoTicketDimSegment>();
          List<AutoTicketDimSegment> second = new List<AutoTicketDimSegment>();
          List<AutoTicketDimSegment> ticketDimSegmentList2 = new List<AutoTicketDimSegment>();
          ticketDimSegmentList1.OrderBy<AutoTicketDimSegment, double>((Func<AutoTicketDimSegment, double>) (seg => seg.direction.DotProduct(seg.position)));
          List<List<AutoTicketDimSegment>> source2 = new List<List<AutoTicketDimSegment>>();
          List<AutoTicketDimSegment> source3 = new List<AutoTicketDimSegment>();
          string str = "";
          foreach (AutoTicketDimSegment ticketDimSegment in ticketDimSegmentList1)
          {
            if (ticketDimSegment.segment.ValueString.Equals(str))
            {
              source3.Add(ticketDimSegment);
            }
            else
            {
              if (source3.Count > 0)
                source2.Add(source3.ToList<AutoTicketDimSegment>());
              source3.Clear();
              source3.Add(ticketDimSegment);
              str = ticketDimSegment.segment.ValueString;
            }
          }
          source2.Add(source3.ToList<AutoTicketDimSegment>());
          foreach (List<AutoTicketDimSegment> ticketDimSegmentList3 in source2)
          {
            foreach (AutoTicketDimSegment ticketDimSegment in ticketDimSegmentList3)
              ticketDimSegment.EQ_Group = ticketDimSegmentList3;
          }
          if (source2.Count == 1)
          {
            source1.AddRange((IEnumerable<AutoTicketDimSegment>) source2.First<List<AutoTicketDimSegment>>());
          }
          else
          {
            foreach (List<AutoTicketDimSegment> ticketDimSegmentList4 in source2)
            {
              AutoTicketDimSegment.UpsetDir upsetDir = source2.IndexOf(ticketDimSegmentList4) < source2.Count / 2 ? AutoTicketDimSegment.UpsetDir.Left : AutoTicketDimSegment.UpsetDir.Right;
              foreach (AutoTicketDimSegment key in ticketDimSegmentList4)
              {
                dictionary.Add(key, upsetDir);
                (upsetDir == AutoTicketDimSegment.UpsetDir.Left ? first : second).Add(key);
              }
            }
            second.Reverse();
            List<AutoTicketDimSegment> list = first.Union<AutoTicketDimSegment>((IEnumerable<AutoTicketDimSegment>) second).ToList<AutoTicketDimSegment>();
            if (source2.Count > 1)
              ticketDimSegmentListList1.Add(list);
          }
        }
        else
          source1.AddRange((IEnumerable<AutoTicketDimSegment>) ticketDimSegmentList1);
      }
      else
      {
        source1.Add(dimSegs[index]);
        ++index;
      }
    }
    List<AutoTicketDimSegment> firstHalf = source1.Where<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => seg.direction.DotProduct(seg.position) < midProj)).ToList<AutoTicketDimSegment>();
    List<AutoTicketDimSegment> list1 = source1.Where<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => !firstHalf.Contains(seg))).Reverse<AutoTicketDimSegment>().ToList<AutoTicketDimSegment>();
    List<AutoTicketDimSegment> list2 = new List<AutoTicketDimSegment>().Union<AutoTicketDimSegment>(firstHalf.Union<AutoTicketDimSegment>((IEnumerable<AutoTicketDimSegment>) list1)).ToList<AutoTicketDimSegment>();
    List<List<AutoTicketDimSegment>> ticketDimSegmentListList2 = new List<List<AutoTicketDimSegment>>();
    List<List<AutoTicketDimSegment>> ticketDimSegmentListList3 = new List<List<AutoTicketDimSegment>>();
    foreach (List<AutoTicketDimSegment> source4 in ticketDimSegmentListList1)
    {
      int num = source4.Count<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => seg.direction.DotProduct(seg.position) < midProj));
      (source4.Count<AutoTicketDimSegment>((Func<AutoTicketDimSegment, bool>) (seg => seg.direction.DotProduct(seg.position) >= midProj)) >= num ? ticketDimSegmentListList3 : ticketDimSegmentListList2).Add(source4);
    }
    ticketDimSegmentListList3.Reverse();
    foreach (List<AutoTicketDimSegment> second in ticketDimSegmentListList2)
      list2 = list2.Union<AutoTicketDimSegment>((IEnumerable<AutoTicketDimSegment>) second).ToList<AutoTicketDimSegment>();
    foreach (List<AutoTicketDimSegment> second in ticketDimSegmentListList3)
      list2 = list2.Union<AutoTicketDimSegment>((IEnumerable<AutoTicketDimSegment>) second).ToList<AutoTicketDimSegment>();
    list2.Add((AutoTicketDimSegment) new DimSegmentDummy(midProj + width / 2.0, midProj + width));
    list2.Add((AutoTicketDimSegment) new DimSegmentDummy(midProj - width, midProj - width / 2.0));
    foreach (AutoTicketDimSegment key in list2)
    {
      if (!key.bHandled && key.hasLeader)
      {
        AutoTicketDimSegment.UpsetDir dir = firstHalf.Contains(key) ? AutoTicketDimSegment.UpsetDir.Left : AutoTicketDimSegment.UpsetDir.Right;
        bool bForceDir = dictionary.ContainsKey(key);
        if (bForceDir)
          dir = dictionary[key];
        double calcHeight;
        AutoTicketDimSegment.UpsetDir upsetDir = key.CheckUpsetHt(list2, dir, out calcHeight, bForceDir);
        key.presumedDir = upsetDir;
        XYZ xyz1;
        switch (upsetDir)
        {
          case AutoTicketDimSegment.UpsetDir.Left:
            xyz1 = key.direction.Negate();
            break;
          case AutoTicketDimSegment.UpsetDir.EQ:
            continue;
          default:
            xyz1 = key.direction;
            break;
        }
        XYZ xyz2 = xyz1;
        double num = calcHeight == -1.0 ? key.verticalOffset / 2.0 : calcHeight + key.verticalOffset;
        key.setTextPosition(key.position + xyz2 * key.lateralOffset + key.orthoDirection * num);
        key.bHandled = true;
        if (key.EQ_Group != null && key.EQ_Group.Count > 1)
        {
          foreach (AutoTicketDimSegment ticketDimSegment in key.EQ_Group)
          {
            if (ticketDimSegment != key)
            {
              ticketDimSegment.setTextPosition(key.position);
              ticketDimSegment.presumedDir = key.presumedDir;
              ticketDimSegment.bHandled = true;
            }
          }
        }
      }
    }
    list2.RemoveAll((Predicate<AutoTicketDimSegment>) (seg => seg is DimSegmentDummy));
    List<AutoTicketDimSegment> list3 = list2.OrderByDescending<AutoTicketDimSegment, double>((Func<AutoTicketDimSegment, double>) (seg => Math.Abs(seg.direction.DotProduct(seg.position) - midProj))).ToList<AutoTicketDimSegment>();
    list3.ForEach((Action<AutoTicketDimSegment>) (seg => seg.bHandled = false));
    double num1 = midProj - width / 2.0;
    double num2 = midProj + width / 2.0;
    list3.Add((AutoTicketDimSegment) new DimSegmentDummy(midProj + width / 2.0, midProj + width));
    list3.Add((AutoTicketDimSegment) new DimSegmentDummy(midProj - width, midProj - width / 2.0));
    foreach (AutoTicketDimSegment ticketDimSegment1 in list3)
    {
      if (!ticketDimSegment1.bHandled && ticketDimSegment1.hasLeader)
      {
        AutoTicketDimSegment.UpsetDir presumedDir = ticketDimSegment1.presumedDir;
        double calcHeight;
        AutoTicketDimSegment.UpsetDir upsetDir = ticketDimSegment1.CheckUpsetHt(list3, presumedDir, out calcHeight, rePass: true);
        XYZ xyz3;
        switch (upsetDir)
        {
          case AutoTicketDimSegment.UpsetDir.Left:
            xyz3 = ticketDimSegment1.direction.Negate();
            break;
          case AutoTicketDimSegment.UpsetDir.EQ:
            continue;
          default:
            xyz3 = ticketDimSegment1.direction;
            break;
        }
        XYZ xyz4 = xyz3;
        double num3 = calcHeight + ticketDimSegment1.verticalOffset <= 0.0 ? 0.0 : calcHeight + ticketDimSegment1.verticalOffset;
        if (ticketDimSegment1.presumedDir == upsetDir)
          ticketDimSegment1.setTextPosition(ticketDimSegment1.position + ticketDimSegment1.orthoDirection * num3);
        else
          ticketDimSegment1.setTextPosition(ticketDimSegment1.position + xyz4 * ticketDimSegment1.lateralOffset * 2.0 + ticketDimSegment1.orthoDirection * num3);
        if (ticketDimSegment1.text_startPoint != null && ticketDimSegment1.text_endPoint != null)
        {
          double num4 = ticketDimSegment1.CalcTextWidth() * 0.2;
          double num5 = ticketDimSegment1.direction.DotProduct(ticketDimSegment1.text_startPoint) - num4;
          double num6 = ticketDimSegment1.direction.DotProduct(ticketDimSegment1.text_endPoint) + num4;
          if (num1 >= num5 && num1 <= num6)
            ticketDimSegment1.setTextPosition(ticketDimSegment1.position + ticketDimSegment1.direction * (num1 - num6));
          else if (num2 >= num5 && num2 <= num6)
            ticketDimSegment1.setTextPosition(ticketDimSegment1.position + ticketDimSegment1.direction * (num2 - num5));
        }
        ticketDimSegment1.bHandled = true;
        if (ticketDimSegment1.EQ_Group != null && ticketDimSegment1.EQ_Group.Count > 1)
        {
          foreach (AutoTicketDimSegment ticketDimSegment2 in ticketDimSegment1.EQ_Group)
          {
            if (ticketDimSegment2 != ticketDimSegment1)
            {
              ticketDimSegment2.setTextPosition(ticketDimSegment1.position);
              ticketDimSegment2.bHandled = true;
            }
          }
        }
      }
    }
  }

  private AutoTicketDimSegment.UpsetDir CheckUpsetHt(
    List<AutoTicketDimSegment> dimSegs,
    AutoTicketDimSegment.UpsetDir dir,
    out double calcHeight,
    bool bForceDir = false,
    bool rePass = false)
  {
    calcHeight = 0.0;
    double num1 = this.direction.DotProduct(this.segment.Origin);
    double num2 = this.direction.DotProduct(this.text_startPoint - this.direction * this.lateralOffset);
    double num3 = this.direction.DotProduct(this.text_endPoint - this.direction * this.lateralOffset);
    double num4 = this.direction.DotProduct(this.text_startPoint + this.direction * this.lateralOffset);
    double num5 = this.direction.DotProduct(this.text_endPoint + this.direction * this.lateralOffset);
    if (rePass)
    {
      switch (dir)
      {
        case AutoTicketDimSegment.UpsetDir.Left:
          num2 = this.direction.DotProduct(this.text_startPoint);
          num3 = this.direction.DotProduct(this.text_endPoint);
          num4 = this.direction.DotProduct(this.text_startPoint + this.direction * this.lateralOffset * 2.0);
          num5 = this.direction.DotProduct(this.text_endPoint + this.direction * this.lateralOffset * 2.0);
          break;
        case AutoTicketDimSegment.UpsetDir.Right:
          num2 = this.direction.DotProduct(this.text_startPoint - this.direction * this.lateralOffset * 2.0);
          num3 = this.direction.DotProduct(this.text_endPoint - this.direction * this.lateralOffset * 2.0);
          num4 = this.direction.DotProduct(this.text_startPoint);
          num5 = this.direction.DotProduct(this.text_endPoint);
          break;
      }
    }
    List<double> source1 = new List<double>()
    {
      num2,
      num3,
      num1
    };
    source1.Sort();
    List<double> source2 = new List<double>()
    {
      num4,
      num5,
      num1
    };
    source2.Sort();
    Line bound1 = Line.CreateBound(new XYZ(source1.First<double>(), 0.0, 0.0), new XYZ(source1.Last<double>(), 0.0, 0.0));
    Line bound2 = Line.CreateBound(new XYZ(source2.First<double>(), 0.0, 0.0), new XYZ(source2.Last<double>(), 0.0, 0.0));
    dimSegs.IndexOf(this);
    AutoTicketDimSegment.UpsetDir upsetDir1 = dir;
    XYZ xyz1 = this.position;
    XYZ xyz2 = this.position;
    AutoTicketDimSegment ticketDimSegment1 = (AutoTicketDimSegment) null;
    AutoTicketDimSegment ticketDimSegment2 = (AutoTicketDimSegment) null;
    double num6 = double.MaxValue;
    if (this.detailLine != null)
      num6 = this.detailLine.Document.Application.ShortCurveTolerance;
    foreach (AutoTicketDimSegment dimSeg in dimSegs)
    {
      if (!this.Equals((object) dimSeg))
      {
        switch (dimSeg)
        {
          case DimSegmentDummy _ when !rePass:
            double textStart = (dimSeg as DimSegmentDummy).text_start;
            double textEnd = (dimSeg as DimSegmentDummy).text_end;
            XYZ endpoint1 = new XYZ(textStart, 0.0, 0.0);
            XYZ xyz3 = new XYZ(textEnd, 0.0, 0.0);
            if (endpoint1.DistanceTo(xyz3) >= num6)
            {
              Line bound3 = Line.CreateBound(endpoint1, xyz3);
              if (bound1.Intersect((Curve) bound3) != SetComparisonResult.Disjoint && ticketDimSegment1 == null)
              {
                ticketDimSegment1 = dimSeg;
                xyz1 = this.segment.Origin;
              }
              if (bound2.Intersect((Curve) bound3) != SetComparisonResult.Disjoint && ticketDimSegment2 == null)
              {
                ticketDimSegment2 = dimSeg;
                xyz2 = this.segment.Origin;
                continue;
              }
              continue;
            }
            continue;
          case DimSegmentDummy _:
            continue;
          default:
            List<double> source3 = new List<double>()
            {
              this.direction.DotProduct(dimSeg.text_startPoint),
              this.direction.DotProduct(dimSeg.text_endPoint),
              this.direction.DotProduct(dimSeg.segment.Origin)
            };
            source3.Sort();
            Line bound4 = Line.CreateBound(new XYZ(source3.First<double>(), 0.0, 0.0), new XYZ(source3.Last<double>(), 0.0, 0.0));
            if (bound1.Intersect((Curve) bound4) != SetComparisonResult.Disjoint && this.direction.DotProduct(dimSeg.segment.Origin) < this.direction.DotProduct(this.segment.Origin) && (ticketDimSegment1 == null || xyz1.DotProduct(this.orthoDirection) <= dimSeg.position.DotProduct(this.orthoDirection)))
            {
              ticketDimSegment1 = dimSeg;
              xyz1 = ticketDimSegment1.position;
            }
            if (bound2.Intersect((Curve) bound4) != SetComparisonResult.Disjoint && this.direction.DotProduct(dimSeg.segment.Origin) > this.direction.DotProduct(this.segment.Origin) && (ticketDimSegment2 == null || xyz2.DotProduct(this.orthoDirection) <= dimSeg.position.DotProduct(this.orthoDirection)))
            {
              ticketDimSegment2 = dimSeg;
              xyz2 = ticketDimSegment2.position;
              continue;
            }
            continue;
        }
      }
    }
    double num7;
    switch (ticketDimSegment2)
    {
      case null:
      case DimSegmentDummy _:
        num7 = -1.0;
        break;
      default:
        num7 = xyz2.DotProduct(this.orthoDirection) - this.position.DotProduct(this.orthoDirection);
        break;
    }
    double num8 = num7;
    double num9;
    switch (ticketDimSegment1)
    {
      case null:
      case DimSegmentDummy _:
        num9 = -1.0;
        break;
      default:
        num9 = xyz1.DotProduct(this.orthoDirection) - this.position.DotProduct(this.orthoDirection);
        break;
    }
    double num10 = num9;
    if (rePass)
    {
      double num11;
      switch (ticketDimSegment2)
      {
        case null:
        case DimSegmentDummy _:
          num11 = -1.0;
          break;
        default:
          num11 = xyz2.DotProduct(this.orthoDirection) - this.position.DotProduct(this.orthoDirection);
          break;
      }
      num8 = num11;
      double num12;
      switch (ticketDimSegment1)
      {
        case null:
        case DimSegmentDummy _:
          num12 = -1.0;
          break;
        default:
          num12 = xyz1.DotProduct(this.orthoDirection) - this.position.DotProduct(this.orthoDirection);
          break;
      }
      num10 = num12;
    }
    if (bForceDir)
    {
      calcHeight = upsetDir1 == AutoTicketDimSegment.UpsetDir.Left ? num10 : num8;
      calcHeight = calcHeight.ApproximatelyEquals(0.0) ? 0.0 : calcHeight;
      if (this.EQ_Group != null && this.EQ_Group.Count != 1 && this.EQ_Group.Count != 0)
      {
        if (dir == AutoTicketDimSegment.UpsetDir.Right)
        {
          if (num1 != this.EQ_Group.Max<AutoTicketDimSegment>((Func<AutoTicketDimSegment, double>) (seg => this.direction.DotProduct(seg.segment.Origin))))
            return AutoTicketDimSegment.UpsetDir.EQ;
        }
        else if (num1 != this.EQ_Group.Min<AutoTicketDimSegment>((Func<AutoTicketDimSegment, double>) (seg => this.direction.DotProduct(seg.segment.Origin))))
          return AutoTicketDimSegment.UpsetDir.EQ;
      }
      return upsetDir1;
    }
    if (ticketDimSegment2 == null || ticketDimSegment1 == null)
    {
      calcHeight = -1.0;
      return dir != AutoTicketDimSegment.UpsetDir.Left ? (ticketDimSegment2 != null ? AutoTicketDimSegment.UpsetDir.Left : dir) : (ticketDimSegment1 != null ? AutoTicketDimSegment.UpsetDir.Right : dir);
    }
    if (num8.ApproximatelyEquals(num10))
    {
      calcHeight = num8.ApproximatelyEquals(0.0) ? 0.0 : num8;
      if (rePass && upsetDir1 == this.presumedDir)
      {
        if (upsetDir1 == AutoTicketDimSegment.UpsetDir.Left && !ticketDimSegment1.bHandled)
          calcHeight = -1.0;
        else if (upsetDir1 == AutoTicketDimSegment.UpsetDir.Right && !ticketDimSegment2.bHandled)
          calcHeight = -1.0;
      }
      return upsetDir1;
    }
    calcHeight = Math.Min(num10, num8);
    calcHeight = calcHeight.ApproximatelyEquals(0.0) ? 0.0 : calcHeight;
    AutoTicketDimSegment.UpsetDir upsetDir2 = num10 < num8 ? AutoTicketDimSegment.UpsetDir.Left : AutoTicketDimSegment.UpsetDir.Right;
    if (rePass && upsetDir2 == this.presumedDir)
    {
      if (upsetDir2 == AutoTicketDimSegment.UpsetDir.Left && !ticketDimSegment1.bHandled)
        calcHeight = -1.0;
      else if (upsetDir2 == AutoTicketDimSegment.UpsetDir.Right && !ticketDimSegment2.bHandled)
        calcHeight = -1.0;
    }
    if (this.EQ_Group != null && this.EQ_Group.Count != 1 && this.EQ_Group.Count != 0)
    {
      if (upsetDir2 == AutoTicketDimSegment.UpsetDir.Right)
      {
        if (num1 != this.EQ_Group.Max<AutoTicketDimSegment>((Func<AutoTicketDimSegment, double>) (seg => this.direction.DotProduct(seg.segment.Origin))))
          return AutoTicketDimSegment.UpsetDir.EQ;
      }
      else if (num1 != this.EQ_Group.Min<AutoTicketDimSegment>((Func<AutoTicketDimSegment, double>) (seg => this.direction.DotProduct(seg.segment.Origin))))
        return AutoTicketDimSegment.UpsetDir.EQ;
    }
    return upsetDir2;
  }

  public enum UpsetDir
  {
    Left,
    Right,
    EQ,
  }
}
