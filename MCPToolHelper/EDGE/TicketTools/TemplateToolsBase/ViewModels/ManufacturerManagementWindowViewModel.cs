// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.ViewModels.ManufacturerManagementWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.HardwareDetail;
using EDGE.TicketTools.TemplateToolsBase.Views;
using EDGE.TicketTools.TicketManager.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.ViewModels;

public class ManufacturerManagementWindowViewModel : INotifyPropertyChanged
{
  protected ManufacturerManagementWindow parentWindow;
  protected TemplateCreatorViewModel mainWindowViewModel;
  public bool addElement = true;
  protected ObservableCollection<ComboBoxItemString> _manufacturerList = new ObservableCollection<ComboBoxItemString>();
  protected string manufacturerString = "";
  public string templateType;

  public ObservableCollection<ComboBoxItemString> manufacturerList
  {
    get => this._manufacturerList;
    set
    {
      if (this._manufacturerList == value)
        return;
      this._manufacturerList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.OnPropertyChanged(nameof (manufacturerList));
    }
  }

  public string ManufacturerString
  {
    get => this.manufacturerString;
    set
    {
      if (!(this.manufacturerString != value))
        return;
      this.manufacturerString = value;
      this.OnPropertyChanged(nameof (ManufacturerString));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public Command AddCommand { set; get; }

  public Command CloseCommand { set; get; }

  public Command RenameCommand { set; get; }

  public Command DeleteCommand { set; get; }

  protected ManufacturerManagementWindowViewModel()
  {
  }

  public ManufacturerManagementWindowViewModel(
    ManufacturerManagementWindow window,
    TemplateCreatorViewModel creatorToolViewModel)
  {
    this.parentWindow = window;
    this.mainWindowViewModel = creatorToolViewModel;
    this.manufacturerList = creatorToolViewModel.manufacturerList;
    this.RemoveNullManufacturers();
    this.AddCommand = new Command(new Command.ICommandOnExecute(this.ExecuteAddCommand), new Command.ICommandOnCanExecute(this.CanExecuteAddCommand));
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteCloseCommand), new Command.ICommandOnCanExecute(this.CanExecuteCloseCommand));
    this.RenameCommand = new Command(new Command.ICommandOnExecute(this.ExecuteRenameManufacturerCommand), new Command.ICommandOnCanExecute(this.CanExecuteRenameManufacturerCommand));
    this.DeleteCommand = new Command(new Command.ICommandOnExecute(this.ExecuteDeleteCommand), new Command.ICommandOnCanExecute(this.CanExecuteDeleteCommand));
  }

  protected void ExecuteAddCommand(object parameter)
  {
    try
    {
      this.addElement = true;
      NewManufacturer newManufacturer = new NewManufacturer(this);
      newManufacturer.Owner = (Window) this.parentWindow;
      newManufacturer.ShowDialog();
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("There was an error adding Manufacturer:\n" + ex.Message);
    }
  }

  protected bool CanExecuteAddCommand(object parameter) => true;

  protected void ExecuteCloseCommand(object parameter)
  {
    try
    {
      this.parentWindow.Close();
    }
    catch (Exception ex)
    {
    }
  }

  protected bool CanExecuteCloseCommand(object parameter) => true;

  protected void ExecuteRenameManufacturerCommand(object parameter)
  {
    try
    {
      this.addElement = false;
      NewManufacturer newManufacturer = new NewManufacturer(this);
      newManufacturer.Owner = (Window) this.parentWindow;
      newManufacturer.ShowDialog();
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("There was an error renaming Manufacturer:\n" + ex.Message);
    }
  }

  protected bool CanExecuteRenameManufacturerCommand(object parameter)
  {
    return !string.IsNullOrWhiteSpace(this.manufacturerString);
  }

