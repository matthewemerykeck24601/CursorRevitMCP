// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.StructuralFramingBoundsObject
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class StructuralFramingBoundsObject
{
  public double xMin;
  public double xMax;
  public double xMid;
  public double yMin;
  public double yMax;
  public double yMid;
  public double Width;
  public double Height;
  public List<XYZ> outlinePoints;
  public Transform viewTransform;
  private FamilyInstance _sfFamInst;
  private View _view;

  public StructuralFramingBoundsObject(
    AssemblyInstance assemblyInstance,
    View view,
    Element selectedElement)
  {
    StructuralFramingBoundsObject framingBoundsObject = this;
    this._view = view;
    this.viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    this.outlinePoints = new List<XYZ>();
    Document document = assemblyInstance.Document;
    if (document == null)
      return;
    this.outlinePoints = new List<XYZ>();
    BoundingBoxXYZ bb1 = new BoundingBoxXYZ()
    {
      Min = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue),
      Max = new XYZ(double.MinValue, double.MinValue, double.MinValue)
    };
    foreach (ElementId memberId in (IEnumerable<ElementId>) assemblyInstance.GetMemberIds())
    {
      Element element = document.GetElement(memberId);
      if (element != null && element is FamilyInstance)
      {
        FamilyInstance elem = element as FamilyInstance;
        bool bSymbol;
        foreach (Solid symbolSolid in Solids.GetSymbolSolids((Element) elem, out bSymbol))
        {
          Solid solid = symbolSolid;
          if (bSymbol)
            solid = SolidUtils.CreateTransformed(symbolSolid, elem.GetTransform());
          List<XYZ> outlinePoints;
          BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(solid, view, out outlinePoints, bSymbol);
          bb1 = Utils.MiscUtils.MiscUtils.AdjustBBox(bb1, viewBbox);
          this.outlinePoints.AddRange((IEnumerable<XYZ>) outlinePoints);
        }
      }
    }
    if (this.outlinePoints.Count == 0)
    {
      BoundingBoxXYZ bbox = assemblyInstance.get_BoundingBox(view);
      double num1 = bbox.Max.X - bbox.Min.X;
      double num2 = bbox.Max.Y - bbox.Min.Y;
      double height = bbox.Max.Z - bbox.Min.Z;
      this.outlinePoints.Add(bbox.Min);
      this.outlinePoints.Add(bbox.Min + XYZ.BasisX * num1);
      this.outlinePoints.Add(bbox.Min + XYZ.BasisY * num2);
      this.outlinePoints.Add(bbox.Min + XYZ.BasisX * num1 + XYZ.BasisY * num2);
      this.outlinePoints.AddRange((IEnumerable<XYZ>) this.outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (oP => oP + XYZ.BasisZ * height)).ToList<XYZ>());
      bbox.Min = bbox.Transform.OfPoint(bbox.Min);
      bbox.Max = bbox.Transform.OfPoint(bbox.Max);
      this.outlinePoints = this.outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (p => bbox.Transform.OfPoint(p))).ToList<XYZ>();
    }
    this.outlinePoints = this.outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => framingBoundsObject.viewTransform.OfPoint(pt))).ToList<XYZ>();
    this.xMin = this.outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.RightDirection.Normalize())));
    this.xMax = this.outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.RightDirection.Normalize())));
    this.yMin = this.outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.UpDirection.Normalize())));
    this.yMax = this.outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.UpDirection.Normalize())));
    this.xMid = (this.xMin + this.xMax) / 2.0;
    this.yMid = (this.yMin + this.yMax) / 2.0;
    this.Width = Math.Abs(this.xMax - this.xMin);
    this.Height = Math.Abs(this.yMax - this.yMin);
  }

  public StructuralFramingBoundsObject(FamilyInstance sfFamInst, View view, bool cloneTicket = false)
  {
    StructuralFramingBoundsObject framingBoundsObject = this;
    this._sfFamInst = sfFamInst;
    this._view = view;
    this.viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    if (!cloneTicket)
    {
      DimReferencesManager.GetViewBBox(sfFamInst, view, out this.outlinePoints);
      this.outlinePoints = this.outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => framingBoundsObject.viewTransform.OfPoint(pt))).ToList<XYZ>();
      Document document = view.Document;
      this.xMin = this.outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.RightDirection.Normalize())));
      this.xMax = this.outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.RightDirection.Normalize())));
      this.yMin = this.outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.UpDirection.Normalize())));
      this.yMax = this.outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.DotProduct(view.UpDirection.Normalize())));
      this.xMid = (this.xMin + this.xMax) / 2.0;
      this.yMid = (this.yMin + this.yMax) / 2.0;
    }
    else
    {
      this.outlinePoints = this.CloneTicketFamilyOutLinePoints(sfFamInst, view);
      if (this.outlinePoints.Count > 0)
      {
        this.xMin = this.outlinePoints.Select<XYZ, double>((Func<XYZ, double>) (pt => pt.X)).Min();
        this.xMax = this.outlinePoints.Select<XYZ, double>((Func<XYZ, double>) (pt => pt.X)).Max();
        this.yMin = this.outlinePoints.Select<XYZ, double>((Func<XYZ, double>) (pt => pt.Y)).Min();
        this.yMax = this.outlinePoints.Select<XYZ, double>((Func<XYZ, double>) (pt => pt.Y)).Max();
      }
      else
      {
        LocationPoint location = sfFamInst.Location as LocationPoint;
        XYZ xyz = this.viewTransform.Inverse.OfPoint(location.Point);
        if (location != null)
        {
          this.xMin = xyz.X;
          this.xMax = xyz.X;
          this.yMin = xyz.Y;
          this.yMax = xyz.Y;
        }
      }
    }
    this.Width = Math.Abs(this.xMax - this.xMin);
    this.Height = Math.Abs(this.yMax - this.yMin);
  }

  private List<XYZ> CloneTicketFamilyOutLinePoints(FamilyInstance elem, View view)
  {
    List<XYZ> outlinePoints = new List<XYZ>();
    DimReferencesManager.GetViewBBox(elem, view, out outlinePoints);
    if (elem.HasSubComponents())
    {
      foreach (ElementId subComponentId in (IEnumerable<ElementId>) elem.GetSubComponentIds())
      {
        if (elem.Document.GetElement(subComponentId) is FamilyInstance element)
          outlinePoints.AddRange((IEnumerable<XYZ>) this.CloneTicketFamilyOutLinePoints(element, view));
      }
    }
    return outlinePoints;
  }
}
