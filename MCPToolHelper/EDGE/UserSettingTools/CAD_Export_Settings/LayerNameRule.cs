// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.LayerNameRule
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Globalization;
using System.Windows.Controls;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class LayerNameRule : ValidationRule
{
  public string layerDataGridName { get; set; }

  public override ValidationResult Validate(object value, CultureInfo cultureInfo)
  {
    return new ValidationResult(false, (object) "");
  }
}
