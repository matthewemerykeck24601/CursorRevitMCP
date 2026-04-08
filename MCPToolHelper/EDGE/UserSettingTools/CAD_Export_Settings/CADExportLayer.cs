// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.CADExportLayer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class CADExportLayer : INotifyPropertyChanged
{
  private string _layerName = string.Empty;
  private ComboBoxExportColorItem _color;
  private bool _layerError;
  private bool _deleteEnabled;

  public string LayerName
  {
    get => this._layerName;
    set
    {
      if (!(this._layerName != value))
        return;
      this._layerName = value;
      this.NotifyPropertyChanged(nameof (LayerName));
    }
  }

  public ComboBoxExportColorItem LayerColor
  {
    get => this._color;
    set
    {
      if (value == null)
        return;
      if (this._color == null)
      {
        this._color = value;
        this.NotifyPropertyChanged(nameof (LayerColor));
        this.NotifyPropertyChanged("LayerColor.colorString");
        this.NotifyPropertyChanged("LayerColor.DisplayValue");
        this.NotifyPropertyChanged("strokeColor");
      }
      if (this._color.Equals(value))
        return;
      this._color = value;
      this.NotifyPropertyChanged(nameof (LayerColor));
      this.NotifyPropertyChanged("LayerColor.colorString");
      this.NotifyPropertyChanged("LayerColor.DisplayValue");
      this.NotifyPropertyChanged("strokeColor");
    }
  }

  [XmlIgnore]
  public bool LayerError
  {
    get => this._layerError;
    set
    {
      if (this._layerError == value)
        return;
      this._layerError = value;
      this.NotifyPropertyChanged(nameof (LayerError));
    }
  }

  [XmlIgnore]
  public bool strokeColor => this.LayerColor != null;

  [XmlIgnore]
  public bool DeleteEnabled
  {
    get => this._deleteEnabled;
    set
    {
      if (this._deleteEnabled == value)
        return;
      this._deleteEnabled = value;
      this.NotifyPropertyChanged(nameof (DeleteEnabled));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public CADExportLayer() => this.DeleteEnabled = false;

  public CADExportLayer(string layerName, ComboBoxExportColorItem color)
  {
    this.LayerName = layerName;
    this.LayerColor = color;
  }

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }

  public override bool Equals(object obj) => this.Equals(obj as CADExportLayer);

  public bool Equals(CADExportLayer obj)
  {
    if (obj == null || !(this.LayerName == obj.LayerName))
      return false;
    if (this.LayerColor != null)
    {
      if (this.LayerColor.Equals(obj.LayerColor))
        return true;
    }
    else if (obj.LayerColor == null)
      return true;
    return false;
  }

  public CADExportLayer Clone() => new CADExportLayer(this.LayerName, this.LayerColor);
}
