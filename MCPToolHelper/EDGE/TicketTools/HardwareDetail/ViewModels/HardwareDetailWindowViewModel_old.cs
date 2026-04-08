// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.ViewModels.HardwareDetailWindowViewModel_old
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.ViewModels;

public class HardwareDetailWindowViewModel_old : INotifyPropertyChanged
{
  private ObservableCollection<ComboBoxItemString> _manufacturerList = new ObservableCollection<ComboBoxItemString>();
  private ObservableCollection<ComboBoxItemString> _templateList = new ObservableCollection<ComboBoxItemString>();
  private ObservableCollection<ComboBoxItemString> _scaleList = new ObservableCollection<ComboBoxItemString>();
  private ObservableCollection<ComboBoxItemString> _titleBlockList = new ObservableCollection<ComboBoxItemString>();
  private string _manufacturerString;
  private string _tempalteString;
  private string _scaleString;
  private string _titleBlockString;

  public ObservableCollection<ComboBoxItemString> ManufacturerList
  {
    get => this._manufacturerList;
    set
    {
      if (this._manufacturerList == value)
        return;
      this._manufacturerList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (ManufacturerList));
    }
  }

  public ObservableCollection<ComboBoxItemString> TemplateList
  {
    get => this._templateList;
    set
    {
      if (this._templateList == value)
        return;
      this._templateList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (TemplateList));
    }
  }

  public ObservableCollection<ComboBoxItemString> ScaleList
  {
    get => this._scaleList;
    set
    {
      if (this._scaleList == value)
        return;
      this._scaleList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (ScaleList));
    }
  }

  public ObservableCollection<ComboBoxItemString> TitleBlockList
  {
    get => this._titleBlockList;
    set
    {
      if (this._titleBlockList == value)
        return;
      this._titleBlockList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (TitleBlockList));
    }
  }

  public string ManufacturerString
  {
    get => this._manufacturerString;
    set
    {
      if (!(this._manufacturerString != value))
        return;
      this._manufacturerString = value;
      this.NotifyPropertyChanged("ManufacturererString");
    }
  }

  public string TemplateString
  {
    get => this._tempalteString;
    set
    {
      if (!(this._tempalteString != value))
        return;
      this._tempalteString = value;
      this.NotifyPropertyChanged(nameof (TemplateString));
    }
  }

  public string ScaleString
  {
    get => this._scaleString;
    set
    {
      if (!(this._scaleString != value))
        return;
      this._scaleString = value;
      this.NotifyPropertyChanged(nameof (ScaleString));
    }
  }

  public string TitleBlockString
  {
    get => this._titleBlockString;
    set
    {
      if (!(this._titleBlockString != value))
        return;
      this._titleBlockString = value;
      this.NotifyPropertyChanged(nameof (TitleBlockString));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
