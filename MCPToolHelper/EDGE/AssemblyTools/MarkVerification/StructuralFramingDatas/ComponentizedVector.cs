// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.StructuralFramingDatas.ComponentizedVector
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.StructuralFramingDatas;

public class ComponentizedVector
{
  public double[] _angles;

  public double Magnitude { get; }

  public bool Matched { get; set; }

  public ComponentizedVector(double magnitude)
  {
    this.Magnitude = magnitude;
    this._angles = new double[3];
  }

  public ComponentizedVector(double magnitude, double xAngle, double yAngle, double zAngle)
  {
    this.Magnitude = magnitude;
    this._angles = new double[3]{ xAngle, yAngle, zAngle };
  }

  public double Xcoord => this._angles[0] * this.Magnitude;

  public double Ycoord => this._angles[1] * this.Magnitude;

  public double Zcoord => this._angles[2] * this.Magnitude;

  public XYZ GetPoint() => new XYZ(this.Xcoord, this.Ycoord, this.Zcoord);

  public override int GetHashCode()
  {
    return Convert.ToInt32(Math.Truncate(Math.Round(this.Magnitude, 4, MidpointRounding.AwayFromZero)));
  }

  public override bool Equals(object obj)
  {
    return obj is ComponentizedVector componentizedVector && componentizedVector.Magnitude.ApproximatelyEquals(this.Magnitude, 0.001) && componentizedVector.Xcoord.ApproximatelyEquals(this.Xcoord, 0.001) && componentizedVector.Ycoord.ApproximatelyEquals(this.Ycoord, 0.001) && componentizedVector.Zcoord.ApproximatelyEquals(this.Zcoord, 0.001);
  }

  public bool IsWithinTolerance(ComponentizedVector otherVector, double tolerance)
  {
    return otherVector.Xcoord.ApproximatelyEquals(this.Xcoord, tolerance) && otherVector.Ycoord.ApproximatelyEquals(this.Ycoord, tolerance) && otherVector.Zcoord.ApproximatelyEquals(this.Zcoord, tolerance);
  }
}
