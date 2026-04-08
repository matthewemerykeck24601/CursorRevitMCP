// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.ElementReferenceInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System.Collections.Generic;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class ElementReferenceInfo
{
  public ElementId elemId;
  public SortingInfoCloneTicketElement type = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
  public List<ReferencedPoint> referencedPoints = new List<ReferencedPoint>();
  public ReferencedPoint lowerBound;
  public ReferencedPoint upperBound;
  public bool Source;
  public List<ReferencedPoint> sourceReferences = new List<ReferencedPoint>();

  public ElementReferenceInfo(
    ElementId elementId,
    SortingInfoCloneTicketElement likeType,
    List<ReferencedPoint> referencedPointList,
    ReferencedPoint min,
    ReferencedPoint max)
  {
    this.elemId = elementId;
    this.type = likeType;
    this.referencedPoints.AddRange((IEnumerable<ReferencedPoint>) referencedPointList);
    this.lowerBound = min;
    this.upperBound = max;
  }

  public ElementReferenceInfo(
    ElementId elementId,
    SortingInfoCloneTicketElement likeType,
    List<ReferencedPoint> referenceList)
  {
    this.elemId = elementId;
    this.type = likeType;
    if (likeType == (SortingInfoCloneTicketElement) null)
      this.type = new SortingInfoCloneTicketElement("", FormLocation.None);
    this.sourceReferences.AddRange((IEnumerable<ReferencedPoint>) referenceList);
    this.Source = true;
  }
}
