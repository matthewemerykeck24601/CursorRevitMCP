// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.EDGEAssemblyComponent_Base
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using Utils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class EDGEAssemblyComponent_Base
{
  public FamilyInstance inst;
  protected static MKTolerances _toleranceComparer;
  protected bool bIgnorePlateRotationFailure;
  private Document _revitDoc;

  public EDGEAssemblyComponent_Base(
    FamilyInstance componentInstance,
    FamilyInstance hostFamilyInstance,
    ComparisonOption compareOption,
    bool ignorePlateRotation = false)
  {
    this._revitDoc = componentInstance.Document;
    this.bIgnorePlateRotationFailure = ignorePlateRotation;
    EDGEAssemblyComponent_Base._toleranceComparer = new MKTolerances(compareOption, this._revitDoc);
    this.inst = componentInstance;
    this.Id = componentInstance.Id;
    this.StatusMessage = "";
    Location location = componentInstance.Location;
    if (location is Autodesk.Revit.DB.LocationPoint)
    {
      this.LocationPoint = (location as Autodesk.Revit.DB.LocationPoint).Point;
      this.OrientationPoint = this.LocationPoint + componentInstance.FacingOrientation * 5.0;
    }
    this.DirectionVector = new XYZ(componentInstance.FacingOrientation.X, componentInstance.FacingOrientation.Y, componentInstance.FacingOrientation.Z);
    Transform transform = componentInstance.GetTransform();
    this.BasisX = transform.BasisX.Normalize();
    this.BasisY = transform.BasisY.Normalize();
    this.BasisZ = transform.BasisZ.Normalize();
    if (componentInstance.Symbol != null)
    {
      this.FamilyTypeName = componentInstance.Symbol.FamilyName + componentInstance.Symbol.Name;
    }
    else
    {
      this.FamilyTypeName = "---";
      QAUtils.DebugMessage($"EDGEAssemblyComponent_Base Constructor: Found Addon: {componentInstance.Id?.ToString()}-{componentInstance.Name} with no symbol.");
    }
  }

  public void TransformLocation(Transform transform)
  {
    this.LocationPoint = transform.OfPoint(this.LocationPoint);
    this.OrientationPoint = transform.OfPoint(this.OrientationPoint);
    this.DirectionVector = transform.OfVector(this.DirectionVector);
    this.BasisX = transform.OfVector(this.BasisX);
    this.BasisY = transform.OfVector(this.BasisY);
    this.BasisZ = transform.OfVector(this.BasisZ);
  }

  public string FamilyTypeName { get; }

  public string StatusMessage { get; set; }

  public ElementId Id { get; set; }

  public XYZ LocationPoint { get; set; }

  public XYZ OrientationPoint { get; set; }

  public XYZ DirectionVector { get; set; }

  public XYZ BasisX { get; set; }

  public XYZ BasisY { get; set; }

  public XYZ BasisZ { get; set; }

  public virtual LocationCompareResult Matches(EDGEAssemblyComponent_Base other)
  {
    return this.Matches(other, MKToleranceAspect.EmbedPosition);
  }

  public virtual LocationCompareResult Matches(
    EDGEAssemblyComponent_Base other,
    MKToleranceAspect aspect)
  {
    if (this.FamilyTypeName != other.FamilyTypeName)
      return LocationCompareResult.Failed;
    bool flag1 = EDGEAssemblyComponent_Base._toleranceComparer.NewPointsAreEqual(this.LocationPoint, other.LocationPoint, aspect);
    int num1 = EDGEAssemblyComponent_Base._toleranceComparer.DeltaWithinTolerance(this.BasisX.AngleTo(other.BasisX), MKToleranceAspect.Angle) ? 1 : 0;
    bool flag2 = EDGEAssemblyComponent_Base._toleranceComparer.DeltaWithinTolerance(this.BasisY.AngleTo(other.BasisY), MKToleranceAspect.Angle);
    bool flag3 = EDGEAssemblyComponent_Base._toleranceComparer.DeltaWithinTolerance(this.BasisZ.AngleTo(other.BasisZ), MKToleranceAspect.Angle);
    int num2 = flag2 ? 1 : 0;
    int num3 = num1 & num2 & (flag3 ? 1 : 0);
    bool flag4 = false;
    if (this.inst.Mirrored && !other.inst.Mirrored || !this.inst.Mirrored && other.inst.Mirrored)
      flag4 = true;
    bool flag5 = num3 != 0;
    if (flag1 & flag4)
      return LocationCompareResult.PlateRotationWarning;
    if (!flag1 || flag1 && !flag5 && !this.bIgnorePlateRotationFailure)
      return LocationCompareResult.Failed;
    return flag1 && !flag5 && this.bIgnorePlateRotationFailure ? LocationCompareResult.PlateRotationWarning : LocationCompareResult.Success;
  }

  public void DrawVisualization()
  {
  }
}
