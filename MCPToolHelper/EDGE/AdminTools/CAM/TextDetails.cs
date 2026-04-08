// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.TextDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.AdminTools.CAM;

internal class TextDetails
{
  public XYZ Location;
  private string _text;

  public string Text
  {
    get => this._text;
    set
    {
      if (value == null || value.Equals(this._text))
        return;
      this._text = string.IsNullOrEmpty(value) ? "-" : value;
    }
  }
}
