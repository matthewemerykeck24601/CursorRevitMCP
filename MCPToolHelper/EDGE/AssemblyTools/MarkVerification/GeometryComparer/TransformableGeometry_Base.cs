// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.TransformableGeometry_Base
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

internal class TransformableGeometry_Base
{
  private MKTolerances _toleranceComparer;

  public List<TransformableSolid> Solids { get; protected set; }

  public TransformableGeometry_Base(FamilyInstance familyInstance, MKTolerances toleranceComparer)
  {
    this._toleranceComparer = toleranceComparer;
    this.FillSolids(familyInstance);
  }

  protected void FillSolids(FamilyInstance familyInstance)
  {
    this.Solids = new List<TransformableSolid>();
    foreach (Solid instanceSolid in Utils.GeometryUtils.Solids.GetInstanceSolids((Element) familyInstance))
      this.Solids.Add(new TransformableSolid(instanceSolid, this._toleranceComparer));
  }

  protected void TransformGeometry(Transform transform)
  {
    foreach (TransformableSolid solid in this.Solids)
      solid.TransformSolid(transform);
  }
}
