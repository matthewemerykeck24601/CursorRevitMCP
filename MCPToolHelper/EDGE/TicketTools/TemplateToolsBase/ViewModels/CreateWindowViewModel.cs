// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.ViewModels.CreateWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.HardwareDetail.ViewModels;
using EDGE.TicketTools.TicketManager.Resources;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System.ComponentModel;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.ViewModels;

internal class CreateWindowViewModel : INotifyPropertyChanged
{
  protected CreateWindow parentWindow;
  protected TemplateCreatorViewModel templateCreatorViewModel;
  protected HWTemplateCreatorViewModel hwTemplateCreatorViewModel;
  protected string _constructionProduct;
  protected string _templateName;
  protected string _sourceSheet;
  public BOMJustification BOMjustification;
  public bool bStackSchedules = true;

  public string ConstructionProduct
  {
    get => this._constructionProduct;
    set
    {
      if (this._constructionProduct != value)
      {
        this._constructionProduct = value;
        this.NotifyPropertyChanged(nameof (ConstructionProduct));
      }
      if (string.IsNullOrWhiteSpace(value))
        this.parentWindow.construcionProductNameLabel.Visibility = Visibility.Hidden;
      else
        this.parentWindow.construcionProductNameLabel.Visibility = Visibility.Visible;
    }
  }

  public string TemplateName
  {
    get => this._templateName;
    set
    {
      if (this._templateName != value)
      {
        this._templateName = value;
        this.NotifyPropertyChanged(nameof (TemplateName));
      }
      if (string.IsNullOrWhiteSpace(value))
        this.parentWindow.templateNameLabel.Visibility = Visibility.Hidden;
      else
        this.parentWindow.templateNameLabel.Visibility = Visibility.Visible;
    }
  }

  public string SourceSheet
  {
    get => this._sourceSheet;
    set
    {
      if (this._sourceSheet != value)
      {
        this._sourceSheet = value;
        this.NotifyPropertyChanged(nameof (SourceSheet));
      }
      if (string.IsNullOrWhiteSpace(value))
        this.parentWindow.sourceSheetNameLabel.Visibility = Visibility.Hidden;
      else
        this.parentWindow.sourceSheetNameLabel.Visibility = Visibility.Visible;
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public Command CloseCommand { get; set; }

  public Command CreateCommand { get; set; }

  protected CreateWindowViewModel()
  {
  }

  protected CreateWindowViewModel(
    CreateWindow window,
    string templateName,
    string constructionProduct,
    string sourceSheet,
    TemplateCreatorViewModel tempViewModel)
  {
    this.parentWindow = window;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCloseCommand), new Command.ICommandOnCanExecute(this.CanExecuteCloseCommand));
    this.CreateCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCreateCommand), new Command.ICommandOnCanExecute(this.CanExecuteCreateCommand));
    this.TemplateName = templateName;
    this.ConstructionProduct = constructionProduct;
    this.SourceSheet = sourceSheet;
    this.templateCreatorViewModel = tempViewModel;
  }

  protected CreateWindowViewModel(
    CreateWindow window,
    string templateName,
    string constructionProduct,
    string sourceSheet,
    HWTemplateCreatorViewModel tempViewModel)
  {
    this.parentWindow = window;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCloseCommand), new Command.ICommandOnCanExecute(this.CanExecuteCloseCommand));
    this.CreateCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCreateCommand), new Command.ICommandOnCanExecute(this.CanExecuteCreateCommand));
    this.TemplateName = templateName;
    this.SourceSheet = sourceSheet;
    this.hwTemplateCreatorViewModel = tempViewModel;
  }

  protected void ExecuteCloseCommand(object parameter) => this.parentWindow.Close();

  protected bool CanExecuteCloseCommand(object parameter) => true;

  protected void ExecuteCreateCommand(object parameter)
  {
    if (this.hwTemplateCreatorViewModel != null)
      this.hwTemplateCreatorViewModel.CreateTemplateFromViewSheet(this.BOMjustification, this.bStackSchedules);
    else
      this.templateCreatorViewModel.CreateTemplateFromViewSheet(this.BOMjustification, this.bStackSchedules);
    this.parentWindow.Close();
  }

  protected bool CanExecuteCreateCommand(object parameter) => true;

  protected void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
