// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.ViewModels.NewManufacturerViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase.Views;
using EDGE.TicketTools.TicketManager.Resources;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.ViewModels;

internal class NewManufacturerViewModel : INotifyPropertyChanged
{
  protected NewManufacturer parentWindow;
  protected ManufacturerManagementWindowViewModel manufacturerManagementViewModel;
  protected TemplateCreatorViewModel templateCreatorViewModel;
  protected string additionString = "";

  public event PropertyChangedEventHandler PropertyChanged;

  public Command CloseCommand { set; get; }

  public Command AddManufacturerCommand { set; get; }

  public string AdditionString
  {
    get => this.additionString;
    set
    {
      if (!(this.additionString != value))
        return;
      this.additionString = value;
      this.NotifyPropertyChanged(nameof (AdditionString));
    }
  }

  protected NewManufacturerViewModel()
  {
  }

  public NewManufacturerViewModel(
    NewManufacturer window,
    ManufacturerManagementWindowViewModel managementWindowViewModel)
  {
    this.parentWindow = window;
    this.manufacturerManagementViewModel = managementWindowViewModel;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCloseCommand), new Command.ICommandOnCanExecute(this.CanExecuteCloseCommand));
    if (this.manufacturerManagementViewModel.addElement)
    {
      this.AddManufacturerCommand = new Command(new Command.ICommandOnExecute(this.ExecuteAddManufacturerCommand), new Command.ICommandOnCanExecute(this.CanExecuteAddManufacturerCommand));
    }
    else
    {
      this.AddManufacturerCommand = new Command(new Command.ICommandOnExecute(this.ExecuteRenameManufacturerCommand), new Command.ICommandOnCanExecute(this.CanExecuteRenameManufacturerCommand));
      this.parentWindow.addButton.Content = (object) "Rename";
      this.parentWindow.Title = "Rename Manufacturer";
    }
  }

  public NewManufacturerViewModel(
    NewManufacturer window,
    TemplateCreatorViewModel templateCreatorWindowViewModel)
  {
    this.parentWindow = window;
    this.templateCreatorViewModel = templateCreatorWindowViewModel;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCloseCommand), new Command.ICommandOnCanExecute(this.CanExecuteCloseCommand));
    this.AddManufacturerCommand = new Command(new Command.ICommandOnExecute(this.ExecuteRenameTemplateCommand), new Command.ICommandOnCanExecute(this.CanExecuteRenameTemplateCommand));
    this.parentWindow.addButton.Content = (object) "Rename";
    this.parentWindow.Title = "Rename Template";
    this.parentWindow.newManufacturerNameLabel.Content = (object) "Enter Template Name:";
  }

  protected void ExecuteCloseCommand(object parameter) => this.parentWindow.Close();

  public bool CanExecuteCloseCommand(object parameter) => true;

  protected void ExecuteAddManufacturerCommand(object parameter)
  {
    if (this.manufacturerManagementViewModel.addManufacturer(this.additionString.Trim()))
    {
      this.parentWindow.Close();
    }
    else
    {
      new TaskDialog("Warning")
      {
        MainContent = "Manufacturer was not added. Make sure the text box is not empty or that the manufacturer does not already exists."
      }.Show();
      this.parentWindow.Focus();
    }
  }

  public bool CanExecuteAddManufacturerCommand(object parameter)
  {
    if (string.IsNullOrWhiteSpace(this.additionString))
      return false;
    int num1 = -1;
    int num2 = 0;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerManagementViewModel.manufacturerList)
    {
      if (manufacturer.ValueString == this.additionString.Trim())
      {
        num1 = num2;
        break;
      }
      ++num2;
    }
    return num1 == -1;
  }

  public void ExecuteRenameManufacturerCommand(object parameter)
  {
    if (this.manufacturerManagementViewModel.renameManufacturer(this.additionString.Trim()))
      this.parentWindow.Close();
    else
      new TaskDialog("Renaming Failed")
      {
        MainInstruction = "Failed to rename Manufacturer."
      }.Show();
  }

  public bool CanExecuteRenameManufacturerCommand(object parapeter)
  {
    if (string.IsNullOrWhiteSpace(this.additionString))
      return false;
    int num1 = -1;
    int num2 = 0;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerManagementViewModel.manufacturerList)
    {
      if (manufacturer.ValueString == this.additionString.Trim())
      {
        num1 = num2;
        break;
      }
      ++num2;
    }
    return num1 == -1;
  }

  protected void ExecuteRenameTemplateCommand(object parameter)
  {
    try
    {
      this.templateCreatorViewModel.RenameTemplate(this.AdditionString);
      this.parentWindow.Close();
    }
    catch (Exception ex)
    {
    }
  }

  public bool CanExecuteRenameTemplateCommand(object parameter)
  {
    return !string.IsNullOrWhiteSpace(this.additionString) && !this.templateCreatorViewModel.templateList.Any<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (a => a.ValueString == this.additionString.Trim()));
  }

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
