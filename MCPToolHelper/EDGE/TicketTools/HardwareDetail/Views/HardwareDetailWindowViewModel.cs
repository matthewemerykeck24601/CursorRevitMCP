// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HardwareDetailWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HardwareDetailWindowViewModel : INotifyPropertyChanged
{
  public static string manufacturerDefaultString = "<no filter>";
  public static string templateDefaultString = "<select template>";
  public static string scaleDefaultString = "<select scale>";
  public static string titleBlockDefaultString = "<select title block>";
  public static string copyDefaultString = "<Select Source>";
  private ObservableCollection<ComboBoxItemString> _manufacturerList;
  private ObservableCollection<ComboBoxItemString> _templateList;
  private ObservableCollection<ComboBoxItemString> _scaleList;
  private ObservableCollection<ComboBoxItemString> _titleBlockList;
  private ObservableCollection<ComboBoxItemString> _copyList;
  private string _manufacturerString;
  private string _tempalteString;
  private string _scaleString;
  private string _titleBlockString;
  private string _copyString;

  public HardwareDetailWindowViewModel()
  {
    ObservableCollection<ComboBoxItemString> observableCollection1 = new ObservableCollection<ComboBoxItemString>();
    observableCollection1.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.manufacturerDefaultString));
    this._manufacturerList = observableCollection1;
    ObservableCollection<ComboBoxItemString> observableCollection2 = new ObservableCollection<ComboBoxItemString>();
    observableCollection2.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.templateDefaultString));
    this._templateList = observableCollection2;
    ObservableCollection<ComboBoxItemString> observableCollection3 = new ObservableCollection<ComboBoxItemString>();
    observableCollection3.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.scaleDefaultString));
    this._scaleList = observableCollection3;
    ObservableCollection<ComboBoxItemString> observableCollection4 = new ObservableCollection<ComboBoxItemString>();
    observableCollection4.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.titleBlockDefaultString));
    this._titleBlockList = observableCollection4;
    ObservableCollection<ComboBoxItemString> observableCollection5 = new ObservableCollection<ComboBoxItemString>();
    observableCollection5.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.copyDefaultString));
    this._copyList = observableCollection5;
    this._manufacturerString = HardwareDetailWindowViewModel.manufacturerDefaultString;
    this._tempalteString = HardwareDetailWindowViewModel.templateDefaultString;
    this._scaleString = HardwareDetailWindowViewModel.scaleDefaultString;
    this._titleBlockString = HardwareDetailWindowViewModel.titleBlockDefaultString;
    this._copyString = string.Empty;
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }

  public ObservableCollection<ComboBoxItemString> ManufacturerList
  {
    get => this._manufacturerList;
    set
    {
      if (this._manufacturerList == value)
        return;
      if (value != null)
      {
        this._manufacturerList = value;
      }
      else
      {
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.manufacturerDefaultString));
        this._manufacturerList = observableCollection;
        this.ManufacturerString = HardwareDetailWindowViewModel.manufacturerDefaultString;
      }
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
      if (value != null)
      {
        this._templateList = value;
      }
      else
      {
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.templateDefaultString));
        this._templateList = observableCollection;
        this.TemplateString = HardwareDetailWindowViewModel.templateDefaultString;
      }
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
      if (value != null)
      {
        this._scaleList = value;
      }
      else
      {
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.scaleDefaultString));
        this._scaleList = observableCollection;
        this.ScaleString = HardwareDetailWindowViewModel.scaleDefaultString;
      }
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
      if (value != null)
      {
        this._titleBlockList = value;
      }
      else
      {
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.titleBlockDefaultString));
        this._titleBlockList = observableCollection;
        this.TitleBlockString = HardwareDetailWindowViewModel.titleBlockDefaultString;
      }
      this.NotifyPropertyChanged(nameof (TitleBlockList));
    }
  }

  public ObservableCollection<ComboBoxItemString> CopyList
  {
    get => this._copyList;
    set
    {
      if (this._copyList == value)
        return;
      if (value != null)
      {
        this._copyList = value;
      }
      else
      {
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.copyDefaultString));
        this._copyList = observableCollection;
        this.CopyString = HardwareDetailWindowViewModel.copyDefaultString;
      }
      this.NotifyPropertyChanged(nameof (CopyList));
    }
  }

  public string ManufacturerString
  {
    get => this._manufacturerString;
    set
    {
      if (!(this._manufacturerString != value))
        return;
      this._manufacturerString = value != null ? value : HardwareDetailWindowViewModel.manufacturerDefaultString;
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

  public string CopyString
  {
    get => this._copyString;
    set
    {
      if (!(this._copyString != value))
        return;
      this._copyString = value;
      this.NotifyPropertyChanged(nameof (CopyString));
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
