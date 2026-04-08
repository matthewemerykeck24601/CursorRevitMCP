// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.ComboBoxExportColorItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Windows.Media;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class ComboBoxExportColorItem
{
  public string colorName;
  public Color color;
  public int cadNumber;

  public string colorString => this.color.ToString();

  public string DisplayValue => $"{this.colorName} : {this.cadNumber.ToString()}";

  public ComboBoxExportColorItem()
  {
  }

  public ComboBoxExportColorItem(string colorName, Color color, int cadNumber)
  {
    this.colorName = colorName;
    this.color = color;
    this.cadNumber = cadNumber;
  }

  public override bool Equals(object obj) => this.Equals(obj as ComboBoxExportColorItem);

  public bool Equals(ComboBoxExportColorItem obj)
  {
    return obj != null && this.colorName == obj.colorName && this.color.Equals(obj.color) && this.cadNumber == obj.cadNumber;
  }
}
