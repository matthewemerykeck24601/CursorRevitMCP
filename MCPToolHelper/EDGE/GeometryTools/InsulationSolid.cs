// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.InsulationSolid
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.GeometryTools;

public class InsulationSolid
{
  public Solid globalSolid { get; set; }

  public Solid zeroSolid { get; set; }

  public BoundingBoxXYZ solidBBox { get; set; }

  public FamilyInstance insulationInstance { get; set; }

  public MKTolerances toleranceComparer { get; set; }

  public double Volume { get; set; }

  public XYZ CenterPoint { get; set; }

  public XYZ Centroid { get; set; }

  public Dictionary<Type, int> FaceTypeCount { get; set; }

  public InsulationSolid(Solid solid, FamilyInstance insulation, MKTolerances tolerances)
  {
    this.globalSolid = solid;
    this.zeroSolid = SolidUtils.CreateTransformed(solid, insulation.GetTransform().Inverse);
    this.insulationInstance = insulation;
    this.solidBBox = this.zeroSolid.GetBoundingBox();
    this.toleranceComparer = tolerances;
    this.Volume = solid.Volume;
    this.CenterPoint = this.solidBBox.Transform.OfPoint(Line.CreateBound(this.solidBBox.Min, this.solidBBox.Max).Evaluate(0.5, true));
    this.FaceTypeCount = this.CalculateFaceTypeCount();
    this.zeroSolid = SolidUtils.CreateTransformed(this.zeroSolid, Transform.CreateTranslation(this.CenterPoint).Inverse);
    this.Centroid = this.zeroSolid.ComputeCentroid();
  }

  private Dictionary<Type, int> CalculateFaceTypeCount()
  {
    Dictionary<Type, int> faceTypeCount = new Dictionary<Type, int>();
    foreach (Face face in this.globalSolid.Faces)
    {
      if (faceTypeCount.ContainsKey(face.GetType()))
        faceTypeCount[face.GetType()]++;
      else
        faceTypeCount.Add(face.GetType(), 1);
    }
    return faceTypeCount;
  }

  public Solid RotateSolid(XYZ axis, double rad)
  {
    return SolidUtils.CreateTransformed(this.zeroSolid, Transform.CreateRotation(axis, rad));
  }

  public XYZ SolidCentroid(Solid solid) => solid.ComputeCentroid();

  private bool CompareBBox(InsulationSolid other)
  {
    XYZ xyz1 = this.solidBBox.Max - this.solidBBox.Min;
    List<double> list1 = new List<double>()
    {
      xyz1.X,
      xyz1.Y,
      xyz1.Z
    }.OrderByDescending<double, double>((Func<double, double>) (dim => dim)).ToList<double>();
    XYZ xyz2 = other.solidBBox.Max - other.solidBBox.Min;
    List<double> list2 = new List<double>()
    {
      xyz2.X,
      xyz2.Y,
      xyz2.Z
    }.OrderByDescending<double, double>((Func<double, double>) (dim => dim)).ToList<double>();
    for (int index = 0; index < 3; ++index)
    {
      if (!this.toleranceComparer.NewValuesAreEqual(list1[index], list2[index], MKToleranceAspect.Insulation_Geometry))
        return false;
    }
    return true;
  }

  private bool subtractSolid(Solid other)
  {
    try
    {
      double tolerance = this.toleranceComparer.GetTolerance(MKToleranceAspect.Insulation_Volume);
      double num = this.Volume >= other.Volume ? this.Volume * tolerance : other.Volume * tolerance;
      if (BooleanOperationsUtils.ExecuteBooleanOperation(this.zeroSolid, other, BooleanOperationsType.Difference).Volume > num)
        return false;
      if (BooleanOperationsUtils.ExecuteBooleanOperation(other, this.zeroSolid, BooleanOperationsType.Difference).Volume > num)
        return false;
    }
    catch (Exception ex)
    {
      return true;
    }
    return true;
  }

  public bool Equals(InsulationSolid other)
  {
    if (!this.toleranceComparer.NewValuesAreEqual(this.Volume, other.Volume, MKToleranceAspect.Insulation_Volume) || this.FaceTypeCount.Keys.Count != other.FaceTypeCount.Keys.Count || other.FaceTypeCount.Keys.Any<Type>((Func<Type, bool>) (t => !this.FaceTypeCount.Keys.Contains<Type>(t))))
      return false;
    foreach (Type key in this.FaceTypeCount.Keys)
    {
      if (other.FaceTypeCount[key] != this.FaceTypeCount[key])
        return false;
    }
    if (!this.CompareBBox(other))
      return false;
    if (this.toleranceComparer.NewPointsAreEqual(this.Centroid, other.Centroid, MKToleranceAspect.Insulation_Geometry) && this.subtractSolid(other.zeroSolid))
      return true;
    for (double rad = Math.PI / 2.0; rad < 2.0 * Math.PI; rad += Math.PI / 2.0)
    {
      Solid other1 = other.RotateSolid(XYZ.BasisX, rad);
      if (this.toleranceComparer.NewPointsAreEqual(other1.ComputeCentroid(), this.Centroid, MKToleranceAspect.Insulation_Geometry) && this.subtractSolid(other1))
        return true;
      Solid other2 = other.RotateSolid(XYZ.BasisY, rad);
      if (this.toleranceComparer.NewPointsAreEqual(other2.ComputeCentroid(), this.Centroid, MKToleranceAspect.Insulation_Geometry) && this.subtractSolid(other2))
        return true;
      Solid other3 = other.RotateSolid(XYZ.BasisZ, rad);
      if (this.toleranceComparer.NewPointsAreEqual(other3.ComputeCentroid(), this.Centroid, MKToleranceAspect.Insulation_Geometry) && this.subtractSolid(other3))
        return true;
    }
    return false;
  }
}
