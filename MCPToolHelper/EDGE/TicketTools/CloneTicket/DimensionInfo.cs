// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.DimensionInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System.Collections.Generic;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class DimensionInfo
{
  public Dimension sourceDimension;
  public List<ReferencedPoint> sourceReferencedPoints = new List<ReferencedPoint>();
  public List<ReferencedPoint> dimReferencesToPlace = new List<ReferencedPoint>();
  public DimensionEdge dimEdge;
  public DimensionEdge oppEdge;
  public bool? topLeft;
  public SortingInfoCloneTicketElement majorityType = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
  public bool allSFRefs;
  public int sfRefCoutner;
  public List<SortingInfoCloneTicketElement> allTypes = new List<SortingInfoCloneTicketElement>();
  public bool lowerSFExtent;
  public bool upperSFExtent;
  public bool overallDimension;
  public bool blockoutDimension;
  public bool multipleTypes;
  public Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> alignmentDictionary = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());

  public DimensionInfo(
    Dimension dimension,
    List<ReferencedPoint> list,
    List<ReferencedPoint> sourceList,
    DimensionEdge edge,
    bool? side,
    SortingInfoCloneTicketElement majority,
    int majorityCount,
    List<SortingInfoCloneTicketElement> types,
    bool allSFrefs,
    int sfCounter,
    bool lowerExtent,
    bool upperExtetnt,
    bool multipleElmeentTypes,
    Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> alignDict,
    bool overall,
    bool blockout)
  {
    this.sourceDimension = dimension;
    this.dimReferencesToPlace.AddRange((IEnumerable<ReferencedPoint>) list);
    this.sourceReferencedPoints.AddRange((IEnumerable<ReferencedPoint>) sourceList);
    this.dimEdge = edge;
    this.topLeft = side;
    this.majorityType = majority;
    this.allTypes.AddRange((IEnumerable<SortingInfoCloneTicketElement>) types);
    this.allSFRefs = allSFrefs;
    this.sfRefCoutner = sfCounter;
    this.lowerSFExtent = lowerExtent;
    this.upperSFExtent = upperExtetnt;
    this.multipleTypes = multipleElmeentTypes;
    this.alignmentDictionary = alignDict;
    this.overallDimension = overall;
    this.blockoutDimension = blockout;
    switch (this.dimEdge)
    {
      case DimensionEdge.Left:
        this.oppEdge = DimensionEdge.Right;
        break;
      case DimensionEdge.Right:
        this.oppEdge = DimensionEdge.Left;
        break;
      case DimensionEdge.Top:
        this.oppEdge = DimensionEdge.Bottom;
        break;
      case DimensionEdge.Bottom:
        this.oppEdge = DimensionEdge.Top;
        break;
      default:
        this.oppEdge = DimensionEdge.None;
        break;
    }
  }
}