  protected void ExecuteDeleteCommand(object parameter)
  {
    try
    {
      if (this.deleteManufacturer(this.manufacturerString))
      {
        int num = (int) MessageBox.Show("Successfully deleted Manufacturer!");
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("There was an error deleting Manufacturer:\n" + ex.Message);
    }
    this.parentWindow.Focus();
  }

  protected bool CanExecuteDeleteCommand(object parameter)
  {
    return !string.IsNullOrWhiteSpace(this.manufacturerString);
  }

  public bool addManufacturer(string manufacturer)
  {
    ComboBoxItemString comboBoxItemString = new ComboBoxItemString(manufacturer);
    if (this.manufacturerList.Contains(comboBoxItemString))
      return false;
    this.manufacturerList.Add(comboBoxItemString);
    this.manufacturerList = this.mainWindowViewModel.AlhabetizeList(this.manufacturerList);
    this.OnPropertyChanged("manufacturerList");
    this.parentWindow.manufacturerListBox.Items.Refresh();
    this.mainWindowViewModel.manufacturerList = this.manufacturerList;
    return true;
  }

  public bool deleteManufacturer(string manufacturerToDelete)
  {
    TaskDialogResult taskDialogResult = new TaskDialog("Delete Manufacturer")
    {
      MainInstruction = "Do you want to delete templates that currently have the selected Manufacturer?",
      CommonButtons = ((TaskDialogCommonButtons) 14)
    }.Show();
    if (taskDialogResult == 2)
      return false;
    if (taskDialogResult == 6 && this.mainWindowViewModel.TemplateSettings != null)
    {
      List<TicketTemplate> ticketTemplateList = new List<TicketTemplate>();
      foreach (TicketTemplate template in this.mainWindowViewModel.TemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == manufacturerToDelete)
          ticketTemplateList.Add(template);
      }
      foreach (TicketTemplate ticketTemplate in ticketTemplateList)
      {
        this.mainWindowViewModel.DeleteTemplate(ticketTemplate.TemplateName);
        this.mainWindowViewModel.TemplateSettings.Templates.Remove(ticketTemplate);
      }
    }
    else if (taskDialogResult == 6 && this.mainWindowViewModel.TemplateSettings == null)
    {
      List<HWDetailTemplate> hwDetailTemplateList = new List<HWDetailTemplate>();
      foreach (HWDetailTemplate template in this.mainWindowViewModel.HWTemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == manufacturerToDelete)
          hwDetailTemplateList.Add(template);
      }
      foreach (HWDetailTemplate hwDetailTemplate in hwDetailTemplateList)
      {
        this.mainWindowViewModel.DeleteTemplate(hwDetailTemplate.TemplateName);
        this.mainWindowViewModel.HWTemplateSettings.Templates.Remove(hwDetailTemplate);
      }
    }
    else if (taskDialogResult == 7 && this.mainWindowViewModel.TemplateSettings != null)
    {
      foreach (TicketTemplate template in this.mainWindowViewModel.TemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == manufacturerToDelete)
          template.TemplateManufacturerName = (string) null;
      }
    }
    else if (taskDialogResult == 7 && this.mainWindowViewModel.TemplateSettings == null)
    {
      foreach (HWDetailTemplate template in this.mainWindowViewModel.HWTemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == manufacturerToDelete)
          template.TemplateManufacturerName = (string) null;
      }
    }
    int index = -1;
    int num = 0;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
    {
      if (manufacturer.ValueString == manufacturerToDelete)
      {
        index = num;
        break;
      }
      ++num;
    }
    if (index <= -1)
      return false;
    this.manufacturerList.RemoveAt(index);
    this.OnPropertyChanged("manufacturerList");
    this.mainWindowViewModel.DeleteManufacturer(manufacturerToDelete);
    return true;
  }

  public bool renameManufacturer(string manufacturer)
  {
    if (this.mainWindowViewModel.TemplateSettings != null)
    {
      foreach (TicketTemplate template in this.mainWindowViewModel.TemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == this.ManufacturerString)
          template.TemplateManufacturerName = manufacturer;
      }
    }
    else if (this.mainWindowViewModel.TemplateSettings == null)
    {
      foreach (HWDetailTemplate template in this.mainWindowViewModel.HWTemplateSettings.Templates)
      {
        if (template.TemplateManufacturerName == this.ManufacturerString)
          template.TemplateManufacturerName = manufacturer;
      }
    }
    ComboBoxItemString comboBoxItemString = new ComboBoxItemString(manufacturer);
    int index = -1;
    int num = 0;
    foreach (ComboBoxItemString manufacturer1 in (Collection<ComboBoxItemString>) this.manufacturerList)
    {
      if (manufacturer1.ValueString == this.ManufacturerString)
      {
        index = num;
        break;
      }
      ++num;
    }
    if (index == -1)
      return false;
    this.manufacturerList[index] = comboBoxItemString;
    this.manufacturerList = this.mainWindowViewModel.AlhabetizeList(this.manufacturerList);
    this.OnPropertyChanged("manufacturerList");
    this.parentWindow.manufacturerListBox.Items.Refresh();
    this.mainWindowViewModel.manufacturerList = this.manufacturerList;
    return true;
  }

  protected void RemoveNullManufacturers()
  {
    int index = -1;
    int num = 0;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
    {
      if (manufacturer.ValueString == null)
      {
        index = num;
        break;
      }
      ++num;
    }
    if (index <= -1)
      return;
    this.manufacturerList.RemoveAt(index);
    this.OnPropertyChanged("manufacturerList");
    this.parentWindow.manufacturerListBox.Items.Refresh();
    this.mainWindowViewModel.RefreshWindow();
  }

  protected void OnPropertyChanged(string name)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(name));
  }
}
