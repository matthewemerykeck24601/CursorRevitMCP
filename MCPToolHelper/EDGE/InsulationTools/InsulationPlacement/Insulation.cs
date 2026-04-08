// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.Insulation
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement;

public class Insulation
{
  public double InsulLength;
  public double InsulWidth;
  public double InsulThickness;
  public double MaxLength;
  public double MaxWidth;
  public double MinLength;
  public double MinWidth;
  public double WallLength;
  public double WallWidth;
  public double WallSide1SolidZone;
  public double WallSide2SolidZone;
  public double WallTopSolidZone;
  public double WallBottomSolidZone;
  public string manufComponent = "INSULATION";
  public string InsulPlacementLocation;
  public double WytheOuter;
  public double WytheInner;
  public string InsulWytheInsulation;
  public string InsulDefault;
  public XYZ DirectionVector;
  public PlanarFace Face;
  public FamilyInstance WallInstance;
  public List<Solid> InsulSolids;
  public List<Solid> WallSolids;
  public List<Solid> InvalidInsulSolids;
  public Solid UnionedSolid;
  public PlanarFace UnionedFace;
  public bool BSymbol;
  public bool is2DView;
  public bool IsMirrored;
  public PlanarFace loopFace;
  public Document RevitDoc;
  public Transform WallTransform;
  public bool OutOfBounds;
  public Solid IntersectionSolid;
  public XYZ BasisY;
  public bool ExpandedToMin;
  public bool ExpandedToMax;
  public double toleranceLine;
  public static ICollection<ElementId> Addons;

  public Insulation(Document revitDoc, Element wall)
  {
    this.InsulSolids = new List<Solid>();
    this.WallSolids = new List<Solid>();
    this.InvalidInsulSolids = new List<Solid>();
    this.RevitDoc = revitDoc;
    if (revitDoc.ActiveView.ViewType != ViewType.ThreeD)
      this.is2DView = true;
    this.OutOfBounds = false;
    this.ExpandedToMin = false;
    this.ExpandedToMax = false;
    this.WallInstance = wall as FamilyInstance;
    this.WallTransform = this.WallInstance.GetTransform();
    this.IsMirrored = this.WallInstance.FacingFlipped || this.WallInstance.Mirrored;
  }
}
