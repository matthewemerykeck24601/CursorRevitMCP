// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.EDGEFinishComponent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class EDGEFinishComponent
{
  public Wall _finish;
  protected static MKTolerances _toleranceComparer;
  private Document _revitDoc;

  public string HostGuid { get; private set; }

  public ElementId MaterialId { get; private set; }

  public double MaterialVolume_InInternalUnits { get; private set; }

  public bool IsValidForMarkVerification { get; }

  public string FamilyTypeName { get; }

  public string StatusMessage { get; set; }

  public ElementId Id { get; set; }

  public XYZ LocationPoint { get; set; }

  public Curve LocationCurve { get; set; }

  public XYZ OrientationPoint { get; set; }

  public XYZ DirectionVector { get; set; }

  public XYZ BasisX { get; set; }

  public XYZ BasisY { get; set; }

  public XYZ BasisZ { get; set; }

  public EDGEFinishComponent(Wall finish, FamilyInstance host, ComparisonOption compareOption)
  {
    this._revitDoc = finish.Document;
    this._finish = finish;
    EDGEFinishComponent._toleranceComparer = new MKTolerances(compareOption, this._revitDoc);
    this.Id = finish.Id;
    this.FamilyTypeName = finish.Name;
    this.LocationCurve = (finish.Location as Autodesk.Revit.DB.LocationCurve).Curve;
    this.LocationPoint = (finish.Location as Autodesk.Revit.DB.LocationCurve).Curve.Evaluate(0.5, false);
    this.OrientationPoint = this.LocationPoint + finish.Orientation;
    this.DirectionVector = new XYZ(finish.Orientation.X, finish.Orientation.Y, finish.Orientation.Z);
    this.BasisY = new XYZ(this.DirectionVector.X, this.DirectionVector.Y, this.DirectionVector.Z);
    this.BasisZ = XYZ.BasisZ;
    this.BasisX = this.BasisZ.CrossProduct(this.BasisY);
    this.StatusMessage = "";
    this.IsValidForMarkVerification = true;
    this.HostGuid = Parameters.GetParameterAsString((Element) this._finish, "HOST_GUID");
    ICollection<ElementId> materialIds = this._finish.GetMaterialIds(false);
    if (materialIds.Count > 1)
      this.StatusMessage += "Detected multiple materials in addon product.";
    else if (materialIds.Count == 1)
      this.MaterialId = materialIds.First<ElementId>();
    else
      this.StatusMessage += "No materials assigned to this addon.";
    if (this.MaterialId != (ElementId) null && this.MaterialId != ElementId.InvalidElementId)
      this.MaterialVolume_InInternalUnits = this._finish.GetMaterialVolume(this.MaterialId);
    if (Math.Abs(this.MaterialVolume_InInternalUnits) >= 1E-07)
      return;
    this.MaterialVolume_InInternalUnits = this._finish.GetElementVolume();
    if (this.MaterialVolume_InInternalUnits >= 1E-07)
      return;
    this.IsValidForMarkVerification = false;
    QA.LogError("AddonComponentData Ctor", "unable to find addon material volume");
  }

  public void TransformLocation(Transform transform)
  {
    this.LocationCurve = this.LocationCurve.CreateTransformed(transform);
    this.LocationPoint = transform.OfPoint(this.LocationPoint);
    this.OrientationPoint = transform.OfPoint(this.OrientationPoint);
    this.DirectionVector = transform.OfVector(this.DirectionVector);
    this.BasisX = transform.OfVector(this.BasisX);
    this.BasisY = transform.OfVector(this.BasisY);
    this.BasisZ = transform.OfVector(this.BasisZ);
  }
}
