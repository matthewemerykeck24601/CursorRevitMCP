// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.SolidComparison
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class SolidComparison
{
  public Solid globalSolid { get; set; }

  public Solid zeroSolid { get; set; }

  public BoundingBoxXYZ solidBBox { get; set; }

  public FamilyInstance instance { get; set; }

  public MKTolerances toleranceComparer { get; set; }

  public double Volume { get; set; }

  public XYZ CenterPoint { get; set; }

  public SolidComparison(Solid solid, FamilyInstance finstance, MKTolerances tolerances)
  {
    this.globalSolid = solid;
    this.zeroSolid = SolidUtils.CreateTransformed(solid, finstance.GetTransform().Inverse);
    this.instance = finstance;
    this.solidBBox = this.zeroSolid.GetBoundingBox();
    this.toleranceComparer = tolerances;
    this.Volume = solid.Volume;
    this.CenterPoint = this.solidBBox.Transform.OfPoint(Line.CreateBound(this.solidBBox.Min, this.solidBBox.Max).Evaluate(0.5, true));
    this.zeroSolid = SolidUtils.CreateTransformed(this.zeroSolid, Transform.CreateTranslation(this.CenterPoint).Inverse);
  }

  public Solid RotateSolid(XYZ axis, double rad)
  {
    return SolidUtils.CreateTransformed(this.zeroSolid, Transform.CreateRotation(axis, rad));
  }

  public double subtractSolid(Solid other, out Solid subtractedSolid, Solid mySolid = null)
  {
    try
    {
      Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation((GeometryObject) mySolid == (GeometryObject) null ? this.zeroSolid : SolidUtils.Clone(mySolid), other, BooleanOperationsType.Difference);
      subtractedSolid = solid;
      return solid.Volume;
    }
    catch (Exception ex)
    {
      subtractedSolid = (Solid) null;
      return 0.0;
    }
  }
}
