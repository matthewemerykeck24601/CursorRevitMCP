// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HWEditTitleblockViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketManager.Resources;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HWEditTitleblockViewModel : INotifyPropertyChanged
{
  private ObservableCollection<ComboBoxItemString> _titleBlockList = new ObservableCollection<ComboBoxItemString>();
  private string _selectedTitleBlock;
  private HWEditTitleblockWindow parentWindow;

  public ObservableCollection<ComboBoxItemString> titleblockList
  {
    get => this._titleBlockList;
    set
    {
      if (this._titleBlockList == value)
        return;
      this._titleBlockList = value;
      this.NotifyPropertyChanged(nameof (titleblockList));
    }
  }

  public string SelectedTitleBlock
  {
    get => this._selectedTitleBlock;
    set
    {
      if (!(value != this._selectedTitleBlock))
        return;
      this._selectedTitleBlock = value;
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public Command EditTitleblockCommand { set; get; }

  public HWEditTitleblockViewModel()
  {
  }

  public HWEditTitleblockViewModel(
    HWEditTitleblockWindow window,
    List<ComboBoxItemString> titleblocks)
  {
    this.parentWindow = window;
    this.SelectedTitleBlock = (string) null;
    foreach (ComboBoxItemString titleblock in titleblocks)
      this.titleblockList.Add(titleblock);
    this.EditTitleblockCommand = new Command(new Command.ICommandOnExecute(this.ExecuteEditTitleblockCommand), new Command.ICommandOnCanExecute(this.CanExecuteEditTitleblockCommand));
  }

  public void ExecuteEditTitleblockCommand(object parameter)
  {
    this.parentWindow.SetTittleBlockMainWindow(this.SelectedTitleBlock);
    this.parentWindow.Close();
  }

  public bool CanExecuteEditTitleblockCommand(object parameter) => this.SelectedTitleBlock != null;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
