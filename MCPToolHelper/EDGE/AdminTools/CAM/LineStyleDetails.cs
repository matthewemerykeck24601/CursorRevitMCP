// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LineStyleDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.AdminTools.CAM;

public class LineStyleDetails
{
  public string _name { get; set; }

  public int _weight { get; set; }

  public Color _color { get; set; }

  public int _cadColor { get; set; }

  public LineStyleDetails(string name, int weight, Color color, int ACADColor)
  {
    this._name = name;
    this._weight = weight;
    this._color = color;
    this._cadColor = ACADColor;
  }
}
