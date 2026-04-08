// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.SimpleVector
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class SimpleVector
{
  public double X;
  public double Y;
  public double Z;

  public SimpleVector(double x, double y, double z)
  {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public SimpleVector()
  {
    this.X = 0.0;
    this.Y = 0.0;
    this.Z = 0.0;
  }

  public SimpleVector(XYZ RevitVector)
  {
    this.X = RevitVector.X;
    this.Y = RevitVector.Y;
    this.Z = RevitVector.Z;
  }

  public static SimpleVector operator +(SimpleVector sv1, SimpleVector sv2)
  {
    return new SimpleVector(sv1.X + sv2.X, sv1.Y + sv2.Y, sv1.Z + sv2.Z);
  }

  public static SimpleVector operator +(SimpleVector sv1, XYZ xyz2)
  {
    return new SimpleVector(sv1.X + xyz2.X, sv1.Y + xyz2.Y, sv1.Z + xyz2.Z);
  }

  public static XYZ operator +(XYZ xyz1, SimpleVector sv2)
  {
    return new XYZ(xyz1.X + sv2.X, xyz1.Y + sv2.Y, xyz1.Z + sv2.Z);
  }

  public override string ToString()
  {
    return $"sv: ({this.X.ToString()} , {this.Y.ToString()} , {this.Z.ToString()})";
  }

  public XYZ GetXYZ() => new XYZ(this.X, this.Y, this.Z);
}
