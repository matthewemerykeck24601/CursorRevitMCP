// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.TransformedFace
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;

public class TransformedFace
{
  public PlanarFace Face;
  public XYZ Origin;
  public XYZ Normal;
  public Transform Transform;

  public TransformedFace(Transform t, PlanarFace f)
  {
    this.Transform = t;
    this.Face = f;
    this.Origin = this.Transform.Inverse.OfPoint(this.Face.Origin);
    this.Normal = this.Transform.Inverse.OfVector(this.Face.FaceNormal);
  }

  public TransformedFace(Transform t1, Transform t2, PlanarFace f)
  {
    this.Transform = t2;
    this.Face = f;
    this.Origin = t1.OfPoint(this.Face.Origin);
    this.Origin = t2.Inverse.OfPoint(this.Origin);
    this.Normal = t1.OfVector(this.Face.FaceNormal);
    this.Normal = t2.Inverse.OfVector(this.Normal);
  }
}
