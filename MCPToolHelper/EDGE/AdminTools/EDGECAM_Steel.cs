// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.EDGECAM_Steel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AdminTools;

public class EDGECAM_Steel
{
  public List<Tuple<double, List<Curve>>> Bars { get; set; }

  public string Name { get; set; }

  public double BendRadius { get; set; }

  public BoundingBoxXYZ Bbox { get; set; }

  public bool bMesh { get; set; }

  public EDGECAM_Steel()
  {
  }

  public EDGECAM_Steel(
    List<Tuple<double, List<Curve>>> bars,
    string name,
    double bendRadius,
    BoundingBoxXYZ bbox,
    bool mesh)
  {
    this.Bars = bars;
    this.Name = name;
    this.BendRadius = bendRadius;
    this.Bbox = bbox;
    this.bMesh = mesh;
  }
}
